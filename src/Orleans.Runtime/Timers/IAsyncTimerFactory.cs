using System;

namespace Orleans.Runtime
{
    public interface IAsyncTimerFactory
    {
        IAsyncTimer Create(TimeSpan period, string name);
    }
}
