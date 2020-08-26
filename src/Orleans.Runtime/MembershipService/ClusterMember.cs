using System;

namespace Orleans.Runtime
{
    [Serializable]
    public sealed class ClusterMember : IEquatable<ClusterMember>
    {
        public ClusterMember(SiloAddress siloAddress, SiloStatus status, string region)
        {
            this.SiloAddress = siloAddress ?? throw new ArgumentNullException(nameof(siloAddress));
            this.Status = status;
            this.Region = region;
        }

        public SiloAddress SiloAddress { get; }
        public SiloStatus Status { get; }

        public string Region { get; }

        public override bool Equals(object obj) => this.Equals(obj as ClusterMember);

        public bool Equals(ClusterMember other) => other != null && this.SiloAddress.Equals(other.SiloAddress) && this.Status == other.Status;

        public override int GetHashCode() => this.SiloAddress.GetConsistentHashCode();

        public override string ToString() => $"{this.SiloAddress}/{this.Status}";
    }
}
