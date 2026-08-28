// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         ProcDumpReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tooling surface for the Sysinternals ProcDump utility.
///     ProcDump writes dump files to disk, which is a state-changing side effect;
///     therefore only the availability probe is marked read-only and the capture
///     operation is honestly declared as non-read-only (spec §4.3, §7.1).
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class ProcDumpReadTool
{

    /// <summary>
    ///     Probes whether Sysinternals ProcDump is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_ProcDump_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals ProcDump utility is installed and reports its version and path.")]
    public async Task<ToolResult> ProcDumpAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("procdump")).ConfigureAwait(false);
    }








    /// <summary>
    ///     Captures a full user-mode dump of a process to a caller-specified file
    ///     path. This operation writes a file and is therefore not read-only.
    /// </summary>
    /// <param name="processId">The numeric PID of the process to dump.</param>
    /// <param name="dumpFilePath">The absolute .dmp file path to write. The containing directory must exist.</param>
    /// <returns>A <see cref="ToolResult" /> containing the ProcDump console output.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_ProcDump_Capture_Dump", ReadOnly = false, Destructive = false)]
    [Description("Captures a full process dump to a specified file path using Sysinternals ProcDump. Writes a dump file; requires ProcDump to be installed and elevation for most processes.")]
    public async Task<ToolResult> ProcDumpCaptureDumpAsync([Description("The numeric PID of the process to dump.")] int processId, [Description("The absolute .dmp file path to write. The containing directory must exist.")] string dumpFilePath)
    {
        ToolResult? pidValidation = InputValidator.ValidatePositive(processId, "processId");
        if (pidValidation is not null)
        {
            return pidValidation;
        }

        ToolResult? pathValidation = SysinternalsHelper.ValidateArgument(dumpFilePath, "dumpFilePath");
        if (pathValidation is not null)
        {
            return pathValidation;
        }

        if (!Path.IsPathRooted(dumpFilePath) || !dumpFilePath.EndsWith(".dmp", StringComparison.OrdinalIgnoreCase))
        {
            return ToolResult.Fail("dumpFilePath must be an absolute path ending in .dmp.", "ProcDump capture");
        }

        string? directory = Path.GetDirectoryName(dumpFilePath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return ToolResult.Fail($"Dump directory does not exist: {directory}", "ProcDump capture");
        }

        // -ma captures full memory; -accepteula suppresses the EULA prompt.
        ToolResult result = await SysinternalsHelper.RunAsync("procdump", $"-ma -accepteula {processId} \"{dumpFilePath}\"", "ProcDump capture", timeoutSeconds: 120).ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, 100), "ProcDump capture complete.");
    }
}