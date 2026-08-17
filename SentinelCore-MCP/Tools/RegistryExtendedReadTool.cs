// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         RegistryExtendedReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying extended registry information: ACLs on registry keys
///     and COM class registrations for COM hijacking detection.
/// </summary>
[McpServerToolType]
public sealed class RegistryExtendedReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Registry_Read_Acl", ReadOnly = true, Destructive = false)]
    [Description("Reads the ACL (access control list) of a registry key for permission auditing.")]
    public static ToolResult RegistryReadAcl(
        [Description("Registry hive abbreviation (HKLM, HKCU, HKCR, HKU, HKCC).")] string hive,
        [Description("The key path within the hive.")] string keyPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hive))
            {
                return ToolResult.Fail("hive is required.");
            }

            if (string.IsNullOrWhiteSpace(keyPath))
            {
                return ToolResult.Fail("keyPath is required.");
            }

            RegistryKey? root = hive.ToUpperInvariant() switch
            {
                "HKLM" => Registry.LocalMachine,
                "HKCU" => Registry.CurrentUser,
                "HKCR" => Registry.ClassesRoot,
                "HKU" => Registry.Users,
                "HKCC" => Registry.CurrentConfig,
                _ => null
            };

            if (root is null)
            {
                return ToolResult.Fail($"Unknown registry hive: {hive}");
            }

            using RegistryKey? key = root.OpenSubKey(keyPath, false);
            if (key is null)
            {
                return ToolResult.Fail($"Registry key not found: {hive}\\{keyPath}");
            }

            // Use PowerShell to get ACL since . doesn't expose registry ACLs easily
            System.Diagnostics.ProcessStartInfo psi = new()
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -Command \"Get-Acl -Path 'Registry::{hive}\\{keyPath}' | Format-List\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return ToolResult.Fail("Unable to start PowerShell for ACL query.");
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return ToolResult.Ok(output);
        }
        catch
        {
            return ToolResult.Fail("Registry ACL read failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "COM_List_Classes", ReadOnly = true, Destructive = false)]
    [Description("Lists COM class registrations from the registry for COM hijacking detection.")]
    public static ToolResult ComListClasses([Description("Maximum number of classes to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();

            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64))
            {
                using RegistryKey? clsidKey = baseKey.OpenSubKey(@"CLSID", false);
                if (clsidKey is not null)
                {
                    foreach (string clsid in clsidKey.GetSubKeyNames())
                    {
                        if (results.Count >= maxRecords) break;

                        using RegistryKey? classKey = clsidKey.OpenSubKey(clsid, false);
                        if (classKey is null) continue;

                        object? inprocServer = null;
                        object? localServer = null;
                        string? inprocPath = null;

                        using RegistryKey? inprocKey = classKey.OpenSubKey(@"InprocServer32", false);
                        if (inprocKey is not null)
                        {
                            inprocServer = inprocKey.GetValue(null); // Default value is the path
                            inprocPath = inprocServer?.ToString();
                        }

                        using RegistryKey? localServerKey = classKey.OpenSubKey(@"LocalServer32", false);
                        if (localServerKey is not null)
                        {
                            localServer = localServerKey.GetValue(null);
                        }

                        string? progId = null;
                        using RegistryKey? progIdKey = classKey.OpenSubKey(@"ProgID", false);
                        if (progIdKey is not null)
                        {
                            progId = progIdKey.GetValue(null)?.ToString();
                        }

                        results.Add(new
                        {
                            CLSID = clsid,
                            ProgID = progId ?? "",
                            InprocServer32 = inprocPath ?? "",
                            LocalServer32 = localServer?.ToString() ?? ""
                        });
                    }
                }
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("COM class listing failed.");
        }
    }
}