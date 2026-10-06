using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Oto.Setup
{
    internal static class TrustedRuntime
    {
        internal static async Task DownloadAsync(HttpClient client, string destination, IProgress<SetupProgress> progress, CancellationToken cancellation)
        {
            using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
            {
                deadline.CancelAfter(TimeSpan.FromMinutes(10));
                using (var response = await client.GetAsync(RuntimeRequirement.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false))
                {
                    if (response.StatusCode != HttpStatusCode.OK) throw new IOException("Unduhan runtime belum tersedia. Periksa koneksi, lalu coba lagi.");
                    var length = response.Content.Headers.ContentLength;
                    if (length.HasValue && (length.Value <= 0 || length.Value > RuntimeRequirement.MaximumDownload)) throw new InvalidDataException("Ukuran unduhan runtime tidak sesuai.");
                    using (deadline.Token.Register(() => response.Dispose()))
                    using (var input = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[81920]; long total = 0; int count;
                        while ((count = await input.ReadAsync(buffer, 0, buffer.Length, deadline.Token).ConfigureAwait(false)) > 0)
                        {
                            total += count;
                            if (total > RuntimeRequirement.MaximumDownload) throw new InvalidDataException("Unduhan runtime melampaui batas.");
                            await output.WriteAsync(buffer, 0, count, deadline.Token).ConfigureAwait(false);
                            progress.Report(new SetupProgress("Mengunduh .NET 10", string.Format("{0:0.0} MB diunduh dari Microsoft…", total / 1048576d), length.HasValue ? (int)(total * 100 / length.Value) : -1, true));
                        }
                        if (length.HasValue && total != length.Value) throw new IOException("Unduhan belum lengkap. Coba kembali.");
                    }
                }
            }
        }

        internal static void VerifyHash(Stream stream, string expected)
        {
            using (var hash = SHA512.Create())
            {
                var actual = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "");
                if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Pemeriksaan keamanan unduhan gagal. Runtime tidak dijalankan.");
            }
        }

        internal static void VerifyMicrosoftSignature(string file)
        {
            var fileInfo = new TrustFile { Size = (uint)Marshal.SizeOf(typeof(TrustFile)), FilePath = file };
            var pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TrustFile)));
            Marshal.StructureToPtr(fileInfo, pointer, false);
            var data = new TrustData { Size = (uint)Marshal.SizeOf(typeof(TrustData)), UiChoice = 2, RevocationChecks = 1, UnionChoice = 1, FileInfo = pointer, StateAction = 1, ProviderFlags = 0x80 };
            var action = new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");
            try
            {
                // Full Windows Authenticode trust and chain revocation checks; no certificate bypass.
                if (WinVerifyTrust(new IntPtr(-1), ref action, ref data) != 0) throw new InvalidDataException("Tanda tangan runtime belum dapat diverifikasi. Periksa koneksi dan tanggal komputer, lalu coba lagi.");
                using (var certificate = new X509Certificate2(X509Certificate.CreateFromSignedFile(file)))
                {
                    var names = certificate.SubjectName.Decode(X500DistinguishedNameFlags.UseNewLines).Split('\r', '\n');
                    if (!names.Any(line => string.Equals(line.Trim(), "O=Microsoft Corporation", StringComparison.OrdinalIgnoreCase)))
                        throw new InvalidDataException("Penerbit runtime tidak sesuai. File tidak dijalankan.");
                }
            }
            finally
            {
                data.StateAction = 2; WinVerifyTrust(new IntPtr(-1), ref action, ref data);
                Marshal.DestroyStructure(pointer, typeof(TrustFile)); Marshal.FreeHGlobal(pointer);
            }
        }

        [DllImport("wintrust.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int WinVerifyTrust(IntPtr window, ref Guid action, ref TrustData data);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct TrustFile
        { internal uint Size; [MarshalAs(UnmanagedType.LPWStr)] internal string FilePath; internal IntPtr File, KnownSubject; }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct TrustData
        {
            internal uint Size; internal IntPtr Policy, Sip; internal uint UiChoice, RevocationChecks, UnionChoice;
            internal IntPtr FileInfo; internal uint StateAction; internal IntPtr StateData, UrlReference;
            internal uint ProviderFlags, UiContext; internal IntPtr SignatureSettings;
        }
    }
}
