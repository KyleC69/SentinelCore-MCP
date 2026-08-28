// Solution: SentinelCore-MCP
// Project:   SentinelCoreMCP.Tests
// File:         WmiQueryToolTests.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Reflection;
using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;




namespace SentinelCoreMCP.Tests;





/// <summary>
///     Tests for <see cref="WmiQueryTool" /> covering input validation,
///     query execution, and result formatting.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WmiQueryToolTests
{
    private readonly WmiQueryTool _tool = new();








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WmiListClasses_DefaultParameters_ContainsNamespaceOrCount()
    {
        ToolResult result = await _tool.WmiListClassesAsync();

        Assert.True(result.Success);
        // Structured result contains either Namespace or Count
        object payload = result.Results!;
        Assert.NotNull(payload.GetType().GetProperty("Namespace"));
        Assert.NotNull(payload.GetType().GetProperty("Count"));
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WmiListClasses_DefaultParameters_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.WmiListClassesAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WmiListClasses_InvalidNamespace_ReturnsFailure()
    {
        ToolResult result = await _tool.WmiListClassesAsync("root\\InvalidNamespace_12345");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WmiListClasses_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.WmiListClassesAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WmiListClasses_RespectsMaxResults()
    {
        const int maxResults = 5;
        ToolResult result = await _tool.WmiListClassesAsync(maxResults: maxResults);

        Assert.True(result.Success);
        object payload = result.Results!;
        PropertyInfo? countProperty = payload.GetType().GetProperty("Count");
        Assert.NotNull(countProperty);
        int count = (int)countProperty.GetValue(payload)!;
        Assert.True(count <= maxResults, $"Expected at most {maxResults} classes but got {count}");
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WmiListClasses_WithPrefix_ReturnsFilteredResults()
    {
        ToolResult result = await _tool.WmiListClassesAsync(prefix: "Win32");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    public async Task WmiQuery_EmptyQuery_ReturnsFailure()
    {
        ToolResult result = await _tool.WmiQueryAsync("");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WmiQuery_InvalidQuery_ReturnsFailure()
    {
        ToolResult result = await _tool.WmiQueryAsync("INVALID WQL SYNTAX HERE");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }








    [Fact]
    public async Task WmiQuery_NonSelectQuery_ReturnsFailure()
    {
        ToolResult result = await _tool.WmiQueryAsync("DELETE FROM Win32_Process");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WmiQuery_NullErrorDetailsOnSuccess()
    {
        ToolResult result = await _tool.WmiQueryAsync("SELECT Name FROM Win32_OperatingSystem");

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }








    [Fact]
    public async Task WmiQuery_NullQuery_ReturnsFailure()
    {
        ToolResult result = await _tool.WmiQueryAsync(null!);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WmiQuery_RespectsMaxProperties()
    {
        ToolResult result = await _tool.WmiQueryAsync("SELECT * FROM Win32_OperatingSystem", maxProperties: 3);

        Assert.True(result.Success);
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WmiQuery_RespectsMaxRows()
    {
        ToolResult result = await _tool.WmiQueryAsync("SELECT * FROM Win32_Process", maxRows: 3);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WmiQuery_ValidQuery_ContainsQueryField()
    {
        ToolResult result = await _tool.WmiQueryAsync("SELECT Name FROM Win32_OperatingSystem");

        Assert.True(result.Success);
        object payload = result.Results!;
        Assert.NotNull(payload.GetType().GetProperty("Query"));
    }








    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WmiQuery_ValidQuery_ReturnsSuccessfulToolResult()
    {
        ToolResult result = await _tool.WmiQueryAsync("SELECT Name, Status FROM Win32_OperatingSystem");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }








    [Fact]
    public async Task WmiQuery_WhitespaceQuery_ReturnsFailure()
    {
        ToolResult result = await _tool.WmiQueryAsync("   ");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }
}