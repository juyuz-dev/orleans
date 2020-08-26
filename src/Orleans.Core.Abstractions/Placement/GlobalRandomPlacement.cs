using System;

namespace Orleans.Runtime
{
    [Serializable]
    public class GlobalRandomPlacement : PlacementStrategy
    {
        internal static GlobalRandomPlacement Singleton { get; } = new GlobalRandomPlacement();
    }
}
