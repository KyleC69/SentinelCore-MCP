// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         WindowsUpdateReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Windows Update settings.
/// </summary>
[McpServerToolType]
public sealed class WindowsUpdateReadTool
{

    private const string WindowsUpdateAutoUpdateKey = "SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU";
    private const string WindowsUpdatePolicyKey = "SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate";








    [SupportedOSPlatform("windows")]
    private static void ReadKeyValues(RegistryKey root, string keyPath, StringBuilder sb)
    {
        using RegistryKey? key = root.OpenSubKey(keyPath, false);
        if (key is null)
        {
            return;
        }

        sb.AppendLine($"[{keyPath}]");
        foreach (string valueName in key.GetValueNames())
        {
            string displayName = string.IsNullOrEmpty(valueName) ? "(Default)" : valueName;
            sb.AppendLine($"  {displayName}={key.GetValue(valueName)}");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Windows_Update_List_History", ReadOnly = true, Destructive = false)]
    [Description("Lists installed Windows update history using the COM UpdateSession.")]
    public ToolResult windowsUpdateListHistory()
    {
        try
        {
            return ToolResult.Fail("Windows Update history listing is not yet implemented.");
        }
        catch
        {
            return ToolResult.Fail("Windows Update history listing failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Windows_Update_Read_Settings", ReadOnly = true, Destructive = false)]
    [Description("Reads Windows Update policy settings from the registry.")]
    public ToolResult windowsUpdateReadSettings()
    {
        try
        {
            StringBuilder sb = new();
            ReadKeyValues(Registry.LocalMachine, WindowsUpdatePolicyKey, sb);
            ReadKeyValues(Registry.LocalMachine, WindowsUpdateAutoUpdateKey, sb);

            return ToolResult.Ok(sb.Length == 0 ? "No Windows Update policy settings configured." : sb.ToString());

        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Windows Update settings read failed: {ex.Message}");
        }
    }
}
