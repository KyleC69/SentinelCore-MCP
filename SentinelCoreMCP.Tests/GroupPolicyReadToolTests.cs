// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         GroupPolicyReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="GroupPolicyReadTool" /> covering local group policy
///     key listing and value reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class GroupPolicyReadToolTests
{
    private readonly GroupPolicyReadTool _tool = new();

    #region Group_Policy_List tests

    [Fact]
    public async Task GroupPolicyList_EmptyKeyPath_ReturnsFailure()
    {
        ToolResult result = await _tool.GroupPolicyListAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GroupPolicyList_NullKeyPath_ReturnsFailure()
    {
        ToolResult result = await _tool.GroupPolicyListAsync(null!);

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task GroupPolicyList_NonExistentPath_ReturnsFailure()
    {
        ToolResult result = await _tool.GroupPolicyListAsync("NonExistentPolicyPath_12345");

        // No policy keys under a non-existent path is a graceful failure
        Assert.False(result.Success);
        Assert.Contains("No group policy keys found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task GroupPolicyList_KnownPolicyPath_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.GroupPolicyListAsync("Microsoft\\Windows");

        // The path may or may not have values depending on system configuration
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region Group_Policy_Read_Value tests

    [Fact]
    public async Task GroupPolicyReadValue_EmptyKeyPath_ReturnsFailure()
    {
        ToolResult result = await _tool.GroupPolicyReadValueAsync("", "SomeValue");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GroupPolicyReadValue_EmptyValueName_ReturnsFailure()
    {
        ToolResult result = await _tool.GroupPolicyReadValueAsync("Microsoft\\Windows", "");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task GroupPolicyReadValue_NonExistentValue_ReturnsFailure()
    {
        ToolResult result = await _tool.GroupPolicyReadValueAsync("Microsoft\\Windows", "NonExistentValue_12345");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }

    #endregion
}
