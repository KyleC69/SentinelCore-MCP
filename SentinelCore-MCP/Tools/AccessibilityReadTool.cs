// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         AccessibilityReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using Microsoft.Win32;

using ModelContextProtocol.Server;

using SentinelCoreMCP.Tools.Interop;

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying accessibility and ease-of-access settings via the UI Automation COM API.
/// </summary>
[SupportedOSPlatform("windows")]
[McpServerToolType]
public sealed class AccessibilityReadTool
{

    private const string AccessibilityKey = "Control Panel\\Accessibility";
    private const int UiaControlTypePropertyId = 30003;

    private const int UiaNamePropertyId = 30005;
    private static readonly Guid CuiAutomationClsid = new("FF48DBDA-A5CA-44D2-830A-E3AFC40F06DE");







    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Accessibility_Read", ReadOnly = true, Destructive = false)]
    [Description("Reads specific ease-of-access feature configuration such as high contrast or sticky keys.")]
    public static ToolResult AccessibilityReadFeature([Description("The feature subkey name, e.g. HighContrast, StickyKeys, ToggleKeys, MouseKeys.")] string featureName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                return ToolResult.Fail("featureName is required.");
            }

            string keyPath = $"{AccessibilityKey}\\{featureName}";
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath, false);
            if (key is null)
            {
                return ToolResult.Fail($"Accessibility feature key not found: {keyPath}");
            }

            StringBuilder sb = new();
            sb.AppendLine($"[{keyPath}]");
            foreach (string valueName in key.GetValueNames()) sb.AppendLine($"  {valueName}={key.GetValue(valueName)}");

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Accessibility feature read failed.");
        }
    }








    [McpServerTool(Name = "Accessibility_Read_Settings", ReadOnly = true, Destructive = false)]
    [Description("Reads accessibility settings from the registry.")]
    public static ToolResult AccessibilityReadSettings()
    {
        try
        {
            StringBuilder sb = new();
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(AccessibilityKey, false);
            if (key is null)
            {
                return ToolResult.Fail($"Accessibility registry key not found: {AccessibilityKey}");
            }

            sb.AppendLine($"[{AccessibilityKey}]");
            foreach (string valueName in key.GetValueNames()) sb.AppendLine($"  {valueName}={key.GetValue(valueName)}");

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Accessibility settings read failed.");
        }
    }








    [McpServerTool(Name = "Accessibility_Read_UIA_Root", ReadOnly = true, Destructive = false)]
    [Description("Reads the root UI Automation element name and control type to confirm the UI Automation API is reachable.")]
    public static ToolResult AccessibilityReadUiaRoot()
    {
        try
        {
            using SafeComObject com = new(CuiAutomationClsid);
            if (com.Instance is not IUIAutomation automation)
            {
                return ToolResult.Fail("Unable to create CUIAutomation.");
            }

            int hr = automation.GetRootElement(out var root);
            if (hr < 0)
            {
                return ToolResult.Fail("Could not retrieve UI Automation root element.");
            }

            using MarshalReleaseScope rootScope = new(root);
            root.GetCurrentPropertyValue(UiaNamePropertyId, out var nameValue);
            root.GetCurrentPropertyValue(UiaControlTypePropertyId, out var controlTypeValue);
            string? name = nameValue.ToString() != null ? nameValue.ToString() : string.Empty;
            string? controlType = controlTypeValue.ToString() != null ? controlTypeValue.ToString() : string.Empty;

            return ToolResult.Ok($"RootName={name} ControlType={controlType}");
        }
        catch
        {
            return ToolResult.Fail("UI Automation root read failed.");
        }
    }








    [ComImport]
    [Guid("30CBE57D-D9D0-452B-AB13-7AC5AC4825EE")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IUIAutomation
    {
        [PreserveSig]
        int GetRootElement(out IUIAutomationElement root);








        [PreserveSig]
        int CreatePropertyCondition(int propertyId, object value, out IUIAutomationCondition condition);
    }





    [ComImport]
    [Guid("D22108AA-8AC5-49A5-837B-37BBB3D7591E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IUIAutomationElement
    {
        [PreserveSig]
        int GetCurrentPropertyValue(int propertyId, out object value);








        [PreserveSig]
        int GetRuntimeId(out object runtimeId);








        // IUnknown v-table placeholders for unused methods.
        void Get_BoundingRectangle();


        void Get_LabeledBy();


        void Get_AriaRole();


        void Get_AriaProperties();


        void Get_ProviderDescription();


        void Get_ClickablePoint();
    }





    [ComImport]
    [Guid("3526BAE7-0970-431C-9204-4B49373050AF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IUIAutomationCondition
    {
    }





    private sealed class MarshalReleaseScope : IDisposable
    {
        private object? _obj;








        public MarshalReleaseScope(object obj)
        {
            _obj = obj;
        }








        public void Dispose()
        {
            if (_obj is not null)
            {
                Marshal.ReleaseComObject(_obj);
                _obj = null;
            }
        }
    }
}
