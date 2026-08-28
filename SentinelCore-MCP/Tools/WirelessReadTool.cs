// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         WirelessReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;
using System.Text;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for enumerating wireless network interfaces and saved profiles.
///     Uses CIM (not legacy WMI) via Microsoft.Management.Infrastructure where possible; falls back to System.Management
///     for WQL queries.
/// </summary>
[McpServerToolType]
public sealed class WirelessReadTool
{

    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Wireless_List_Interfaces", ReadOnly = true, Destructive = false)]
    [Description("Lists wireless network interfaces on the system using CIM/MSNdis classes.")]
    public async Task<ToolResult> wirelessListInterfacesAsync([Description("Maximum number of interfaces to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();
            using ManagementObjectSearcher searcher = new("root\\StandardCimv2", "SELECT InstanceID, Name, InterfaceDescription, State, Active FROM MSFT_NetAdapter WHERE InterfaceDescription LIKE '%Wireless%' OR InterfaceDescription LIKE '%Wi-Fi%'");
            foreach (ManagementObject adapter in searcher.Get())
            {
                if (results.Count >= maxRecords)
                {
                    break;
                }

                results.Add(new
                {
                        InstanceID = adapter["InstanceID"]?.ToString(),
                        Name = adapter["Name"]?.ToString(),
                        InterfaceDescription = adapter["InterfaceDescription"]?.ToString(),
                        State = adapter["State"]?.ToString(),
                        Active = adapter["Active"]?.ToString()
                });
            }

            return ToolResult.Ok(results, "WirelessReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Wireless interface listing failed: {ex.Message}", "WirelessReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Wireless_List_Profiles", ReadOnly = true, Destructive = false)]
    [Description("Lists saved Wi-Fi profiles using netsh as a read-only native command invocation.")]
    public async Task<ToolResult> wirelessListProfilesAsync([Description("Maximum number of profiles to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                    FileName = "netsh",
                    Arguments = "wlan show profiles",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
            };

            using Process? process = Process.Start(startInfo);
            if (process is null)
            {
                return ToolResult.Fail("Failed to start netsh.", "WirelessReadTool");
            }

            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                return ToolResult.Fail($"netsh failed: {stderr}", "WirelessReadTool");
            }

            StringBuilder sb = new();
            int count = 0;
            foreach (string line in stdout.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                if (line.Contains("Profile", StringComparison.OrdinalIgnoreCase))
                {
                    if (count >= maxRecords)
                    {
                        continue;
                    }

                    count++;
                }

                sb.AppendLine(line);
            }

            return ToolResult.Ok(sb.ToString(), "WirelessReadTool");
        }
        catch
        {
            return ToolResult.Fail("Wireless profile listing failed.", "WirelessReadTool");
        }
    }
}