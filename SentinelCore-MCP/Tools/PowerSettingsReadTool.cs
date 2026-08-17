// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         PowerSettingsReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801




using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Management;
using System.Text;
using System.Runtime.Versioning;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Windows power plans and settings.
/// </summary>
[McpServerToolType]
public sealed class PowerSettingsReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Power_List_Plans", ReadOnly = true, Destructive = false)]
    [Description("Lists active and available power plans using WMI.")]
    public ToolResult powerListPlans()
    {
        try
        {
            StringBuilder sb = new();
            using ManagementObjectSearcher searcher = new("root\\cimv2\\power", "SELECT InstanceId, ElementName, IsActive FROM Win32_PowerPlan");
            foreach (ManagementObject plan in searcher.Get())
            {
                string instanceId = plan["InstanceId"]?.ToString() ?? string.Empty;
                string name = plan["ElementName"]?.ToString() ?? string.Empty;
                string isActive = plan["IsActive"]?.ToString() ?? string.Empty;
                sb.AppendLine($"InstanceId={instanceId}, Name={name}, IsActive={isActive}");
            }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Power plan listing failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Power_List_Settings", ReadOnly = true, Destructive = false)]
    [Description("Lists power settings for the active power plan.")]
    public ToolResult powerListSettings()
    {
        try
        {
            StringBuilder sb = new();
            using ManagementObjectSearcher searcher = new("root\\cimv2\\power", "SELECT InstanceId, ElementName, Value FROM Win32_PowerSettingDataIndex");
            foreach (ManagementObject setting in searcher.Get())
            {
                string instanceId = setting["InstanceId"]?.ToString() ?? string.Empty;
                string name = setting["ElementName"]?.ToString() ?? string.Empty;
                string value = setting["Value"]?.ToString() ?? string.Empty;
                sb.AppendLine($"InstanceId={instanceId}, Name={name}, Value={value}");
            }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Power setting listing failed.");
        }
    }
}
