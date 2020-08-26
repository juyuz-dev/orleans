using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;

namespace Orleans.Runtime.MembershipService
{
    internal class GlobalAzureBasedMembershipTable : AzureBasedMembershipTable, IGlobalMembershipTable
    {
        public GlobalAzureBasedMembershipTable(
            ILoggerFactory loggerFactory,
            IOptions<AzureStorageClusteringGlobalOptions> clusteringOptions,
            IOptions<ClusterOptions> clusterOptions) : base(loggerFactory, clusteringOptions, clusterOptions)
        {
        }
    }
}
