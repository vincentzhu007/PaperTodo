using Avalonia;
using Avalonia.Media;

namespace PaperTodo.Mac;

// macOS implementation of the core screen platform seam (ADR 0005). Avalonia's Screens expose
// working areas that already exclude the menu bar and Dock (visibleFrame semantics), so the mac
// shell supplies those directly. Monitor device names are a Windows concept: v1 returns no
// primary name and falls back to the primary work area for unknown devices.
internal sealed class MacScreenPlatform : IScreenPlatform
{
    private readonly Avalonia.Controls.Screens? _screens;

    public MacScreenPlatform(Avalonia.Controls.Screens? screens) => _screens = screens;

    public string? PrimaryMonitorDeviceName => null;

    public DipRect PrimaryWorkArea => ToDipRect(_screens?.Primary?.WorkingArea, _screens?.Primary?.Scaling ?? 1);

    public DipRect? WorkAreaForDevice(string normalizedDeviceName) => null;

    public bool TryGetMonitorGeometryForDevice(string? deviceName, out MonitorGeometry geometry)
    {
        geometry = default;
        return false;
    }

    private static DipRect ToDipRect(Avalonia.PixelRect? area, double scale)
    {
        if (area is { } a && scale > 0)
        {
            return new DipRect(a.X / scale, a.Y / scale, a.Width / scale, a.Height / scale);
        }

        return new DipRect(0, 0, 0, 0);
    }
}
