using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Orleans.Runtime.MembershipService
{
    internal class GlobalMembershipTableCleanupAgent : MembershipTableCleanupAgent
    {
        public GlobalMembershipTableCleanupAgent(
            IOptions<ClusterMembershipOptions> clusterMembershipOptions,
            IGlobalMembershipTable membershipTableProvider,
            ILogger<MembershipTableCleanupAgent> log,
            IAsyncTimerFactory timerFactory) : base(clusterMembershipOptions, membershipTableProvider, log, timerFactory)
        {
        }
    }
}
