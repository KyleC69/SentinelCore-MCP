// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         UacReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Security.Principal;
using System.Text;
using System.Runtime.Versioning;




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
    public ToolResult uacReadSettings()
    {
        try
        {
            StringBuilder sb = new();
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", false);
            if (key is null)
            {
                return ToolResult.Fail("System UAC policy key not found.");
            }

            foreach (string valueName in key.GetValueNames()) sb.AppendLine($"{valueName}={key.GetValue(valueName)}");

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("UAC settings read failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "UAC_Read_Token_Elevation", ReadOnly = true, Destructive = false)]
    [Description("Reports whether the current process token is elevated.")]
    public ToolResult uacReadTokenElevation()
    {
        try
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            bool elevated = !identity.IsSystem && isAdmin;
            return ToolResult.Ok($"IsElevated={elevated}, IsAdministrator={isAdmin}");
        }
        catch
        {
            return ToolResult.Fail("Token elevation read failed.");
        }
    }
}
