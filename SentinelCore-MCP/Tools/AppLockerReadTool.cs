// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         AppLockerReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using JetBrains.Annotations;

using ModelContextProtocol.Server;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Management.Automation;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying AppLocker policy.
/// </summary>
[McpServerToolType]
public sealed class AppLockerReadTool
{








    [McpServerTool(Name = "AppLocker_Get_Effective_Policy", ReadOnly = true, Destructive = false)]
    [Description("Retrieves the effective AppLocker policy as XML.")]
    [UsedImplicitly]
    public static ToolResult ApplockerGetEffectivePolicy()
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
                return ToolResult.Fail($"PowerShell AppLocker query failed: {errors}");
            }

            foreach (PSObject result in results) sb.AppendLine(result.ToString());

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("AppLocker policy query failed.");
        }
    }








    [McpServerTool(Name = "AppLocker_List_Rule_Collections", ReadOnly = true, Destructive = false)]
    [Description("Retrieves AppLocker rule collections from the effective policy.")]
    [UsedImplicitly]
    public static ToolResult ApplockerListRuleCollections()
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
                return ToolResult.Fail($"PowerShell AppLocker rule listing failed: {errors}");
            }

            foreach (PSObject result in results) sb.AppendLine(result.ToString());

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("AppLocker rule collection listing failed.");
        }
    }
}
