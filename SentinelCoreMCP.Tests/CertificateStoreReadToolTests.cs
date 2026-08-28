// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         CertificateStoreReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="CertificateStoreReadTool" /> covering certificate store
///     listing and individual certificate reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class CertificateStoreReadToolTests
{
    private readonly CertificateStoreReadTool _tool = new();

    #region Certificate_List validation tests

    [Fact]
    public async Task CertificateList_NullStoreName_ReturnsFailure()
    {
        ToolResult result = await _tool.CertificateListAsync(null!);

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CertificateList_EmptyStoreName_ReturnsFailure()
    {
        ToolResult result = await _tool.CertificateListAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Certificate_List integration tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CertificateList_RootStore_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.CertificateListAsync("Root");

        // The LocalMachine Root store exists on all Windows systems
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CertificateList_RootStore_ContainsCertificateDetails()
    {
        ToolResult result = await _tool.CertificateListAsync("Root");

        Assert.True(result.Success);
        string output = (string)result.Results!;
        // Root store always has certificates on Windows
        Assert.Contains("Subject=", output);
        Assert.Contains("Thumbprint=", output);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CertificateList_RespectsMaxRecords()
    {
        const int maxRecords = 3;
        ToolResult result = await _tool.CertificateListAsync("Root", maxRecords: maxRecords);

        Assert.True(result.Success);
        string output = (string)result.Results!;
        int certCount = 0;
        int index = 0;
        while ((index = output.IndexOf("Subject=", index, StringComparison.Ordinal)) >= 0)
        {
            certCount++;
            index++;
        }

        Assert.True(certCount <= maxRecords,
            $"Expected at most {maxRecords} certificates but got {certCount}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CertificateList_CurrentUserMy_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.CertificateListAsync("My", System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CertificateList_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.CertificateListAsync("Root");

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Certificate_Read tests

    [Fact]
    public async Task CertificateRead_EmptyThumbprint_ReturnsFailure()
    {
        ToolResult result = await _tool.CertificateReadAsync("", "Root");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CertificateRead_EmptyStoreName_ReturnsFailure()
    {
        ToolResult result = await _tool.CertificateReadAsync("ABC", "");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CertificateRead_NonExistentThumbprint_ReturnsFailure()
    {
        ToolResult result = await _tool.CertificateReadAsync("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "Root");

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CertificateRead_RootStoreCertificate_ReturnsSuccessfulToolResult()
    {
        using System.Security.Cryptography.X509Certificates.X509Store store =
            new(System.Security.Cryptography.X509Certificates.StoreName.Root,
                System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine);
        store.Open(System.Security.Cryptography.X509Certificates.OpenFlags.ReadOnly | System.Security.Cryptography.X509Certificates.OpenFlags.OpenExistingOnly);

        if (store.Certificates.Count == 0)
        {
            return; // No certificates in Root store on this system
        }

        string thumbprint = store.Certificates[0].Thumbprint;
        ToolResult result = await _tool.CertificateReadAsync(thumbprint, "Root");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        string output = (string)result.Results!;
        Assert.Contains($"Thumbprint={thumbprint}", output);
    }

    #endregion
}
