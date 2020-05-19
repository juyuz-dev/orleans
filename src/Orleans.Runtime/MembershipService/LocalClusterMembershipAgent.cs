using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Orleans.Runtime.MembershipService
{
    internal class LocalClusterMembershipAgent : MembershipAgent
    {
        public LocalClusterMembershipAgent(
            MembershipTableManager tableManager,
            ClusterHealthMonitor clusterHealthMonitor,
            ILocalSiloDetails localSilo,
            IFatalErrorHandler fatalErrorHandler,
            IOptions<ClusterMembershipOptions> options,
            ILogger<MembershipAgent> log,
            IAsyncTimerFactory timerFactory) : base (tableManager, clusterHealthMonitor, localSilo, fatalErrorHandler, options, log, timerFactory)
        {
            Console.WriteLine("####Inside Local cluster membership agent.");
        }
    }
}
