// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         RemoteDesktopReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Runtime.Versioning;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Remote Desktop settings.
/// </summary>
[McpServerToolType]
public sealed class RemoteDesktopReadTool
{

    private const string RdpTcpKey = "SYSTEM\\CurrentControlSet\\Control\\Terminal Server\\WinStations\\RDP-Tcp";
    private const string TerminalServerKey = "SYSTEM\\CurrentControlSet\\Control\\Terminal Server";








    [SupportedOSPlatform("windows")]
    private static void ReadKeyValues(RegistryKey root, string keyPath, StringBuilder sb, string[] valueNames)
    {
        using RegistryKey? key = root.OpenSubKey(keyPath, false);
        if (key is null)
        {
            return;
        }

        sb.AppendLine($"[{keyPath}]");
        foreach (string valueName in valueNames)
        {
            object? value = key.GetValue(valueName);
            if (value is not null)
            {
                sb.AppendLine($"  {valueName}={value}");
            }
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "RDP_Read_Listener_Config", ReadOnly = true, Destructive = false)]
    [Description("Reads RDP listener port and security layer settings.")]
    public async Task<ToolResult> rdpReadListenerConfigAsync()
    {
        try
        {
            StringBuilder sb = new();
            ReadKeyValues(Registry.LocalMachine, RdpTcpKey, sb, ["PortNumber", "SecurityLayer", "MinEncryptionLevel", "UserAuthentication", "SSLCertificateSHA1Hash"]);

            return ToolResult.Ok(sb.ToString(), "RemoteDesktopReadTool");
        }
        catch
        {
            return ToolResult.Fail("RDP listener config read failed.", "RemoteDesktopReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "RDP_Read_Settings", ReadOnly = true, Destructive = false)]
    [Description("Reads Remote Desktop configuration from the registry.")]
    public async Task<ToolResult> rdpReadSettingsAsync()
    {
        try
        {
            StringBuilder sb = new();
            ReadKeyValues(Registry.LocalMachine, TerminalServerKey, sb, ["fDenyTSConnections", "fSingleSessionPerUser", "UserAuthentication"]);
            ReadKeyValues(Registry.LocalMachine, RdpTcpKey, sb, ["PortNumber", "MinEncryptionLevel", "SecurityLayer", "UserAuthentication"]);

            return ToolResult.Ok(sb.ToString(), "RemoteDesktopReadTool");
        }
        catch
        {
            return ToolResult.Fail("RDP settings read failed.", "RemoteDesktopReadTool");
        }
    }
}
