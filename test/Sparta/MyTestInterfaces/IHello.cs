using System.Threading.Tasks;
using Orleans.GrainDirectory;

namespace MyTestInterfaces
{
    [RegionalLocal]
    public interface IHello : Orleans.IGrainWithGuidKey
    {
        Task<string> SayHello(string greeting);
    }
}
