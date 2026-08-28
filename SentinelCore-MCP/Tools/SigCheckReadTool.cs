// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         SigCheckReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;




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
    ///     Probes whether Sysinternals SigCheck is installed and reports its version.
    /// </summary>
    /// <returns>A <see cref="ToolResult" /> containing the availability record.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sysinternals_SigCheck_Availability", ReadOnly = true, Destructive = false)]
    [Description("Checks whether the Sysinternals SigCheck utility is installed and reports its version and path.")]
    public async Task<ToolResult> SigCheckAvailabilityAsync()
    {
        return await Task.Run(() => SysinternalsHelper.ProbeAvailability("sigcheck")).ConfigureAwait(false);
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
    public async Task<ToolResult> SigCheckVerifyFileAsync(
        [Description("The absolute path of the file to verify.")] string filePath,
        [Description("Maximum number of output lines to return. Defaults to 100.")] int maxLines = 100)
    {
        ToolResult? pathValidation = SysinternalsHelper.ValidateArgument(filePath, "filePath");
        if (pathValidation is not null)
        {
            return pathValidation;
        }

        if (!File.Exists(filePath!))
        {
            return ToolResult.Fail($"File not found: {filePath}", "SigCheck verification");
        }

        ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxLines, "maxLines");
        if (maxValidation is not null)
        {
            return maxValidation;
        }

        // -a shows extended version info; -accepteula suppresses the EULA prompt.
        ToolResult result = await SysinternalsHelper.RunAsync(
            "sigcheck",
            $"-a -accepteula \"{filePath}\"",
            "SigCheck verification",
            timeoutSeconds: 120).ConfigureAwait(false);
        if (!result.Success)
        {
            return result;
        }

        return ToolResult.Ok(SysinternalsHelper.LimitLines((string)result.Results!, maxLines), "SigCheck verification complete.");
    }
}
