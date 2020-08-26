using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Runtime.MembershipService
{
    public interface IMembershipGossiper
    {
        Task GossipToRemoteSilos(
            List<SiloAddress> gossipPartners,
            MembershipTableSnapshot snapshot,
            SiloAddress updatedSilo,
            SiloStatus updatedStatus);
    }
}
