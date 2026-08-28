// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         SysinternalsHelper.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Diagnostics;
using System.Runtime.Versioning;




namespace SentinelCoreMCP.Tools.Interop;





/// <summary>
///     Shared infrastructure for locating and invoking Windows Sysinternals
///     diagnostic utilities in a read-only manner.
///     Sysinternals binaries are optional on the host system; every consumer must
///     fail gracefully with a structured <see cref="ToolResult" /> when a binary
///     is not installed rather than crashing the server (spec §2.4).
/// </summary>
[SupportedOSPlatform("windows")]
internal static class SysinternalsHelper
{

    /// <summary>
    ///     Well-known directories where Sysinternals suites are commonly installed.
    /// </summary>
    private static readonly string[] CandidateDirectories =
    [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Sysinternals"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Sysinternals"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sysinternals"),
            @"C:\Sysinternals",
            @"C:\Tools\Sysinternals"
    ];

    /// <summary>
    ///     Members of the PsTools suite probed by the PsTools suite inventory tool.
    /// </summary>
    internal static readonly string[] PsToolsSuiteMembers =
    [
            "psexec", "psfile", "psgetsid", "psinfo", "pskill", "pslist",
            "psloggedon", "psloglist", "pspasswd", "psservice", "psshutdown", "pssuspend"
    ];








    /// <summary>
    ///     Limits native command output to the specified number of lines so that
    ///     verbose Sysinternals tools cannot flood the MCP channel.
    /// </summary>
    /// <param name="output">The raw standard output.</param>
    /// <param name="maxLines">The maximum number of lines to retain.</param>
    /// <returns>The truncated output, with an ellipsis marker when lines were dropped.</returns>
    internal static string LimitLines(string output, int maxLines)
    {
        string[] lines = output.Split(["\r\n", "\n"], StringSplitOptions.None);
        if (lines.Length <= maxLines)
        {
            return output;
        }

        IEnumerable<string> retained = lines.Take(maxLines);
        return string.Join(Environment.NewLine, retained) + Environment.NewLine + $"...[truncated at {maxLines} lines]";
    }








    /// <summary>
    ///     Probes the availability and version metadata of a Sysinternals binary
    ///     without executing it.
    /// </summary>
    /// <param name="toolName">The base tool name without extension.</param>
    /// <returns>A <see cref="SysinternalsBinaryInfo" /> wrapped in a successful <see cref="ToolResult" />.</returns>
    internal static ToolResult ProbeAvailability(string toolName)
    {
        string? path = ResolveBinary(toolName);
        string binaryName = toolName + "64.exe";

        if (path is null)
        {
            return ToolResult.Ok(new SysinternalsBinaryInfo(toolName, binaryName, Available: false, Path: null, FileVersion: null, FileDescription: null), $"Sysinternals {toolName} is not installed on this system.");
        }

        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
        return ToolResult.Ok(new SysinternalsBinaryInfo(ToolName: toolName, BinaryName: Path.GetFileName(path), Available: true, Path: path, FileVersion: versionInfo.FileVersion, FileDescription: versionInfo.FileDescription), $"Sysinternals {toolName} is available.");
    }








    /// <summary>
    ///     Resolves the executable path for a Sysinternals tool, preferring the 64-bit
    ///     variant. Probes well-known install directories and then the PATH.
    /// </summary>
    /// <param name="toolName">The base tool name without extension (e.g., "accesschk").</param>
    /// <returns>The full path of the resolved binary, or null when not found.</returns>
    internal static string? ResolveBinary(string toolName)
    {
        string[] nameVariants = [toolName + "64.exe", toolName + ".exe"];

        foreach (string name in nameVariants)
        {
            foreach (string dir in CandidateDirectories)
            {
                string candidatePath = Path.Combine(dir, name);
                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }
            }

            string? pathVariable = Environment.GetEnvironmentVariable("PATH");
            if (pathVariable is null)
            {
                continue;
            }

            foreach (string rawDir in pathVariable.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    string candidatePath = Path.Combine(rawDir.Trim(), name);
                    if (File.Exists(candidatePath))
                    {
                        return candidatePath;
                    }
                }
                catch (ArgumentException)
                {
                    // Skip malformed PATH entries.
                }
            }
        }

        return null;
    }








    /// <summary>
    ///     Runs a Sysinternals utility with the specified arguments and returns the
    ///     standard output, or a failure result when the binary is missing, times out,
    ///     or exits non-zero.
    /// </summary>
    /// <param name="toolName">The base tool name without extension (e.g., "handle").</param>
    /// <param name="arguments">The validated command-line arguments.</param>
    /// <param name="operation">The operation description for error reporting.</param>
    /// <param name="timeoutSeconds">The maximum time to wait for exit. Defaults to 60.</param>
    /// <returns>A <see cref="ToolResult" /> containing the standard output on success.</returns>
    internal static async Task<ToolResult> RunAsync(string toolName, string arguments, string operation, int timeoutSeconds = 60)
    {
        string? binaryPath = ResolveBinary(toolName);
        if (binaryPath is null)
        {
            return ToolResult.Fail($"Sysinternals tool '{toolName}' was not found. Install the Sysinternals suite and ensure {toolName}64.exe is on the PATH or in a well-known install directory.", operation);
        }

        try
        {
            ProcessStartInfo startInfo = new()
            {
                    FileName = binaryPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
            };

            using Process process = new() { StartInfo = startInfo };
            process.Start();

            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> errorTask = process.StandardError.ReadToEndAsync();

            bool exited = await Task.Run(() => process.WaitForExit(timeoutSeconds * 1000)).ConfigureAwait(false);
            if (!exited)
            {
                process.Kill();
                return ToolResult.Fail($"{toolName} timed out after {timeoutSeconds} seconds.", operation);
            }

            string output = await outputTask.ConfigureAwait(false);
            string error = await errorTask.ConfigureAwait(false);

            if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(output))
            {
                return ToolResult.Fail($"{toolName} exited with code {process.ExitCode}: {error.Trim()}", operation);
            }

            return ToolResult.Ok(output, operation);
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, operation);
        }
    }








    /// <summary>
    ///     Validates a user-supplied argument value before it is interpolated into a
    ///     native command line. Blocks quotes and shell metacharacters to prevent
    ///     argument shaping (spec §5.1).
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">The parameter name for error messages.</param>
    /// <returns>A failure result if validation fails, or <c>null</c> if the value is safe.</returns>
    internal static ToolResult? ValidateArgument(string? value, string paramName)
    {
        ToolResult? requiredResult = InputValidator.ValidateRequired(value, paramName);
        if (requiredResult is not null)
        {
            return requiredResult;
        }

        if (value!.IndexOfAny(['"', '\'', '`', '&', '|', ';', '<', '>', '%', '$', '!', '\r', '\n']) >= 0)
        {
            return ToolResult.Fail($"{paramName} contains characters that are not permitted: {value}. Arguments must not contain quotes or shell metacharacters.", "Sysinternals argument validation");
        }

        return null;
    }








    /// <summary>
    ///     A single Sysinternals binary availability record.
    /// </summary>
    /// <param name="ToolName">The canonical Sysinternals tool name (e.g., AccessChk).</param>
    /// <param name="BinaryName">The binary file name that was probed (e.g., accesschk64.exe).</param>
    /// <param name="Available">Whether the binary was found on this system.</param>
    /// <param name="Path">The full path of the resolved binary, or null when not found.</param>
    /// <param name="FileVersion">The file version of the resolved binary, if available.</param>
    /// <param name="FileDescription">The file description of the resolved binary, if available.</param>
    public sealed record SysinternalsBinaryInfo(string ToolName, string BinaryName, bool Available, string? Path, string? FileVersion, string? FileDescription);
}