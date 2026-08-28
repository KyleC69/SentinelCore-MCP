// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         PnpDeviceReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="PnpDeviceReadTool" /> covering PnP device enumeration.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class PnpDeviceReadToolTests
{
    private readonly PnpDeviceReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PnpListDevices_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.PnpListDevicesAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PnpListDevices_RespectsMaxRecords()
    {
        ToolResult result = await _tool.PnpListDevicesAsync(maxRecords: 5);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PnpListDevices_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.PnpListDevicesAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    public async Task PnpReadDevice_EmptyDeviceId_ReturnsFailure()
    {
        ToolResult result = await _tool.PnpReadDeviceAsync("");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PnpReadDevice_NonExistentDeviceId_ReturnsFailureOrGracefulError()
    {
        ToolResult result = await _tool.PnpReadDeviceAsync("NonExistentDeviceId_12345");

        // Should fail gracefully - device doesn't exist
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }
}