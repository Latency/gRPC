// ****************************************************************************
// Project:  Grpc.Client
// File:     Xilinxs.cs
// Author:   Latency McLaughlin
// Date:     07/25/2025
// ****************************************************************************

using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Server;

namespace Grpc.Client;

internal sealed class Xilinxs(GrpcChannel channel) : ClientBase(channel)
{
    private readonly Xilinx.XilinxClient _xilinxServiceClient = new(channel);


    public override async Task Body(string? input)
    {
        var command = input!;
        await GetCommand(command);
    }


    public override Task Greeting() => Task.Run(async () =>
    {
        Console.Write(Environment.GetEnvironmentVariable("WelcomeMessage"));

        for (var x = 0; x < 5; x++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
            Console.Write(".");
        }

        Console.Write($"{Environment.NewLine}{Environment.GetEnvironmentVariable("Prompt")}");
    });


    private async Task GetCommand(string command)
    {
        var getCommand = new XilinxRequest
        {
            Command = command
        };

        try
        {
            var reply = _xilinxServiceClient.SendCommand(getCommand, await Header());
            if (reply is not null)
                Console.WriteLine(reply.Message);
            Console.Write(Environment.GetEnvironmentVariable("Prompt"));
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{ex.Status.StatusCode} - {ex.Status.Detail}");
        }
    }
}