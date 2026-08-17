// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         SessionsReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for enumerating active user sessions on the local machine.
///     Equivalent to the `query session` / `qwinsta` command output.
/// </summary>
[McpServerToolType]
public sealed class SessionsReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sessions_List_Active", ReadOnly = true, Destructive = false)]
    [Description("Lists active user sessions (console, RDP, etc.) on the local machine.")]
    public static ToolResult SessionsListActive([Description("Maximum number of sessions to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();
            System.Diagnostics.ProcessStartInfo psi = new()
            {
                FileName = "query",
                Arguments = "session",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return ToolResult.Fail("Unable to start query session.");
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse query session output
            // Format: SESSIONNAME  USERNAME  ID  STATE  TYPE  DEVICE
            string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (results.Count >= maxRecords) break;
                string trimmed = line.Trim();
                // Skip header line
                if (trimmed.StartsWith("SESSIONNAME", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("====", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Parse the fixed-width format
                string[] parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    // The format varies: sometimes session name is blank
                    int idIndex = -1;
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (int.TryParse(parts[i], out _))
                        {
                            idIndex = i;
                            break;
                        }
                    }

                    if (idIndex >= 0)
                    {
                        results.Add(new
                        {
                            SessionName = idIndex > 0 ? parts[0] : "",
                            UserName = idIndex > 1 ? string.Join(" ", parts[1..idIndex]) : "",
                            SessionId = parts[idIndex],
                            State = idIndex + 1 < parts.Length ? parts[idIndex + 1] : "",
                            Type = idIndex + 2 < parts.Length ? parts[idIndex + 2] : ""
                        });
                    }
                }
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Active session listing failed.");
        }
    }
}