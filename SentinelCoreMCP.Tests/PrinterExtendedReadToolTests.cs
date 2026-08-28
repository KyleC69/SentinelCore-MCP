// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         PrinterExtendedReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="PrinterExtendedReadTool" /> covering print spooler
///     job enumeration via the Win32_PrintJob CIM class.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class PrinterExtendedReadToolTests
{
    private readonly PrinterExtendedReadTool _tool = new();

    #region Printer_List_Jobs tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PrinterListJobs_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.PrinterListJobsAsync();

        // The spooler service runs on all Windows systems; empty queue is valid
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PrinterListJobs_ReturnsTypedListRecords()
    {
        ToolResult result = await _tool.PrinterListJobsAsync();

        Assert.True(result.Success);
        // Results is a List<PrintJobRecord> (may be empty when no jobs are queued)
        Assert.IsType<List<PrinterExtendedReadTool.PrintJobRecord>>(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PrinterListJobs_RespectsMaxRecords()
    {
        const int maxRecords = 3;
        ToolResult result = await _tool.PrinterListJobsAsync(maxRecords: maxRecords);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        List<PrinterExtendedReadTool.PrintJobRecord> jobs =
            Assert.IsType<List<PrinterExtendedReadTool.PrintJobRecord>>(result.Results);
        Assert.True(jobs.Count <= maxRecords,
            $"Expected at most {maxRecords} jobs but got {jobs.Count}");
    }

    [Fact]
    public async Task PrinterListJobs_ZeroMaxRecords_ReturnsFailure()
    {
        ToolResult result = await _tool.PrinterListJobsAsync(maxRecords: 0);

        Assert.False(result.Success);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PrinterListJobs_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.PrinterListJobsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
