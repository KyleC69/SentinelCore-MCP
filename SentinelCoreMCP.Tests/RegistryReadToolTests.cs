// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         RegistryReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Reflection;
using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="RegistryReadTool" /> covering hive validation,
///     key listing, value reading, and format helpers.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RegistryReadToolTests
{
    private readonly RegistryReadTool _tool = new();

    private static readonly Type RegistryHelperType = typeof(RegistryReadTool).Assembly.GetType("SentinelCoreMCP.Tools.RegistryHelper") ?? throw new InvalidOperationException("RegistryHelper type not found.");








    [Fact]
    public void FormatRegistryValue_BinaryValue_ReturnsHex()
    {
        string? result = InvokeFormatRegistryValue(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, (int)Microsoft.Win32.RegistryValueKind.Binary);
        Assert.Equal("DEADBEEF", result);
    }








    [Fact]
    public void FormatRegistryValue_DWordValue_ReturnsString()
    {
        string? result = InvokeFormatRegistryValue(42, (int)Microsoft.Win32.RegistryValueKind.DWord);
        Assert.Equal("42", result);
    }








    [Fact]
    public void FormatRegistryValue_MultiStringValue_ReturnsPipeDelimited()
    {
        string? result = InvokeFormatRegistryValue(new[] { "a", "b", "c" }, (int)Microsoft.Win32.RegistryValueKind.MultiString);
        Assert.Equal("a|b|c", result);
    }








    [Fact]
    public void FormatRegistryValue_NullValue_ReturnsEmptyString()
    {
        string? result = InvokeFormatRegistryValue(null!, (int)Microsoft.Win32.RegistryValueKind.String);
        Assert.Equal(string.Empty, result);
    }








    [Fact]
    public void FormatRegistryValue_StringValue_ReturnsString()
    {
        string? result = InvokeFormatRegistryValue("test value", (int)Microsoft.Win32.RegistryValueKind.String);
        Assert.Equal("test value", result);
    }








    [Theory]
    [InlineData("INVALID")]
    [InlineData("hklmx")]
    [InlineData("")]
    [InlineData("XYZ")]
    public void GetHiveRoot_InvalidHives_ReturnsNull(string hive)
    {
        object? result = InvokeGetHiveRoot(hive);
        Assert.Null(result);
    }








    [Theory]
    [InlineData("HKLM")]
    [InlineData("hklm")]
    [InlineData("HKCU")]
    [InlineData("hkcu")]
    [InlineData("HKCR")]
    [InlineData("HKU")]
    [InlineData("HKCC")]
    public void GetHiveRoot_ValidHives_ReturnsRegistryKey(string hive)
    {
        object? result = InvokeGetHiveRoot(hive);
        Assert.NotNull(result);
    }








    private static string? InvokeFormatRegistryValue(object value, int kind)
    {
        MethodInfo? method = RegistryHelperType.GetMethod("FormatRegistryValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        return (string?)method.Invoke(null, [value, (Microsoft.Win32.RegistryValueKind)kind]);
    }








    private static object? InvokeGetHiveRoot(string hive)
    {
        MethodInfo? method = RegistryHelperType.GetMethod("GetHiveRoot", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        return method.Invoke(null, [hive]);
    }








    [Fact]
    public async Task RegistryListKey_EmptyHive_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryListKeyAsync("", "SOFTWARE\\Microsoft");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    public async Task RegistryListKey_EmptyKeyPath_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryListKeyAsync("HKLM", "");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryListKey_HkcuHive_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.RegistryListKeyAsync("HKCU", "SOFTWARE");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    public async Task RegistryListKey_InvalidHive_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryListKeyAsync("INVALID", "SOFTWARE\\Microsoft");

        Assert.False(result.Success);
        Assert.Contains("hive", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryListKey_NonExistentKey_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryListKeyAsync("HKLM", "SOFTWARE\\NonExistentPath12345");

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryListKey_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.RegistryListKeyAsync("HKLM", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion");

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    public async Task RegistryListKey_NullHive_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryListKeyAsync(null!, "SOFTWARE\\Microsoft");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryListKey_RespectsMaxRecords()
    {
        const int maxRecords = 5;
        ToolResult result = await _tool.RegistryListKeyAsync("HKLM", "SOFTWARE", maxRecords: maxRecords);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        RegistryReadTool.RegistryKeyListingRecord listing = Assert.IsType<RegistryReadTool.RegistryKeyListingRecord>(result.Results);
        Assert.True(listing.SubKeys.Count <= maxRecords, $"Expected at most {maxRecords} subkeys but got {listing.SubKeys.Count}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryListKey_ValidHklmKey_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.RegistryListKeyAsync("HKLM", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryListKey_ValidKey_ReturnsTypedListingRecord()
    {
        ToolResult result = await _tool.RegistryListKeyAsync("HKLM", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion");

        Assert.True(result.Success);
        RegistryReadTool.RegistryKeyListingRecord listing = Assert.IsType<RegistryReadTool.RegistryKeyListingRecord>(result.Results);
        Assert.Equal("HKLM", listing.Hive);
        Assert.NotNull(listing.SubKeys);
        Assert.NotNull(listing.Values);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryReadValue_DefaultValue_ReturnsSuccessfulOrNotFound()
    {
        ToolResult result = await _tool.RegistryReadValueAsync("HKLM", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion");

        // Default value may or may not exist; both outcomes are acceptable
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    public async Task RegistryReadValue_EmptyHive_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryReadValueAsync("", "SOFTWARE\\Microsoft");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    public async Task RegistryReadValue_EmptyKeyPath_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryReadValueAsync("HKLM", "");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    public async Task RegistryReadValue_InvalidHive_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryReadValueAsync("INVALID", "SOFTWARE\\Microsoft");

        Assert.False(result.Success);
        Assert.Contains("hive", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryReadValue_NonExistentValue_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryReadValueAsync("HKLM", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion", "NonExistentValue_12345");

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryReadValue_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.RegistryReadValueAsync("HKLM", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion", "ProgramFilesDir");

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryReadValue_ValidKeyValue_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.RegistryReadValueAsync("HKLM", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion", "ProgramFilesDir");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryReadValue_ValidKeyValue_ReturnsTypedRecord()
    {
        ToolResult result = await _tool.RegistryReadValueAsync("HKLM", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion", "ProgramFilesDir");

        Assert.True(result.Success);
        RegistryReadTool.RegistryValueReadRecord record = Assert.IsType<RegistryReadTool.RegistryValueReadRecord>(result.Results);
        Assert.Equal("HKLM", record.Hive);
        Assert.Equal("ProgramFilesDir", record.ValueName);
        Assert.False(string.IsNullOrEmpty(record.Value));
    }
}