// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         DefenderReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Management;
using System.Runtime.Versioning;
using System.Text;

using Microsoft.Win32;

using ModelContextProtocol.Server;




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
    public async Task<ToolResult> DefenderReadRegistryConfigAsync()
    {
        try
        {
            StringBuilder sb = new();
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using RegistryKey? key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender", false);
            if (key is null)
            {
                return ToolResult.Fail("Windows Defender registry key not found.", "DefenderReadTool");
            }

            foreach (string valueName in key.GetValueNames()) sb.AppendLine($"{valueName}={key.GetValue(valueName)}");

            return ToolResult.Ok(sb.ToString(), "DefenderReadTool");
        }
        catch
        {
            return ToolResult.Fail("Defender registry config read failed.", "DefenderReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Defender_Read_Status", ReadOnly = true, Destructive = false)]
    [Description("Reads Microsoft Defender antivirus status via the Windows Security Center WMI provider.")]
    public async Task<ToolResult> DefenderReadStatusAsync()
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

            return ToolResult.Ok(results, "DefenderReadTool");
        }
        catch
        {
            return ToolResult.Fail("Defender status read failed.", "DefenderReadTool");
        }
    }
}