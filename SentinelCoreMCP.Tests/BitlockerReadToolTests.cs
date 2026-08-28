// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         BitlockerReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="BitlockerReadTool" /> covering volume listing,
///     single volume reads, and input validation.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class BitlockerReadToolTests
{
    private readonly BitlockerReadTool _tool = new();

    #region Bitlocker_List_Volumes tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BitlockerListVolumes_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.BitlockerListVolumesAsync();

        // BitLocker WMI namespace may not be available on all systems
        // The tool should either succeed or fail gracefully
        Assert.True(result.Success || (result.ErrorDetails != null),
            $"Expected success or graceful failure but got: Success={result.Success}, ErrorDetails={result.ErrorDetails}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BitlockerListVolumes_WhenSuccessful_ReturnsListContent()
    {
        ToolResult result = await _tool.BitlockerListVolumesAsync();

        // Skip assertion if BitLocker WMI is not available
        if (!result.Success)
        {
            return;
        }

        Assert.NotNull(result.Results);
        // Results is a List<object> of volume records
        Assert.IsType<List<object>>(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BitlockerListVolumes_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.BitlockerListVolumesAsync();

        if (!result.Success)
        {
            return; // BitLocker WMI not available on this system
        }

        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Bitlocker_Read_Volume tests

    [Fact]
    public async Task BitlockerReadVolume_NullDeviceId_ReturnsFailure()
    {
        ToolResult result = await _tool.BitlockerReadVolumeAsync(null!);

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BitlockerReadVolume_EmptyDeviceId_ReturnsFailure()
    {
        ToolResult result = await _tool.BitlockerReadVolumeAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BitlockerReadVolume_WhitespaceDeviceId_ReturnsFailure()
    {
        ToolResult result = await _tool.BitlockerReadVolumeAsync("   ");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BitlockerReadVolume_NonExistentDeviceId_ReturnsFailureOrGracefulError()
    {
        ToolResult result = await _tool.BitlockerReadVolumeAsync("NonExistentVolume12345");

        // Should fail - either "not found" or "failed" since the volume doesn't exist
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }

    #endregion
}
