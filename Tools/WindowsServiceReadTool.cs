// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         WindowsServiceReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  102702



using System.ComponentModel;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Text;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Windows services.
/// </summary>
[SupportedOSPlatform("windows")]
[McpServerToolType]
public sealed class WindowsServiceReadTool
{

    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Service_List", ReadOnly = true, Destructive = false)]
    [Description("Lists installed Windows services and their current status.")]
    public async Task<ToolResult> ServiceListAsync([Description("Optional service name filter (partial match).")] string? nameFilter = null, [Description("Maximum number of services to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            var services = ServiceController.GetServices();
            List<object> results = new();
            int count = 0;
            foreach (ServiceController service in services)
            {
                if (!string.IsNullOrWhiteSpace(nameFilter) && service.ServiceName.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (count >= maxRecords)
                {
                    break;
                }

                results.Add(new
                {
                    Name = service.ServiceName,
                    DisplayName = Helpers.CleanValue(service.DisplayName),
                    Status = service.Status.ToString(),
                    StartType = service.StartType.ToString()
                });
                count++;
            }

            return ToolResult.Ok(results, "WindowsServiceReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Service listing failed: {ex.Message}", "WindowsServiceReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Service_Read", ReadOnly = true, Destructive = false)]
    [Description("Reads detailed information about a specific Windows service.")]
    public async Task<ToolResult> ServiceReadAsync([Description("The service name (not display name) to inspect.")] string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return ToolResult.Fail("serviceName is required", "WindowsServiceReadTool");
            }

            using ServiceController service = new(serviceName);
            service.Refresh();

            var result = new
            {
                Name = service.ServiceName,
                DisplayName = Helpers.CleanValue(service.DisplayName),
                Status = service.Status.ToString(),
                StartType = service.StartType.ToString(),
                CanPauseAndContinue = service.CanPauseAndContinue,
                CanShutdown = service.CanShutdown,
                CanStop = service.CanStop,
                DependentServices = service.DependentServices.Select(s => s.ServiceName).ToArray(),
                ServicesDependedOn = service.ServicesDependedOn.Select(s => s.ServiceName).ToArray(),
                MachineName = service.MachineName
            };

            return ToolResult.Ok(result, "WindowsServiceReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Service read failed: {ex.Message}", "WindowsServiceReadTool");
        }
    }
}
