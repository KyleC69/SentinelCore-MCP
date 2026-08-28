// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         HyperVReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="HyperVReadTool" /> covering Hyper-V switch, VM, and
///     VM detail queries via the virtualization WMI namespace.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class HyperVReadToolTests
{
    private readonly HyperVReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task HypervListSwitches_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.HypervListSwitchesAsync();

        if (!result.Success)
        {
            return;
        } // Skip if Hyper-V not installed

        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task HypervListSwitches_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.HypervListSwitchesAsync();

        // Hyper-V is typically not installed; the tool should fail gracefully
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task HypervListVms_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.HypervListVmsAsync();

        // Hyper-V is typically not installed; the tool should fail gracefully
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    public async Task HypervReadVm_EmptyVmName_ReturnsFailure()
    {
        ToolResult result = await _tool.HypervReadVmAsync("");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task HypervReadVm_NonExistentVm_ReturnsFailureOrGracefulError()
    {
        ToolResult result = await _tool.HypervReadVmAsync("NonExistentVM_12345");

        // Fails when Hyper-V is not installed or the VM does not exist
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }
}