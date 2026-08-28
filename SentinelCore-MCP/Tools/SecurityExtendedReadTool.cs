// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         SecurityExtendedReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tools for querying extended Windows security features:
///     Credential Guard, Secure Boot, TPM, and Exploit Protection (DEP/ASLR/CFG).
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class SecurityExtendedReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Security_Read_Credential_Guard", ReadOnly = true, Destructive = false)]
    [Description("Reads Credential Guard and LSA Protection status from the registry and system configuration.")]
    public async Task<ToolResult> SecurityReadCredentialGuardAsync()
    {
        try
        {
            StringBuilder sb = new();

            // LSA Protection (RunAsPPL)
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using RegistryKey? lsaKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Lsa", false);
                if (lsaKey is not null)
                {
                    object? runAsPpl = lsaKey.GetValue("RunAsPPL");
                    object? lsaCfgFlags = lsaKey.GetValue("LsaCfgFlags");
                    sb.AppendLine($"RunAsPPL={runAsPpl}");
                    sb.AppendLine($"LsaCfgFlags={lsaCfgFlags}");
                }

                // Credential Guard status via Device Guard
                using RegistryKey? dgKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\DeviceGuard", false);
                if (dgKey is not null)
                {
                    object? enableVirtualizationBasedSecurity = dgKey.GetValue("EnableVirtualizationBasedSecurity");
                    object? requirePlatformSecurityFeatures = dgKey.GetValue("RequirePlatformSecurityFeatures");
                    object? running = dgKey.GetValue("Running");
                    sb.AppendLine($"[Device Guard]");
                    sb.AppendLine($"EnableVirtualizationBasedSecurity={enableVirtualizationBasedSecurity}");
                    sb.AppendLine($"RequirePlatformSecurityFeatures={requirePlatformSecurityFeatures}");
                    sb.AppendLine($"Running={running}");
                }
            }

            return ToolResult.Ok(sb.ToString(), "SecurityExtendedReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "Credential Guard read");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Security_Read_SecureBoot", ReadOnly = true, Destructive = false)]
    [Description("Reads Secure Boot status from the registry and UEFI variables.")]
    public async Task<ToolResult> SecurityReadSecureBootAsync()
    {
        try
        {
            StringBuilder sb = new();

            // Check Secure Boot state from registry
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using RegistryKey? sbKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot", false);
                if (sbKey is not null)
                {
                    sb.AppendLine("[SecureBoot Registry Key]");
                    foreach (string valueName in sbKey.GetValueNames())
                    {
                        sb.AppendLine($"  {valueName}={sbKey.GetValue(valueName)}");
                    }

                    // Check State subkey
                    using RegistryKey? stateKey = sbKey.OpenSubKey("State", false);
                    if (stateKey is not null)
                    {
                        sb.AppendLine("[SecureBoot State]");
                        foreach (string valueName in stateKey.GetValueNames())
                        {
                            sb.AppendLine($"  {valueName}={stateKey.GetValue(valueName)}");
                        }
                    }
                }
            }

            // Also check via the UEFI variable through the registry-backed state value
            // (Confirm-SecureBootUEFI requires admin; the registry State value is readable without elevation).
            object? uefiState = null;
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using RegistryKey? uefiKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State", false);
                uefiState = uefiKey?.GetValue("UEFISecureBootEnabled");
            }

            sb.AppendLine(uefiState is not null
                ? $"[UEFI State] SecureBootEnabled={uefiState}"
                : "[UEFI State] Secure Boot state not available (system may not support UEFI Secure Boot).");

            return ToolResult.Ok(sb.ToString(), "SecurityExtendedReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "Secure Boot read");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Security_Read_TPM", ReadOnly = true, Destructive = false)]
    [Description("Reads TPM (Trusted Platform Module) status and information from the registry.")]
    public async Task<ToolResult> SecurityReadTpmAsync()
    {
        try
        {
            StringBuilder sb = new();

            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                // TPM Ready status
                using RegistryKey? tpmKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Tpm", false);
                if (tpmKey is not null)
                {
                    sb.AppendLine("[TPM]");
                    object? ready = tpmKey.GetValue("Ready");
                    sb.AppendLine($"  Ready={ready}");

                    // Check subkeys for more info
                    foreach (string subKeyName in tpmKey.GetSubKeyNames())
                    {
                        using RegistryKey? subKey = tpmKey.OpenSubKey(subKeyName, false);
                        if (subKey is not null)
                        {
                            sb.AppendLine($"  [{subKeyName}]");
                            foreach (string valueName in subKey.GetValueNames())
                            {
                                sb.AppendLine($"    {valueName}={subKey.GetValue(valueName)}");
                            }
                        }
                    }
                }

                // TPM Spec version from base services
                using RegistryKey? tpmSpecKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tpm\Parameters", false);
                if (tpmSpecKey is not null)
                {
                    sb.AppendLine("[TPM Parameters]");
                    foreach (string valueName in tpmSpecKey.GetValueNames())
                    {
                        object? val = tpmSpecKey.GetValue(valueName);
                        if (val is byte[] bytes)
                        {
                            sb.AppendLine($"  {valueName}={Convert.ToHexString(bytes)}");
                        }
                        else
                        {
                            sb.AppendLine($"  {valueName}={val}");
                        }
                    }
                }
            }

            return ToolResult.Ok(sb.ToString(), "SecurityExtendedReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "TPM read");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Security_Read_Exploit_Protection", ReadOnly = true, Destructive = false)]
    [Description("Reads Windows Defender Exploit Guard, DEP, ASLR, and Control Flow Guard settings from the registry.")]
    public async Task<ToolResult> SecurityReadExploitProtectionAsync()
    {
        try
        {
            StringBuilder sb = new();

            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                // DEP (Data Execution Prevention)
                using RegistryKey? depKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", false);
                if (depKey is not null)
                {
                    sb.AppendLine("[Memory Management / DEP]");
                    object? depEnable = depKey.GetValue("EnableDEP");
                    object? depPolicy = depKey.GetValue("DEPFlags");
                    sb.AppendLine($"  EnableDEP={depEnable}");
                    sb.AppendLine($"  DEPFlags={depPolicy}");
                }

                // Exploit Protection system settings
                using RegistryKey? epKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options", false);
                if (epKey is not null)
                {
                    sb.AppendLine("[Image File Execution Options]");
                    foreach (string subKeyName in epKey.GetSubKeyNames())
                    {
                        using RegistryKey? subKey = epKey.OpenSubKey(subKeyName, false);
                        if (subKey is not null)
                        {
                            object? mitigationOptions = subKey.GetValue("MitigationOptions");
                            if (mitigationOptions is not null)
                            {
                                sb.AppendLine($"  {subKeyName}: MitigationOptions present");
                            }
                        }
                    }
                }

                // Exploit Guard settings
                using RegistryKey? egKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard", false);
                if (egKey is not null)
                {
                    sb.AppendLine("[Windows Defender Exploit Guard]");
                    foreach (string valueName in egKey.GetValueNames())
                    {
                        sb.AppendLine($"  {valueName}={egKey.GetValue(valueName)}");
                    }
                }

                // ASLR
                using RegistryKey? aslrKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", false);
                if (aslrKey is not null)
                {
                    object? moveImages = aslrKey.GetValue("MoveImages");
                    sb.AppendLine($"[ASLR]");
                    sb.AppendLine($"  MoveImages={moveImages}");
                }
            }

            return ToolResult.Ok(sb.ToString(), "SecurityExtendedReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "Exploit protection read");
        }
    }
}
