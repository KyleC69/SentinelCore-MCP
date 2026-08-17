// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         NotificationsReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Runtime.Versioning;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Windows notification platform configuration and installed notification apps.
/// </summary>
[McpServerToolType]
public sealed class NotificationsReadTool
{

    private const string ToastKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings";








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Notification_List_Apps", ReadOnly = true, Destructive = false)]
    [Description("Lists notification settings and app entries from the Windows notification registry store.")]
    public ToolResult notificationListApps()
    {
        try
        {
            List<Dictionary<string, object?>> results = new();
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            using RegistryKey? settingsKey = baseKey.OpenSubKey(ToastKey, false);
            if (settingsKey is not null)
            {
                foreach (string appKeyName in settingsKey.GetSubKeyNames())
                    try
                    {
                        using RegistryKey? appKey = settingsKey.OpenSubKey(appKeyName, false);
                        if (appKey is null)
                        {
                            continue;
                        }

                        results.Add(new Dictionary<string, object?>
                        {
                            ["App"] = appKeyName,
                            ["Enabled"] = appKey.GetValue("Enabled"),
                            ["ShowBanner"] = appKey.GetValue("ShowBannerAndSound"),
                            ["ShowNotificationActions"] = appKey.GetValue("ShowNotificationActions"),
                            ["LastModified"] = appKey.GetValue("LastNotificationAdded")
                        });
                    }
                    catch
                    {
                        // Ignore unreadable entries.
                    }
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Notification app listing failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Notification_Read_Quiet_Hours", ReadOnly = true, Destructive = false)]
    [Description("Reads the global Windows quiet hours / do-not-disturb state from the registry.")]
    public ToolResult notificationReadQuietHours()
    {
        try
        {
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            using RegistryKey? key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\NOC_GLOBAL_SETTING", false);
            if (key is null)
            {
                return ToolResult.Fail("Quiet-hours registry key not present.");
            }

            StringBuilder sb = new();
            foreach (string valueName in key.GetValueNames()) sb.AppendLine($"{valueName}={key.GetValue(valueName)}");

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Quiet hours read failed.");
        }
    }
}
