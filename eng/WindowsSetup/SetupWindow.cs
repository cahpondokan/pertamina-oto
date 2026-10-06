using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Oto.Setup
{
    internal sealed class SetupWindow : Form
    {
        private readonly Label osValue, runtimeValue, diskValue;
        private readonly Label statusTitle = MakeText("Memeriksa komputer…", 12, true);
        private readonly Label statusDetail = MakeText("Sebentar, installer sedang memeriksa kebutuhan OTO.", 10, false);
        private readonly Label consent = MakeText("", 9, false);
        private readonly ProgressBar progress = new ProgressBar { Dock = DockStyle.Top, Height = 8, Margin = new Padding(0, 14, 0, 14), Style = ProgressBarStyle.Marquee };
        private readonly Button primary = new Button { Text = "Memeriksa…", AutoSize = true, MinimumSize = new Size(190, 42), FlatStyle = FlatStyle.Flat, Enabled = false, BackColor = Color.FromArgb(20, 103, 211), ForeColor = Color.White, Cursor = Cursors.Hand };
        private readonly Button secondary = new Button { Text = "Tutup", AutoSize = true, MinimumSize = new Size(88, 42), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.FromArgb(32, 52, 78), Cursor = Cursors.Hand };
        private readonly CheckBox openApp = new CheckBox { Text = "Buka OTO setelah selesai", AutoSize = true, Visible = false, Margin = new Padding(0, 6, 0, 12) };
        private readonly TableLayoutPanel content;
        private CancellationTokenSource cancellation;
        private bool busy, canCancel, completed, restartRequired;
        private readonly bool audit;

        internal SetupWindow(bool auditMode = false)
        {
            audit = auditMode;
            Text = "Pasang PERTAMINA OTO"; Font = new Font("Segoe UI", 10);
            AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96, 96);
            ClientSize = new Size(940, 650); MinimumSize = new Size(780, 620);
            StartPosition = FormStartPosition.CenterScreen; BackColor = Color.White; MaximizeBox = false;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            var columns = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty, Padding = Padding.Empty };
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 218)); columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var brand = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 29, 59), Margin = Padding.Empty };
            Image logoImage;
            using (var resource = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("OTO.Brand.ico"))
            using (var brandIcon = new Icon(resource, new Size(256, 256))) logoImage = brandIcon.ToBitmap();
            var logo = new PictureBox { Location = new Point(30, 40), Size = new Size(82, 82), SizeMode = PictureBoxSizeMode.Zoom, Image = logoImage };
            brand.Controls.Add(logo);
            var name = MakeText("PERTAMINA\nOTO", 17, true); name.Dock = DockStyle.None; name.ForeColor = Color.White; name.AutoSize = false; name.SetBounds(30, 142, 172, 88); brand.Controls.Add(name);
            var tagline = MakeText("Dokumen beres.\nPekerjaan lanjut.", 11, false); tagline.Dock = DockStyle.None; tagline.ForeColor = Color.FromArgb(182, 205, 234); tagline.AutoSize = false; tagline.SetBounds(30, 252, 172, 76); brand.Controls.Add(tagline);
            var bottom = MakeText("WINDOWS x64\nVersi pengembangan", 9, false); bottom.ForeColor = Color.FromArgb(182, 205, 234); bottom.Dock = DockStyle.Bottom; bottom.Height = 90; bottom.AutoSize = false; bottom.Padding = new Padding(30, 12, 16, 0); brand.Controls.Add(bottom);
            columns.Controls.Add(brand, 0, 0);
            var right = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(32, 30, 32, 22), Margin = Padding.Empty };
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var scroller = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Margin = Padding.Empty };
            content = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Padding = new Padding(0, 0, 12, 12), Margin = Padding.Empty };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Add(MakeText("SELAMAT DATANG", 9, true, Color.FromArgb(20, 103, 211)), 0, 10);
            Add(MakeText("Pasang OTO", 26, true), 0, 10);
            Add(MakeText("Installer akan memeriksa komputer, lalu menyiapkan OTO untuk akun Windows Anda.", 11, false), 0, 22);
            Add(Requirement("Windows", out osValue), 0, 10);
            Add(Requirement(".NET 10 Runtime x64", out runtimeValue), 0, 10);
            Add(Requirement("Ruang penyimpanan", out diskValue), 0, 20);
            Add(statusTitle, 0, 6); Add(statusDetail, 0, 0); Add(progress, 8, 10); Add(consent, 0, 12);
            var license = new LinkLabel { Text = "Tentang lisensi .NET dari Microsoft", AutoSize = true, LinkColor = Color.FromArgb(20, 103, 211), Font = new Font("Segoe UI", 9), Margin = new Padding(0, 0, 0, 16) };
            license.LinkClicked += (_, __) => { try { Process.Start(new ProcessStartInfo("https://github.com/dotnet/runtime/blob/main/LICENSE.TXT") { UseShellExecute = true }); } catch (Exception) { } };
            Add(license, 0, 6); Add(openApp, 0, 0);
            scroller.Controls.Add(content); right.Controls.Add(scroller, 0, 0);
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 14, 0, 0), Margin = Padding.Empty };
            primary.FlatAppearance.BorderSize = 0; primary.Padding = new Padding(14, 4, 14, 4); primary.AccessibleName = "Lanjutkan pemasangan OTO";
            secondary.FlatAppearance.BorderColor = Color.FromArgb(210, 220, 233); secondary.Margin = new Padding(0, 3, 10, 3);
            actions.Controls.Add(primary); actions.Controls.Add(secondary); right.Controls.Add(actions, 0, 1);
            columns.Controls.Add(right, 1, 0); Controls.Add(columns);
            primary.Click += async (_, __) => { if (!audit) await StartInstallAsync(); };
            secondary.Click += (_, __) => { if (busy) { if (canCancel) { secondary.Enabled = false; cancellation.Cancel(); statusDetail.Text = "Membatalkan persiapan. Tunggu sebentar…"; } } else Close(); };
            FormClosing += (_, e) => { if (busy) { e.Cancel = true; if (canCancel) cancellation.Cancel(); } };
            Shown += async (_, __) => { if (!audit) await InspectAsync(); };
            FormClosed += (_, __) => { cancellation?.Dispose(); logo.Image.Dispose(); };
        }

        private static Label MakeText(string value, float size, bool bold, Color? color = null)
        { return new Label { Text = value, AutoSize = true, Dock = DockStyle.Fill, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), ForeColor = color ?? Color.FromArgb(31, 48, 69), Margin = Padding.Empty, UseMnemonic = false }; }
        private void Add(Control control, int top, int bottom)
        { control.Margin = new Padding(0, top, 0, bottom); content.Controls.Add(control, 0, content.RowCount++); content.RowStyles.Add(new RowStyle(SizeType.AutoSize)); }
        private static Control Requirement(string name, out Label value)
        {
            var panel = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 1, RowCount = 2, Padding = new Padding(14, 10, 14, 10), BackColor = Color.FromArgb(244, 247, 251), Margin = Padding.Empty };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.Controls.Add(MakeText(name, 10, true), 0, 0);
            value = MakeText("Memeriksa…", 9, false, Color.FromArgb(81, 104, 132)); value.Margin = new Padding(0, 4, 0, 0); panel.Controls.Add(value, 0, 1); return panel;
        }
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
        }
        private void ShowProgress(SetupProgress value)
        {
            if (!busy || IsDisposed) return;
            statusTitle.Text = value.Title; statusDetail.Text = value.Detail; canCancel = value.CanCancel;
            secondary.Enabled = canCancel; primary.Enabled = false;
            progress.Style = value.Percent < 0 ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
            if (value.Percent >= 0) progress.Value = Math.Min(100, Math.Max(0, value.Percent));
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
            openApp.Visible = !outcome.RestartRequired; consent.Text = "Versi " + PayloadInfo.Version; runtimeValue.Text = "✓ .NET 10 Runtime x64 tersedia";
        }
        private void Failure(string detail)
        {
            statusTitle.Text = "Pemasangan belum selesai"; statusDetail.Text = detail; progress.Style = ProgressBarStyle.Continuous; progress.Value = 0;
            primary.Text = "Coba lagi"; primary.Enabled = true; secondary.Text = "Tutup"; secondary.Enabled = true;
        }

        internal void AuditState(string state)
        {
            Ready(true, state == "runtime-needed" ? null : "10.0.11", true);
            if (state == "downloading") { busy = true; ShowProgress(new SetupProgress("Mengunduh .NET 10", "18,5 MB diunduh dari Microsoft…", 52, true)); }
            else if (state == "failed") Failure("Unduhan belum lengkap. Periksa koneksi internet, lalu pilih Coba lagi. OTO yang sudah terpasang tetap tersedia.");
            else if (state == "complete") Complete(new SetupOutcome());
            else if (state == "restart") Complete(new SetupOutcome { RestartRequired = true });
        }
        internal void AuditDone() { busy = false; }
        internal bool ActionsVisible()
        { return primary.Visible && primary.Enabled == !busy && primary.Right <= primary.Parent.ClientSize.Width && primary.Bottom <= primary.Parent.ClientSize.Height; }
    }
}
