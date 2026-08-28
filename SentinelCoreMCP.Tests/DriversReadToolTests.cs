// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         DriversReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="DriversReadTool" /> covering driver enumeration.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class DriversReadToolTests
{
    private readonly DriversReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DriverList_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.DriverListAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DriverList_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.DriverListAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DriverList_WithKernelFilter_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.DriverListAsync(typeFilter: "kernel");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DriversList_ContainsDriverInfo()
    {
        ToolResult result = await _tool.DriverListAsync();

        Assert.True(result.Success);
        Assert.NotEmpty(((string)result.Results!).Trim());
    }
}