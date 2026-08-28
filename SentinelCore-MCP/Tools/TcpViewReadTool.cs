// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         TcpViewReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;




/// <summary>
///     Read-only tool for enumerating TCP/UDP endpoints using Tcpvcon, the
///     command-line companion of the Sysinternals TcpView utility. Endpoint
///     attribution to owning processes is a core network forensics primitive.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class TcpViewReadTool
{




    /// <summary>
    ///     Probes whether Sysinternals TcpView (Tcpvcon) is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_TcpView_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals Tcpvcon utility (TcpView command-line companion) is installed and reports its version and path.")]
    public async Task<ToolResult> TcpViewAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("tcpvcon")).ConfigureAwait(false);
    }






    /// <summary>
    ///     Lists active TCP/UDP endpoints with owning process attribution using
    ///     Tcpvcon.
    /// </summary>
    /// <param name="includeUdp">Whether to include UDP endpoints. Defaults to true.</param>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 200.</param>
    /// <returns>A <see cref="ToolResult" /> containing the endpoint listing.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_TcpView_List_Endpoints", ReadOnly = true, Destructive = false)]
    [Description("Lists active TCP/UDP endpoints with owning process attribution using Sysinternals Tcpvcon. Requires Tcpvcon to be installed; process attribution requires elevation.")]
    public async Task<ToolResult> TcpViewListEndpointsAsync(
        [Description("Whether to include UDP endpoints. Defaults to true.")] bool includeUdp = true,
        [Description("Maximum number of output lines to return. Defaults to 200.")] int maxLines = 200)
    {
        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -a includes all endpoint states, -c renders CSV, -n resolves addresses;
        // -accepteula suppresses the EULA prompt.
        string arguments = includeUdp
            ? "-a -c -n -accepteula"
            : "-a -c -n -p -accepteula";

        ToolResult result = await SysinternalsHelper.RunAsync("tcpvcon", arguments, "Tcpvcon endpoint enumeration").ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "Tcpvcon endpoint enumeration complete.");
    }
}
