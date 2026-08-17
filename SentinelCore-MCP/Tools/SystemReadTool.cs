// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         SystemReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;




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
    public static ToolResult SystemReadInfo()
    {
        try
        {
            var result = new
            {
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.VersionString,
                OSPlatform = Environment.OSVersion.Platform.ToString(),
                Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                Is64BitProcess = Environment.Is64BitProcess,
                ProcessorCount = Environment.ProcessorCount,
                SystemDirectory = Environment.SystemDirectory,
                UserName = Environment.UserName,
                UserDomainName = Environment.UserDomainName,
                CLRVersion = Environment.Version.ToString(),
                SystemUpTime = TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(@"dd\.hh\:mm\:ss"),
                WorkingSet = Environment.WorkingSet,
                CurrentDirectory = Environment.CurrentDirectory
            };

            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("System info read failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "System_Read_TimeZone", ReadOnly = true, Destructive = false)]
    [Description("Reads the system time zone information for timeline correlation.")]
    public static ToolResult SystemReadTimeZone()
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
                SupportsDaylightSavingTime = tz.SupportsDaylightSavingTime,
                CurrentUtcOffset = tz.GetUtcOffset(DateTimeOffset.Now).ToString()
            };

            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Time zone read failed.");
        }
    }
}