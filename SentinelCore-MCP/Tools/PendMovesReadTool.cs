// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         PendMovesReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for enumerating pending file rename and delete operations
///     registered in the PendingFileRenameOperations registry value, replicating
///     the Sysinternals PendMoves diagnostic. Malware commonly stages payloads via
///     this mechanism, making it a high-value forensic surface.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class PendMovesReadTool
{

    /// <summary>
    ///     Probes whether Sysinternals PendMoves is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PendMoves_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals PendMoves utility is installed and reports its version and path.")]
    public async Task<ToolResult> PendMovesAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("pendmoves")).ConfigureAwait(false);
    }








    /// <summary>
    ///     Lists file rename and delete operations scheduled for the next reboot by
    ///     reading the PendingFileRenameOperations registry value directly. This
    ///     managed read does not execute the PendMoves binary and requires no
    ///     elevation.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the pending rename operations.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PendMoves_List_Pending", ReadOnly = true, Destructive = false)]
    [Description("Lists file operations scheduled for the next reboot from PendingFileRenameOperations (PendMoves equivalent). Managed read; no Sysinternals binary required.")]
    public async Task<ToolResult> PendMovesListPendingAsync()
    {
        const string sessionManagerKey = @"SYSTEM\CurrentControlSet\Control\Session Manager";
        const string pendingRenamesValue = "PendingFileRenameOperations";

        return await Task.Run(() =>
                {
                    try
                    {
                        using Microsoft.Win32.RegistryKey? key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sessionManagerKey, false);
                        if (key is null)
                        {
                            return ToolResult.Fail($"Registry key not found: HKLM\\{sessionManagerKey}", "Pending moves read");
                        }

                        if (key.GetValue(pendingRenamesValue) is not string[] operations || operations.Length == 0)
                        {
                            return ToolResult.Ok("No pending file rename operations.", "Pending moves read complete.");
                        }

                        System.Text.StringBuilder sb = new();
                        sb.AppendLine($"[{pendingRenamesValue}]");
                        for (int i = 0; i < operations.Length; i += 2)
                        {
                            string source = operations[i];
                            string destination = i + 1 < operations.Length ? operations[i + 1] : string.Empty;
                            sb.AppendLine(destination.Length > 0 ? $"  Rename: {source} -> {destination}" : $"  Delete: {source}");
                        }

                        return ToolResult.Ok(sb.ToString(), "Pending moves read complete.");
                    }
                    catch (Exception ex)
                    {
                        return ToolResult.Fail(ex.Message, "Pending moves read");
                    }
                })
                .ConfigureAwait(false);
    }
}