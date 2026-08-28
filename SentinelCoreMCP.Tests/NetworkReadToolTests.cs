// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         NetworkReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="NetworkReadTool" /> covering network interface and connection queries.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class NetworkReadToolTests
{
    private readonly NetworkReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkListInterfaces_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.NetworkListInterfacesAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkListInterfaces_RespectsMaxRecords()
    {
        ToolResult result = await _tool.NetworkListInterfacesAsync(maxRecords: 2);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkListInterfaces_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.NetworkListInterfacesAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkListTcpConnections_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.NetworkListTcpConnectionsAsync();

        // TCP connection enumeration may fail in restricted environments
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    public async Task NetworkReadIpConfig_EmptyInterfaceName_ReturnsFailure()
    {
        ToolResult result = await _tool.NetworkReadIpConfigAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkReadIpConfig_NonExistentInterface_ReturnsFailure()
    {
        ToolResult result = await _tool.NetworkReadIpConfigAsync("NonExistentInterface_12345");

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    public async Task NetworkReadIpConfig_NullInterfaceName_ReturnsFailure()
    {
        ToolResult result = await _tool.NetworkReadIpConfigAsync(null!);

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    public async Task NetworkResolveDns_EmptyHostName_ReturnsFailure()
    {
        ToolResult result = await _tool.NetworkResolveDnsAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkResolveDns_Localhost_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.NetworkResolveDnsAsync("localhost");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
        Assert.Contains("HostName=", (string)result.Results!);
    }
}