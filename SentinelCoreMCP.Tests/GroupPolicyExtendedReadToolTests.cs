// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         GroupPolicyExtendedReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="GroupPolicyExtendedReadTool" /> covering Resultant Set
///     of Policy (RSOP) reads via the RSOP WMI namespace.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class GroupPolicyExtendedReadToolTests
{
    private readonly GroupPolicyExtendedReadTool _tool = new();

    #region Group_Policy_Read_RSOP tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task GroupPolicyReadRsop_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.GroupPolicyReadRsopAsync();

        // The RSOP WMI namespace may not be available on all systems
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task GroupPolicyReadRsop_WhenSuccessful_ReturnsListContent()
    {
        ToolResult result = await _tool.GroupPolicyReadRsopAsync();

        if (!result.Success) { return; } // Skip if RSOP namespace unavailable

        Assert.NotNull(result.Results);
        // Results is a List<object> of RSOP entries
        Assert.IsType<List<object>>(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task GroupPolicyReadRsop_RespectsMaxRecords()
    {
        const int maxRecords = 3;
        ToolResult result = await _tool.GroupPolicyReadRsopAsync(maxRecords: maxRecords);

        if (!result.Success) { return; } // Skip if RSOP namespace unavailable

        List<object> entries = Assert.IsType<List<object>>(result.Results);
        Assert.True(entries.Count <= maxRecords,
            $"Expected at most {maxRecords} entries but got {entries.Count}");
    }

    [Fact]
    public async Task GroupPolicyReadRsop_ZeroMaxRecords_ReturnsFailure()
    {
        ToolResult result = await _tool.GroupPolicyReadRsopAsync(maxRecords: 0);

        Assert.False(result.Success);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
