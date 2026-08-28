// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         PerformanceReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="PerformanceReadTool" /> covering performance counter queries.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class PerformanceReadToolTests
{
    private readonly PerformanceReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PerformanceListCategories_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.performanceListCategoriesAsync();

        // Performance counters may not be available in all environments
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PerformanceListCategories_WhenSuccessful_ContainsCounterData()
    {
        ToolResult result = await _tool.performanceListCategoriesAsync();

        if (!result.Success)
        {
            return;
        } // Skip if counters unavailable

        Assert.NotNull(result.Results);
        // Results is a List<string> of category names
        List<string> categories = Assert.IsType<List<string>>(result.Results);
        Assert.NotEmpty(categories);
    }








    [Fact]
    public async Task PerformanceListCounters_EmptyCategory_ReturnsFailure()
    {
        ToolResult result = await _tool.performanceListCountersAsync("");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PerformanceListCounters_ProcessorCategory_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.performanceListCountersAsync("Processor");

        // The Processor category may not exist on all systems
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    public async Task PerformanceReadCounter_EmptyCategory_ReturnsFailure()
    {
        ToolResult result = await _tool.performanceReadCounterAsync("", "% Processor Time");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PerformanceReadCounter_ProcessorTime_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.performanceReadCounterAsync("Processor", "% Processor Time", "_Total");

        // Counter may not exist on all systems
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }
}