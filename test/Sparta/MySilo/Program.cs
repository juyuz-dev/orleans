using System;
using System.Threading.Tasks;
using MyTestGrains;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Microsoft.Extensions.Logging;

namespace MySilo
{
    class Program
    {
        public static int Main(string[] args)
        {
            int siloPort = 20001;
            int gatewayPort = 20002;
            int region = 1;

            if (args.Length >= 3)
            {
                region = int.Parse(args[0]);
                siloPort = int.Parse(args[1]);
                gatewayPort = int.Parse(args[2]);
            }

            return RunMainAsync(region, siloPort, gatewayPort).Result;
        }

        private static async Task<int> RunMainAsync(int region, int siloPort, int gatewayPort)
        {
            try
            {
                var host = await StartSilo(region, siloPort, gatewayPort);
                Console.WriteLine("\n\n Press Enter to terminate...\n\n");
                Console.ReadLine();

                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo(int region, int siloPort, int gatewayPort)
        {
            // define the cluster configuration
            var builder = new SiloHostBuilder()
                .UseAzureStorageGlobalClustering(options =>
                {
                    options.ConnectionString = "<>";
                    options.TableName = "GlobalClustering";
                })
                .UseAzureStorageClustering(options =>
                {
                    options.ConnectionString = "<>";
                    options.TableName = $"LocalClustering{region}";
                })
                .AddAzureTableGrainDirectory("TestAzureDirectory", options =>
                {
                    options.ConnectionString = "<>";
                    options.TableName = "GrainDirectory";
                })
                .ConfigureEndpoints("localhost", siloPort, gatewayPort, System.Net.Sockets.AddressFamily.InterNetwork, true)
                // .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansBasics";
                    options.Region = "West US2";
                })

                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
