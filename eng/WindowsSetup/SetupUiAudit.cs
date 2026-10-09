using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Oto.Setup
{
    internal static class SetupUiAudit
    {
        internal static int Run(string output)
        {
            Directory.CreateDirectory(output);
            var checks = new List<string>(); var cases = 0; var workAreas = 0;
            var states = new[] { "ready", "runtime-needed", "downloading", "runtime-installing", "app-installing", "blocked", "failed", "complete", "restart" };
            // Equivalent logical viewports: physical work areas divided by scale.
            // This exercises reflow, not an OS-wide DPI change or physical monitor UAT.
            var screens = new[] { new Size(1024, 600), new Size(1024, 768), new Size(1280, 720), new Size(1366, 768), new Size(1440, 900), new Size(1536, 864), new Size(1600, 900), new Size(1920, 1080), new Size(2560, 1440), new Size(3440, 1440), new Size(3840, 2160), new Size(1080, 1920) };
            var scales = new[] { 1f, 1.25f, 1.5f, 1.75f, 2f, 2.5f, 3f };
            foreach (var screen in screens)
            foreach (var scale in scales)
            {
                var logical = new Size((int)(screen.Width / scale), (int)((screen.Height - 48 * scale) / scale));
                // Minimum supported usable logical desktop, excluding taskbar.
                if (logical.Width < 480 || logical.Height < 300) continue;
                var bounds = SetupWindow.BoundedWindow(new Rectangle(Point.Empty, logical), new Size(940, 650), 8);
                if (!new Rectangle(Point.Empty, logical).Contains(bounds)) throw new Exception("Window exceeds work area");
                checks.Add("PASS working area " + screen + " scale " + scale);
                workAreas++;
                using (var form = new SetupWindow(true))
                {
                    form.ShowInTaskbar = false; form.Opacity = 0;
                    form.AuditConfigure(new Size(bounds.Width - 16, bounds.Height - 40), 96, 1);
                    form.Show();
                    foreach (var state in states)
                    {
                        form.AuditState(state); Application.DoEvents(); form.AuditLayout(checks, screen + " / " + scale + " / " + state);
                        cases++;
                        if ((screen.Width == 1366 && (scale == 1 || scale == 1.5f)) || (screen.Width == 3840 && scale == 3) || (screen.Width == 1280 && scale == 2 && (state == "complete" || state == "runtime-installing")))
                            Capture(form, Path.Combine(output, screen.Width + "-" + (int)(scale * 100) + "-" + state + ".png"));
                    }
                    form.AuditDone(); form.Close();
                }
            }
            // Pixel-level scaling and larger text at fixed physical client bounds.
            foreach (var dpi in new[] { 96, 120, 144, 168, 192 })
            foreach (var textScale in new[] { 1f, 1.25f, 1.5f })
            using (var form = new SetupWindow(true))
            {
                form.ShowInTaskbar = false; form.Opacity = 0;
                form.AuditConfigure(new Size(1280, 900), dpi, textScale); form.Show();
                foreach (var state in new[] { "runtime-needed", "downloading", "runtime-installing", "app-installing", "complete", "failed" })
                {
                    form.AuditState(state); Application.DoEvents(); form.AuditLayout(checks, "pixel dpi " + dpi + " text " + textScale + " " + state);
                    cases++;
                }
                if (dpi == 144 && textScale == 1.5f) Capture(form, Path.Combine(output, "large-text-150.png"));
                // Repeated same-process changes must not cumulatively shrink or grow text.
                form.AuditConfigure(new Size(940, 650), 96, 1); form.AuditState("runtime-needed");
                Application.DoEvents(); form.AuditLayout(checks, "return to 96 dpi");
                cases++;
                form.AuditDone(); form.Close();
            }
            checks.Add("LIMIT: simulated work areas/scales/text; no physical monitor movement or OS DPI setting change, no UAC or actual installation");
            File.WriteAllText(Path.Combine(output, "summary.json"), "{\"workAreaScalePairs\":" + workAreas + ",\"nativeStateCases\":" + cases + ",\"assertions\":" + (checks.Count - 1) + ",\"failures\":0,\"physicalDpiUat\":false,\"actualInstallation\":false}");
            File.WriteAllLines(Path.Combine(output, "audit.txt"), checks); return 0;
        }
        private static void Capture(Form form, string path)
        {
            var until = DateTime.UtcNow.AddMilliseconds(120);
            while (DateTime.UtcNow < until) { Application.DoEvents(); Thread.Sleep(10); }
            using (var bitmap = new Bitmap(form.Width, form.Height))
            { form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size)); bitmap.Save(path, ImageFormat.Png); }
        }
    }
    internal sealed partial class SetupWindow
    {
        internal void AuditConfigure(Size client, int dpi, float textScale)
        { auditDpi = dpi; auditTextScale = textScale; MinimumSize = Size.Empty; ClientSize = client; Reflow(); }
        internal void AuditLayout(List<string> checks, string context)
        {
            Action<bool, string> check = (ok, name) => { if (!ok) throw new InvalidOperationException(context + ": " + name); checks.Add(("PASS " + context + ": " + name).Replace("\r", " ").Replace("\n", " / ")); };
            check(header.Top == 0 && footer.Bottom == ClientSize.Height && scroller.Height >= Px(40) && scroller.Bottom <= footer.Top, "pinned header/footer and usable detail viewport");
            check(header.Bottom <= scroller.Top && footer.Top > header.Bottom, "no region overlap");
            foreach (var spec in fontSpecs)
            {
                var control = spec.Key; if (!control.Visible) continue;
                check(control.Left >= 0 && control.Right <= control.Parent.ClientSize.Width && control.Top >= 0 && control.Bottom <= control.Parent.ClientSize.Height, "bounds " + control.GetType().Name + " " + control.Text);
                if (control is Label) check(control.Height >= TextHeight(control, control.Width), "wrapped text fits " + control.Text);
                if (control is Button) check(control.Width >= TextRenderer.MeasureText(control.Text, control.Font).Width + Px(20), "button label fits " + control.Text);
            }
            check(!scroller.HorizontalScroll.Visible, "no horizontal scrolling");
            var oldHeader = header.Bounds; var oldFooter = footer.Bounds;
            scroller.AutoScrollPosition = new Point(0, content.Height); Application.DoEvents();
            check(oldHeader == header.Bounds && oldFooter == footer.Bounds, "detail scrolling cannot hide status/actions");
            check(content.Bottom <= scroller.ClientSize.Height + 2, "last detail reachable by vertical scroll");
            scroller.AutoScrollPosition = Point.Empty;
            if (statusTitle.Text == "Ada kebutuhan yang perlu dipenuhi") check(!primary.Enabled, "unsupported requirements cannot start installation");
            if (completed) check(primary.Enabled && primary.Text == "Selesai" && !secondary.Visible && openApp.Visible == !restartRequired, "completion and restart controls");
            if (busy)
            {
                check(!primary.Enabled && primary.Text == "Sedang memasang…" && secondary.Enabled == canCancel, "busy action semantics");
                if (statusTitle.Text == "Memasang .NET 10")
                {
                    check(runtimeValue.Text.StartsWith("Sedang dipasang") && progress.Style == ProgressBarStyle.Marquee && !canCancel && actionHint.Text.Contains("tidak tersedia"), "runtime status and noncancelable explanation");
                    var args = new FormClosingEventArgs(CloseReason.UserClosing, false); OnFormClosing(args);
                    check(args.Cancel, "close cannot terminate active vendor installation");
                }
                if (statusTitle.Text == "Memasang OTO") check(runtimeValue.Text.Contains("tersedia"), "runtime confirmed before OTO");
            }
        }
    }
}
