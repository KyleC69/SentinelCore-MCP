// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         AccessEnumReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  102702



using System.Runtime.Versioning;

using ModelContextProtocol.Server;




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
}