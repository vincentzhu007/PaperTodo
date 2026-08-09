namespace PaperTodo;

// Platform-neutral rectangle in the same logical coordinate space as WPF's Rect /
// Avalonia's Rect: X/Y is the top-left origin, sizes are DIPs. Windows callers convert
// to WPF Rect at the boundary via ScreenGeometryWpf.ToWpfRect; the mac shell maps from
// its own Rect natively.
public readonly record struct DipRect(double X, double Y, double Width, double Height)
{
    public double Left => X;
    public double Top => Y;
    public double Right => X + Width;
    public double Bottom => Y + Height;

    public static DipRect FromLeftTop(double left, double top, double width, double height) =>
        new(left, top, width, height);
}
