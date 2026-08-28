// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         WindowsUpdateReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="WindowsUpdateReadTool" /> covering Windows Update
///     policy settings reads from the registry.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsUpdateReadToolTests
{
    private readonly WindowsUpdateReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WindowsUpdateReadSettings_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.WindowsUpdateReadSettingsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WindowsUpdateReadSettings_ReturnsNonEmptyOutput()
    {
        ToolResult result = await _tool.WindowsUpdateReadSettingsAsync();

        Assert.True(result.Success);
        // Either policy values or the "not configured" message
        string output = (string)result.Results!;
        Assert.False(string.IsNullOrWhiteSpace(output));
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WindowsUpdateReadSettings_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.WindowsUpdateReadSettingsAsync();

        // Succeeds even when no policy settings are configured
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }
}