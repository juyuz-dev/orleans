using Microsoft.Extensions.Logging;

namespace Orleans.Runtime.MembershipService
{
    internal class GlobalClusterMembershipService : ClusterMembershipService, IGlobalClusterMembershipService
    {
        public GlobalClusterMembershipService(
            GlobalMembershipTableManager membershipTableManager,
            ILogger<ClusterMembershipService> log,
            IFatalErrorHandler fatalErrorHandler) : base(membershipTableManager, log, fatalErrorHandler)
        {
        }
    }
}
