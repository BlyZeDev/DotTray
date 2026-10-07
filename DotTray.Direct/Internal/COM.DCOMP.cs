namespace DotTray.Direct.Internal;

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal static partial class COM
{
    [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
    [Guid("c37ea93a-e7aa-450d-b16f-9746cb0407f3")]
    internal partial interface IDCompositionDevice
    {
        void Commit();
        void WaitForCommitCompletion_Unbound();
        void GetFrameStatistics_Unbound();
        void CreateTargetForHwnd(nint hwnd, int topmost, out IDCompositionTarget target);
        void CreateVisual(out IDCompositionVisual visual);
    }

    [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
    [Guid("eacdd04c-117e-4e17-88f4-d1b12b0e3d89")]
    internal partial interface IDCompositionTarget
    {
        void SetRoot(IDCompositionVisual visual);
    }

    [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
    [Guid("4d93059d-097b-4651-9a60-f0f25116e2f3")]
    internal partial interface IDCompositionVisual
    {
        void SetOffsetXAnimation_Unbound();
        void SetOffsetX_Unbound();
        void SetOffsetYAnimation_Unbound();
        void SetOffsetY_Unbound();
        void SetTransformObject_Unbound();
        void SetTransformMatrix_Unbound();
        void SetTransformParent_Unbound();
        void SetEffect_Unbound();
        void SetBitmapInterpolationMode_Unbound();
        void SetBorderMode_Unbound();
        void SetClipObject_Unbound();
        void SetClipRect_Unbound();
        void SetContent(IDXGISwapChain1 content);
    }
}