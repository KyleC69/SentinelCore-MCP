// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         ShareEnumReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for reporting the availability of the Sysinternals ShareEnum
///     utility. ShareEnum is a GUI application with no supported command-line
///     interface; share enumeration is available from the managed Network_List_Shares tool.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class ShareEnumReadTool
{
    // ShareEnum is GUI-only with no command-line interface. No tools exposed.
}
