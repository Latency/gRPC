// ****************************************************************************
// Project:  Grpc.Client
// File:     Program.cs
// Author:   Latency McLaughlin
// Date:     07/24/2025
// ****************************************************************************

using Grpc.Net.Client;

namespace Grpc.Client;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var grpcChannel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("ServerAddress")!);

        var obj =
#if XILINX
        new Xilinxs(grpcChannel);
#elif USERS
        new Users(grpcChannel);
#endif

        await obj.Greeting();

        while (true)
        {
            var input = Console.ReadLine();
            if (input == "exit")
                break;

            try
            {
                await obj.Body(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid input:  {ex.Message}");
            }
        }

        Console.WriteLine("End");
    }
}