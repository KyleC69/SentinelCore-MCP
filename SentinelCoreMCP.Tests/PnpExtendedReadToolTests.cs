// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         PnpExtendedReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="PnpExtendedReadTool" /> covering USB device connection
///     history enumeration from the registry.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class PnpExtendedReadToolTests
{
    private readonly PnpExtendedReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PnpListUsbHistory_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.PnpListUsbHistoryAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PnpListUsbHistory_RespectsMaxRecords()
    {
        const int maxRecords = 3;
        ToolResult result = await _tool.PnpListUsbHistoryAsync(maxRecords: maxRecords);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        List<object> devices = Assert.IsType<List<object>>(result.Results);
        Assert.True(devices.Count <= maxRecords, $"Expected at most {maxRecords} devices but got {devices.Count}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PnpListUsbHistory_ReturnsListContent()
    {
        ToolResult result = await _tool.PnpListUsbHistoryAsync();

        Assert.True(result.Success);
        // Results is a List<object> of USB device records
        Assert.IsType<List<object>>(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PnpListUsbHistory_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.PnpListUsbHistoryAsync();

        // The USBSTOR/USB enum keys exist on all Windows systems
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }
}