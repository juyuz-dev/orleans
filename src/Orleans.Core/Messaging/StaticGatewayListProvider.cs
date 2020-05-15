using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Orleans.Messaging
{
    public class StaticGatewayListProvider : IGatewayListProvider
    {
        private readonly StaticGatewayListProviderOptions options;
        private readonly TimeSpan maxStaleness;
        public StaticGatewayListProvider(IOptions<StaticGatewayListProviderOptions> options, IOptions<GatewayOptions> gatewayOptions)
        {
            this.options = options.Value;
            this.maxStaleness = gatewayOptions.Value.GatewayListRefreshPeriod;
        }

        public Task InitializeGatewayListProvider() => Task.CompletedTask;
        

        public Task<IList<(Uri,int)>> GetGateways() => Task.FromResult<IList<(Uri,int)>>(this.options.Gateways.Select(r => (r,0)).ToList());

        public TimeSpan MaxStaleness
        {
            get => this.maxStaleness;
        }

        public bool IsUpdatable
        {
            get => true;
        }
    }
}
