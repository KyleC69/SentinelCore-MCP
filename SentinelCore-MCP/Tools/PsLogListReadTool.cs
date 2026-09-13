// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         PsLogListReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for dumping event log records using the Sysinternals
///     PsLogList utility. Complements the managed Event_Log_Query tool with
///     Sysinternals-native formatting.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class PsLogListReadTool
{

    /// <summary>
    ///     Probes whether Sysinternals PsLogList is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PsLogList_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals PsLogList utility is installed and reports its version and path.")]
    public async Task<ToolResult> PsLogListAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("psloglist")).ConfigureAwait(false);
    }








    /// <summary>
    ///     Dumps recent events from an event log channel using PsLogList.
    /// </summary>
    /// <param name="logName">The event log name to dump, e.g. Application, System, Security.</param>
    /// <param name="maxEvents">Maximum number of events to dump. Defaults to 50.</param>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 200.</param>
    /// <returns>A <see cref="ToolResult" /> containing the event dump.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PsLogList_Dump_Events", ReadOnly = true, Destructive = false)]
    [Description("Dumps recent event log records using Sysinternals PsLogList. Requires PsLogList to be installed; the Security log requires elevation.")]
    public async Task<ToolResult> PsLogListDumpEventsAsync([Description("The event log name to dump, e.g. Application, System, Security.")] string logName, [Description("Maximum number of events to dump. Defaults to 50.")] int maxEvents = 50, [Description("Maximum number of output lines to return. Defaults to 200.")] int maxLines = 200)
    {
        ToolResult? logValidation = SysinternalsHelper.ValidateArgument(logName, "logName");
        if (logValidation is not null)
        {
            return logValidation;
        }

        // Log names are restricted to a safe character set to prevent argument shaping.
        if (!System.Text.RegularExpressions.Regex.IsMatch(logName!, @"^[a-zA-Z0-9 _\-]+$"))
        {
            return ToolResult.Fail("Invalid log name format.", "PsLogList dump");
        }

        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxEvents, "maxEvents");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        ToolResult? maxLinesValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -n limits the event count; -accepteula suppresses the EULA prompt.
        ToolResult result = await SysinternalsHelper.RunAsync("psloglist", $"-n {maxEvents} -accepteula \"{logName}\"", "PsLogList dump").ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "PsLogList dump complete.");
    }
}
