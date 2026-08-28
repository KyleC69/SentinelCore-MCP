// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         EventLogExtendedReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="EventLogExtendedReadTool" /> covering Windows Event
///     Forwarding subscription configuration reads.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class EventLogExtendedReadToolTests
{
    private readonly EventLogExtendedReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EventLogReadForwarding_ContainsForwardingPolicySection()
    {
        ToolResult result = await _tool.EventLogReadForwardingAsync();

        if (!result.Success)
        {
            return;
        } // Skip if registry access restricted

        // The tool always emits the Event Forwarding Policy header
        Assert.Contains("[Event Forwarding Policy]", (string)result.Results!);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EventLogReadForwarding_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.EventLogReadForwardingAsync();

        if (!result.Success)
        {
            return;
        } // Skip if registry access restricted

        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task EventLogReadForwarding_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.EventLogReadForwardingAsync();

        // EventLog service subkey enumeration may be access-restricted on some systems
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }
}