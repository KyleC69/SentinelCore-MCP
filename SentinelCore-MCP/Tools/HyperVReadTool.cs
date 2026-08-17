// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         HyperVReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Management.Infrastructure;

using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Hyper-V virtual machines and settings via the CIM-based Hyper-V WMI v2 namespace.
/// </summary>
[McpServerToolType]
public sealed class HyperVReadTool
{

    private const string HyperVNamespace = @"root\virtualization\v2";








    [McpServerTool(Name = "HyperV_List_Switches", ReadOnly = true, Destructive = false)]
    [Description("Lists Hyper-V virtual switches.")]
    public static ToolResult HypervListSwitches()
    {
        try
        {
            StringBuilder sb = new();
            using CimSession? session = CimSession.Create(null);
            const string query = "SELECT Name, ElementName FROM Msvm_VirtualEthernetSwitch";
            foreach (CimInstance? switchObj in session.QueryInstances(HyperVNamespace, "WQL", query))
            {
                string name = !ReferenceEquals(switchObj.CimInstanceProperties, null) && switchObj.CimInstanceProperties["Name"] != null ? switchObj.CimInstanceProperties["Name"]?.Value?.ToString() ?? string.Empty : string.Empty;
                string elementName = switchObj.CimInstanceProperties?["ElementName"] != null ? switchObj.CimInstanceProperties["ElementName"]?.Value?.ToString() ?? string.Empty : string.Empty;
                sb.AppendLine($"Name={name}, ElementName={elementName}");
            }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Hyper-V switch listing failed.");
        }
    }








    [McpServerTool(Name = "HyperV_List_VMs", ReadOnly = true, Destructive = false)]
    [Description("Lists Hyper-V virtual machines.")]
    public static ToolResult HypervListVms()
    {
        try
        {
            StringBuilder sb = new();
            using CimSession? session = CimSession.Create(null!);
            const string query = "SELECT Name, ElementName, EnabledState, HealthState FROM Msvm_ComputerSystem WHERE Caption = 'Virtual Machine'";
            foreach (CimInstance? vm in session.QueryInstances(HyperVNamespace, "WQL", query))
            {
                string name = vm.CimInstanceProperties["Name"]?.Value?.ToString() ?? string.Empty;
                string elementName = vm.CimInstanceProperties["ElementName"]?.Value?.ToString() ?? string.Empty;
                string enabledState = vm.CimInstanceProperties["EnabledState"]?.Value?.ToString() ?? string.Empty;
                string healthState = vm.CimInstanceProperties["HealthState"]?.Value?.ToString() ?? string.Empty;
                sb.AppendLine($"Name={name}, ElementName={elementName}, EnabledState={enabledState}, HealthState={healthState}");
            }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Hyper-V VM listing failed.");
        }
    }








    [McpServerTool(Name = "HyperV_Read_VM", ReadOnly = true, Destructive = false)]
    [Description("Reads settings of a specific Hyper-V virtual machine.")]
    public static ToolResult HypervReadVm([Description("The VM name (ElementName).")] string vmName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(vmName))
            {
                return ToolResult.Fail("vmName is required.");
            }

            string escaped = vmName.Replace("'", "''");
            string query = $"SELECT * FROM Msvm_ComputerSystem WHERE ElementName = '{escaped}' AND Caption = 'Virtual Machine'";
            List<Dictionary<string, object?>> results = new();
            using CimSession? session = CimSession.Create(null!);
            foreach (CimInstance? vm in session.QueryInstances(HyperVNamespace, "WQL", query))
            {
                Dictionary<string, object?> record = new();
                foreach (CimProperty? property in vm.CimInstanceProperties)
                    record[property.Name] = property.Value switch
                    {
                        Array array => string.Join("|", array.Cast<object>().Select(x => x != null ? x.ToString() != null ? x.ToString() : string.Empty : string.Empty)),
                        _ => property.Value
                    };

                results.Add(record);
            }

            if (results.Count == 0)
            {
                return ToolResult.Fail($"Hyper-V VM not found: {vmName}");
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Hyper-V VM read failed.");
        }
    }
}
