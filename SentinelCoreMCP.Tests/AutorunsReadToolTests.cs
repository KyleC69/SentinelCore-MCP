// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         AutorunsReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="AutorunsReadTool" /> covering autorun/autostart enumeration.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class AutorunsReadToolTests
{
    private readonly AutorunsReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AutorunsListIfeo_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.AutorunsListIfeoAsync();

        if (!result.Success)
        {
            return;
        } // Skip if not elevated

        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AutorunsListIfeo_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.AutorunsListIfeoAsync();

        // AutorunsReadTool uses Process.Start which may fail without admin privileges
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AutorunsList_ContainsPersistenceCategories()
    {
        ToolResult result = await _tool.AutorunsListAsync();

        if (!result.Success)
        {
            return;
        } // Skip if not elevated

        string output = (string)result.Results!;
        Assert.Contains("HKLM Run", output);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AutorunsList_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.AutorunsListAsync();

        if (!result.Success)
        {
            return;
        } // Skip if not elevated

        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AutorunsList_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.AutorunsListAsync();

        // AutorunsReadTool uses Process.Start which may fail without admin privileges
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AutorunsList_WhenSuccessful_ContainsOutput()
    {
        ToolResult result = await _tool.AutorunsListAsync();

        if (!result.Success)
        {
            return;
        } // Skip if not elevated

        Assert.NotNull(result.Results);
        Assert.NotEmpty(((string)result.Results!).Trim());
    }
}