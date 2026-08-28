// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         SystemReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Reflection;
using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="SystemReadTool" /> covering system information queries.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SystemReadToolTests
{
    private readonly SystemReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SystemReadInfo_ContainsMachineName()
    {
        ToolResult result = await _tool.SystemReadInfoAsync();

        Assert.True(result.Success);
        // The anonymous object should expose MachineName matching the environment
        object info = result.Results!;
        PropertyInfo? machineNameProperty = info.GetType().GetProperty("MachineName");
        Assert.NotNull(machineNameProperty);
        Assert.Equal(Environment.MachineName, (string)machineNameProperty.GetValue(info)!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SystemReadInfo_ContainsOsVersion()
    {
        ToolResult result = await _tool.SystemReadInfoAsync();

        Assert.True(result.Success);
        object info = result.Results!;
        PropertyInfo? osVersionProperty = info.GetType().GetProperty("OSVersion");
        Assert.NotNull(osVersionProperty);
        Assert.False(string.IsNullOrEmpty((string)osVersionProperty.GetValue(info)!));
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SystemReadInfo_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.SystemReadInfoAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SystemReadInfo_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.SystemReadInfoAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SystemReadTimeZone_ContainsTimeZoneId()
    {
        ToolResult result = await _tool.SystemReadTimeZoneAsync();

        Assert.True(result.Success);
        object info = result.Results!;
        PropertyInfo? idProperty = info.GetType().GetProperty("Id");
        Assert.NotNull(idProperty);
        Assert.Equal(TimeZoneInfo.Local.Id, (string)idProperty.GetValue(info)!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SystemReadTimeZone_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.SystemReadTimeZoneAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SystemReadTimeZone_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.SystemReadTimeZoneAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }
}