// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         EventLogReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.Versioning;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Windows Event Logs.
/// </summary>
[McpServerToolType]
public sealed class EventLogReadTool
{







    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Event_Log_List_Channels", ReadOnly = true, Destructive = false)]
    [Description("Lists available event log channels.")]
    public async Task<ToolResult> EventLogListChannelsAsync([Description("Maximum number of channels to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            StringBuilder sb = new();
            int count = 0;
            foreach (string? logName in EventLogSession.GlobalSession.GetLogNames())
            {
                if (count >= maxRecords)
                {
                    break;
                }

                sb.AppendLine(logName);
                count++;
            }

            return ToolResult.Ok(sb.ToString(), "EventLogReadTool");
        }
        catch
        {
            return ToolResult.Fail("Event log channel listing failed.", "EventLogReadTool");
        }
    }







    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Event_Log_Query", ReadOnly = true, Destructive = false)]
    [Description("Queries events from a specific event log channel.")]
    public async Task<ToolResult> EventLogQueryAsync([Description("The event log channel name, e.g. Application or System.")] string channel, [Description("Optional XPath filter expression. Defaults to all events.")] string? query = null, [Description("Maximum number of events to return. Defaults to 50.")] int maxEvents = 50)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                return ToolResult.Fail("channel is required.", "EventLogReadTool");
            }

            string xpath = string.IsNullOrWhiteSpace(query) ? "*" : query;
            StringBuilder sb = new();
            int count = 0;
            using EventLogReader reader = new(new EventLogQuery(channel, PathType.LogName, xpath));
            EventRecord? record;
            while ((record = reader.ReadEvent()) is not null && count < maxEvents)
            {
                sb.AppendLine($"TimeCreated={record.TimeCreated}, Level={record.LevelDisplayName}, Provider={record.ProviderName}, Id={record.Id}, Message={record.FormatDescription()}");
                count++;
            }

            return ToolResult.Ok(sb.ToString(), "EventLogReadTool");
        }
        catch
        {
            return ToolResult.Fail("Event log query failed.", "EventLogReadTool");
        }
    }







    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Event_Log_Read_Configuration", ReadOnly = true, Destructive = false)]
    [Description("Reads event log configuration such as retention and file size.")]
    public async Task<ToolResult> EventLogReadConfigurationAsync([Description("The event log channel name.")] string channel)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                return ToolResult.Fail("channel is required.", "EventLogReadTool");
            }

            EventLogConfiguration config = new(channel);
            var result = new
            {
                ChannelName = config.LogName,
                config.LogType,
                config.IsEnabled,
                config.MaximumSizeInBytes,
                config.LogFilePath,
                config.IsClassicLog
            };

            return ToolResult.Ok(result, "Event log configuration read.");
        }
        catch
        {
            return ToolResult.Fail("Event log configuration read failed.", "EventLogReadTool");
        }
    }
}
