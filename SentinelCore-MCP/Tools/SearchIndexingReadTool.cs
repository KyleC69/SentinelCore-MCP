// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         SearchIndexingReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Runtime.Versioning;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Windows Search indexing options.
/// </summary>
[McpServerToolType]
public sealed class SearchIndexingReadTool
{

    private const string CrawlScopeKey = "SOFTWARE\\Microsoft\\Windows Search\\CrawlScopeManager";
    private const string WindowsSearchKey = "SOFTWARE\\Microsoft\\Windows Search";








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Search_Indexing_List_Scopes", ReadOnly = true, Destructive = false)]
    [Description("Lists indexed locations from the Windows Search crawl scope registry.")]
    public async Task<ToolResult> searchIndexingListScopesAsync()
    {
        try
        {
            StringBuilder sb = new();
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(CrawlScopeKey, false);
            if (key is null)
            {
                return ToolResult.Fail($"Crawl scope registry key not found: {CrawlScopeKey}", "SearchIndexingReadTool");
            }

            sb.AppendLine($"[{CrawlScopeKey}]");
            foreach (string subKeyName in key.GetSubKeyNames()) sb.AppendLine($"  {subKeyName}");

            foreach (string valueName in key.GetValueNames()) sb.AppendLine($"  {valueName}={key.GetValue(valueName)}");

            return ToolResult.Ok(sb.ToString(), "SearchIndexingReadTool");
        }
        catch
        {
            return ToolResult.Fail("Search indexing scope listing failed.", "SearchIndexingReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Search_Indexing_Read_Settings", ReadOnly = true, Destructive = false)]
    [Description("Reads Windows Search service configuration from the registry.")]
    public async Task<ToolResult> searchIndexingReadSettingsAsync()
    {
        try
        {
            StringBuilder sb = new();
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(WindowsSearchKey, false);
            if (key is null)
            {
                return ToolResult.Fail($"Windows Search registry key not found: {WindowsSearchKey}", "SearchIndexingReadTool");
            }

            sb.AppendLine($"[{WindowsSearchKey}]");
            foreach (string valueName in key.GetValueNames()) sb.AppendLine($"  {valueName}={key.GetValue(valueName)}");

            return ToolResult.Ok(sb.ToString(), "SearchIndexingReadTool");
        }
        catch
        {
            return ToolResult.Fail("Search indexing settings read failed.", "SearchIndexingReadTool");
        }
    }
}
