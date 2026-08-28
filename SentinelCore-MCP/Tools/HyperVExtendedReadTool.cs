// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         HyperVExtendedReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using Microsoft.Management.Infrastructure;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Hyper-V virtual machine checkpoints (snapshots)
///     for VM rollback detection in forensic investigations.
/// </summary>
[McpServerToolType]
public sealed class HyperVExtendedReadTool
{

    private const string HyperVNamespace = @"root\virtualization\v2";








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "HyperV_List_Checkpoints", ReadOnly = true, Destructive = false)]
    [Description("Lists Hyper-V virtual machine checkpoints (snapshots) for VM rollback detection.")]
    public async Task<ToolResult> HyperVListCheckpointsAsync([Description("Maximum number of checkpoints to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();
            using CimSession session = CimSession.Create(null);

            string query = "SELECT ElementName, Description, CreationTime, VirtualSystemName FROM Msvm_VirtualSystemSettingData WHERE SettingType = 5";
            foreach (CimInstance checkpoint in session.QueryInstances(HyperVNamespace, "WQL", query))
            {
                if (results.Count >= maxRecords) break;

                string? elementName = checkpoint.CimInstanceProperties["ElementName"]?.Value?.ToString();
                string? description = checkpoint.CimInstanceProperties["Description"]?.Value?.ToString();
                string? creationTime = checkpoint.CimInstanceProperties["CreationTime"]?.Value?.ToString();
                string? vmName = checkpoint.CimInstanceProperties["VirtualSystemName"]?.Value?.ToString();

                results.Add(new { CheckpointName = elementName ?? "", Description = description ?? "", CreationTime = creationTime ?? "", VMName = vmName ?? "" });
            }

            return ToolResult.Ok(results, "HyperVExtendedReadTool");
        }
        catch
        {
            return ToolResult.Fail("Hyper-V checkpoint listing failed.", "HyperVExtendedReadTool");
        }
    }
}