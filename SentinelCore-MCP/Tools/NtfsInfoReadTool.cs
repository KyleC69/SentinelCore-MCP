// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         NtfsInfoReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




namespace SentinelCoreMCP.Tools;




/// <summary>
///     Read-only tool for reporting NTFS volume information (cluster size, MFT
///     records, journal state) using the Sysinternals NTFSInfo utility.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class NtfsInfoReadTool
{




    /// <summary>
    ///     Probes whether Sysinternals NTFSInfo is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_NtfsInfo_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals NTFSInfo utility is installed and reports its version and path.")]
    public async Task<ToolResult> NtfsInfoAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("ntfsinfo")).ConfigureAwait(false);
    }






    /// <summary>
    ///     Reports NTFS volume geometry and metadata for the specified drive letter.
    /// </summary>
    /// <param name="driveLetter">The drive letter to inspect, e.g. C. Defaults to the system drive.</param>
    /// <param name="maxLines">Maximum number of output lines to return. Defaults to 200.</param>
    /// <returns>A <see cref="ToolResult" /> containing the NTFS volume report.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_NtfsInfo_Read_Volume", ReadOnly = true, Destructive = false)]
    [Description("Reports NTFS volume geometry, MFR size, and metadata file layout using Sysinternals NTFSInfo. Requires NTFSInfo to be installed.")]
    public async Task<ToolResult> NtfsInfoReadVolumeAsync(
        [Description("The drive letter to inspect, e.g. C. Defaults to the system drive.")] string? driveLetter = null,
        [Description("Maximum number of output lines to return. Defaults to 200.")] int maxLines = 200)
    {
        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        string drive = string.IsNullOrWhiteSpace(driveLetter)
            ? Environment.SystemDirectory[..2]
            : driveLetter.TrimEnd(':').ToUpperInvariant();

        if (drive.Length != 1 || drive[0] is < 'A' or > 'Z')
        {
            return ToolResult.Fail($"Invalid drive letter: {drive}. A single letter A-Z is required.", "NTFSInfo volume read");
        }

        // -accepteula suppresses the EULA prompt; the drive letter is the sole operand.
        ToolResult result = await SysinternalsHelper.RunAsync("ntfsinfo", $"{drive}: -accepteula", "NTFSInfo volume read").ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "NTFSInfo volume read complete.");
    }
}
