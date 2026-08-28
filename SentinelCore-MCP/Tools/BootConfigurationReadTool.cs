// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         BootConfigurationReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;


namespace SentinelCoreMCP.Tools;




/// <summary>
///     Read-only tool for querying system boot configuration using bcdedit.exe.
///     There is no managed .NET API for BCD; shelling bcdedit is the accepted approach.
/// </summary>
[SupportedOSPlatform("windows")]
[McpServerToolType]
public sealed class BootConfigurationReadTool
{

    /// <summary>
    ///     Runs bcdedit.exe with the specified arguments and returns the standard output asynchronously.
    /// </summary>
    /// <param name="arguments">The arguments to pass to bcdedit.exe.</param>
    /// <returns>The standard output of bcdedit.exe.</returns>
    /// <exception cref="TimeoutException">Thrown when bcdedit times out.</exception>
    /// <exception cref="InvalidOperationException">Thrown when bcdedit fails with a non-zero exit code.</exception>
    private static async Task<string> RunBcdeditAsync(string arguments)
    {
        using Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bcdedit.exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();

        bool exited = await Task.Run(() => process.WaitForExit(30_000)).ConfigureAwait(false);
        if (!exited)
        {
            process.Kill();
            throw new TimeoutException($"bcdedit {arguments} timed out after 30 seconds.");
        }

        string output = await outputTask.ConfigureAwait(false);
        string error = await errorTask.ConfigureAwait(false);

        if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(output))
        {
            throw new InvalidOperationException($"bcdedit {arguments} failed with exit code {process.ExitCode}: {error}");
        }

        return string.IsNullOrWhiteSpace(output) ? error : output;
    }

    /// <summary>
    ///     Returns the current boot entry GUID from the BCD store.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the bcdedit output.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Boot_Configuration_Read_Current", ReadOnly = true, Destructive = false)]
    [Description("Returns the current boot entry GUID from the BCD store.")]
    public async Task<ToolResult> BcdeditCurrentAsync()
    {
        try
        {
            string output = await RunBcdeditAsync("/enum {current}").ConfigureAwait(false);
            return ToolResult.Ok(output, "BCD current boot entry read.");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "BCD current entry read");
        }
    }

    /// <summary>
    ///     Enumerates the active boot configuration store entries.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the bcdedit output.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Boot_Configuration_Enum", ReadOnly = true, Destructive = false)]
    [Description("Enumerates the active boot configuration store entries.")]
    public async Task<ToolResult> BcdeditEnumAsync()
    {
        try
        {
            string output = await RunBcdeditAsync("/enum").ConfigureAwait(false);
            return ToolResult.Ok(output, "BCD enumeration complete.");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "BCD enumeration");
        }
    }
}
