// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         EnvironmentVariablesReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using ModelContextProtocol.Server;
using System.Runtime.Versioning;

using System.Collections;
using System.ComponentModel;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying environment variables.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class EnvironmentVariablesReadTool
{








    [McpServerTool(Name = "Environment_Variables_List", ReadOnly = true, Destructive = false)]
    [Description("Lists environment variables for the current process, user, or machine.")]
    public async Task<ToolResult> EnvironmentListAsync([Description("The target scope: Process, User, or Machine. Defaults to Process.")] EnvironmentVariableTarget target = EnvironmentVariableTarget.Process, [Description("Maximum number of variables to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            IDictionary variables = Environment.GetEnvironmentVariables(target);
            StringBuilder sb = new();
            int count = 0;
            foreach (DictionaryEntry entry in variables)
            {
                if (count >= maxRecords)
                {
                    break;
                }

                sb.AppendLine($"{entry.Key}={entry.Value}");
                count++;
            }

            return ToolResult.Ok(sb.ToString(), "EnvironmentVariablesReadTool");
        }
        catch
        {
            return ToolResult.Fail("Environment variable listing failed.", "EnvironmentVariablesReadTool");
        }
    }








    [McpServerTool(Name = "Environment_Variables_Read_Value", ReadOnly = true, Destructive = false)]
    [Description("Reads a specific environment variable for the current process, user, or machine.")]
    public async Task<ToolResult> EnvironmentReadValueAsync([Description("The name of the environment variable.")] string variableName, [Description("The target scope: Process, User, or Machine. Defaults to Process.")] EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(variableName))
            {
                return ToolResult.Fail("variableName is required.", "EnvironmentVariablesReadTool");
            }

            string? value = Environment.GetEnvironmentVariable(variableName, target);
            return value is null ? ToolResult.Fail($"Environment variable not found: {variableName} ({target})", "EnvironmentVariablesReadTool") : ToolResult.Ok($"{variableName}={value}", "EnvironmentVariablesReadTool");

        }
        catch
        {
            return ToolResult.Fail("Environment variable read failed.", "EnvironmentVariablesReadTool");
        }
    }
}
