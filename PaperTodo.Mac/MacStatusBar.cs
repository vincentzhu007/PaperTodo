using System;
using System.Collections.Generic;

namespace PaperTodo.Mac;

/// <summary>
/// Status-bar item + menu (the macOS counterpart of the Windows tray, ADR 0004).
/// Menu actions are NSMenuItem selectors routed to C# delegates via ObjCRuntime.
/// </summary>
internal static class MacStatusBar
{
    private static IntPtr _statusItem;
    private static IntPtr _target; // ObjC targets are not retained by NSMenuItem; keep alive.

    /// <summary>
    /// Installs the status item. <paramref name="menuItems"/> maps menu titles to selector names
    /// (each selector must have been registered on the action target by the caller).
    /// </summary>
    public static void Install(IReadOnlyDictionary<string, string> menuItems, Action<string> handler)
    {
        var actions = new Dictionary<string, Action>(StringComparer.Ordinal);
        foreach (var pair in menuItems)
        {
            actions[pair.Value] = () => handler(pair.Value);
        }

        _target = ObjCRuntime.CreateActionTarget(actions);

        var statusBar = ObjCRuntime.Send(ObjCRuntime.Cls("NSStatusBar"), ObjCRuntime.Sel("systemStatusBar"));
        _statusItem = ObjCRuntime.Send(statusBar, ObjCRuntime.Sel("statusItemWithLength:"), -1.0); // NSVariableStatusItemLength

        var button = ObjCRuntime.Send(_statusItem, ObjCRuntime.Sel("button"));
        SetSymbolImage(button, "checklist");

        var menu = ObjCRuntime.Send(ObjCRuntime.Cls("NSMenu"), ObjCRuntime.Sel("alloc"));
        menu = ObjCRuntime.Send(menu, ObjCRuntime.Sel("init"));
        foreach (var pair in menuItems)
        {
            var itemTitle = ObjCRuntime.CreateNSString(pair.Key);
            var emptyKey = ObjCRuntime.CreateNSString("");
            try
            {
                var item = ObjCRuntime.Send(
                    menu,
                    ObjCRuntime.Sel("addItemWithTitle:action:keyEquivalent:"),
                    itemTitle,
                    ObjCRuntime.Sel(pair.Value),
                    emptyKey);
                // The menu retains both the item and a copy of its title; only our +1 title
                // needs releasing. The item itself is owned by the menu.
                ObjCRuntime.SendVoid(item, ObjCRuntime.Sel("setTarget:"), _target);
            }
            finally
            {
                ObjCRuntime.Release(itemTitle);
                ObjCRuntime.Release(emptyKey);
            }
        }

        ObjCRuntime.SendVoid(_statusItem, ObjCRuntime.Sel("setMenu:"), menu);
    }

    private static void SetSymbolImage(IntPtr button, string symbolName)
    {
        var name = ObjCRuntime.CreateNSString(symbolName);
        var description = ObjCRuntime.CreateNSString("PaperTodo");
        try
        {
            // SF Symbols are template images; the status bar button renders them natively.
            var image = ObjCRuntime.Send(
                ObjCRuntime.Cls("NSImage"),
                ObjCRuntime.Sel("imageWithSystemSymbolName:accessibilityDescription:"),
                name,
                description);
            ObjCRuntime.SendVoid(button, ObjCRuntime.Sel("setImage:"), image);
        }
        finally
        {
            ObjCRuntime.Release(name);
            ObjCRuntime.Release(description);
        }
    }
}
