// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         BrowserConfigReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;

using Microsoft.Win32;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for reading browser configuration stored in the Windows registry.
///     This covers common browser default settings and per-browser policy/proxy entries where available.
/// </summary>
[McpServerToolType]
public sealed class BrowserConfigReadTool
{

    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Browser_Config_Read_Chrome_Policies", ReadOnly = true, Destructive = false)]
    [Description("Reads Google Chrome policy entries from the registry if present.")]
    public async Task<ToolResult> BrowserReadChromePoliciesAsync()
    {
        try
        {
            StringBuilder sb = new();
            ReadRegistryValues(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Google\Chrome", sb, "HKLM Chrome Policies");
            ReadRegistryValues(RegistryHive.CurrentUser, @"SOFTWARE\Policies\Google\Chrome", sb, "HKCU Chrome Policies");
            return ToolResult.Ok(sb.ToString(), "BrowserConfigReadTool");
        }
        catch
        {
            return ToolResult.Fail("Chrome policy read failed.", "BrowserConfigReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Browser_Config_Read_Default", ReadOnly = true, Destructive = false)]
    [Description("Reads the default browser ProgId from the registry.")]
    public async Task<ToolResult> BrowserReadDefaultAsync()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\Shell\Associations\UrlAssociations\http\\UserChoice", false);
            if (key is null)
            {
                return ToolResult.Fail("Default browser UserChoice key not found.", "BrowserConfigReadTool");
            }

            string progId = key.GetValue("ProgId")?.ToString() ?? string.Empty;
            return ToolResult.Ok($"DefaultBrowserProgId={progId}", "BrowserConfigReadTool");
        }
        catch
        {
            return ToolResult.Fail("Default browser read failed.", "BrowserConfigReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Browser_Config_Read_IE_Settings", ReadOnly = true, Destructive = false)]
    [Description("Reads Internet Explorer / Edge proxy and security zone settings from the registry.")]
    public async Task<ToolResult> BrowserReadIeSettingsAsync()
    {
        try
        {
            StringBuilder sb = new();
            using RegistryKey? proxyKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings", false);
            if (proxyKey is not null)
            {
                sb.AppendLine($"ProxyEnable={proxyKey.GetValue("ProxyEnable")}");
                sb.AppendLine($"ProxyServer={proxyKey.GetValue("ProxyServer")}");
                sb.AppendLine($"ProxyOverride={proxyKey.GetValue("ProxyOverride")}");
                sb.AppendLine($"AutoConfigURL={proxyKey.GetValue("AutoConfigURL")}");
            }

            using RegistryKey? zonesKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\\Zones", false);
            if (zonesKey is not null)
            {
                sb.AppendLine("Zones:");
                foreach (string zone in zonesKey.GetSubKeyNames())
                {
                    using RegistryKey? zoneSub = zonesKey.OpenSubKey(zone, false);
                    if (zoneSub is null)
                    {
                        continue;
                    }

                    sb.AppendLine($"  Zone={zone}, CurrentLevel={zoneSub.GetValue("CurrentLevel")}");
                }
            }

            return ToolResult.Ok(sb.ToString(), "BrowserConfigReadTool");
        }
        catch
        {
            return ToolResult.Fail("IE/Edge browser settings read failed.", "BrowserConfigReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    private static void ReadRegistryValues(RegistryHive hive, string path, StringBuilder sb, string label)
    {
        using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using RegistryKey? key = baseKey.OpenSubKey(path, false);
        if (key is null)
        {
            return;
        }

        sb.AppendLine($"{label}:");
        foreach (string valueName in key.GetValueNames()) sb.AppendLine($"  {valueName}={key.GetValue(valueName)}");
    }
}