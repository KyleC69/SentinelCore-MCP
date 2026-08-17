// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         SafeComObject.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using System.Runtime.InteropServices;
using System.Runtime.Versioning;




namespace SentinelCoreMCP.Tools.Interop;





/// <summary>
///     Disposable wrapper for a COM object obtained through Type.GetTypeFromCLSID / Activator.CreateInstance.
///     Ensures deterministic release without weakening the read-only safety contract.
/// </summary>
internal sealed class SafeComObject : IDisposable
{
    [SupportedOSPlatform("windows")]
    public SafeComObject(Guid clsid)
    {
        Type? type = Type.GetTypeFromCLSID(clsid);
        if (type is not null)
        {
            Instance = Activator.CreateInstance(type);
        }
    }








    public object? Instance { get; private set; }








    [SupportedOSPlatform("windows")]
    public void Dispose()
    {
        if (Instance is not null)
        {
            Marshal.ReleaseComObject(Instance);
            Instance = null;
        }
    }
}
