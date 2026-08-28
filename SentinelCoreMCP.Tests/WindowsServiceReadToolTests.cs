// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         WindowsServiceReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="WindowsServiceReadTool" /> covering Windows service queries.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsServiceReadToolTests
{
    private readonly WindowsServiceReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ServiceList_ContainsServiceInfo()
    {
        ToolResult result = await _tool.serviceListAsync();

        Assert.True(result.Success);
        // Service list output should contain service-related information
        Assert.NotEmpty(((string)result.Results!).Trim());
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ServiceList_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.serviceListAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ServiceList_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.serviceListAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ServiceList_WithNameFilter_ReturnsFilteredResults()
    {
        ToolResult result = await _tool.serviceListAsync(nameFilter: "Windows");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    public async Task ServiceRead_EmptyServiceName_ReturnsFailure()
    {
        ToolResult result = await _tool.serviceReadAsync("");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ServiceRead_KnownService_ReturnsSuccessfulToolResult()
    {
        // EventLog service exists on all Windows systems
        ToolResult result = await _tool.serviceReadAsync("EventLog");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ServiceRead_NonExistentService_ReturnsFailure()
    {
        ToolResult result = await _tool.serviceReadAsync("NonExistentService_12345");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }
}