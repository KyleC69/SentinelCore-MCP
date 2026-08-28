// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         BitlockerReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Management;
using System.Runtime.Versioning;
using System.Text;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying BitLocker volume encryption status via BitLocker WMI v2.
/// </summary>
[SupportedOSPlatform("windows")]
[McpServerToolType]
public sealed class BitlockerReadTool
{

    [McpServerTool(Name = "Bitlocker_List_Volumes", ReadOnly = true, Destructive = false)]
    [Description("Lists BitLocker-protected volumes and their encryption status.")]
    public async Task<ToolResult> BitlockerListVolumesAsync()
    {
        try
        {
            List<object> results = new();
            using ManagementObjectSearcher searcher = new(@"root\cimv2\security\MicrosoftVolumeEncryption", "SELECT DeviceID, ProtectionStatus, EncryptionMethod, ConversionStatus FROM Win32_EncryptableVolume");
            foreach (ManagementObject volume in searcher.Get())
                results.Add(new { DeviceID = volume["DeviceID"]?.ToString(), ProtectionStatus = volume["ProtectionStatus"]?.ToString(), EncryptionMethod = volume["EncryptionMethod"]?.ToString(), ConversionStatus = volume["ConversionStatus"]?.ToString() });

            return ToolResult.Ok(results, "BitlockerReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "BitLocker volume listing");
        }
    }








    [McpServerTool(Name = "Bitlocker_Read_Volume", ReadOnly = true, Destructive = false)]
    [Description("Reads the BitLocker metadata / key protector types for a specific volume.")]
    public async Task<ToolResult> BitlockerReadVolumeAsync([Description("The device ID of the encryptable volume, e.g. \\\\?\\\\Volume{GUID}\\\\.")] string deviceId)
    {
        try
        {
            ToolResult? idValidation = InputValidator.ValidateSanitizedWqlValue(deviceId, "deviceId");
            if (idValidation is not null) return idValidation;

            string query = $"SELECT * FROM Win32_EncryptableVolume WHERE DeviceID='{deviceId.Replace("'", "''")}'";
            StringBuilder sb = new();
            using ManagementObjectSearcher searcher = new(@"root\cimv2\security\MicrosoftVolumeEncryption", query);
            foreach (ManagementObject volume in searcher.Get())
                foreach (PropertyData? property in volume.Properties)
                    sb.AppendLine($"{property.Name}={property.Value}");

            return sb.Length == 0 ? ToolResult.Fail("Volume not found.", "BitlockerReadTool") : ToolResult.Ok(sb.ToString(), "BitlockerReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "BitLocker volume read");
        }
    }
}