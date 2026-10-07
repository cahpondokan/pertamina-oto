using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private const double OrganizerCardWidth = 184, OrganizerCardHeight = 250;
    private double OrganizerPitchX => 184 * previewScale + 32;
    private double OrganizerPitchY => 250 * previewScale + 28;
    private readonly Canvas organizerBoard = new() { Background = Brush(247, 248, 252) };
    private readonly Dictionary<long, OrganizerCard> organizerCards = [];
    private readonly Dictionary<(int Source, int Page), Task<ImageSource?>> organizerThumbnails = [];
    private readonly List<Grid> organizerGaps = [];
    private readonly List<TextBlock> organizerSourceLabels = [];
    private readonly TextBlock organizerBlankSummary = Copy("", 12);
    private readonly Border organizerPlaceholder = new() { Width = OrganizerCardWidth, Height = OrganizerCardHeight, BorderBrush = Brush(23, 104, 212), BorderThickness = new Thickness(2), Background = Brush(219, 235, 255), CornerRadius = new CornerRadius(8), IsHitTestVisible = false, Visibility = Visibility.Collapsed };
    private bool organizerConfigured;
    private OrganizerDragOrder? organizerDrag;
    private OrganizerCard? organizerHeld;
    private Pointer? organizerPointer;
    private Point organizerPress, organizerPointerPosition;
    private Border? organizerGhost;
    private int organizerColumns = 1;
    private bool organizerValidDrop;
    private readonly DispatcherTimer organizerScrollTimer = new() { Interval = TimeSpan.FromMilliseconds(30) };

    private sealed class OrganizerCard(OrganizerPage page)
    {
        internal OrganizerPage Page = page;
        internal readonly ContentControl Root = new() { Width = OrganizerCardWidth, Height = OrganizerCardHeight, IsTabStop = true, HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Stretch };
        internal readonly Border Frame = new() { Background = Brush(255, 255, 255), BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(8), Padding = new Thickness(12) };
        internal readonly Image Picture = new() { Width = 154, Height = 194, Stretch = Stretch.Uniform, RenderTransformOrigin = new Point(.5, .5) };
        internal readonly TextBlock Number = new() { HorizontalAlignment = HorizontalAlignment.Center, FontSize = 12 };
        internal readonly StackPanel Actions = new() { Orientation = Orientation.Horizontal, Spacing = 3, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Opacity = 0, IsHitTestVisible = false };
        internal readonly RotateTransform Angle = new();
        internal readonly TranslateTransform Position = new();
        internal Storyboard? Motion;
        internal double X = double.NaN, Y = double.NaN;
        internal bool Hover;
    }

    private void ConfigureOrganizerDrop()
    {
        if (organizerConfigured) return;
        organizerConfigured = true;
        pageGrid.Content = organizerBoard; pageGrid.AllowDrop = true;
        organizerBoard.Children.Add(organizerPlaceholder);
        pageGrid.SizeChanged += (_, _) => { CancelOrganizerDrag(); ArrangeOrganizerCards(pageLayout, false); };
        pageGrid.Unloaded += (_, _) => CancelOrganizerDrag();
        var escape = new KeyboardAccelerator { Key = VirtualKey.Escape };
        escape.Invoked += (_, e) => { if (organizerHeld is null) return; e.Handled = true; CancelOrganizerDrag(); };
        pageGrid.KeyboardAccelerators.Add(escape);
        pageGrid.DragOver += (_, e) => e.AcceptedOperation = active is null && !importing && organizerHeld is null && e.DataView.Contains(StandardDataFormats.StorageItems) ? DataPackageOperation.Copy : DataPackageOperation.None;
        pageGrid.Drop += async (_, e) =>
        {
            e.Handled = true; var deferral = e.GetDeferral();
            try { if (e.DataView.Contains(StandardDataFormats.StorageItems)) await AcceptOrganizerFilesAsync(await e.DataView.GetStorageItemsAsync()); }
            catch (Exception) { status.Text = "File tidak dapat ditambahkan."; }
            finally { deferral.Complete(); }
        };
        organizerScrollTimer.Tick += (_, _) =>
        {
            if (organizerDrag is null) return;
            var top = pageGrid.TransformToVisual(root).TransformPoint(new(0, 0));
            var y = organizerPointerPosition.Y - top.Y;
            var delta = y < 36 ? -18 : y > pageGrid.ActualHeight - 36 ? 18 : 0;
            if (delta != 0) pageGrid.ChangeView(null, Math.Clamp(pageGrid.VerticalOffset + delta, 0, pageGrid.ScrollableHeight), null, true);
        };
        pageGrid.ViewChanged += (_, _) => { if (organizerDrag is not null) PreviewOrganizerDrag(organizerPointerPosition); };
        Activated += (_, e) => { if (e.WindowActivationState == WindowActivationState.Deactivated) CancelOrganizerDrag(); };
    }

    private void RenderOrganizerCards()
    {
        // Reconcile by stable identity. Never clear the board or recreate unaffected cards.
        var live = pageLayout.Select(p => p.Id).ToHashSet();
        foreach (var id in organizerCards.Keys.Where(id => !live.Contains(id)).ToArray())
        {
            organizerCards[id].Motion?.Stop(); organizerBoard.Children.Remove(organizerCards[id].Root); organizerCards.Remove(id);
        }
        for (var i = 0; i < pageLayout.Count; i++)
        {
            var page = pageLayout[i];
            if (!organizerCards.TryGetValue(page.Id, out var card))
            {
                card = CreateOrganizerCard(page); organizerCards.Add(page.Id, card); organizerBoard.Children.Add(card.Root);
            }
            card.Page = page;
            if (card.Angle.Angle != page.Rotation) card.Angle.Angle = page.Rotation;
            UpdateOrganizerNumber(card, i);
            card.Frame.BorderBrush = organizerSources.Count > 1 ? OrganizerColor(page.Source) : Brush(222, 227, 235);
            var sourceName = page.Source < 0 ? "Halaman kosong A4" : Path.GetFileName(organizerSources[page.Source]);
            ToolTipService.SetToolTip(card.Root, sourceName);
        }
        chosenPages.Clear();
        SyncOrganizerGaps(); UpdateOrganizerSummary(); ArrangeOrganizerCards(pageLayout, true);
    }

    private OrganizerCard CreateOrganizerCard(OrganizerPage page)
    {
        var card = new OrganizerCard(page);
        card.Root.RenderTransform = card.Position; card.Picture.RenderTransform = card.Angle;
        var content = new Grid(); content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var sheet = new Grid();
        if (page.Source < 0) sheet.Children.Add(new Border { Width = 130, Height = 184, Background = Brush(255, 255, 255), BorderBrush = Brush(224, 227, 234), BorderThickness = new Thickness(1), RenderTransform = card.Angle, RenderTransformOrigin = new Point(.5, .5) });
        sheet.Children.Add(card.Picture); content.Children.Add(sheet); Grid.SetRow(card.Number, 1); content.Children.Add(card.Number);
        foreach (var operation in new[] { "rotate", "remove", "preview" })
        {
            var button = new Button { Content = operation switch { "rotate" => "↻", "remove" => "×", _ => "⤢" }, Width = 32, Height = 32, Padding = new Thickness(0), FontSize = 18, Background = Brush(255, 255, 255) };
            var label = operation switch { "rotate" => "Putar 90°", "remove" => "Hapus halaman dari hasil", _ => "Perbesar halaman" };
            AutomationProperties.SetName(button, label); ToolTipService.SetToolTip(button, label);
            button.Click += async (_, _) => { if (operation == "preview") await PreviewOrganizerCardAsync(card); else ActOnOrganizerCard(card.Page.Id, operation); };
            card.Actions.Children.Add(button);
        }
        content.Children.Add(card.Actions); card.Frame.Child = content; card.Root.Content = card.Frame;
        card.Root.PointerEntered += (_, _) => { card.Hover = true; RevealOrganizerActions(card); };
        card.Root.PointerExited += (_, _) => { card.Hover = false; RevealOrganizerActions(card); };
        card.Root.GotFocus += (_, _) => RevealOrganizerActions(card);
        card.Root.LostFocus += (_, _) => DispatcherQueue.TryEnqueue(() => RevealOrganizerActions(card));
        card.Root.KeyDown += async (_, e) =>
        {
            if (organizerHeld is not null) { if (e.Key == VirtualKey.Escape) { CancelOrganizerDrag(); e.Handled = true; } return; }
            var ctrl = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            if (e.Key == VirtualKey.Delete) { ActOnOrganizerCard(card.Page.Id, "remove"); e.Handled = true; }
            else if (ctrl && e.Key is VirtualKey.Left or VirtualKey.Right or VirtualKey.D)
            { ActOnOrganizerCard(card.Page.Id, e.Key == VirtualKey.D ? "duplicate" : e.Key == VirtualKey.Left ? "left" : "right"); e.Handled = true; }
            else if (e.Key == VirtualKey.Enter && ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot), card.Root)) { await PreviewOrganizerCardAsync(card); e.Handled = true; }
        };
        var menu = new MenuFlyout();
        foreach (var operation in new[] { "duplicate", "left", "right" })
        {
            var item = new MenuFlyoutItem { Text = operation == "duplicate" ? "Duplikat halaman" : operation == "left" ? "Pindah sebelumnya" : "Pindah berikutnya" };
            item.Click += (_, _) => ActOnOrganizerCard(card.Page.Id, operation); menu.Items.Add(item);
        }
        card.Root.ContextFlyout = menu; AttachOrganizerDrag(card);
        if (page.Source >= 0) _ = SetOrganizerThumbnailAsync(card);
        return card;
    }

    private void RevealOrganizerActions(OrganizerCard card)
    {
        var focused = false;
        if (root.XamlRoot is not null)
        {
            var focus = FocusManager.GetFocusedElement(root.XamlRoot) as Control;
            if (focus?.FocusState == FocusState.Keyboard)
                for (var node = focus as DependencyObject; node is not null; node = VisualTreeHelper.GetParent(node))
                    if (ReferenceEquals(node, card.Root)) { focused = true; break; }
        }
        var visible = organizerDrag is null && (card.Hover || focused);
        card.Actions.Opacity = visible ? 1 : 0; card.Actions.IsHitTestVisible = visible;
    }

    private async Task SetOrganizerThumbnailAsync(OrganizerCard card)
    {
        var key = (card.Page.Source, card.Page.Page);
        if (!organizerThumbnails.TryGetValue(key, out var task))
        {
            task = LoadOrganizerThumbnailAsync(key.Source, key.Page); organizerThumbnails.Add(key, task);
        }
        card.Picture.Source = await task;
        if (card.Page.Source >= 0 && card.Page.Source < organizerSources.Count)
        {
            var raster = previewRasters.GetValue(card.Picture, _ => new());
            raster.Path = organizerSources[card.Page.Source]; raster.Page = (uint)card.Page.Page; raster.Width = 240;
        }
    }

    private async Task<ImageSource?> LoadOrganizerThumbnailAsync(int source, int page)
    {
        // Cache pixels independently of IsLoaded. Reparenting a card must not restart rendering.
        await RenderGate.WaitAsync();
        try
        {
            var path = organizerSources[source];
            var (file, properties) = await OpenPreviewStorageAsync(path);
            if (properties.Size > 100 * 1024 * 1024) return null;
            if (!PreviewDocuments.TryGetValue(path, out var cached) || cached.Modified != properties.DateModified || cached.Size != properties.Size)
            {
                var document = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);
                if (PreviewDocuments.Count >= 2) PreviewDocuments.Clear();
                cached = (properties.DateModified, properties.Size, document); PreviewDocuments[path] = cached;
            }
            if (cached.Document.PageCount is 0 or > 1000 || page >= cached.Document.PageCount) return null;
            using var pdfPage = cached.Document.GetPage((uint)page);
            using var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            await pdfPage.RenderToStreamAsync(stream, new Windows.Data.Pdf.PdfPageRenderOptions { DestinationWidth = 240 });
            stream.Seek(0);
            var bitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(); await bitmap.SetSourceAsync(stream); return bitmap;
        }
        catch (Exception) { return null; }
        finally { RenderGate.Release(); }
    }

    private async Task PreviewOrganizerCardAsync(OrganizerCard card)
    {
        if (active is not null || importing || messageOpen || organizerHeld is not null) return;
        messageOpen = true;
        try
        {
            object content = card.Page.Source < 0 ? new Border { Width = 280, Height = 396, Background = Brush(255, 255, 255), BorderBrush = Brush(200, 207, 217), BorderThickness = new Thickness(1) }
                : PreviewPanel(organizerSources[card.Page.Source], (uint)card.Page.Page, card.Page.Rotation);
            await new ContentDialog { XamlRoot = root.XamlRoot, Title = "Halaman " + card.Number.Text, Content = content, CloseButtonText = "Tutup" }.ShowAsync();
        }
        catch (Exception) { status.Text = "Preview tidak tersedia."; }
        finally { messageOpen = false; }
    }

    private void ActOnOrganizerCard(long id, string operation)
    {
        if (active is not null || importing || organizerHeld is not null) return;
        var index = pageLayout.FindIndex(p => p.Id == id); if (index < 0) return;
        chosenPages.Clear(); chosenPages.Add(index); ChangeLayout(operation);
    }

    private void SyncOrganizerGaps()
    {
        while (organizerGaps.Count > pageLayout.Count + 1) { organizerBoard.Children.Remove(organizerGaps[^1]); organizerGaps.RemoveAt(organizerGaps.Count - 1); }
        while (organizerGaps.Count < pageLayout.Count + 1)
        {
            var gap = new Grid { Width = 28, Height = OrganizerCardHeight, Background = Brush(247, 248, 252) };
            var button = new Button { Content = "+", Width = 26, Height = 34, Padding = new Thickness(0), FontSize = 22, VerticalAlignment = VerticalAlignment.Center, Opacity = 0 };
            var hovering = false;
            button.Click += (_, _) => InsertOrganizerBlank(organizerGaps.IndexOf(gap));
            gap.PointerEntered += (_, _) => { hovering = true; button.Opacity = 1; };
            gap.PointerExited += (_, _) => { hovering = false; if (button.FocusState != FocusState.Keyboard) button.Opacity = 0; };
            button.GotFocus += (_, _) => button.Opacity = 1; button.LostFocus += (_, _) => button.Opacity = hovering ? 1 : 0;
            gap.Children.Add(button); organizerGaps.Add(gap); organizerBoard.Children.Add(gap);
        }
        for (var i = 0; i < organizerGaps.Count; i++)
        {
            ((Button)organizerGaps[i].Children[0]).TabIndex = i * 5;
            AutomationProperties.SetName((Button)organizerGaps[i].Children[0], i == 0 ? "Sisip halaman kosong di awal" : i == pageLayout.Count ? "Sisip halaman kosong di akhir" : $"Sisip halaman kosong sebelum halaman {i + 1}");
        }
    }

    private void UpdateOrganizerSummary()
    {
        UpdateAutomaticOutputName();
        layoutStatus.Text = $"{pageLayout.Count} halaman · Seret untuk mengatur urutan · Arahkan ke celah untuk menambah halaman kosong · Ctrl+Z: Undo";
        if (sourceSummary.Children.Count == 0) sourceSummary.Children.Add(Copy("File sumber", 18));
        while (organizerSourceLabels.Count < organizerSources.Count)
        {
            var i = organizerSourceLabels.Count; var label = Copy("", 13); organizerSourceLabels.Add(label);
            var row = new Grid(); row.ColumnDefinitions.Add(new ColumnDefinition()); row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); row.Children.Add(label);
            var remove = new Button { Content = "Keluarkan dari daftar" }; Grid.SetColumn(remove, 1); row.Children.Add(remove);
            ToolTipService.SetToolTip(remove, "Keluarkan dokumen dari daftar");
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(remove, "Keluarkan dari daftar: " + Path.GetFileName(organizerSources[i]));
            remove.Click += (_, _) => ExcludeOrganizerSource(i);
            sourceSummary.Children.Add(new Border { Child = row, BorderBrush = OrganizerColor(i), BorderThickness = new Thickness(4, 0, 0, 0), Padding = new Thickness(8) });
        }
        for (var i = 0; i < organizerSourceLabels.Count; i++)
        {
            var count = pageLayout.Count(p => p.Source == i);
            organizerSourceLabels[i].Text = $"{Path.GetFileName(organizerSources[i])}\n{count} halaman";
            if (organizerSourceLabels[i].Parent is Grid row && row.Parent is Border border) border.Visibility = count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        if (!sourceSummary.Children.Contains(organizerBlankSummary)) sourceSummary.Children.Add(organizerBlankSummary);
        organizerBlankSummary.Text = $"{pageLayout.Count(p => p.Source < 0)} halaman kosong A4 · File asli tetap utuh.";
        run.IsEnabled = active is null && !importing && organizerHeld is null && pageLayout.Count > 0 && organizerSources.Count > 0;
    }

    private Point OrganizerSlot(int index) => new(28 + index % organizerColumns * OrganizerPitchX, 14 + index / organizerColumns * OrganizerPitchY);

    private void ArrangeOrganizerCards(IReadOnlyList<OrganizerPage> order, bool animate)
    {
        organizerColumns = Math.Max(1, (int)((Math.Max(240, pageGrid.ActualWidth) - 32) / OrganizerPitchX));
        organizerBoard.Width = Math.Max(240, pageGrid.ActualWidth - 18);
        organizerBoard.Height = Math.Max(OrganizerPitchY, Math.Ceiling(order.Count / (double)organizerColumns) * OrganizerPitchY + 18);
        for (var i = 0; i < order.Count; i++)
        {
            if (!organizerCards.TryGetValue(order[i].Id, out var card)) continue;
            card.Root.Width = OrganizerCardWidth * previewScale;
            card.Root.Height = OrganizerCardHeight * previewScale;
            card.Picture.Width = 154 * previewScale;
            card.Picture.Height = 194 * previewScale;
            if (organizerDrag?.MovingId == order[i].Id) continue;
            var slot = OrganizerSlot(i); MoveOrganizerVisual(card, slot, animate);
            if (organizerDrag is null) UpdateOrganizerNumber(card, i);
        }
        for (var i = 0; i < organizerGaps.Count; i++)
        {
            var slot = i == order.Count && i > 0 ? OrganizerSlot(i - 1) : OrganizerSlot(i);
            organizerGaps[i].Height = OrganizerCardHeight * previewScale;
            Canvas.SetLeft(organizerGaps[i], i == order.Count && i > 0 ? slot.X + OrganizerCardWidth * previewScale + 2 : slot.X - 28);
            Canvas.SetTop(organizerGaps[i], slot.Y); organizerGaps[i].Visibility = organizerDrag is null ? Visibility.Visible : Visibility.Collapsed;
        }
        organizerPlaceholder.Width = OrganizerCardWidth * previewScale;
        organizerPlaceholder.Height = OrganizerCardHeight * previewScale;
    }

    private void UpdateOrganizerNumber(OrganizerCard card, int index)
    {
        card.Root.TabIndex = index * 5 + 1;
        for (var i = 0; i < card.Actions.Children.Count; i++) ((Button)card.Actions.Children[i]).TabIndex = index * 5 + 2 + i;
        var number = (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (card.Number.Text == number) return;
        card.Number.Text = number;
        var name = card.Page.Source < 0 ? "Halaman kosong A4" : Path.GetFileName(organizerSources[card.Page.Source]);
        AutomationProperties.SetName(card.Root, $"Halaman {number}, {name}. Seret untuk pindah. Ctrl+panah: pindah, Ctrl+D: duplikat.");
    }

    private static void MoveOrganizerVisual(OrganizerCard card, Point target, bool animate)
    {
        if (card.X == target.X && card.Y == target.Y) return;
        var fromX = card.Position.X; var fromY = card.Position.Y; card.Motion?.Stop();
        var wasPlaced = !double.IsNaN(card.X); card.X = target.X; card.Y = target.Y;
        card.Position.X = target.X; card.Position.Y = target.Y;
        if (!animate || !wasPlaced) return;
        var storyboard = new Storyboard();
        foreach (var axis in new[] { ("X", fromX, target.X), ("Y", fromY, target.Y) })
        {
            var motion = new DoubleAnimation { From = axis.Item2, To = axis.Item3, Duration = new Duration(TimeSpan.FromMilliseconds(150)), EnableDependentAnimation = true, EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(motion, card.Position); Storyboard.SetTargetProperty(motion, axis.Item1); storyboard.Children.Add(motion);
        }
        card.Motion = storyboard; storyboard.Begin();
    }
}
