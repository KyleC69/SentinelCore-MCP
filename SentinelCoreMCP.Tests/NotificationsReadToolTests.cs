// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         NotificationsReadToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="NotificationsReadTool" /> covering notification app
///     enumeration and quiet hours reads from the registry.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class NotificationsReadToolTests
{
    private readonly NotificationsReadTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NotificationListApps_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.notificationListAppsAsync();

        if (!result.Success)
        {
            return;
        }

        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NotificationListApps_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.notificationListAppsAsync();

        // The notification registry key may not exist on all systems
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NotificationListApps_WhenSuccessful_ReturnsDictionaryList()
    {
        ToolResult result = await _tool.notificationListAppsAsync();

        if (!result.Success)
        {
            return;
        } // Skip if registry key unavailable

        Assert.NotNull(result.Results);
        // Results is a List<Dictionary<string, object?>>
        Assert.IsType<List<Dictionary<string, object?>>>(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NotificationReadQuietHours_ReturnsSuccessfulOrGracefulFailure()
    {
        ToolResult result = await _tool.notificationReadQuietHoursAsync();

        // The quiet hours key may not exist on all systems
        Assert.True(result.Success || result.ErrorDetails != null, $"Expected success or graceful failure but got: Success={result.Success}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NotificationReadQuietHours_WhenSuccessful_ContainsKeyValuePairs()
    {
        ToolResult result = await _tool.notificationReadQuietHoursAsync();

        if (!result.Success)
        {
            return;
        } // Skip if registry key unavailable

        string output = (string)result.Results!;
        Assert.Contains("=", output);
    }
}