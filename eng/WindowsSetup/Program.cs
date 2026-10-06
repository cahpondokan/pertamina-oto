using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: AssemblyTitle("PERTAMINA OTO Setup")]
[assembly: AssemblyProduct("PERTAMINA OTO")]
[assembly: AssemblyCompany("PERTAMINA OTO")]
[assembly: AssemblyDescription("Pemasang OTO dengan pemeriksaan kebutuhan Windows")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.8")]

namespace Oto.Setup
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            AppContext.SetSwitch("Switch.System.IO.UseLegacyPathHandling", false);
            AppContext.SetSwitch("Switch.System.IO.BlockLongPaths", false);
            try
            {
                if (args.Length == 2 && args[0] == "--self-test") return SetupTests.RunAsync(Path.GetFullPath(args[1])).GetAwaiter().GetResult();
                if (args.Length == 3 && args[0] == "--verify-runtime")
                {
                    using (var file = new FileStream(args[1], FileMode.Open, FileAccess.Read, FileShare.Read)) TrustedRuntime.VerifyHash(file, RuntimeRequirement.Sha512);
                    TrustedRuntime.VerifyMicrosoftSignature(args[1]); File.WriteAllText(args[2], "PASS pinned SHA512 and Microsoft Authenticode; executable was not run."); return 0;
                }
                if (args.Length == 2 && args[0] == "--verify-bundle")
                {
                    SetupOperations.VerifyPayload(); var output = Path.GetFullPath(args[1]); Directory.CreateDirectory(output);
                    using (var payload = SetupOperations.Payload()) SetupOperations.Extract(payload, output, PayloadInfo.ExpandedBytes, new Progress<SetupProgress>(), CancellationToken.None);
                    File.WriteAllText(Path.Combine(output, "bundle-verified.txt"), "PASS embedded payload SHA256 and bounded extraction. Version " + PayloadInfo.Version); return 0;
                }
                Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
                if (args.Length == 2 && args[0] == "--audit-ui") return Audit(Path.GetFullPath(args[1]));
                if (args.Length != 0) return 2;
                using (var window = new SetupWindow()) Application.Run(window);
                return 0;
            }
            catch (Exception error)
            {
                if (args.Length == 2 && args[0] == "--verify-bundle") File.WriteAllText(Path.Combine(args[1], "bundle-error.txt"), error.ToString());
                if (args.Length > 0) return 1;
                MessageBox.Show("Installer belum dapat dibuka. Unduh ulang dari oto.ayani.id dan pastikan Windows memenuhi kebutuhan aplikasi.", "PERTAMINA OTO", MessageBoxButtons.OK, MessageBoxIcon.Information); return 1;
            }
        }
        private static int Audit(string output)
        {
            Directory.CreateDirectory(output); var checks = new System.Collections.Generic.List<string>();
            var states = new[] { "ready", "runtime-needed", "downloading", "failed", "complete", "restart" };
            foreach (var state in states)
            {
                using (var form = new SetupWindow(true))
                {
                    form.Show(); form.AuditState(state); Application.DoEvents(); form.PerformLayout();
                    if (!form.ActionsVisible()) throw new InvalidOperationException("Action clipped: " + state);
                    using (var bitmap = new Bitmap(form.Width, form.Height)) { form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size)); bitmap.Save(Path.Combine(output, state + ".png"), ImageFormat.Png); }
                    checks.Add("PASS native wizard " + state + " state and visible action");
                    if (state == "runtime-needed")
                    {
                        form.ClientSize = new Size(780, 620); Application.DoEvents(); form.PerformLayout();
                        if (!form.ActionsVisible()) throw new InvalidOperationException("Compact action clipped");
                        using (var bitmap = new Bitmap(form.Width, form.Height)) { form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size)); bitmap.Save(Path.Combine(output, "compact.png"), ImageFormat.Png); }
                        checks.Add("PASS compact native wizard action remains visible");
                    }
                    form.AuditDone(); form.Close();
                }
            }
            File.WriteAllLines(Path.Combine(output, "audit.txt"), checks); return 0;
        }
    }
}
