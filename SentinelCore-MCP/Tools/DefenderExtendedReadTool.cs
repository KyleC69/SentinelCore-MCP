// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         DefenderExtendedReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Microsoft Defender SmartScreen reputation-based protection status.
/// </summary>
[McpServerToolType]
public sealed class DefenderExtendedReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Defender_Read_SmartScreen", ReadOnly = true, Destructive = false)]
    [Description("Reads Windows Defender SmartScreen and reputation-based protection settings from the registry.")]
    public static ToolResult DefenderReadSmartScreen()
    {
        try
        {
            StringBuilder sb = new();

            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                // SmartScreen settings
                using RegistryKey? smartScreenKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\SmartScreenEnabled", false);
                if (smartScreenKey is not null)
                {
                    sb.AppendLine("[SmartScreen Enabled]");
                    sb.AppendLine($"  Value={smartScreenKey.GetValue(null)}");
                }

                // SmartScreen policy
                using RegistryKey? policyKey = baseKey.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\System", false);
                if (policyKey is not null)
                {
                    object? enableSmartScreen = policyKey.GetValue("EnableSmartScreen");
                    object? shellSmartScreen = policyKey.GetValue("ShellSmartScreenEnabled");
                    sb.AppendLine("[SmartScreen Policy]");
                    sb.AppendLine($"  EnableSmartScreen={enableSmartScreen}");
                    sb.AppendLine($"  ShellSmartScreenEnabled={shellSmartScreen}");
                }

                // Windows Defender Smart App Control
                using RegistryKey? sacKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\CI\Policy", false);
                if (sacKey is not null)
                {
                    sb.AppendLine("[Smart App Control / CI Policy]");
                    foreach (string valueName in sacKey.GetValueNames())
                    {
                        sb.AppendLine($"  {valueName}={sacKey.GetValue(valueName)}");
                    }
                }

                // Microsoft Defender Application Guard
                using RegistryKey? appGuardKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Windows Defender Application Guard", false);
                if (appGuardKey is not null)
                {
                    sb.AppendLine("[Application Guard]");
                    foreach (string valueName in appGuardKey.GetValueNames())
                    {
                        sb.AppendLine($"  {valueName}={appGuardKey.GetValue(valueName)}");
                    }
                }

                // Windows Defender Attack Surface Reduction
                using RegistryKey? asrKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard\ASR", false);
                if (asrKey is not null)
                {
                    sb.AppendLine("[Attack Surface Reduction]");
                    foreach (string valueName in asrKey.GetValueNames())
                    {
                        sb.AppendLine($"  {valueName}={asrKey.GetValue(valueName)}");
                    }
                }
            }

            // HKCU SmartScreen
            using (RegistryKey hkcu = Registry.CurrentUser)
            {
                using RegistryKey? smKey = hkcu.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppHost\EnableSmartScreen", false);
                if (smKey is not null)
                {
                    sb.AppendLine("[SmartScreen HKCU]");
                    sb.AppendLine($"  Value={smKey.GetValue(null)}");
                }
            }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("SmartScreen read failed.");
        }
    }
}
