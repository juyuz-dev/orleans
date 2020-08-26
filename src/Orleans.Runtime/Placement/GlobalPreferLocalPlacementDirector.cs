using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Orleans.Runtime.MembershipService;
using Orleans.Runtime.Versions.Compatibility;
using Orleans.Runtime.Versions.Selector;

namespace Orleans.Runtime.Placement
{
    internal class GlobalPreferLocalPlacementDirector : GlobalRandomPlacementDirector, IPlacementDirector
    {
        private Task<SiloAddress> cachedLocalSilo;

        public GlobalPreferLocalPlacementDirector(
            MembershipTableManager membershipTableManager,
            GlobalMembershipTableManager globalMembershipTableManager,
            VersionSelectorManager versionSelectorManager,
            CompatibilityDirectorManager compatibilityDirectorManager,
            IRegionSelector regionSelector,
            IConfiguration configuration)
            : base(membershipTableManager, globalMembershipTableManager, versionSelectorManager, compatibilityDirectorManager, regionSelector, configuration)
        {
        }

        public override Task<SiloAddress>
            OnAddActivation(PlacementStrategy strategy, PlacementTarget target, IPlacementContext context)
        {
            // Make sure the grain is placed on desired silo.
            string primaryKey = target.GrainIdentity.PrimaryKeyString;

            if (string.IsNullOrEmpty(primaryKey))
            {
                throw new OrleansException($"Primary key cannot be empty string for type '{target.GrainIdentity.TypeCode}'");
            }

            SiloAddress targetSiloAddress;

            try
            {
                targetSiloAddress = SiloAddress.FromParsableString(primaryKey);
            }
            catch (Exception)
            {
                throw new OrleansException($"Primary key '{primaryKey}' is not a valid SiloAddress for type '{target.GrainIdentity.TypeCode}'");
            }

            if (targetSiloAddress == context.LocalSilo)
            {
                if (context.LocalSiloStatus == SiloStatus.Active || context.LocalSiloStatus == SiloStatus.Joining)
                {
                    cachedLocalSilo = cachedLocalSilo ?? Task.FromResult(context.LocalSilo);
                    return cachedLocalSilo;
                }
                else
                {
                    throw new OrleansException($"Local Silo is not active. Cannot place grain on it.");
                }
            }

            if (!this.globalMembershipTableManager.MembershipTableSnapshot.Entries.TryGetValue(targetSiloAddress, out var memEntry)
                || memEntry.Status != SiloStatus.Active)
            {
                throw new OrleansException($"Remote Silo is not active. Cannot place grain on it.");
            }

            return Task.FromResult(targetSiloAddress);
        }
    }
}
