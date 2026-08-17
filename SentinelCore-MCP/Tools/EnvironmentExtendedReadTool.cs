// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         EnvironmentExtendedReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for analyzing the system PATH environment variable
///     for hijack detection and misconfiguration.
/// </summary>
[McpServerToolType]
public sealed class EnvironmentExtendedReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Environment_Read_Path", ReadOnly = true, Destructive = false)]
    [Description("Reads and analyzes the system and user PATH environment variables for hijack detection.")]
    public static ToolResult EnvironmentReadPath()
    {
        try
        {
            List<object> results = new();
            string[] systemPathEntries = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine)?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? [];
            string[] userPathEntries = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? [];
            string[] processPathEntries = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process)?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? [];

            foreach (string entry in systemPathEntries)
            {
                bool exists = Directory.Exists(entry);
                bool isWritable = false;
                try
                {
                    string testFile = Path.Combine(entry, $"_sentinel_test_{Guid.NewGuid():N}");
                    File.Create(testFile).Close();
                    File.Delete(testFile);
                    isWritable = true;
                }
                catch
                {
                    // Not writable
                }

                results.Add(new
                {
                    Path = entry,
                    Source = "System",
                    Exists = exists,
                    IsWritable = isWritable
                });
            }

            foreach (string entry in userPathEntries)
            {
                bool exists = Directory.Exists(entry);
                bool isWritable = false;
                try
                {
                    string testFile = Path.Combine(entry, $"_sentinel_test_{Guid.NewGuid():N}");
                    File.Create(testFile).Close();
                    File.Delete(testFile);
                    isWritable = true;
                }
                catch
                {
                    // Not writable
                }

                results.Add(new
                {
                    Path = entry,
                    Source = "User",
                    Exists = exists,
                    IsWritable = isWritable
                });
            }

            // Check for duplicates
            var duplicates = processPathEntries
                .GroupBy(p => p, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            string json = JsonSerializer.Serialize(new
            {
                Entries = results,
                DuplicatePaths = duplicates,
                TotalSystemPaths = systemPathEntries.Length,
                TotalUserPaths = userPathEntries.Length
            }, new JsonSerializerOptions { WriteIndented = true });

            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("PATH analysis failed.");
        }
    }
}
