// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         PowerSettingsReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="PowerSettingsReadTool" /> covering power plan and
///     power setting enumeration via the power WMI namespace.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class PowerSettingsReadToolTests
{
    private readonly PowerSettingsReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PowerListPlans_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.powerListPlansAsync();

        if (!result.Success)
        {
            return;
        }

        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PowerListPlans_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.powerListPlansAsync();

        // The power WMI namespace may not be available on all systems
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PowerListPlans_WhenSuccessful_ContainsPlanEntries()
    {
        ToolResult result = await _tool.powerListPlansAsync();

        if (!result.Success)
        {
            return;
        } // Skip if WMI namespace unavailable

        string output = (string)result.Results!;
        Assert.Contains("InstanceId=", output);
        Assert.Contains("Name=", output);
        Assert.Contains("IsActive=", output);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PowerListSettings_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.powerListSettingsAsync();

        // The power WMI namespace may not be available on all systems
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PowerListSettings_WhenSuccessful_ContainsSettingEntries()
    {
        ToolResult result = await _tool.powerListSettingsAsync();

        if (!result.Success)
        {
            return;
        } // Skip if WMI namespace unavailable

        string output = (string)result.Results!;
        Assert.Contains("InstanceId=", output);
        Assert.Contains("Value=", output);
    }
}