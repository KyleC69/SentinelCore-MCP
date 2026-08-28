// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         AccessChkReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;




/// <summary>
///     Read-only tool for auditing effective permissions on files, registry keys,
///     services, and processes using the Sysinternals AccessChk utility.
///     AccessChk must be installed on the host; the tool fails gracefully when absent.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class AccessChkReadTool
{




    /// <summary>
    ///     Probes whether Sysinternals AccessChk is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_AccessChk_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals AccessChk utility is installed and reports its version and path.")]
    public async Task<ToolResult> AccessChkAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("accesschk")).ConfigureAwait(false);
    }






    /// <summary>
    ///     Audits effective permissions granted to a user or group on a file,
    ///     directory, registry key, service, or process.
    /// </summary>
    /// <param name="target">The object to inspect: a file/directory path, registry key path (prefixed with hklm\ or hkcu\), service name (prefixed with "service:"), or process name.</param>
    /// <param name="account">Optional account name to audit. Defaults to the current user.</param>
    /// <param name="recurse">Whether to recurse into sub-objects. Defaults to false.</param>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 200.</param>
    /// <returns>A <see cref="ToolResult" /> containing the AccessChk permission report.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_AccessChk_Read_Permissions", ReadOnly = true, Destructive = false)]
    [Description("Audits effective permissions on files, registry keys, services, or processes using Sysinternals AccessChk. Requires AccessChk to be installed.")]
    public async Task<ToolResult> AccessChkReadPermissionsAsync(
        [Description("The object to inspect: file/directory path, registry key (hklm\\...), service (service:Name), or process name.")] string target,
        [Description("Optional account name to audit. Defaults to the current user.")] string? account = null,
        [Description("Whether to recurse into sub-objects. Defaults to false.")] bool recurse = false,
        [Description("Maximum number of output lines to return. Defaults to 200.")] int maxLines = 200)
    {
        ToolResult? targetValidation = SysinternalsHelper.ValidateArgument(target, "target");
        if (targetValidation is not null)
        {
            return targetValidation;
        }

        if (account is not null)
        {
            ToolResult? accountValidation = SysinternalsHelper.ValidateArgument(account, "account");
            if (accountValidation is not null)
            {
                return accountValidation;
            }
        }

        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -w reports write-level access only (read-only audit surface); -q suppresses the banner.
        StringBuilder arguments = new("-w -q");
        if (recurse)
        {
            arguments.Append(" -s");
        }

        arguments.Append(" \"").Append(account ?? Environment.UserName).Append("\" \"").Append(target).Append('"');

        ToolResult result = await SysinternalsHelper.RunAsync("accesschk", arguments.ToString(), "AccessChk permission audit").ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "AccessChk permission audit");
    }
}
