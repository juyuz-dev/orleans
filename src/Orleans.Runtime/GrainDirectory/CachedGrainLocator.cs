using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.GrainDirectory;
using Orleans.Internal;
using Orleans.Runtime.MembershipService;

namespace Orleans.Runtime.GrainDirectory
{
    /// <summary>
    /// Implementation of <see cref="IGrainLocator"/> that uses <see cref="IGrainDirectory"/> stores.
    /// </summary>
    internal class CachedGrainLocator : IGrainLocator, ILifecycleParticipant<ISiloLifecycle>, CachedGrainLocator.ITestAccessor
    {
        private readonly IGrainDirectoryResolver grainDirectoryResolver;
        private readonly DhtGrainLocator inClusterGrainLocator;
        private readonly IGrainDirectoryCache cache;
        private readonly IServiceProvider serviceProvider;

        private readonly CancellationTokenSource shutdownToken = new CancellationTokenSource();
        private readonly IClusterMembershipService clusterMembershipService;
        private readonly IGlobalClusterMembershipService globalClusterMembershipService;

        private HashSet<SiloAddress> knownDeadSilos = new HashSet<SiloAddress>();

        private HashSet<SiloAddress> knownDeadGlobalSilos = new HashSet<SiloAddress>();

        private Task listenToClusterChangeTask;
        private Task listenToGlobalClusterChangeTask;

        internal interface ITestAccessor
        {
            MembershipVersion LastMembershipVersion { get; set; }

            MembershipVersion LastGlobalMembershipVersion { get; set; }
        }

        MembershipVersion ITestAccessor.LastMembershipVersion { get; set; }

        MembershipVersion ITestAccessor.LastGlobalMembershipVersion { get; set; }

        public CachedGrainLocator(
            IGrainDirectoryResolver grainDirectoryResolver,
            DhtGrainLocator inClusterGrainLocator,
            IClusterMembershipService clusterMembershipService,
            IServiceProvider serviceProvider)
        {
            this.grainDirectoryResolver = grainDirectoryResolver;
            this.inClusterGrainLocator = inClusterGrainLocator;
            this.clusterMembershipService = clusterMembershipService;
            this.serviceProvider = serviceProvider;
            this.cache = new LRUBasedGrainDirectoryCache(GrainDirectoryOptions.DEFAULT_CACHE_SIZE, GrainDirectoryOptions.DEFAULT_MAXIMUM_CACHE_TTL);

            this.globalClusterMembershipService = this.serviceProvider.GetService<IGlobalClusterMembershipService>();
        }

        public async Task<List<ActivationAddress>> Lookup(GrainId grainId)
        {
            List<ActivationAddress> results;

            // Check cache first
            if (TryLocalLookup(grainId, out results))
            {
                return results;
            }

            results = new List<ActivationAddress>();

            var entry = await GetGrainDirectory(grainId).Lookup(grainId.ToParsableString());

            // Nothing found
            if (entry == null)
                return results;

            var activationAddress = entry.ToActivationAddress();

            // Check if the entry is pointing to a dead silo
            if (this.IsDead(grainId, activationAddress.Silo))
            {
                // Remove it from the directory
                await GetGrainDirectory(grainId).Unregister(entry);
            }
            else
            {
                // Add to the local cache and return it
                results.Add(activationAddress);
                this.cache.AddOrUpdate(grainId, new List<Tuple<SiloAddress, ActivationId>> { Tuple.Create(activationAddress.Silo, activationAddress.Activation) }, 0);
            }

            return results;
        }

        public async Task<ActivationAddress> Register(ActivationAddress address)
        {
            if (address.Grain.IsClient)
                return await this.inClusterGrainLocator.Register(address);

            var grainAddress = address.ToGrainAddress();
            var grainId = address.Grain;

            var result = await GetGrainDirectory(grainId).Register(grainAddress);
            var activationAddress = result.ToActivationAddress();

            // Check if the entry point to a dead silo
            if (this.IsDead(grainId, activationAddress.Silo))
            {
                // Remove outdated entry and retry to register
                await GetGrainDirectory(grainId).Unregister(result);
                result = await GetGrainDirectory(grainId).Register(grainAddress);
                activationAddress = result.ToActivationAddress();
            }

            // Cache update
            this.cache.AddOrUpdate(
                activationAddress.Grain,
                new List<Tuple<SiloAddress, ActivationId>>() { Tuple.Create(activationAddress.Silo, activationAddress.Activation) },
                0);

            return activationAddress;
        }

        public bool TryLocalLookup(GrainId grainId, out List<ActivationAddress> addresses)
        {
            if (this.cache.LookUp(grainId, out var results))
            {
                // IGrainDirectory only supports single activation
                var result = results[0];

                // If the silo is dead, remove the entry
                if (this.IsDead(grainId, result.Item1))
                {
                    this.cache.Remove(grainId);
                }
                else
                {
                    // Entry found and valid -> return it
                    addresses = new List<ActivationAddress>() { ActivationAddress.GetAddress(result.Item1, grainId, result.Item2) };
                    return true;
                }
            }

            addresses = null;
            return false;
        }

        public async Task Unregister(ActivationAddress address, UnregistrationCause cause)
        {
            try
            {
                await GetGrainDirectory(address.Grain).Unregister(address.ToGrainAddress());
            }
            finally
            {
                this.cache.Remove(address.Grain);
            }
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            Task OnStart(CancellationToken ct)
            {
                this.listenToClusterChangeTask = ListenToClusterChange(this.clusterMembershipService, false);

                if (this.globalClusterMembershipService != null)
                {
                    this.listenToGlobalClusterChangeTask = ListenToClusterChange(this.globalClusterMembershipService, true);
                }
                return Task.CompletedTask;
            };
            async Task OnStop(CancellationToken ct)
            {
                this.shutdownToken.Cancel();
                if (listenToClusterChangeTask != default && !ct.IsCancellationRequested)
                    await listenToClusterChangeTask.WithCancellation(ct);

                if (this.listenToGlobalClusterChangeTask != default && !ct.IsCancellationRequested)
                    await this.listenToGlobalClusterChangeTask.WithCancellation(ct);
            };
            lifecycle.Subscribe(nameof(CachedGrainLocator), ServiceLifecycleStage.RuntimeGrainServices, OnStart, OnStop);
        }

        private bool IsDead(GrainId grainId, SiloAddress silo)
        {
            // TODO: For different grain type, we should have different logic.
            // If it's a local grain, check local dead silos only.
            // If it's a global grain, check global dead silos only.
            // Once we define our global placement director, update GrainInterfaceMap to include
            // global allocation information.
            return this.knownDeadSilos.Contains(silo) || this.knownDeadGlobalSilos.Contains(silo);
        }

        private IGrainDirectory GetGrainDirectory(GrainId grainId) => this.grainDirectoryResolver.Resolve(grainId);

        private async Task ListenToClusterChange(IClusterMembershipService membershipService, bool isGlobal)
        {
            var previousSnapshot = membershipService.CurrentSnapshot;
            // Update the list of known dead silos for lazy filtering for the first time
            var tempDeadSilos = new HashSet<SiloAddress>(previousSnapshot.Members.Values
                .Where(m => m.Status == SiloStatus.Dead)
                .Select(m => m.SiloAddress));

            if (isGlobal)
            {
                this.knownDeadGlobalSilos = tempDeadSilos;
                ((ITestAccessor)this).LastGlobalMembershipVersion = previousSnapshot.Version;
            }
            else
            {
                this.knownDeadSilos = tempDeadSilos;
                ((ITestAccessor)this).LastMembershipVersion = previousSnapshot.Version;
            }

            var updates = membershipService.MembershipUpdates.WithCancellation(this.shutdownToken.Token);
            await foreach (var snapshot in updates)
            {
                // Update the list of known dead silos for lazy filtering
                tempDeadSilos = new HashSet<SiloAddress>(snapshot.Members.Values
                    .Where(m => m.Status.IsTerminating())
                    .Select(m => m.SiloAddress));

                if (isGlobal)
                {
                    this.knownDeadGlobalSilos = tempDeadSilos;
                    ((ITestAccessor)this).LastGlobalMembershipVersion = previousSnapshot.Version;
                }
                else
                {
                    this.knownDeadSilos = tempDeadSilos;
                    ((ITestAccessor)this).LastMembershipVersion = previousSnapshot.Version;
                }

                // Active filtering: detect silos that went down and try to clean proactively the directory
                var changes = snapshot.CreateUpdate(previousSnapshot).Changes;
                var deadSilos = changes
                    .Where(member => member.Status.IsTerminating())
                    .Select(member => member.SiloAddress.ToParsableString())
                    .ToList();

                if (deadSilos.Count > 0)
                {
                    var tasks = new List<Task>();
                    foreach (var directory in this.grainDirectoryResolver.Directories)
                    {
                        tasks.Add(directory.UnregisterSilos(deadSilos));
                    }
                    await Task.WhenAll(tasks).WithCancellation(this.shutdownToken.Token);
                }
            }
        }
    }

    internal static class AddressHelpers
    {
        public static ActivationAddress ToActivationAddress(this GrainAddress addr)
        {
            return ActivationAddress.GetAddress(
                    SiloAddress.FromParsableString(addr.SiloAddress),
                    GrainId.FromParsableString(addr.GrainId),
                    ActivationId.GetActivationId(UniqueKey.Parse(addr.ActivationId.AsSpan())));
        }

        public static GrainAddress ToGrainAddress(this ActivationAddress addr)
        {
            return new GrainAddress
            {
                SiloAddress = addr.Silo.ToParsableString(),
                GrainId = addr.Grain.ToParsableString(),
                ActivationId = (addr.Activation.Key.ToHexString())
            };
        }
    }
}
