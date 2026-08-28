// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         DefenderReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="DefenderReadTool" /> covering registry config reads
///     and WMI-based status queries.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class DefenderReadToolTests
{
    private readonly DefenderReadTool _tool = new();

    #region Defender_Read_Registry_Config tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DefenderReadRegistryConfig_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.DefenderReadRegistryConfigAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DefenderReadRegistryConfig_ContainsDefenderKey()
    {
        ToolResult result = await _tool.DefenderReadRegistryConfigAsync();

        Assert.True(result.Success);
        // Should contain at least one Defender registry value
        Assert.NotEmpty(((string)result.Results!).Trim());
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DefenderReadRegistryConfig_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.DefenderReadRegistryConfigAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Defender_Read_Status tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DefenderReadStatus_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.DefenderReadStatusAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DefenderReadStatus_ReturnsListContent()
    {
        ToolResult result = await _tool.DefenderReadStatusAsync();

        Assert.True(result.Success);
        // Results is a List<object> of Defender status records
        Assert.IsType<List<object>>(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DefenderReadStatus_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.DefenderReadStatusAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
