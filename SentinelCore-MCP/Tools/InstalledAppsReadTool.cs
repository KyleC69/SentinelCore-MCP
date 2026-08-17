// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         InstalledAppsReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Management;
using System.Runtime.Versioning;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for enumerating installed applications via MSI registry entries and the Win32_Product CIM class.
/// </summary>
[McpServerToolType]
public sealed class InstalledAppsReadTool
{

    private const string UninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    private const string Wow64UninstallKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";








    [SupportedOSPlatform("windows")]
    private static void CollectFromRegistry(RegistryHive hive, string keyPath, List<Dictionary<string, string?>> results, string? filter)
    {
        using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using RegistryKey? uninstallKey = baseKey.OpenSubKey(keyPath, false);
        if (uninstallKey is null)
        {
            return;
        }

        foreach (string subKeyName in uninstallKey.GetSubKeyNames())
            try
            {
                using RegistryKey? subKey = uninstallKey.OpenSubKey(subKeyName, false);
                if (subKey is null)
                {
                    continue;
                }

                string? displayName = subKey.GetValue("DisplayName")?.ToString();
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(filter) && displayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                results.Add(new Dictionary<string, string?>
                {
                    ["DisplayName"] = displayName,
                    ["Publisher"] = subKey.GetValue("Publisher")?.ToString(),
                    ["Version"] = subKey.GetValue("DisplayVersion")?.ToString(),
                    ["InstallDate"] = subKey.GetValue("InstallDate")?.ToString(),
                    ["UninstallString"] = subKey.GetValue("UninstallString")?.ToString(),
                    ["RegistryPath"] = $"{hive}\\{keyPath}\\{subKeyName}"
                });
            }
            catch
            {
                // Ignore individual corrupted entries.
            }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Installed_Apps_List", ReadOnly = true, Destructive = false)]
    [Description("Lists installed applications from the Add/Remove Programs registry entries.")]
    public static ToolResult InstalledAppsList([Description("Optional publisher or display name filter (partial match).")] string? filter = null)
    {
        try
        {
            List<Dictionary<string, string?>> results = new();
            CollectFromRegistry(RegistryHive.LocalMachine, UninstallKey, results, filter);
            CollectFromRegistry(RegistryHive.LocalMachine, Wow64UninstallKey, results, filter);
            CollectFromRegistry(RegistryHive.CurrentUser, UninstallKey, results, filter);

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Installed app listing failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Installed_Apps_MSI_List", ReadOnly = true, Destructive = false)]
    [Description("Lists installed applications using the Win32_Product CIM provider (MSI API surface).")]
    public static ToolResult InstalledAppsMsiList([Description("Optional product name filter (partial match).")] string? filter = null)
    {
        try
        {
            List<object> results = new();
            using ManagementObjectSearcher searcher = new("root\\cimv2", "SELECT Name, Version, Vendor, InstallDate, IdentifyingNumber FROM Win32_Product");
            foreach (ManagementObject product in searcher.Get())
            {
                string name = product["Name"]?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(filter) && name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                results.Add(new
                {
                    Name = name,
                    Version = product["Version"]?.ToString(),
                    Vendor = product["Vendor"]?.ToString(),
                    InstallDate = product["InstallDate"]?.ToString(),
                    IdentifyingNumber = product["IdentifyingNumber"]?.ToString()
                });
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("MSI product listing failed.");
        }
    }
}
