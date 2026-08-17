// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         AuditingTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Diagnostics;




namespace SentinelCoreMCP.Tools;





[McpServerToolType]
public sealed class AuditingTool
{








    [McpServerTool(Name = "Auditing", ReadOnly = true, Destructive = false)]
    [Description("Gets the auditing policy by running command auditpol.exe /get /category:*")]
    public static ToolResult GetAuditPolicy()
    {
        try
        {
            Process process = new();
            process.StartInfo.FileName = "auditpol.exe";
            process.StartInfo.Arguments = "/get /category:*";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();

            return ToolResult.Ok(process.StandardOutput.ReadToEnd());
        }
        catch
        {
            return ToolResult.Fail("Audit policy query failed.");
        }
    }
}
