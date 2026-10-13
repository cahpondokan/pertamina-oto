using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: AssemblyTitle("PERTAMINA OTO Online Setup")]
[assembly: AssemblyProduct("PERTAMINA OTO")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.8")]
namespace Oto.OnlineSetup
{
    internal static class Program
    {
        [STAThread] private static int Main(string[] args)
        {
            try
            {
                if (args.Length == 2 && args[0] == "--self-test") return OnlineTests.Run(args[1]);
                if (args.Length == 3 && args[0] == "--verify-local-download") return OnlineTests.VerifyLocalDownload(args[1], args[2]);
                Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
                if (args.Length == 2 && args[0] == "--audit-ui") return OnlineUiAudit.Run(args[1]);
                if (args.Length != 0) return 2;
                using (var window = new OnlineWindow()) Application.Run(window);
                return 0;
            }
            catch (Exception error)
            {
                if (args.Length == 2 && args[0] == "--audit-ui") { Directory.CreateDirectory(args[1]); File.WriteAllText(Path.Combine(args[1], "audit-error.txt"), error.ToString()); }
                return 1;
            }
        }
    }
    internal sealed class OnlineWindow : Form
    {
        private readonly Label status = new Label { AutoSize = true, MaximumSize = new Size(590, 0), UseMnemonic = false, Text = "Internet diperlukan untuk mengunduh installer lengkap." };
        private readonly ProgressBar progress = new ProgressBar { Dock = DockStyle.Top, Height = 24 };
        private readonly Button primary = new Button { AutoSize = true, Text = "Unduh dan lanjutkan" };
        private readonly Button cancel = new Button { AutoSize = true, Text = "Tutup" };
        private CancellationTokenSource cancellation;
        private bool busy, canCancel;
        internal OnlineWindow(bool audit = false)
        {
            Text = "PERTAMINA OTO · Installer online"; Font = new Font("Segoe UI", 10); AutoScaleMode = AutoScaleMode.Dpi;
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            StartPosition = FormStartPosition.CenterScreen;
            var area = Screen.PrimaryScreen.WorkingArea;
            Size = new Size(Math.Min(680, area.Width), Math.Min(520, area.Height));
            MinimumSize = new Size(Math.Min(360, area.Width), Math.Min(320, area.Height));
            var body = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(20) };
            var title = new Label { AutoSize = true, Text = "PERTAMINA OTO", Font = new Font("Segoe UI", 20, FontStyle.Bold) };
            var detail = new Label { AutoSize = true, MaximumSize = new Size(590, 0), UseMnemonic = false, Text = "Versi " + OnlineInfo.ProductVersion + " · Pengembangan\nUnduhan lengkap: " + (OnlineInfo.Size / 1048576d).ToString("N1") + " MB\nRuntime disertakan. Tidak memasang .NET 10 global.\n\nSesudah unduhan diverifikasi, wizard offline membuka pilihan pemasangan. Dokumen, pengaturan dan versi sebelumnya tetap disimpan.\n\nPaket pengembangan belum ditandatangani. Hash memeriksa integritas, bukan identitas penerbit." };
            body.Controls.Add(title); body.Controls.Add(detail);
            var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(16), FlowDirection = FlowDirection.TopDown, WrapContents = false };
            var actions = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
            actions.Controls.Add(cancel); actions.Controls.Add(primary); footer.Controls.Add(status); footer.Controls.Add(progress); footer.Controls.Add(actions); Controls.Add(body); Controls.Add(footer);
            Resize += (_, __) => { var width = Math.Max(180, ClientSize.Width - 48); detail.MaximumSize = status.MaximumSize = new Size(width, 0); progress.Width = width; actions.MaximumSize = new Size(width, 0); };
            primary.Click += async (_, __) => { if (!audit) await DownloadAsync(); };
            cancel.Click += (_, __) => { if (busy) { if (canCancel) cancellation.Cancel(); } else Close(); };
            FormClosing += (_, e) => { if (busy) { e.Cancel = true; if (canCancel) cancellation.Cancel(); } };
            FormClosed += (_, __) => cancellation?.Dispose();
            AcceptButton = primary; CancelButton = cancel;
            primary.AccessibleName = "Unduh installer lengkap dan lanjutkan"; cancel.AccessibleName = "Batalkan atau tutup";
        }
        private async Task DownloadAsync()
        {
            if (busy) return;
            if (!OnlinePlatform.Supported()) { State("Memerlukan Windows 10 versi 2004+ atau Windows 11 x64.", 0); return; }
            var folder = Path.Combine(Path.GetTempPath(), "OTO-OnlineSetup-" + Guid.NewGuid().ToString("N"));
            try
            {
                NoLinks(folder);
                var security = new DirectorySecurity(); security.SetAccessRuleProtection(true, false);
                security.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                Directory.CreateDirectory(folder, security);
                var drive = new DriveInfo(Path.GetPathRoot(folder));
                // Downloaded full installer + its embedded ZIP/extraction; application drive is checked by offline wizard.
                if (drive.AvailableFreeSpace < checked(OnlineInfo.Size * 2 + OnlineInfo.ExpandedBytes + 256L * 1024 * 1024)) throw new IOException("space");
                busy = canCancel = true; primary.Enabled = false; cancel.Text = "Batalkan";
                cancellation?.Dispose(); cancellation = new CancellationTokenSource();
                var reports = new Progress<Tuple<long, long>>(value => State("Mengunduh " + value.Item1.ToString("N0") + " / " + value.Item2.ToString("N0") + " byte", (int)(value.Item1 * 100 / value.Item2)));
                var path = Path.Combine(folder, "OfflineSetup.exe");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (var handler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false, CheckCertificateRevocationList = true })
                using (var client = new HttpClient(handler) { Timeout = System.Threading.Timeout.InfiniteTimeSpan })
                    await OnlineDownload.FetchAsync(client, new Uri(OnlineInfo.Url), path, OnlineInfo.Size, OnlineInfo.Sha256, (done, total) => ((IProgress<Tuple<long, long>>)reports).Report(Tuple.Create(done, total)), cancellation.Token);
                State("Memverifikasi installer…", 100);
                OnlineDownload.Verify(path, OnlineInfo.Size, OnlineInfo.Sha256); cancellation.Token.ThrowIfCancellationRequested();
                using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // Keep the hash-verified bytes locked against writes/replacement through child exit.
                    OnlineDownload.Verify(path, OnlineInfo.Size, OnlineInfo.Sha256);
                    canCancel = false; cancel.Enabled = false; State("Wizard pemasangan terbuka. Lanjutkan atau batalkan di wizard tersebut.", 100);
                    using (var child = Process.Start(new ProcessStartInfo(path) { UseShellExecute = false, WorkingDirectory = folder }))
                    { if (child == null) throw new IOException("start"); await Task.Run(() => child.WaitForExit()); if (child.ExitCode != 0) throw new IOException("installer"); }
                }
                State("Wizard selesai. Gunakan shortcut OTO setelah pemasangan berhasil.", 100);
            }
            catch (OperationCanceledException) { State("Unduhan dibatalkan. Aplikasi yang terpasang tetap tersedia.", 0); }
            catch (Exception) { State("Belum berhasil. Periksa internet, ruang kosong, dan paket. Aplikasi yang terpasang tetap tersedia sebelum wizard mengaktifkan versi baru.", 0); }
            finally { busy = canCancel = false; primary.Enabled = cancel.Enabled = true; cancel.Text = "Tutup"; }
            // Retain the unique staging folder; never recursively delete user directories or partially installed data.
        }
        private static void NoLinks(string path)
        { for (var part = Path.GetFullPath(path); part != null; part = Path.GetDirectoryName(part)) if ((File.Exists(part) || Directory.Exists(part)) && (File.GetAttributes(part) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("link"); }
        internal void State(string text, int percent) { status.Text = text; progress.Value = percent; }
    }
}
