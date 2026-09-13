// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         VpnReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="VpnReadTool" /> covering VPN/RAS connection enumeration
///     and phonebook status reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class VpnReadToolTests
{
    private readonly VpnReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task VpnListConnections_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.VpnListConnectionsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task VpnListConnections_ReturnsDictionaryList()
    {
        ToolResult result = await _tool.VpnListConnectionsAsync();

        Assert.True(result.Success);
        // Results is a List<Dictionary<string, string?>> of phonebook entries
        List<Dictionary<string, string?>> entries = Assert.IsType<List<Dictionary<string, string?>>>(result.Results);
        // Every entry must have a Name key
        Assert.All(entries, e => Assert.True(e.ContainsKey("Name")));
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task VpnListConnections_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.VpnListConnectionsAsync();

        // Succeeds even when no VPN connections are configured (empty list)
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task VpnReadPhonebookStatus_ContainsPathAndExists()
    {
        ToolResult result = await _tool.VpnReadPhonebookStatusAsync();

        Assert.True(result.Success);
        string output = (string)result.Results!;
        Assert.Contains("PhonebookPath=", output);
        Assert.Matches(@"Exists=(True|False)", output);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task VpnReadPhonebookStatus_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.VpnReadPhonebookStatusAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task VpnReadPhonebookStatus_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.VpnReadPhonebookStatusAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }
}
