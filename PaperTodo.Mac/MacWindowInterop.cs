using System;
using System.Runtime.InteropServices;

namespace PaperTodo.Mac;

/// <summary>
/// Minimal NSWindow interop (libobjc). Supplies the capsule platform behaviors validated in the
/// spike: floating window level and "all spaces" collection behavior. v1 uses level 3
/// (floating above normal windows) + canJoinAllSpaces|stationary|ignoresCycle for papers too;
/// edge capsules will reuse the same primitives.
/// </summary>
internal static class MacWindowInterop
{
    private const string LibObjc = "/usr/lib/libobjc.dylib";

    [DllImport(LibObjc, EntryPoint = "sel_registerName")]
    private static extern IntPtr RegisterSelector(string name);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern void SendVoidLong(IntPtr receiver, IntPtr selector, long value);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern long SendLong(IntPtr receiver, IntPtr selector);

    private static readonly IntPtr SelSetLevel = RegisterSelector("setLevel:");
    private static readonly IntPtr SelLevel = RegisterSelector("level");
    private static readonly IntPtr SelSetCollectionBehavior = RegisterSelector("setCollectionBehavior:");
    private static readonly IntPtr SelCollectionBehavior = RegisterSelector("collectionBehavior");

    public const long NSNormalWindowLevel = 0;
    public const long NSFloatingWindowLevel = 3;

    public const ulong CanJoinAllSpaces = 1 << 0;
    public const ulong Stationary = 1 << 4;
    public const ulong IgnoresCycle = 1 << 6;
    public const ulong PaperBehavior = CanJoinAllSpaces | Stationary | IgnoresCycle;

    public static long GetLevel(IntPtr nsWindow) => SendLong(nsWindow, SelLevel);

    public static void SetLevel(IntPtr nsWindow, long level) => SendVoidLong(nsWindow, SelSetLevel, level);

    public static ulong GetCollectionBehavior(IntPtr nsWindow) => (ulong)SendLong(nsWindow, SelCollectionBehavior);

    public static void SetCollectionBehavior(IntPtr nsWindow, ulong behavior) =>
        SendVoidLong(nsWindow, SelSetCollectionBehavior, (long)behavior);
}
