// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         NetworkDnsReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="NetworkDnsReadTool" /> covering DNS configuration
///     registry reads for hijacking detection.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class NetworkDnsReadToolTests
{
    private readonly NetworkDnsReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkReadDnsSettings_ContainsDnsClientSection()
    {
        ToolResult result = await _tool.NetworkReadDnsSettingsAsync();

        Assert.True(result.Success);
        string output = (string)result.Results!;
        Assert.Contains("[DNS Client Parameters]", output);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkReadDnsSettings_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.NetworkReadDnsSettingsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NetworkReadDnsSettings_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.NetworkReadDnsSettingsAsync();

        // The Dnscache parameters key exists on all Windows systems
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }
}