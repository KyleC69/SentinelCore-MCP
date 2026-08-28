// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         CredentialsReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="CredentialsReadTool" /> covering credential target enumeration
///     via the Windows Credential Manager P/Invoke API.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class CredentialsReadToolTests
{
    private readonly CredentialsReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CredentialListTargets_DoesNotContainPasswords()
    {
        ToolResult result = await _tool.CredentialListTargetsAsync();

        Assert.True(result.Success);
        // The tool should only list targets, never passwords
        List<CredentialsReadTool.CredentialTargetRecord> records = Assert.IsType<List<CredentialsReadTool.CredentialTargetRecord>>(result.Results);
        Assert.All(records, r =>
        {
            Assert.DoesNotContain("password", r.UserName, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("password", r.Type, StringComparison.OrdinalIgnoreCase);
        });
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CredentialListTargets_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.CredentialListTargetsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CredentialListTargets_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.CredentialListTargetsAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CredentialListTargets_ReturnsTypedRecords()
    {
        ToolResult result = await _tool.CredentialListTargetsAsync();

        Assert.True(result.Success);
        // Results is a List<CredentialTargetRecord>
        List<CredentialsReadTool.CredentialTargetRecord> records = Assert.IsType<List<CredentialsReadTool.CredentialTargetRecord>>(result.Results);
        // Every record must have a non-empty target
        Assert.All(records, r => Assert.False(string.IsNullOrWhiteSpace(r.Target)));
    }
}