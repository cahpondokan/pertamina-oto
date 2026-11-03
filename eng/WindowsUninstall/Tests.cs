using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using Microsoft.Win32;

namespace Oto.Uninstall
{
    internal static class Tests
    {
        private static readonly List<string> Results = new List<string>();
        private static void Check(bool value, string label) { if (!value) throw new Exception(label); Results.Add("PASS " + label); }
        private static void Reject(Action action, string label)
        { try { action(); } catch (Exception error) when (error is IOException || error is InvalidDataException) { Results.Add("PASS " + label); return; } throw new Exception(label); }
        private const string Version = "20261014-002535-877d8390";
        private static string Fixture(string evidence, string name)
        {
            var root = Path.Combine(evidence, name); Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "current.json"), "{\"Version\":\"" + Version + "\",\"ProductVersion\":\"0.3.0\"}");
            File.WriteAllText(Path.Combine(root, "PERTAMINA-OTO.exe"), "inert");
            var app = Path.Combine(root, "versions", Version); Directory.CreateDirectory(app);
            File.WriteAllText(Path.Combine(app, "Pertamina.Oto.Desktop.App.exe"), "inert");
            File.WriteAllText(Path.Combine(app, "runtime.dll"), "inert");
            return root;
        }
        internal static int Run(string evidence)
        {
            Directory.CreateDirectory(evidence);
            var subtree = @"Software\PertaminaOTO-UninstallTest-" + Guid.NewGuid().ToString("N");
            try
            {
                using (var registry = Registry.CurrentUser.CreateSubKey(subtree))
                {
                    var root = Fixture(evidence, "installation with spaces");
                    var documents = Path.Combine(evidence, "documents"); Directory.CreateDirectory(documents);
                    File.WriteAllText(Path.Combine(documents, "keep.pdf"), "personal document");
                    var settings = Path.Combine(evidence, "PertaminaOTO"); Directory.CreateDirectory(settings);
                    File.WriteAllText(Path.Combine(settings, "history.json"), "personal history");
                    File.WriteAllText(Path.Combine(root, "keep.pdf"), "unknown root file");
                    File.WriteAllText(Path.Combine(root, "unknown.dll"), "unknown root file");
                    Program.Register(root, registry);
                    using (var key = registry.OpenSubKey(Program.Key))
                    {
                        Check((string)key.GetValue("DisplayName") == "PERTAMINA OTO", "registered display name");
                        Check((string)key.GetValue("DisplayVersion") == "0.3.0", "product version");
                        Check((string)key.GetValue("UninstallString") == "\"" + Path.Combine(root, Program.Exe) + "\" --uninstall", "quoted uninstall command");
                        Check((int)key.GetValue("SystemComponent") == 0, "visible in Windows list");
                    }
                    Check(File.Exists(Path.Combine(root, Program.Exe + ".config")), "independent Framework uninstaller deployed");
                    var pointer = File.ReadAllText(Path.Combine(root, "current.json"));
                    Program.Register(root, registry);
                    Check(pointer == File.ReadAllText(Path.Combine(root, "current.json")), "registration is idempotent and preserves selected version");
                    foreach (var name in new[] { "setup.lock", "launcher-maintenance.lock", "update.lock", "pdf-association.lock" })
                    {
                        using (File.Open(Path.Combine(root, name), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) Reject(() => Program.Remove(root, registry, false), "busy " + name + " refuses uninstall");
                        Check(File.Exists(Path.Combine(root, "PERTAMINA-OTO.exe")), "busy refusal preserves application");
                    }
                    using (File.Open(Path.Combine(root, "versions", Version, "runtime.dll"), FileMode.Open, FileAccess.Read, FileShare.None)) Reject(() => Program.Remove(root, registry, false), "locked payload refuses before deleting");
                    Check(File.Exists(Path.Combine(root, "PERTAMINA-OTO.exe")), "locked payload preserves application");
                    Reject(() => Program.Child(root, "../documents/keep.pdf"), "traversal rejected");
                    Reject(() => Program.Child(root, "file:stream"), "alternate stream rejected");
                    Reject(() => Program.Child(root, Path.Combine(evidence, "documents")), "absolute path rejected");
                    using (var choice = registry.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.pdf\UserChoice")) choice.SetValue("ProgId", "Other.Reader");
                    using (var other = registry.CreateSubKey(@"Software\Classes\Applications\PertaminaOTO.PdfOpener.exe\shell\open\command")) other.SetValue("", "\"C:\\other\\reader.exe\" \"%1\"");
                    Program.Remove(root, registry, false);
                    Check(!File.Exists(Path.Combine(root, "PERTAMINA-OTO.exe")), "actual fixture payload removed");
                    using (var key = registry.OpenSubKey(Program.Key)) Check(key == null, "uninstall entry removed on success");
                    Check(File.ReadAllText(Path.Combine(documents, "keep.pdf")) == "personal document", "external documents preserved");
                    Check(File.ReadAllText(Path.Combine(settings, "history.json")) == "personal history", "user history preserved");
                    Check(File.Exists(Path.Combine(root, "keep.pdf")) && File.Exists(Path.Combine(root, "unknown.dll")), "unknown root files preserved");
                    using (var choice = registry.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.pdf\UserChoice")) Check((string)choice.GetValue("ProgId") == "Other.Reader", "Windows UserChoice preserved");
                    using (var other = registry.OpenSubKey(@"Software\Classes\Applications\PertaminaOTO.PdfOpener.exe\shell\open\command")) Check(other != null, "unrelated file association preserved");

                    var retry = Fixture(evidence, "retry"); Program.Register(retry, registry);
                    var plan = Program.BuildPlan(retry);
                    var journal = new Program.Journal { Version = Version, Files = plan.Files.Select(p => p.Substring(retry.Length + 1)).ToArray(), Directories = plan.Directories.Select(p => p.Substring(retry.Length + 1)).ToArray() };
                    File.WriteAllText(Path.Combine(retry, "uninstall-pending.json"), new JavaScriptSerializer().Serialize(journal));
                    File.Delete(plan.Files[0]); // Simulate interruption after one owned file is removed.
                    Program.Remove(retry, registry, false);
                    Check(!Directory.Exists(retry), "interrupted uninstall resumes from bounded journal");

                    var malformed = Fixture(evidence, "malformed");
                    File.WriteAllText(Path.Combine(malformed, "current.json"), "{\"Version\":\"../../documents\"}");
                    Reject(() => Program.BuildPlan(malformed), "malformed installation identity rejected");
                    var escaped = Fixture(evidence, "escaped journal");
                    File.WriteAllText(Path.Combine(escaped, "uninstall-pending.json"), new JavaScriptSerializer().Serialize(new Program.Journal { Version = Version, Files = new[] { "../documents/keep.pdf" }, Directories = new string[0] }));
                    Reject(() => Program.Remove(escaped, registry, false), "escaped recovery journal rejected");
                    var linked = Fixture(evidence, "linked");
                    var link = Path.Combine(linked, "versions", Version, "escape");
                    if (!CreateSymbolicLink(link, documents, 3)) throw new IOException("Reparse-point test needs a permitted symbolic link.");
                    Reject(() => Program.BuildPlan(linked), "nested reparse point rejected before deletion");
                    Check(File.Exists(Path.Combine(documents, "keep.pdf")), "reparse target preserved");
                    Directory.Delete(link, false);
                }
                File.WriteAllLines(Path.Combine(evidence, "results.txt"), Results); return 0;
            }
            catch (Exception error) { Results.Add("FAIL " + error.GetType().Name + " " + error.Message); File.WriteAllLines(Path.Combine(evidence, "results.txt"), Results); return 1; }
            finally { Registry.CurrentUser.DeleteSubKeyTree(subtree, false); }
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CreateSymbolicLink(string link, string target, int flags);
    }
}
