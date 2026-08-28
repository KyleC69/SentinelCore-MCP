// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         PipeListReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for enumerating named pipes on the local system using the
///     Sysinternals PipeList utility. Unusual named pipes are a common
///     local-IPC backdoor mechanism, making this a high-value forensic surface.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class PipeListReadTool
{

    /// <summary>
    ///     Probes whether Sysinternals PipeList is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PipeList_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals PipeList utility is installed and reports its version and path.")]
    public async Task<ToolResult> PipeListAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("pipelist")).ConfigureAwait(false);
    }








    /// <summary>
    ///     Lists named pipes with their instances and maximum instance counts using
    ///     PipeList.
    /// </summary>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 200.</param>
    /// <returns>A <see cref="ToolResult" /> containing the named pipe listing.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PipeList_List_Pipes", ReadOnly = true, Destructive = false)]
    [Description("Lists named pipes with instance counts using Sysinternals PipeList. Requires PipeList to be installed.")]
    public async Task<ToolResult> PipeListListPipesAsync([Description("Maximum number of output lines to return. Defaults to 200.")] int maxLines = 200)
    {
        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -accepteula suppresses the EULA prompt.
        ToolResult result = await SysinternalsHelper.RunAsync("pipelist", "-accepteula", "Named pipe enumeration").ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "Named pipe enumeration complete.");
    }
}