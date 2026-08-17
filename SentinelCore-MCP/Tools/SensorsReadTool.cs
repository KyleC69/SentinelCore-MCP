// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         SensorsReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801




using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Management;
using System.Text;
using System.Text.Json;
using System.Runtime.Versioning;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for enumerating sensor devices using the Windows Sensor API surface (via CIM).
/// </summary>
[McpServerToolType]
public sealed class SensorsReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sensor_List_Devices", ReadOnly = true, Destructive = false)]
    [Description("Lists sensor devices via CIM Win32_PnPEntity matching common sensor class names.")]
    public ToolResult sensorListDevices()
    {
        try
        {
            List<object> results = new();
            using ManagementObjectSearcher searcher = new("root\\cimv2", "SELECT DeviceID, Name, Status, PNPClass, Manufacturer FROM Win32_PnPEntity WHERE PNPClass LIKE '%Sensor%' OR Name LIKE '%Sensor%'");
            foreach (ManagementObject device in searcher.Get())
                results.Add(new
                {
                    DeviceID = device["DeviceID"]?.ToString(),
                    Name = device["Name"]?.ToString(),
                    Status = device["Status"]?.ToString(),
                    PNPClass = device["PNPClass"]?.ToString(),
                    Manufacturer = device["Manufacturer"]?.ToString()
                });

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Sensor device listing failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sensor_Read_Location_Service", ReadOnly = true, Destructive = false)]
    [Description("Reads the Windows sensor permissions / location service status from CIM.")]
    public ToolResult sensorReadLocationService()
    {
        try
        {
            StringBuilder sb = new();
            using ManagementObjectSearcher searcher = new("root\\cimv2", "SELECT Status FROM Win32_Service WHERE Name='lfsvc'");
            foreach (ManagementObject service in searcher.Get())
                sb.AppendLine($"LocationService(lfsvc) Status={service["Status"]}");

            return sb.Length == 0 ? ToolResult.Fail("Location service not found.") : ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Sensor location service read failed.");
        }
    }
}
