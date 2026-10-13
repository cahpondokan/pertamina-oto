using System.Text.Json;
using Microsoft.Win32;
using Pertamina.Oto.Desktop.App;

namespace Pertamina.Oto.PdfOpener;

internal static class PdfAssociationTests
{
    private static readonly string[] FailureStages = ["input", "file", "installation", "launch"];
    internal static int Run(string evidenceRoot)
    {
        var output = Path.Combine(evidenceRoot, "pdf-association-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(output);
        var report = new List<string>();
        var testRegistryPath = @"Software\PertaminaOTO\AssociationTests\" + Guid.NewGuid().ToString("N");
        void Check(bool ok, string name) { if (!ok) throw new InvalidOperationException(name); report.Add("PASS " + name); }
        try
        {
            var source = Path.Combine(output, "payload"); Directory.CreateDirectory(source);
            foreach (var file in new[] { PdfAssociation.HelperName, "PertaminaOTO.PdfOpener.dll", "PertaminaOTO.PdfOpener.deps.json", "PertaminaOTO.PdfOpener.runtimeconfig.json" }) File.WriteAllText(Path.Combine(source, file), "synthetic " + file);
            var root = Path.Combine(output, "Instalasi uji é"); Directory.CreateDirectory(root);
            var helper = PdfAssociation.PrepareHelper(root, source);
            Check(File.Exists(helper) && helper.StartsWith(root, StringComparison.Ordinal), "helper deployed under isolated install root");
            Check(PdfAssociation.PrepareHelper(root, source) == helper, "repeated preparation reuses verified immutable helper");
            var plan = PdfAssociation.Registration(helper);
            Check(plan.All(v => v.Key.StartsWith(@"Software\", StringComparison.Ordinal) && !v.Key.Contains("UserChoice", StringComparison.Ordinal) && !v.Key.Contains("FileExts", StringComparison.Ordinal)), "registration never writes user default or protected association data");
            Check(plan.Any(v => v.Key == @"Software\RegisteredApplications" && v.Name == PdfAssociation.AppName && (string)v.Data == PdfAssociation.Capabilities), "per-user RegisteredApplications capability mapping present");
            Check(plan.Any(v => v.Key.EndsWith(@"\FileAssociations", StringComparison.Ordinal) && v.Name == ".pdf" && (string)v.Data == PdfAssociation.ProgId), "Default Apps identifies PDF capability");
            Check(plan.Single(v => v.Key.EndsWith(@"\.pdf\OpenWithProgids", StringComparison.Ordinal)).Kind == RegistryValueKind.None, "Open With membership uses REG_NONE without changing extension default");
            Check(plan.Where(v => v.Key.EndsWith(@"\shell\open\command", StringComparison.Ordinal)).All(v => (string)v.Data == $"\"{helper}\" \"%1\""), "registry command quotes helper and PDF placeholder");
            using (var testRoot = Registry.CurrentUser.CreateSubKey(testRegistryPath))
            {
                // This root is inert: Software/Classes below it is NOT the live Windows classes hive.
                PdfAssociation.WriteRegistration(testRoot, plan);
                using var membership = testRoot.CreateSubKey(@"Software\Classes\.pdf\OpenWithProgids"); membership.SetValue("UnrelatedTestApp", Array.Empty<byte>(), RegistryValueKind.None);
                PdfAssociation.WriteRegistration(testRoot, plan);
                Check(membership.GetValueNames().Contains("UnrelatedTestApp", StringComparer.Ordinal), "idempotent registration preserves unrelated entries");
                foreach (var entry in plan)
                {
                    using var key = testRoot.OpenSubKey(entry.Key);
                    Check(key is not null && key.GetValueKind(entry.Name) == entry.Kind && (entry.Data is byte[] ? key.GetValue(entry.Name) is byte[] : Equals(key.GetValue(entry.Name), entry.Data)), "registry round-trip: " + entry.Key + " / " + entry.Name);
                }
            }
            Check(PdfAssociation.SettingsUri.AbsoluteUri == "ms-settings:defaultapps?registeredAppUser=PERTAMINA%20OTO", "settings URI targets per-user app registration");
            const string first = "20260930-000000-12345678"; const string second = "20260930-000001-12345678";
            foreach (var version in new[] { first, second }) { var dir = Path.Combine(root, "versions", version); Directory.CreateDirectory(dir); File.WriteAllText(Path.Combine(dir, "Pertamina.Oto.Desktop.App.exe"), "inert fixture, never executed"); }
            void PointTo(string version) => File.WriteAllText(Path.Combine(root, "current.json"), JsonSerializer.Serialize(new LocalAppUpdate.Release(version, "")));
            var pdf = Path.Combine(output, "Contoh berkas é & biaya (1).pdf"); File.WriteAllText(pdf, "%PDF-synthetic command fixture");
            PointTo(first); var command = PdfOpenCommand.Create(root, [pdf]);
            Check(!command.UseShellExecute && command.ArgumentList.SequenceEqual(["--open-reader", pdf]), "spaces Unicode and shell characters remain one literal PDF argument");
            Check(command.FileName.Contains(first, StringComparison.Ordinal), "helper chooses current installed version");
            PointTo(second); Check(PdfOpenCommand.Create(root, [pdf]).FileName.Contains(second, StringComparison.Ordinal), "unchanged helper follows subsequent update pointer");
            PointTo("../../escape"); var traversal = false;
            try { PdfOpenCommand.Create(root, [pdf]); } catch (InvalidDataException) { traversal = true; }
            Check(traversal, "invalid current pointer cannot escape installation"); PointTo(first);
            foreach (var input in new[] { "relative.pdf", @"\\server\share\file.pdf", "https://example.invalid/file.pdf", Path.Combine(output, "missing.pdf"), Path.ChangeExtension(pdf, ".exe"), pdf + ":ads.pdf", pdf + "\".pdf" })
            {
                var rejected = false; try { PdfOpenCommand.Create(root, [input]); } catch (Exception) { rejected = true; }
                Check(rejected, "unsupported or unsafe input rejected");
            }
            foreach (var args in new string[][] { [], [pdf, pdf], ["--install", pdf] })
            {
                var rejected = false; try { PdfOpenCommand.Create(root, args); } catch (ArgumentException) { rejected = true; }
                Check(rejected, "extra arguments cannot select internal tool actions");
            }
            File.WriteAllText(helper, "tampered"); var damaged = false;
            try { PdfAssociation.PrepareHelper(root, source); } catch (InvalidDataException) { damaged = true; }
            Check(damaged && File.ReadAllText(helper) == "tampered", "damaged helper rejected without overwriting existing binaries");
            Check(File.ReadAllText(pdf) == "%PDF-synthetic command fixture", "activation planning preserves source bytes");
            var stage = "input";
            try { PdfOpenCommand.Create(root, [Path.Combine(output, "missing.pdf")], value => stage = value); } catch (FileNotFoundException) { }
            Check(stage == "file", "missing PDF classified as file failure");
            File.WriteAllText(Path.Combine(root, "current.json"), "invalid JSON");
            try { PdfOpenCommand.Create(root, [pdf], value => stage = value); } catch (JsonException) { }
            Check(stage == "installation", "invalid pointer classified as installation failure");
            PointTo(first);
            File.Delete(Path.Combine(root, "versions", first, "Pertamina.Oto.Desktop.App.exe"));
            try { PdfOpenCommand.Create(root, [pdf], value => stage = value); } catch (FileNotFoundException) { }
            Check(stage == "installation", "missing application classified separately from missing PDF");
            Check(FailureStages.Select(s => PdfOpenFailure.Message(s, new IOException("private.pdf"))).Distinct().Count() == 4,
                "four failure stages have distinct actionable messages");
            Check(!PdfOpenFailure.Message("launch", new IOException("private.pdf")).Contains("private.pdf", StringComparison.Ordinal), "error message does not expose exception details");
            report.Add($"SUMMARY {report.Count} PASS"); return 0;
        }
        catch (Exception error) { report.Add("FAIL " + error.GetType().Name + ": " + error.Message); return 1; }
        finally
        {
            // Delete only the fresh GUID-owned inert test subtree, never live association keys.
            Registry.CurrentUser.DeleteSubKeyTree(testRegistryPath, throwOnMissingSubKey: false);
            File.WriteAllLines(Path.Combine(output, "audit.txt"), report);
        }
    }
}
