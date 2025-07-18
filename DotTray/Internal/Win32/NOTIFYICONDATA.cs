namespace DotTray.Internal.Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal unsafe struct NOTIFYICONDATA
{
    public const int SZTIP_BYTE_SIZE = 128;
    public const int SZINFO_BYTE_SIZE = 256;
    public const int SZINFOTITLE_BYTE_SIZE = 64;

    public uint cbSize;
    public nint hWnd;
    public uint uID;
    public uint uFlags;
    public uint uCallbackMessage;
    public nint hIcon;
    public fixed byte szTip[SZTIP_BYTE_SIZE];
    public uint dwState;
    public uint dwStateMask;
    public fixed byte szInfo[SZINFO_BYTE_SIZE];
    public uint uTimeoutOrVersion;
    public fixed byte szInfoTitle[SZINFOTITLE_BYTE_SIZE];
    public uint dwInfoFlags;
}