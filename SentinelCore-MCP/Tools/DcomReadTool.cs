// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         DcomReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Management;
using System.Runtime.Versioning;
using System.Text;

using Microsoft.Win32;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying DCOM application configuration through WMI.
/// </summary>
[McpServerToolType]
public sealed class DcomReadTool
{

    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "DCOM_List_Applications", ReadOnly = true, Destructive = false)]
    [Description("Lists DCOM applications registered on the system using WMI Win32_DCOMApplication.")]
    public async Task<ToolResult> DcomListApplicationsAsync()
    {
        try
        {
            StringBuilder sb = new();
            using ManagementObjectSearcher searcher = new("SELECT AppID, Name FROM Win32_DCOMApplication");
            foreach (ManagementObject app in searcher.Get())
            {
                string appId = app["AppID"]?.ToString() ?? string.Empty;
                string name = app["Name"]?.ToString() ?? string.Empty;
                sb.AppendLine($"AppID={appId}, Name={name}");
            }

            return ToolResult.Ok(sb.ToString(), "DcomReadTool");
        }
        catch
        {
            return ToolResult.Fail("DCOM application query failed.", "DcomReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "DCOM_Read_AppId_Settings", ReadOnly = true, Destructive = false)]
    [Description("Reads DCOM AppID settings from the registry under HKLM\\SOFTWARE\\Classes\\AppID.")]
    public async Task<ToolResult> DcomReadAppidSettingsAsync([Description("The AppID GUID to inspect, including braces.")] string appId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return ToolResult.Fail("appId is required.", "DcomReadTool");
            }

            string keyPath = $"SOFTWARE\\Classes\\AppID\\{appId}";
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath, false);
            if (key is null)
            {
                return ToolResult.Fail($"AppID registry key not found: {keyPath}", "DcomReadTool");
            }

            StringBuilder sb = new();
            sb.AppendLine($"AppID={appId}");
            foreach (string valueName in key.GetValueNames())
            {
                string displayName = string.IsNullOrEmpty(valueName) ? "(Default)" : valueName;
                sb.AppendLine($"{displayName}={key.GetValue(valueName)}");
            }

            return ToolResult.Ok(sb.ToString(), "DcomReadTool");
        }
        catch
        {
            return ToolResult.Fail("DCOM AppID read failed.", "DcomReadTool");
        }
    }
}