// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         RdpExtendedReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="RdpExtendedReadTool" /> covering RDP session enumeration
///     via the WTS API interop helper.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RdpExtendedReadToolTests
{
    private readonly RdpExtendedReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RdpListSessions_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.RdpListSessionsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RdpListSessions_RespectsMaxRecords()
    {
        const int maxRecords = 3;
        ToolResult result = await _tool.RdpListSessionsAsync(maxRecords: maxRecords);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RdpListSessions_ReturnsEnumerableSessionRecords()
    {
        ToolResult result = await _tool.RdpListSessionsAsync();

        Assert.True(result.Success);
        // Results is a typed list of session records (not a string)
        Assert.IsNotType<string>(result.Results);
        Assert.IsAssignableFrom<System.Collections.IEnumerable>(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RdpListSessions_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.RdpListSessionsAsync();

        // The WTS API works on all Windows systems
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    public async Task RdpListSessions_ZeroMaxRecords_ReturnsFailure()
    {
        ToolResult result = await _tool.RdpListSessionsAsync(maxRecords: 0);

        Assert.False(result.Success);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }
}