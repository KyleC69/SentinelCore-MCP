// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         DisplayReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying display configuration using Win32 display APIs and CIM video classes.
/// </summary>
[McpServerToolType]
public sealed class DisplayReadTool
{







    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Display_List_Monitors", ReadOnly = true, Destructive = false)]
    [Description("Lists active display monitors using the Win32 display CIM classes.")]
    public static ToolResult DisplayListMonitors()
    {
        try
        {
            List<object> results = new();
            using ManagementObjectSearcher searcher = new("root\\cimv2", "SELECT DeviceID, Name, ScreenWidth, ScreenHeight, PixelsPerXLogicalInch, PixelsPerYLogicalInch FROM Win32_DesktopMonitor");
            foreach (ManagementObject monitor in searcher.Get())
                results.Add(new
                {
                    DeviceID = monitor["DeviceID"]?.ToString(),
                    Name = monitor["Name"]?.ToString(),
                    ScreenWidth = monitor["ScreenWidth"]?.ToString(),
                    ScreenHeight = monitor["ScreenHeight"]?.ToString(),
                    PixelsPerXLogicalInch = monitor["PixelsPerXLogicalInch"]?.ToString(),
                    PixelsPerYLogicalInch = monitor["PixelsPerYLogicalInch"]?.ToString()
                });

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Display monitor listing failed.");
        }
    }







    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Display_Read_Virtual_Screen", ReadOnly = true, Destructive = false)]
    [Description("Reads the virtual screen geometry using Win32 API (GetSystemMetrics).")]
    public static ToolResult DisplayReadVirtualScreen()
    {
        try
        {
            int x = NativeMethods.GetSystemMetrics(NativeMethods.SM_XVIRTUALSCREEN);
            int y = NativeMethods.GetSystemMetrics(NativeMethods.SM_YVIRTUALSCREEN);
            int cx = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXVIRTUALSCREEN);
            int cy = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYVIRTUALSCREEN);
            int monitors = NativeMethods.GetSystemMetrics(NativeMethods.SM_CMONITORS);

            return ToolResult.Ok($"VirtualScreen=({x},{y},{cx},{cy}), Monitors={monitors}");
        }
        catch
        {
            return ToolResult.Fail("Virtual screen read failed.");
        }
    }








    private static class NativeMethods
    {
        public const int SM_CMONITORS = 80;
        public const int SM_CXVIRTUALSCREEN = 78;
        public const int SM_CYVIRTUALSCREEN = 79;
        public const int SM_XVIRTUALSCREEN = 76;
        public const int SM_YVIRTUALSCREEN = 77;








        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);
    }
}
