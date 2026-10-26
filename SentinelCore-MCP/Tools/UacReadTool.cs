// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         UacReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text;

using Microsoft.Win32;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying UAC and token elevation state.
/// </summary>
[McpServerToolType]
public sealed class UacReadTool
{

    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "UAC_Read_Settings", ReadOnly = true, Destructive = false)]
    [Description("Reads UAC policy settings from the registry.")]
    public async Task<ToolResult> UacReadSettingsAsync()
    {
        try
        {
            StringBuilder sb = new();
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", false);
            if (key is null)
            {
                return ToolResult.Fail("UAC registry key not found.", "UacReadSettingsTool");
            }

            foreach (string valueName in key.GetValueNames()) sb.AppendLine($"{valueName}={key.GetValue(valueName)}");

            return ToolResult.Ok(sb.ToString(), "UacReadSettingsTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "UacReadSettingsTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "UAC_Read_Token_Elevation", ReadOnly = true, Destructive = false)]
    [Description("Reports whether the current process token is elevated.")]
    public async Task<ToolResult> UacReadTokenElevationAsync()
    {
        try
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            bool elevated = !identity.IsSystem && isAdmin;
            return ToolResult.Ok($"IsElevated={elevated}, IsAdministrator={isAdmin}", "UacReadTokenElevationTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "UacReadTokenElevationTool");
        }
    }
}
