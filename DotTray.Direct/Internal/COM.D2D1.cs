namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.Models;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal static partial class COM
{
    [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
    [Guid("2cd90691-12e2-11dc-9fed-001143a055f9")]
    internal partial interface ID2D1Resource
    {
        [PreserveSig]
        void GetFactory(out nint factory);
    }

    [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
    [Guid("2cd906a8-12e2-11dc-9fed-001143a055f9")]
    internal partial interface ID2D1Brush : ID2D1Resource
    {
        [PreserveSig]
        void SetOpacity(float opacity);
        [PreserveSig]
        void SetTransform(in Matrix3x2 transform);
        [PreserveSig]
        float GetOpacity();
        void GetTransform_Unbound();
    }

    [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
    [Guid("2cd906a9-12e2-11dc-9fed-001143a055f9")]
    internal partial interface ID2D1SolidColorBrush : ID2D1Brush
    {
        [PreserveSig]
        void SetColor(in ColorF color);
        void GetColor_Unbound();
    }

    [GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16, Options = ComInterfaceOptions.ComObjectWrapper)]
    [Guid("2cd90694-12e2-11dc-9fed-001143a055f9")]
    internal partial interface ID2D1RenderTarget : ID2D1Resource
    {
        void CreateBitmap_Unbound();
        void CreateBitmapFromWicBitmap_Unbound();
        void CreateSharedBitmap_Unbound();
        void CreateBitmapBrush_Unbound();
        void CreateSolidColorBrush(in ColorF color, nint brushProperties, out ID2D1SolidColorBrush brush);
        void CreateGradientStopCollection_Unbound();
        void CreateLinearGradientBrush_Unbound();
        void CreateRadialGradientBrush_Unbound();
        void CreateCompatibleRenderTarget_Unbound();
        void CreateLayer_Unbound();
        void CreateMesh_Unbound();

        [PreserveSig]
        void DrawLine(PointF p0, PointF p1, ID2D1Brush brush, float strokeWidth, nint strokeStyle);
        [PreserveSig]
        void DrawRectangle(in RectF rect, ID2D1Brush brush, float strokeWidth, nint strokeStyle);
        [PreserveSig]
        void FillRectangle(in RectF rect, ID2D1Brush brush);
        [PreserveSig]
        void DrawRoundedRectangle(in RoundedRectF rect, ID2D1Brush brush, float strokeWidth, nint strokeStyle);
        [PreserveSig]
        void FillRoundedRectangle(in RoundedRectF rect, ID2D1Brush brush);
        [PreserveSig]
        void DrawEllipse(in EllipseF ellipse, ID2D1Brush brush, float strokeWidth, nint strokeStyle);
        [PreserveSig]
        void FillEllipse(in EllipseF ellipse, ID2D1Brush brush);

        void DrawGeometry_Unbound();
        void FillGeometry_Unbound();
        void FillMesh_Unbound();
        void FillOpacityMask_Unbound();
        void DrawBitmap_Unbound();

        [PreserveSig]
        void DrawText(string text, uint length, IDWriteTextFormat format, in RectF layoutRect, ID2D1Brush brush, uint options, uint measuringMode);
        [PreserveSig]
        void DrawTextLayout(PointF origin, IDWriteTextLayout layout, ID2D1Brush brush, uint options);

        void DrawGlyphRun_Unbound();
        [PreserveSig]
        void SetTransform(in Matrix3x2 transform);
        void GetTransform_Unbound();
        [PreserveSig]
        void SetAntialiasMode(uint mode);
        void GetAntialiasMode_Unbound();
        [PreserveSig]
        void SetTextAntialiasMode(uint mode);
        void GetTextAntialiasMode_Unbound();
        void SetTextRenderingParams_Unbound();
        void GetTextRenderingParams_Unbound();
        void SetTags_Unbound();
        void GetTags_Unbound();
        void PushLayer_Unbound();
        void PopLayer_Unbound();
        void Flush_Unbound();
        void SaveDrawingState_Unbound();
        void RestoreDrawingState_Unbound();
        [PreserveSig]
        void PushAxisAlignedClip(in RectF clipRect, uint antialiasMode);
        [PreserveSig]
        void PopAxisAlignedClip();
        [PreserveSig]
        void Clear(in ColorF clearColor);
        [PreserveSig]
        void BeginDraw();
        [PreserveSig]
        int EndDraw(nint tag1, nint tag2);
        void GetPixelFormat_Unbound();
        [PreserveSig]
        void SetDpi(float dpiX, float dpiY);
        [PreserveSig]
        void GetDpi(out float dpiX, out float dpiY);
        void GetSize_Unbound();
        void GetPixelSize_Unbound();
        void GetMaximumBitmapSize_Unbound();
        void IsSupported_Unbound();
    }

    [GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16, Options = ComInterfaceOptions.ComObjectWrapper)]
    [Guid("e8f7fe7a-191c-466d-ad95-975678bda998")]
    internal partial interface ID2D1DeviceContext : ID2D1RenderTarget
    {
        void CreateBitmapDc_Unbound();
        void CreateBitmapFromWicBitmapDc_Unbound();
        void CreateColorContext_Unbound();
        void CreateColorContextFromFilename_Unbound();
        void CreateColorContextFromWicColorContext_Unbound();

        void CreateBitmapFromDxgiSurface(nint surface, in BitmapProperties1 properties, out nint bitmap);

        void CreateEffect_Unbound();
        void CreateGradientStopCollectionDc_Unbound();
        void CreateImageBrush_Unbound();
        void CreateBitmapBrushDc_Unbound();
        void CreateCommandList_Unbound();
        void IsDxgiFormatSupported_Unbound();
        void IsBufferPrecisionSupported_Unbound();
        void GetImageLocalBounds_Unbound();
        void GetImageWorldBounds_Unbound();
        void GetGlyphRunWorldBounds_Unbound();
        void GetDevice_Unbound();

        [PreserveSig]
        void SetTarget(nint image);
    }

    [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
    [Guid("47dd575d-ac05-4cdd-8049-9b02cd16f44c")]
    internal partial interface ID2D1Device : ID2D1Resource
    {
        void CreateDeviceContext(uint options, out ID2D1DeviceContext deviceContext);
    }
}