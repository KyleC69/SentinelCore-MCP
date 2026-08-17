// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         WmiQueryTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Management.Infrastructure;

using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for executing CIM/MI WQL queries.
///     Uses Microsoft.Management.Infrastructure instead of legacy System.Management to align with the approved CIM/MI
///     APIs.
/// </summary>
[McpServerToolType]
public sealed class WmiQueryTool
{







    [McpServerTool(Name = "WMI_List_Classes", ReadOnly = true, Destructive = false)]
    [Description("Lists CIM class names in the specified namespace, compact output.")]
    public ToolResult wmiListClasses(
            string nameSpace = @"root\cimv2",
            string? prefix = null,
            int maxResults = 50)
    {
        try
        {
            using CimSession session = CimSession.Create(null);

            var classes = session.EnumerateClasses(nameSpace)
                    .Select(c => c.CimSystemProperties.ClassName)
                    .Where(n => prefix == null || n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .Take(maxResults)
                    .ToList();

            var result = new
            {
                    Namespace = nameSpace,
                    Count = classes.Count,
                    Classes = classes
            };

            return ToolResult.Ok(JsonSerializer.Serialize(result));
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"CIM class listing failed: {ex.Message}");
        }
    }







    [McpServerTool(Name = "WMI_Query", ReadOnly = true, Destructive = false)]
    [Description("Executes a read-only CIM WQL query and returns a compact result set.")]
    public ToolResult wmiQuery(
            string query,
            int maxRows = 25,
            int maxProperties = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return ToolResult.Fail("query is required.");

            using CimSession session = CimSession.Create(null);

            var compactRows = new List<Dictionary<string, object?>>();
            int totalRows = 0;

            foreach (var instance in session.QueryInstances(@"root\cimv2", "WQL", query))
            {
                totalRows++;

                if (compactRows.Count >= maxRows)
                    continue; // count but don't include

                var row = new Dictionary<string, object?>();

                foreach (var prop in instance.CimInstanceProperties.Take(maxProperties))
                {
                    object? value = prop.Value switch
                    {
                            CimInstance nested => nested.CimSystemProperties.ClassName,
                            Array array => string.Join(",", array.Cast<object?>().Where(x => x != null)),
                            _ => prop.Value
                    };

                    row[prop.Name] = value;
                }

                compactRows.Add(row);
            }

            var result = new
            {
                    Query = query,
                    TotalRows = totalRows,
                    ReturnedRows = compactRows.Count,
                    MaxRows = maxRows,
                    MaxProperties = maxProperties,
                    Rows = compactRows
            };

            return ToolResult.Ok(JsonSerializer.Serialize(result));
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"CIM query failed: {ex.Message}");
        }
    }

}
