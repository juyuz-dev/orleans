using System;
using System.Net;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Orleans.Runtime
{
    internal class LocalSiloDetails : ILocalSiloDetails
    {
        private readonly Lazy<SiloAddress> siloAddressLazy;
        private readonly Lazy<SiloAddress> gatewayAddressLazy;

        public LocalSiloDetails(
            IOptions<SiloOptions> siloOptions,
            IOptions<ClusterOptions> clusterOptions,
            IOptions<EndpointOptions> siloEndpointOptions)
        {
            this.Name = siloOptions.Value.SiloName;
            this.ClusterId = clusterOptions.Value.ClusterId;
            this.DnsHostName = Dns.GetHostName();
            this.Region = clusterOptions.Value.Region;

            var endpointOptions = siloEndpointOptions.Value;
            this.siloAddressLazy = new Lazy<SiloAddress>(() => SiloAddress.New(endpointOptions.GetPublicSiloEndpoint(), SiloAddress.AllocateNewGeneration()));
            this.gatewayAddressLazy = new Lazy<SiloAddress>(() =>
            {
                var publicProxyEndpoint = endpointOptions.GetPublicProxyEndpoint();
                return publicProxyEndpoint != null
                        ? SiloAddress.New(publicProxyEndpoint, 0)
                        : null;
            });
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string ClusterId { get; }

        public string Region { get; }

        /// <inheritdoc />
        public string DnsHostName { get; }

        /// <inheritdoc />
        public SiloAddress SiloAddress => this.siloAddressLazy.Value;

        /// <inheritdoc />
        public SiloAddress GatewayAddress => this.gatewayAddressLazy.Value;
    }
}