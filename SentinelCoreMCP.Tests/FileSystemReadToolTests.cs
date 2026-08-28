// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         FileSystemReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="FileSystemReadTool" /> covering directory listing,
///     ACL reads, metadata, and content reads with temp file fixtures.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class FileSystemReadToolTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _tempFile;
    private readonly FileSystemReadTool _tool = new();








    public FileSystemReadToolTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"SentinelCoreTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        Directory.CreateDirectory(Path.Combine(_tempDir, "SubDir"));
        _tempFile = Path.Combine(_tempDir, "test.txt");
        File.WriteAllLines(_tempFile, ["line one", "line two", "line three", "line four", "line five"]);
    }








    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Best-effort cleanup
        }
    }








    [Fact]
    public async Task FileSystemListDirectory_EmptyPath_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemListDirectoryAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemListDirectory_NonExistentPath_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemListDirectoryAsync(Path.Combine(_tempDir, "DoesNotExist"));

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    public async Task FileSystemListDirectory_NullPath_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemListDirectoryAsync(null!);

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemListDirectory_RespectsMaxRecords()
    {
        const int maxRecords = 1;
        ToolResult result = await _tool.FileSystemListDirectoryAsync(_tempDir, maxRecords: maxRecords);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        string output = (string)result.Results!;
        // With maxRecords=1, only one entry (the subdirectory) should be listed
        int entryCount = output.Split('\n').Count(l => l.StartsWith("  "));
        Assert.True(entryCount <= maxRecords, $"Expected at most {maxRecords} entries but got {entryCount}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemListDirectory_TempDirectory_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.FileSystemListDirectoryAsync(_tempDir);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        string output = (string)result.Results!;
        Assert.Contains("Directories:", output);
        Assert.Contains("Files:", output);
    }








    [Fact]
    public async Task FileSystemReadAcl_EmptyPath_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemReadAclAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadAcl_NonExistentPath_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemReadAclAsync(Path.Combine(_tempDir, "DoesNotExist"));

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadAcl_TempDirectory_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.FileSystemReadAclAsync(_tempDir);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        string output = (string)result.Results!;
        Assert.Contains("Owner=", output);
        Assert.Contains("AccessRules:", output);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadAcl_TempFile_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.FileSystemReadAclAsync(_tempFile);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        string output = (string)result.Results!;
        Assert.Contains("Owner=", output);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadContent_BinaryFile_ReturnsFailure()
    {
        string binaryFile = Path.Combine(_tempDir, "binary.bin");
        await File.WriteAllBytesAsync(binaryFile, [0x00, 0x01, 0x02, 0x00, 0x03]);

        ToolResult result = await _tool.FileSystemReadContentAsync(binaryFile);

        Assert.False(result.Success);
        Assert.Contains("binary", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadContent_DirectoryPath_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemReadContentAsync(_tempDir);

        // The tool checks FileInfo.Exists first; a directory path fails as "not found"
        // or as "is a directory" depending on which check fires first
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }








    [Fact]
    public async Task FileSystemReadContent_EmptyPath_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemReadContentAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadContent_NonExistentFile_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemReadContentAsync(Path.Combine(_tempDir, "DoesNotExist.txt"));

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadContent_StartLineBeyondEnd_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemReadContentAsync(_tempFile, startLine: 100);

        Assert.False(result.Success);
        Assert.Contains("exceeds total line count", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadContent_TempFile_ReturnsContent()
    {
        ToolResult result = await _tool.FileSystemReadContentAsync(_tempFile);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        string output = (string)result.Results!;
        Assert.Contains("line two", output);
        Assert.Contains("Total lines: 5", output);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadContent_UnsupportedEncoding_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemReadContentAsync(_tempFile, encoding: "not-a-real-encoding");

        Assert.False(result.Success);
        Assert.Contains("encoding", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadContent_WithLineRange_ReturnsSubset()
    {
        ToolResult result = await _tool.FileSystemReadContentAsync(_tempFile, startLine: 2, lineCount: 2);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        string output = (string)result.Results!;
        Assert.Contains("line two", output);
        Assert.Contains("line three", output);
        Assert.DoesNotContain("line one", output);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadMetadata_Directory_ReturnsIsDirectoryTrue()
    {
        ToolResult result = await _tool.FileSystemReadMetadataAsync(_tempDir);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        object payload = result.Results!;
        bool isDirectory = (bool)payload.GetType().GetProperty("IsDirectory")!.GetValue(payload)!;
        Assert.True(isDirectory, "Expected IsDirectory=true for a directory path");
    }








    [Fact]
    public async Task FileSystemReadMetadata_EmptyPath_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemReadMetadataAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadMetadata_NonExistentPath_ReturnsFailure()
    {
        ToolResult result = await _tool.FileSystemReadMetadataAsync(Path.Combine(_tempDir, "DoesNotExist"));

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task FileSystemReadMetadata_TempFile_ReturnsTypedMetadata()
    {
        ToolResult result = await _tool.FileSystemReadMetadataAsync(_tempFile);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        object payload = result.Results!;
        Assert.NotNull(payload.GetType().GetProperty("Path"));
        Assert.NotNull(payload.GetType().GetProperty("IsDirectory"));
        Assert.NotNull(payload.GetType().GetProperty("Length"));
    }
}