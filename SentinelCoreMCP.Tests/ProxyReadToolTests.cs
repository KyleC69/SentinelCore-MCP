// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         ProxyReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="ProxyReadTool" /> covering registry-based system proxy reads
///     and netsh-based WinHTTP proxy reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ProxyReadToolTests
{
    private readonly ProxyReadTool _tool = new();

    #region Proxy_Read_System tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProxyReadSystem_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.ProxyReadSystemAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProxyReadSystem_ContainsProxyOrAutoConfigFields()
    {
        ToolResult result = await _tool.ProxyReadSystemAsync();

        Assert.True(result.Success);
        // The output should contain ProxyEnable, ProxyServer, or AutoConfigURL fields
        string output = (string)result.Results!;
        Assert.True(
            output.Contains("Proxy", StringComparison.OrdinalIgnoreCase) ||
            output.Contains("AutoConfig", StringComparison.OrdinalIgnoreCase),
            "Expected proxy or auto-config fields in output");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProxyReadSystem_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.ProxyReadSystemAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region Proxy_Read_WinHTTP tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProxyReadWinhttp_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.ProxyReadWinhttpAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProxyReadWinhttp_ContainsProxyInfo()
    {
        ToolResult result = await _tool.ProxyReadWinhttpAsync();

        Assert.True(result.Success);
        // netsh winhttp show proxy output contains "Proxy" or "Direct access"
        string output = (string)result.Results!;
        Assert.True(
            output.Contains("Proxy", StringComparison.OrdinalIgnoreCase) ||
            output.Contains("Direct", StringComparison.OrdinalIgnoreCase) ||
            output.Contains("access", StringComparison.OrdinalIgnoreCase),
            "Expected proxy or direct access info in output");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProxyReadWinhttp_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.ProxyReadWinhttpAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion
}
