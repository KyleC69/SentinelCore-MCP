// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         WinObjReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;




/// <summary>
///     Read-only tool for reporting the availability of the Sysinternals WinObj
///     utility. WinObj is a GUI application with no supported command-line
///     interface, and the NT object manager namespace has no managed enumeration
///     API, so only an availability probe is exposed (spec §6.2: avoid P/Invoke).
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class WinObjReadTool
{




    /// <summary>
    ///     Probes whether Sysinternals WinObj is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_WinObj_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals WinObj utility is installed and reports its version and path. WinObj is GUI-only, so no automated NT object namespace enumeration is available.")]
    public async Task<ToolResult> WinObjAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("winobj")).ConfigureAwait(false);
    }
}