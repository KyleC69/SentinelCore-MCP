// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         NetworkDnsReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;

using Microsoft.Win32;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying DNS server settings from the registry
///     to detect DNS hijacking or misconfiguration.
/// </summary>
[McpServerToolType]
public sealed class NetworkDnsReadTool
{

    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Network_Read_DNS_Settings", ReadOnly = true, Destructive = false)]
    [Description("Reads DNS server configuration settings from the registry for DNS hijacking detection.")]
    public async Task<ToolResult> NetworkReadDnsSettingsAsync()
    {
        try
        {
            StringBuilder sb = new();

            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                // DNS Client settings
                using RegistryKey? dnsKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters", false);
                if (dnsKey is not null)
                {
                    sb.AppendLine("[DNS Client Parameters]");
                    foreach (string valueName in dnsKey.GetValueNames())
                    {
                        sb.AppendLine($"  {valueName}={dnsKey.GetValue(valueName)}");
                    }
                }

                // DNS Server settings (if this machine is a DNS server)
                using RegistryKey? dnsServerKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\DNS Server", false);
                if (dnsServerKey is not null)
                {
                    sb.AppendLine("[DNS Server]");
                    foreach (string valueName in dnsServerKey.GetValueNames())
                    {
                        sb.AppendLine($"  {valueName}={dnsServerKey.GetValue(valueName)}");
                    }
                }

                // TCP/IP DNS settings per interface
                using RegistryKey? tcpIpKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", false);
                if (tcpIpKey is not null)
                {
                    sb.AppendLine("[TCP/IP DNS Parameters]");
                    foreach (string valueName in tcpIpKey.GetValueNames())
                    {
                        sb.AppendLine($"  {valueName}={tcpIpKey.GetValue(valueName)}");
                    }
                }

                // DNS over HTTPS
                using RegistryKey? dohKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters\DohWellKnownServers", false);
                if (dohKey is not null)
                {
                    sb.AppendLine("[DNS over HTTPS Well-Known Servers]");
                    foreach (string valueName in dohKey.GetValueNames())
                    {
                        sb.AppendLine($"  {valueName}={dohKey.GetValue(valueName)}");
                    }
                }
            }

            return ToolResult.Ok(sb.ToString(), "NetworkDnsReadTool");
        }
        catch
        {
            return ToolResult.Fail("DNS settings read failed.", "NetworkDnsReadTool");
        }
    }
}