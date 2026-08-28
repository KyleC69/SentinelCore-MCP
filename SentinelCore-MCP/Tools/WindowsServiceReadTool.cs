// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         WindowsServiceReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801




using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Text;




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
    public async Task<ToolResult> serviceListAsync([Description("Optional service name filter (partial match).")] string? nameFilter = null, [Description("Maximum number of services to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            var services = ServiceController.GetServices();
            StringBuilder sb = new();
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

                sb.AppendLine($"Name={service.ServiceName}, DisplayName={service.DisplayName}, Status={service.Status}, StartType={service.StartType}");
                count++;
            }

            return ToolResult.Ok(sb.ToString(), "WindowsServiceReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Service listing failed: {ex.Message}", "WindowsServiceReadTool");
        }
    }







    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Service_Read", ReadOnly = true, Destructive = false)]
    [Description("Reads detailed information about a specific Windows service.")]
    public async Task<ToolResult> serviceReadAsync([Description("The service name (not display name) to inspect.")] string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return ToolResult.Fail("serviceName is required.", "WindowsServiceReadTool");
            }

            using ServiceController service = new(serviceName);
            service.Refresh();

            StringBuilder sb = new();
            sb.AppendLine($"Name={service.ServiceName}");
            sb.AppendLine($"DisplayName={service.DisplayName}");
            sb.AppendLine($"Status={service.Status}");
            sb.AppendLine($"StartType={service.StartType}");
            sb.AppendLine($"CanPauseAndContinue={service.CanPauseAndContinue}");
            sb.AppendLine($"CanShutdown={service.CanShutdown}");
            sb.AppendLine($"CanStop={service.CanStop}");
            sb.AppendLine($"DependentServices={string.Join(", ", service.DependentServices.Select(s => s.ServiceName))}");
            sb.AppendLine($"ServicesDependedOn={string.Join(", ", service.ServicesDependedOn.Select(s => s.ServiceName))}");
            sb.AppendLine($"MachineName={service.MachineName}");

            return ToolResult.Ok(sb.ToString(), "WindowsServiceReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Service read failed: {ex.Message}", "WindowsServiceReadTool");
        }
    }
}
