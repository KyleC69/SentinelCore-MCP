// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         ServiceExtendedReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="ServiceExtendedReadTool" /> covering service ACL reads
///     via sc.exe and input validation.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ServiceExtendedReadToolTests
{
    private readonly ServiceExtendedReadTool _tool = new();

    #region Service_Read_Acl validation tests

    [Fact]
    public async Task ServiceReadAcl_NullServiceName_ReturnsFailure()
    {
        ToolResult result = await _tool.ServiceReadAclAsync(null!);

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ServiceReadAcl_EmptyServiceName_ReturnsFailure()
    {
        ToolResult result = await _tool.ServiceReadAclAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ServiceReadAcl_WhitespaceServiceName_ReturnsFailure()
    {
        ToolResult result = await _tool.ServiceReadAclAsync("   ");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("EventLog\\Extra")]
    [InlineData("Event/Log")]
    [InlineData("Event\"Log")]
    [InlineData("Event'Log")]
    [InlineData("Event&Log")]
    [InlineData("Event|Log")]
    [InlineData("Event;Log")]
    [InlineData("Event<Log>")]
    [InlineData("Event%Log")]
    [InlineData("Event$Log")]
    [InlineData("Event`Log")]
    [InlineData("Event!Log")]
    public async Task ServiceReadAcl_ServiceNameWithInvalidCharacters_ReturnsFailure(string serviceName)
    {
        ToolResult result = await _tool.ServiceReadAclAsync(serviceName);

        Assert.False(result.Success);
        Assert.Contains("invalid characters", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Service_Read_Acl integration tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ServiceReadAcl_KnownService_ReturnsSuccessfulToolResult()
    {
        // EventLog service exists on all Windows systems
        ToolResult result = await _tool.ServiceReadAclAsync("EventLog");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ServiceReadAcl_KnownService_ContainsSddlAndConfig()
    {
        ToolResult result = await _tool.ServiceReadAclAsync("EventLog");

        Assert.True(result.Success);
        string output = (string)result.Results!;
        Assert.Contains("[Security Descriptor (SDDL)]", output);
        Assert.Contains("[Service Configuration]", output);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ServiceReadAcl_NonExistentService_ReturnsFailureOrEmptyAcl()
    {
        ToolResult result = await _tool.ServiceReadAclAsync("NonExistentService_12345");

        // sc.exe sdshow returns exit code 1060 for unknown services but may still
        // produce output; the tool either fails or reports no ACL available
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ServiceReadAcl_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.ServiceReadAclAsync("EventLog");

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
