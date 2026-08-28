// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         RemoteDesktopReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="RemoteDesktopReadTool" /> covering RDP listener
///     configuration and Remote Desktop settings reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RemoteDesktopReadToolTests
{
    private readonly RemoteDesktopReadTool _tool = new();

    #region RDP_Read_Listener_Config tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RdpReadListenerConfig_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.rdpReadListenerConfigAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RdpReadListenerConfig_ContainsPortNumber()
    {
        ToolResult result = await _tool.rdpReadListenerConfigAsync();

        Assert.True(result.Success);
        // The RDP TCP key always exists on Windows systems with the PortNumber value
        Assert.Contains("PortNumber=", (string)result.Results!);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RdpReadListenerConfig_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.rdpReadListenerConfigAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region RDP_Read_Settings tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RdpReadSettings_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.rdpReadSettingsAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RdpReadSettings_ContainsDenyTSConnections()
    {
        ToolResult result = await _tool.rdpReadSettingsAsync();

        Assert.True(result.Success);
        // Terminal Server key always exists with fDenyTSConnections
        Assert.Contains("fDenyTSConnections=", (string)result.Results!);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RdpReadSettings_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.rdpReadSettingsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
