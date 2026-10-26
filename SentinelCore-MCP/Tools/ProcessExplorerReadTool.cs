// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         ProcessExplorerReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for reporting the availability of the Sysinternals
///     ProcessExplorer utility. ProcessExplorer is a GUI application with no
///     supported command-line interface; process-level detail is available from
///     the managed Process_List and Process_Read tools.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class ProcessExplorerReadTool
{
    // ProcessExplorer is GUI-only with no command-line interface. No tools exposed.
}
