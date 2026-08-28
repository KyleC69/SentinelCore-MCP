// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         ProxyReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;

using Microsoft.Win32;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying system proxy configuration from the registry
///     (user Internet Settings and machine WinHTTP settings).
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class ProxyReadTool
{
    private const string WinHttpSettingsKey = @"SYSTEM\CurrentControlSet\Services\Http\Parameters\ProxySettings";








    /// <summary>
    ///     Reads the per-user system proxy configuration from the Internet Settings registry key.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the user proxy settings.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Proxy_Read_System", ReadOnly = true, Destructive = false)]
    [Description("Reads the system proxy configuration from the Internet Settings registry key.")]
    public async Task<ToolResult> ProxyReadSystemAsync()
    {
        try
        {
            StringBuilder sb = new();
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings", false);
            if (key is null)
            {
                return ToolResult.Fail("Internet Settings registry key not found.", "ProxyReadTool");
            }

            foreach (string valueName in key.GetValueNames())
            {
                if (valueName.Contains("Proxy", StringComparison.OrdinalIgnoreCase) || valueName.Contains("AutoConfig", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"{valueName}={key.GetValue(valueName)}");
                }
            }

            return ToolResult.Ok(sb.ToString(), "ProxyReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "System proxy read");
        }
    }








    /// <summary>
    ///     Reads the machine-wide WinHTTP proxy configuration from the registry
    ///     instead of shelling out to <c>netsh winhttp show proxy</c>.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the WinHTTP proxy settings.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Proxy_Read_WinHTTP", ReadOnly = true, Destructive = false)]
    [Description("Reads the machine-wide WinHTTP proxy configuration from the registry.")]
    public async Task<ToolResult> ProxyReadWinhttpAsync()
    {
        try
        {
            StringBuilder sb = new();
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(WinHttpSettingsKey, false);
            if (key is null)
            {
                return ToolResult.Ok("WinHttpSettings=DirectAccess (no proxy configured)", "ProxyReadTool");
            }

            sb.AppendLine($"[HKLM\\{WinHttpSettingsKey}]");
            foreach (RegistryHelper.RegistryValueRecord v in RegistryHelper.ReadValues(key)) sb.AppendLine($"  {v.Name}={v.Value}");

            return ToolResult.Ok(sb.ToString(), "ProxyReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "WinHTTP proxy read");
        }
    }
}