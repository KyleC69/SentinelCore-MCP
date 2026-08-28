// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         LocalAccountsReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801

using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.Text;

using System.Runtime.Versioning;

using ModelContextProtocol.Server;

namespace SentinelCoreMCP.Tools;


/// <summary>
///     Read-only tool for querying local users and groups.
/// </summary>
[McpServerToolType]
public sealed class LocalAccountsReadTool
{

    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Local_Accounts_List_Groups", ReadOnly = true, Destructive = false)]
    [Description("Lists local groups and their members.")]
    public async Task<ToolResult> LocalGroupListAsync([Description("Optional group name to filter. If provided, members of that group are listed.")] string? groupName = null, [Description("Maximum number of groups to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            StringBuilder sb = new();

            using PrincipalContext context = new(ContextType.Machine);
            using GroupPrincipal filter = new(context);
            using PrincipalSearcher searcher = new(filter);

            PrincipalSearchResult<Principal> results = searcher.FindAll();
            int count = 0;
            foreach (GroupPrincipal group in results.OfType<GroupPrincipal>())
            {
                if (!string.IsNullOrWhiteSpace(groupName) && !group.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (count >= maxRecords)
                {
                    break;
                }

                sb.AppendLine($"Group={group.Name}, Description={group.Description}");

                PrincipalSearchResult<Principal> members = group.GetMembers();
                foreach (Principal member in members)
                    sb.AppendLine($"  Member={member.Name} ({member.StructuralObjectClass})");

                count++;
            }

            return ToolResult.Ok(sb.ToString(), "LocalAccountsReadTool");
        }
        catch
        {
            return ToolResult.Fail("Local group listing failed.", "LocalAccountsReadTool");
        }
    }

    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Local_Accounts_List_Users", ReadOnly = true, Destructive = false)]
    [Description("Lists local user accounts on the system.")]
    public async Task<ToolResult> LocalUserListAsync([Description("Maximum number of users to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            StringBuilder sb = new();

            using PrincipalContext context = new(ContextType.Machine);
            using UserPrincipal filter = new(context);
            using PrincipalSearcher searcher = new(filter);

            PrincipalSearchResult<Principal> results = searcher.FindAll();
            int count = 0;
            foreach (UserPrincipal user in results.OfType<UserPrincipal>())
            {
                if (count >= maxRecords)
                {
                    break;
                }

                sb.AppendLine($"Name={user.Name}, Enabled={user.Enabled}, LastLogon={user.LastLogon}, PasswordNeverExpires={user.PasswordNeverExpires}, UserCannotChangePassword={user.UserCannotChangePassword}");
                count++;
            }

            return ToolResult.Ok(sb.ToString(), "LocalAccountsReadTool");
        }
        catch
        {
            return ToolResult.Fail("Local user listing failed.", "LocalAccountsReadTool");
        }
    }
}
