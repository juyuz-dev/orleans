using System;

namespace Orleans.Runtime
{
    [Serializable]
    public class GlobalPreferLocalPlacement : PlacementStrategy
    {
        internal static GlobalPreferLocalPlacement Singleton { get; } = new GlobalPreferLocalPlacement();
    }
}
