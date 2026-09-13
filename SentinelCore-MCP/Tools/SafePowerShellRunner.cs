// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num:  ${CurrentDate.Month}${CurrentDate.Day}${CurrentDate.Hour}
//



using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;




/*
 AGENTS: THIS IS AN AI TOOL IN AN MCP SERVER SOME DESIGN PATTERNS DO NOT APPLY.
 FOR EXAMPLE LOGGING IS DONE THROUGH THE RETURN OBJECT.
 WHEN IT THROWS, THE EXCEPTION MESSAGE MUST BE RETURNED IN THE RESULT OBJECT
 ALL EXCEPTIONS MUST BE CAUGHT AND RETURNED AS A FAIL OBJECT, THIS IS INTENTIONAL DESIGN.
 THIS CLASS IS A COMBINATION OF A WHITELIST AND DENY BY DESIGN PATTERNS.
 NOTHING SHOULD THROW AN EXCEPTION OUTSIDE THE TRY/CATCH, THIS IS INTENTIONAL DESIGN.
The hardcoded sandbox path is J:\AgentSandbox, this is intentional design for testing purposes only.
*/



public sealed class SafePowerShellRunner
{
    private const int MaxOutputLength = 10_000;
    private static readonly string SandboxRoot = Path.GetFullPath(@"J:\AgentSandbox");

    // made internal for testing purposes, can be called as datasource for test methods
    internal static readonly HashSet<string> AllowedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "Get-Command",
        "Get-Help",
        "Get-Member",
        "Get-Variable",
        "Get-Alias",
        "Get-PSDrive",
        "Get-PSProvider",
        "Get-History",
        "Get-Host",
        "Get-Item",
        "Get-ChildItem",
        "Get-Content",
        "Set-Content",
        "Add-Content",
        "Clear-Content",
        "Test-Path",
        "Join-Path",
        "Split-Path",
        "Resolve-Path",
        "Convert-Path",
        "Copy-Item",
        "Move-Item",
        "Rename-Item",
        "Remove-Item",
        "Get-Date",
        "Get-Random",
        "Get-Location",
        "Set-Location",
        "New-Item",
        "New-PSDrive"
    };

    // made internal for testing purposes, can be called as datasource for test methods
    internal static readonly HashSet<string> AllowedVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "Get",
        "Set",
        "Add",
        "Clear",
        "Test",
        "Join",
        "Split",
        "Resolve",
        "Convert",
        "Copy",
        "Move",
        "Rename",
        "Remove",
        "New"
    };

    // made internal for testing purposes
    internal string CapOutput(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length <= MaxOutputLength)
        {
            return text;
        }

        return text.Substring(0, MaxOutputLength) + "\n\n[Output truncated for safety]";
    }

    public static Runspace CreateConstrainedRunspace(HashSet<string> allowedCommands)
    {
        if (allowedCommands == null)
        {
            throw new ArgumentNullException(nameof(allowedCommands), "Allowed commands set cannot be null.");
        }

        try
        {
            InitialSessionState? initialSessionState = InitialSessionState.CreateDefault();
            if (initialSessionState == null)
            {
                throw new InvalidOperationException("Failed to create InitialSessionState.");
            }

            initialSessionState.LanguageMode = PSLanguageMode.NoLanguage;

            // Remove all providers except FileSystem
            List<SessionStateProviderEntry> providersToRemove = new();
            if (initialSessionState.Providers == null)
            {
                throw new InvalidOperationException("InitialSessionState.Providers is null.");
            }

            foreach (SessionStateProviderEntry provider in initialSessionState.Providers)
            {
                if (!string.Equals(provider.Name, "FileSystem", StringComparison.OrdinalIgnoreCase))
                {
                    providersToRemove.Add(provider);
                }
            }

            foreach (SessionStateProviderEntry provider in providersToRemove)
            {
                initialSessionState.Providers.Remove(provider.Name, null);
            }

            // Filter commands to only include whitelisted ones
            List<SessionStateCommandEntry> commandsToRemove = new();
            foreach (SessionStateCommandEntry command in initialSessionState.Commands)
            {
                if (!allowedCommands.Contains(command.Name))
                {
                    commandsToRemove.Add(command);
                }
            }

            foreach (SessionStateCommandEntry command in commandsToRemove)
            {
                initialSessionState.Commands.Remove(command.Name, null);
            }

            // Clear variables
            initialSessionState.Variables.Clear();

            Runspace? runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            return runspace;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error creating constrained runspace: {ex.Message}");
            throw;
        }
    }

    [Description("Runs a PowerShell command in a constrained and safe environment. The command must be whitelisted and parameters are validated.")]
    public PowerShellResult RunSafeCommand(string command, Dictionary<string, object> parameters)
    {
        // -------------------------------
        // Layer 1: Command whitelist
        // -------------------------------
        if (!AllowedCommands.Contains(command))
        {
            return PowerShellResult.Fail($"Command '{command}' is not allowed.");
        }

        // -------------------------------
        // Layer 2: Verb whitelist
        // -------------------------------
        string[] parts = command.Split('-', 2);
        if (parts.Length != 2)
        {
            return PowerShellResult.Fail("Invalid command format. Commands must follow the format 'Verb-Noun'.");
        }

        string verb = parts[0];
        if (!AllowedVerbs.Contains(verb))
        {
            return PowerShellResult.Fail($"Verb '{verb}' is not allowed.");
        }

        // -------------------------------
        // Layer 3: Parameter validation
        // -------------------------------
        foreach (KeyValuePair<string, object> kvp in parameters)
        {
            ValidationResult validationResult = ValidateParameter(command, kvp.Key, kvp.Value);
            if (!validationResult.IsValid)
            {
                return PowerShellResult.Fail(validationResult.FailureReason);
            }
        }

        // -------------------------------
        // Layer 4: Constrained runspace
        // -------------------------------
        using Runspace runspace = CreateConstrainedRunspace(AllowedCommands);
        using PowerShell ps = PowerShell.Create();
        ps.Runspace = runspace;
        ps.AddCommand(command);
        foreach (KeyValuePair<string, object> kvp in parameters)
        {
            ps.AddParameter(kvp.Key, kvp.Value);
        }

        Collection<PSObject>? results = ps.Invoke();
        if (ps.Streams.Error.Count > 0)
        {
            string errors = string.Join("\n", ps.Streams.Error.Select(e => e.ToString()));
            return PowerShellResult.Fail($"PowerShell execution errors: {errors}");
        }

        // -------------------------------
        // Layer 5: Output cap
        // -------------------------------
        string output = string.Join("\n", results.Select(r => r.ToString()));
        output = CapOutput(output);
        return PowerShellResult.Ok(output);
    }

    private static bool IsPathParameter(string name)
    {
        // Any parameter that can reasonably contain a path must be sandboxed
        return name.Equals("Path", StringComparison.OrdinalIgnoreCase)
               || name.Equals("LiteralPath", StringComparison.OrdinalIgnoreCase)
               || name.Equals("Destination", StringComparison.OrdinalIgnoreCase)
               || name.Equals("FilePath", StringComparison.OrdinalIgnoreCase)
               || name.Equals("Include", StringComparison.OrdinalIgnoreCase)
               || name.Equals("Exclude", StringComparison.OrdinalIgnoreCase);
    }

    private static ValidationResult ValidatePathValue(string name, string text)
    {
        try
        {
            string fullPath = Path.GetFullPath(text);

            // Normalize sandbox root once and enforce strict containment
            string sandbox = SandboxRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            bool isExactRoot = string.Equals(fullPath, sandbox, StringComparison.OrdinalIgnoreCase);
            bool isUnderRoot = fullPath.StartsWith(
                sandbox + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);

            if (!isExactRoot && !isUnderRoot)
            {
                return ValidationResult.Fail($"Parameter '{name}' specifies a path outside the sandbox.");
            }

            // Prevent obvious traversal attempts even if they normalize inside
            if (text.Contains("..", StringComparison.Ordinal))
            {
                return ValidationResult.Fail($"Parameter '{name}' contains directory traversal segments.");
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            return ValidationResult.Fail($"Parameter '{name}' contains an invalid path: {ex.Message}");
        }
    }

    private static ValidationResult ValidateParameter(string command, string name, object value)
    {
        if (value is null)
        {
            return ValidationResult.Fail($"Parameter '{name}' cannot be null.");
        }

        if (value is ScriptBlock)
        {
            return ValidationResult.Fail($"Parameter '{name}' cannot be a ScriptBlock.");
        }

        if (value is Array)
        {
            return ValidationResult.Fail($"Parameter '{name}' cannot be an array.");
        }

        if (value is Hashtable)
        {
            return ValidationResult.Fail($"Parameter '{name}' cannot be a hashtable.");
        }

        string? text = value.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Fail($"Parameter '{name}' cannot be empty or whitespace.");
        }

        HashSet<char> forbiddenChars = new()
        {
            '|',
            ';',
            '&',
            '>',
            '<',
            '*',
            '?',
            '$',
            '@',
            '(',
            ')',
            '[',
            ']',
            '{',
            '}',
            '`'
        };
        if (text.Any(forbiddenChars.Contains))
        {
            return ValidationResult.Fail($"Parameter '{name}' contains forbidden characters.");
        }

        if (text.Contains("$("))
        {
            return ValidationResult.Fail($"Parameter '{name}' contains forbidden $() expressions.");
        }

        // Harden all path-like parameters, not just "Path"
        if (IsPathParameter(name))
        {
            ValidationResult pathResult = ValidatePathValue(name, text);
            if (!pathResult.IsValid)
            {
                return pathResult;
            }
        }

        // Prevent name-based path injection for New-Item
        if (command.Equals("New-Item", StringComparison.OrdinalIgnoreCase)
            && name.Equals("Name", StringComparison.OrdinalIgnoreCase))
        {
            if (text.Contains("..", StringComparison.Ordinal))
            {
                return ValidationResult.Fail($"Parameter '{name}' cannot contain directory traversal segments.");
            }

            if (text.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, ':' }) >= 0)
            {
                return ValidationResult.Fail($"Parameter '{name}' cannot contain path separators or drive specifiers.");
            }
        }

        // Block creation of links that can bridge the sandbox
        if (command.Equals("New-Item", StringComparison.OrdinalIgnoreCase)
            && name.Equals("ItemType", StringComparison.OrdinalIgnoreCase))
        {
            if (text.Equals("SymbolicLink", StringComparison.OrdinalIgnoreCase)
                || text.Equals("Junction", StringComparison.OrdinalIgnoreCase)
                || text.Equals("HardLink", StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Fail("Creation of links (SymbolicLink, Junction, HardLink) is not allowed in the sandbox.");
            }
        }

        return ValidationResult.Success();
    }
}
