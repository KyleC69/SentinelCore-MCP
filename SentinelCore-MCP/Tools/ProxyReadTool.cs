// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         ProxyReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying system proxy configuration from the registry and WinHTTP.
/// </summary>
[McpServerToolType]
public sealed class ProxyReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Proxy_Read_System", ReadOnly = true, Destructive = false)]
    [Description("Reads the system proxy configuration from the Internet Settings registry key.")]
    public ToolResult proxyReadSystem()
    {
        try
        {
            StringBuilder sb = new();
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings", false);
            if (key is null)
            {
                return ToolResult.Fail("Internet Settings registry key not found.");
            }

            foreach (string valueName in key.GetValueNames())
                if (valueName.Contains("Proxy", StringComparison.OrdinalIgnoreCase) || valueName.Contains("AutoConfig", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"{valueName}={key.GetValue(valueName)}");
                }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("System proxy read failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Proxy_Read_WinHTTP", ReadOnly = true, Destructive = false)]
    [Description("Reads the WinHTTP proxy configuration using the netsh native command (read-only).")]
    public ToolResult proxyReadWinhttp()
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "netsh",
                Arguments = "winhttp show proxy",
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
            return ToolResult.Fail("WinHTTP proxy read failed.");
        }
    }
}
