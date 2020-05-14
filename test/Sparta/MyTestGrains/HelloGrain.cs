using System;
using System.Threading.Tasks;
using MyTestInterfaces;
using Orleans;

namespace MyTestGrains
{
    public class HelloGrain : Grain, IHello
    {
        public Task<string> SayHello(string greeting)
        {
            Console.WriteLine($"####Hello from: {greeting}");
            return Task.FromResult("anything");
        }
    }
}
