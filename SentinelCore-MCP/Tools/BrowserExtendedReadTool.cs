// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         BrowserExtendedReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tools for querying browser extensions
///     from Chrome and Edge local profiles.
/// </summary>
[SupportedOSPlatform("windows")]
[McpServerToolType]
public sealed class BrowserExtendedReadTool
{








    private string GetChromeProfilePath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data");
    }








    private string GetEdgeProfilePath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data");
    }








    [McpServerTool(Name = "Browser_List_Extensions", ReadOnly = true, Destructive = false)]
    [Description("Lists installed browser extensions for Chrome and Edge from their local profile directories.")]
    public async Task<ToolResult> BrowserListExtensionsAsync([Description("The browser to query: Chrome, Edge, or All. Defaults to All.")] string browser = "All", [Description("Maximum number of extensions to return per browser. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxRecords);
            if (maxValidation is not null)
            {
                return maxValidation;
            }

            List<object> results = new();

            if (browser.Equals("Chrome", StringComparison.OrdinalIgnoreCase) || browser.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                string chromePath = GetChromeProfilePath();
                if (Directory.Exists(chromePath))
                {
                    string extensionsPath = Path.Combine(chromePath, "Default", "Extensions");
                    if (Directory.Exists(extensionsPath))
                    {
                        foreach (string extDir in Directory.GetDirectories(extensionsPath))
                        {
                            if (results.Count >= maxRecords) break;
                            string extId = Path.GetFileName(extDir);
                            string[] versionDirs = Directory.GetDirectories(extDir);
                            string version = versionDirs.Length > 0 ? Path.GetFileName(versionDirs[0]) : "unknown";

                            results.Add(new
                            {
                                Browser = "Chrome",
                                ExtensionId = extId,
                                Version = version,
                                Path = extDir
                            });
                        }
                    }
                }
            }

            if (browser.Equals("Edge", StringComparison.OrdinalIgnoreCase) || browser.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                string edgePath = GetEdgeProfilePath();
                if (Directory.Exists(edgePath))
                {
                    string extensionsPath = Path.Combine(edgePath, "Default", "Extensions");
                    if (Directory.Exists(extensionsPath))
                    {
                        foreach (string extDir in Directory.GetDirectories(extensionsPath))
                        {
                            if (results.Count >= maxRecords) break;
                            string extId = Path.GetFileName(extDir);
                            string[] versionDirs = Directory.GetDirectories(extDir);
                            string version = versionDirs.Length > 0 ? Path.GetFileName(versionDirs[0]) : "unknown";

                            results.Add(new
                            {
                                Browser = "Edge",
                                ExtensionId = extId,
                                Version = version,
                                Path = extDir
                            });
                        }
                    }
                }
            }

            return ToolResult.Ok(results, "BrowserExtendedReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "Browser extension listing");
        }
    }
}









