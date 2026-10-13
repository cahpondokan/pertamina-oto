using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Oto.Setup
{
    internal static class RuntimeRequirement
    {
        internal const string VersionToDownload = "10.0.12";
        internal const string DownloadUrl = "https://builds.dotnet.microsoft.com/dotnet/Runtime/10.0.12/dotnet-runtime-10.0.12-win-x64.exe";
        internal const string Sha512 = "b937ae539c4ab21f885b033178dadea98018baad777252c76251e760d81c18463b2f802e86b6a67ea066690e9ede2d66a6bc3c3e614e391505e98c4c41121400";
        internal const long MaximumDownload = 128L * 1024 * 1024;

        internal static bool AcceptsVersion(string value)
        {
            Version parsed;
            return !string.IsNullOrEmpty(value) && System.Version.TryParse(value, out parsed)
                && parsed.Major == 10 && parsed.Minor == 0 && parsed.Build >= 0 && parsed.Revision < 0;
        }

        internal static string FindInRoot(string root)
        {
            if (string.IsNullOrEmpty(root) || !Path.IsPathRooted(root)) return null;
            try
            {
                if (!IsX64Image(Path.Combine(root, "dotnet.exe"))) return null;
                var shared = Path.Combine(root, "shared", "Microsoft.NETCore.App");
                if (!Directory.Exists(shared)) return null;
                var fxr = Path.Combine(root, "host", "fxr");
                if (!Directory.Exists(fxr) || !Directory.GetDirectories(fxr).Any(p => { Version version; return System.Version.TryParse(Path.GetFileName(p), out version) && version.Major >= 10 && IsX64Image(Path.Combine(p, "hostfxr.dll")); })) return null;
                return Directory.GetDirectories(shared).Select(Path.GetFileName)
                    .Where(AcceptsVersion).OrderByDescending(x => System.Version.Parse(x))
                    .FirstOrDefault(x => IsX64Image(Path.Combine(shared, x, "coreclr.dll"))
                        && File.Exists(Path.Combine(shared, x, "System.Private.CoreLib.dll")));
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException) { return null; }
        }

        internal static string Detect()
        {
            var roots = new List<string>();
            using (var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var key = machine.OpenSubKey(@"SOFTWARE\dotnet\Setup\InstalledVersions\x64"))
            { if (key != null) roots.Add(key.GetValue("InstallLocation") as string); }
            roots.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet"));
            return roots.Select(FindInRoot).Where(x => x != null).OrderByDescending(x => System.Version.Parse(x)).FirstOrDefault();
        }

        internal static bool IsX64Image(string path)
        {
            if (!File.Exists(path)) return false;
            using (var stream = File.OpenRead(path))
            using (var reader = new BinaryReader(stream))
            {
                if (stream.Length < 64 || reader.ReadUInt16() != 0x5A4D) return false;
                stream.Position = 60; var offset = reader.ReadInt32();
                if (offset < 64 || offset > stream.Length - 6) return false;
                stream.Position = offset;
                return reader.ReadUInt32() == 0x4550 && reader.ReadUInt16() == 0x8664;
            }
        }

        internal static bool SupportedWindows()
        {
            SystemInfo info; GetNativeSystemInfo(out info);
            return info.Architecture == 9 && Environment.OSVersion.Platform == PlatformID.Win32NT
                && Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 19041;
        }
        [DllImport("kernel32.dll")] private static extern void GetNativeSystemInfo(out SystemInfo info);
        [StructLayout(LayoutKind.Sequential)] private struct SystemInfo
        {
            internal ushort Architecture, Reserved;
            internal uint PageSize;
            internal IntPtr MinimumAddress, MaximumAddress, ActiveMask;
            internal uint Processors, ProcessorType, AllocationGranularity;
            internal ushort Level, Revision;
        }

        internal static bool HasDiskSpace(long expandedPayload, bool runtimePresent)
        {
            var needed = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            Action<string, long> add = (path, size) =>
            { var drive = Path.GetPathRoot(Path.GetFullPath(path)); if (!needed.ContainsKey(drive)) needed[drive] = 128L * 1024 * 1024; needed[drive] += size; };
            add(Path.GetTempPath(), expandedPayload + (runtimePresent ? 0 : MaximumDownload));
            add(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), expandedPayload);
            if (!runtimePresent) add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), 512L * 1024 * 1024);
            return needed.All(pair => new DriveInfo(pair.Key).AvailableFreeSpace >= pair.Value);
        }
    }
}
