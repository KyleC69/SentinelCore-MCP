// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         PnpDeviceReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801




using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Diagnostics;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Plug and Play devices through the pnputil.exe command.
///     Only non-destructive pnputil options are permitted; any disallowed option is rejected.
/// </summary>
[McpServerToolType]
public sealed class PnpDeviceReadTool
{

    private static readonly HashSet<string> AllowedOptions = new(StringComparer.OrdinalIgnoreCase)
    {
            "/enum-devices",
            "/device-info",
            "/properties",
            "/status",
            "/class",
            "/connected",
            "/problem",
            "/ids",
            "/instanceids",
            "/format:csv",
            "/format:table",
            "/?"
    };

    private static readonly HashSet<string> DisallowedOptions = new(StringComparer.OrdinalIgnoreCase)
    {
            "/add-driver",
            "/delete-driver",
            "/install",
            "/delete",
            "/disable",
            "/enable",
            "/remove",
            "/export-driver",
            "/import-driver",
            "/scan-devices",
            "/update-driver",
            "/export"
    };









    private static string EscapeArgument(string argument)
    {
        return argument.Contains(' ') || argument.Contains('\t') || argument.Contains('"') ? $"\"{argument.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"" : argument;

    }








    private static ToolResult? RunPnputil(List<string> arguments, out string output)
    {
        output = string.Empty;
        ProcessStartInfo startInfo = new()
        {
            FileName = "pnputil.exe",
            Arguments = string.Join(" ", arguments.Select(EscapeArgument)),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process? process = Process.Start(startInfo);
        if (process is null)
        {
            return ToolResult.Fail("Unable to start pnputil.exe.");
        }

        output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return process.ExitCode != 0 ? string.IsNullOrWhiteSpace(error) ? ToolResult.Fail($"pnputil exited with code {process.ExitCode}.") : ToolResult.Fail($"pnputil exited with code {process.ExitCode}: {error.Trim()}") : null;

    }








    private static ToolResult? ValidateArguments(IEnumerable<string> arguments)
    {
        foreach (string arg in arguments)
            if (arg.StartsWith('/'))
            {
                if (DisallowedOptions.Contains(arg))
                {
                    return ToolResult.Fail($"PnP option '{arg}' is not allowed because it is destructive or state-changing.");
                }

                if (!AllowedOptions.Contains(arg))
                {
                    return ToolResult.Fail($"PnP option '{arg}' is not in the allowed whitelist.");
                }
            }

        return null;
    }








    [McpServerTool(Name = "PnpListDevices", ReadOnly = true, Destructive = false)]
    [Description("Lists PnP devices using the pnputil /enum-devices command. Optional class and status filters are applied when provided.")]
    public static ToolResult PnpListDevices([Description("Optional class filter for the PnP devices.")] string className = "", [Description("Optional status filter for the PnP devices.")] string status = "")
    {
        try
        {
            List<string> args = new() { "/enum-devices" };

            if (!string.IsNullOrWhiteSpace(className))
            {
                args.Add("/class");
                args.Add(className);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("Problem", StringComparison.OrdinalIgnoreCase))
                {
                    args.Add("/problem");
                }
                else if (status.Equals("Connected", StringComparison.OrdinalIgnoreCase))
                {
                    args.Add("/connected");
                }
            }

            ToolResult? validation = ValidateArguments(args);
            if (validation is not null)
            {
                return validation;
            }

            ToolResult? error = RunPnputil(args, out string output);
            return error ?? ToolResult.Ok(output);
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"PnP device listing failed: {ex.Message}");
        }
    }








    [McpServerTool(Name = "Pnp_Read_Device", ReadOnly = true, Destructive = false)]
    [Description("Reads detailed properties for a specific PnP device using the pnputil /device-info command.")]
    public static ToolResult PnpReadDevice([Description("The ID of the PnP device to read.")] string deviceId)
    {

        try
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return ToolResult.Fail("deviceId is required.");
            }

            List<string> args = new() { "/device-info", deviceId };
            ToolResult? validation = ValidateArguments(args);
            if (validation is not null)
            {
                return validation;
            }

            ToolResult? err = RunPnputil(args, out string result);
            return err ?? ToolResult.Ok(result);
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"PnP device read failed: {ex.Message}");
        }
    }
}
