namespace DotTray.Internal.Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct DRAWITEMSTRUCT
{
    public uint CtlType;
    public uint CtlID;
    public uint itemID;
    public uint itemAction;
    public uint itemState;
    public nint hwndItem;
    public nint hDC;
    public RECT rcItem;
    public nint itemData;
}