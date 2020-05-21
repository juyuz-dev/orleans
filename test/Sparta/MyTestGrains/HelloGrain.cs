using System;
using System.Threading.Tasks;
using MyTestInterfaces;
using Orleans;
using Orleans.GrainDirectory;
using Orleans.Placement;

namespace MyTestGrains
{
    [GlobalRandomPlacement]
    [GrainDirectory(GrainDirectoryName = "TestAzureDirectory")]
    public class HelloGrain : Grain, IHello
    {
        public async Task<string> SayHello(string greeting)
        {
            Console.WriteLine($"####Hello from: {greeting}");

            var a = this.GrainFactory.GetGrain<IHello>(Guid.NewGuid());
            await a.Test();

            return "anything";
        }

        public Task<string> Test()
        {
            Console.WriteLine("Inside test ...");

            return Task.FromResult("abc");
        }
    }
}
