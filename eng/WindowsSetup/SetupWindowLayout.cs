using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Oto.Setup
{
    internal sealed partial class SetupWindow
    {
        internal const string InstallerRevision = "1.0.1";
        private Panel brand, header, footer, scroller;
        private PictureBox brandLogo;
        private Label brandName, tagline, brandVersion, identity, productVersion;
        private readonly Label actionHint = MakeText("", 9, false);
        private readonly List<Panel> requirementCards = new List<Panel>();
        private readonly Dictionary<Control, Tuple<float, bool>> fontSpecs = new Dictionary<Control, Tuple<float, bool>>();
        private readonly List<Font> layoutFonts = new List<Font>();
        private bool layoutReady, arranging;
        private float fontDpi, fontTextScale, auditTextScale = 1;
        private int auditDpi;
        [DllImport("user32.dll")] private static extern uint GetDpiForWindow(IntPtr hwnd);
        private float LayoutDpi { get { return auditDpi > 0 ? (float)auditDpi : IsHandleCreated ? (float)Math.Max(96, GetDpiForWindow(Handle)) : 96f; } }
        private int Px(float value) { return (int)Math.Ceiling(value * LayoutDpi / 96f); }
        private void RegisterFont(Control control, float points, bool bold) { fontSpecs.Add(control, Tuple.Create(points, bold)); }
        private void BuildLayout(Image image)
        {
            DoubleBuffered = true;
            brand = new Panel { BackColor = Color.FromArgb(8, 29, 59) };
            header = new Panel { BackColor = Color.White };
            footer = new Panel { BackColor = Color.FromArgb(246, 248, 252) };
            scroller = new Panel { AutoScroll = true, TabStop = true, AccessibleName = "Rincian kebutuhan pemasangan; gulir untuk membaca" };
            content = new Panel(); scroller.Controls.Add(content);
            brandLogo = new PictureBox { Image = image, SizeMode = PictureBoxSizeMode.Zoom };
            brandName = MakeText("PERTAMINA\nOTO", 18, true, Color.White);
            tagline = MakeText("Dokumen beres.\nPekerjaan lanjut.", 11, false, Color.FromArgb(202, 220, 240));
            brandVersion = MakeText("WINDOWS x64\nVersi pengembangan", 9, false, Color.FromArgb(202, 220, 240));
            brand.Controls.AddRange(new Control[] { brandLogo, brandName, tagline, brandVersion });
            identity = MakeText("PERTAMINA OTO · PEMASANGAN", 12, true, Color.FromArgb(20, 103, 211));
            productVersion = MakeText("Aplikasi " + PayloadInfo.ProductVersion + " · Pengembangan · Installer " + InstallerRevision, 9, false);
            header.Controls.AddRange(new Control[] { identity, productVersion, statusTitle, statusDetail, progress });
            footer.Controls.AddRange(new Control[] { actionHint, openApp, primary, secondary });
            Controls.AddRange(new Control[] { brand, header, scroller, footer });
            progress.Dock = DockStyle.None;
            primary.AutoSize = secondary.AutoSize = openApp.AutoSize = false;
            primary.MinimumSize = secondary.MinimumSize = Size.Empty;
            primary.TabIndex = 1; secondary.TabIndex = 2; openApp.TabIndex = 0;
            header.TabIndex = 0; scroller.TabIndex = 1; footer.TabIndex = 2;
            RegisterFont(brandName, 18, true); RegisterFont(tagline, 11, false); RegisterFont(brandVersion, 9, false);
            RegisterFont(identity, 12, true); RegisterFont(productVersion, 9, false);
            RegisterFont(statusTitle, 12, true); RegisterFont(statusDetail, 10, false);
            RegisterFont(consent, 9, false); RegisterFont(actionHint, 9, false);
            RegisterFont(openApp, 10, false); RegisterFont(primary, 10, false); RegisterFont(secondary, 10, false);
            primary.EnabledChanged += (_, __) =>
            {
                primary.BackColor = primary.Enabled ? (SystemInformation.HighContrast ? SystemColors.Highlight : Color.FromArgb(20, 103, 211)) : SystemColors.Control;
                primary.ForeColor = primary.Enabled ? Color.White : SystemColors.GrayText;
            };
        }
        private Label AddRequirement(string title)
        {
            var card = new Panel { BackColor = Color.FromArgb(244, 247, 251) };
            var name = MakeText(title, 10, true);
            var value = MakeText("Memeriksa…", 10, false);
            RegisterFont(name, 10, true); RegisterFont(value, 10, false);
            card.Controls.AddRange(new Control[] { name, value });
            requirementCards.Add(card); content.Controls.Add(card); return value;
        }
        private void UpdateFonts()
        {
            if (fontDpi == LayoutDpi && fontTextScale == auditTextScale) return;
            var old = layoutFonts.ToArray(); layoutFonts.Clear();
            foreach (var spec in fontSpecs)
            {
                var font = new Font("Segoe UI", spec.Value.Item1 * LayoutDpi / 72f * auditTextScale,
                    spec.Value.Item2 ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);
                layoutFonts.Add(font); spec.Key.Font = font;
            }
            foreach (var font in old) font.Dispose();
            fontDpi = LayoutDpi; fontTextScale = auditTextScale;
        }
        private void DisposeLayoutFonts() { foreach (var font in layoutFonts) font.Dispose(); layoutFonts.Clear(); }
        private int TextHeight(Control control, int width)
        {
            return TextRenderer.MeasureText(control.Text, control.Font, new Size(Math.Max(1, width), int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPrefix).Height + Px(3);
        }
        private int PlaceText(Control control, int x, int y, int width)
        {
            var height = TextHeight(control, width);
            control.SetBounds(x, y, Math.Max(1, width), height); return y + height;
        }
        protected override void OnResize(EventArgs e) { base.OnResize(e); Reflow(); }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // Explicit measurement also covers monitor changes with no app.config sidecar.
            if (m.Msg == 0x02E0 && layoutReady && !audit) { FitToWorkingArea(false); Reflow(); }
        }
        internal static Rectangle BoundedWindow(Rectangle work, Size desired, int margin)
        {
            var width = Math.Min(desired.Width, Math.Max(1, work.Width - margin * 2));
            var height = Math.Min(desired.Height, Math.Max(1, work.Height - margin * 2));
            return new Rectangle(work.X + (work.Width - width) / 2, work.Y + (work.Height - height) / 2, width, height);
        }
        private void FitToWorkingArea(bool initial)
        {
            var work = Screen.FromControl(this).WorkingArea;
            MinimumSize = new Size(Math.Min(Px(420), work.Width), Math.Min(Px(320), work.Height));
            var desired = initial ? new Size(Px(940), Px(650) + Height - ClientSize.Height) : Size;
            if (initial || Width > work.Width || Height > work.Height) Bounds = BoundedWindow(work, desired, Px(8));
            else if (!work.IntersectsWith(Bounds)) Location = work.Location;
            Reflow();
        }
        private void Reflow()
        {
            if (!layoutReady || arranging || ClientSize.Width < 1 || ClientSize.Height < 1) return;
            arranging = true;
            try
            {
                UpdateFonts();
                primary.Padding = new Padding(Px(12), Px(4), Px(12), Px(4));
                var pad = Px(ClientSize.Width < Px(600) ? 16 : 24);
                var shortWindow = ClientSize.Height < Px(480) || (auditTextScale > 1.25f && ClientSize.Height < Px(600));
                var detailParent = shortWindow ? content : header;
                if (statusDetail.Parent != detailParent) detailParent.Controls.Add(statusDetail);
                var hintParent = shortWindow ? content : footer;
                if (actionHint.Parent != hintParent) hintParent.Controls.Add(actionHint);
                if (openApp.Parent != hintParent) hintParent.Controls.Add(openApp);
                var showBrand = ClientSize.Width >= Px(900) && ClientSize.Height >= Px(540) && auditTextScale <= 1.25f;
                var left = showBrand ? Px(218) : 0; brand.Visible = showBrand;
                brand.SetBounds(0, 0, left, ClientSize.Height);
                if (showBrand)
                {
                    var bx = Px(24); var bw = left - bx * 2;
                    brandLogo.SetBounds(bx, Px(36), Px(76), Px(76));
                    var by = PlaceText(brandName, bx, Px(138), bw) + Px(30);
                    PlaceText(tagline, bx, by, bw);
                    PlaceText(brandVersion, bx, ClientSize.Height - TextHeight(brandVersion, bw) - Px(28), bw);
                }
                var rightWidth = ClientSize.Width - left;
                var width = Math.Max(1, rightWidth - pad * 2);
                var y = PlaceText(identity, pad, Px(16), width) + Px(4);
                y = PlaceText(productVersion, pad, y, width) + Px(16);
                y = PlaceText(statusTitle, pad, y, width) + Px(5);
                if (!shortWindow) y = PlaceText(statusDetail, pad, y, width) + Px(10);
                progress.SetBounds(pad, y, width, Px(7)); y += Px(21);
                header.SetBounds(left, 0, rightWidth, y);
                var fy = Px(12);
                actionHint.Visible = !string.IsNullOrEmpty(actionHint.Text);
                if (actionHint.Visible && !shortWindow) fy = PlaceText(actionHint, pad, fy, width) + Px(8);
                if (openApp.Visible && !shortWindow)
                {
                    openApp.SetBounds(pad, fy, width, TextHeight(openApp, Math.Max(1, width - Px(28))) + Px(5));
                    fy += openApp.Height + Px(8);
                }
                var primaryWidth = Math.Min(width, TextRenderer.MeasureText(primary.Text, primary.Font).Width + Px(34));
                var secondaryWidth = TextRenderer.MeasureText(secondary.Text, secondary.Font).Width + Px(30);
                var buttonHeight = Math.Max(Px(40), TextHeight(primary, primaryWidth - Px(20)) + Px(12));
                var stacked = secondary.Visible && primaryWidth + secondaryWidth + Px(12) > width;
                if (stacked)
                {
                    primary.SetBounds(pad, fy, width, buttonHeight); fy += buttonHeight + Px(8);
                    secondary.SetBounds(pad, fy, width, buttonHeight);
                }
                else
                {
                    primary.SetBounds(rightWidth - pad - primaryWidth, fy, primaryWidth, buttonHeight);
                    secondary.SetBounds(primary.Left - Px(12) - secondaryWidth, fy, secondaryWidth, buttonHeight);
                }
                fy += buttonHeight + Px(12);
                footer.SetBounds(left, ClientSize.Height - fy, rightWidth, fy);
                scroller.SetBounds(left + pad, header.Bottom, width, Math.Max(1, footer.Top - header.Bottom - Px(10)));
                // Reserve a vertical gutter even before a scrollbar is needed: no width oscillation.
                var contentWidth = Math.Max(1, width - Math.Max(Px(20), SystemInformation.VerticalScrollBarWidth));
                var cy = 0;
                if (shortWindow)
                {
                    cy = PlaceText(statusDetail, 0, cy, contentWidth) + Px(10);
                    if (actionHint.Visible) cy = PlaceText(actionHint, 0, cy, contentWidth) + Px(10);
                    if (openApp.Visible)
                    {
                        openApp.SetBounds(0, cy, contentWidth, TextHeight(openApp, Math.Max(1, contentWidth - Px(28))) + Px(5));
                        cy += openApp.Height + Px(10);
                    }
                }
                foreach (var card in requirementCards)
                {
                    var innerWidth = Math.Max(1, contentWidth - Px(28));
                    var ty = PlaceText(card.Controls[0], Px(14), Px(10), innerWidth) + Px(3);
                    ty = PlaceText(card.Controls[1], Px(14), ty, innerWidth) + Px(10);
                    card.SetBounds(0, cy, contentWidth, ty); cy += ty + Px(10);
                }
                cy = PlaceText(consent, 0, cy + Px(3), contentWidth) + Px(12);
                foreach (Control control in content.Controls)
                    if (control is LinkLabel) cy = PlaceText(control, 0, cy, contentWidth) + Px(12);
                content.Size = new Size(contentWidth, cy);
                scroller.AutoScrollMinSize = new Size(0, cy);
            }
            finally { arranging = false; }
        }
    }
}
