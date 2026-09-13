// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         ShellExplorerReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="ShellExplorerReadTool" /> covering Explorer settings
///     and taskbar pinned item reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ShellExplorerReadToolTests
{
    private readonly ShellExplorerReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ShellExplorerReadSettings_ContainsAdvancedKeySection()
    {
        ToolResult result = await _tool.ShellExplorerReadSettingsAsync();

        Assert.True(result.Success);
        string output = (string)result.Results!;
        Assert.Contains("Advanced", output);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ShellExplorerReadSettings_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.ShellExplorerReadSettingsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ShellExplorerReadSettings_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.ShellExplorerReadSettingsAsync();

        // The Explorer Advanced key exists on all Windows systems
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ShellTaskbarPinnedList_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.ShellTaskbarPinnedListAsync();

        // The pinned path may not exist on all systems/profiles
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ShellTaskbarPinnedList_WhenSuccessful_ReturnsFileList()
    {
        ToolResult result = await _tool.ShellTaskbarPinnedListAsync();

        if (!result.Success)
        {
            return;
        } // Skip if pinned path not present

        Assert.NotNull(result.Results);
    }
}
