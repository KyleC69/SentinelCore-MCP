// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         AppLockerReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="AppLockerReadTool" /> covering AppLocker policy queries
///     via the PowerShell SDK.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class AppLockerReadToolTests
{
    private readonly AppLockerReadTool _tool = new();

    #region AppLocker_Get_Effective_Policy tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    [Trait("Category", "RequiresPowerShell")]
    public async Task ApplockerGetEffectivePolicy_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.ApplockerGetEffectivePolicyAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    [Trait("Category", "RequiresPowerShell")]
    public async Task ApplockerGetEffectivePolicy_ReturnsXmlContent()
    {
        ToolResult result = await _tool.ApplockerGetEffectivePolicyAsync();

        Assert.True(result.Success);
        // AppLocker policy output should contain XML tags
        string output = (string)result.Results!;
        Assert.True(
            output.Contains("<") || output.Contains("Objects", StringComparison.OrdinalIgnoreCase),
            "Expected XML or object content in AppLocker policy output");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    [Trait("Category", "RequiresPowerShell")]
    public async Task ApplockerGetEffectivePolicy_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.ApplockerGetEffectivePolicyAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region AppLocker_List_Rule_Collections tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    [Trait("Category", "RequiresPowerShell")]
    public async Task ApplockerListRuleCollections_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.ApplockerListRuleCollectionsAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    [Trait("Category", "RequiresPowerShell")]
    public async Task ApplockerListRuleCollections_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.ApplockerListRuleCollectionsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
