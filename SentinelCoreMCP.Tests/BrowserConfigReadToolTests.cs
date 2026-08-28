// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         BrowserConfigReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="BrowserConfigReadTool" /> covering browser policy,
///     default browser, and IE/Edge proxy settings reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class BrowserConfigReadToolTests
{
    private readonly BrowserConfigReadTool _tool = new();

    #region Browser_Config_Read_Chrome_Policies tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserReadChromePolicies_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.BrowserReadChromePoliciesAsync();

        // Succeeds even when no Chrome policies exist (empty output)
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserReadChromePolicies_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.BrowserReadChromePoliciesAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Browser_Config_Read_Default tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserReadDefault_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.BrowserReadDefaultAsync();

        // The UserChoice key exists on all Windows systems with a default browser
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserReadDefault_ContainsProgId()
    {
        ToolResult result = await _tool.BrowserReadDefaultAsync();

        Assert.True(result.Success);
        Assert.Contains("DefaultBrowserProgId=", (string)result.Results!);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserReadDefault_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.BrowserReadDefaultAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Browser_Config_Read_IE_Settings tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserReadIeSettings_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.BrowserReadIeSettingsAsync();

        // The IE Internet Settings key exists on all Windows systems
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BrowserReadIeSettings_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.BrowserReadIeSettingsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
