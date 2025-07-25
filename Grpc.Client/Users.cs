// ****************************************************************************
// Project:  Grpc.Client
// File:     Users.cs
// Author:   Latency McLaughlin
// Date:     07/25/2025
// ****************************************************************************

using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Server;

namespace Grpc.Client;

internal sealed class Users(GrpcChannel channel) : ClientBase(channel)
{
    private readonly User.UserClient _userServiceClient = new(channel);


    public override async Task Body(string? input)
    {
        var userId = int.Parse(input!);
        await GetUserById(userId);
    }


    public override async Task Greeting()
    {
        GetUsersInGroupList();
        Console.WriteLine("");
        await GetUsersInGroupStream();

        Console.WriteLine($"{Environment.NewLine}Enter the ID of the user to list their details, or type \"exit\" to quit.");
    }


    private async Task GetUsersInGroupStream()
    {
        var getUsersInGroupViewModel = new GetUsersInGroupViewModel
        {
            GroupId = 1
        };

        using var call = _userServiceClient.GetUsersInGroupStream(getUsersInGroupViewModel);
        if (call is null)
            return;

        while (await call.ResponseStream.MoveNext())
        {
            var user = call.ResponseStream.Current;
            if (user is null)
                return;

            Console.WriteLine($"{user.Id} - {user.FirstName} {user.Surname} - {user.EmailAddress} - {user.Role}|{user.MembershipId}");
        }
    }


    private void GetUsersInGroupList()
    {
        var getUsersInGroupViewModel = new GetUsersInGroupViewModel
        {
            GroupId = 1
        };

        var userList = _userServiceClient.GetUsersInGroupList(getUsersInGroupViewModel);
        if (userList?.Users == null)
            return;

        foreach (var user in userList.Users)
            Console.WriteLine($"{user.Id} - {user.FirstName} {user.Surname} - {user.EmailAddress} - {user.Role}|{user.MembershipId}");
    }


    private async Task GetUserById(int userId)
    {
        var getUserByIdViewModel = new GetUserByIdViewModel
        {
            UserId = userId
        };

        try
        {
            var user = _userServiceClient.GetUser(getUserByIdViewModel, await Header());
            if (user is not null)
                Console.WriteLine($"{user.Id} - {user.FirstName} {user.Surname} - {user.EmailAddress}");
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{ex.Status.StatusCode} - {ex.Status.Detail}");
        }
    }
}