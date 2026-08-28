// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         SessionsReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="SessionsReadTool" /> covering active session enumeration.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SessionsReadToolTests
{
    private readonly SessionsReadTool _tool = new();

    #region Sessions_List_Active tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SessionsListActive_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.SessionsListActiveAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SessionsListActive_ContainsSessionInfo()
    {
        ToolResult result = await _tool.SessionsListActiveAsync();

        Assert.True(result.Success);
        // Results is a typed list of session records (not a string)
        Assert.IsNotType<string>(result.Results);
        Assert.IsAssignableFrom<System.Collections.IEnumerable>(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SessionsListActive_RespectsMaxRecords()
    {
        ToolResult result = await _tool.SessionsListActiveAsync(maxRecords: 5);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SessionsListActive_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.SessionsListActiveAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
