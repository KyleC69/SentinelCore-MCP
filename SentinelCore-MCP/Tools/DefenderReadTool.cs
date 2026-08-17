// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         DefenderReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Management;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Microsoft Defender configuration and status.
///     Uses Windows Security Center API surface via CIM and registry.
/// </summary>
[McpServerToolType]
public sealed class DefenderReadTool
{







    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Defender_Read_Registry_Config", ReadOnly = true, Destructive = false)]
    [Description("Reads Defender exclusion and general configuration registry entries.")]
    public static ToolResult DefenderReadRegistryConfig()
    {
        try
        {
            StringBuilder sb = new();
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using RegistryKey? key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender", false);
            if (key is null)
            {
                return ToolResult.Fail("Windows Defender registry key not found.");
            }

            foreach (string valueName in key.GetValueNames()) sb.AppendLine($"{valueName}={key.GetValue(valueName)}");

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Defender registry config read failed.");
        }
    }







    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Defender_Read_Status", ReadOnly = true, Destructive = false)]
    [Description("Reads Microsoft Defender antivirus status via the Windows Security Center WMI provider.")]
    public static ToolResult DefenderReadStatus()
    {
        try
        {
            List<object> results = new();
            using ManagementObjectSearcher searcher = new(@"root\Microsoft\Windows\Defender", "SELECT * FROM MSFT_MpComputerStatus");
            foreach (ManagementObject status in searcher.Get())
            {
                Dictionary<string, object?> record = new();
                foreach (PropertyData? property in status.Properties) record[property.Name] = property.Value;

                results.Add(record);
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Defender status read failed.");
        }
    }
}
