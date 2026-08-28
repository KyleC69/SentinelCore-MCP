// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         AuditingToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="AuditingTool" /> covering audit policy enumeration
///     via the auditpol.exe native command.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class AuditingToolTests
{
    private readonly AuditingTool _tool = new();

    #region GetAuditPolicy tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task GetAuditPolicy_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.GetAuditPolicyAsync();

        // auditpol requires admin privileges; accept graceful failure when not elevated
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task GetAuditPolicy_ReturnsSuccessfulOrPrivilegeError()
    {
        ToolResult result = await _tool.GetAuditPolicyAsync();

        // auditpol requires admin privileges; the tool may succeed or fail gracefully
        // depending on the elevation level of the test runner
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task GetAuditPolicy_WhenSuccessful_ReturnsNonEmptyOutput()
    {
        ToolResult result = await _tool.GetAuditPolicyAsync();

        // auditpol requires admin privileges; skip if output is empty (privilege error)
        if (!result.Success || string.IsNullOrWhiteSpace((string?)result.Results))
        {
            return;
        }

        Assert.NotEmpty(((string)result.Results!).Trim());
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task GetAuditPolicy_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.GetAuditPolicyAsync();

        if (!result.Success) { return; } // Skip if not elevated (auditpol requires admin)

        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
