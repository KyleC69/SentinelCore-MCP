// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         ScheduledTaskReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801

using System.ComponentModel;
using System.Text;

using Microsoft.Win32.TaskScheduler;
using ModelContextProtocol.Server;

using ScheduledTask = Microsoft.Win32.TaskScheduler.Task;

namespace SentinelCoreMCP.Tools;

/// <summary>
///     Read-only tool for querying Windows Scheduled Tasks.
/// </summary>
[McpServerToolType]
public sealed class ScheduledTaskReadTool
{
    [McpServerTool(Name = "Scheduled_Task_List", ReadOnly = true, Destructive = false)]
    [Description("Lists scheduled tasks in the specified folder path.")]
    public static ToolResult ScheduledTaskList([Description("The task folder path, e.g. \\ or \\Microsoft\\Windows.")] string folderPath = "\\", [Description("Maximum number of tasks to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            using TaskService taskService = new();
            TaskFolder? folder = taskService.GetFolder(folderPath);
            if (folder is null)
            {
                return ToolResult.Fail($"Task folder not found: {folderPath}");
            }

            StringBuilder sb = new();
            int count = 0;
            foreach (ScheduledTask scheduledTask in folder.Tasks)
            {
                if (count >= maxRecords)
                {
                    break;
                }

                sb.AppendLine($"Name={scheduledTask.Name}, Path={scheduledTask.Path}, State={scheduledTask.State}, Enabled={scheduledTask.Enabled}");
                count++;
            }

            foreach (TaskFolder subFolder in folder.SubFolders)
            {
                sb.AppendLine($"[Folder] {subFolder.Path}");
            }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Scheduled task listing failed.");
        }
    }

    [McpServerTool(Name = "Scheduled_Task_Read", ReadOnly = true, Destructive = false)]
    [Description("Reads details of a specific scheduled task.")]
    public static ToolResult ScheduledTaskRead([Description("The full task path, e.g. \\Microsoft\\Windows\\Defender\\Defender Scheduled Scan.")] string taskPath)
    {
        if (string.IsNullOrWhiteSpace(taskPath))
        {
            return ToolResult.Fail("taskPath is required.");
        }

        try
        {
            using TaskService taskService = new();
            ScheduledTask? scheduledTask = taskService.GetTask(taskPath);
            if (scheduledTask is null)
            {
                return ToolResult.Fail($"Task not found: {taskPath}");
            }

            StringBuilder sb = new();
            sb.AppendLine($"Name={scheduledTask.Name}");
            sb.AppendLine($"Path={scheduledTask.Path}");
            sb.AppendLine($"Enabled={scheduledTask.Enabled}");
            sb.AppendLine($"State={scheduledTask.State}");
            sb.AppendLine($"LastRunTime={scheduledTask.LastRunTime}");
            sb.AppendLine($"NextRunTime={scheduledTask.NextRunTime}");
            sb.AppendLine($"LastTaskResult={scheduledTask.LastTaskResult}");
            sb.AppendLine($"NumberOfMissedRuns={scheduledTask.NumberOfMissedRuns}");
            sb.AppendLine($"Definition.Triggers.Count={scheduledTask.Definition.Triggers.Count}");
            sb.AppendLine($"Definition.Actions.Count={scheduledTask.Definition.Actions.Count}");
            sb.AppendLine($"Definition.Settings.AllowDemandStart={scheduledTask.Definition.Settings.AllowDemandStart}");
            sb.AppendLine($"Definition.Settings.StartWhenAvailable={scheduledTask.Definition.Settings.StartWhenAvailable}");

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Scheduled task read failed.");
        }
    }
}
