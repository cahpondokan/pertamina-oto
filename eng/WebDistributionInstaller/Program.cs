using System.Runtime.Versioning;
using Pertamina.Oto.Desktop.App;

if (!OperatingSystem.IsWindows() || !Environment.Is64BitOperatingSystem) return 1;
try
{
    if (args.Length == 2 && args[0] == "--verify") { InstallPackage.Validate(Path.GetFullPath(args[1])); Console.WriteLine("Package verified."); return 0; }
    if (args.Length == 1 && args[0] == "--install-silent")
    {
        // Called only after the visible bootstrapper's user-initiated install flow.
        InstallPackage.Install(AppContext.BaseDirectory, LocalAppUpdate.InstallRoot);
        try { InstallPackage.RegisterUninstall(AppContext.BaseDirectory); }
        catch (Exception) { return 3; }
        try { CreateShortcut(); return 0; }
        catch (Exception) { return 2; }
    }
    var package = AppContext.BaseDirectory;
    var release = LocalAppUpdate.InspectFeed(Path.Combine(package, "feed")).Release;
    Console.WriteLine("PERTAMINA OTO — Pemasangan Windows (pengembangan)");
    Console.WriteLine("Versi " + release.Version);
    Console.WriteLine("Simpan pekerjaan Anda. Aplikasi yang sedang berjalan tidak akan ditutup.");
    Console.WriteLine("Versi sebelumnya, pengaturan, dan dokumen tetap disimpan.");
    Console.Write("Pasang untuk pengguna Windows ini? Ketik PASANG lalu Enter: ");
    if (!string.Equals(Console.ReadLine()?.Trim(), "PASANG", StringComparison.OrdinalIgnoreCase)) return 0;
    InstallPackage.Install(package, LocalAppUpdate.InstallRoot);
    try { InstallPackage.RegisterUninstall(package); }
    catch (Exception) { Console.WriteLine("OTO terpasang, tetapi pendaftaran uninstall gagal. Jalankan ulang installer untuk memperbaikinya."); Console.ReadLine(); return 3; }
    try { CreateShortcut(); }
    catch (Exception) { Console.WriteLine("Shortcut belum dapat dibuat. Buka PERTAMINA-OTO.exe di %LOCALAPPDATA%/Programs/PertaminaOTO."); }
    Console.WriteLine("Pemasangan selesai. Tutup OTO setelah menyimpan pekerjaan, lalu buka kembali shortcut PERTAMINA OTO.");
    Console.WriteLine("Tekan Enter untuk menutup."); Console.ReadLine(); return 0;
}
catch (Exception)
{
    if (args.Length == 1 && args[0] == "--install-silent") return 1;
    Console.WriteLine("Paket belum dapat dipasang. Pastikan ZIP diekstrak lengkap ke folder lokal dan versi ini lebih baru.");
    Console.WriteLine("Versi aktif dan dokumen tidak dihapus. Unduh ulang dari oto.ayani.id jika paket rusak.");
    Console.WriteLine("Tekan Enter untuk menutup."); Console.ReadLine(); return 1;
}

[SupportedOSPlatform("windows")]
static void CreateShortcut()
{
    var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    var path = Path.Combine(desktop, "PERTAMINA OTO.lnk");
    if (File.Exists(path)) return;
    dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell")!)!;
    dynamic shortcut = shell.CreateShortcut(path);
    shortcut.TargetPath = Path.Combine(LocalAppUpdate.InstallRoot, "PERTAMINA-OTO.exe");
    shortcut.WorkingDirectory = LocalAppUpdate.InstallRoot;
    shortcut.Description = "PERTAMINA OTO";
    shortcut.IconLocation = shortcut.TargetPath + ",0";
    shortcut.Save();
}
