using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Runtime.MembershipService
{
    internal class GlobalMembershipGossiper : IMembershipGossiper
    {
        public Task GossipToRemoteSilos(List<SiloAddress> gossipPartners, MembershipTableSnapshot snapshot, SiloAddress updatedSilo, SiloStatus updatedStatus)
        {
            return Task.CompletedTask;
        }
    }
}
