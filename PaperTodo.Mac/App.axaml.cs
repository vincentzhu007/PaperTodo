using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace PaperTodo.Mac;

public partial class App : Application
{
    private static string DataDirectory =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PaperTodo");

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var store = new StateStore(DataDirectory);
            var state = store.Load();

            var papers = state.Papers
                .Where(p => p.IsVisible && p.Type == PaperTypes.Todo)
                .ToArray();

            if (papers.Length == 0)
            {
                // No data yet: show a minimal blank paper instead of creating default data.
                var blank = new TodoPaperWindow(new PaperData { Title = "PaperTodo" });
                desktop.MainWindow = blank;
                blank.Show();
                RegisterScreenPlatform(blank);
            }
            else
            {
                Window first = null!;
                foreach (var paper in papers)
                {
                    var window = new TodoPaperWindow(paper);
                    window.Show();
                    first ??= window;
                }

                desktop.MainWindow = first;
                RegisterScreenPlatform(first);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void RegisterScreenPlatform(Window anyWindow)
    {
        // Avalonia's Screens are bound to a live window's display connection.
        ScreenPlatform.Current = new MacScreenPlatform(anyWindow.Screens);
    }
}
