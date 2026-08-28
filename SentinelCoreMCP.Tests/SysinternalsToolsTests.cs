// Solution: SentinelCore
// Project:   SentinelCoreMCP.Tests
// File:         SysinternalsToolsTests.cs
// Author: Kyle L. Crowder

using System.Runtime.Versioning;

using SentinelCoreMCP.Tools;

namespace SentinelCoreMCP.Tests;

/// <summary>
///     Tests for the Sysinternals read-only tool suite. Sysinternals binaries are
///     optional on the host, so every tool must either succeed or fail gracefully
///     with a structured error (spec §2.4). Availability probes must always succeed.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SysinternalsToolsTests
{
    #region AccessChk tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AccessChkAvailability_AlwaysSucceeds()
    {
        AccessChkReadTool tool = new();
        ToolResult result = await tool.AccessChkAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AccessChkReadPermissions_EmptyTarget_ReturnsFailure()
    {
        AccessChkReadTool tool = new();
        ToolResult result = await tool.AccessChkReadPermissionsAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AccessChkReadPermissions_DangerousTarget_ReturnsFailure()
    {
        AccessChkReadTool tool = new();
        ToolResult result = await tool.AccessChkReadPermissionsAsync("C:\\test\" & whoami");

        Assert.False(result.Success);
        Assert.Contains("not permitted", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AccessChkReadPermissions_ZeroMaxLines_ReturnsFailure()
    {
        AccessChkReadTool tool = new();
        ToolResult result = await tool.AccessChkReadPermissionsAsync("C:\\Windows", maxLines: 0);

        Assert.False(result.Success);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AccessChkReadPermissions_ReturnsSuccessfulOrGracefulFailure()
    {
        AccessChkReadTool tool = new();
        ToolResult result = await tool.AccessChkReadPermissionsAsync("C:\\Windows");

        // AccessChk may not be installed; the tool must fail gracefully
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region AccessEnum tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task AccessEnumAvailability_AlwaysSucceeds()
    {
        AccessEnumReadTool tool = new();
        ToolResult result = await tool.AccessEnumAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    #endregion

    #region Active Directory tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ActiveDirectoryAvailability_AlwaysSucceedsOrFailsGracefully()
    {
        ActiveDirectoryReadTool tool = new();
        ToolResult result = await tool.ActiveDirectoryAvailabilityAsync();

        // Domain-joined or not, the probe must never throw
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ActiveDirectoryBrowseContainer_InvalidLdapPath_ReturnsFailure()
    {
        ActiveDirectoryReadTool tool = new();
        ToolResult result = await tool.ActiveDirectoryBrowseContainerAsync("not-a-ldap-path");

        Assert.False(result.Success);
        Assert.Contains("LDAP://", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ActiveDirectoryBrowseContainer_ZeroMaxRecords_ReturnsFailure()
    {
        ActiveDirectoryReadTool tool = new();
        ToolResult result = await tool.ActiveDirectoryBrowseContainerAsync(maxRecords: 0);

        Assert.False(result.Success);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ActiveDirectoryBrowseContainer_ReturnsSuccessfulOrGracefulFailure()
    {
        ActiveDirectoryReadTool tool = new();
        ToolResult result = await tool.ActiveDirectoryBrowseContainerAsync();

        // Non-domain-joined hosts fail gracefully
        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ADInsightAvailability_AlwaysSucceeds()
    {
        ActiveDirectoryReadTool tool = new();
        ToolResult result = await tool.ADInsightAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    #endregion

    #region CoreInfo tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CoreInfoAvailability_AlwaysSucceeds()
    {
        CoreInfoReadTool tool = new();
        ToolResult result = await tool.CoreInfoAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CoreInfoReadSystem_ZeroMaxLines_ReturnsFailure()
    {
        CoreInfoReadTool tool = new();
        ToolResult result = await tool.CoreInfoReadSystemAsync(maxLines: 0);

        Assert.False(result.Success);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task CoreInfoReadSystem_ReturnsSuccessfulOrGracefulFailure()
    {
        CoreInfoReadTool tool = new();
        ToolResult result = await tool.CoreInfoReadSystemAsync();

        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region Handle tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task HandleAvailability_AlwaysSucceeds()
    {
        HandleReadTool tool = new();
        ToolResult result = await tool.HandleAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task HandleList_DangerousFilter_ReturnsFailure()
    {
        HandleReadTool tool = new();
        ToolResult result = await tool.HandleListAsync("test\" & whoami");

        Assert.False(result.Success);
        Assert.Contains("not permitted", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task HandleList_ReturnsSuccessfulOrGracefulFailure()
    {
        HandleReadTool tool = new();
        ToolResult result = await tool.HandleListAsync();

        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region ListDlls tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ListDllsAvailability_AlwaysSucceeds()
    {
        ListDllsReadTool tool = new();
        ToolResult result = await tool.ListDllsAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ListDllsListLoaded_InvalidProcessName_ReturnsFailure()
    {
        ListDllsReadTool tool = new();
        // Asterisks pass the generic argument check but fail the image-name whitelist
        ToolResult result = await tool.ListDllsListLoadedAsync("explorer*");

        Assert.False(result.Success);
        Assert.Contains("must be a numeric PID", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ListDllsListLoaded_ReturnsSuccessfulOrGracefulFailure()
    {
        ListDllsReadTool tool = new();
        ToolResult result = await tool.ListDllsListLoadedAsync();

        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region LogonSessions tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task LogonSessionsAvailability_AlwaysSucceeds()
    {
        LogonSessionsReadTool tool = new();
        ToolResult result = await tool.LogonSessionsAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task LogonSessionsListActive_ReturnsSuccessfulOrGracefulFailure()
    {
        LogonSessionsReadTool tool = new();
        ToolResult result = await tool.LogonSessionsListActiveAsync();

        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region NtfsInfo tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NtfsInfoAvailability_AlwaysSucceeds()
    {
        NtfsInfoReadTool tool = new();
        ToolResult result = await tool.NtfsInfoAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NtfsInfoReadVolume_InvalidDriveLetter_ReturnsFailure()
    {
        NtfsInfoReadTool tool = new();
        ToolResult result = await tool.NtfsInfoReadVolumeAsync("1");

        Assert.False(result.Success);
        Assert.Contains("invalid drive letter", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NtfsInfoReadVolume_MultiCharacterDrive_ReturnsFailure()
    {
        NtfsInfoReadTool tool = new();
        ToolResult result = await tool.NtfsInfoReadVolumeAsync("AB");

        Assert.False(result.Success);
        Assert.Contains("invalid drive letter", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task NtfsInfoReadVolume_ReturnsSuccessfulOrGracefulFailure()
    {
        NtfsInfoReadTool tool = new();
        ToolResult result = await tool.NtfsInfoReadVolumeAsync();

        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region PendMoves tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PendMovesAvailability_AlwaysSucceeds()
    {
        PendMovesReadTool tool = new();
        ToolResult result = await tool.PendMovesAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PendMovesListPending_ReturnsSuccessfulToolResult()
    {
        PendMovesReadTool tool = new();
        ToolResult result = await tool.PendMovesListPendingAsync();

        // The Session Manager key is readable without elevation
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PendMovesListPending_NullErrorDetailsOnSuccess()
    {
        PendMovesReadTool tool = new();
        ToolResult result = await tool.PendMovesListPendingAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorDetails);
    }

    #endregion

    #region PipeList tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PipeListAvailability_AlwaysSucceeds()
    {
        PipeListReadTool tool = new();
        ToolResult result = await tool.PipeListAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PipeListListPipes_ReturnsSuccessfulOrGracefulFailure()
    {
        PipeListReadTool tool = new();
        ToolResult result = await tool.PipeListListPipesAsync();

        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region ProcDump tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcDumpAvailability_AlwaysSucceeds()
    {
        ProcDumpReadTool tool = new();
        ToolResult result = await tool.ProcDumpAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcDumpCaptureDump_ZeroPid_ReturnsFailure()
    {
        ProcDumpReadTool tool = new();
        ToolResult result = await tool.ProcDumpCaptureDumpAsync(0, "C:\\temp\\test.dmp");

        Assert.False(result.Success);
        Assert.Contains("positive", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcDumpCaptureDump_RelativePath_ReturnsFailure()
    {
        ProcDumpReadTool tool = new();
        ToolResult result = await tool.ProcDumpCaptureDumpAsync(1234, "relative.dmp");

        Assert.False(result.Success);
        Assert.Contains("absolute path", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcDumpCaptureDump_NonDmpExtension_ReturnsFailure()
    {
        ProcDumpReadTool tool = new();
        ToolResult result = await tool.ProcDumpCaptureDumpAsync(1234, "C:\\temp\\test.txt");

        Assert.False(result.Success);
        Assert.Contains(".dmp", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcDumpCaptureDump_NonExistentDirectory_ReturnsFailure()
    {
        ProcDumpReadTool tool = new();
        ToolResult result = await tool.ProcDumpCaptureDumpAsync(1234, "C:\\__no_such_dir_12345__\\test.dmp");

        Assert.False(result.Success);
        Assert.Contains("does not exist", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region ProcessExplorer tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ProcessExplorerAvailability_AlwaysSucceeds()
    {
        ProcessExplorerReadTool tool = new();
        ToolResult result = await tool.ProcessExplorerAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    #endregion

    #region PsInfo tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsInfoAvailability_AlwaysSucceeds()
    {
        PsInfoReadTool tool = new();
        ToolResult result = await tool.PsInfoAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsInfoReadSystem_ReturnsSuccessfulOrGracefulFailure()
    {
        PsInfoReadTool tool = new();
        ToolResult result = await tool.PsInfoReadSystemAsync();

        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region PsList tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsListAvailability_AlwaysSucceeds()
    {
        PsListReadTool tool = new();
        ToolResult result = await tool.PsListAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsListListProcesses_InvalidProcessName_ReturnsFailure()
    {
        PsListReadTool tool = new();
        // Asterisks pass the generic argument check but fail the image-name whitelist
        ToolResult result = await tool.PsListListProcessesAsync("explorer*");

        Assert.False(result.Success);
        Assert.Contains("must be a numeric PID", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsListListProcesses_ReturnsSuccessfulOrGracefulFailure()
    {
        PsListReadTool tool = new();
        ToolResult result = await tool.PsListListProcessesAsync();

        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region PsLogList tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsLogListAvailability_AlwaysSucceeds()
    {
        PsLogListReadTool tool = new();
        ToolResult result = await tool.PsLogListAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsLogListDumpEvents_EmptyLogName_ReturnsFailure()
    {
        PsLogListReadTool tool = new();
        ToolResult result = await tool.PsLogListDumpEventsAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsLogListDumpEvents_InvalidLogName_ReturnsFailure()
    {
        PsLogListReadTool tool = new();
        // Asterisks pass the generic argument check but fail the log-name whitelist
        ToolResult result = await tool.PsLogListDumpEventsAsync("Application*");

        Assert.False(result.Success);
        Assert.Contains("must contain only", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsLogListDumpEvents_ZeroMaxEvents_ReturnsFailure()
    {
        PsLogListReadTool tool = new();
        ToolResult result = await tool.PsLogListDumpEventsAsync("Application", maxEvents: 0);

        Assert.False(result.Success);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsLogListDumpEvents_ReturnsSuccessfulOrGracefulFailure()
    {
        PsLogListReadTool tool = new();
        ToolResult result = await tool.PsLogListDumpEventsAsync("Application");

        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region PsService tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsServiceAvailability_AlwaysSucceeds()
    {
        PsServiceReadTool tool = new();
        ToolResult result = await tool.PsServiceAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsServiceListServices_InvalidServiceName_ReturnsFailure()
    {
        PsServiceReadTool tool = new();
        // Asterisks pass the generic argument check but fail the service-name whitelist
        ToolResult result = await tool.PsServiceListServicesAsync("EventLog*");

        Assert.False(result.Success);
        Assert.Contains("must contain only", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsServiceListServices_ReturnsSuccessfulOrGracefulFailure()
    {
        PsServiceReadTool tool = new();
        ToolResult result = await tool.PsServiceListServicesAsync();

        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region PsTools suite tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsToolsListSuite_AlwaysSucceeds()
    {
        PsToolsReadTool tool = new();
        ToolResult result = await tool.PsToolsListSuiteAsync();

        // The inventory probe always succeeds, even when no members are installed
        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task PsToolsListSuite_ReturnsFullSuiteInventory()
    {
        PsToolsReadTool tool = new();
        ToolResult result = await tool.PsToolsListSuiteAsync();

        Assert.True(result.Success);
        // Results is a typed list of binary info records (not a string)
        Assert.IsNotType<string>(result.Results);
        Assert.IsAssignableFrom<System.Collections.IEnumerable>(result.Results);

        // The PsTools suite has 12 members
        var inventory = ((System.Collections.IEnumerable)result.Results!).Cast<object>().ToList();
        Assert.Equal(12, inventory.Count);
    }

    #endregion

    #region RegDelNull tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegDelNullAvailability_AlwaysSucceeds()
    {
        RegDelNullReadTool tool = new();
        ToolResult result = await tool.RegDelNullAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegDelNullScanNulls_EmptyKeyPath_ReturnsFailure()
    {
        RegDelNullReadTool tool = new();
        ToolResult result = await tool.RegDelNullScanNullsAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegDelNullScanNulls_ZeroMaxRecords_ReturnsFailure()
    {
        RegDelNullReadTool tool = new();
        ToolResult result = await tool.RegDelNullScanNullsAsync("SOFTWARE", maxRecords: 0);

        Assert.False(result.Success);
        Assert.Contains("between 1 and 500", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegDelNullScanNulls_ValidKey_ReturnsSuccessfulToolResult()
    {
        RegDelNullReadTool tool = new();
        ToolResult result = await tool.RegDelNullScanNullsAsync("SOFTWARE\\Microsoft", maxRecords: 10);

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task RegDelNullScanNulls_ReturnsScanPayload()
    {
        RegDelNullReadTool tool = new();
        ToolResult result = await tool.RegDelNullScanNullsAsync("SOFTWARE\\Microsoft", maxRecords: 5);

        Assert.True(result.Success);
        object payload = result.Results!;
        Assert.NotNull(payload.GetType().GetProperty("ScannedPath"));
        Assert.NotNull(payload.GetType().GetProperty("Findings"));
        Assert.NotNull(payload.GetType().GetProperty("FindingCount"));
    }

    #endregion

    #region ShareEnum tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task ShareEnumAvailability_AlwaysSucceeds()
    {
        ShareEnumReadTool tool = new();
        ToolResult result = await tool.ShareEnumAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    #endregion

    #region SigCheck tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SigCheckAvailability_AlwaysSucceeds()
    {
        SigCheckReadTool tool = new();
        ToolResult result = await tool.SigCheckAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SigCheckVerifyFile_NonExistentFile_ReturnsFailure()
    {
        SigCheckReadTool tool = new();
        ToolResult result = await tool.SigCheckVerifyFileAsync("C:\\__no_such_file_12345__.exe");

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SigCheckVerifyFile_EmptyPath_ReturnsFailure()
    {
        SigCheckReadTool tool = new();
        ToolResult result = await tool.SigCheckVerifyFileAsync("");

        Assert.False(result.Success);
        Assert.Contains("required", result.ErrorDetails, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task SigCheckVerifyFile_ReturnsSuccessfulOrGracefulFailure()
    {
        SigCheckReadTool tool = new();
        string target = Environment.ProcessPath ?? Path.Combine(Environment.SystemDirectory, "notepad.exe");

        if (!File.Exists(target)) { return; } // Skip if the probe target is absent

        ToolResult result = await tool.SigCheckVerifyFileAsync(target);

        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region TcpView tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task TcpViewAvailability_AlwaysSucceeds()
    {
        TcpViewReadTool tool = new();
        ToolResult result = await tool.TcpViewAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task TcpViewListEndpoints_ReturnsSuccessfulOrGracefulFailure()
    {
        TcpViewReadTool tool = new();
        ToolResult result = await tool.TcpViewListEndpointsAsync();

        Assert.True(result.Success || result.ErrorDetails != null,
            $"Expected success or graceful failure but got: Success={result.Success}");
    }

    #endregion

    #region WinObj tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public async Task WinObjAvailability_AlwaysSucceeds()
    {
        WinObjReadTool tool = new();
        ToolResult result = await tool.WinObjAvailabilityAsync();

        Assert.True(result.Success, $"Expected Success=true but got failure: {result.ErrorDetails}");
        Assert.NotNull(result.Results);
    }

    #endregion
}
