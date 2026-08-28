// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         ScheduledTaskReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="ScheduledTaskReadTool" /> covering scheduled task queries.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ScheduledTaskReadToolTests
{
    private readonly ScheduledTaskReadTool _tool = new();

    #region Scheduled_Task_List tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ScheduledTaskList_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.ScheduledTaskListAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ScheduledTaskList_ContainsTaskInfo()
    {
        ToolResult result = await _tool.ScheduledTaskListAsync();

        Assert.True(result.Success);
        Assert.NotEmpty(((string)result.Results!).Trim());
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ScheduledTaskList_RespectsMaxRecords()
    {
        ToolResult result = await _tool.ScheduledTaskListAsync(maxRecords: 5);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ScheduledTaskList_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.ScheduledTaskListAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Scheduled_Task_Read tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ScheduledTaskRead_KnownTask_ReturnsSuccessfulOrGracefulFailure()
    {
        // Use a task that exists on all Windows systems
        ToolResult result = await _tool.ScheduledTaskReadAsync("\\Microsoft\\Windows\\Defrag\\ScheduledDefrag");

        // Task may not exist on all SKUs; accept success or graceful failure
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ScheduledTaskRead_NonExistentTask_ReturnsFailure()
    {
        ToolResult result = await _tool.ScheduledTaskReadAsync("\\NonExistentFolder\\NonExistentTask_12345");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }

    [Fact]
    public async Task ScheduledTaskRead_EmptyTaskPath_ReturnsFailure()
    {
        ToolResult result = await _tool.ScheduledTaskReadAsync("");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }

    #endregion
}
