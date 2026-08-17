// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         NetworkExtendedReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tools for querying extended network state: listening ports, DNS cache, ARP table,
///     routing table, and network shares.
/// </summary>
[McpServerToolType]
public sealed class NetworkExtendedReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Network_List_Listening_Ports", ReadOnly = true, Destructive = false)]
    [Description("Lists TCP and UDP listening ports with process association where available.")]
    public static ToolResult NetworkListListeningPorts([Description("Maximum number of ports to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            // TCP listeners
            foreach (IPEndPoint endpoint in properties.GetActiveTcpListeners())
            {
                if (results.Count >= maxRecords) break;
                results.Add(new
                {
                    Protocol = "TCP",
                    endpoint.Address,
                    Port = endpoint.Port,
                    State = "Listen"
                });
            }

            // UDP listeners
            foreach (IPEndPoint endpoint in properties.GetActiveUdpListeners())
            {
                if (results.Count >= maxRecords) break;
                results.Add(new
                {
                    Protocol = "UDP",
                    endpoint.Address,
                    Port = endpoint.Port,
                    State = "Listen"
                });
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Listening port listing failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Network_Read_DNS_Cache", ReadOnly = true, Destructive = false)]
    [Description("Reads the local DNS resolver cache entries.")]
    public static ToolResult NetworkReadDnsCache([Description("Maximum number of entries to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            foreach (IPAddressCollection? entry in new[] { properties.GetActiveTcpListeners().Select(e => e.Address).Distinct().Select(a => new { Address = a }) })
            {
                // Fallback: managed API does not expose DNS cache directly; use ipconfig /displaydns
            }

            // The managed IPGlobalProperties API does not expose the DNS cache.
            // Use the ipconfig /displaydns command to retrieve it.
            StringBuilder sb = new();
            System.Diagnostics.ProcessStartInfo psi = new()
            {
                FileName = "ipconfig",
                Arguments = "/displaydns",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return ToolResult.Fail("Unable to start ipconfig.");
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse the output to extract DNS cache entries
            List<object> entries = new();
            string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string? currentName = null;
            string? currentType = null;
            string? currentData = null;
            int count = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("Record Name", StringComparison.OrdinalIgnoreCase))
                {
                    if (currentName is not null && count < maxRecords)
                    {
                        entries.Add(new { RecordName = currentName, RecordType = currentType ?? "", Data = currentData ?? "" });
                        count++;
                    }

                    currentName = line.Substring(line.IndexOf('.') + 1).Trim();
                    currentType = null;
                    currentData = null;
                }
                else if (line.StartsWith("Record Type", StringComparison.OrdinalIgnoreCase))
                {
                    currentType = line.Substring(line.IndexOf('.') + 1).Trim();
                }
                else if (line.StartsWith("A (Host) Record", StringComparison.OrdinalIgnoreCase) ||
                         line.StartsWith("Data", StringComparison.OrdinalIgnoreCase))
                {
                    currentData = line.Substring(line.IndexOf('.') + 1).Trim();
                }
            }

            // Add last entry
            if (currentName is not null && count < maxRecords)
            {
                entries.Add(new { RecordName = currentName, RecordType = currentType ?? "", Data = currentData ?? "" });
            }

            string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("DNS cache read failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Network_Read_ARP_Table", ReadOnly = true, Destructive = false)]
    [Description("Reads the ARP cache table mapping IP addresses to physical addresses.")]
    public static ToolResult NetworkReadArpTable([Description("Maximum number of entries to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();
            System.Diagnostics.ProcessStartInfo psi = new()
            {
                FileName = "arp",
                Arguments = "-a",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return ToolResult.Fail("Unable to start arp.");
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse arp -a output
            string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (results.Count >= maxRecords) break;
                string trimmed = line.Trim();
                // Lines with IP/MAC look like: 192.168.1.1   00-11-22-33-44-55   dynamic
                if (trimmed.Length > 0 && char.IsDigit(trimmed[0]))
                {
                    string[] parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        results.Add(new
                        {
                            IPAddress = parts[0],
                            PhysicalAddress = parts[1],
                            Type = parts.Length >= 3 ? parts[2] : ""
                        });
                    }
                }
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("ARP table read failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Network_Read_Routing_Table", ReadOnly = true, Destructive = false)]
    [Description("Reads the IPv4 routing table.")]
    public static ToolResult NetworkReadRoutingTable([Description("Maximum number of routes to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();
            System.Diagnostics.ProcessStartInfo psi = new()
            {
                FileName = "route",
                Arguments = "print -4",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return ToolResult.Fail("Unable to start route.");
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse route print output for IPv4 active routes
            string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            bool inRoutes = false;
            foreach (string line in lines)
            {
                if (results.Count >= maxRecords) break;
                string trimmed = line.Trim();
                if (trimmed.StartsWith("Network Destination", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("====", StringComparison.OrdinalIgnoreCase))
                {
                    inRoutes = true;
                    continue;
                }

                if (inRoutes && trimmed.Length > 0 && char.IsDigit(trimmed[0]))
                {
                    string[] parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        results.Add(new
                        {
                            NetworkDestination = parts[0],
                            Netmask = parts[1],
                            Gateway = parts[2],
                            Interface = parts[3],
                            Metric = parts.Length >= 5 ? parts[4] : ""
                        });
                    }
                }
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Routing table read failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Network_List_Shares", ReadOnly = true, Destructive = false)]
    [Description("Lists network shares on the local machine.")]
    public static ToolResult NetworkListShares([Description("Maximum number of shares to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();
            System.Diagnostics.ProcessStartInfo psi = new()
            {
                FileName = "net",
                Arguments = "share",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return ToolResult.Fail("Unable to start net share.");
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (results.Count >= maxRecords) break;
                string trimmed = line.Trim();
                // Skip header and separator lines
                if (string.IsNullOrWhiteSpace(trimmed) ||
                    trimmed.StartsWith("-") ||
                    trimmed.StartsWith("Share", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string[] parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 1)
                {
                    results.Add(new
                    {
                        ShareName = parts[0],
                        Resource = parts.Length >= 2 ? parts[1] : "",
                        Remark = parts.Length >= 3 ? string.Join(" ", parts[2..]) : ""
                    });
                }
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Network share listing failed.");
        }
    }
}