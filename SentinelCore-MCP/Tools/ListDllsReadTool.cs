// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         ListDllsReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;




/// <summary>
///     Read-only tool for enumerating DLLs loaded into processes using the
///     Sysinternals ListDLLs utility. Loaded-module inventory is a core primitive
///     for DLL injection and DLL hijacking detection.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class ListDllsReadTool
{




    /// <summary>
    ///     Probes whether Sysinternals ListDLLs is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_ListDlls_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals ListDLLs utility is installed and reports its version and path.")]
    public async Task<ToolResult> ListDllsAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("listdlls")).ConfigureAwait(false);
    }






    /// <summary>
    ///     Lists DLLs loaded by all processes, or by a specific process when a name
    ///     or PID is supplied.
    /// </summary>
    /// <param name="processNameOrPid">Optional process name (e.g., explorer) or numeric PID to scope the listing.</param>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 300.</param>
    /// <returns>A <see cref="ToolResult" /> containing the loaded-DLL listing.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_ListDlls_List_Loaded", ReadOnly = true, Destructive = false)]
    [Description("Lists DLLs loaded by processes using Sysinternals ListDLLs. Requires ListDLLs to be installed; full output requires elevation.")]
    public async Task<ToolResult> ListDllsListLoadedAsync(
        [Description("Optional process name (e.g., explorer) or numeric PID to scope the listing.")] string? processNameOrPid = null,
        [Description("Maximum number of output lines to return. Defaults to 300.")] int maxLines = 300)
    {
        if (processNameOrPid is not null)
        {
            ToolResult? processValidation = SysinternalsHelper.ValidateArgument(processNameOrPid, "processNameOrPid");
            if (processValidation is not null)
            {
                return processValidation;
            }

            // Numeric PIDs and simple image names are the only accepted shapes.
            if (!uint.TryParse(processNameOrPid, out _) &&
                !System.Text.RegularExpressions.Regex.IsMatch(processNameOrPid, @"^[a-zA-Z0-9._\- ]+$"))
            {
                return ToolResult.Fail(
                    "processNameOrPid must be a numeric PID or a simple image name (letters, digits, dots, hyphens, underscores, spaces).",
                    "ListDLLs enumeration");
            }
        }

        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -accepteula suppresses the EULA prompt.
        string arguments = string.IsNullOrWhiteSpace(processNameOrPid)
            ? "-accepteula"
            : $"-accepteula \"{processNameOrPid}\"";

        ToolResult result = await SysinternalsHelper.RunAsync("listdlls", arguments, "ListDLLs enumeration").ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "ListDLLs enumeration complete.");
    }
}
