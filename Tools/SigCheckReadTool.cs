// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         SigCheckReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  102702



using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for verifying file digital signatures and reputation using
///     the Sysinternals SigCheck utility. Unsigned or revoked binaries are a
///     primary malware indicator.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class SigCheckReadTool
{

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

        string[] lines = output.Split('\n');
        if (lines.Length <= maxLines)
        {
            return output;
        }

        return string.Join('\n', lines.Take(maxLines)) + $"\n... (truncated to {maxLines} lines)";
    }








    /// <summary>
    ///     Verifies the digital signature of a file and reports certificate details
    ///     using SigCheck.
    /// </summary>
    /// <param name="filePath">The absolute path of the file to verify.</param>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 100.</param>
    /// <returns>A <see cref="ToolResult" /> containing the SigCheck verification output.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_SigCheck_Verify_File", ReadOnly = true, Destructive = false)]
    [Description("Verifies a file's digital signature and reports certificate details using Sysinternals SigCheck. Requires SigCheck to be installed; revocation checks make outbound network calls to certificate authorities.")]
    public async Task<ToolResult> SigCheckVerifyFileAsync([Description("The absolute path of the file to verify.")] string filePath, [Description("Maximum number of output lines to return. Defaults to 100.")] int maxLines = 100)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return ToolResult.Fail("filePath cannot be empty.", "Input validation");
        }

        if (!File.Exists(filePath))
        {
            return ToolResult.Fail($"File not found: {filePath}", "SigCheck verification");
        }

        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -a shows extended version info; -nobanner suppresses the banner; /accepteula suppresses the EULA prompt.
        try
        {
            ProcessStartInfo psi = new()
            {
                    FileName = "sigcheck.exe",
                    Arguments = $"-a -nobanner /accepteula \"{filePath}\"",
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
                return ToolResult.Fail($"SigCheck failed with exit code {process.ExitCode}: {error.Trim()}", "SigCheck verification");
            }

            // Limit lines
            string limitedOutput = LimitLines(output, maxLines);

            return ToolResult.Ok(limitedOutput, "SigCheck verification complete.");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "SigCheck verification failed.");
        }
    }
}