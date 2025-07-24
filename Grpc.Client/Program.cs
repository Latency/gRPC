// ****************************************************************************
// Project:  Grpc.Client
// File:     Program.cs
// Author:   Latency McLaughlin
// Date:     07/24/2025
// ****************************************************************************

using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Server;

namespace Grpc.Client;

internal class Program
{
    private static readonly string GrpcServerAddress = Environment.GetEnvironmentVariable("ServerAddress")!;

    private static async Task Main(string[] args)
    {
        using var grpcChannel       = GrpcChannel.ForAddress(GrpcServerAddress);
        var       userServiceClient = new User.UserClient(grpcChannel);
        var       xilinxServiceClient = new Xilinx.XilinxClient(grpcChannel);

        GetUsersInGroupList(userServiceClient);
        Console.WriteLine("");
        await GetUsersInGroupStream(userServiceClient);

        //Console.WriteLine($"{Environment.NewLine}Enter the ID of the user to list their details, or type \"exit\" to quit.");

        while (true)
        {
            var input = Console.ReadLine();
            if (input == "exit")
                break;

            try
            {
                var userId  = int.Parse(input!);
                var command = input!;
                //await GetCommand(command, xilinxServiceClient);
                await GetUserById(userId, userServiceClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid input:  {ex.Message}");
            }
        }

        Console.WriteLine("End");
    }

    private static async Task GetUsersInGroupStream(User.UserClient userServiceClient)
    {
        var getUsersInGroupViewModel = new GetUsersInGroupViewModel
        {
            GroupId = 1
        };

        using var call = userServiceClient.GetUsersInGroupStream(getUsersInGroupViewModel);
        while (await call.ResponseStream.MoveNext())
        {
            var user = call.ResponseStream.Current;

            Console.WriteLine($"{user.Id} - {user.FirstName} {user.Surname} - {user.EmailAddress} - {user.Role}|{user.MembershipId}");
        }
    }

    private static void GetUsersInGroupList(User.UserClient userServiceClient)
    {
        var getUsersInGroupViewModel = new GetUsersInGroupViewModel
        {
            GroupId = 1
        };

        var userList = userServiceClient.GetUsersInGroupList(getUsersInGroupViewModel);

        foreach (var user in userList.Users)
            Console.WriteLine($"{user.Id} - {user.FirstName} {user.Surname} - {user.EmailAddress} - {user.Role}|{user.MembershipId}");
    }

    private static async Task GetUserById(int userId, User.UserClient userServiceClient)
    {
        var jwt = await GetJwt();
        var headers = new Metadata
        {
            { "Authorization", $"Bearer {jwt}" }
        };

        var getUserByIdViewModel = new GetUserByIdViewModel
        {
            UserId = userId
        };

        try
        {
            var user = userServiceClient.GetUser(getUserByIdViewModel, headers);

            Console.WriteLine($"{user.Id} - {user.FirstName} {user.Surname} - {user.EmailAddress}");
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{ex.Status.StatusCode} - {ex.Status.Detail}");
        }
    }


    private static async Task GetCommand(string command, Xilinx.XilinxClient xilinxServiceClient)
    {
        var jwt = await GetJwt();
        var headers = new Metadata
        {
            { "Authorization", $"Bearer {jwt}" }
        };

        var getCommand = new XilinxRequest
        {
            Command = command
        };

        try
        {
            var reply = xilinxServiceClient.SendCommand(getCommand, headers);

            Console.WriteLine(reply.Message);
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{ex.Status.StatusCode} - {ex.Status.Detail}");
        }
    }


    private static async Task<string> GetJwt()
    {
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri($"{GrpcServerAddress}/jwt"),
            Method     = HttpMethod.Get,
            Version    = new Version(2, 0)
        };
        var jwtResponse = await httpClient.SendAsync(request);

        jwtResponse.EnsureSuccessStatusCode();

        var jwt = await jwtResponse.Content.ReadAsStringAsync();
        return jwt;
    }
}