namespace PaperTodo;

/// <summary>
/// Platform seam (ADR 0005): the shell registers the concrete screen/work-area provider.
/// Core layout math and persisted-state normalization never call platform APIs directly.
/// </summary>
public interface IScreenPlatform
{
    string? PrimaryMonitorDeviceName { get; }

    /// <summary>Primary display's work area in DIPs (menu bar / Dock excluded).</summary>
    DipRect PrimaryWorkArea { get; }

    /// <summary>Work area of a normalized device name in DIPs, or null when unknown.</summary>
    DipRect? WorkAreaForDevice(string normalizedDeviceName);

    bool TryGetMonitorGeometryForDevice(string? deviceName, out MonitorGeometry geometry);
}

/// <summary>Registration point for <see cref="IScreenPlatform"/>. Shell sets <see cref="Current"/>
/// at startup before any state load or capsule layout runs.</summary>
public static class ScreenPlatform
{
    public static IScreenPlatform? Current { get; set; }

    /// <summary>
    /// Trim + map the primary monitor's device name to "" (empty means primary).
    /// Keeps the persisted queue-key / capsule-monitor convention stable across shells.
    /// </summary>
    public static string NormalizeQueueMonitorDeviceName(string? deviceName)
    {
        var value = (deviceName ?? "").Trim();
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        var primary = Current?.PrimaryMonitorDeviceName;
        return !string.IsNullOrEmpty(primary) && string.Equals(value, primary, StringComparison.Ordinal)
            ? ""
            : value;
    }
}
