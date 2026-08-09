using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PaperTodo.Mac;

// Skeleton todo paper: frameless floating window showing one todo paper's items.
// Edge capsules, hide/collapse semantics, saving and multi-monitor identity arrive in
// later increments; this establishes the paper window form on macOS.
public partial class TodoPaperWindow : Window
{
    private readonly PaperData _paper;

    public event Action? CollapseRequested;

    public TodoPaperWindow(PaperData paper)
    {
        InitializeComponent();
        _paper = paper;
        TitleText.Text = paper.Title;
        ItemsList.ItemsSource = paper.Items;
        Position = new Avalonia.PixelPoint((int)paper.X, (int)paper.Y);
        Width = paper.Width;
        Height = paper.Height;

        Opened += (_, _) => ApplyWindowBehaviors();
        PointerPressed += OnPointerPressedForDrag;
        CollapseButton.Click += (_, _) => CollapseRequested?.Invoke();
    }

    public void UpdateCollapseButton(bool canCollapse)
    {
        CollapseButton.IsVisible = canCollapse;
    }

    private void ApplyWindowBehaviors()
    {
        if (TryGetPlatformHandle() is { HandleDescriptor: "NSWindow" } handle)
        {
            MacWindowInterop.SetLevel(handle.Handle, MacWindowInterop.NSFloatingWindowLevel);
            MacWindowInterop.SetCollectionBehavior(handle.Handle, MacWindowInterop.PaperBehavior);
        }
    }

    private void OnPointerPressedForDrag(object? sender, PointerPressedEventArgs e)
    {
        // Do not hijack presses on the collapse button (or any other Button).
        if (e.Source is Button)
        {
            return;
        }

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }
}
