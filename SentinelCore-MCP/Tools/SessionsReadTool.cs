// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         SessionsReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  082808



using System.ComponentModel;
using System.Runtime.Versioning;

using ModelContextProtocol.Server;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for enumerating active user sessions on the local machine.
///     Uses the WTS API via <see cref="Interop.SessionHelper" /> instead of shelling
///     out to the <c>query session</c> command.
/// </summary>
[McpServerToolType]
[SupportedOSPlatform("windows")]
public sealed class SessionsReadTool
{

    /// <summary>
    ///     Lists active user sessions (console, RDP, etc.) on the local machine.
    /// </summary>
    /// <param name="maxRecords">Maximum number of sessions to return. Defaults to 50.</param>
    /// <returns>A <see cref="ToolResult" /> containing typed session records.</returns>
    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Sessions_List_Active", ReadOnly = true, Destructive = false)]
    [Description("Lists active user sessions (console, RDP, etc.) on the local machine.")]
    public async Task<ToolResult> SessionsListActiveAsync([Description("Maximum number of sessions to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            ToolResult? maxValidation = InputValidator.ValidateMaxRecords(maxRecords);
            if (maxValidation is not null)
            {
                return maxValidation;
            }

            return await Interop.SessionHelper.EnumerateSessionsAsync(maxRecords).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "Active session listing");
        }
    }
}