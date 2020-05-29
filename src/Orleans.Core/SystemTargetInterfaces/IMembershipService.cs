using System.Threading.Tasks;


namespace Orleans.Runtime
{
    internal interface IMembershipService : ISystemTarget
    {
        /// <summary>
        /// Receive notifications about silo status events. 
        /// </summary>
        /// <param name="updatedSilo">Silo to update about</param>
        /// <param name="status">Status of the silo</param>
        /// <param name="isGlobal">true if it's updates for global clustering</param>
        /// <returns></returns>
        /// TODO REMOVE in a next version
        Task SiloStatusChangeNotification(SiloAddress updatedSilo, SiloStatus status, bool isGlobal);

        /// <summary>
        /// Receive notifications about a change in the membership table
        /// </summary>
        /// <param name="snapshot">Snapshot of the membership table</param>
        /// <param name="isGlobal">true if it's updates for global clustering</param>
        /// <returns></returns>
        Task MembershipChangeNotification(MembershipTableSnapshot snapshot, bool isGlobal);

        /// <summary>
        /// Ping request from another silo that probes the liveness of the recipient silo.
        /// </summary>
        /// <param name="pingNumber">A unique sequence number for ping message, to facilitate testing and debugging.</param>
        Task Ping(int pingNumber);
    }
}
