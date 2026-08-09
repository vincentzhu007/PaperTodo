namespace PaperTodo;

/// <summary>
/// Platform seam for global-shortcut state normalization. The shortcut catalog is bound to
/// the shell's input model (Windows: WPF Key/ModifierKeys + Win32 registration), so core only
/// declares the hook. Windows registers GlobalShortcutCatalog at startup; the mac shell leaves
/// the identity defaults (persisted hotkey strings pass through untouched — lossless for data
/// it does not interpret).
/// </summary>
public static class GlobalShortcutState
{
    public static Func<Dictionary<string, string>?, Dictionary<string, string>> NormalizeBindings { get; set; } =
        source => source ?? new Dictionary<string, string>(StringComparer.Ordinal);

    public static Func<Dictionary<string, bool>?, Dictionary<string, bool>> NormalizeEnabled { get; set; } =
        source => source ?? new Dictionary<string, bool>();
}
