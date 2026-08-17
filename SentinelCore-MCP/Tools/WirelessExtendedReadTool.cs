// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         WirelessExtendedReadTool.cs
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
///     Read-only tool for querying wireless connection history from the registry
///     for rogue access point detection.
/// </summary>
[McpServerToolType]
public sealed class WirelessExtendedReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Wireless_List_Connection_History", ReadOnly = true, Destructive = false)]
    [Description("Lists wireless network connection history from the registry for rogue access point detection.")]
    public static ToolResult WirelessListConnectionHistory([Description("Maximum number of profiles to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();

            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                // Wireless profiles are stored under HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles
                using RegistryKey? profilesKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles", false);
                if (profilesKey is not null)
                {
                    foreach (string profileGuid in profilesKey.GetSubKeyNames())
                    {
                        if (results.Count >= maxRecords) break;

                        using RegistryKey? profileKey = profilesKey.OpenSubKey(profileGuid, false);
                        if (profileKey is null) continue;

                        object? profileName = profileKey.GetValue("ProfileName");
                        object? description = profileKey.GetValue("Description");
                        object? category = profileKey.GetValue("Category");
                        object? categoryType = profileKey.GetValue("CategoryType");
                        object? dateCreated = profileKey.GetValue("DateCreated");
                        object? dateLastConnected = profileKey.GetValue("DateLastConnected");
                        object? managed = profileKey.GetValue("Managed");
                        object? nameType = profileKey.GetValue("NameType");

                        // Only include wireless profiles (CategoryType 71 = Wireless)
                        int catType = categoryType is int ct ? ct : -1;
                        if (catType != 71 && catType != -1)
                        {
                            // Include all network types for completeness
                        }

                        results.Add(new
                        {
                            Guid = profileGuid,
                            ProfileName = profileName?.ToString() ?? "",
                            Description = description?.ToString() ?? "",
                            Category = category?.ToString() ?? "",
                            CategoryType = catType,
                            DateCreated = dateCreated?.ToString() ?? "",
                            DateLastConnected = dateLastConnected?.ToString() ?? "",
                            Managed = managed?.ToString() ?? "",
                            NameType = nameType?.ToString() ?? ""
                        });
                    }
                }
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Wireless connection history listing failed.");
        }
    }
}