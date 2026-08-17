// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         GroupPolicyExtendedReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Group Policy Resultant Set of Policy (RSOP)
///     for effective policy analysis.
/// </summary>
[McpServerToolType]
public sealed class GroupPolicyExtendedReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Group_Policy_Read_RSOP", ReadOnly = true, Destructive = false)]
    [Description("Reads the Resultant Set of Policy (RSOP) for the current user and computer.")]
    public static ToolResult GroupPolicyReadRsop([Description("Maximum number of policy entries to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            System.Diagnostics.ProcessStartInfo psi = new()
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -Command \"gpresult /Scope Computer /V | Select-Object -First {maxRecords}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return ToolResult.Fail("Unable to start gpresult.");
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                // gpresult may return non-zero but still have useful output
                if (string.IsNullOrWhiteSpace(output))
                {
                    return ToolResult.Fail($"gpresult failed: {error}");
                }
            }

            return ToolResult.Ok(output);
        }
        catch
        {
            return ToolResult.Fail("RSOP read failed.");
        }
    }
}