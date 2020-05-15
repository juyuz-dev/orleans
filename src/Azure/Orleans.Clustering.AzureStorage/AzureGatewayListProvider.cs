using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.Messaging;

namespace Orleans.AzureUtils
{
    internal class AzureGatewayListProvider : IGatewayListProvider
    {
        private OrleansSiloInstanceManager siloInstanceManager;
        private readonly string clusterId;
        private readonly AzureStorageGatewayOptions options;
        private readonly ILoggerFactory loggerFactory;

        public AzureGatewayListProvider(ILoggerFactory loggerFactory, IOptions<AzureStorageGatewayOptions> options, IOptions<ClusterOptions> clusterOptions, IOptions<GatewayOptions> gatewayOptions)
        {
            this.loggerFactory = loggerFactory;
            this.clusterId = clusterOptions.Value.ClusterId;
            this.MaxStaleness = gatewayOptions.Value.GatewayListRefreshPeriod;
            this.options = options.Value;
        }

        public async Task InitializeGatewayListProvider()
        {
            this.siloInstanceManager = await OrleansSiloInstanceManager.GetManager(
                this.clusterId,
                this.loggerFactory,
                this.options);
        }
        // no caching
        public async Task<IList<(Uri,int)>> GetGateways()
        {
            // FindAllGatewayProxyEndpoints already returns a deep copied List<Uri>.
            return (await this.siloInstanceManager.FindAllGatewayProxyEndpoints()).Select(r => (r, 0)).ToList();
        }

        public TimeSpan MaxStaleness { get; }

        public bool IsUpdatable => true;
    }
}
