// ****************************************************************************
// Project:  Grpc.Server
// File:     XilinxsService.cs
// Author:   Latency McLaughlin
// Date:     07/24/2025
// ****************************************************************************

using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.Reflection;

namespace Grpc.Server.Services;

// Here "Xilinx" in "Xilinx.XilinxBase" refers to the "Xilinx" service in the proto file
public sealed class XilinxsService : Xilinx.XilinxBase
{
    private readonly ILogger<XilinxsService> _logger;
    private readonly ProcessStartInfo        _psi;
    private          Process?                _process;


    /// <summary>
    ///     Constructor
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public XilinxsService(ILogger<XilinxsService> logger)
    {
        const string key        = "XilinxPath";
        var          xilinxPath = Environment.GetEnvironmentVariable(key);

        if (xilinxPath is null)
            throw new NullReferenceException($@"'{key}' is not found within the environment variables @ 'Properties\launchSettings.json'");

        _logger = logger;
        _psi = new ProcessStartInfo(xilinxPath)
        {
            RedirectStandardInput  = true,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false
        };
    }


    /// <summary>
    ///     Sends a command to the Xilinx shell and awaits the reply.
    /// </summary>
    /// <remarks>
    ///     Override the 'SendCommand' method.
    /// </remarks>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns><see cref="XilinxReply"/></returns>
    [Authorize]
    public override Task<XilinxReply> SendCommand(XilinxRequest request, ServerCallContext context)
    {
        string message;

        _logger.LogDebug("Sending command:  '{RequestCommand}'", request.Command);

        switch (request.Command?.ToLower())
        {
            case "help":
                if (_process is null)
                    message = """
                              Available Help Commands

                              close         - Closes the remote Xilinx process.
                              open          - Opens the remote Xilinx process.
                              <cmd>         - Xilinx command to send.  [Xilinx process must already be opened]

                              """;
                else
                    goto default;
                break;
            case "open":
                if (_process is not null)
                    message = CheckStatus(true);
                else
                {
                    _process = Process.Start(_psi);

                    if (_process is null)
                    {
                        message = $"Unable to start the '{_psi.FileName}' process.{Environment.NewLine}Check the file name to ensure it exists.";
                        _logger.LogError(message);
                    }
                    else
                        message = Log(request.Command.ToUpperInvariant());
                }
                break;
            case "close":
                if (_process is null)
                    message = CheckStatus(false);
                else
                {
                    _process.Close();
                    _process = null;
                    message  = Log(request.Command.ToUpperInvariant());
                }
                break;
            default:
                if (_process is null)
                    return Task.FromResult(new XilinxReply
                    {
                        Message = CheckStatus(false).Replace("already ", "")
                    });

                _process.StandardInput.WriteLine(request.Command);
                message = _process.StandardOutput.ReadToEnd();
                break;
        }

        // Return a output from the Xilinx shell.
        return Task.FromResult(new XilinxReply
        {
            Message = message
        });
    }


    private string Log(string methodName)
    {
        var msg = $"{methodName}ing the XSCT console";
        _logger.LogInformation(msg);
        return msg;
    }


    private static string CheckStatus(bool isOpening) => $"The XSCT console is already {(isOpening ? "open" : "clos")}ed.";
}