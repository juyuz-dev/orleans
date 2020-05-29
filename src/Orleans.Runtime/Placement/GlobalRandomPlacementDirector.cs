using System.Linq;
using System.Threading.Tasks;
using Orleans.Runtime.MembershipService;

namespace Orleans.Runtime.Placement
{
    internal class GlobalRandomPlacementDirector : IPlacementDirector
    {
        private readonly GlobalMembershipTableManager membershipTableManager;

        public GlobalRandomPlacementDirector(GlobalMembershipTableManager membershipTableManager)
        {
            this.membershipTableManager = membershipTableManager;
        }

        public Task<SiloAddress> OnAddActivation(PlacementStrategy strategy, PlacementTarget target, IPlacementContext context)
        {
            var activeSilos = this.membershipTableManager.MembershipTableSnapshot.Entries.Where(e => e.Value.Status == SiloStatus.Active).Select(e => e.Key);

            var selectedSilos = activeSilos.Where(s => !s.Equals(context.LocalSilo));
            if(!selectedSilos.Any())
            {
                selectedSilos = activeSilos;
            }

            return Task.FromResult(selectedSilos.First());
        }
    }
}
