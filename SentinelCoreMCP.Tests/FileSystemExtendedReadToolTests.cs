// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         FileSystemExtendedReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="FileSystemExtendedReadTool" /> covering file hashing,
///     alternate data stream listing, and hosts file reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class FileSystemExtendedReadToolTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _tempFile;
    private readonly FileSystemExtendedReadTool _tool = new();








    public FileSystemExtendedReadToolTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"SentinelCoreTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _tempFile = Path.Combine(_tempDir, "sample.txt");
        File.WriteAllText(_tempFile, "hello world");
    }








    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Ignore cleanup failures
        }
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemComputeHash_DefaultAlgorithm_IsSha256()
    {
        ToolResult result = await _tool.FileSystemComputeHashAsync(_tempFile);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        object payload = result.Results!;
        Assert.Equal("SHA256", (string)payload.GetType().GetProperty("Algorithm")!.GetValue(payload)!);
        // SHA-256 hash is 64 hex characters
        string hash = (string)payload.GetType().GetProperty("Hash")!.GetValue(payload)!;
        Assert.Equal(64, hash.Length);
    }








    [Fact]
    public async Task FileSystemComputeHash_EmptyPath_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemComputeHashAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Theory]
    [InlineData("MD5", "5EB63BBBE01EEED093CB22BB8F5ACDC3")]
    [InlineData("SHA1", "2AAE6C35C94FCFB415DBE95F408B9CE91EE846ED")]
    [InlineData("SHA256", "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9")]
    public async Task FileSystemComputeHash_KnownAlgorithms_ReturnsExpectedHash(string algorithm, string expectedHash)
    {
        // "hello world" hashes are well-known constants
        ToolResult result = await _tool.FileSystemComputeHashAsync(_tempFile, algorithm: algorithm);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        object payload = result.Results!;
        string hash = (string)payload.GetType().GetProperty("Hash")!.GetValue(payload)!;
        Assert.Equal(expectedHash, hash, ignoreCase: true);
        Assert.Equal(algorithm, (string)payload.GetType().GetProperty("Algorithm")!.GetValue(payload)!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemComputeHash_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.FileSystemComputeHashAsync(_tempFile);

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemComputeHash_UnknownAlgorithm_FallsBackToSha256()
    {
        ToolResult result = await _tool.FileSystemComputeHashAsync(_tempFile, algorithm: "NOT-AN-ALGO");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        object payload = result.Results!;
        // The tool echoes the requested algorithm name but computes with SHA-256 (64 hex chars)
        string hash = (string)payload.GetType().GetProperty("Hash")!.GetValue(payload)!;
        Assert.Equal(64, hash.Length);
    }








    [Fact]
    public async Task FileSystemListStreams_EmptyPath_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemListStreamsAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemListStreams_ExistingFile_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.FileSystemListStreamsAsync(_tempFile);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
        // Results is a List<object> of stream records (may be empty)
        Assert.IsType<List<object>>(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemListStreams_NonExistentPath_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemListStreamsAsync(Path.Combine(_tempDir, "DoesNotExist"));

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadHosts_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.FileSystemReadHostsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadHosts_ReturnsEntryList()
    {
        ToolResult result = await _tool.FileSystemReadHostsAsync();

        Assert.True(result.Success);
        // Results is a List<object> of host entries
        Assert.IsType<List<object>>(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadHosts_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.FileSystemReadHostsAsync();

        // The hosts file exists on all Windows systems
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }
}