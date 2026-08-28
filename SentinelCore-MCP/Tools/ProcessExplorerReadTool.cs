// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         ProcessExplorerReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;




/// <summary>
///     Read-only tool for reporting the availability of the Sysinternals
///     ProcessExplorer utility. ProcessExplorer is a GUI application with no
///     supported command-line interface, so only an availability probe is exposed;
///     process-level detail is available from the managed Process_List and
///     Process_Read tools.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class ProcessExplorerReadTool
{




    /// <summary>
    ///     Probes whether Sysinternals ProcessExplorer is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_ProcessExplorer_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals ProcessExplorer utility is installed and reports its version and path. ProcessExplorer is GUI-only, so no automated inspection is available.")]
    public async Task<ToolResult> ProcessExplorerAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("procexp")).ConfigureAwait(false);
    }
}
