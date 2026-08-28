// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         ShareEnumReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for reporting the availability of the Sysinternals ShareEnum
///     utility. ShareEnum is a GUI application with no supported command-line
///     interface, so only an availability probe is exposed; share enumeration is
///     available from the managed Network_List_Shares tool.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class ShareEnumReadTool
{

    /// <summary>
    ///     Probes whether Sysinternals ShareEnum is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_ShareEnum_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals ShareEnum utility is installed and reports its version and path. ShareEnum is GUI-only, so no automated enumeration is available.")]
    public async Task<ToolResult> ShareEnumAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("shareenum")).ConfigureAwait(false);
    }
}