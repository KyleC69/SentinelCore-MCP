// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         AppLockerReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Management.Automation;
using System.Runtime.Versioning;
using System.Text;

using JetBrains.Annotations;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying AppLocker policy.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class AppLockerReadTool
{

    [McpServerTool(Name = "AppLocker_Get_Effective_Policy", ReadOnly = true, Destructive = false)]
    [Description("Retrieves the effective AppLocker policy as XML.")]
    [UsedImplicitly]
    public async Task<ToolResult> ApplockerGetEffectivePolicyAsync()
    {
        try
        {
            StringBuilder sb = new();
            using PowerShell? powerShell = PowerShell.Create();
            powerShell.AddScript("Get-AppLockerPolicy -Effective | ConvertTo-Xml -NoTypeInformation");
            Collection<PSObject>? results = powerShell.Invoke();
            if (powerShell.HadErrors)
            {
                string errors = string.Join("; ", powerShell.Streams.Error.Select(e => e.ToString()));
                return ToolResult.Fail($"PowerShell AppLocker query failed: {errors}", "AppLockerReadTool");
            }

            foreach (PSObject result in results) sb.AppendLine(result.ToString());

            return ToolResult.Ok(sb.ToString(), "AppLockerReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "AppLockerReadTool");
        }
    }








    [McpServerTool(Name = "AppLocker_List_Rule_Collections", ReadOnly = true, Destructive = false)]
    [Description("Retrieves AppLocker rule collections from the effective policy.")]
    [UsedImplicitly]
    public async Task<ToolResult> ApplockerListRuleCollectionsAsync()
    {
        try
        {
            StringBuilder sb = new();
            using PowerShell? powerShell = PowerShell.Create();
            powerShell.AddScript("$policy = Get-AppLockerPolicy -Effective; $policy.RuleCollections | Select-Object Name, RuleCollectionType, EnforcementMode | Format-List | Out-String");
            Collection<PSObject>? results = powerShell.Invoke();
            if (powerShell.HadErrors)
            {
                string errors = string.Join("; ", powerShell.Streams.Error.Select(e => e.ToString()));
                return ToolResult.Fail($"PowerShell AppLocker rule listing failed: {errors}", "AppLockerReadTool");
            }

            foreach (PSObject result in results) sb.AppendLine(result.ToString());

            return ToolResult.Ok(sb.ToString(), "AppLockerReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "AppLockerReadTool");
        }
    }
}
