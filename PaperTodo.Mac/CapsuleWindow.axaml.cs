using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace PaperTodo.Mac;

// Collapsed-capsule form of a paper (Windows semantics mirrored): click the body to expand,
// drag the body to move, the × hides the paper (not deletes). Single-click vs drag is
// discriminated by a movement threshold, like the Windows capsule pointer state machine.
public partial class CapsuleWindow : Window
{
    private const double DragThreshold = 4;

    private readonly PaperData _paper;
    private PointerPressedEventArgs? _pressArgs;
    private Point _pressPoint;
    private bool _dragStarted;

    public event Action? ExpandRequested;
    public event Action? HideRequested;

    public CapsuleWindow(PaperData paper)
    {
        InitializeComponent();
        _paper = paper;
        IconText.Text = "☑";
        TitleText.Text = paper.Title;
        Position = new PixelPoint((int)paper.X, (int)paper.Y);

        Opened += (_, _) => ApplyWindowBehaviors();

        Body.PointerPressed += OnBodyPointerPressed;
        Body.PointerMoved += OnBodyPointerMoved;
        Body.PointerReleased += OnBodyPointerReleased;
        CloseButton.Click += (_, _) => HideRequested?.Invoke();
    }

    private void ApplyWindowBehaviors()
    {
        if (TryGetPlatformHandle() is { HandleDescriptor: "NSWindow" } handle)
        {
            MacWindowInterop.SetLevel(handle.Handle, MacWindowInterop.NSFloatingWindowLevel);
            MacWindowInterop.SetCollectionBehavior(handle.Handle, MacWindowInterop.PaperBehavior);
        }
    }

    private void OnBodyPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _pressPoint = e.GetPosition(this);
        _pressArgs = e;
        _dragStarted = false;
        e.Handled = true;
    }

    private void OnBodyPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragStarted || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var current = e.GetPosition(this);
        if (Math.Abs(current.X - _pressPoint.X) > DragThreshold ||
            Math.Abs(current.Y - _pressPoint.Y) > DragThreshold)
        {
            _dragStarted = true;
            if (_pressArgs is not null)
            {
                BeginMoveDrag(_pressArgs);
            }

            e.Handled = true;
        }
    }

    private void OnBodyPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_dragStarted)
        {
            ExpandRequested?.Invoke();
        }

        _dragStarted = false;
    }
}
