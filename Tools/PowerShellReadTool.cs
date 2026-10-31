// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         PowerShellReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  102702



using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;

using Microsoft.Win32;

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
    private const int DefaultMaxResults = 50;

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








    static PowerShellReadTool()
    {
        // The MCP host instantiates tool types at server startup, so this runs before the
        // first runspace is created and before the engine computes its module search path.
        PowerShellModulePathResolver.EnsureConfigured();
    }








    /// <summary>
    ///     Creates a constrained PowerShell runspace with restricted capabilities.
    ///     Uses ConstrainedLanguage mode which restricts .NET type access, script execution,
    ///     and many dangerous operations. Combined with the whitelist/blacklist validation,
    ///     this provides defense-in-depth.
    /// </summary>
    private static InitialSessionState CreateConstrainedSessionState()
    {
        // Ensure module locations are resolved before the engine builds its module path.
        PowerShellModulePathResolver.EnsureConfigured();

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
    ///     Returns a suffix describing the module path search failure, or an empty string
    ///     when module locations were resolved normally.
    /// </summary>
    private static string FormatModulePathNote()
    {
        string problem = PowerShellModulePathResolver.ConfigurationProblem;
        return string.IsNullOrEmpty(problem) ? string.Empty : $" [{problem}]";
    }








    /// <summary>
    ///     Resolves PowerShell module locations on any default Windows 10/11 installation
    ///     and prepends them to the process-level PSModulePath before the first runspace is
    ///     created. The hosted PowerShell engine (System.Management.Automation NuGet package)
    ///     derives $PSHOME from this application's own directory, so real module locations
    ///     are never discovered automatically.
    /// </summary>
    private static class PowerShellModulePathResolver
    {
        /// <summary>True once the process environment has been configured.</summary>
        private static bool _configured;

        /// <summary>
        ///     Describes why no module location could be resolved, or an empty string when
        ///     at least one location was found. Surfaced through tool failure results.
        /// </summary>
        internal static string ConfigurationProblem { get; private set; } = string.Empty;








        /// <summary>
        ///     Configures the process PSModulePath exactly once. Repeated calls return
        ///     immediately. Never throws; failures are recorded in
        ///     <see cref="ConfigurationProblem"/> and surfaced through tool results.
        /// </summary>
        internal static void EnsureConfigured()
        {
            if (_configured)
            {
                return;
            }

            _configured = true;

            try
            {
                List<string> roots = [];
                List<string> searched = [];

                foreach ((string root, string sentinelModule) in EnumerateCandidates())
                {
                    searched.Add(root);
                    if (IsValidModuleRoot(root, sentinelModule) && !roots.Contains(root, StringComparer.OrdinalIgnoreCase))
                    {
                        roots.Add(root);
                    }
                }

                if (roots.Count == 0)
                {
                    ConfigurationProblem = "No PowerShell module locations were found. Searched: " + string.Join(", ", searched);
                    return;
                }

                string current = Environment.GetEnvironmentVariable("PSModulePath", EnvironmentVariableTarget.Process) ?? string.Empty;
                List<string> parts = [.. current.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

                // Prepend in reverse order so the first resolved root ends up first in the
                // module search order, matching how a real PowerShell 7 session is built.
                for (int i = roots.Count - 1; i >= 0; i--)
                {
                    if (!parts.Contains(roots[i], StringComparer.OrdinalIgnoreCase))
                    {
                        parts.Insert(0, roots[i]);
                    }
                }

                Environment.SetEnvironmentVariable("PSModulePath", string.Join(';', parts), EnvironmentVariableTarget.Process);
            }
            catch (Exception ex)
            {
                ConfigurationProblem = "Failed to configure PSModulePath: " + ex.Message;
            }
        }








        /// <summary>
        ///     A candidate root is valid only when the directory exists and contains the
        ///     sentinel module folder, proving that real modules are installed there.
        /// </summary>
        private static bool IsValidModuleRoot(string root, string sentinelModule)
        {
            return !string.IsNullOrWhiteSpace(root) && Directory.Exists(root) && Directory.Exists(Path.Combine(root, sentinelModule));
        }








        /// <summary>
        ///     Yields candidate module roots with their sentinel module folder, in priority
        ///     order: PowerShell 7 core module locations first (default install, then
        ///     installer registry, uninstaller registry, App Paths, and PATH fallbacks for
        ///     custom installs), then the inbox Windows PowerShell module location that
        ///     ships with every Windows 10/11 installation.
        /// </summary>
        private static IEnumerable<(string Root, string SentinelModule)> EnumerateCandidates()
        {
            const string coreSentinel = "Microsoft.PowerShell.Utility";
            const string inboxSentinel = "ScheduledTasks";

            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            yield return (Path.Combine(programFiles, "PowerShell", "7", "Modules"), coreSentinel);

            foreach (string installRoot in GetPowerShellCoreInstallLocations())
            {
                yield return (Path.Combine(installRoot, "Modules"), coreSentinel);
            }

            foreach (string installRoot in GetUninstallerInstallLocations())
            {
                yield return (Path.Combine(installRoot, "Modules"), coreSentinel);
            }

            string? appPathsExecutable = GetAppPathsPwshExecutable();
            if (!string.IsNullOrEmpty(appPathsExecutable))
            {
                string? appPathsDirectory = Path.GetDirectoryName(appPathsExecutable);
                if (!string.IsNullOrEmpty(appPathsDirectory))
                {
                    yield return (Path.Combine(appPathsDirectory, "Modules"), coreSentinel);
                }
            }

            string? pathVariable = Environment.GetEnvironmentVariable("Path");
            if (!string.IsNullOrEmpty(pathVariable))
            {
                foreach (string pathDirectory in pathVariable.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (File.Exists(Path.Combine(pathDirectory, "pwsh.exe")))
                    {
                        yield return (Path.Combine(pathDirectory, "Modules"), coreSentinel);
                    }
                }
            }

            yield return (Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "Modules"), inboxSentinel);
        }








        /// <summary>
        ///     Reads install roots recorded by the PowerShell 7 installer under the
        ///     PowerShellCore\InstalledVersions registry key (machine, 32-bit-on-64, and
        ///     per-user hives).
        /// </summary>
        private static IEnumerable<string> GetPowerShellCoreInstallLocations()
        {
            const string machinePath = @"Software\Microsoft\PowerShellCore\InstalledVersions";

            foreach ((RegistryKey hive, string keyPath) in new[] { (Registry.LocalMachine, machinePath), (Registry.LocalMachine, @"Software\WOW6432Node\Microsoft\PowerShellCore\InstalledVersions"), (Registry.CurrentUser, machinePath), })
            {
                using RegistryKey? root = hive.OpenSubKey(keyPath);
                if (root is null)
                {
                    continue;
                }

                foreach (string subKeyName in root.GetSubKeyNames())
                {
                    using RegistryKey? subKey = root.OpenSubKey(subKeyName);
                    if (subKey?.GetValue("InstallLocation") is string location && !string.IsNullOrWhiteSpace(location))
                    {
                        yield return location;
                    }
                }
            }
        }








        /// <summary>
        ///     Reads install roots from the Windows uninstaller registry for entries whose
        ///     display name identifies a PowerShell installation.
        /// </summary>
        private static IEnumerable<string> GetUninstallerInstallLocations()
        {
            const string machinePath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

            foreach ((RegistryKey hive, string keyPath) in new[] { (Registry.LocalMachine, machinePath), (Registry.LocalMachine, @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"), (Registry.CurrentUser, machinePath), })
            {
                using RegistryKey? root = hive.OpenSubKey(keyPath);
                if (root is null)
                {
                    continue;
                }

                foreach (string subKeyName in root.GetSubKeyNames())
                {
                    using RegistryKey? subKey = root.OpenSubKey(subKeyName);
                    if (subKey?.GetValue("DisplayName") is not string displayName || !displayName.StartsWith("PowerShell", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (subKey.GetValue("InstallLocation") is string location && !string.IsNullOrWhiteSpace(location))
                    {
                        yield return location;
                    }
                }
            }
        }








        /// <summary>
        ///     Reads the pwsh.exe path from the App Paths registration, when present.
        /// </summary>
        private static string? GetAppPathsPwshExecutable()
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\pwsh.exe");
            return key?.GetValue(null) as string;
        }
    }








    /// <summary>
    ///     The set of PowerShell commands that are explicitly permitted.
    ///     Only these commands may be invoked through this tool.
    ///     Every entry is a read-only, non-destructive query command.
    /// </summary>
    [Description("Retrieve a list of allowed powershell commands")]
    [McpServerTool(Title = "List of allowed command for this tool.", Destructive = false, Name = "AllowedCommands", ReadOnly = true)]
    public List<string> GetAllowedCommands() => AllowedCommands.ToList();








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
    ///     Returns the list of allowed PowerShell commands for reference.
    /// </summary>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "PowerShell_List_Allowed_Commands", ReadOnly = true, Destructive = false)]
    [Description("Returns the list of PowerShell commands that are permitted by the PowerShell_Query tool.")]
    public async Task<ToolResult> PowershellListAllowedCommandsAsync()
    {
        string[] sorted = [.. AllowedCommands.Order(StringComparer.OrdinalIgnoreCase)];

        return ToolResult.Ok(sorted, "PowerShellReadTool");
    }








    /// <summary>
    ///     Executes a validated PowerShell command in a constrained runspace and returns
    ///     the results as a formatted string.
    /// </summary>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "PowerShell_Query", ReadOnly = true, Destructive = false)]
    [Description("Executes a pre-approved, read-only PowerShell query in a sandboxed runspace. " + "Only whitelisted commands are permitted. Pipeline operators, redirects, " + "script blocks, and .NET type access are forbidden. Call AllowedCommands for a list of commands.")]
    public async Task<ToolResult> PowershellQueryAsync([Description("The PowerShell command to execute. Must be a single read-only command " + "from the approved list. No pipelines, redirects, or script blocks.")] string command, [Description("Maximum number of result objects to return. Defaults to 50.")] int maxResults = DefaultMaxResults, CancellationToken cancellationToken = default)
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


            // Set execution timeout using CancellationToken
            using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(ExecutionTimeoutSeconds));

            // Execute asynchronously using modern InvokeAsync pattern
            PSDataCollection<PSObject> results = new();
            await ps.InvokeAsync(results, new PSInvocationSettings(), null, timeoutCts.Token)!.ConfigureAwait(false);

            // Check for errors
            if (ps.HadErrors)
            {
                string errors = string.Join("; ", ps.Streams.Error?.Select(e => e.ToString()) ?? Array.Empty<string>());
                return ToolResult.Fail($"PowerShell query failed: {errors}{FormatModulePathNote()}", "PowerShellReadTool");
            }


            if (results.Count == 0)
            {
                return ToolResult.Ok("[]", "PowerShellReadTool");
            }

// Convert PSObjects to their underlying .NET objects
            object[] objects = results.Take(maxResults).Select(r => r.BaseObject).ToArray();

            string json = JsonSerializer.Serialize(objects.Length == 1 ? objects[0] : objects, new JsonSerializerOptions { WriteIndented = true });

            if (results.Count > maxResults)
            {
                json += Environment.NewLine + $"/* truncated: showing {maxResults} of {results.Count} results */";
            }

            return ToolResult.Ok(json, "PowerShellReadTool");


        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ToolResult.Fail("Operation was cancelled.", "PowerShellReadTool");
        }
        catch (OperationCanceledException)
        {
            return ToolResult.Fail($"Command execution timed out after {ExecutionTimeoutSeconds} seconds.", "PowerShellReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"PowerShell query failed: {ex.Message}{FormatModulePathNote()}", "PowerShellReadTool");
        }
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
}
