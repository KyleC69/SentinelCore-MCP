// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         BitlockerReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Management;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying BitLocker volume encryption status via BitLocker WMI v2.
/// </summary>
[McpServerToolType]
public sealed class BitlockerReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Bitlocker_List_Volumes", ReadOnly = true, Destructive = false)]
    [Description("Lists BitLocker-protected volumes and their encryption status.")]
    public static ToolResult BitlockerListVolumes()
    {
        try
        {
            List<object> results = new();
            using ManagementObjectSearcher searcher = new(@"root\cimv2\security\MicrosoftVolumeEncryption", "SELECT DeviceID, ProtectionStatus, EncryptionMethod, ConversionStatus FROM Win32_EncryptableVolume");
            foreach (ManagementObject volume in searcher.Get())
                results.Add(new { DeviceID = volume["DeviceID"]?.ToString(), ProtectionStatus = volume["ProtectionStatus"]?.ToString(), EncryptionMethod = volume["EncryptionMethod"]?.ToString(), ConversionStatus = volume["ConversionStatus"]?.ToString() });

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("BitLocker volume listing failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Bitlocker_Read_Volume", ReadOnly = true, Destructive = false)]
    [Description("Reads the BitLocker metadata / key protector types for a specific volume.")]
    public static ToolResult BitlockerReadVolume([Description("The device ID of the encryptable volume, e.g. \\\\?\\\\Volume{GUID}\\\\.")] string deviceId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return ToolResult.Fail("deviceId is required.");
            }

            string escaped = deviceId.Replace("\\", "\\\\");
            string query = $"SELECT * FROM Win32_EncryptableVolume WHERE DeviceID='{escaped}'";
            StringBuilder sb = new();
            using ManagementObjectSearcher searcher = new(@"root\cimv2\security\MicrosoftVolumeEncryption", query);
            foreach (ManagementObject volume in searcher.Get())
                foreach (PropertyData? property in volume.Properties)
                    sb.AppendLine($"{property.Name}={property.Value}");

            return sb.Length == 0 ? ToolResult.Fail("Volume not found.") : ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("BitLocker volume read failed.");
        }
    }
}
