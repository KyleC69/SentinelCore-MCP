// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         AccessEnumReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for reporting the availability of the Sysinternals AccessEnum
///     utility. AccessEnum is a GUI application with no supported command-line
///     interface, so only an availability probe is exposed; the tool fails
///     gracefully when the binary is not installed (spec §2.4).
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class AccessEnumReadTool
{

    /// <summary>
    ///     Probes whether Sysinternals AccessEnum is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_AccessEnum_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals AccessEnum utility is installed and reports its version and path. AccessEnum is GUI-only, so no automated enumeration is available.")]
    public async Task<ToolResult> AccessEnumAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("accessenum")).ConfigureAwait(false);
    }
}