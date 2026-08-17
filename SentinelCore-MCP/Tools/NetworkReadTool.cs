// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         NetworkReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tools for querying network interfaces, TCP/IP configuration, and DNS.
/// </summary>
[McpServerToolType]
public sealed class NetworkReadTool
{








    [McpServerTool(Name = "Network_List_Interfaces", ReadOnly = true, Destructive = false)]
    [Description("Lists network interfaces and their operational status.")]
    public static ToolResult NetworkListInterfaces([Description("Maximum number of interfaces to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces().Take(maxRecords);
            var results = interfaces.Select(ni => new
            {
                ni.Name,
                ni.Description,
                ni.OperationalStatus,
                ni.Speed,
                ni.NetworkInterfaceType,
                ni.GetIPProperties().UnicastAddresses.Count
            });

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Network interface listing failed.");
        }
    }








    [McpServerTool(Name = "Network_List_Tcp_Connections", ReadOnly = true, Destructive = false)]
    [Description("Lists active TCP connections and their local/remote endpoints.")]
    public static ToolResult NetworkListTcpConnections([Description("Maximum number of connections to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            var results = connections.Take(maxRecords).Select(c => new { LocalEndpoint = c.LocalEndPoint.ToString(), RemoteEndpoint = c.RemoteEndPoint.ToString(), c.State });

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("TCP connection listing failed.");
        }
    }








    [McpServerTool(Name = "Network_Read_IP_Config", ReadOnly = true, Destructive = false)]
    [Description("Reads IP configuration for a specific network interface.")]
    public static ToolResult NetworkReadIpConfig([Description("The network interface name.")] string interfaceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return ToolResult.Fail("interfaceName is required.");
            }

            NetworkInterface? ni = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(x => x.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase));

            if (ni is null)
            {
                return ToolResult.Fail($"Network interface not found: {interfaceName}");
            }

            IPInterfaceProperties props = ni.GetIPProperties();
            StringBuilder sb = new();
            sb.AppendLine($"Name={ni.Name}");
            sb.AppendLine($"Description={ni.Description}");
            sb.AppendLine($"OperationalStatus={ni.OperationalStatus}");
            sb.AppendLine($"DnsSuffix={props.DnsSuffix}");
            sb.AppendLine("IPv4Addresses:");
            foreach (UnicastIPAddressInformation addr in props.UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork))
                sb.AppendLine($"  {addr.Address}/{addr.PrefixLength}");

            sb.AppendLine("IPv6Addresses:");
            foreach (UnicastIPAddressInformation addr in props.UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetworkV6))
                sb.AppendLine($"  {addr.Address}/{addr.PrefixLength}");

            sb.AppendLine("DnsServers:");
            foreach (IPAddress dns in props.DnsAddresses) sb.AppendLine($"  {dns}");

            sb.AppendLine("Gateways:");
            foreach (GatewayIPAddressInformation gw in props.GatewayAddresses) sb.AppendLine($"  {gw.Address}");

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("IP config read failed.");
        }
    }








    [McpServerTool(Name = "Network_Resolve_DNS", ReadOnly = true, Destructive = false)]
    [Description("Resolves a hostname to IP addresses using DNS.")]
    public static ToolResult NetworkResolveDns([Description("The hostname or domain to resolve.")] string hostName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hostName))
            {
                return ToolResult.Fail("hostName is required.");
            }

            IPHostEntry entries = Dns.GetHostEntry(hostName);
            StringBuilder sb = new();
            sb.AppendLine($"HostName={entries.HostName}");
            sb.AppendLine("Addresses:");
            foreach (IPAddress address in entries.AddressList) sb.AppendLine($"  {address}");

            sb.AppendLine("Aliases:");
            foreach (string alias in entries.Aliases) sb.AppendLine($"  {alias}");

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("DNS resolution failed.");
        }
    }
}
