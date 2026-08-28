// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         PsInfoReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for enumerating system information (OS version, kernel,
///     install date, hotfixes) using the Sysinternals PsInfo utility.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class PsInfoReadTool
{

    /// <summary>
    ///     Probes whether Sysinternals PsInfo is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PsInfo_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals PsInfo utility is installed and reports its version and path.")]
    public async Task<ToolResult> PsInfoAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("psinfo")).ConfigureAwait(false);
    }








    /// <summary>
    ///     Reports operating system, kernel, install date, and hotfix information
    ///     using PsInfo.
    /// </summary>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 100.</param>
    /// <returns>A <see cref="ToolResult" /> containing the PsInfo system report.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PsInfo_Read_System", ReadOnly = true, Destructive = false)]
    [Description("Reports OS version, kernel build, install date, and hotfixes using Sysinternals PsInfo. Requires PsInfo to be installed.")]
    public async Task<ToolResult> PsInfoReadSystemAsync([Description("Maximum number of output lines to return. Defaults to 200.")] int maxLines = 200)
    {
        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -h lists hotfixes; -s lists installed software; -accepteula suppresses the EULA prompt.
        ToolResult result = await SysinternalsHelper.RunAsync("psinfo", "-h -accepteula", "PsInfo system read").ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "PsInfo system read complete.");
    }
}