using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pertamina.Oto.LocalPdf;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class PdfReaderView
    {
        private readonly TextBox searchInput = new() { PlaceholderText = "Cari isi PDF · Ctrl+F", MaxLength = 200, MinWidth = 100 };
        private readonly Button searchButton = HistoryButton("search", "Cari", true);
        private readonly Button searchCancel = HistoryButton("x", "Batalkan pencarian", true);
        private readonly Button searchPrevious = HistoryButton("chevron-left", "Hasil sebelumnya");
        private readonly Button searchNext = HistoryButton("chevron-right", "Hasil berikutnya");
        private readonly TextBlock searchStatus = HistoryText("", 12);
        private readonly StackPanel searchPanel = new() { Spacing = 6, Padding = new Thickness(16, 0, 16, 8), Visibility = Visibility.Collapsed };
        private readonly ListView searchResults = new() { MaxHeight = 120, SelectionMode = ListViewSelectionMode.Single };
        private CancellationTokenSource? searchCancellation;
        private int searchGeneration;
        private PdfSearchHit[] searchHits = [];
        private string? searchedText;
        private Task searchNavigation = Task.CompletedTask;
        private string searchCoverage = "";
        private int searchNavigationGeneration;
        private int searchViewportGeneration = -1;
        private PdfSearchHit[]? overlayHits;
        private PageSlot[]? overlayPages;
        private List<Border>[] searchMarkers = [];
        private int activeMarker = -1;
        private bool searchCompleted;
#if DEBUG
        private int searchWorkerRuns;
#endif

        private Grid BuildSearchControls()
        {
            var controls = new Grid { ColumnSpacing = 6, VerticalAlignment = VerticalAlignment.Center };
            controls.ColumnDefinitions.Add(new ColumnDefinition()); controls.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            controls.Children.Add(searchInput); Grid.SetColumn(searchButton, 1); controls.Children.Add(searchButton);
            AutomationProperties.SetName(searchInput, "Cari teks dalam PDF");
            ToolTipService.SetToolTip(searchInput, "Cari teks yang dapat dibaca; gambar scan memerlukan OCR. Tekan Enter untuk mencari.");
            searchInput.KeyDown += async (_, e) =>
            {
                if (e.Key == VirtualKey.Escape) { e.Handled = true; ClearSearch(); viewport.Focus(FocusState.Programmatic); }
                if (e.Key == VirtualKey.Enter)
                {
                    e.Handled = true;
                    var shift = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
                    await SearchEnterAsync(shift);
                }
            };
            searchInput.TextChanged += (_, _) => { if (searchedText != searchInput.Text) ClearSearch(); };
            searchButton.Click += async (_, _) => await SearchAsync();
            searchCancel.Click += (_, _) => CancelSearch();
            var summary = new Grid { ColumnSpacing = 8 };
            summary.ColumnDefinitions.Add(new ColumnDefinition()); summary.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            searchStatus.TextWrapping = TextWrapping.Wrap; summary.Children.Add(searchStatus);
            AutomationProperties.SetLiveSetting(searchStatus, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
            var actions = new ResponsiveWrapPanel { Spacing = 4 };
            var close = HistoryButton("x", "Tutup hasil pencarian"); close.Click += (_, _) => ClearSearch();
            foreach (var button in new[] { searchPrevious, searchNext, searchCancel, close }) actions.Children.Add(button);
            searchPrevious.Click += (_, _) => SelectSearchResult(-1); searchNext.Click += (_, _) => SelectSearchResult(1);
            Grid.SetColumn(actions, 1); summary.Children.Add(actions); ResponsiveLayout.Header(summary, searchStatus, actions, 600); searchPanel.Children.Add(summary);
            AutomationProperties.SetName(searchResults, "Hasil pencarian PDF; pilih untuk membuka halaman");
            searchResults.SelectionChanged += (_, _) =>
            {
                var index = searchResults.SelectedIndex;
                if (index >= 0 && index < searchHits.Length && Visible && !busy)
                { searchResults.ScrollIntoView(searchResults.SelectedItem); searchNavigation = NavigateSearchAsync(index); }
            };
            searchPanel.Children.Add(searchResults); Grid.SetRow(searchPanel, 3); View.Children.Add(searchPanel);
            return controls;
        }

        internal void FocusSearch() { searchInput.Focus(FocusState.Programmatic); searchInput.SelectAll(); }

        private async Task SearchEnterAsync(bool shift)
        {
            if (searchedText == searchInput.Text && searchHits.Length > 0)
            { SelectSearchResult(shift ? -1 : 1); await searchNavigation; }
            else await SearchAsync();
        }

        private void DrawSearchOverlay()
        {
            if (ReferenceEquals(overlayHits, searchHits) && ReferenceEquals(overlayPages, pageSlots))
            {
                SetMarkerActive(activeMarker, false); activeMarker = searchResults.SelectedIndex; SetMarkerActive(activeMarker, true);
                return;
            }
            foreach (var slot in pageSlots) slot.SearchOverlay.Children.Clear();
            overlayHits = searchHits; overlayPages = pageSlots; activeMarker = searchResults.SelectedIndex;
            searchMarkers = Enumerable.Range(0, searchHits.Length).Select(_ => new List<Border>()).ToArray();
            for (var i = 0; i < searchHits.Length; i++)
            {
                var hit = searchHits[i];
                if (hit.Page < 0 || hit.Page >= pageSlots.Length || hit.Boxes is null) continue;
                var slot = pageSlots[hit.Page]; var active = i == searchResults.SelectedIndex;
                foreach (var box in hit.Boxes)
                {
                    var marker = new Border { Width = box.Width * slot.Image.Width, Height = box.Height * slot.Image.Height,
                        Background = new SolidColorBrush(Windows.UI.Color.FromArgb(70, 255, active ? (byte)140 : (byte)230, 0)),
                        BorderBrush = active ? Brush(163, 77, 0) : null, BorderThickness = new Thickness(active ? 1 : 0), IsHitTestVisible = false };
                    Canvas.SetLeft(marker, box.X * slot.Image.Width); Canvas.SetTop(marker, box.Y * slot.Image.Height);
                    slot.SearchOverlay.Children.Add(marker);
                    searchMarkers[i].Add(marker);
                }
            }
        }

        private void SetMarkerActive(int index, bool active)
        {
            if (index < 0 || index >= searchMarkers.Length) return;
            foreach (var marker in searchMarkers[index])
            {
                marker.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(70, 255, active ? (byte)140 : (byte)230, 0));
                marker.BorderBrush = active ? Brush(163, 77, 0) : null;
                marker.BorderThickness = new Thickness(active ? 1 : 0);
            }
        }

        private async Task NavigateSearchAsync(int index)
        {
            if (index < 0 || index >= searchHits.Length || document is null || suspended || !Visible) return;
            var ticket = searchGeneration; var navigation = ++searchNavigationGeneration;
            var hit = searchHits[index]; var original = path; var pages = pageSlots; var zoom = viewport.ZoomFactor;
            bool Current() => ticket == searchGeneration && navigation == searchNavigationGeneration && path == original
                && ReferenceEquals(pages, pageSlots) && searchResults.SelectedIndex == index && Visible && !suspended
                && Math.Abs(viewport.ZoomFactor - zoom) < .0001;
            DrawSearchOverlay();
            searchStatus.Text = $"Hasil {index + 1} dari {searchHits.Length} · Halaman {hit.Page + 1}. " + searchCoverage
                + (hit.Boxes is not { Length: > 0 } ? " Teks ditemukan di halaman ini; posisi tepat belum tersedia." : "");
            fit = false; notice.Text = "";
            var slot = pages[hit.Page];
            var horizontal = Math.Max(0, (pageSurface.Width * zoom - viewport.ActualWidth) / 2);
            var vertical = slot.Top * zoom;
            if (hit.Boxes is { Length: > 0 })
            {
                var box = hit.Boxes[0];
                var left = (pageSurface.Width - slot.Image.Width) / 2 + box.X * slot.Image.Width;
                var top = (slot.Top + box.Y * slot.Image.Height) * zoom;
                horizontal = viewport.HorizontalOffset; vertical = viewport.VerticalOffset;
                if (left * zoom < horizontal || (left + box.Width * slot.Image.Width) * zoom > horizontal + viewport.ViewportWidth)
                    horizontal = Math.Max(0, left * zoom - viewport.ViewportWidth * .3);
                if (top < vertical || top + box.Height * slot.Image.Height * zoom > vertical + viewport.ViewportHeight)
                    vertical = Math.Max(slot.Top * zoom, top - viewport.ViewportHeight * .25);
            }
            var destinationPage = PageAt(vertical / zoom);
            var first = Math.Max(0, destinationPage - 1);
            var last = Math.Min(pages.Length - 1, PageAt((vertical + viewport.ViewportHeight) / zoom) + 1);
            // Prepare the destination before moving. The currently visible images stay valid.
            var renderTicket = ++generation;
            Rendering = RenderVisiblePagesAsync(first, last, renderTicket, prepareNavigation: true, stillCurrent: Current);
            await Rendering;
            if (!Current() || renderTicket != generation) return;
            settingZoom = true;
            searchViewportGeneration = renderTicket;
            var settled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            void Changed(object? sender, ScrollViewerViewChangedEventArgs e) { if (!e.IsIntermediate) settled.TrySetResult(); }
            viewport.ViewChanged += Changed;
            try
            {
                UpdatePageExtent(zoom);
                if (Math.Abs(horizontal - viewport.HorizontalOffset) > .5 || Math.Abs(vertical - viewport.VerticalOffset) > .5)
                    if (viewport.ChangeView(horizontal, vertical, null, true)) await Task.WhenAny(settled.Task, Task.Delay(250));
            }
            finally
            {
                viewport.ViewChanged -= Changed;
                if (searchViewportGeneration == renderTicket) searchViewportGeneration = -1;
                if (renderTicket == generation) settingZoom = false;
            }
            if (!Current() || renderTicket != generation) return;
            RefreshScroll(force: true); await Rendering;
        }

        private void SelectSearchResult(int direction)
        {
            if (searchHits.Length == 0) return;
            searchResults.SelectedIndex = (searchResults.SelectedIndex + direction + searchHits.Length) % searchHits.Length;
            searchResults.ScrollIntoView(searchResults.SelectedItem);
        }

        private void UpdateSearchControls()
        {
            searchInput.IsEnabled = document is not null && !busy;
            searchButton.IsEnabled = document is not null && !busy && searchCancellation is null && !string.IsNullOrWhiteSpace(searchInput.Text);
            searchCancel.Visibility = searchCancellation is null ? Visibility.Collapsed : Visibility.Visible;
            searchPrevious.IsEnabled = searchNext.IsEnabled = !busy && searchHits.Length > 0;
        }

        private void CancelSearch()
        {
            searchGeneration++;
            searchNavigationGeneration++;
            // Cancellation releases only this search's scroll suppression and prepared neighbors.
            if (searchViewportGeneration == generation)
            { searchViewportGeneration = -1; settingZoom = false; RefreshScroll(); }
            for (var i = 0; i < pageSlots.Length; i++)
                if (i < renderedRange.First || i > renderedRange.Last)
                {
#if DEBUG
                    if (pageSlots[i].Image.Source is not null) bitmapEvictions++;
#endif
                    pageSlots[i].Image.Source = null; pageSlots[i].RenderWidth = 0;
                }
            if (searchCancellation is not null)
            {
                searchCancellation.Cancel(); searchCancellation = null;
                searchHits = []; searchResults.Items.Clear(); DrawSearchOverlay();
                searchStatus.Text = "Pencarian dibatalkan. Dokumen tetap tersedia.";
            }
            UpdateSearchControls();
        }

        private void ClearSearch()
        {
            CancelSearch(); searchedText = null; searchCompleted = false; searchHits = []; searchResults.Items.Clear(); searchStatus.Text = "";
            searchCoverage = ""; DrawSearchOverlay();
            searchPanel.Visibility = Visibility.Collapsed; UpdateSearchControls();
        }

        private async Task SearchAsync()
        {
            if (document is null || path is null || busy || !Visible) return;
            if (searchedText == searchInput.Text && (searchCancellation is not null || searchCompleted))
            {
                if (searchCompleted && searchHits.Length > 0) { SelectSearchResult(1); await searchNavigation; }
                return;
            }
            ClearSearch(); var query = searchInput.Text.Trim();
            if (query.Length == 0) return;
            searchedText = searchInput.Text;
            var original = path; var ticket = searchGeneration;
            using var cancellation = new CancellationTokenSource(); searchCancellation = cancellation;
            searchPanel.Visibility = Visibility.Visible; searchStatus.Text = "Mencari teks lokal…"; UpdateSearchControls();
            try
            {
#if DEBUG
                searchWorkerRuns++;
#endif
                var reply = await WorkerClient.RunAsync(new("reader-search", [original],
                    Path.Combine(Path.GetTempPath(), "oto-search-" + Guid.NewGuid().ToString("N") + ".pdf"), Text: query), cancellation.Token);
                if (ticket != searchGeneration || cancellation.IsCancellationRequested || path != original || !Visible) return;
                if (!reply.Success || reply.Search is null) { searchStatus.Text = reply.Message; return; }
                var result = reply.Search; searchHits = result.Hits; searchCompleted = true;
                var coverage = $"Teks terbaca di {result.TextPages}/{reply.Pages} halaman.";
                var limited = result.PartialPages > 0 ? $" {result.PartialPages} halaman memiliki bagian yang belum didukung; hasil dapat belum lengkap." : "";
                var noText = result.TextPages < reply.Pages ? " Halaman tanpa teks terbaca mungkin berupa scan; perlu OCR. Pencarian ini tidak menjalankan OCR." : "";
                searchCoverage = (result.Truncated ? "Ditampilkan 100 pertama; hasil terpotong. " : "") + coverage + limited + noText;
                searchStatus.Text = (searchHits.Length == 0 ? "Tidak ditemukan pada teks yang dapat dibaca. "
                    : $"{searchHits.Length}{(result.Truncated ? "+" : "")} hasil{(result.Truncated ? "; ditampilkan 100 pertama" : "")}. Pilih hasil untuk membuka halaman. ") + coverage + limited + noText;
                foreach (var hit in searchHits)
                {
                    var label = $"Halaman {hit.Page + 1} · {hit.Snippet}";
                    var excerpt = new TextBlock { TextWrapping = TextWrapping.Wrap, MaxLines = 2, TextTrimming = TextTrimming.CharacterEllipsis };
                    excerpt.Inlines.Add(new Run { Text = $"Halaman {hit.Page + 1} · " });
                    var at = hit.SnippetMatch; var length = hit.MatchLength;
                    if (length <= 0 || at < 0 || at + length > hit.Snippet.Length) excerpt.Inlines.Add(new Run { Text = hit.Snippet });
                    else
                    {
                        excerpt.Inlines.Add(new Run { Text = hit.Snippet[..at] });
                        excerpt.Inlines.Add(new Run { Text = hit.Snippet.Substring(at, length), FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = Brush(8, 102, 232) });
                        excerpt.Inlines.Add(new Run { Text = hit.Snippet[(at + length)..] });
                    }
                    var item = new ListViewItem { Content = excerpt, HorizontalContentAlignment = HorizontalAlignment.Stretch };
                    AutomationProperties.SetName(item, label); searchResults.Items.Add(item);
                }
                if (searchHits.Length > 0) { searchResults.SelectedIndex = 0; await searchNavigation; }
            }
            catch (Exception error)
            {
                if (ticket == searchGeneration) { LocalDiagnostics.Record("reader-search", error); searchStatus.Text = "Pencarian gagal. Coba lagi; dokumen tidak diubah."; }
            }
            finally
            {
                if (ReferenceEquals(searchCancellation, cancellation)) searchCancellation = null;
                UpdateSearchControls();
            }
        }
    }
}
