// ****************************************************************************
// Project:  Grpc.Server
// File:     XilinxService.cs
// Author:   Latency McLaughlin
// Date:     07/24/2025
// ****************************************************************************

using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.Reflection;

namespace Grpc.Server.Services;

// Here "Xilinx" in "Xilinx.XilinxBase" refers to the "Xilinx" service in the proto file
public class XilinxService : Xilinx.XilinxBase
{
    private readonly ILogger<XilinxService> _logger;
    private readonly ProcessStartInfo       _psi;
    private          Process?               _process;


    /// <summary>
    ///     Constructor
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public XilinxService(ILogger<XilinxService> logger)
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
        if (_process is null)
            return Task.FromResult(new XilinxReply
            {
                Message = CheckStatus(false).Replace("is already ", "")
            });

        _logger.LogDebug($"Sending command:  '{request.Command}'");

        _process.StandardInput.WriteLine(request.Command);
        var message = _process.StandardOutput.ReadToEnd();

        // Return a output from the Xilinx shell.
        return Task.FromResult(new XilinxReply
        {
            Message = message
        });
    }


    /// <summary>
    ///     Opens the Xilinx stream.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Authorize]
    public override Task<XilinxReply> Open(Void _, ServerCallContext context)
    {
        if (_process is not null)
            return Task.FromResult(new XilinxReply
            {
                Message = CheckStatus(true)
            });

        string message;
        _process = Process.Start(_psi);

        if (_process is null)
        {
            message = $"Unable to start the '{_psi.FileName}' process.{Environment.NewLine}Check the file name to ensure it exists.";
            _logger.LogError(message);
        }
        else
        {
            message = Log(MethodBase.GetCurrentMethod()!.Name);
        }

        // Return a output from the Xilinx shell.
        return Task.FromResult(new XilinxReply
        {
            Message = message
        });
    }


    /// <summary>
    ///     Closes the Xilinx stream.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Authorize]
    public override Task<XilinxReply> Close(Void _, ServerCallContext context)
    {
        if (_process is null)
            return Task.FromResult(new XilinxReply
            {
                Message = CheckStatus(false)
            });

        var message = Log(MethodBase.GetCurrentMethod()!.Name);
        _process.Close();
        _process = null;

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


    private static string CheckStatus(bool isOpening)
    {
        return $"The XSCT console is already {(isOpening ? "open" : "clos")}ed.";
    }
}