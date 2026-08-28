// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         HandleReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;




/// <summary>
///     Read-only tool for enumerating open file and kernel object handles using
///     the Sysinternals Handle utility. Handle enumeration is a core forensic
///     primitive for detecting file-locking malware and resource leaks.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class HandleReadTool
{




    /// <summary>
    ///     Probes whether Sysinternals Handle is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_Handle_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals Handle utility is installed and reports its version and path.")]
    public async Task<ToolResult> HandleAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("handle")).ConfigureAwait(false);
    }






    /// <summary>
    ///     Lists open handles for all processes, optionally filtered by a search
    ///     substring matched against handle names.
    /// </summary>
    /// <param name="searchFilter">Optional substring to match against handle names (e.g., a file path fragment).</param>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 200.</param>
    /// <returns>A <see cref="ToolResult" /> containing the handle listing.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_Handle_List", ReadOnly = true, Destructive = false)]
    [Description("Lists open file and kernel object handles using Sysinternals Handle. Requires Handle to be installed; handle enumeration requires elevation.")]
    public async Task<ToolResult> HandleListAsync(
        [Description("Optional substring to match against handle names, e.g. a file path fragment.")] string? searchFilter = null,
        [Description("Maximum number of output lines to return. Defaults to 200.")] int maxLines = 200)
    {
        if (searchFilter is not null)
        {
            ToolResult? filterValidation = SysinternalsHelper.ValidateArgument(searchFilter, "searchFilter");
            if (filterValidation is not null)
            {
                return filterValidation;
            }
        }

        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -a dumps all handle types; -accepteula suppresses the EULA prompt.
        StringBuilder arguments = new("-a -accepteula");
        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            arguments.Append(" \"").Append(searchFilter).Append('"');
        }

        ToolResult result = await SysinternalsHelper.RunAsync("handle", arguments.ToString(), "Handle enumeration").ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "Handle enumeration complete.");
    }
}
