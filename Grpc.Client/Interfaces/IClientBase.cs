// ****************************************************************************
// Project:  Grpc.Client
// File:     ClientBase.cs
// Author:   Latency McLaughlin
// Date:     07/25/2025
// ****************************************************************************

namespace Grpc.Client.Interfaces;

internal interface IClientBase
{
    Task Body(string? input);
    Task Greeting();
}