// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         EnvironmentExtendedReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Collections;
using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="EnvironmentExtendedReadTool" /> covering PATH analysis
///     for hijack detection.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class EnvironmentExtendedReadToolTests
{
    private readonly EnvironmentExtendedReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EnvironmentReadPath_EntriesHaveSourceLabels()
    {
        ToolResult result = await _tool.EnvironmentReadPathAsync();

        Assert.True(result.Success);
        object payload = result.Results!;
        IEnumerable entries = (System.Collections.IEnumerable)payload.GetType().GetProperty("Entries")!.GetValue(payload)!;
        bool hasEntries = false;
        foreach (object entry in entries)
        {
            hasEntries = true;
            Assert.NotNull(entry.GetType().GetProperty("Path"));
            Assert.NotNull(entry.GetType().GetProperty("Source"));
            Assert.NotNull(entry.GetType().GetProperty("Exists"));
            Assert.NotNull(entry.GetType().GetProperty("IsWritable"));
        }

        // A typical Windows system has at least one PATH entry
        Assert.True(hasEntries, "Expected at least one PATH entry");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EnvironmentReadPath_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.EnvironmentReadPathAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EnvironmentReadPath_ReturnsAnalysisPayload()
    {
        ToolResult result = await _tool.EnvironmentReadPathAsync();

        Assert.True(result.Success);
        // Results is an anonymous object with Entries, DuplicatePaths, TotalSystemPaths, TotalUserPaths
        object payload = result.Results!;
        Assert.NotNull(payload.GetType().GetProperty("Entries"));
        Assert.NotNull(payload.GetType().GetProperty("DuplicatePaths"));
        Assert.NotNull(payload.GetType().GetProperty("TotalSystemPaths"));
        Assert.NotNull(payload.GetType().GetProperty("TotalUserPaths"));
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EnvironmentReadPath_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.EnvironmentReadPathAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }
}