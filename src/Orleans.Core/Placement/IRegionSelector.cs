using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Runtime.Placement
{
    public interface IRegionSelector
    {
        Task<HashSet<string>> SelectRegions(PlacementTarget target, IPlacementContext context);
    }
}
