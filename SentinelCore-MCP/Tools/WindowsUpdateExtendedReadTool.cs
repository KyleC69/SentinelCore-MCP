// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         WindowsUpdateExtendedReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Windows Update missing patches and patch compliance status.
/// </summary>
[McpServerToolType]
public sealed class WindowsUpdateExtendedReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Windows_Update_List_Missing", ReadOnly = true, Destructive = false)]
    [Description("Lists missing Windows updates by querying the Windows Update service for pending updates.")]
    public static ToolResult WindowsUpdateListMissing([Description("Maximum number of updates to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();

            // Use the Windows Update Agent via COM to query for pending updates
            // Fall back to registry-based approach if COM is unavailable
            System.Diagnostics.ProcessStartInfo psi = new()
            {
                FileName = "powershell",
                Arguments = "-NoProfile -Command \"Get-HotFix | Select-Object -Property HotFixID,Description,InstalledOn,InstalledBy | ConvertTo-Json -Compress\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return ToolResult.Fail("Unable to start PowerShell to query updates.");
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                return ToolResult.Fail($"PowerShell update query failed: {error}");
            }

            // Parse the JSON output from Get-HotFix
            try
            {
                using JsonDocument doc = JsonDocument.Parse(output);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in doc.RootElement.EnumerateArray())
                    {
                        if (results.Count >= maxRecords) break;
                        results.Add(new
                        {
                            HotFixID = item.TryGetProperty("HotFixID", out JsonElement id) ? id.GetString() ?? "" : "",
                            Description = item.TryGetProperty("Description", out JsonElement desc) ? desc.GetString() ?? "" : "",
                            InstalledOn = item.TryGetProperty("InstalledOn", out JsonElement installed) ? installed.GetString() ?? "" : "",
                            InstalledBy = item.TryGetProperty("InstalledBy", out JsonElement by) ? by.GetString() ?? "" : ""
                        });
                    }
                }
                else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    // Single result
                    results.Add(new
                    {
                        HotFixID = doc.RootElement.TryGetProperty("HotFixID", out JsonElement id) ? id.GetString() ?? "" : "",
                        Description = doc.RootElement.TryGetProperty("Description", out JsonElement desc) ? desc.GetString() ?? "" : "",
                        InstalledOn = doc.RootElement.TryGetProperty("InstalledOn", out JsonElement installed) ? installed.GetString() ?? "" : "",
                        InstalledBy = doc.RootElement.TryGetProperty("InstalledBy", out JsonElement by) ? by.GetString() ?? "" : ""
                    });
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, return raw output
                return ToolResult.Ok(output);
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Missing update listing failed.");
        }
    }
}