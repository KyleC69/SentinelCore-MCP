// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         HyperVExtendedReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="HyperVExtendedReadTool" /> covering Hyper-V checkpoint
///     enumeration for VM rollback detection.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class HyperVExtendedReadToolTests
{
    private readonly HyperVExtendedReadTool _tool = new();

    #region HyperV_List_Checkpoints tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task HyperVListCheckpoints_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.HyperVListCheckpointsAsync();

        // Hyper-V is typically not installed; the tool should fail gracefully
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task HyperVListCheckpoints_WhenSuccessful_ReturnsListContent()
    {
        ToolResult result = await _tool.HyperVListCheckpointsAsync();

        if (!result.Success) { return; } // Skip if Hyper-V not installed

        Assert.NotNull(result.Results);
        // Results is a List<object> of checkpoint records
        Assert.IsType<List<object>>(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task HyperVListCheckpoints_RespectsMaxRecords()
    {
        const int maxRecords = 3;
        ToolResult result = await _tool.HyperVListCheckpointsAsync(maxRecords: maxRecords);

        if (!result.Success) { return; } // Skip if Hyper-V not installed

        List<object> checkpoints = Assert.IsType<List<object>>(result.Results);
        Assert.True(checkpoints.Count <= maxRecords,
            $"Expected at most {maxRecords} checkpoints but got {checkpoints.Count}");
    }

    #endregion
}
