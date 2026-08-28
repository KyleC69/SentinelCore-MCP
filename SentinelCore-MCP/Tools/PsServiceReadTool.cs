// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         PsServiceReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for enumerating Windows services and their configuration
///     using the Sysinternals PsService utility. Complements the managed
///     Service_List and Service_Read tools with Sysinternals-native output.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class PsServiceReadTool
{

    /// <summary>
    ///     Probes whether Sysinternals PsService is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PsService_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals PsService utility is installed and reports its version and path.")]
    public async Task<ToolResult> PsServiceAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("psservice")).ConfigureAwait(false);
    }








    /// <summary>
    ///     Lists installed services with their configuration and status, or the
    ///     detailed configuration of a single service.
    /// </summary>
    /// <param name="serviceName">Optional service name to inspect in detail. When omitted, all services are listed.</param>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 200.</param>
    /// <returns>A <see cref="ToolResult" /> containing the service listing or detail.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_PsService_List_Services", ReadOnly = true, Destructive = false)]
    [Description("Lists services and their configuration using Sysinternals PsService. Requires PsService to be installed.")]
    public async Task<ToolResult> PsServiceListServicesAsync([Description("Optional service name to inspect in detail. When omitted, all services are listed.")] string? serviceName = null, [Description("Maximum number of output lines to return. Defaults to 200.")] int maxLines = 200)
    {
        if (serviceName is not null)
        {
            ToolResult? nameValidation = SysinternalsHelper.ValidateArgument(serviceName, "serviceName");
            if (nameValidation is not null)
            {
                return nameValidation;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(serviceName!, @"^[a-zA-Z0-9._\- ]+$"))
            {
                return ToolResult.Fail("serviceName must contain only letters, digits, dots, hyphens, underscores, and spaces.", "PsService enumeration");
            }
        }

        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -c renders the output as CSV; -accepteula suppresses the EULA prompt.
        string arguments = string.IsNullOrWhiteSpace(serviceName) ? "-c -accepteula" : $"-c -accepteula \"{serviceName}\"";

        ToolResult result = await SysinternalsHelper.RunAsync("psservice", arguments, "PsService enumeration").ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "PsService enumeration complete.");
    }
}