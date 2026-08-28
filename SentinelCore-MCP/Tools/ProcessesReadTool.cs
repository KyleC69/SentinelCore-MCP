// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         ProcessesReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801




using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for enumerating running processes and basic metadata.
///     Uses the managed Process API (a safe read-only interface over ToolHelp32Snapshot / NtQuery APIs).
/// </summary>
[SupportedOSPlatform("windows")]
[McpServerToolType]
public sealed class ProcessesReadTool
{








    private T? SafeGet<T>(Func<T> getter)
    {
        try
        {
            return getter();
        }
        catch (Exception)
        {
            return default;
        }
    }








    [McpServerTool(Name = "Process_List", ReadOnly = true, Destructive = false)]
    [Description("Lists running processes with PID, name, and basic metadata.")]
    public async Task<ToolResult> ProcessListAsync([Description("Optional process name filter (partial match).")] string? nameFilter = null, [Description("Maximum number of processes to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            List<object> results = new();
            foreach (Process process in Process.GetProcesses())
                try
                {
                    if (!string.IsNullOrWhiteSpace(nameFilter) && process.ProcessName.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    if (results.Count >= maxRecords)
                    {
                        break;
                    }

                    results.Add(new
                    {
                        process.Id,
                        process.ProcessName,
                        process.MainWindowTitle,
                        process.SessionId,
                        process.Responding,
                        StartTime = SafeGet(() => process.StartTime),
                        WorkingSet = process.WorkingSet64,
                        PagedMemorySize = process.PagedMemorySize64
                    });
                }
                catch (Exception)
                {
                    // Skip processes we cannot inspect (e.g., protected/elevated).
                }

            return ToolResult.Ok(results, "ProcessesReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "Process listing failed.");
        }
    }








    [McpServerTool(Name = "Process_Read", ReadOnly = true, Destructive = false)]
    [Description("Reads details for a specific process by PID.")]
    public async Task<ToolResult> ProcessReadAsync([Description("The process identifier.")] int processId)
    {
        try
        {
            using Process process = Process.GetProcessById(processId);
            process.Refresh();

            List<object> moduleList = new();
            try
            {
                foreach (ProcessModule module in process.Modules)
                    moduleList.Add(new { ModuleName = module.ModuleName, FileName = module.FileName });
            }
            catch (Exception)
            {
                moduleList.Add(new { ModuleName = "(unavailable)", FileName = "(unavailable)" });
            }

            var processInfo = new
            {
                Id = process.Id,
                Name = process.ProcessName,
                MainWindowTitle = process.MainWindowTitle,
                SessionId = process.SessionId,
                Responding = process.Responding,
                StartTime = SafeGet(() => process.StartTime),
                WorkingSet64 = process.WorkingSet64,
                PagedMemorySize64 = process.PagedMemorySize64,
                VirtualMemorySize64 = process.VirtualMemorySize64,
                HandleCount = process.HandleCount,
                ThreadCount = process.Threads.Count,
                Modules = moduleList
            };
            return ToolResult.Ok(processInfo, "ProcessesReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "Process read failed.");
        }
    }
}
