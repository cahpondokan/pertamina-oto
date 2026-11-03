namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Shared lane contract; Agent 0 integrates this file after all three handoffs.
    private sealed record TransitionDocument(string Path, string Name, string Detail,
        string SourceKey, int SourceIndex, string? Problem = null);
    private sealed record TransitionAction(string Id, string Title, string Description,
        bool Destructive = false);
    private sealed record FeatureTransitionOrigin(string? ToolId, string Label,
        string[] Files, bool HasPendingChanges, string? SavedResult);
}
