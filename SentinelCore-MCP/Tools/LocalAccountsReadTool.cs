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
    public static ToolResult LocalGroupList([Description("Optional group name to filter. If provided, members of that group are listed.")] string? groupName = null)
    {
        try
        {
            StringBuilder sb = new();

            using PrincipalContext context = new(ContextType.Machine);
            using GroupPrincipal filter = new(context);
            using PrincipalSearcher searcher = new(filter);

            PrincipalSearchResult<Principal> results = searcher.FindAll();
            foreach (GroupPrincipal group in results.OfType<GroupPrincipal>())
            {
                if (!string.IsNullOrWhiteSpace(groupName) && !group.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                sb.AppendLine($"Group={group.Name}, Description={group.Description}");

                PrincipalSearchResult<Principal> members = group.GetMembers();
                foreach (Principal member in members)
                    sb.AppendLine($"  Member={member.Name} ({member.StructuralObjectClass})");
            }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Local group listing failed.");
        }
    }

    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Local_Accounts_List_Users", ReadOnly = true, Destructive = false)]
    [Description("Lists local user accounts on the system.")]
    public static ToolResult LocalUserList()
    {
        try
        {
            StringBuilder sb = new();

            using PrincipalContext context = new(ContextType.Machine);
            using UserPrincipal filter = new(context);
            using PrincipalSearcher searcher = new(filter);

            PrincipalSearchResult<Principal> results = searcher.FindAll();
            foreach (UserPrincipal user in results.OfType<UserPrincipal>())
            {
                sb.AppendLine($"Name={user.Name}, Enabled={user.Enabled}, LastLogon={user.LastLogon}, PasswordNeverExpires={user.PasswordNeverExpires}, UserCannotChangePassword={user.UserCannotChangePassword}");
            }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Local user listing failed.");
        }
    }
}
