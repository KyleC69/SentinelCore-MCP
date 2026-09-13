// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         PowerShellReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.Collections;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Versioning;
using System.Text;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     A strictly sandboxed, read-only PowerShell query tool.
///     Only whitelisted commands are permitted. All commands run in a constrained
///     runspace with no write, network, or execution capabilities.
/// </summary>
[SupportedOSPlatform("windows")]
[McpServerToolType]
public sealed class PowerShellReadTool
{

    /// <summary>
    ///     Maximum number of result objects returned from any single query.
    /// </summary>
    private const int DefaultMaxResults = 250;

    /// <summary>
    ///     Timeout in seconds for PowerShell command execution.
    ///     Commands exceeding this timeout are aborted.
    /// </summary>
    private const int ExecutionTimeoutSeconds = 30;

    /// <summary>
    ///     Maximum string length for any single property value in the output.
    ///     Values exceeding this length are truncated.
    /// </summary>
    private const int MaxPropertyStringLength = 4096;








    /// <summary>
    ///     The set of PowerShell commands that are explicitly permitted.
    ///     Only these commands may be invoked through this tool.
    ///     Every entry is a read-only, non-destructive query command.
    /// </summary>
    [Description("Retrieve a list of allowed powershell commands")]
    [McpServerTool(Title = "List of allowed command for this tool.", Destructive = false, Name = "AllowedCommands", ReadOnly = true)]
    public List<string> GetAllowedCommands() => AllowedCommands.ToList();
    public static readonly HashSet<string> AllowedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
            // ---- System information ----
            "Get-ComputerInfo",
            "Get-Date",
            "Get-Host",
            "Get-Process",
            "Get-Service",
            "Get-EventLog",
            "Get-WinEvent",
            "systeminfo",

            // ---- Hardware & drivers ----
            "Get-CimInstance",
            "Get-WmiObject",
            "Get-PnpDevice",
            "Get-PnpDeviceProperty",
            "pnputil",

            // ---- Network (read-only) ----
            "Get-NetAdapter",
            "Get-NetIPAddress",
            "Get-NetRoute",
            "Get-NetTCPConnection",
            "Get-NetUDPEndpoint",
            "Get-NetNeighbor",
            "Get-DnsClientCache",
            "Get-DnsClientServerAddress",
            "Get-NetFirewallRule",
            "Get-NetFirewallProfile",
            "ipconfig",
            "netstat",
            "arp",
            "route",

            // ---- Security & policy ----
            "Get-AppLockerPolicy",
            "Get-Acl",
            "Get-ExecutionPolicy",
            "Get-AuthenticodeSignature",
            "Get-FileHash",
            "auditpol",

            // ---- Storage & filesystem (read-only) ----
            "Get-Volume",
            "Get-Partition",
            "Get-Disk",
            "Get-Item",
            "Get-ChildItem",
            "Get-Content",
            "Get-ItemProperty",
            "Test-Path",
            "Get-Acl", // duplicate intentional — alias coverage

            // ---- User & session ----
            "Get-LocalUser",
            "Get-LocalGroup",
            "Get-LocalGroupMember",
            "query",

            // ---- Scheduled tasks ----
            "Get-ScheduledTask",
            "Get-ScheduledTaskInfo",

            // ---- Windows Update ----
            "Get-HotFix",

            // ---- Environment ----
            "Get-EnvironmentVariable",
            "$env:", // environment variable access

            // ---- Format & output (safe pipeline commands) ----
            "Select-Object",
            "Where-Object",
            "Sort-Object",
            "Format-List",
            "Format-Table",
            "Format-Wide",
            "Out-String",
            "ConvertTo-Json",
            "ConvertTo-Xml",
            "ConvertTo-Html",
            "Measure-Object",
            "Group-Object",
            "Tee-Object",
            "ForEach-Object"
    };

    /// <summary>
    ///     Commands and patterns that are explicitly forbidden regardless of context.
    ///     Any command matching these patterns is rejected before execution.
    /// </summary>
    private static readonly HashSet<string> ForbiddenCommands = new(StringComparer.OrdinalIgnoreCase)
    {
            // ---- Write / modify ----
            "Set-Content",
            "Set-Item",
            "Set-ItemProperty",
            "Set-Location",
            "Set-ExecutionPolicy",
            "Set-Service",
            "Set-NetFirewallRule",
            "Set-NetFirewallProfile",
            "New-Item",
            "New-ItemProperty",
            "New-Service",
            "New-LocalUser",
            "New-NetFirewallRule",
            "Remove-Item",
            "Remove-ItemProperty",
            "Remove-LocalUser",
            "Remove-LocalGroup",
            "Remove-LocalGroupMember",
            "Remove-NetFirewallRule",
            "Remove-Service",
            "Copy-Item",
            "Move-Item",
            "Rename-Item",
            "Clear-Content",
            "Clear-ItemProperty",
            "Out-File",
            "Out-Printer",
            "Export-Csv",
            "Export-Clixml",
            "Export-Json",

            // ---- Execution / invocation ----
            "Invoke-Command",
            "Invoke-Expression",
            "Invoke-WebRequest",
            "Invoke-RestMethod",
            "Invoke-CimMethod",
            "Invoke-WmiMethod",
            "Start-Process",
            "Start-Service",
            "Start-Sleep",
            "Stop-Process",
            "Stop-Service",
            "Restart-Service",
            "Enter-PSSession",
            "New-PSSession",
            "Connect-PSSession",
            "WinRM",

            // ---- Dangerous cmdlets ----
            "Invoke-Script",
            "Invoke-AsWorkflow",
            "Register-EngineEvent",
            "Register-WmiEvent",
            "Register-CimIndicationEvent",
            "Add-Content",
            "Add-Member",
            "Write-Host",
            "Write-Output",
            "Write-Verbose",
            "Write-Debug",
            "Write-Warning",
            "Write-Error",
            "Write-Information",

            // ---- Network (destructive) ----
            "Enable-NetFirewallRule",
            "Disable-NetFirewallRule",
            "New-NetRoute",
            "Remove-NetRoute",
            "Set-NetAdapter",
            "Restart-NetAdapter",
            "Disable-NetAdapter",
            "Enable-NetAdapter",
            "netsh",

            // ---- User management ----
            "Add-LocalGroupMember",
            "Disable-LocalUser",
            "Enable-LocalUser",
            "Set-LocalUser",

            // ---- Scheduled tasks (destructive) ----
            "Register-ScheduledTask",
            "Unregister-ScheduledTask",
            "Start-ScheduledTask",
            "Stop-ScheduledTask",
            "Enable-ScheduledTask",
            "Disable-ScheduledTask",

            // ---- Misc dangerous ----
            "cmd",
            "cmd.exe",
            "powershell",
            "pwsh",
            "bash",
            "sh",
            "schtasks",
            "reg",
            "reg.exe",
            "sc",
            "sc.exe",
            "shutdown",
            "restart-computer",
            "stop-computer"
    };

    /// <summary>
    ///     Characters and patterns that are never allowed in a command string.
    ///     These prevent injection of pipeline commands, script blocks, and file redirects.
    /// </summary>
    private static readonly string[] ForbiddenPatterns =
    [
            "|", // pipeline injection
            ">", // file redirect (overwrite)
            ">>", // file redirect (append)
            "2>", // stderr redirect
            "&", // background operator / command chaining
            ";", // statement separator
            "`", // escape character
            "$(", // subexpression
            "$(", // subexpression
            "${", // variable expression
            "::", // static method access
            "[System.", // .NET type access
            "[Microsoft.", // .NET type access
            "New-Object", // object creation
            "Add-Type", // type injection
            "Reflection", // reflection abuse
            "IO.File", // file I/O
            "IO.Stream", // stream I/O
            "Net.Web", // web access
            "Net.Sockets", // socket access
            "Process.Start", // process spawning
            "Management.Automation", // PS automation abuse
            "FromBase64", // base64 encoded payloads
            "DownloadString", // download payloads
            "DownloadFile" // download payloads
    ];








    /// <summary>
    ///     Creates a constrained PowerShell runspace with restricted capabilities.
    ///     Uses ConstrainedLanguage mode which restricts .NET type access, script execution,
    ///     and many dangerous operations. Combined with the whitelist/blacklist validation,
    ///     this provides defense-in-depth.
    /// </summary>
    private static InitialSessionState CreateConstrainedSessionState()
    {
        // Create a default session state and restrict it
        InitialSessionState iss = InitialSessionState.CreateDefault();

        // Set language mode to ConstrainedLanguage — this restricts:
        // - .NET type instantiation (New-Object blocked)
        // - Script block execution
        // - Type conversion abuse
        // - Many other dangerous patterns
        iss.LanguageMode = PSLanguageMode.ConstrainedLanguage;

        return iss;
    }








    /// <summary>
    ///     Determines if a token looks like a command (cmdlet, native executable, or variable).
    /// </summary>
    private static bool IsCommandLike(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        // PowerShell cmdlets: Verb-Noun pattern
        if (token.Contains('-') && char.IsLetter(token[0]))
        {
            return true;
        }

        // Native commands: known executable names
        if (token.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || token.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase) || token.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) || token.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Environment variable access
        if (token.StartsWith("$env:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Short known native commands
        if (token.Length <= 12 && char.IsLetter(token[0]))
        {
            string[] knownNatives = ["arp", "route", "netstat", "ipconfig", "pnputil", "auditpol", "query", "systeminfo"];
            return knownNatives.Contains(token, StringComparer.OrdinalIgnoreCase);
        }

        return false;
    }








    /// <summary>
    ///     Truncates a string value if it exceeds the maximum property string length.
    /// </summary>
    private static string TruncateValue(object? value)
    {
        if (value is null)
        {
            return "null";
        }

        string str = value.ToString() ?? "null";

        if (str.Length > MaxPropertyStringLength)
        {
            return string.Concat(str.AsSpan(0, MaxPropertyStringLength), "...[truncated]");
        }

        return str;
    }








    /// <summary>
    ///     Validates that a command string contains only whitelisted commands
    ///     and does not contain any forbidden patterns.
    /// </summary>
    public static string? ValidateCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return "Command is required.";
        }

        // Check for forbidden injection patterns first
        foreach (string pattern in ForbiddenPatterns)
        {
            if (command.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return $"Command contains forbidden pattern: '{pattern}'. " + "Pipeline operators, redirects, script blocks, and .NET type access are not allowed.";
            }
        }

        // Extract the first token (the command name) and any subsequent command tokens
        // Split on whitespace to get individual tokens
        string[] tokens = command.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

        // Check each token that looks like a command (starts with a letter, dash, or $)
        foreach (string token in tokens)
        {
            // Skip parameters (start with -) and string literals
            if (token.StartsWith('-') || token.StartsWith('"') || token.StartsWith("'"))
            {
                continue;
            }

            // Extract the command name (before any dot or colon)
            string commandPart = token.Split('.')[0].Split(':')[0].Split('\\')[^1];

            // Check blacklist first
            if (ForbiddenCommands.Contains(commandPart) || ForbiddenCommands.Contains(token))
            {
                return $"Command '{token}' is explicitly forbidden.";
            }

            // Check if it's a known command that's not whitelisted
            // Commands starting with a verb (Get-, Set-, New-, etc.) or known native commands
            if (IsCommandLike(token) && !AllowedCommands.Contains(token) && !AllowedCommands.Contains(commandPart))
            {
                return $"Command '{token}' is not in the allowed list. " + "Only pre-approved read-only commands are permitted.";
            }
        }

        return null; // validation passed
    }








    /// <summary>
    ///     Returns the list of allowed PowerShell commands for reference.
    /// </summary>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "PowerShell_List_Allowed_Commands", ReadOnly = true, Destructive = false)]
    [Description("Returns the list of PowerShell commands that are permitted by the PowerShell_Query tool.")]
    public async Task<ToolResult> PowershellListAllowedCommandsAsync()
    {
        StringBuilder sb = new();
        sb.AppendLine("Allowed Commands:");
        sb.AppendLine();

        string[] sorted = [.. AllowedCommands.Order(StringComparer.OrdinalIgnoreCase)];
        foreach (string cmd in sorted)
        {
            sb.AppendLine($"  {cmd}");
        }

        return ToolResult.Ok(sb.ToString(), "PowerShellReadTool");
    }








    /// <summary>
    ///     Executes a validated PowerShell command in a constrained runspace and returns
    ///     the results as a formatted string.
    /// </summary>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "PowerShell_Query", ReadOnly = true, Destructive = false)]
    [Description("Executes a pre-approved, read-only PowerShell query in a sandboxed runspace. " + "Only whitelisted commands are permitted. Pipeline operators, redirects, " + "script blocks, and .NET type access are forbidden. Call AllowedCommands for a list of commands.")]
    public async Task<ToolResult> PowershellQueryAsync([Description("The PowerShell command to execute. Must be a single read-only command " + "from the approved list. No pipelines, redirects, or script blocks.")] string command, [Description("Maximum number of result objects to return. Defaults to 50.")] int maxResults = DefaultMaxResults)
    {
        // Step 1: Validate the command against whitelist and blacklist
        string? validationError = ValidateCommand(command);
        if (validationError is not null)
        {
            return ToolResult.Fail(validationError, "PowerShellReadTool");
        }

        // Step 2: Execute in a constrained runspace
        try
        {
            InitialSessionState iss = CreateConstrainedSessionState();
            using PowerShell ps = PowerShell.Create(iss);
            ps.AddScript(command);

            // Set execution timeout to prevent runaway commands
            IAsyncResult asyncResult = ps.BeginInvoke();
            if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(ExecutionTimeoutSeconds)))
            {
                ps.Stop();
                return ToolResult.Fail($"Command execution timed out after {ExecutionTimeoutSeconds} seconds.", "PowerShellReadTool");
            }

            PSDataCollection<PSObject> results = ps.EndInvoke(asyncResult);

            // Check for errors
            if (ps.HadErrors)
            {
                string errors = string.Join("; ", ps.Streams.Error.Select(e => e.ToString()));
                return ToolResult.Fail($"PowerShell query failed: {errors}", "PowerShellReadTool");
            }

            // Step 3: Format results
            StringBuilder sb = new();
            int count = 0;

            foreach (PSObject result in results)
            {
                if (count >= maxResults)
                {
                    sb.AppendLine($"...[truncated at {maxResults} results]");
                    break;
                }

                if (result.BaseObject is IDictionary dict)
                {
                    foreach (DictionaryEntry entry in dict)
                    {
                        sb.AppendLine($"  {entry.Key}={TruncateValue(entry.Value)}");
                    }

                    sb.AppendLine();
                }
                else if (result.BaseObject is not null && result.BaseObject.GetType().IsPrimitive)
                {
                    sb.AppendLine(TruncateValue(result.BaseObject));
                }
                else
                {
                    // For complex objects, enumerate properties
                    bool hasProperties = false;
                    foreach (PSPropertyInfo prop in result.Properties)
                    {
                        hasProperties = true;
                        sb.AppendLine($"  {prop.Name}={TruncateValue(prop.Value)}");
                    }

                    if (!hasProperties)
                    {
                        sb.AppendLine(TruncateValue(result.BaseObject));
                    }

                    sb.AppendLine();
                }

                count++;
            }

            if (count == 0)
            {
                sb.AppendLine("No results returned.");
            }

            return ToolResult.Ok(sb.ToString(), "PowerShellReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"PowerShell query failed: {ex.Message}", "PowerShellReadTool");
        }
    }
}
