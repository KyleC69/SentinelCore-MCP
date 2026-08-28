// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         PsToolsReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;




/// <summary>
///     Read-only tool for reporting which members of the Sysinternals PsTools
///     suite are installed on the host. PsTools binaries are console utilities;
///     the individual read-only members (PsInfo, PsList, PsLogList, PsService)
///     are exposed as dedicated tools.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class PsToolsReadTool
{




    /// <summary>
    ///     Probes each PsTools suite member and reports its installation state,
    ///     path, and version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the suite inventory.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PsTools_List_Suite", ReadOnly = true, Destructive = false)]
    [Description("Reports which Sysinternals PsTools utilities are installed, with their paths and versions. State-changing members (PsExec, PsKill, etc.) are reported for inventory only and are never executed.")]
    public async Task<ToolResult> PsToolsListSuiteAsync()
    {
        return await Task.Run(() =>
        {
            List<SysinternalsHelper.SysinternalsBinaryInfo> results = new();

            foreach (string member in SysinternalsHelper.PsToolsSuiteMembers)
            {
                ToolResult probe = SysinternalsHelper.ProbeAvailability(member);
                if (probe.Success && probe.Results is SysinternalsHelper.SysinternalsBinaryInfo info)
                {
                    results.Add(info);
                }
            }

            return ToolResult.Ok(results, $"Probed {results.Count} PsTools suite member(s).");
        }).ConfigureAwait(false);
    }
}
