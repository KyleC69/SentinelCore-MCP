// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         BatteryReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="BatteryReadTool" /> covering battery listing and power settings.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class BatteryReadToolTests
{
    private readonly BatteryReadTool _tool = new();

    #region Battery_List tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BatteryList_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.BatteryListAsync();

        // Battery WMI may not be available on desktops without batteries
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BatteryList_WhenSuccessful_ReturnsListContent()
    {
        ToolResult result = await _tool.BatteryListAsync();

        if (!result.Success) { return; } // Skip if no battery

        Assert.NotNull(result.Results);
        // Results is a List<object> of battery records
        Assert.IsType<List<object>>(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BatteryList_RespectsMaxRecords()
    {
        const int maxRecords = 2;
        ToolResult result = await _tool.BatteryListAsync(maxRecords: maxRecords);

        if (!result.Success) { return; } // Skip if no battery

        Assert.NotNull(result.Results);
        List<object> batteries = Assert.IsType<List<object>>(result.Results);
        Assert.True(batteries.Count <= maxRecords,
            $"Expected at most {maxRecords} batteries but got {batteries.Count}");
    }

    #endregion

    #region Battery_Read_Power_Settings tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BatteryReadPowerSettings_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.BatteryReadPowerSettingsAsync();

        // Power settings WMI namespace may not be available on all systems
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion
}

