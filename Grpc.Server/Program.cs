// ****************************************************************************
// Project:  Grpc.Server
// File:     Program.cs
// Author:   Latency McLaughlin
// Date:     07/24/2025
// ****************************************************************************

namespace Grpc.Server;

internal class Program
{
    internal static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    internal static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}