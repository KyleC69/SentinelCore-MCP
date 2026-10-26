// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         AccessChkReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

using ModelContextProtocol.Server;




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
    ///     Audits effective permissions granted to a user or group on a file,
    ///     directory, registry key, service, or process.
    /// </summary>
    /// <param name="target">
    ///     The object to inspect: a file/directory path, registry key path (prefixed with hklm\ or hkcu\),
    ///     service name (prefixed with "service:"), or process name.
    /// </param>
    /// <param name="account">Optional account name to audit. Defaults to the current user.</param>
    /// <param name="recurse">Whether to recurse into sub-objects. Defaults to false.</param>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 200.</param>
    /// <returns>A <see cref="ToolResult" /> containing the AccessChk permission report.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_AccessChk_Read_Permissions", ReadOnly = true, Destructive = false)]
    [Description("Audits effective permissions on files, registry keys, services, or processes using Sysinternals AccessChk. Requires AccessChk to be installed.")]
    public async Task<ToolResult> AccessChkReadPermissionsAsync([Description("The object to inspect: file/directory path, registry key (hklm\\...), service (service:Name), or process name.")] string target, [Description("Optional account name to audit. Defaults to the current user.")] string? account = null, [Description("Whether to recurse into sub-objects. Defaults to false.")] bool recurse = false, [Description("Maximum number of output lines to return. Defaults to 200.")] int maxLines = 200)
    {
        // Validate target
        if (string.IsNullOrWhiteSpace(target))
        {
            return ToolResult.Fail("target is required.", "Input validation");
        }

        // Validate account if provided
        if (account is not null && string.IsNullOrWhiteSpace(account))
        {
            return ToolResult.Fail("account cannot be empty.", "Input validation");
        }

        // Validate maxLines
        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -w reports write-level access only (read-only audit surface); -q suppresses the banner.
        // -nobanner suppresses the banner; /accepteula suppresses the EULA prompt.
        StringBuilder arguments = new("-w -q -nobanner /accepteula");
        if (recurse)
        {
            arguments.Append(" -s");
        }

        arguments.Append(" \"").Append(account ?? Environment.UserName).Append("\" \"").Append(target).Append('"');

        try
        {
            ProcessStartInfo psi = new()
            {
                    FileName = "accesschk.exe",
                    Arguments = arguments.ToString(),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
            };

            using Process process = new() { StartInfo = psi };
            process.Start();

            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync().ConfigureAwait(false);

            string output = await outputTask.ConfigureAwait(false);
            string error = await errorTask.ConfigureAwait(false);

            if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(output))
            {
                return ToolResult.Fail($"AccessChk failed with exit code {process.ExitCode}: {error.Trim()}", "AccessChk permission audit");
            }

            // Limit lines
            string limitedOutput = LimitLines(output, maxLines);

            return ToolResult.Ok(limitedOutput, "AccessChk permission audit complete.");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "AccessChk permission audit failed.");
        }
    }








    /// <summary>
    ///     Limits the output to the specified maximum number of lines.
    /// </summary>
    /// <param name="output">The output to limit.</param>
    /// <param name="maxLines">Maximum number of lines to return.</param>
    /// <returns>The limited output.</returns>
    private static string LimitLines(string output, int maxLines)
    {
        if (string.IsNullOrEmpty(output))
        {
            return output;
        }

        string[] lines = output.Split(["\r\n", "\n"], StringSplitOptions.None);
        if (lines.Length <= maxLines)
        {
            return output;
        }

        return string.Join("\n", lines.Take(maxLines)) + $"\n... (truncated to {maxLines} lines)";
    }
}
