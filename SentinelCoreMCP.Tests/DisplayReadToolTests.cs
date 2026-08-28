// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         DisplayReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="DisplayReadTool" /> covering monitor enumeration,
///     video controller inventory, and virtual screen geometry.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class DisplayReadToolTests
{
    private readonly DisplayReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DisplayListMonitors_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.DisplayListMonitorsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DisplayListMonitors_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.DisplayListMonitorsAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DisplayListMonitors_ReturnsTypedMonitorRecords()
    {
        ToolResult result = await _tool.DisplayListMonitorsAsync();

        Assert.True(result.Success);
        List<DisplayReadTool.MonitorRecord> monitors = Assert.IsType<List<DisplayReadTool.MonitorRecord>>(result.Results);
        // Every record must have a device ID
        Assert.All(monitors, m => Assert.False(string.IsNullOrWhiteSpace(m.DeviceId)));
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DisplayListVideoControllers_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.DisplayListVideoControllersAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DisplayListVideoControllers_ReturnsNonEmptyRecords()
    {
        ToolResult result = await _tool.DisplayListVideoControllersAsync();

        Assert.True(result.Success);
        List<DisplayReadTool.VideoControllerRecord> controllers = Assert.IsType<List<DisplayReadTool.VideoControllerRecord>>(result.Results);
        // Every system has at least one video controller
        Assert.NotEmpty(controllers);
        Assert.All(controllers, c => Assert.False(string.IsNullOrWhiteSpace(c.Name)));
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DisplayListVideoControllers_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.DisplayListVideoControllersAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DisplayReadVirtualScreen_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.DisplayReadVirtualScreenAsync();

        // May fail on headless systems or VMs without active display
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DisplayReadVirtualScreen_WhenSuccessful_ReturnsTypedRecord()
    {
        ToolResult result = await _tool.DisplayReadVirtualScreenAsync();

        if (!result.Success)
        {
            return;
        } // Skip on headless systems

        DisplayReadTool.VirtualScreenRecord record = Assert.IsType<DisplayReadTool.VirtualScreenRecord>(result.Results);
        // Geometry values are non-negative; 0 is valid on headless/VM systems
        Assert.True(record.HorizontalResolution >= 0);
        Assert.True(record.VerticalResolution >= 0);
        Assert.True(record.BitsPerPixel >= 0);
    }
}