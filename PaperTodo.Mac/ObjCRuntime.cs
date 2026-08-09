using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PaperTodo.Mac;

/// <summary>
/// Raw libobjc helpers for the mac shell: selector/class lookup, objc_msgSend variants,
/// NSString creation, and an ObjC action target (a dynamically registered class whose
/// methods route back into C# delegates — used by status-bar menu items).
/// </summary>
internal static class ObjCRuntime
{
    private const string LibObjc = "/usr/lib/libobjc.dylib";

    [DllImport(LibObjc, EntryPoint = "sel_registerName")]
    private static extern IntPtr SelRegister(string name);

    [DllImport(LibObjc, EntryPoint = "sel_getName")]
    private static extern IntPtr SelGetName(IntPtr selector);

    [DllImport(LibObjc, EntryPoint = "objc_getClass")]
    private static extern IntPtr ClassGet(string name);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr MsgSendId(IntPtr receiver, IntPtr selector);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr MsgSendIdId(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr MsgSendIdIdId(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr MsgSendIdIdSelId(IntPtr receiver, IntPtr selector, IntPtr title, IntPtr action, IntPtr key);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr MsgSendIdDouble(IntPtr receiver, IntPtr selector, double arg);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern void MsgSendVoidId(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern void MsgSendVoid(IntPtr receiver, IntPtr selector);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern void MsgSendVoidLong(IntPtr receiver, IntPtr selector, long arg);

    [DllImport(LibObjc, EntryPoint = "objc_allocateClassPair")]
    private static extern IntPtr ClassAllocate(IntPtr superclass, string name, UIntPtr extraBytes);

    [DllImport(LibObjc, EntryPoint = "objc_registerClassPair")]
    private static extern void ClassRegister(IntPtr cls);

    [DllImport(LibObjc, EntryPoint = "class_addMethod")]
    private static extern int ClassAddMethod(IntPtr cls, IntPtr selector, IntPtr imp, string types);

    [DllImport(LibObjc, EntryPoint = "class_createInstance")]
    private static extern IntPtr ClassCreateInstance(IntPtr cls, UIntPtr extraBytes);

    public static IntPtr Sel(string name) => SelRegister(name);

    public static string SelName(IntPtr selector)
    {
        var ptr = SelGetName(selector);
        return ptr == IntPtr.Zero ? "" : Marshal.PtrToStringAnsi(ptr) ?? "";
    }

    public static IntPtr Cls(string name) => ClassGet(name);

    public static IntPtr Send(IntPtr receiver, IntPtr selector) => MsgSendId(receiver, selector);

    public static IntPtr Send(IntPtr receiver, IntPtr selector, IntPtr arg) => MsgSendIdId(receiver, selector, arg);

    public static IntPtr Send(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2) =>
        MsgSendIdIdId(receiver, selector, arg1, arg2);

    public static IntPtr Send(IntPtr receiver, IntPtr selector, double arg) => MsgSendIdDouble(receiver, selector, arg);

    public static IntPtr Send(IntPtr receiver, IntPtr selector, IntPtr title, IntPtr action, IntPtr key) =>
        MsgSendIdIdSelId(receiver, selector, title, action, key);

    public static void SendVoid(IntPtr receiver, IntPtr selector, IntPtr arg) =>
        MsgSendVoidId(receiver, selector, arg);

    public static void SendVoid(IntPtr receiver, IntPtr selector) =>
        MsgSendVoid(receiver, selector);

    public static void SendVoid(IntPtr receiver, IntPtr selector, long arg) =>
        MsgSendVoidLong(receiver, selector, arg);

    /// <summary>Creates an autoreleased-free NSString (caller owns the +1; release after use).</summary>
    public static IntPtr CreateNSString(string value)
    {
        var buffer = Marshal.StringToHGlobalAnsi(value);
        try
        {
            var alloc = Send(Cls("NSString"), Sel("alloc"));
            return Send(alloc, Sel("initWithUTF8String:"), buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public static void Release(IntPtr obj) => SendVoid(obj, Sel("release"));

    private delegate void ObjCActionCallback(IntPtr self, IntPtr cmd);

    private static readonly Dictionary<string, Action> ActionMap = new(StringComparer.Ordinal);
    private static readonly ObjCActionCallback Handler = OnAction;
    private static IntPtr _targetClass;
    private static IntPtr _targetInstance;

    /// <summary>
    /// Registers an ObjC class whose methods route by selector name into the given C# actions.
    /// Menu items use these selectors as their action; the shared instance stays alive forever.
    /// </summary>
    public static IntPtr CreateActionTarget(IReadOnlyDictionary<string, Action> actions)
    {
        if (_targetInstance != IntPtr.Zero)
        {
            return _targetInstance;
        }

        _targetClass = ClassAllocate(Cls("NSObject"), "PaperTodoActionTarget", UIntPtr.Zero);
        var imp = Marshal.GetFunctionPointerForDelegate(Handler);
        foreach (var pair in actions)
        {
            ActionMap[pair.Key] = pair.Value;
            ClassAddMethod(_targetClass, Sel(pair.Key), imp, "v@:");
        }

        ClassRegister(_targetClass);
        _targetInstance = ClassCreateInstance(_targetClass, UIntPtr.Zero);
        return _targetInstance;
    }

    private static void OnAction(IntPtr self, IntPtr cmd)
    {
        if (ActionMap.TryGetValue(SelName(cmd), out var action))
        {
            action();
        }
    }
}
