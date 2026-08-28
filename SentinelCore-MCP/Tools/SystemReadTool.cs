// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         SystemReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying system information: OS version, build, architecture,
///     uptime, hostname, and hardware inventory (CPU, RAM, disk).
/// </summary>
[McpServerToolType]
public sealed class SystemReadTool
{

    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "System_Read_Info", ReadOnly = true, Destructive = false)]
    [Description("Reads system information including OS version, build, architecture, uptime, and hostname.")]
    public async Task<ToolResult> SystemReadInfoAsync()
    {
        try
        {
            var result = new
            {
                    Environment.MachineName,
                    OSVersion = Environment.OSVersion.VersionString,
                    OSPlatform = Environment.OSVersion.Platform.ToString(),
                    Environment.Is64BitOperatingSystem,
                    Environment.Is64BitProcess,
                    Environment.ProcessorCount,
                    Environment.SystemDirectory,
                    Environment.UserName,
                    Environment.UserDomainName,
                    CLRVersion = Environment.Version.ToString(),
                    SystemUpTime = TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(@"dd\.hh\:mm\:ss"),
                    Environment.WorkingSet,
                    Environment.CurrentDirectory
            };

            return ToolResult.Ok(result, "SystemReadTool");
        }
        catch
        {
            return ToolResult.Fail("System info read failed.", "SystemReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "System_Read_TimeZone", ReadOnly = true, Destructive = false)]
    [Description("Reads the system time zone information for timeline correlation.")]
    public async Task<ToolResult> SystemReadTimeZoneAsync()
    {
        try
        {
            TimeZoneInfo tz = TimeZoneInfo.Local;
            var result = new
            {
                    tz.Id,
                    tz.DisplayName,
                    tz.StandardName,
                    tz.DaylightName,
                    BaseUtcOffset = tz.BaseUtcOffset.ToString(),
                    tz.SupportsDaylightSavingTime,
                    CurrentUtcOffset = tz.GetUtcOffset(DateTimeOffset.Now).ToString()
            };

            return ToolResult.Ok(result, "SystemReadTool");
        }
        catch
        {
            return ToolResult.Fail("Time zone read failed.", "SystemReadTool");
        }
    }
}