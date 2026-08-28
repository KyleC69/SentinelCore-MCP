// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         CertificateExtendedReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="CertificateExtendedReadTool" /> covering certificate
///     trust chain verification.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class CertificateExtendedReadToolTests
{
    private readonly CertificateExtendedReadTool _tool = new();








    [Fact]
    public async Task CertificateVerify_EmptyThumbprint_ReturnsFailure()
    {
        ToolResult result = await _tool.CertificateVerifyAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CertificateVerify_NonExistentThumbprint_ReturnsFailure()
    {
        ToolResult result = await _tool.CertificateVerifyAsync("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CertificateVerify_NullErrorDetailsOnSuccess()
    {
        using System.Security.Cryptography.X509Certificates.X509Store store = new(System.Security.Cryptography.X509Certificates.StoreName.Root, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine);
        store.Open(System.Security.Cryptography.X509Certificates.OpenFlags.ReadOnly | System.Security.Cryptography.X509Certificates.OpenFlags.OpenExistingOnly);

        if (store.Certificates.Count == 0)
        {
            return;
        }

        string thumbprint = store.Certificates[0].Thumbprint;
        ToolResult result = await _tool.CertificateVerifyAsync(thumbprint, "Root");

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    public async Task CertificateVerify_NullThumbprint_ReturnsFailure()
    {
        ToolResult result = await _tool.CertificateVerifyAsync(null!);

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CertificateVerify_RootStoreCertificate_ReturnsSuccessfulToolResult()
    {
        // Find any certificate in the LocalMachine Root store to verify
        using System.Security.Cryptography.X509Certificates.X509Store store = new(System.Security.Cryptography.X509Certificates.StoreName.Root, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine);
        store.Open(System.Security.Cryptography.X509Certificates.OpenFlags.ReadOnly | System.Security.Cryptography.X509Certificates.OpenFlags.OpenExistingOnly);

        if (store.Certificates.Count == 0)
        {
            return; // No certificates in Root store on this system
        }

        string thumbprint = store.Certificates[0].Thumbprint;
        ToolResult result = await _tool.CertificateVerifyAsync(thumbprint, "Root");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CertificateVerify_SuccessfulVerification_ContainsCertificateDetails()
    {
        using System.Security.Cryptography.X509Certificates.X509Store store = new(System.Security.Cryptography.X509Certificates.StoreName.Root, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine);
        store.Open(System.Security.Cryptography.X509Certificates.OpenFlags.ReadOnly | System.Security.Cryptography.X509Certificates.OpenFlags.OpenExistingOnly);

        if (store.Certificates.Count == 0)
        {
            return;
        }

        string thumbprint = store.Certificates[0].Thumbprint;
        ToolResult result = await _tool.CertificateVerifyAsync(thumbprint, "Root");

        Assert.True(result.Success);
        object payload = result.Results!;
        Assert.NotNull(payload.GetType().GetProperty("Subject"));
        Assert.NotNull(payload.GetType().GetProperty("Thumbprint"));
        Assert.NotNull(payload.GetType().GetProperty("IsExpired"));
        Assert.NotNull(payload.GetType().GetProperty("IsNotYetValid"));
    }








    [Fact]
    public async Task CertificateVerify_WhitespaceThumbprint_ReturnsFailure()
    {
        ToolResult result = await _tool.CertificateVerifyAsync("   ");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }
}