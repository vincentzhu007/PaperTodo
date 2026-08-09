using System.Windows;

namespace PaperTodo;

// WPF <-> core geometry glue. Core types (DeviceScreenPoint / GlobalScreenDipPoint / DipRect)
// stay platform-neutral; every WPF Point/Rect crossing goes through here so the conversions
// are visible and the mac shell provides its own equivalents.
internal static class ScreenGeometryWpf
{
    public static DeviceScreenPoint FromPoint(Point point) => new(point.X, point.Y);

    public static Point ToPoint(this DeviceScreenPoint point) => new(point.X, point.Y);

    public static Point ToPoint(this GlobalScreenDipPoint point) => new(point.X, point.Y);

    public static DipRect ToDipRect(this Rect rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

    public static Rect ToWpfRect(this DipRect rect) => new(rect.X, rect.Y, rect.Width, rect.Height);
}
