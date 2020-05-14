using System;

namespace Orleans.GrainDirectory
{
    /// <summary>
    /// This attribute is used to indicate the activation should be done in local region.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class RegionalLocalAttribute : Attribute
    {
    }
}
