// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         FirewallReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Reflection;
using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="FirewallReadTool" /> covering normalize helpers and
///     integration execution on Windows.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class FirewallReadToolTests
{
    private readonly FirewallReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_DefaultParameters_ContainsRuleName()
    {
        ToolResult result = await _tool.firewallListRulesAsync();

        Assert.True(result.Success);
        Assert.Contains("Rule Name:", (string)result.Results!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_DefaultParameters_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.firewallListRulesAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
        Assert.NotEmpty((string)result.Results!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_InboundFilterOutputContainsOnlyInboundRules()
    {
        ToolResult result = await _tool.firewallListRulesAsync(direction: "Inbound");

        Assert.True(result.Success);
        // When filtered by Inbound, all Direction lines should say "In"
        string[] lines = ((string)result.Results!).Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var directionLines = lines.Where(l => l.StartsWith("Direction:", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.NotEmpty(directionLines);
        Assert.All(directionLines, line => Assert.Contains("In", line, StringComparison.OrdinalIgnoreCase));
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_OutboundFilterOutputContainsOnlyOutboundRules()
    {
        ToolResult result = await _tool.firewallListRulesAsync(direction: "Outbound");

        Assert.True(result.Success);
        // When filtered by Outbound, all Direction lines should say "Out"
        string[] lines = ((string)result.Results!).Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var directionLines = lines.Where(l => l.StartsWith("Direction:", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.NotEmpty(directionLines);
        Assert.All(directionLines, line => Assert.Contains("Out", line, StringComparison.OrdinalIgnoreCase));
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_OutputContainsExpectedFields()
    {
        ToolResult result = await _tool.firewallListRulesAsync();

        Assert.True(result.Success);
        // netsh output should contain these standard fields
        Assert.Contains("Rule Name:", (string)result.Results!);
        Assert.Contains("Enabled:", (string)result.Results);
        Assert.Contains("Direction:", (string)result.Results);
        Assert.Contains("Action:", (string)result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_ReturnsNullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.firewallListRulesAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_WithDirectionAndProfileFilter_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.firewallListRulesAsync(direction: "Inbound", profile: "Private");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_WithDomainProfileFilter_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.firewallListRulesAsync(profile: "Domain");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_WithInboundFilter_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.firewallListRulesAsync(direction: "Inbound");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_WithInvalidDirection_ReturnsSuccessfulToolResult()
    {
        // Invalid direction should be treated as "no filter" (returns all directions)
        ToolResult result = await _tool.firewallListRulesAsync(direction: "InvalidDirection");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_WithInvalidProfile_ReturnsSuccessfulToolResult()
    {
        // Invalid profile should be treated as "no filter" (returns all profiles)
        ToolResult result = await _tool.firewallListRulesAsync(profile: "InvalidProfile");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_WithMaxRecords_RespectsLimit()
    {
        const int maxRecords = 5;
        ToolResult result = await _tool.firewallListRulesAsync(maxRecords: maxRecords);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);

        // Count the number of rule blocks (each starts with "Rule Name:")
        string output = (string)result.Results!;
        int ruleCount = 0;
        int index = 0;
        while ((index = output.IndexOf("Rule Name:", index, StringComparison.Ordinal)) >= 0)
        {
            ruleCount++;
            index++;
        }

        Assert.True(ruleCount <= maxRecords, $"Expected at most {maxRecords} rule blocks but got {ruleCount}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_WithOutboundFilter_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.firewallListRulesAsync(direction: "Outbound");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_WithPrivateProfileFilter_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.firewallListRulesAsync(profile: "Private");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallListRules_WithPublicProfileFilter_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.firewallListRulesAsync(profile: "Public");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallReadProfiles_ContainsAllThreeProfiles()
    {
        ToolResult result = await _tool.firewallReadProfilesAsync();

        Assert.True(result.Success);
        Assert.Contains("Domain Profile", (string)result.Results!);
        Assert.Contains("Private Profile", (string)result.Results);
        Assert.Contains("Public Profile", (string)result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallReadProfiles_ContainsFirewallPolicyField()
    {
        ToolResult result = await _tool.firewallReadProfilesAsync();

        Assert.True(result.Success);
        Assert.Contains("Firewall Policy", (string)result.Results!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallReadProfiles_ContainsLoggingSection()
    {
        ToolResult result = await _tool.firewallReadProfilesAsync();

        Assert.True(result.Success);
        Assert.Contains("Logging", (string)result.Results!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallReadProfiles_ContainsStateField()
    {
        ToolResult result = await _tool.firewallReadProfilesAsync();

        Assert.True(result.Success);
        Assert.Contains("State", (string)result.Results!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallReadProfiles_ReturnsNullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.firewallReadProfilesAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FirewallReadProfiles_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.firewallReadProfilesAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    private static string? InvokeNormalizeDirection(string? direction)
    {
        MethodInfo? method = typeof(FirewallReadTool).GetMethod("NormalizeDirection", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        return (string?)method.Invoke(null, [direction]);
    }








    private static string? InvokeNormalizeProfile(string? profile)
    {
        MethodInfo? method = typeof(FirewallReadTool).GetMethod("NormalizeProfile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        return (string?)method.Invoke(null, [profile]);
    }








    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("unknown")]
    [InlineData("BIDIRECTIONAL")]
    [InlineData("both")]
    public void NormalizeDirection_InvalidOrNullDirections_ReturnsNull(string? direction)
    {
        string? result = InvokeNormalizeDirection(direction);
        Assert.Null(result);
    }








    [Theory]
    [InlineData("Inbound", "In")]
    [InlineData("inbound", "In")]
    [InlineData("INBOUND", "In")]
    [InlineData("InBound", "In")]
    [InlineData("Outbound", "Out")]
    [InlineData("outbound", "Out")]
    [InlineData("OUTBOUND", "Out")]
    [InlineData("OutBound", "Out")]
    public void NormalizeDirection_ValidDirections_ReturnsExpectedValue(string direction, string expected)
    {
        string? result = InvokeNormalizeDirection(direction);
        Assert.Equal(expected, result);
    }








    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("unknown")]
    [InlineData("GUEST")]
    public void NormalizeProfile_InvalidOrNullProfiles_ReturnsNull(string? profile)
    {
        string? result = InvokeNormalizeProfile(profile);
        Assert.Null(result);
    }








    [Theory]
    [InlineData("Domain", "Domain")]
    [InlineData("domain", "Domain")]
    [InlineData("DOMAIN", "Domain")]
    [InlineData("Private", "Private")]
    [InlineData("private", "Private")]
    [InlineData("PRIVATE", "Private")]
    [InlineData("Public", "Public")]
    [InlineData("public", "Public")]
    [InlineData("PUBLIC", "Public")]
    public void NormalizeProfile_ValidProfiles_ReturnsExpectedValue(string profile, string expected)
    {
        string? result = InvokeNormalizeProfile(profile);
        Assert.Equal(expected, result);
    }
}