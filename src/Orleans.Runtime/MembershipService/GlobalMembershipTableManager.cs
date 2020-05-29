using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Orleans.Runtime.MembershipService
{
    internal class GlobalMembershipTableManager : MembershipTableManager
    {
        public GlobalMembershipTableManager(
            ILocalSiloDetails localSiloDetails,
            IOptions<ClusterMembershipOptions> clusterMembershipOptions,
            IGlobalMembershipTable membershipTable,
            IFatalErrorHandler fatalErrorHandler,
            GlobalMembershipGossiper gossiper,
            ILogger<MembershipTableManager> log,
            IAsyncTimerFactory timerFactory,
            ISiloLifecycle siloLifecycle,
            GrainTypeManager grainTypeManager)
            : base(localSiloDetails, clusterMembershipOptions, membershipTable, fatalErrorHandler, gossiper, log, timerFactory, siloLifecycle, grainTypeManager)
        {
        }
    }
}