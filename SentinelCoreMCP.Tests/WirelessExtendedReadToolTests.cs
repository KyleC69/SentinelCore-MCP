// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         WirelessExtendedReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="WirelessExtendedReadTool" /> covering wireless network
///     connection history enumeration from the registry.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WirelessExtendedReadToolTests
{
    private readonly WirelessExtendedReadTool _tool = new();

    #region Wireless_List_Connection_History tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WirelessListConnectionHistory_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.WirelessListConnectionHistoryAsync();

        // Registry access to network profiles may be restricted on some systems
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WirelessListConnectionHistory_WhenSuccessful_ReturnsListContent()
    {
        ToolResult result = await _tool.WirelessListConnectionHistoryAsync();

        if (!result.Success) { return; } // Skip if registry access restricted

        Assert.NotNull(result.Results);
        // Results is a List<object> of network profile records
        Assert.IsType<List<object>>(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WirelessListConnectionHistory_RespectsMaxRecords()
    {
        const int maxRecords = 3;
        ToolResult result = await _tool.WirelessListConnectionHistoryAsync(maxRecords: maxRecords);

        if (!result.Success) { return; } // Skip if registry access restricted

        List<object> profiles = Assert.IsType<List<object>>(result.Results);
        Assert.True(profiles.Count <= maxRecords,
            $"Expected at most {maxRecords} profiles but got {profiles.Count}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WirelessListConnectionHistory_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.WirelessListConnectionHistoryAsync();

        if (!result.Success) { return; }

        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
