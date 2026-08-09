namespace PaperTodo;

// Cross-platform font scale factor. The Windows shell keeps its WPF font families /
// weights in AppTypography and delegates the numeric scaling here; the mac shell calls
// this directly. Configure the factor once at startup via SetScale (default 1.0).
public static class Typography
{
    private static double _scale = 1.0;

    public static double ScaleFactor => _scale;

    public static void SetScale(double scale) => _scale = OverallFontScales.Normalize(scale);

    public static double Scale(double fontSize)
    {
        return Math.Round(fontSize * _scale, 1, MidpointRounding.AwayFromZero);
    }

    public static double FitChrome(double normalSize)
    {
        return _scale <= 1.0
            ? normalSize
            : Math.Ceiling(normalSize * _scale);
    }
}
