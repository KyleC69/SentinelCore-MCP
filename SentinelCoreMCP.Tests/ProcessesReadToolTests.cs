// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         ProcessesReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="ProcessesReadTool" /> covering process listing and reading.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ProcessesReadToolTests
{
    private readonly ProcessesReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcessList_DefaultParameters_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.ProcessListAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcessList_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.ProcessListAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcessList_RespectsMaxRecords()
    {
        const int maxRecords = 5;
        ToolResult result = await _tool.ProcessListAsync(maxRecords: maxRecords);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        List<object> processes = Assert.IsType<List<object>>(result.Results);
        Assert.True(processes.Count <= maxRecords, $"Expected at most {maxRecords} processes but got {processes.Count}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcessList_ReturnsNonEmptyList()
    {
        ToolResult result = await _tool.ProcessListAsync();

        Assert.True(result.Success);
        List<object> processes = Assert.IsType<List<object>>(result.Results);
        Assert.NotEmpty(processes);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcessList_WithNameFilter_ReturnsFilteredResults()
    {
        ToolResult result = await _tool.ProcessListAsync(nameFilter: "explorer");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcessRead_CurrentProcess_ReturnsMatchingPid()
    {
        int currentPid = Environment.ProcessId;
        ToolResult result = await _tool.ProcessReadAsync(currentPid);

        Assert.True(result.Success);
        Assert.NotNull(result.Results);
        // The anonymous object should expose an Id property equal to the requested PID
        object processInfo = result.Results!;
        System.Reflection.PropertyInfo? idProperty = processInfo.GetType().GetProperty("Id");
        Assert.NotNull(idProperty);
        Assert.Equal(currentPid, (int)idProperty.GetValue(processInfo)!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcessRead_CurrentProcess_ReturnsSuccessfulToolResult()
    {
        int currentPid = Environment.ProcessId;
        ToolResult result = await _tool.ProcessReadAsync(currentPid);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcessRead_NonExistentPid_ReturnsFailure()
    {
        // Use a very high PID that is unlikely to exist
        ToolResult result = await _tool.ProcessReadAsync(999999);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }
}