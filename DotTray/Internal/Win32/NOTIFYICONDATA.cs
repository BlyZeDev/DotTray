namespace DotTray.Internal.Win32;

using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NOTIFYICONDATA
{
    public const int SZTIP_LENGTH = 128;
    public const int SZINFO_LENGTH = 256;
    public const int SZINFOTITLE_LENGTH = 64;

    public uint cbSize;
    public nint hWnd;
    public uint uID;
    public uint uFlags;
    public uint uCallbackMessage;
    public nint hIcon;
    public fixed char szTip[SZTIP_LENGTH];
    public uint dwState;
    public uint dwStateMask;
    public fixed char szInfo[SZINFO_LENGTH];
    public uint uTimeoutOrVersion;
    public fixed char szInfoTitle[SZINFOTITLE_LENGTH];
    public uint dwInfoFlags;
    public Guid guidItem;
    public nint hBalloonIcon;
}