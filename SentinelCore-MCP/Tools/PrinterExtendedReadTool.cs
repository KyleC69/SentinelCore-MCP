// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         PrinterExtendedReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying print spooler jobs for print spooler vulnerability investigation.
/// </summary>
[McpServerToolType]
public sealed class PrinterExtendedReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Printer_List_Jobs", ReadOnly = true, Destructive = false)]
    [Description("Lists current print spooler jobs for print spooler vulnerability investigation.")]
    public static ToolResult PrinterListJobs([Description("Maximum number of jobs to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();

            System.Diagnostics.ProcessStartInfo psi = new()
            {
                FileName = "powershell",
                Arguments = "-NoProfile -Command \"Get-PrintJob | Select-Object -Property PrinterName,JobName,UserName,SubmittedTime,JobStatus,Size -First " + maxRecords + " | ConvertTo-Json -Compress\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return ToolResult.Fail("Unable to start PowerShell for print job query.");
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (string.IsNullOrWhiteSpace(output.Trim()))
            {
                return ToolResult.Ok("No print jobs found.");
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(output);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in doc.RootElement.EnumerateArray())
                    {
                        if (results.Count >= maxRecords) break;
                        results.Add(new
                        {
                            PrinterName = item.TryGetProperty("PrinterName", out JsonElement pn) ? pn.GetString() ?? "" : "",
                            JobName = item.TryGetProperty("JobName", out JsonElement jn) ? jn.GetString() ?? "" : "",
                            UserName = item.TryGetProperty("UserName", out JsonElement un) ? un.GetString() ?? "" : "",
                            SubmittedTime = item.TryGetProperty("SubmittedTime", out JsonElement st) ? st.GetString() ?? "" : "",
                            JobStatus = item.TryGetProperty("JobStatus", out JsonElement js) ? js.GetString() ?? "" : "",
                            Size = item.TryGetProperty("Size", out JsonElement sz) ? sz.GetInt64() : 0
                        });
                    }
                }
            }
            catch (JsonException)
            {
                return ToolResult.Ok(output);
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Print job listing failed.");
        }
    }
}