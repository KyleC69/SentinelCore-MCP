// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         ShellExplorerReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;

using Microsoft.Win32;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying shell and Explorer settings.
/// </summary>
[McpServerToolType]
public sealed class ShellExplorerReadTool
{

    private const string AdvancedKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";
    private const string ExplorerKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer";








    [SupportedOSPlatform("windows")]
    private static void ReadKeyValues(RegistryKey root, string keyPath, StringBuilder sb, string[] valueNames)
    {
        using RegistryKey? key = root.OpenSubKey(keyPath, false);
        if (key is null)
        {
            return;
        }

        sb.AppendLine($"[{keyPath}]");
        foreach (string valueName in valueNames)
        {
            object? value = key.GetValue(valueName);
            if (value is not null)
            {
                sb.AppendLine($"  {valueName}={value}");
            }
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Shell_Explorer_Read_Settings", ReadOnly = true, Destructive = false)]
    [Description("Reads common Explorer settings such as hidden files and file extensions.")]
    public async Task<ToolResult> shellExplorerReadSettingsAsync()
    {
        try
        {
            StringBuilder sb = new();
            ReadKeyValues(Registry.CurrentUser, AdvancedKey, sb, ["Hidden", "ShowSuperHidden", "HideFileExt", "LaunchTo"]);
            ReadKeyValues(Registry.CurrentUser, ExplorerKey, sb, ["EnableAutoTray", "ShellState"]);

            return ToolResult.Ok(sb.ToString(), "ShellExplorerReadTool");
        }
        catch
        {
            return ToolResult.Fail("Shell Explorer settings read failed.", "ShellExplorerReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Shell_Taskbar_Pinned_List", ReadOnly = true, Destructive = false)]
    [Description("Lists pinned items in the Windows taskbar Quick Launch/User Pinned path.")]
    public async Task<ToolResult> shellTaskbarPinnedListAsync()
    {
        try
        {
            string pinnedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar");
            if (!Directory.Exists(pinnedPath))
            {
                return ToolResult.Fail($"Taskbar pinned path not found: {pinnedPath}", "ShellExplorerReadTool");
            }

            StringBuilder sb = new();
            foreach (string file in Directory.GetFiles(pinnedPath, "*.lnk")) sb.AppendLine(Path.GetFileName(file));

            return ToolResult.Ok(sb.ToString(), "ShellExplorerReadTool");
        }
        catch
        {
            return ToolResult.Fail("Taskbar pinned listing failed.", "ShellExplorerReadTool");
        }
    }
}