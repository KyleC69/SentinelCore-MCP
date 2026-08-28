// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         PowerShellReadToolTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for <see cref="PowerShellReadTool" /> covering command validation
///     (whitelist, blacklist, injection patterns) and integration execution.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class PowerShellReadToolTests
{
    #region ValidateCommand — Whitelist acceptance tests

    [Theory]
    [InlineData("Get-Process")]
    [InlineData("Get-Service")]
    [InlineData("Get-ComputerInfo")]
    [InlineData("Get-CimInstance")]
    [InlineData("Get-NetAdapter")]
    [InlineData("Get-NetIPAddress")]
    [InlineData("Get-NetFirewallRule")]
    [InlineData("Get-LocalUser")]
    [InlineData("Get-LocalGroup")]
    [InlineData("Get-ScheduledTask")]
    [InlineData("Get-HotFix")]
    [InlineData("Get-Date")]
    [InlineData("Get-Host")]
    [InlineData("Get-Volume")]
    [InlineData("Get-Partition")]
    [InlineData("Get-Disk")]
    [InlineData("Get-AppLockerPolicy")]
    [InlineData("Get-Acl")]
    [InlineData("Get-ExecutionPolicy")]
    [InlineData("Get-FileHash")]
    [InlineData("Get-AuthenticodeSignature")]
    [InlineData("Select-Object")]
    [InlineData("Where-Object")]
    [InlineData("Sort-Object")]
    [InlineData("Format-List")]
    [InlineData("ConvertTo-Json")]
    [InlineData("ipconfig")]
    [InlineData("netstat")]
    [InlineData("arp")]
    [InlineData("route")]
    [InlineData("pnputil")]
    [InlineData("auditpol")]
    [InlineData("systeminfo")]
    [InlineData("query")]
    public void ValidateCommand_AllowedCommands_ReturnsNull(string command)
    {
        string? result = PowerShellReadTool.ValidateCommand(command);
        Assert.Null(result);
    }

    #endregion

    #region ValidateCommand — Blacklist rejection tests

    [Theory]
    [InlineData("Set-Content")]
    [InlineData("Remove-Item")]
    [InlineData("New-Item")]
    [InlineData("Invoke-Command")]
    [InlineData("Invoke-Expression")]
    [InlineData("Invoke-WebRequest")]
    [InlineData("Start-Process")]
    [InlineData("Stop-Process")]
    [InlineData("Restart-Computer")]
    [InlineData("Stop-Computer")]
    [InlineData("New-Object")]
    [InlineData("Add-Type")]
    [InlineData("Enter-PSSession")]
    [InlineData("New-PSSession")]
    [InlineData("Write-Host")]
    [InlineData("Write-Output")]
    [InlineData("Out-File")]
    [InlineData("netsh")]
    [InlineData("cmd")]
    [InlineData("cmd.exe")]
    [InlineData("powershell")]
    [InlineData("pwsh")]
    [InlineData("reg")]
    [InlineData("reg.exe")]
    [InlineData("sc")]
    [InlineData("sc.exe")]
    [InlineData("schtasks")]
    [InlineData("shutdown")]
    [InlineData("Enable-NetFirewallRule")]
    [InlineData("Disable-NetFirewallRule")]
    [InlineData("New-NetFirewallRule")]
    [InlineData("Remove-NetFirewallRule")]
    [InlineData("Register-ScheduledTask")]
    [InlineData("Unregister-ScheduledTask")]
    public void ValidateCommand_ForbiddenCommands_ReturnsError(string command)
    {
        string? result = PowerShellReadTool.ValidateCommand(command);
        Assert.NotNull(result);
        Assert.Contains("forbidden", result, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region ValidateCommand — Injection pattern rejection tests

    [Theory]
    [InlineData("Get-Process | Stop-Process")]
    [InlineData("Get-Process > output.txt")]
    [InlineData("Get-Process >> output.txt")]
    [InlineData("Get-Process ; Remove-Item C:\\")]
    [InlineData("Get-Process & whoami")]
    [InlineData("Get-Process $(whoami)")]
    [InlineData("Get-Process [System.Diagnostics.Process]::Start('cmd')")]
    [InlineData("New-Object System.Net.WebClient")]
    [InlineData("Add-Type -TypeDefinition '...'")]
    [InlineData("Get-Process | Where-Object { $_.Name -eq 'explorer' }")]
    public void ValidateCommand_InjectionPatterns_ReturnsError(string command)
    {
        string? result = PowerShellReadTool.ValidateCommand(command);
        Assert.NotNull(result);
        Assert.Contains("forbidden pattern", result, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region ValidateCommand — Edge case tests

    [Fact]
    public void ValidateCommand_NullCommand_ReturnsRequiredError()
    {
        string? result = PowerShellReadTool.ValidateCommand(null!);
        Assert.NotNull(result);
        Assert.Contains("required", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateCommand_EmptyCommand_ReturnsRequiredError()
    {
        string? result = PowerShellReadTool.ValidateCommand("");
        Assert.NotNull(result);
        Assert.Contains("required", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateCommand_WhitespaceCommand_ReturnsRequiredError()
    {
        string? result = PowerShellReadTool.ValidateCommand("   ");
        Assert.NotNull(result);
        Assert.Contains("required", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateCommand_UnknownCmdlet_ReturnsNotAllowedError()
    {
        string? result = PowerShellReadTool.ValidateCommand("Get-DangerousThing");
        Assert.NotNull(result);
        Assert.Contains("not in the allowed list", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateCommand_AllowedCommandWithParameters_ReturnsNull()
    {
        string? result = PowerShellReadTool.ValidateCommand("Get-Process -Name explorer");
        Assert.Null(result);
    }

    [Fact]
    public void ValidateCommand_AllowedCommandWithMultipleParameters_ReturnsNull()
    {
        string? result = PowerShellReadTool.ValidateCommand("Get-CimInstance -ClassName Win32_OperatingSystem");
        Assert.Null(result);
    }

    #endregion

    #region PowerShell_Query integration tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    [Trait("Category", "RequiresPowerShell")]
    public async Task PowerShellQuery_GetDate_ReturnsSuccessfulToolResult()
    {
        PowerShellReadTool tool = new();
        ToolResult result = await tool.powershellQueryAsync("Get-Date");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    [Trait("Category", "RequiresPowerShell")]
    public async Task PowerShellQuery_GetProcess_ReturnsSuccessfulToolResult()
    {
        PowerShellReadTool tool = new();
        ToolResult result = await tool.powershellQueryAsync("Get-Process -Name explorer");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    [Trait("Category", "RequiresPowerShell")]
    public async Task PowerShellQuery_GetComputerInfo_ReturnsSuccessfulToolResult()
    {
        PowerShellReadTool tool = new();
        ToolResult result = await tool.powershellQueryAsync("Get-ComputerInfo");

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PowerShellQuery_ForbiddenCommand_ReturnsFailure()
    {
        PowerShellReadTool tool = new();
        ToolResult result = await tool.powershellQueryAsync("Remove-Item C:\\test");

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorDetails);
        Assert.Contains("forbidden", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PowerShellQuery_InjectionAttempt_ReturnsFailure()
    {
        PowerShellReadTool tool = new();
        ToolResult result = await tool.powershellQueryAsync("Get-Process ; whoami");

        Assert.False(result.Success);
        Assert.Contains("forbidden pattern", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    [Trait("Category", "RequiresPowerShell")]
    public async Task PowerShellQuery_MaxResultsParameter_RespectsLimit()
    {
        PowerShellReadTool tool = new();
        ToolResult result = await tool.powershellQueryAsync("Get-Service", maxResults: 3);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PowerShellQuery_EmptyCommand_ReturnsFailure()
    {
        PowerShellReadTool tool = new();
        ToolResult result = await tool.powershellQueryAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region PowerShell_List_Allowed_Commands tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PowerShellListAllowedCommands_ReturnsSuccessfulToolResult()
    {
        PowerShellReadTool tool = new();
        ToolResult result = await tool.powershellListAllowedCommandsAsync();

        Assert.True(result.Success);
        Assert.NotNull(result.Results);
        Assert.Contains("Get-Process", (string)result.Results!);
        Assert.Contains("Get-Service", (string)result.Results);
        Assert.Contains("Get-CimInstance", (string)result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PowerShellListAllowedCommands_DoesNotContainForbiddenCommands()
    {
        PowerShellReadTool tool = new();
        ToolResult result = await tool.powershellListAllowedCommandsAsync();

        Assert.True(result.Success);
        string output = (string)result.Results!;
        Assert.DoesNotContain("Remove-Item", output);
        Assert.DoesNotContain("Invoke-Expression", output);
        Assert.DoesNotContain("Start-Process", output);
    }

    #endregion
}
