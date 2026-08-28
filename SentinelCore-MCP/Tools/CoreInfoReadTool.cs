// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         CoreInfoReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;




/// <summary>
///     Read-only tool for enumerating low-level system information (CPU topology,
///     cache geometry, NUMA nodes, virtualization) using the Sysinternals Coreinfo
///     utility.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class CoreInfoReadTool
{




    /// <summary>
    ///     Probes whether Sysinternals Coreinfo is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_CoreInfo_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals Coreinfo utility is installed and reports its version and path.")]
    public async Task<ToolResult> CoreInfoAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("coreinfo")).ConfigureAwait(false);
    }






    /// <summary>
    ///     Enumerates processor topology, cache geometry, NUMA nodes, and
    ///     virtualization support using Coreinfo.
    /// </summary>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 200.</param>
    /// <returns>A <see cref="ToolResult" /> containing the Coreinfo system report.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_CoreInfo_Read_System", ReadOnly = true, Destructive = false)]
    [Description("Enumerates CPU topology, cache geometry, NUMA nodes, and virtualization support using Sysinternals Coreinfo. Requires Coreinfo to be installed.")]
    public async Task<ToolResult> CoreInfoReadSystemAsync(
        [Description("Maximum number of output lines to return. Defaults to 200.")] int maxLines = 200)
    {
        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -c processor cores, -v virtualization; -accepteula suppresses the EULA prompt.
        ToolResult result = await SysinternalsHelper.RunAsync("coreinfo", "-c -v -accepteula", "Coreinfo system read").ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "Coreinfo system read complete.");
    }
}
