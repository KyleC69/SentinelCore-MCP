// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         RegistryExtendedReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="RegistryExtendedReadTool" /> covering registry ACL reads
///     and COM class enumeration for hijacking detection.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RegistryExtendedReadToolTests
{
    private readonly RegistryExtendedReadTool _tool = new();

    #region Registry_Read_Acl tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryReadAcl_ValidKey_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.RegistryReadAclAsync("HKLM", "SOFTWARE\\Microsoft");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryReadAcl_ValidKey_ReturnsTypedAclPayload()
    {
        ToolResult result = await _tool.RegistryReadAclAsync("HKLM", "SOFTWARE\\Microsoft");

        Assert.True(result.Success);
        // Results is a RegistryAclResult record (internal type, verify via reflection on properties)
        object payload = result.Results!;
        Assert.NotNull(payload.GetType().GetProperty("Path"));
        Assert.NotNull(payload.GetType().GetProperty("Owner"));
        Assert.NotNull(payload.GetType().GetProperty("AccessRules"));
    }

    [Fact]
    public async Task RegistryReadAcl_EmptyHive_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryReadAclAsync("", "SOFTWARE\\Microsoft");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegistryReadAcl_InvalidHive_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryReadAclAsync("INVALID", "SOFTWARE\\Microsoft");

        Assert.False(result.Success);
        Assert.Contains("hive", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegistryReadAcl_EmptyKeyPath_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryReadAclAsync("HKLM", "");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegistryReadAcl_NonExistentKey_ReturnsFailure()
    {
        ToolResult result = await _tool.RegistryReadAclAsync("HKLM", "SOFTWARE\\NonExistentKey_12345");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }

    #endregion

    #region COM_List_Classes tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ComListClasses_DefaultParameters_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.ComListClassesAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ComListClasses_ReturnsTypedComClassRecords()
    {
        ToolResult result = await _tool.ComListClassesAsync();

        Assert.True(result.Success);
        List<RegistryExtendedReadTool.ComClassRecord> records =
            Assert.IsType<List<RegistryExtendedReadTool.ComClassRecord>>(result.Results);
        Assert.NotEmpty(records);
        // Every record must have a CLSID
        Assert.All(records, r => Assert.False(string.IsNullOrWhiteSpace(r.Clsid)));
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ComListClasses_RespectsMaxRecords()
    {
        const int maxRecords = 5;
        ToolResult result = await _tool.ComListClassesAsync(maxRecords: maxRecords);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        List<RegistryExtendedReadTool.ComClassRecord> records =
            Assert.IsType<List<RegistryExtendedReadTool.ComClassRecord>>(result.Results);
        Assert.True(records.Count <= maxRecords,
            $"Expected at most {maxRecords} COM classes but got {records.Count}");
    }

    [Fact]
    public async Task ComListClasses_ZeroMaxRecords_ReturnsFailure()
    {
        ToolResult result = await _tool.ComListClassesAsync(maxRecords: 0);

        Assert.False(result.Success);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ComListClasses_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.ComListClassesAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
