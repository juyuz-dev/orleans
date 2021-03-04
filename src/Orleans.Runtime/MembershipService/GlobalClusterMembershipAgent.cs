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
            ILocalSiloDetails localSilo,
            IFatalErrorHandler fatalErrorHandler,
            IOptions<ClusterMembershipOptions> options,
            ILogger<MembershipAgent> log,
            IAsyncTimerFactory timerFactory,
            IRemoteSiloProber siloProber) : base(tableManager, localSilo, fatalErrorHandler, options, log, timerFactory, siloProber)
        {
        }
    }
}
