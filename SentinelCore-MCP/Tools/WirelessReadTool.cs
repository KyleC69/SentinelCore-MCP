// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         WirelessReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801




using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;
using System.Text.Json;




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
    public ToolResult wirelessListInterfaces()
    {
        try
        {
            List<object> results = new();
            using ManagementObjectSearcher searcher = new("root\\StandardCimv2", "SELECT InstanceID, Name, InterfaceDescription, State, Active FROM MSFT_NetAdapter WHERE InterfaceDescription LIKE '%Wireless%' OR InterfaceDescription LIKE '%Wi-Fi%'");
            foreach (ManagementObject adapter in searcher.Get())
                results.Add(new
                {
                    InstanceID = adapter["InstanceID"]?.ToString(),
                    Name = adapter["Name"]?.ToString(),
                    InterfaceDescription = adapter["InterfaceDescription"]?.ToString(),
                    State = adapter["State"]?.ToString(),
                    Active = adapter["Active"]?.ToString()
                });

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Wireless interface listing failed: {ex.Message}");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Wireless_List_Profiles", ReadOnly = true, Destructive = false)]
    [Description("Lists saved Wi-Fi profiles using netsh as a read-only native command invocation.")]
    public ToolResult wirelessListProfiles()
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
                return ToolResult.Fail("Failed to start netsh.");
            }

            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode != 0 ? ToolResult.Fail($"netsh failed: {stderr}") : ToolResult.Ok(stdout);

        }
        catch
        {
            return ToolResult.Fail("Wireless profile listing failed.");
        }
    }
}
