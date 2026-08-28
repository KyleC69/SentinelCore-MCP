// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         WirelessReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="WirelessReadTool" /> covering wireless interface
///     enumeration and Wi-Fi profile listing.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WirelessReadToolTests
{
    private readonly WirelessReadTool _tool = new();

    #region Wireless_List_Interfaces tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WirelessListInterfaces_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.wirelessListInterfacesAsync();

        // Systems without wireless adapters or the StandardCimv2 namespace may fail
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WirelessListInterfaces_WhenSuccessful_ReturnsListContent()
    {
        ToolResult result = await _tool.wirelessListInterfacesAsync();

        if (!result.Success) { return; } // Skip if no wireless adapter

        Assert.NotNull(result.Results);
        // Results is a List<object> of interface records
        Assert.IsType<List<object>>(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WirelessListInterfaces_RespectsMaxRecords()
    {
        const int maxRecords = 2;
        ToolResult result = await _tool.wirelessListInterfacesAsync(maxRecords: maxRecords);

        if (!result.Success) { return; }

        List<object> interfaces = Assert.IsType<List<object>>(result.Results);
        Assert.True(interfaces.Count <= maxRecords,
            $"Expected at most {maxRecords} interfaces but got {interfaces.Count}");
    }

    #endregion

    #region Wireless_List_Profiles tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WirelessListProfiles_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.wirelessListProfilesAsync();

        // netsh wlan may not be available on systems without wireless capability
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WirelessListProfiles_WhenSuccessful_ContainsProfileOutput()
    {
        ToolResult result = await _tool.wirelessListProfilesAsync();

        if (!result.Success) { return; } // Skip if wireless not supported

        string output = (string)result.Results!;
        Assert.NotEmpty(output.Trim());
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WirelessListProfiles_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.wirelessListProfilesAsync();

        if (!result.Success) { return; }

        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
