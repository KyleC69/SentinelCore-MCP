// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         SearchIndexingReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="SearchIndexingReadTool" /> covering Windows Search
///     crawl scope and service configuration reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SearchIndexingReadToolTests
{
    private readonly SearchIndexingReadTool _tool = new();

    #region Search_Indexing_List_Scopes tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SearchIndexingListScopes_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.searchIndexingListScopesAsync();

        // The Windows Search service may not be installed on all systems
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SearchIndexingListScopes_WhenSuccessful_ContainsCrawlScopeSection()
    {
        ToolResult result = await _tool.searchIndexingListScopesAsync();

        if (!result.Success) { return; } // Skip if Windows Search not installed

        string output = (string)result.Results!;
        Assert.Contains("CrawlScopeManager", output);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SearchIndexingListScopes_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.searchIndexingListScopesAsync();

        if (!result.Success) { return; }

        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Search_Indexing_Read_Settings tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SearchIndexingReadSettings_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.searchIndexingReadSettingsAsync();

        // The Windows Search key may not exist on all systems
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SearchIndexingReadSettings_WhenSuccessful_ContainsSearchKeySection()
    {
        ToolResult result = await _tool.searchIndexingReadSettingsAsync();

        if (!result.Success) { return; } // Skip if Windows Search not installed

        string output = (string)result.Results!;
        Assert.Contains("Windows Search", output);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SearchIndexingReadSettings_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.searchIndexingReadSettingsAsync();

        if (!result.Success) { return; }

        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
