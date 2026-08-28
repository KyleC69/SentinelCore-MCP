// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         DcomReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="DcomReadTool" /> covering DCOM application enumeration
///     and AppID settings reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class DcomReadToolTests
{
    private readonly DcomReadTool _tool = new();

    #region DCOM_List_Applications tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DcomListApplications_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.DcomListApplicationsAsync();

        // Win32_DCOMApplication may not be available on all systems
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DcomListApplications_WhenSuccessful_ContainsAppIdEntries()
    {
        ToolResult result = await _tool.DcomListApplicationsAsync();

        if (!result.Success) { return; } // Skip if WMI class unavailable

        string output = (string)result.Results!;
        // Every entry line uses the AppID= format
        if (output.Trim().Length > 0)
        {
            Assert.Contains("AppID=", output);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DcomListApplications_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.DcomListApplicationsAsync();

        if (!result.Success) { return; }

        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region DCOM_Read_AppId_Settings tests

    [Fact]
    public async Task DcomReadAppidSettings_NullAppId_ReturnsFailure()
    {
        ToolResult result = await _tool.DcomReadAppidSettingsAsync(null!);

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DcomReadAppidSettings_EmptyAppId_ReturnsFailure()
    {
        ToolResult result = await _tool.DcomReadAppidSettingsAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task DcomReadAppidSettings_NonExistentAppId_ReturnsFailure()
    {
        ToolResult result = await _tool.DcomReadAppidSettingsAsync("{00000000-0000-0000-0000-000000000000}");

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
