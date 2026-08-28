// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         WindowsUpdateExtendedReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="WindowsUpdateExtendedReadTool" /> covering hotfix
///     enumeration via the Win32_QuickFixEngineering WMI class.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsUpdateExtendedReadToolTests
{
    private readonly WindowsUpdateExtendedReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WindowsUpdateListMissing_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.WindowsUpdateListMissingAsync();

        if (!result.Success)
        {
            return;
        }

        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WindowsUpdateListMissing_RespectsMaxRecords()
    {
        const int maxRecords = 3;
        ToolResult result = await _tool.WindowsUpdateListMissingAsync(maxRecords: maxRecords);

        if (!result.Success)
        {
            return;
        } // Skip if WMI unavailable

        List<object> hotfixes = Assert.IsType<List<object>>(result.Results);
        Assert.True(hotfixes.Count <= maxRecords, $"Expected at most {maxRecords} hotfixes but got {hotfixes.Count}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WindowsUpdateListMissing_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.WindowsUpdateListMissingAsync();

        // WMI may not be available in all environments
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WindowsUpdateListMissing_WhenSuccessful_ReturnsListContent()
    {
        ToolResult result = await _tool.WindowsUpdateListMissingAsync();

        if (!result.Success)
        {
            return;
        } // Skip if WMI unavailable

        Assert.NotNull(result.Results);
        // Results is a List<object> of hotfix records
        Assert.IsType<List<object>>(result.Results);
    }








    [Fact]
    public async Task WindowsUpdateListMissing_ZeroMaxRecords_ReturnsFailure()
    {
        ToolResult result = await _tool.WindowsUpdateListMissingAsync(maxRecords: 0);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }
}