using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Orleans.Runtime.Utilities
{
    public sealed class FailoverUtility
    {
        private static string cachedConfig;

        private static HashSet<string> cachedRegions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static HashSet<string> GetExcludedRegions(IConfiguration configuration)
        {
            var excludedRegions = configuration["ExcludedRegions"];

            if (string.Equals(excludedRegions, cachedConfig, StringComparison.OrdinalIgnoreCase))
            {
                return cachedRegions;
            }

            HashSet<string> regions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(excludedRegions))
            {
                foreach (var region in excludedRegions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!string.IsNullOrWhiteSpace(region))
                    {
                        regions.Add(region.Trim());
                    }
                }
            }

            cachedRegions = regions;
            cachedConfig = excludedRegions;

            return cachedRegions;
        }
    }
}
