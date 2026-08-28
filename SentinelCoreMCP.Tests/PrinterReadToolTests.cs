// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         PrinterReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="PrinterReadTool" /> covering printer enumeration
///     and individual printer reads via the Win32_Printer CIM class.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class PrinterReadToolTests
{
    private readonly PrinterReadTool _tool = new();

    #region Printer_List tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PrinterList_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.PrinterListAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PrinterList_ReturnsTypedListRecords()
    {
        ToolResult result = await _tool.PrinterListAsync();

        Assert.True(result.Success);
        // Results is a List<PrinterRecord>
        List<PrinterReadTool.PrinterRecord> printers =
            Assert.IsType<List<PrinterReadTool.PrinterRecord>>(result.Results);
        // Every record must have a name
        Assert.All(printers, p => Assert.False(string.IsNullOrWhiteSpace(p.Name)));
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PrinterList_RespectsMaxRecords()
    {
        const int maxRecords = 2;
        ToolResult result = await _tool.PrinterListAsync(maxRecords: maxRecords);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        List<PrinterReadTool.PrinterRecord> printers =
            Assert.IsType<List<PrinterReadTool.PrinterRecord>>(result.Results);
        Assert.True(printers.Count <= maxRecords,
            $"Expected at most {maxRecords} printers but got {printers.Count}");
    }

    [Fact]
    public async Task PrinterList_ZeroMaxRecords_ReturnsFailure()
    {
        ToolResult result = await _tool.PrinterListAsync(maxRecords: 0);

        Assert.False(result.Success);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PrinterList_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.PrinterListAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Printer_Read tests

    [Fact]
    public async Task PrinterRead_NullPrinterName_ReturnsFailure()
    {
        ToolResult result = await _tool.PrinterReadAsync(null!);

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PrinterRead_EmptyPrinterName_ReturnsFailure()
    {
        ToolResult result = await _tool.PrinterReadAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PrinterRead_NonExistentPrinter_ReturnsFailure()
    {
        ToolResult result = await _tool.PrinterReadAsync("NonExistentPrinter_12345");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }

    #endregion
}
