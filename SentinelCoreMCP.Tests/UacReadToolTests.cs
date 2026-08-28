// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         UacReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="UacReadTool" /> covering UAC settings registry reads
///     and token elevation detection.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class UacReadToolTests
{
    private readonly UacReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task UacReadSettings_ContainsConsentPromptBehavior()
    {
        ToolResult result = await _tool.uacReadSettingsAsync();

        Assert.True(result.Success);
        Assert.Contains("ConsentPromptBehavior", (string)result.Results!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task UacReadSettings_ContainsEnableLUA()
    {
        ToolResult result = await _tool.uacReadSettingsAsync();

        Assert.True(result.Success);
        Assert.Contains("EnableLUA", (string)result.Results!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task UacReadSettings_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.uacReadSettingsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task UacReadSettings_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.uacReadSettingsAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task UacReadTokenElevation_ContainsIsAdministrator()
    {
        ToolResult result = await _tool.uacReadTokenElevationAsync();

        Assert.True(result.Success);
        Assert.Contains("IsAdministrator=", (string)result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task UacReadTokenElevation_ContainsIsElevated()
    {
        ToolResult result = await _tool.uacReadTokenElevationAsync();

        Assert.True(result.Success);
        Assert.Contains("IsElevated=", (string)result.Results!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task UacReadTokenElevation_HasBooleanValues()
    {
        ToolResult result = await _tool.uacReadTokenElevationAsync();

        Assert.True(result.Success);
        // The values should be True or False
        string output = (string)result.Results!;
        Assert.Matches(@"IsElevated=(True|False)", output);
        Assert.Matches(@"IsAdministrator=(True|False)", output);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task UacReadTokenElevation_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.uacReadTokenElevationAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task UacReadTokenElevation_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.uacReadTokenElevationAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }
}