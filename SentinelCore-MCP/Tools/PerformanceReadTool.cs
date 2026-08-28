// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         PerformanceReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801




using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Windows performance counters (PDH API surface).
///     Uses the .NET PerformanceCounter wrapper, which internally uses PDH.
/// </summary>
[McpServerToolType]
public sealed class PerformanceReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Performance_List_Categories", ReadOnly = true, Destructive = false)]
    [Description("Lists performance counter categories available on the system.")]
    public async Task<ToolResult> performanceListCategoriesAsync()
    {
        try
        {
            var categories = PerformanceCounterCategory.GetCategories().Select(c => c.CategoryName).OrderBy(n => n).ToList();

            return ToolResult.Ok(categories, "PerformanceReadTool");
        }
        catch
        {
            return ToolResult.Fail("Performance category listing failed.", "PerformanceReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Performance_List_Counters", ReadOnly = true, Destructive = false)]
    [Description("Lists counters for a given performance counter category and optional instance.")]
    public async Task<ToolResult> performanceListCountersAsync([Description("The performance counter category name, e.g. Processor.")] string categoryName, [Description("Optional instance name, e.g. _Total.")] string? instanceName = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return ToolResult.Fail("categoryName is required.", "PerformanceReadTool");
            }

            PerformanceCounterCategory category = new(categoryName);
            StringBuilder sb = new();
            sb.AppendLine($"Category={categoryName}");
            sb.AppendLine("Counters:");
            foreach (PerformanceCounter? counter in category.GetCounters(instanceName ?? string.Empty))
                sb.AppendLine($"  {counter.CounterName}");

            return ToolResult.Ok(sb.ToString(), "PerformanceReadTool");
        }
        catch
        {
            return ToolResult.Fail("Performance counter listing failed.", "PerformanceReadTool");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Performance_Read_Counter", ReadOnly = true, Destructive = false)]
    [Description("Reads the current value of a performance counter.")]
    public async Task<ToolResult> performanceReadCounterAsync([Description("The performance counter category name.")] string categoryName, [Description("The counter name, e.g. % Processor Time.")] string counterName, [Description("Optional instance name, e.g. _Total.")] string? instanceName = null, [Description("Optional machine name. Defaults to local.")] string machineName = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(categoryName) || string.IsNullOrWhiteSpace(counterName))
            {
                return ToolResult.Fail("categoryName and counterName are required.", "PerformanceReadTool");
            }

            using PerformanceCounter counter = string.IsNullOrWhiteSpace(instanceName) ? new PerformanceCounter(categoryName, counterName, machineName) : new PerformanceCounter(categoryName, counterName, instanceName, machineName);

            counter.NextValue(); // prime
            float value = counter.NextValue();
            return ToolResult.Ok($"Counter={categoryName}/{counterName}[{instanceName ?? "(none)"}]={value}", "PerformanceReadTool");
        }
        catch
        {
            return ToolResult.Fail("Performance counter read failed.", "PerformanceReadTool");
        }
    }
}
