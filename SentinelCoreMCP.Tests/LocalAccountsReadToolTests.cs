// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         LocalAccountsReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="LocalAccountsReadTool" /> covering local user and group queries.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class LocalAccountsReadToolTests
{
    private readonly LocalAccountsReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task LocalGroupList_ContainsGroupInfo()
    {
        ToolResult result = await _tool.LocalGroupListAsync();

        Assert.True(result.Success);
        Assert.NotEmpty(((string)result.Results!).Trim());
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task LocalGroupList_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.LocalGroupListAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task LocalGroupList_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.LocalGroupListAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task LocalUserList_ContainsUserInfo()
    {
        ToolResult result = await _tool.LocalUserListAsync();

        Assert.True(result.Success);
        Assert.NotEmpty(((string)result.Results!).Trim());
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task LocalUserList_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.LocalUserListAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task LocalUserList_RespectsMaxRecords()
    {
        ToolResult result = await _tool.LocalUserListAsync(maxRecords: 5);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task LocalUserList_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.LocalUserListAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }
}