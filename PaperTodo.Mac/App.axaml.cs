using System;
using System.Collections.Generic;
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

    private readonly List<Window> _paperWindows = new();
    private IClassicDesktopStyleApplicationLifetime? _desktop;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
            // Agent app (ADR 0004): no Dock icon, no main window; only the status item.
            // The app stays alive until the menu's quit action calls Shutdown().
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var store = new StateStore(DataDirectory);
            var state = store.Load();

            var papers = state.Papers
                .Where(p => p.IsVisible && p.Type == PaperTypes.Todo)
                .ToArray();

            foreach (var paper in papers)
            {
                var window = new TodoPaperWindow(paper);
                _paperWindows.Add(window);
                window.Show();
            }

            RegisterScreenPlatform(_paperWindows.FirstOrDefault());

            InstallStatusBar();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void RegisterScreenPlatform(Window? anyWindow)
    {
        // Avalonia's Screens are bound to a live window's display connection.
        ScreenPlatform.Current = new MacScreenPlatform(anyWindow?.Screens);
    }

    private void InstallStatusBar()
    {
        // Avalonia's macOS backend forces the regular activation policy, which re-adds the
        // Dock icon even though Info.plist says LSUIElement. Re-assert the accessory policy
        // (no Dock icon, no menu bar) at runtime after the UI is up.
        var nsApp = ObjCRuntime.Send(ObjCRuntime.Cls("NSApplication"), ObjCRuntime.Sel("sharedApplication"));
        ObjCRuntime.SendVoid(nsApp, ObjCRuntime.Sel("setActivationPolicy:"), 1); // NSApplicationActivationPolicyAccessory

        MacStatusBar.Install(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["显示全部纸片"] = "showAllPapers:",
                ["退出 PaperTodo"] = "quit:"
            },
            HandleStatusAction);
    }

    private void HandleStatusAction(string selector)
    {
        switch (selector)
        {
            case "showAllPapers:":
                foreach (var window in _paperWindows)
                {
                    window.Show();
                }

                break;
            case "quit:":
                _desktop?.Shutdown();
                break;
        }
    }
}
