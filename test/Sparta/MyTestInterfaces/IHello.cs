using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.GrainDirectory;

namespace MyTestInterfaces
{
    public interface IHello : Orleans.IGrainWithGuidKey
    {
        [AlwaysInterleave]
        Task<string> SayHello(string greeting);

        [AlwaysInterleave]
        Task<string> Test();
    }
}
