// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         PsListReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for enumerating process and thread detail (CPU time, context
///     switches, thread states) using the Sysinternals PsList utility.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class PsListReadTool
{

    /// <summary>
    ///     Probes whether Sysinternals PsList is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PsList_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals PsList utility is installed and reports its version and path.")]
    public async Task<ToolResult> PsListAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("pslist")).ConfigureAwait(false);
    }








    /// <summary>
    ///     Lists running processes with kernel/user CPU time and thread counts using
    ///     PsList.
    /// </summary>
    /// <param name="processNameOrPid">Optional process name or numeric PID to scope the listing.</param>
    /// <param name="includeThreads">Whether to include per-thread statistics. Defaults to false.</param>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 200.</param>
    /// <returns>A <see cref="ToolResult" /> containing the process listing.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PsList_List_Processes", ReadOnly = true, Destructive = false)]
    [Description("Lists processes with CPU and memory statistics using Sysinternals PsList. Requires PsList to be installed.")]
    public async Task<ToolResult> PsListListProcessesAsync([Description("Optional process name (e.g., explorer) or numeric PID to scope the listing.")] string? processNameOrPid = null, [Description("Whether to include per-thread statistics. Defaults to false.")] bool includeThreads = false, [Description("Maximum number of output lines to return. Defaults to 200.")] int maxLines = 200)
    {
        if (processNameOrPid is not null)
        {
            ToolResult? processValidation = SysinternalsHelper.ValidateArgument(processNameOrPid, "processNameOrPid");
            if (processValidation is not null)
            {
                return processValidation;
            }

            if (!uint.TryParse(processNameOrPid, out _) && !System.Text.RegularExpressions.Regex.IsMatch(processNameOrPid, @"^[a-zA-Z0-9._\- ]+$"))
            {
                return ToolResult.Fail("processNameOrPid must be a numeric PID or a simple image name (letters, digits, dots, hyphens, underscores, spaces).", "PsList enumeration");
            }
        }

        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        StringBuilder arguments = new("-accepteula");
        if (includeThreads)
        {
            arguments.Append(" -t");
        }

        if (!string.IsNullOrWhiteSpace(processNameOrPid))
        {
            arguments.Append(" \"").Append(processNameOrPid).Append('"');
        }

        ToolResult result = await SysinternalsHelper.RunAsync("pslist", arguments.ToString(), "PsList enumeration").ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "PsList enumeration complete.");
    }
}