// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         LogonSessionsReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;




/// <summary>
///     Read-only tool for enumerating active logon sessions and the processes
///     running in them using the Sysinternals LogonSessions utility.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class LogonSessionsReadTool
{




    /// <summary>
    ///     Probes whether Sysinternals LogonSessions is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_LogonSessions_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals LogonSessions utility is installed and reports its version and path.")]
    public async Task<ToolResult> LogonSessionsAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("logonsessions")).ConfigureAwait(false);
    }






    /// <summary>
    ///     Lists active logon sessions with their authentication package, SID, and
    ///     associated processes.
    /// </summary>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 200.</param>
    /// <returns>A <see cref="ToolResult" /> containing the logon session listing.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_LogonSessions_List_Active", ReadOnly = true, Destructive = false)]
    [Description("Lists active logon sessions with authentication details using Sysinternals LogonSessions. Requires LogonSessions to be installed.")]
    public async Task<ToolResult> LogonSessionsListActiveAsync(
        [Description("Maximum number of output lines to return. Defaults to 200.")] int maxLines = 200)
    {
        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -p lists processes in each session; -accepteula suppresses the EULA prompt.
        ToolResult result = await SysinternalsHelper.RunAsync("logonsessions", "-p -accepteula", "Logon session enumeration").ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "Logon session enumeration complete.");
    }
}
