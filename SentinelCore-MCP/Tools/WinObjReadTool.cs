// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         WinObjReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;




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

    // WinObj is GUI-only with no command-line interface. No tools exposed.
}
