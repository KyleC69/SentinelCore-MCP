// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         HyperVReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Management.Infrastructure;

using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Hyper-V virtual machines and settings via the CIM-based Hyper-V WMI v2 namespace.
/// </summary>
[SupportedOSPlatform("windows")]
[McpServerToolType]
public sealed class HyperVReadTool
{

    private const string HyperVNamespace = @"root\virtualization\v2";








    [McpServerTool(Name = "HyperV_List_Switches", ReadOnly = true, Destructive = false)]
    [Description("Lists Hyper-V virtual switches.")]
    public async Task<ToolResult> HypervListSwitchesAsync()
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

            return ToolResult.Ok(sb.ToString(), "HyperVReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "Hyper-V switch listing");
        }
    }








    [McpServerTool(Name = "HyperV_List_VMs", ReadOnly = true, Destructive = false)]
    [Description("Lists Hyper-V virtual machines.")]
    public async Task<ToolResult> HypervListVmsAsync()
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

            return ToolResult.Ok(sb.ToString(), "HyperVReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "Hyper-V VM listing");
        }
    }








    [McpServerTool(Name = "HyperV_Read_VM", ReadOnly = true, Destructive = false)]
    [Description("Reads settings of a specific Hyper-V virtual machine.")]
    public async Task<ToolResult> HypervReadVmAsync([Description("The VM name (ElementName).")] string vmName)
    {
        try
        {
            ToolResult? nameValidation = InputValidator.ValidateRequired(vmName, "vmName");
            if (nameValidation is not null) return nameValidation;

            ToolResult? sanitizedValidation = InputValidator.ValidateSanitizedWqlValue(vmName, "vmName");
            if (sanitizedValidation is not null) return sanitizedValidation;

            string query = $"SELECT * FROM Msvm_ComputerSystem WHERE ElementName = '{vmName.Replace("'", "''")}' AND Caption = 'Virtual Machine'";
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
                return ToolResult.Fail($"Hyper-V VM not found: {vmName}", "HyperVReadTool");
            }

            return ToolResult.Ok(results, "HyperVReadTool");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(ex.Message, "Hyper-V VM read");
        }
    }
}
