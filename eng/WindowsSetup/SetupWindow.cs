using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Oto.Setup
{
    internal sealed partial class SetupWindow : Form
    {
        private readonly Label osValue, runtimeValue, diskValue;
        private readonly Label statusTitle = MakeText("Memeriksa komputer…", 12, true);
        private readonly Label statusDetail = MakeText("Sebentar, installer sedang memeriksa kebutuhan OTO.", 10, false);
        private readonly Label consent = MakeText("", 9, false);
        private readonly ProgressBar progress = new ProgressBar { Dock = DockStyle.Top, Height = 8, Margin = new Padding(0, 14, 0, 14), Style = ProgressBarStyle.Marquee };
        private readonly Button primary = new Button { Text = "Memeriksa…", AutoSize = true, MinimumSize = new Size(190, 42), FlatStyle = FlatStyle.Flat, Enabled = false, BackColor = Color.FromArgb(20, 103, 211), ForeColor = Color.White, Cursor = Cursors.Hand };
        private readonly Button secondary = new Button { Text = "Tutup", AutoSize = true, MinimumSize = new Size(88, 42), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.FromArgb(32, 52, 78), Cursor = Cursors.Hand };
        private readonly CheckBox openApp = new CheckBox { Text = "Buka OTO setelah selesai", AutoSize = true, Visible = false, Margin = new Padding(0, 6, 0, 12) };
        private Panel content;
        private CancellationTokenSource cancellation;
        private bool busy, canCancel, completed, restartRequired;
        private readonly bool audit;

        internal SetupWindow(bool auditMode = false)
        {
            audit = auditMode;
            Text = "Pasang PERTAMINA OTO";
            // Layout is measured at the current monitor DPI, never scaled cumulatively.
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(940, 650);
            StartPosition = FormStartPosition.Manual; BackColor = Color.White; MaximizeBox = true;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Image logoImage;
            using (var resource = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("OTO.Brand.ico"))
            using (var brandIcon = new Icon(resource, new Size(256, 256))) logoImage = brandIcon.ToBitmap();
            BuildLayout(logoImage);
            osValue = AddRequirement("Windows");
            runtimeValue = AddRequirement(".NET 10 Runtime x64");
            diskValue = AddRequirement("Ruang penyimpanan");
            content.Controls.Add(consent);
            var license = new LinkLabel { Text = "Tentang lisensi .NET dari Microsoft", LinkColor = Color.FromArgb(20, 103, 211), UseMnemonic = false };
            license.LinkClicked += (_, __) => { try { Process.Start(new ProcessStartInfo("https://github.com/dotnet/runtime/blob/main/LICENSE.TXT") { UseShellExecute = true }); } catch (Exception) { } };
            content.Controls.Add(license); RegisterFont(license, 9, false);
            primary.FlatAppearance.BorderSize = 0; primary.Padding = new Padding(14, 4, 14, 4); primary.AccessibleName = "Lanjutkan pemasangan OTO";
            secondary.FlatAppearance.BorderColor = Color.FromArgb(210, 220, 233); secondary.Margin = new Padding(0, 3, 10, 3);
            primary.Click += async (_, __) => { if (!audit) await StartInstallAsync(); };
            secondary.Click += (_, __) => { if (busy) { if (canCancel) { secondary.Enabled = false; cancellation.Cancel(); statusDetail.Text = "Membatalkan persiapan. Tunggu sebentar…"; Reflow(); } } else Close(); };
            FormClosing += (_, e) => { if (busy) { e.Cancel = true; if (canCancel) cancellation.Cancel(); } };
            Shown += async (_, __) => { if (!audit) { FitToWorkingArea(true); await InspectAsync(); } };
            FormClosed += (_, __) => { cancellation?.Dispose(); logoImage.Dispose(); DisposeLayoutFonts(); };
            layoutReady = true; Reflow();
        }

        private static Label MakeText(string value, float size, bool bold, Color? color = null)
        { return new Label { Text = value, AutoSize = false, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), ForeColor = color ?? Color.FromArgb(31, 48, 69), Margin = Padding.Empty, UseMnemonic = false }; }
        private async Task InspectAsync()
        {
            try
            {
                var runtime = await Task.Run(() => RuntimeRequirement.Detect());
                var os = RuntimeRequirement.SupportedWindows();
                var disk = await Task.Run(() => RuntimeRequirement.HasDiskSpace(PayloadInfo.ExpandedBytes, runtime != null));
                Ready(os, runtime, disk);
            }
            catch (Exception) { Failure("Kebutuhan komputer belum dapat diperiksa. Pastikan folder sementara dan drive pengguna dapat diakses."); }
        }
        private void Ready(bool os, string runtime, bool disk)
        {
            osValue.Text = os ? "✓ Windows x64 kompatibel" : "Memerlukan Windows 10 versi 2004+ atau Windows 11, x64";
            runtimeValue.Text = runtime == null ? "Belum tersedia · akan diunduh dari Microsoft" : "✓ Sudah tersedia · versi " + runtime;
            diskValue.Text = disk ? "✓ Cukup untuk aplikasi dan kebutuhan pemasangan" : "Ruang kosong belum mencukupi";
            progress.Style = ProgressBarStyle.Continuous; progress.Value = 0; primary.Enabled = os && disk;
            primary.Text = runtime == null ? "Pasang .NET dan OTO" : "Pasang OTO";
            statusTitle.Text = os && disk ? "Komputer siap untuk pemasangan" : "Ada kebutuhan yang perlu dipenuhi";
            statusDetail.Text = runtime == null ? "Klik tombol di bawah. Installer akan mengunduh .NET, lalu melanjutkan pemasangan OTO." : "Semua kebutuhan tersedia. OTO dapat dipasang tanpa internet.";
            consent.Text = runtime == null ? "Pemasangan .NET memerlukan internet dan izin administrator dari Windows. Komputer tidak dimulai ulang otomatis." : "Paket OTO sudah disertakan. Dokumen, pengaturan, dan versi sebelumnya tetap disimpan.";
            Reflow();
        }
        private void ShowProgress(SetupProgress value)
        {
            if (!busy || IsDisposed) return;
            statusTitle.Text = value.Title; statusDetail.Text = value.Detail; canCancel = value.CanCancel;
            secondary.Enabled = canCancel; primary.Enabled = false;
            primary.Text = "Sedang memasang…"; secondary.Text = "Batalkan";
            if (value.Title == "Mengunduh .NET 10") runtimeValue.Text = "Sedang diunduh dari Microsoft";
            else if (value.Title == "Memasang .NET 10") runtimeValue.Text = "Sedang dipasang · tunggu pemasang Microsoft";
            else if (value.Title == "Memasang OTO") runtimeValue.Text = "✓ .NET 10 Runtime x64 tersedia";
            actionHint.Text = canCancel ? "Anda dapat membatalkan tahap persiapan ini." : "Pembatalan sementara tidak tersedia selama pemasang berjalan.";
            progress.Style = value.Percent < 0 ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
            if (value.Percent >= 0) progress.Value = Math.Min(100, Math.Max(0, value.Percent));
            Reflow();
        }
        private async Task StartInstallAsync()
        {
            if (completed)
            {
                if (openApp.Checked && !restartRequired)
                {
                    try { Process.Start(new ProcessStartInfo(SetupOperations.InstalledLauncher) { UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(SetupOperations.InstalledLauncher) }); }
                    catch (Exception) { MessageBox.Show(this, "Buka OTO melalui shortcut PERTAMINA OTO di desktop.", "Buka OTO", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                }
                Close(); return;
            }
            if (busy) return;
            cancellation?.Dispose(); cancellation = new CancellationTokenSource();
            busy = true; canCancel = true; primary.Enabled = false; secondary.Text = "Batalkan"; secondary.Enabled = true;
            ShowProgress(new SetupProgress("Menyiapkan OTO", "Memeriksa paket aplikasi sebelum pemasangan…", -1, true));
            var updates = new Progress<SetupProgress>(ShowProgress);
            try
            {
                SetupOutcome outcome;
                using (var operations = new SetupOperations()) outcome = await SetupFlow.RunAsync(operations, updates, cancellation.Token);
                busy = false; Complete(outcome);
            }
            catch (OperationCanceledException) { busy = false; Failure("Pemasangan dibatalkan. Versi OTO yang sudah terpasang tetap tersedia. Anda dapat mencoba kembali."); }
            catch (Exception e)
            {
                busy = false;
                var message = e is InvalidOperationException || e is InvalidDataException ? e.Message : "Pemasangan belum dapat dilanjutkan. Periksa koneksi, ruang penyimpanan, dan izin Windows; lalu coba kembali.";
                Failure(message);
            }
        }
        private void Complete(SetupOutcome outcome)
        {
            completed = true; restartRequired = outcome.RestartRequired;
            statusTitle.Text = outcome.RestartRequired ? "OTO terpasang · mulai ulang diperlukan" : "OTO berhasil dipasang";
            statusDetail.Text = outcome.RestartRequired ? "Simpan pekerjaan, lalu mulai ulang komputer sebelum membuka OTO." : "Buka melalui shortcut PERTAMINA OTO. Jika versi lama masih terbuka, simpan pekerjaan dan tutup sebelum membuka versi baru.";
            if (outcome.ShortcutWarning) statusDetail.Text += " Shortcut belum dapat dibuat; aplikasi tersedia di %LOCALAPPDATA%/Programs/PertaminaOTO.";
            progress.Style = ProgressBarStyle.Continuous; progress.Value = 100; primary.Text = "Selesai"; primary.Enabled = true; secondary.Visible = false;
            openApp.Visible = !outcome.RestartRequired; consent.Text = "Build " + PayloadInfo.Version; runtimeValue.Text = "✓ .NET 10 Runtime x64 tersedia";
            actionHint.Text = ""; Reflow();
        }
        private void Failure(string detail)
        {
            statusTitle.Text = "Pemasangan belum selesai"; statusDetail.Text = detail; progress.Style = ProgressBarStyle.Continuous; progress.Value = 0;
            primary.Text = "Coba lagi"; primary.Enabled = true; secondary.Text = "Tutup"; secondary.Enabled = true;
            runtimeValue.Text = "Akan diperiksa ulang saat mencoba lagi"; actionHint.Text = "Versi OTO yang sudah terpasang tetap disimpan."; Reflow();
        }

        internal void AuditState(string state)
        {
            busy = canCancel = completed = restartRequired = false;
            secondary.Visible = true; secondary.Text = "Tutup"; secondary.Enabled = true;
            openApp.Visible = false; actionHint.Text = "";
            cancellation?.Dispose(); cancellation = new CancellationTokenSource();
            Ready(true, state == "ready" || state == "app-installing" ? "10.0.11" : null, true);
            if (state == "downloading") { busy = true; ShowProgress(new SetupProgress("Mengunduh .NET 10", "18,5 MB diunduh dari Microsoft…", 52, true)); }
            else if (state == "runtime-installing") { busy = true; ShowProgress(new SetupProgress("Memasang .NET 10", "Setujui permintaan izin Windows untuk pemasang Microsoft. Tunggu sampai selesai.", -1, false)); }
            else if (state == "app-installing") { busy = true; ShowProgress(new SetupProgress("Memasang OTO", "Aplikasi dan shortcut sedang disiapkan. Dokumen serta versi sebelumnya tetap disimpan.", -1, false)); }
            else if (state == "blocked") Ready(false, null, false);
            else if (state == "failed") Failure("Unduhan belum lengkap. Periksa koneksi internet, lalu pilih Coba lagi. OTO yang sudah terpasang tetap tersedia.");
            else if (state == "complete") Complete(new SetupOutcome());
            else if (state == "restart") Complete(new SetupOutcome { RestartRequired = true });
            Reflow();
        }
        internal void AuditDone() { busy = false; }
    }
}
