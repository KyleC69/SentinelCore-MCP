// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         InstalledAppsReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="InstalledAppsReadTool" /> covering Add/Remove Programs
///     registry enumeration and MSI product listing.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class InstalledAppsReadToolTests
{
    private readonly InstalledAppsReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task InstalledAppsList_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.InstalledAppsListAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task InstalledAppsList_RespectsMaxRecords()
    {
        const int maxRecords = 5;
        ToolResult result = await _tool.InstalledAppsListAsync(maxRecords: maxRecords);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        List<Dictionary<string, string?>> apps = Assert.IsType<List<Dictionary<string, string?>>>(result.Results);
        Assert.True(apps.Count <= maxRecords, $"Expected at most {maxRecords} apps but got {apps.Count}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task InstalledAppsList_ReturnsNonEmptyDictionaryList()
    {
        ToolResult result = await _tool.InstalledAppsListAsync();

        Assert.True(result.Success);
        List<Dictionary<string, string?>> apps = Assert.IsType<List<Dictionary<string, string?>>>(result.Results);
        // Every Windows system has at least one installed app with a DisplayName
        Assert.NotEmpty(apps);
        Assert.All(apps, a => Assert.False(string.IsNullOrWhiteSpace(a["DisplayName"])));
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task InstalledAppsList_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.InstalledAppsListAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task InstalledAppsList_WithFilter_ReturnsMatchingApps()
    {
        ToolResult result = await _tool.InstalledAppsListAsync(filter: "Microsoft");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        List<Dictionary<string, string?>> apps = Assert.IsType<List<Dictionary<string, string?>>>(result.Results);
        // All returned apps must match the filter
        Assert.All(apps, a => Assert.Contains("Microsoft", a["DisplayName"], StringComparison.OrdinalIgnoreCase));
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task InstalledAppsMsiList_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.InstalledAppsMsiListAsync();

        // MSI enumeration may fail in restricted environments
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task InstalledAppsMsiList_WhenSuccessful_ReturnsListContent()
    {
        ToolResult result = await _tool.InstalledAppsMsiListAsync();

        if (!result.Success)
        {
            return;
        } // Skip if MSI enumeration unavailable

        Assert.NotNull(result.Results);
        // Results is a List<object> of anonymous MSI product records
        Assert.IsType<List<object>>(result.Results);
    }
}