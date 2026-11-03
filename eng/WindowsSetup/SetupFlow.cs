using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oto.Setup
{
    internal sealed class SetupProgress
    {
        internal readonly string Title, Detail;
        internal readonly int Percent;
        internal readonly bool CanCancel;
        internal SetupProgress(string title, string detail, int percent, bool canCancel)
        { Title = title; Detail = detail; Percent = percent; CanCancel = canCancel; }
    }
    internal sealed class SetupOutcome
    {
        internal bool RestartRequired, ShortcutWarning;
    }
    internal interface ISetupOperations
    {
        bool RuntimeBundled { get; }
        string DetectRuntime();
        Task PrepareAsync(IProgress<SetupProgress> progress, CancellationToken cancellation);
        Task DownloadRuntimeAsync(IProgress<SetupProgress> progress, CancellationToken cancellation);
        Task<int> InstallRuntimeAsync();
        Task<int> InstallAppAsync();
    }
    internal static class SetupFlow
    {
        internal static async Task<SetupOutcome> RunAsync(ISetupOperations operations, IProgress<SetupProgress> progress, CancellationToken cancellation)
        {
            var outcome = new SetupOutcome();
            await operations.PrepareAsync(progress, cancellation);
            cancellation.ThrowIfCancellationRequested();
            if (!operations.RuntimeBundled && operations.DetectRuntime() == null)
            {
                await operations.DownloadRuntimeAsync(progress, cancellation);
                cancellation.ThrowIfCancellationRequested();
                progress.Report(new SetupProgress("Memasang .NET 10", "Setujui permintaan izin Windows untuk pemasang Microsoft. Tunggu sampai selesai.", -1, false));
                var code = await operations.InstallRuntimeAsync();
                if (code == 1602 || code == 1223) throw new OperationCanceledException("Pemasangan .NET dibatalkan. OTO belum dipasang.");
                if (code == 1641) throw new InvalidOperationException("Windows memerlukan mulai ulang. Buka kembali installer ini setelah komputer dinyalakan ulang.");
                if (code != 0 && code != 3010) throw new InvalidOperationException("Pemasangan .NET belum berhasil (kode " + code + "). Coba kembali setelah menyelesaikan pemasangan Windows lainnya.");
                if (operations.DetectRuntime() == null) throw new InvalidOperationException(".NET 10 x64 belum terdeteksi setelah pemasangan. Mulai ulang komputer, lalu buka kembali installer ini.");
                outcome.RestartRequired = code == 3010;
            }
            cancellation.ThrowIfCancellationRequested();
            progress.Report(new SetupProgress("Memasang OTO", "Aplikasi dan shortcut sedang disiapkan. Dokumen serta versi sebelumnya tetap disimpan.", -1, false));
            var result = await operations.InstallAppAsync();
            if (result == 3) throw new InvalidOperationException("OTO terpasang, tetapi pendaftaran uninstall belum berhasil. Jalankan ulang installer ini untuk memperbaikinya.");
            if (result != 0 && result != 2) throw new InvalidOperationException("OTO belum dapat dipasang. Pastikan paket ini lebih baru dan tidak ada pemasangan lain yang berjalan. Versi aktif tetap tersedia.");
            outcome.ShortcutWarning = result == 2;
            return outcome;
        }
    }
}
