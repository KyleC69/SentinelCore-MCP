// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         ServiceExtendedReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Windows service ACLs (access control lists)
///     for service permission misconfiguration detection.
/// </summary>
[McpServerToolType]
public sealed class ServiceExtendedReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Service_Read_Acl", ReadOnly = true, Destructive = false)]
    [Description("Reads the ACL (access control list) of a Windows service for permission auditing.")]
    public static ToolResult ServiceReadAcl([Description("The service name to inspect.")] string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return ToolResult.Fail("serviceName is required.");
            }

            // Use sc.exe sdshow to get the service security descriptor
            System.Diagnostics.ProcessStartInfo psi = new()
            {
                FileName = "sc",
                Arguments = $"sdshow {serviceName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return ToolResult.Fail("Unable to start sc.exe.");
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (string.IsNullOrWhiteSpace(output.Trim()))
            {
                return ToolResult.Fail($"Service not found or no ACL available: {serviceName}");
            }

            // Also get service config for context
            System.Diagnostics.ProcessStartInfo psiConfig = new()
            {
                FileName = "sc",
                Arguments = $"qc {serviceName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using System.Diagnostics.Process? processConfig = System.Diagnostics.Process.Start(psiConfig);
            StringBuilder sb = new();
            sb.AppendLine($"[Service: {serviceName}]");
            sb.AppendLine("[Security Descriptor (SDDL)]");
            sb.AppendLine(output.Trim());

            if (processConfig is not null)
            {
                string configOutput = processConfig.StandardOutput.ReadToEnd();
                processConfig.WaitForExit();
                sb.AppendLine("[Service Configuration]");
                sb.AppendLine(configOutput.Trim());
            }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Service ACL read failed.");
        }
    }
}