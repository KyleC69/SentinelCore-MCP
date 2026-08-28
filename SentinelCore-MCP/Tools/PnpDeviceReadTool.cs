// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         PnpDeviceReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Plug and Play devices through the pnputil.exe command.
///     Only non-destructive pnputil options are permitted; any disallowed option is rejected.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
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








    private static string LimitOutput(string output, int maxRecords)
    {
        if (maxRecords <= 0)
        {
            return output;
        }

        string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        StringBuilder sb = new();
        int deviceCount = 0;
        foreach (string line in lines)
        {
            if (line.StartsWith("Device", StringComparison.OrdinalIgnoreCase) || line.Contains(":"))
            {
                // Counting device entries by header pattern in pnputil output
                if (line.StartsWith("Device", StringComparison.OrdinalIgnoreCase))
                {
                    deviceCount++;
                }
            }

            if (deviceCount > maxRecords)
            {
                sb.AppendLine($"\n... Output truncated to {maxRecords} device(s).");
                break;
            }

            sb.AppendLine(line);
        }

        return sb.ToString().TrimEnd();
    }








    [McpServerTool(Name = "PnpListDevices", ReadOnly = true, Destructive = false)]
    [Description("Lists PnP devices using the pnputil /enum-devices command. Optional class and status filters are applied when provided. Results are limited to maxRecords.")]
    public async Task<ToolResult> PnpListDevicesAsync([Description("Optional class filter for the PnP devices.")] string className = "", [Description("Optional status filter for the PnP devices.")] string status = "", [Description("Maximum number of device records to return. Defaults to 50.")] int maxRecords = 50)
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
            if (error is not null)
            {
                return error;
            }

            string limited = LimitOutput(output, maxRecords);
            return ToolResult.Ok(limited, "PnpDeviceReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"PnP device listing failed: {ex.Message}", "PnpDeviceReadTool");
        }
    }








    [McpServerTool(Name = "Pnp_Read_Device", ReadOnly = true, Destructive = false)]
    [Description("Reads detailed properties for a specific PnP device using the pnputil /device-info command.")]
    public async Task<ToolResult> PnpReadDeviceAsync([Description("The ID of the PnP device to read.")] string deviceId)
    {

        try
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return ToolResult.Fail("deviceId is required.", "PnpDeviceReadTool");
            }

            List<string> args = new() { "/device-info", deviceId };
            ToolResult? validation = ValidateArguments(args);
            if (validation is not null)
            {
                return validation;
            }

            ToolResult? err = RunPnputil(args, out string result);
            return err ?? ToolResult.Ok(result, "PnpDeviceReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"PnP device read failed: {ex.Message}", "PnpDeviceReadTool");
        }
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
            return ToolResult.Fail("Unable to start pnputil.exe.", "PnpDeviceReadTool");
        }

        output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return process.ExitCode != 0 ? string.IsNullOrWhiteSpace(error) ? ToolResult.Fail($"pnputil exited with code {process.ExitCode}.", "PnpDeviceReadTool") : ToolResult.Fail($"pnputil exited with code {process.ExitCode}: {error.Trim()}", "PnpDeviceReadTool") : null;

    }








    private static ToolResult? ValidateArguments(IEnumerable<string> arguments)
    {
        foreach (string arg in arguments)
            if (arg.StartsWith('/'))
            {
                if (DisallowedOptions.Contains(arg))
                {
                    return ToolResult.Fail($"PnP option '{arg}' is not allowed because it is destructive or state-changing.", "PnpDeviceReadTool");
                }

                if (!AllowedOptions.Contains(arg))
                {
                    return ToolResult.Fail($"PnP option '{arg}' is not in the allowed whitelist.", "PnpDeviceReadTool");
                }
            }

        return null;
    }
}