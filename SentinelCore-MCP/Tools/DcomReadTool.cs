// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         DcomReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Management;
using System.Runtime.Versioning;
using System.Text;




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
    public static ToolResult DcomListApplications()
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

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("DCOM application query failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "DCOM_Read_AppId_Settings", ReadOnly = true, Destructive = false)]
    [Description("Reads DCOM AppID settings from the registry under HKLM\\SOFTWARE\\Classes\\AppID.")]
    public static ToolResult DcomReadAppidSettings([Description("The AppID GUID to inspect, including braces.")] string appId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return ToolResult.Fail("appId is required.");
            }

            string keyPath = $"SOFTWARE\\Classes\\AppID\\{appId}";
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath, false);
            if (key is null)
            {
                return ToolResult.Fail($"AppID registry key not found: {keyPath}");
            }

            StringBuilder sb = new();
            sb.AppendLine($"AppID={appId}");
            foreach (string valueName in key.GetValueNames())
            {
                string displayName = string.IsNullOrEmpty(valueName) ? "(Default)" : valueName;
                sb.AppendLine($"{displayName}={key.GetValue(valueName)}");
            }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("DCOM AppID read failed.");
        }
    }
}
