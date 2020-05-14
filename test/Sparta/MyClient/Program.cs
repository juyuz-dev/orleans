using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyTestInterfaces;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace MyClient
{
    class Program
    {
        static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                using (var client = await ConnectClient())
                {
                    for (int i = 0; i < 100; i++)
                    {
                        await Task.Delay(1000);
                        await DoClientWork(client);
                    }
                    Console.ReadKey();
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nException while trying to run client: {e.Message}");
                Console.WriteLine("Make sure the silo the client is trying to connect to is running.");
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
                return 1;
            }
        }

        private static async Task<IClusterClient> ConnectClient()
        {
            IClusterClient client;
            client = new ClientBuilder()
                .UseAzureStorageClustering(options =>
                {
                    options.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=juyuzorleanstest;AccountKey=ZPGqxB/Vzpjo0k6oJm9PhCi3jEULfn+gXQVQZtLOYRmiew667HYEv+D6/kXvfIobY+76LeEdpr0DaRq6S/N2Hg==;EndpointSuffix=core.windows.net";
                })
                // .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansBasics";
                    options.Region = 2;
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            await client.Connect();
            Console.WriteLine("Client successfully connected to silo host \n");
            return client;
        }

        private static async Task DoClientWork(IClusterClient client)
        {
            // example of calling grains from the initialized client
            var friend = client.GetGrain<IHello>(Guid.NewGuid());
            var response = await friend.SayHello("Good morning, HelloGrain!");
            Console.WriteLine("\n\n{0}\n\n", response);
        }
    }
}
