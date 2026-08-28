# SentinelCore-MCP

![SentinelMCP](..\assets\sentinelmcp.png)

A Windows security and configuration reconnaissance server implementing the [Model Context Protocol (MCP)](https://modelcontextprotocol.io/). SentinelCore-MCP exposes **70+ read-only tools** that allow AI assistants and MCP clients to inspect Windows system state — firewall rules, Defender status, registry keys, services, processes, certificates, and much more — without modifying anything on the host.

This server is a tool module for the Sentinel Core Forensic Investigation Platform. The tools are universal and could be used elsewhere if appropriate.
All tools are read-only and are designed only for Windows 10-11 Operating Systems. Server implements the MCP protocol over stdio and HTTP transports, and is discoverable by MCP clients.
Server can run alongside Sentinel Core or be placed on a separate host for remote inspection of Windows systems. Ideal for enterprise environments where AI agents need to inspect Windows hosts without installing additional software or agents.

> **Windows only.** This server uses Windows-specific APIs (WMI, CIM, Registry, COM, Sysinternal tools) and is marked `<UnsupportedOSPlatform>linux;osx</UnsupportedOSPlatform>`.

---

## Features

- **70+ read-only tools** across 40+ tool classes covering Windows security, configuration, and diagnostics
- **Never throws** — every tool returns a `ToolResult` object with `Success`, `Results`, and `ErrorDetails`
- **All tools are marked `ReadOnly = true, Destructive = false`** — safe for AI agents to call
- **`server/discover` support** — responds to MCP discovery requests with server capabilities, instructions, and supported protocol versions (2026-07-28 revision, SEP-2575)
- **Stdio transport** — integrates with VS Code, Visual Studio, Claude Desktop, and any MCP-compatible client
- **Self-contained single-file executable** — no .NET runtime required on the target machine
- **NuGet package** — install via `dnx` or `dotnet tool install`

---

## Tool Catalog

### Accessibility

| Tool                          | Description                                         |
| ----------------------------- | --------------------------------------------------- |
| `Accessibility_Read`          | Reads specific ease-of-access feature configuration |
| `Accessibility_Read_Settings` | Reads all accessibility settings                    |
| `Accessibility_Read_UIA_Root` | Reads the root UI Automation element                |

### AppLocker

| Tool                              | Description                                     |
| --------------------------------- | ----------------------------------------------- |
| `AppLocker_Get_Effective_Policy`  | Retrieves the effective AppLocker policy as XML |
| `AppLocker_List_Rule_Collections` | Lists AppLocker rule collections                |

### Audio

| Tool                        | Description                                       |
| --------------------------- | ------------------------------------------------- |
| `Audio_List_Devices`        | Lists active audio playback and recording devices |
| `Audio_Read_Default_Device` | Reads the default audio playback device           |

### Auditing

| Tool       | Description              |
| ---------- | ------------------------ |
| `Auditing` | Gets the auditing policy |

### Battery & Power

| Tool                          | Description                                    |
| ----------------------------- | ---------------------------------------------- |
| `Battery_List`                | Lists battery status using Win32_Battery       |
| `Battery_Read_Power_Settings` | Reads power plan settings related to battery   |
| `Power_List_Plans`            | Lists active and available power plans         |
| `Power_List_Settings`         | Lists power settings for the active power plan |

### BitLocker

| Tool                     | Description                                             |
| ------------------------ | ------------------------------------------------------- |
| `Bitlocker_List_Volumes` | Lists BitLocker-protected volumes                       |
| `Bitlocker_Read_Volume`  | Reads BitLocker volume metadata and key protector types |

### Boot Configuration

| Tool                              | Description                                |
| --------------------------------- | ------------------------------------------ |
| `Boot_Configuration_Read_Current` | Returns the GUID of the current boot entry |
| `Boot_Configuration_Enum`         | Enumerates all active BCD entries          |

### Browser Configuration

| Tool                                  | Description                                          |
| ------------------------------------- | ---------------------------------------------------- |
| `Browser_Config_Read_Chrome_Policies` | Reads Google Chrome policy entries from the registry |
| `Browser_Config_Read_Default`         | Reads the default browser ProgId from the registry   |
| `Browser_Config_Read_IE_Settings`     | Reads Internet Explorer zone and security settings   |

### Certificates

| Tool               | Description                             |
| ------------------ | --------------------------------------- |
| `Certificate_List` | Lists certificates in a specified store |
| `Certificate_Read` | Reads certificate details by thumbprint |

### Credentials

| Tool                       | Description                                            |
| -------------------------- | ------------------------------------------------------ |
| `Credentials_List_Targets` | Lists stored Windows credential targets (no passwords) |

### DCOM

| Tool                       | Description                           |
| -------------------------- | ------------------------------------- |
| `DCOM_List_Applications`   | Lists registered DCOM application IDs |
| `DCOM_Read_AppId_Settings` | Reads DCOM application settings       |

### Defender

| Tool                            | Description                                                 |
| ------------------------------- | ----------------------------------------------------------- |
| `Defender_Read_Registry_Config` | Reads Defender exclusion and configuration registry entries |
| `Defender_Read_Status`          | Reads Microsoft Defender antivirus status via WMI           |

### Display

| Tool                          | Description                   |
| ----------------------------- | ----------------------------- |
| `Display_List_Monitors`       | Lists connected monitors      |
| `Display_Read_Virtual_Screen` | Reads virtual screen geometry |

### Drivers

| Tool          | Description                          |
| ------------- | ------------------------------------ |
| `Driver_List` | Lists kernel and file-system drivers |

### Environment Variables

| Tool                               | Description                           |
| ---------------------------------- | ------------------------------------- |
| `Environment_Variables_List`       | Lists all environment variables       |
| `Environment_Variables_Read_Value` | Reads a specific environment variable |

### Event Log

| Tool                           | Description                            |
| ------------------------------ | -------------------------------------- |
| `Event_Log_List_Channels`      | Lists event log channels               |
| `Event_Log_Query`              | Queries events from a specific channel |
| `Event_Log_Read_Configuration` | Reads event log channel configuration  |

### File System

| Tool                         | Description                                     |
| ---------------------------- | ----------------------------------------------- |
| `File_System_List_Directory` | Lists files and directories in a path           |
| `File_System_Read_Acl`       | Reads NTFS ACL for a file or directory          |
| `File_System_Read_Metadata`  | Reads file or directory metadata and attributes |

### Firewall

| Tool                     | Description                                        |
| ------------------------ | -------------------------------------------------- |
| `Firewall_List_Rules`    | Lists Windows Firewall rules with optional filters |
| `Firewall_Read_Profiles` | Reads current firewall profile settings            |

### Group Policy

| Tool                      | Description                              |
| ------------------------- | ---------------------------------------- |
| `Group_Policy_List`       | Lists local group policy keys and values |
| `Group_Policy_Read_Value` | Reads a specific group policy value      |

### Hyper-V

| Tool                   | Description                     |
| ---------------------- | ------------------------------- |
| `HyperV_List_Switches` | Lists Hyper-V virtual switches  |
| `HyperV_List_VMs`      | Lists Hyper-V virtual machines  |
| `HyperV_Read_VM`       | Reads detailed VM configuration |

### Installed Applications

| Tool                      | Description                                           |
| ------------------------- | ----------------------------------------------------- |
| `Installed_Apps_List`     | Lists installed applications from Add/Remove Programs |
| `Installed_Apps_MSI_List` | Lists MSI-installed applications                      |

### Local Accounts

| Tool                         | Description                         |
| ---------------------------- | ----------------------------------- |
| `Local_Accounts_List_Groups` | Lists local groups with memberships |
| `Local_Accounts_List_Users`  | Lists local user accounts           |

### Network

| Tool                           | Description                         |
| ------------------------------ | ----------------------------------- |
| `Network_List_Interfaces`      | Lists network interfaces            |
| `Network_List_Tcp_Connections` | Lists active TCP connections        |
| `Network_Read_IP_Config`       | Reads IP configuration              |
| `Network_Resolve_DNS`          | Resolves a hostname to IP addresses |

### Notifications

| Tool                            | Description                                 |
| ------------------------------- | ------------------------------------------- |
| `Notification_List_Apps`        | Lists notification settings and app entries |
| `Notification_Read_Quiet_Hours` | Reads quiet hours / do-not-disturb state    |

### Performance Counters

| Tool                          | Description                                      |
| ----------------------------- | ------------------------------------------------ |
| `Performance_List_Categories` | Lists performance counter categories             |
| `Performance_List_Counters`   | Lists counters in a category                     |
| `Performance_Read_Counter`    | Reads the current value of a performance counter |

### PnP Devices

| Tool              | Description                                |
| ----------------- | ------------------------------------------ |
| `PnpListDevices`  | Lists PnP devices using pnputil            |
| `Pnp_Read_Device` | Reads properties for a specific PnP device |

### Printers

| Tool           | Description                               |
| -------------- | ----------------------------------------- |
| `Printer_List` | Lists installed printers and queue status |
| `Printer_Read` | Reads details of a specific printer       |

### Processes

| Tool           | Description                                          |
| -------------- | ---------------------------------------------------- |
| `Process_List` | Lists running processes with PID, name, and metadata |
| `Process_Read` | Reads details for a specific process by PID          |

### Proxy

| Tool                 | Description                                        |
| -------------------- | -------------------------------------------------- |
| `Proxy_Read_System`  | Reads system proxy configuration from the registry |
| `Proxy_Read_WinHTTP` | Reads WinHTTP proxy configuration via netsh        |

### Random Number

| Tool                | Description                                   |
| ------------------- | --------------------------------------------- |
| `Random_Get_Number` | Generates a random number between min and max |

### Registry

| Tool                  | Description                                             |
| --------------------- | ------------------------------------------------------- |
| `Registry_List_Key`   | Lists subkey names and value names under a registry key |
| `Registry_Read_Value` | Reads a registry value from a specified key path        |

### Remote Desktop

| Tool                       | Description                                          |
| -------------------------- | ---------------------------------------------------- |
| `RDP_Read_Listener_Config` | Reads RDP listener port and security layer settings  |
| `RDP_Read_Settings`        | Reads Remote Desktop configuration from the registry |

### Scheduled Tasks

| Tool                  | Description                        |
| --------------------- | ---------------------------------- |
| `Scheduled_Task_List` | Lists scheduled tasks              |
| `Scheduled_Task_Read` | Reads scheduled task configuration |

### Search Indexing

| Tool                            | Description                                                 |
| ------------------------------- | ----------------------------------------------------------- |
| `Search_Indexing_List_Scopes`   | Lists indexed locations from the Windows Search crawl scope |
| `Search_Indexing_Read_Settings` | Reads Windows Search service configuration                  |

### Sensors

| Tool                           | Description                   |
| ------------------------------ | ----------------------------- |
| `Sensor_List_Devices`          | Lists sensor devices via CIM  |
| `Sensor_Read_Location_Service` | Reads location service status |

### Shell & Explorer

| Tool                           | Description                                                   |
| ------------------------------ | ------------------------------------------------------------- |
| `Shell_Explorer_Read_Settings` | Reads Explorer settings (hidden files, file extensions, etc.) |
| `Shell_Taskbar_Pinned_List`    | Lists pinned taskbar items                                    |

### UAC

| Tool                       | Description                                           |
| -------------------------- | ----------------------------------------------------- |
| `UAC_Read_Settings`        | Reads UAC policy settings from the registry           |
| `UAC_Read_Token_Elevation` | Reports whether the current process token is elevated |

### VPN

| Tool                        | Description                                  |
| --------------------------- | -------------------------------------------- |
| `VPN_List_Connections`      | Lists configured VPN/RAS connections         |
| `VPN_Read_Phonebook_Status` | Reads phonebook directory path and existence |

### Windows Services

| Tool           | Description                                         |
| -------------- | --------------------------------------------------- |
| `Service_List` | Lists installed Windows services and their status   |
| `Service_Read` | Reads detailed information about a specific service |

### Windows Update

| Tool                           | Description                                            |
| ------------------------------ | ------------------------------------------------------ |
| `Windows_Update_List_History`  | Lists installed Windows update history                 |
| `Windows_Update_Read_Settings` | Reads Windows Update policy settings from the registry |

### Wireless

| Tool                       | Description                          |
| -------------------------- | ------------------------------------ |
| `Wireless_List_Interfaces` | Lists wireless network interfaces    |
| `Wireless_List_Profiles`   | Lists saved Wi-Fi profiles via netsh |

### WMI / CIM Queries

| Tool               | Description                                         |
| ------------------ | --------------------------------------------------- |
| `WMI_List_Classes` | Lists CIM class names in a namespace                |
| `WMI_Query`        | Executes a read-only CIM WQL query and returns JSON |

### Sysinternals

Read-only wrappers around the [Sysinternals](https://learn.microsoft.com/sysinternals/) diagnostic suite. Sysinternals binaries are optional; every tool probes availability first and fails gracefully with a structured error when a binary is not installed. GUI-only tools expose an availability probe only.

| Tool                                        | Description                                                                       |
| ------------------------------------------- | --------------------------------------------------------------------------------- |
| `Sysinternals_AccessChk_Availability`       | Checks whether AccessChk is installed                                             |
| `Sysinternals_AccessChk_Read_Permissions`   | Audits effective permissions on files, registry keys, services, or processes      |
| `Sysinternals_AccessEnum_Availability`      | Checks whether AccessEnum is installed (GUI-only)                                 |
| `Sysinternals_AD_Availability`              | Reports whether the host is joined to an Active Directory domain                  |
| `Sysinternals_AD_Browse_Container`          | Browses an AD container and lists child objects (ADExplorer-style)                |
| `Sysinternals_ADInsight_Availability`       | Checks whether ADInsight is installed (GUI-only)                                  |
| `Sysinternals_CoreInfo_Availability`        | Checks whether Coreinfo is installed                                              |
| `Sysinternals_CoreInfo_Read_System`         | Enumerates CPU topology, cache, NUMA, and virtualization support                  |
| `Sysinternals_Handle_Availability`          | Checks whether Handle is installed                                                |
| `Sysinternals_Handle_List`                  | Lists open file and kernel object handles                                         |
| `Sysinternals_ListDlls_Availability`        | Checks whether ListDLLs is installed                                              |
| `Sysinternals_ListDlls_List_Loaded`         | Lists DLLs loaded by processes                                                    |
| `Sysinternals_LogonSessions_Availability`   | Checks whether LogonSessions is installed                                         |
| `Sysinternals_LogonSessions_List_Active`    | Lists active logon sessions with authentication details                           |
| `Sysinternals_NtfsInfo_Availability`        | Checks whether NTFSInfo is installed                                              |
| `Sysinternals_NtfsInfo_Read_Volume`         | Reports NTFS volume geometry and metadata                                         |
| `Sysinternals_PendMoves_Availability`       | Checks whether PendMoves is installed                                             |
| `Sysinternals_PendMoves_List_Pending`       | Lists file operations scheduled for the next reboot (managed read)                |
| `Sysinternals_PipeList_Availability`        | Checks whether PipeList is installed                                              |
| `Sysinternals_PipeList_List_Pipes`          | Lists named pipes with instance counts                                            |
| `Sysinternals_ProcDump_Availability`        | Checks whether ProcDump is installed                                              |
| `Sysinternals_ProcDump_Capture_Dump`        | Captures a full process dump to a specified path (writes a file; not read-only)   |
| `Sysinternals_ProcessExplorer_Availability` | Checks whether ProcessExplorer is installed (GUI-only)                            |
| `Sysinternals_PsInfo_Availability`          | Checks whether PsInfo is installed                                                |
| `Sysinternals_PsInfo_Read_System`           | Reports OS version, kernel, install date, and hotfixes                            |
| `Sysinternals_PsList_Availability`          | Checks whether PsList is installed                                                |
| `Sysinternals_PsList_List_Processes`        | Lists processes with CPU and memory statistics                                    |
| `Sysinternals_PsLogList_Availability`       | Checks whether PsLogList is installed                                             |
| `Sysinternals_PsLogList_Dump_Events`        | Dumps recent event log records                                                    |
| `Sysinternals_PsService_Availability`       | Checks whether PsService is installed                                             |
| `Sysinternals_PsService_List_Services`      | Lists services and their configuration                                            |
| `Sysinternals_PsTools_List_Suite`           | Reports which PsTools suite members are installed (inventory only)                |
| `Sysinternals_RegDelNull_Availability`      | Checks whether RegDelNull is installed                                            |
| `Sysinternals_RegDelNull_Scan_Nulls`        | Scans a registry subtree for embedded-null values (detection only; never deletes) |
| `Sysinternals_ShareEnum_Availability`       | Checks whether ShareEnum is installed (GUI-only)                                  |
| `Sysinternals_SigCheck_Availability`        | Checks whether SigCheck is installed                                              |
| `Sysinternals_SigCheck_Verify_File`         | Verifies a file's digital signature and certificate details                       |
| `Sysinternals_TcpView_Availability`         | Checks whether Tcpvcon is installed                                               |
| `Sysinternals_TcpView_List_Endpoints`       | Lists TCP/UDP endpoints with owning process attribution                           |
| `Sysinternals_WinObj_Availability`          | Checks whether WinObj is installed (GUI-only)                                     |

---

## Getting Started

### Prerequisites

- Windows 10/11 or Windows Server 2019+
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (for building from source)

### Running from Source

```bash
dotnet run --project SentinelCore-MCP
```

### Configuring in VS Code

Add to your `.vscode/mcp.json` or VS Code settings:

```json
{
  "servers": {
    "SentinelCore-MCP": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "<PATH-TO-SENTINELCORE-MCP>"]
    }
  }
}
```

### Configuring in Visual Studio

Create a `.mcp.json` file in your solution directory:

```json
{
  "servers": {
    "SentinelCore-MCP": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "<PATH-TO-SENTINELCORE-MCP>"]
    }
  }
}
```

### Installing as a NuGet Tool

```bash
dotnet tool install --global SentinelCore-MCP
```

Then configure your MCP client:

```json
{
  "servers": {
    "SentinelCore-MCP": {
      "type": "stdio",
      "command": "dnx",
      "args": ["SentinelCore-MCP", "--version", "0.1.0-beta", "--yes"]
    }
  }
}
```

---

## Architecture

### ToolResult Contract

Every tool returns a `ToolResult` object — **never throws**:

```csharp
public class ToolResult
{
    public bool    Success      { get; set; }
    public string? Results      { get; set; }
    public string? ErrorDetails { get; set; }
    public string? Message      { get; set; }

    public static ToolResult Ok(string results, string message = "Ok");
    public static ToolResult Fail(string errorDetails, string message = "Fail");
}
```

- **Success path** → `ToolResult.Ok(jsonOrText)`
- **Error path** → `ToolResult.Fail("descriptive error: {ex.Message}")`
- All `catch` blocks include the exception message for diagnostics

### Design Principles

1. **Read-only by default** — every `[McpServerTool]` is marked `ReadOnly = true, Destructive = false`
2. **Never throw** — exceptions are caught and wrapped in `ToolResult.Fail(...)`
3. **No custom types** — tools use built-in .NET and Windows API types exclusively
4. **Stdio transport** — communicates over stdin/stdout using the MCP protocol
5. **Discoverable** — supports `server/discover` (2026-07-28 revision) for capability probing without initialization handshake

---

## Publishing

1. Update `<PackageId>` and `<PackageVersion>` in `SentinelCore-MCP.csproj`
2. Run `dotnet pack -c Release`
3. Publish to NuGet.org:

```bash
dotnet nuget push bin/Release/*.nupkg --api-key <YOUR-API-KEY> --source https://api.nuget.org/v3/index.json
```

---

## Links

- [MCP Specification](https://spec.modelcontextprotocol.io/)
- [MCP C# SDK](https://modelcontextprotocol.github.io/csharp-sdk)
- [ModelContextProtocol NuGet](https://www.nuget.org/packages/ModelContextProtocol)
- [Use MCP Servers in VS Code](https://code.visualstudio.com/docs/copilot/chat/mcp-servers)
- [Use MCP Servers in Visual Studio](https://learn.microsoft.com/visualstudio/ide/mcp-servers)

## License

This project is licensed under the terms found in the [LICENSE](../LICENSE) file.
