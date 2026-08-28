// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         SecurityExtendedReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="SecurityExtendedReadTool" /> covering Credential Guard,
///     Secure Boot, TPM, and Exploit Protection status reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SecurityExtendedReadToolTests
{
    private readonly SecurityExtendedReadTool _tool = new();

    #region Security_Read_Credential_Guard tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SecurityReadCredentialGuard_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.SecurityReadCredentialGuardAsync();

        // The LSA registry key exists on all Windows systems
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SecurityReadCredentialGuard_ContainsLsaValues()
    {
        ToolResult result = await _tool.SecurityReadCredentialGuardAsync();

        Assert.True(result.Success);
        string output = (string)result.Results!;
        Assert.Contains("RunAsPPL=", output);
        Assert.Contains("LsaCfgFlags=", output);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SecurityReadCredentialGuard_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.SecurityReadCredentialGuardAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Security_Read_SecureBoot tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SecurityReadSecureBoot_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.SecurityReadSecureBootAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SecurityReadSecureBoot_ContainsUefiStateSection()
    {
        ToolResult result = await _tool.SecurityReadSecureBootAsync();

        Assert.True(result.Success);
        // The tool always emits a UEFI State section (present or not-available)
        string output = (string)result.Results!;
        Assert.Contains("[UEFI State]", output);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SecurityReadSecureBoot_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.SecurityReadSecureBootAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Security_Read_TPM tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SecurityReadTpm_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.SecurityReadTpmAsync();

        // Succeeds even when TPM is absent (empty output)
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SecurityReadTpm_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.SecurityReadTpmAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Security_Read_Exploit_Protection tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SecurityReadExploitProtection_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.SecurityReadExploitProtectionAsync();

        // Registry access to Memory Management may be restricted on some systems
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SecurityReadExploitProtection_ContainsDepSection()
    {
        ToolResult result = await _tool.SecurityReadExploitProtectionAsync();

        if (!result.Success) { return; } // Skip if registry access restricted

        string output = (string)result.Results!;
        Assert.Contains("[Memory Management / DEP]", output);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SecurityReadExploitProtection_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.SecurityReadExploitProtectionAsync();

        if (!result.Success) { return; } // Skip if registry access restricted

        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
