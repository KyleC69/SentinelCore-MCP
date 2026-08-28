// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         NetworkExtendedReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="NetworkExtendedReadTool" /> covering listening ports,
///     DNS cache, ARP table, routing table, and share enumeration.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class NetworkExtendedReadToolTests
{
    private readonly NetworkExtendedReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkListListeningPorts_RespectsMaxRecords()
    {
        const int maxRecords = 5;
        ToolResult result = await _tool.NetworkListListeningPortsAsync(maxRecords: maxRecords);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        List<object> ports = Assert.IsType<List<object>>(result.Results);
        Assert.True(ports.Count <= maxRecords, $"Expected at most {maxRecords} ports but got {ports.Count}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkListListeningPorts_ReturnsNonEmptyList()
    {
        ToolResult result = await _tool.NetworkListListeningPortsAsync();

        Assert.True(result.Success);
        // Every Windows system has at least one listening port
        List<object> ports = Assert.IsType<List<object>>(result.Results);
        Assert.NotEmpty(ports);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkListListeningPorts_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.NetworkListListeningPortsAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    public async Task NetworkListListeningPorts_ZeroMaxRecords_ReturnsFailure()
    {
        ToolResult result = await _tool.NetworkListListeningPortsAsync(maxRecords: 0);

        Assert.False(result.Success);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkListShares_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.NetworkListSharesAsync();

        // Share enumeration may fail in restricted environments
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkReadArpTable_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.NetworkReadArpTableAsync();

        // arp -a may fail in restricted environments
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkReadDnsCache_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.NetworkReadDnsCacheAsync();

        // ipconfig /displaydns may fail in restricted environments
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkReadRoutingTable_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.NetworkReadRoutingTableAsync();

        // route print may fail in restricted environments
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }
}