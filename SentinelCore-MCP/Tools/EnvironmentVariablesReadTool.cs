// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         EnvironmentVariablesReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using ModelContextProtocol.Server;

using System.Collections;
using System.ComponentModel;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying environment variables.
/// </summary>
[McpServerToolType]
public sealed class EnvironmentVariablesReadTool
{








    [McpServerTool(Name = "Environment_Variables_List", ReadOnly = true, Destructive = false)]
    [Description("Lists environment variables for the current process, user, or machine.")]
    public static ToolResult EnvironmentList([Description("The target scope: Process, User, or Machine. Defaults to Process.")] EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
    {
        try
        {
            IDictionary variables = Environment.GetEnvironmentVariables(target);
            StringBuilder sb = new();
            foreach (DictionaryEntry entry in variables) sb.AppendLine($"{entry.Key}={entry.Value}");

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Environment variable listing failed.");
        }
    }








    [McpServerTool(Name = "Environment_Variables_Read_Value", ReadOnly = true, Destructive = false)]
    [Description("Reads a specific environment variable for the current process, user, or machine.")]
    public static ToolResult EnvironmentReadValue([Description("The name of the environment variable.")] string variableName, [Description("The target scope: Process, User, or Machine. Defaults to Process.")] EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(variableName))
            {
                return ToolResult.Fail("variableName is required.");
            }

            string? value = Environment.GetEnvironmentVariable(variableName, target);
            return value is null ? ToolResult.Fail($"Environment variable not found: {variableName} ({target})") : ToolResult.Ok($"{variableName}={value}");

        }
        catch
        {
            return ToolResult.Fail("Environment variable read failed.");
        }
    }
}
