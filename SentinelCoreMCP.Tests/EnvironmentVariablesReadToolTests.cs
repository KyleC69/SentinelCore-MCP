// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         EnvironmentVariablesReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="EnvironmentVariablesReadTool" /> covering environment variable reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class EnvironmentVariablesReadToolTests
{
    private readonly EnvironmentVariablesReadTool _tool = new();

    #region Environment_Variables_List tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EnvironmentList_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.EnvironmentListAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EnvironmentList_ContainsPathSeparator()
    {
        ToolResult result = await _tool.EnvironmentListAsync();

        Assert.True(result.Success);
        Assert.Contains(";", (string)result.Results!);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EnvironmentList_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.EnvironmentListAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Environment_Read_Value tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EnvironmentReadValue_ProcessPath_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.EnvironmentReadValueAsync("PATH");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
        Assert.Contains(";", (string)result.Results!);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EnvironmentReadValue_NonExistentVariable_ReturnsFailure()
    {
        ToolResult result = await _tool.EnvironmentReadValueAsync("__NonExistentVariable_12345__");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }

    [Fact]
    public async Task EnvironmentReadValue_NullVariableName_ReturnsFailure()
    {
        ToolResult result = await _tool.EnvironmentReadValueAsync(null!);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}

