using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Orleans.Runtime.MembershipService
{
    internal class GlobalClusterMembershipAgent : MembershipAgent
    {
        public GlobalClusterMembershipAgent(
            GlobalMembershipTableManager tableManager,
            GlobalClusterHealthMonitor clusterHealthMonitor,
            ILocalSiloDetails localSilo,
            IFatalErrorHandler fatalErrorHandler,
            IOptions<ClusterMembershipOptions> options,
            ILogger<MembershipAgent> log,
            IAsyncTimerFactory timerFactory) : base(tableManager, clusterHealthMonitor, localSilo, fatalErrorHandler, options, log, timerFactory)
        {
            Console.WriteLine("####Inside Global cluster membership agent.");
        }
    }
}
