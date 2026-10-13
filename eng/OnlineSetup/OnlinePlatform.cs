using System;
using System.Runtime.InteropServices;

namespace Oto.OnlineSetup
{
    internal static class OnlinePlatform
    {
        internal static bool Accept(ushort architecture, Version version) { return architecture == 9 && version.Major >= 10 && version.Build >= 19041; }
        internal static bool Supported() { SystemInfo info; GetNativeSystemInfo(out info); return Environment.OSVersion.Platform == PlatformID.Win32NT && Accept(info.Architecture, Environment.OSVersion.Version); }
        [DllImport("kernel32.dll")] private static extern void GetNativeSystemInfo(out SystemInfo info);
        [StructLayout(LayoutKind.Sequential)] private struct SystemInfo
        {
            internal ushort Architecture, Reserved;
            internal uint PageSize;
            internal IntPtr Minimum, Maximum;
            internal UIntPtr Mask;
            internal uint Processors, Type, Allocation;
            internal ushort Level, Revision;
        }
    }
}
