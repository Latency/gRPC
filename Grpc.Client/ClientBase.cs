// ****************************************************************************
// Project:  Grpc.Client
// File:     ClientBase.cs
// Author:   Latency McLaughlin
// Date:     07/25/2025
// ****************************************************************************

using Grpc.Client.Interfaces;
using Grpc.Core;
using Grpc.Net.Client;

namespace Grpc.Client;

internal abstract class ClientBase(GrpcChannel channel) : IClientBase
{
    public abstract Task Body(string? input);
    public virtual  Task Greeting() => Task.CompletedTask;


    protected async Task<Metadata> Header() => new()
    {
        { "Authorization", $"Bearer {await GetJwt()}" }
    };


    private async Task<string> GetJwt()
    {
        var httpClient = new HttpClient();
        var request    = new HttpRequestMessage
        {
            RequestUri                           = new Uri($"https://{channel.Target}/jwt"),
            Method     = HttpMethod.Get, Version = new Version(2, 0)
        };
        var jwtResponse = await httpClient.SendAsync(request);

        jwtResponse.EnsureSuccessStatusCode();

        return await jwtResponse.Content.ReadAsStringAsync();
    }
}