namespace DotTray.Internal.Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct DELETEITEMSTRUCT
{
    public uint CtlType;
    public uint CtlID;
    public uint itemID;
    public nint hwndItem;
    public nint itemData;
}