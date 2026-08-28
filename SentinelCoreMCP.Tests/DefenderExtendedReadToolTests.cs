// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         DefenderExtendedReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="DefenderExtendedReadTool" /> covering SmartScreen and
///     reputation-based protection settings reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class DefenderExtendedReadToolTests
{
    private readonly DefenderExtendedReadTool _tool = new();

    #region Defender_Read_SmartScreen tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DefenderReadSmartScreen_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.DefenderReadSmartScreenAsync();

        // Succeeds even when SmartScreen keys are absent (empty output)
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DefenderReadSmartScreen_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.DefenderReadSmartScreenAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
