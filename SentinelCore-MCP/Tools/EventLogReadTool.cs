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
using System.Text.Json;




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
    public static ToolResult EventLogListChannels()
    {
        try
        {
            StringBuilder sb = new();
            foreach (string? logName in EventLogSession.GlobalSession.GetLogNames()) sb.AppendLine(logName);

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Event log channel listing failed.");
        }
    }







    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Event_Log_Query", ReadOnly = true, Destructive = false)]
    [Description("Queries events from a specific event log channel.")]
    public static ToolResult EventLogQuery([Description("The event log channel name, e.g. Application or System.")] string channel, [Description("Optional XPath filter expression. Defaults to all events.")] string? query = null, [Description("Maximum number of events to return. Defaults to 50.")] int maxEvents = 50)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                return ToolResult.Fail("channel is required.");
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

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Event log query failed.");
        }
    }







    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Event_Log_Read_Configuration", ReadOnly = true, Destructive = false)]
    [Description("Reads event log configuration such as retention and file size.")]
    public static ToolResult EventLogReadConfiguration([Description("The event log channel name.")] string channel)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                return ToolResult.Fail("channel is required.");
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

            return ToolResult.Ok(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            return ToolResult.Fail("Event log configuration read failed.");
        }
    }
}
