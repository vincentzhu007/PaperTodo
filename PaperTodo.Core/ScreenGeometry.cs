namespace PaperTodo;

// WPF exposes both physical PointToScreen pixels and app-wide/system-DPI coordinates as a bare
// Point. These wrappers make crossing the Win32/work-area boundary explicit and prevent mixed-DPI
// math from silently combining the two spaces. WPF Point conversion glue lives in the Windows
// shell (ScreenGeometryWpf), not in core.
public readonly record struct DeviceScreenPoint(double X, double Y);

public readonly record struct DeviceScreenRect(int Left, int Top, int Right, int Bottom)
{
    public int Width => Math.Max(0, Right - Left);
    public int Height => Math.Max(0, Bottom - Top);
    public bool IsEmpty => Width == 0 || Height == 0;

    public DeviceScreenRect WithVerticalEdges(int top, int bottom) =>
        new(Left, top, Right, bottom);
}

public readonly record struct GlobalScreenDipPoint(double X, double Y);

public readonly record struct MonitorGeometry(
    string DeviceName,
    DeviceScreenRect WorkArea,
    double DpiScaleX,
    double DpiScaleY)
{
    public DipRect LocalWorkAreaDip => new(
        0,
        0,
        WorkArea.Width / DpiScaleX,
        WorkArea.Height / DpiScaleY);

    public double DeviceYToLocalDip(double deviceY) => (deviceY - WorkArea.Top) / DpiScaleY;
}
