// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         BootConfigurationReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="BootConfigurationReadTool" /> covering BCD store
///     enumeration via bcdedit.exe.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class BootConfigurationReadToolTests
{
    private readonly BootConfigurationReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BcdeditCurrent_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.BcdeditCurrentAsync();

        // bcdedit may require elevation on some systems
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BcdeditCurrent_WhenSuccessful_ContainsBootEntry()
    {
        ToolResult result = await _tool.BcdeditCurrentAsync();

        if (!result.Success)
        {
            return;
        } // Skip if not elevated

        string output = (string)result.Results!;
        Assert.NotEmpty(output.Trim());
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BcdeditEnum_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.BcdeditEnumAsync();

        if (!result.Success)
        {
            return;
        }

        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BcdeditEnum_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.BcdeditEnumAsync();

        // bcdedit may require elevation on some systems
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task BcdeditEnum_WhenSuccessful_ReturnsNonEmptyOutput()
    {
        ToolResult result = await _tool.BcdeditEnumAsync();

        if (!result.Success)
        {
            return;
        } // Skip if not elevated

        string output = (string)result.Results!;
        // bcdedit /enum produces non-empty output on success
        Assert.NotEmpty(output.Trim());
    }
}