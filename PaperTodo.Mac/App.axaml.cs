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

    // One session per paper: the expanded window and (lazily created) collapsed capsule.
    private readonly Dictionary<string, PaperSession> _sessions = new(StringComparer.Ordinal);
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

            foreach (var paper in state.Papers.Where(p => p.Type == PaperTypes.Todo))
            {
                var session = new PaperSession(paper);
                _sessions[paper.Id] = session;
                session.ShowInitial();
            }

            RegisterScreenPlatform(_sessions.Values.FirstOrDefault()?.PaperWindow);

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
                foreach (var session in _sessions.Values)
                {
                    session.ShowAll();
                }

                break;
            case "quit:":
                _desktop?.Shutdown();
                break;
        }
    }

    /// <summary>Owns the two window forms of one paper (expanded window + capsule).</summary>
    private sealed class PaperSession
    {
        private readonly PaperData _paper;
        private CapsuleWindow? _capsule;

        public PaperSession(PaperData paper)
        {
            _paper = paper;
            PaperWindow = new TodoPaperWindow(paper);
            PaperWindow.CollapseRequested += Collapse;
        }

        public TodoPaperWindow PaperWindow { get; }

        public void ShowInitial()
        {
            if (_paper.IsVisible && !_paper.IsCollapsed)
            {
                PaperWindow.Show();
            }
            else if (_paper.IsVisible)
            {
                GetCapsule().Show();
            }
        }

        public void ShowAll()
        {
            // "Show everything" restores hidden papers too (hide keeps the paper; it is not a
            // delete). Collapsed papers come back as capsules.
            _paper.IsVisible = true;
            if (_paper.IsCollapsed)
            {
                GetCapsule().Show();
            }
            else
            {
                PaperWindow.Show();
            }
        }

        private void Collapse()
        {
            _paper.IsCollapsed = true;
            PaperWindow.Hide();
            var capsule = GetCapsule();
            capsule.Position = PaperWindow.Position;
            capsule.Show();
        }

        private void Expand()
        {
            _paper.IsCollapsed = false;
            _capsule?.Hide();
            PaperWindow.Show();
        }

        private void HideAll()
        {
            _paper.IsVisible = false;
            _capsule?.Hide();
            PaperWindow.Hide();
        }

        private CapsuleWindow GetCapsule()
        {
            if (_capsule is null)
            {
                _capsule = new CapsuleWindow(_paper);
                _capsule.ExpandRequested += Expand;
                _capsule.HideRequested += HideAll;
            }

            return _capsule;
        }
    }
}
