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
            IOptionsMonitor<ClusterMembershipOptions> clusterMembershipOptions,
            IFatalErrorHandler fatalErrorHandler,
            IServiceProvider serviceProvider) : base(localSiloDetails, tableManager, log, clusterMembershipOptions, fatalErrorHandler, serviceProvider)
        {
        }
    }
}
