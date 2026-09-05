namespace Pertamina.Oto.Desktop;
public enum ShellAction { SelectFiles, Run, Cancel }
public sealed record CommandSemantic(ShellAction Action, string Key, string AccessibleName, int FocusOrder);
public static class ShellSemantics
{
    public static IReadOnlyList<CommandSemantic> Commands { get; } = Array.AsReadOnly(new[]
    {
        new CommandSemantic(ShellAction.SelectFiles, "Ctrl+O", "Pilih berkas", 0),
        new CommandSemantic(ShellAction.Run, "Ctrl+Enter", "Gabungkan PDF", 1),
        new CommandSemantic(ShellAction.Cancel, "Escape", "Batalkan proses", 2)
    });
    public static bool IsEnabled(ShellAction action, ShellState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return action switch
        {
            ShellAction.Cancel => state.CanCancel,
            ShellAction.SelectFiles or ShellAction.Run => !state.IsIndeterminate,
            _ => false
        };
    }
    public static double Scale(double logicalSize, double scale)
    {
        if (!double.IsFinite(logicalSize) || logicalSize < 0) throw new ArgumentOutOfRangeException(nameof(logicalSize));
        if (!double.IsFinite(scale) || scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale));
        double result = logicalSize * scale;
        if (!double.IsFinite(result)) throw new ArgumentOutOfRangeException(nameof(scale));
        return result;
    }
    // Prototype globals.css reference. Runtime/visual accessibility is still gated.
    public const string Background = "#f6f8fb";
    public const string Foreground = "#0f172a";
    public const string Primary = "#1768d4";
}
