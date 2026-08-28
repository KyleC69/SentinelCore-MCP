// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         AudioDeviceReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="AudioDeviceReadTool" /> covering pnputil-based device
///     listing and registry-based default device queries.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class AudioDeviceReadToolTests
{
    private readonly AudioDeviceReadTool _tool = new();

    #region Audio_List_Devices tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AudioListDevices_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.audioListDevicesAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AudioListDevices_ContainsAudioEndpointsSection()
    {
        ToolResult result = await _tool.audioListDevicesAsync();

        Assert.True(result.Success);
        Assert.Contains("Audio Endpoints", (string)result.Results!);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AudioListDevices_ContainsAudioDevicesSection()
    {
        ToolResult result = await _tool.audioListDevicesAsync();

        Assert.True(result.Success);
        Assert.Contains("Audio Devices", (string)result.Results!);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AudioListDevices_ContainsDeviceDescription()
    {
        ToolResult result = await _tool.audioListDevicesAsync();

        Assert.True(result.Success);
        Assert.Contains("Device Description:", (string)result.Results!);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AudioListDevices_ContainsInstanceOrStatus()
    {
        ToolResult result = await _tool.audioListDevicesAsync();

        Assert.True(result.Success);
        // pnputil output always includes Instance ID and Status
        string output = (string)result.Results!;
        Assert.True(
            output.Contains("Instance ID:") || output.Contains("Status:"),
            "Expected pnputil output to contain Instance ID or Status fields");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AudioListDevices_ReturnsNullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.audioListDevicesAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Audio_Read_Default_Device tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AudioReadDefaultDevice_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.audioReadDefaultDeviceAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AudioReadDefaultDevice_ContainsActiveRenderEndpointsHeader()
    {
        ToolResult result = await _tool.audioReadDefaultDeviceAsync();

        Assert.True(result.Success);
        Assert.Contains("Active Render Endpoints:", (string)result.Results!);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AudioReadDefaultDevice_ContainsDeviceId()
    {
        ToolResult result = await _tool.audioReadDefaultDeviceAsync();

        Assert.True(result.Success);
        // Active endpoints should have an Id field
        Assert.Contains("Id=", (string)result.Results!);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AudioReadDefaultDevice_ContainsStateInfo()
    {
        ToolResult result = await _tool.audioReadDefaultDeviceAsync();

        Assert.True(result.Success);
        Assert.Contains("State=", (string)result.Results!);
    }

    #endregion
}
