// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         SensorsReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="SensorsReadTool" /> covering sensor device enumeration
///     and location service status.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SensorsReadToolTests
{
    private readonly SensorsReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SensorListDevices_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.SensorListDevicesAsync();

        if (!result.Success)
        {
            return;
        }

        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SensorListDevices_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.SensorListDevicesAsync();

        // Systems without sensors return an empty list (still success)
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SensorListDevices_WhenSuccessful_ReturnsListContent()
    {
        ToolResult result = await _tool.SensorListDevicesAsync();

        if (!result.Success)
        {
            return;
        } // Skip if WMI unavailable

        Assert.NotNull(result.Results);
        // Results is a List<object> of sensor records
        Assert.IsType<List<object>>(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SensorReadLocationService_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.SensorReadLocationServiceAsync();

        // The lfsvc service may not exist on all systems
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SensorReadLocationService_WhenSuccessful_ContainsStatus()
    {
        ToolResult result = await _tool.SensorReadLocationServiceAsync();

        if (!result.Success)
        {
            return;
        } // Skip if service not present

        string output = (string)result.Results!;
        Assert.Contains("LocationService(lfsvc) Status=", output);
    }
}
