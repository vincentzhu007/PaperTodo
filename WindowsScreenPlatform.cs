using System.Windows;

namespace PaperTodo;

// Windows implementation of the core screen platform seam: Win32 monitor enumeration,
// device work areas and the system work area. Registered by App.OnStartup.
internal sealed class WindowsScreenPlatform : IScreenPlatform
{
    public string? PrimaryMonitorDeviceName => WindowWorkAreaHelper.PrimaryMonitorDeviceName();

    public DipRect PrimaryWorkArea => SystemParameters.WorkArea.ToDipRect();

    public DipRect? WorkAreaForDevice(string normalizedDeviceName) =>
        WindowWorkAreaHelper.WorkAreaForDevice(normalizedDeviceName)?.ToDipRect();

    public bool TryGetMonitorGeometryForDevice(string? deviceName, out MonitorGeometry geometry) =>
        WindowWorkAreaHelper.TryGetMonitorGeometryForDevice(deviceName, out geometry);
}
