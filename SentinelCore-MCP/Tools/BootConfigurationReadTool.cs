// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         BootConfigurationReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying system boot configuration using the BCD store APIs.
/// </summary>
[McpServerToolType]
public sealed class BootConfigurationReadTool
{

    private static readonly Guid UnsafeNullGuid = Guid.Empty;








    [McpServerTool(Name = "Boot_Configuration_Read_Current", ReadOnly = true, Destructive = false)]
    [Description("Returns the current boot entry GUID from the BCD store.")]
    public static ToolResult BcdeditCurrent()
    {
        try
        {
            int hResult = NativeMethods.BcdOpenStore(null, out IntPtr store);
            if (hResult < 0 || store == IntPtr.Zero)
            {
                return ToolResult.Fail("Failed to open the BCD store.");
            }

            try
            {
                IntPtr guid = Marshal.AllocHGlobal(16);
                try
                {
                    int zero = 0;
                    Guid nullGuid = Guid.Empty;
                    hResult = NativeMethods.BcdGetElementData(store, ref nullGuid, BcdLibraryElementType.BcdLibraryObjectTypeCurrentBootEntry, guid, ref zero, out _);
                    if (hResult < 0)
                    {
                        return ToolResult.Fail("Failed to query current BCD entry.");
                    }

                    Guid currentGuid = Marshal.PtrToStructure<Guid>(guid);
                    return ToolResult.Ok($"CurrentBootEntry={currentGuid:B}");
                }
                finally
                {
                    Marshal.FreeHGlobal(guid);
                }
            }
            finally
            {
                if (store != IntPtr.Zero)
                {
                    NativeMethods.BcdCloseStore(store);
                }
            }
        }
        catch
        {
            return ToolResult.Fail("BCD current entry read failed.");
        }
    }








    [McpServerTool(Name = "Boot_Configuration_Enum", ReadOnly = true, Destructive = false)]
    [Description("Enumerates the active boot configuration store entries.")]
    public static ToolResult BcdeditEnum()
    {
        try
        {
            StringBuilder sb = new();
            int hResult = NativeMethods.BcdOpenStore(null, out IntPtr store);
            if (hResult < 0 || store == IntPtr.Zero)
            {
                return ToolResult.Fail("Failed to open the BCD store.");
            }

            try
            {
                hResult = NativeMethods.BcdEnumerateAndUnpackEntries(store, IntPtr.Zero, BcdLibraryDeviceType.BcdLibraryDeviceTypeBootDevice, 0, IntPtr.Zero, out int count, IntPtr.Zero);

                if (hResult < 0)
                {
                    return ToolResult.Fail("Failed to enumerate BCD entries.");
                }

                sb.AppendLine($"BcdEnumerateAndUnpackEntries returned {count} entries.");
                sb.AppendLine("Use bcdedit /enum for full human-readable details.");
                return ToolResult.Ok(sb.ToString());
            }
            finally
            {
                if (store != IntPtr.Zero)
                {
                    NativeMethods.BcdCloseStore(store);
                }
            }
        }
        catch
        {
            return ToolResult.Fail("BCD enumeration failed.");
        }
    }








    private static class NativeMethods
    {
        private const string BcdDll = "bcd.dll";








        [DllImport(BcdDll, CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int BcdCloseStore(IntPtr storeHandle);








        [DllImport(BcdDll, CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int BcdEnumerateAndUnpackEntries(IntPtr storeHandle, IntPtr template, BcdLibraryDeviceType deviceType, uint flags, IntPtr buffer, out int count, IntPtr returnedBufferLength);








        [DllImport(BcdDll, CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int BcdGetElementData(IntPtr storeHandle, ref Guid objectGuid, BcdLibraryElementType elementType, IntPtr buffer, ref int bufferSize, out int returnedLength);








        [DllImport(BcdDll, CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int BcdOpenStore(string? fileName, out IntPtr storeHandle);
    }





    private enum BcdLibraryDeviceType : uint
    {
        BcdLibraryDeviceTypeBootDevice = 0x00000001
    }





    private enum BcdLibraryElementType : uint
    {
        BcdLibraryObjectTypeCurrentBootEntry = 0x12000002
    }
}
