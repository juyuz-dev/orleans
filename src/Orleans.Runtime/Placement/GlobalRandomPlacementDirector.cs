using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Orleans.Internal;
using Orleans.Runtime.MembershipService;
using Orleans.Runtime.Utilities;
using Orleans.Runtime.Versions.Compatibility;
using Orleans.Runtime.Versions.Selector;

namespace Orleans.Runtime.Placement
{
    internal class GlobalRandomPlacementDirector : IPlacementDirector
    {
        private readonly SafeRandom random = new SafeRandom();

        private readonly MembershipTableManager membershipTableManager;

        private readonly VersionSelectorManager versionSelectorManager;

        private readonly CompatibilityDirectorManager compatibilityDirectorManager;

        private readonly IRegionSelector regionSelector;

        private readonly IConfiguration configuration;

        private static readonly HashSet<string> EmptyExcludedRegions = new HashSet<string>();

        protected readonly GlobalMembershipTableManager globalMembershipTableManager;

        public GlobalRandomPlacementDirector(
            MembershipTableManager membershipTableManager,
            GlobalMembershipTableManager globalMembershipTableManager,
            VersionSelectorManager versionSelectorManager,
            CompatibilityDirectorManager compatibilityDirectorManager,
            IRegionSelector regionSelector,
            IConfiguration configuration)
        {
            this.membershipTableManager = membershipTableManager;
            this.globalMembershipTableManager = globalMembershipTableManager;
            this.versionSelectorManager = versionSelectorManager;
            this.compatibilityDirectorManager = compatibilityDirectorManager;
            this.regionSelector = regionSelector;
            this.configuration = configuration;
        }

        public virtual async Task<SiloAddress> OnAddActivation(PlacementStrategy strategy, PlacementTarget target, IPlacementContext context)
        {
            var currentSnapshot = this.globalMembershipTableManager.MembershipTableSnapshot;
            HashSet<string> regions = FailoverUtility.GetExcludedRegions(this.configuration);

            List<SiloAddress> silos = new List<SiloAddress>();

            foreach (var kvp in currentSnapshot.Entries)
            {
                if (this.IsActiveAndCompatible(kvp.Value, target, regions))
                {
                    silos.Add(kvp.Key);
                }
            }

            if (silos.Count == 0)
            {
                // Fall back to use local membership table if no available silo in global.
                currentSnapshot = this.membershipTableManager.MembershipTableSnapshot;

                foreach (var kvp in currentSnapshot.Entries)
                {
                    if (this.IsActiveAndCompatible(kvp.Value, target, GlobalRandomPlacementDirector.EmptyExcludedRegions))
                    {
                        silos.Add(kvp.Key);
                    }
                }
            }

            if (silos.Count == 0)
            {
                // Select itself if there is no active silos and current one is joining.
                if (context.LocalSiloStatus == SiloStatus.Joining && this.IsCompatible(this.globalMembershipTableManager.GrainTypeMap, target))
                {
                    return context.LocalSilo;
                }

                throw new OrleansException($"TypeCode ${target.GrainIdentity.TypeCode} not supported in the cluster");
            }

            HashSet<string> preferredRegions = await this.regionSelector.SelectRegions(target, context);

            if (preferredRegions != null && preferredRegions.Count > 0)
            {
                List<SiloAddress> preferredSilos = new List<SiloAddress>();
                foreach (var silo in silos)
                {
                    if (currentSnapshot.Entries.TryGetValue(silo, out var entry)
                        && (string.IsNullOrEmpty(entry.Region) || preferredRegions.Contains(entry.Region)))
                    {
                        preferredSilos.Add(silo);
                    }
                }

                if (preferredSilos.Count != 0)
                {
                    // Prefer region specific silos
                    silos = preferredSilos;
                }
            }

            return silos[random.Next(silos.Count)];
        }

        private bool IsActiveAndCompatible(MembershipEntry membershipEntry, PlacementTarget target, HashSet<string> excludedRegions)
        {
            if (membershipEntry.Status != SiloStatus.Active)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(membershipEntry.Region) && excludedRegions.Contains(membershipEntry.Region))
            {
                return false;
            }

            var grainTypeMap = membershipEntry.GrainTypeMap;
            return this.IsCompatible(grainTypeMap, target);
        }

        private bool IsCompatible(string grainTypeMap, PlacementTarget target)
        {
            if (string.IsNullOrEmpty(grainTypeMap))
            {
                return false;
            }

            SiloGrainTypeMap siloGrainTypeMap = JsonSerializer.Deserialize<SiloGrainTypeMap>(grainTypeMap);

            var typeCode = target.GrainIdentity.TypeCode;

            bool compatible = target.InterfaceVersion > 0
                ? this.IsInterfaceCompatible(typeCode, target.InterfaceId, target.InterfaceVersion, siloGrainTypeMap)
                : siloGrainTypeMap.SupportedGrainClasses.Contains(typeCode);

            return compatible;
        }

        private bool IsInterfaceCompatible(
            int typeCode,
            int ifaceId,
            ushort requestedVersion,
            SiloGrainTypeMap siloGrainTypeMap)
        {
            var versionSelector = this.versionSelectorManager.GetSelector(ifaceId);
            var compatibilityDirector = this.compatibilityDirectorManager.GetDirector(ifaceId);

            var versionsInSilo = siloGrainTypeMap.SupportedGrainInterfaces
                .Where(e => e.InterfaceId == ifaceId)
                .Select(e => e.InterfaceVersion)
                .ToList();

            var versions = versionSelector.GetSuitableVersion(
                requestedVersion,
                versionsInSilo,
                compatibilityDirector);

            return versions.Any() && siloGrainTypeMap.SupportedGrainClasses.Contains(typeCode);
        }
    }
}
