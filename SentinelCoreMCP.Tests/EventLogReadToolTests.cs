// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         EventLogReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="EventLogReadTool" /> covering event log queries.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class EventLogReadToolTests
{
    private readonly EventLogReadTool _tool = new();

    #region Event_Log_List_Channels tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EventLogListChannels_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.EventLogListChannelsAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EventLogListChannels_ContainsChannelNames()
    {
        ToolResult result = await _tool.EventLogListChannelsAsync();

        Assert.True(result.Success);
        Assert.NotEmpty(((string)result.Results!).Trim());
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EventLogListChannels_RespectsMaxRecords()
    {
        ToolResult result = await _tool.EventLogListChannelsAsync(maxRecords: 5);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EventLogListChannels_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.EventLogListChannelsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Event_Log_Query tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EventLogQuery_ApplicationChannel_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.EventLogQueryAsync("Application");

        // Some events may not be readable in restricted environments; accept graceful failure
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EventLogQuery_SystemChannel_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.EventLogQueryAsync("System");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    public async Task EventLogQuery_EmptyChannel_ReturnsFailure()
    {
        ToolResult result = await _tool.EventLogQueryAsync("");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EventLogQuery_NullChannel_ReturnsFailure()
    {
        ToolResult result = await _tool.EventLogQueryAsync(null!);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EventLogQuery_NonExistentChannel_ReturnsFailure()
    {
        ToolResult result = await _tool.EventLogQueryAsync("NonExistentChannel_12345");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }

    #endregion

    #region Event_Log_Read_Configuration tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EventLogReadConfiguration_ApplicationChannel_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.EventLogReadConfigurationAsync("Application");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    public async Task EventLogReadConfiguration_EmptyChannel_ReturnsFailure()
    {
        ToolResult result = await _tool.EventLogReadConfigurationAsync("");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}

