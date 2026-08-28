// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         GroupPolicyExtendedReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Management;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Group Policy Resultant Set of Policy (RSOP)
///     for effective policy analysis via the RSOP WMI namespace.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class GroupPolicyExtendedReadTool
{

    /// <summary>
    ///     Reads the Resultant Set of Policy (RSOP) for the computer via the RSOP WMI namespace.
    /// </summary>
    /// <param name="maxRecords">Maximum number of policy entries to return. Defaults to 50.</param>
    /// <returns>A <see cref="ToolResult" /> containing JSON-formatted RSOP entries.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Group_Policy_Read_RSOP", ReadOnly = true, Destructive = false)]
    [Description("Reads the Resultant Set of Policy (RSOP) applied to the computer via the WMI RSOP namespace.")]
    public async Task<ToolResult> GroupPolicyReadRsopAsync([Description("Maximum number of policy entries to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxRecords);
            if (maxValidation is not null)
            {
                return maxValidation;
            }

            List<object> results = new();
            using ManagementObjectSearcher searcher = new(@"root\rsop\computer", "SELECT namespace, GPOID, SOMID, idName, idVersion FROM RSOP_GPLink");
            foreach (ManagementObject link in searcher.Get())
            {
                if (results.Count >= maxRecords)
                {
                    break;
                }

                results.Add(new
                {
                        Namespace = link["namespace"]?.ToString() ?? string.Empty,
                        GpoId = link["GPOID"]?.ToString() ?? string.Empty,
                        SomId = link["SOMID"]?.ToString() ?? string.Empty,
                        LinkName = link["idName"]?.ToString() ?? string.Empty,
                        LinkVersion = link["idVersion"]?.ToString() ?? string.Empty
                });
            }

            return ToolResult.Ok(results, "GroupPolicyExtendedReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "RSOP read");
        }
    }
}