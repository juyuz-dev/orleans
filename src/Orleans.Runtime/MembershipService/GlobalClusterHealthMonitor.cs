using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Orleans.Runtime.MembershipService
{
    internal class GlobalClusterHealthMonitor : ClusterHealthMonitor
    {
        public GlobalClusterHealthMonitor(
            ILocalSiloDetails localSiloDetails,
            GlobalMembershipTableManager tableManager,
            ILogger<ClusterHealthMonitor> log,
            IOptions<ClusterMembershipOptions> clusterMembershipOptions,
            IFatalErrorHandler fatalErrorHandler,
            IServiceProvider serviceProvider,
            IAsyncTimerFactory timerFactory) : base(localSiloDetails, tableManager, log, clusterMembershipOptions, fatalErrorHandler, serviceProvider, timerFactory)
        {
        }
    }
}
