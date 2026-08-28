// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         Program.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;





internal class Program
{

    private static void ConfigureServerInfo(McpServerOptions options)
    {
        options.ServerInfo = new Implementation
        {
                Name = "io.github.kylec69/SentinelCoreMCP",
                Version = "1.1.1",
                Title = "SentinelCoreMCP",
                Description = "Windows security and configuration reconnaissance server implementing the Model Context Protocol (MCP).",
                WebsiteUrl = "https://github.com/kylec69/SentinelCore-MCP"
        };

        options.ServerInstructions = """
                                     SentinelCore-MCP provides 100+ read-only tools for inspecting Windows system state.

                                     All tools are safe: ReadOnly = true, Destructive = false. They never modify the host.

                                     Key tool categories:
                                     - Firewall: Firewall_List_Rules, Firewall_Read_Profiles
                                     - Defender: Defender_Read_Status, Defender_Read_Registry_Config, Defender_Read_SmartScreen
                                     - Services: Service_List, Service_Read, Service_Read_Acl
                                     - Processes: Process_List, Process_Read
                                     - Registry: Registry_List_Key, Registry_Read_Value, Registry_Read_Acl
                                     - Certificates: Certificate_List, Certificate_Read, Certificate_Verify
                                     - Network: Network_List_Interfaces, Network_Read_IP_Config, Network_List_Listening_Ports, Network_Read_DNS_Cache, Network_Read_ARP_Table, Network_Read_Routing_Table, Network_List_Shares, Network_Read_DNS_Settings
                                     - File System: File_System_Read_Content, File_System_Compute_Hash, File_System_List_Streams, File_System_Read_Hosts
                                     - BitLocker: Bitlocker_List_Volumes, Bitlocker_Read_Volume
                                     - Group Policy: Group_Policy_List, Group_Policy_Read_Value, Group_Policy_Read_RSOP
                                     - UAC: UAC_Read_Settings, UAC_Read_Token_Elevation
                                     - Autoruns: Autoruns_List, Autoruns_List_IFEO
                                     - Security: Security_Read_Credential_Guard, Security_Read_SecureBoot, Security_Read_TPM, Security_Read_Exploit_Protection
                                     - Sessions: Sessions_List_Active
                                     - System: System_Read_Info, System_Read_TimeZone
                                     - Browser: Browser_List_Extensions, Browser_Read_History
                                     - RDP: RDP_Read_Settings, RDP_Read_Listener_Config, RDP_List_Sessions
                                     - USB: Pnp_List_USB_History
                                     - COM: COM_List_Classes
                                     - Event Log: Event_Log_List_Channels, Event_Log_Query, Event_Log_Read_Configuration, Event_Log_Read_Forwarding
                                     - Windows Update: Windows_Update_List_History, Windows_Update_Read_Settings, Windows_Update_List_Missing
                                     - Wireless: Wireless_List_Connection_History
                                     - Environment: Environment_Read_Path

                                     Every tool returns a ToolResult with Success, Results, and ErrorDetails fields.
                                     """;
    }








    private static async Task Main(string[] args)
    {
        // When the MCP_TRANSPORT_TYPE environment variable is set to "http", the server
        // starts with Streamable HTTP transport (ASP.NET Core). Otherwise it defaults to
        // stdio transport for use as a local MCP tool server.
        string? transportType = Environment.GetEnvironmentVariable("MCP_TRANSPORT_TYPE")?.ToLowerInvariant();
        bool useHttp = transportType == "http";

        if (useHttp)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

            _ = builder.Services.AddMcpServer(options => { ConfigureServerInfo(options); }).WithHttpTransport().WithToolsFromAssembly();

            builder.Services.AddCors(options => { options.AddDefaultPolicy(policy => { policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); }); });

            WebApplication app = builder.Build();

            app.MapMcp("/mcp");
            app.UseCors();

            await app.RunAsync();
        }
        else
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            // Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
            builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

            _ = builder.Services.AddMcpServer(options => { ConfigureServerInfo(options); }).WithStdioServerTransport().WithToolsFromAssembly();

            await builder.Build().RunAsync();
        }
    }
}