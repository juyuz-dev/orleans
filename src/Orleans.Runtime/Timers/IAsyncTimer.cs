using System;
using System.Threading.Tasks;

namespace Orleans.Runtime
{
    public interface IAsyncTimer : IDisposable, IHealthCheckable
    {
        Task<bool> NextTick(TimeSpan? overrideDelay = default);
    }
}
