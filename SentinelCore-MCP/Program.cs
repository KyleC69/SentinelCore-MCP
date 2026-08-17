using ModelContextProtocol.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
        builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

        // Add the MCP services: the transport to use (stdio), server identity/instructions,
        // and the tools to register. The server/discover handler is registered automatically
        // by the SDK and will respond with ServerInfo, Capabilities, and Instructions.
        _ = builder.Services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new Implementation
                {
                    Name = "io.github.kylec69/SentinelCoreMCP",
                    Version = "0.1.0-beta",
                    Title = "SentinelCoreMCP",
                    Description = "Windows security and configuration reconnaissance server implementing the Model Context Protocol (MCP).",
                    WebsiteUrl = "https://github.com/kylec69/SentinelCore-MCP",
                };

                options.ServerInstructions =
                    """
                    SentinelCore-MCP provides 70+ read-only tools for inspecting Windows system state.

                    All tools are safe: ReadOnly = true, Destructive = false. They never modify the host.

                    Key tool categories:
                    - Firewall: Firewall_List_Rules, Firewall_Read_Profiles
                    - Defender: Defender_Read_Status, Defender_Read_Registry_Config
                    - Services: Service_List, Service_Read
                    - Processes: Process_List, Process_Read
                    - Registry: Registry_List_Key, Registry_Read_Value
                    - Certificates: Certificate_List, Certificate_Read
                    - Network: Network_List_Interfaces, Network_Read_IP_Config
                    - BitLocker: Bitlocker_List_Volumes, Bitlocker_Read_Volume
                    - Group Policy: Group_Policy_List, Group_Policy_Read_Value
                    - UAC: UAC_Read_Settings, UAC_Read_Token_Elevation

                    Every tool returns a ToolResult with Success, Results, and ErrorDetails fields.
                    """;
            })
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        await builder.Build().RunAsync();
    }
}
