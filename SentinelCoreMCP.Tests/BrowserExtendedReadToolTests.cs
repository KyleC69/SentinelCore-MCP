// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         BrowserExtendedReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="BrowserExtendedReadTool" /> covering browser extension
///     enumeration from Chrome and Edge profile directories.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class BrowserExtendedReadToolTests
{
    private readonly BrowserExtendedReadTool _tool = new();

    #region Browser_List_Extensions tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserListExtensions_DefaultParameters_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.BrowserListExtensionsAsync();

        // Succeeds even when no browsers are installed (empty list)
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserListExtensions_WhenSuccessful_ReturnsListContent()
    {
        ToolResult result = await _tool.BrowserListExtensionsAsync();

        Assert.True(result.Success);
        // Results is a List<object> of extension records
        Assert.IsType<List<object>>(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserListExtensions_ChromeOnly_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.BrowserListExtensionsAsync(browser: "Chrome");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserListExtensions_EdgeOnly_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.BrowserListExtensionsAsync(browser: "Edge");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserListExtensions_RespectsMaxRecords()
    {
        const int maxRecords = 3;
        ToolResult result = await _tool.BrowserListExtensionsAsync(maxRecords: maxRecords);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        List<object> extensions = Assert.IsType<List<object>>(result.Results);
        Assert.True(extensions.Count <= maxRecords,
            $"Expected at most {maxRecords} extensions but got {extensions.Count}");
    }

    [Fact]
    public async Task BrowserListExtensions_ZeroMaxRecords_ReturnsFailure()
    {
        ToolResult result = await _tool.BrowserListExtensionsAsync(maxRecords: 0);

        Assert.False(result.Success);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserListExtensions_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.BrowserListExtensionsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
