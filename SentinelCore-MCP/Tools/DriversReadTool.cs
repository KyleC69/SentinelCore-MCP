// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         DriversReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for enumerating installed Windows drivers using the Service Control Manager (SCM) API.
///     Drivers are exposed as kernel services with service type SERVICE_KERNEL_DRIVER or SERVICE_FILE_SYSTEM_DRIVER.
/// </summary>
[McpServerToolType]
public sealed class DriversReadTool
{







    [SupportedOSPlatform("windows")]
    private static string GetDriverKind(string serviceName)
    {
        using ServiceController service = new(serviceName);
        // ServiceController does not expose ServiceType directly, but device services are by definition kernel/file-system drivers.
        // A conservative classification: file-system drivers commonly include 'fs' naming; otherwise report kernel.
        return serviceName.Contains("fs", StringComparison.OrdinalIgnoreCase) || serviceName.Contains("filter", StringComparison.OrdinalIgnoreCase) ? "filesystem" : "kernel";

    }







    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Driver_List", ReadOnly = true, Destructive = false)]
    [Description("Lists installed kernel and file-system drivers via SCM.")]
    public static ToolResult DriverList([Description("Optional filter: kernel, filesystem, or all. Defaults to all.")] string? typeFilter = null)
    {
        try
        {
            string filter = typeFilter != null ? typeFilter.ToLowerInvariant() : "all";
            StringBuilder sb = new();
            var services = ServiceController.GetDevices();
            foreach (ServiceController service in services)
            {
                string kind;
                try
                {
                    kind = GetDriverKind(service.ServiceName);
                }
                catch
                {
                    kind = "unknown";
                }

                if (!filter.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    if (filter == "kernel" && !kind.Contains("kernel", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (filter == "filesystem" && !kind.Contains("filesystem", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                sb.AppendLine($"Name={service.ServiceName}, DisplayName={service.DisplayName}, Status={service.Status}, StartType={service.StartType}, Kind={kind}");
            }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Driver listing failed.");
        }
    }
}
