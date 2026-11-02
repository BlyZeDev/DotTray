namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.Models;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

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
internal unsafe partial interface ID2D1RenderTarget : ID2D1Resource
{
    void CreateBitmap_Unbound();
    void CreateBitmapFromWicBitmap_Unbound();
    void CreateSharedBitmap_Unbound();
    void CreateBitmapBrush_Unbound();
    void CreateSolidColorBrush(in ColorF color, nint brushProperties, out ID2D1SolidColorBrush brush);
    [PreserveSig]
    int CreateGradientStopCollection(GradientStopF* stops, uint stopsCount, uint gamma, uint extendMode, out nint collection);
    [PreserveSig]
    int CreateLinearGradientBrush(in LinearGradientBrushProperties properties, nint brushProperties, nint stopCollection, out ID2D1Brush brush);
    [PreserveSig]
    int CreateRadialGradientBrush(in RadialGradientBrushProperties properties, nint brushProperties, nint stopCollection, out ID2D1Brush brush);
    void CreateCompatibleRenderTarget_Unbound();
    void CreateLayer_Unbound();
    void CreateMesh_Unbound();

    [PreserveSig]
    void DrawLine(PointF p0, PointF p1, ID2D1Brush brush, float strokeWidth, nint strokeStyle);
    [PreserveSig]
    void DrawRectangle(in RECTF rect, ID2D1Brush brush, float strokeWidth, nint strokeStyle);
    [PreserveSig]
    void FillRectangle(in RECTF rect, ID2D1Brush brush);
    [PreserveSig]
    void DrawRoundedRectangle(in RoundedRectF rect, ID2D1Brush brush, float strokeWidth, nint strokeStyle);
    [PreserveSig]
    void FillRoundedRectangle(in RoundedRectF rect, ID2D1Brush brush);
    [PreserveSig]
    void DrawEllipse(in EllipseF ellipse, ID2D1Brush brush, float strokeWidth, nint strokeStyle);
    [PreserveSig]
    void FillEllipse(in EllipseF ellipse, ID2D1Brush brush);

    [PreserveSig]
    void DrawGeometry(ID2D1Geometry geometry, ID2D1Brush brush, float strokeWidth, ID2D1StrokeStyle? strokeStyle);
    void FillGeometry_Unbound();
    void FillMesh_Unbound();
    void FillOpacityMask_Unbound();
    void DrawBitmap_Unbound();

    [PreserveSig]
    void DrawText(string text, uint length, IDWriteTextFormat format, in RECTF layoutRect, ID2D1Brush brush, uint options, uint measuringMode);
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
    void PushAxisAlignedClip(in RECTF clipRect, uint antialiasMode);
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
internal unsafe partial interface ID2D1DeviceContext : ID2D1RenderTarget
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
    [PreserveSig]
    int GetGlyphRunWorldBounds(PointF baselineOrigin, nint glyphRun, uint measuringMode, out RECTF bounds);
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

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("06152247-6f50-465a-9245-118bfd3b6007")]
internal partial interface ID2D1Factory
{
    void ReloadSystemMetrics_Unbound();
    void GetDesktopDpi_Unbound();
    void CreateRectangleGeometry_Unbound();
    void CreateRoundedRectangleGeometry_Unbound();
    void CreateEllipseGeometry_Unbound();
    void CreateGeometryGroup_Unbound();
    void CreateTransformedGeometry_Unbound();

    void CreatePathGeometry(out ID2D1PathGeometry geometry);
    void CreateStrokeStyle(in StrokeStyleProperties properties, nint dashes, uint dashesCount, out ID2D1StrokeStyle strokeStyle);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("2cd9069d-12e2-11dc-9fed-001143a055f9")]
internal partial interface ID2D1StrokeStyle : ID2D1Resource;

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("2cd906a1-12e2-11dc-9fed-001143a055f9")]
internal partial interface ID2D1Geometry : ID2D1Resource
{
    [PreserveSig]
    int GetBounds(nint worldTransform, out RECTF bounds);
    void GetWidenedBounds_Unbound();
    void StrokeContainsPoint_Unbound();
    void FillContainsPoint_Unbound();
    void CompareWithGeometry_Unbound();
    void Simplify_Unbound();
    void Tessellate_Unbound();
    void CombineWithGeometry_Unbound();
    void Outline_Unbound();
    void ComputeArea_Unbound();
    void ComputeLength_Unbound();
    void ComputePointAtLength_Unbound();
    void Widen_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("2cd906a5-12e2-11dc-9fed-001143a055f9")]
internal partial interface ID2D1PathGeometry : ID2D1Geometry
{
    void Open(out ID2D1GeometrySink sink);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("2cd9069f-12e2-11dc-9fed-001143a055f9")]
internal partial interface ID2D1GeometrySink
{
    [PreserveSig]
    void SetFillMode(uint fillMode);
    [PreserveSig]
    void SetSegmentFlags(uint flags);
    [PreserveSig]
    void BeginFigure(PointF startPoint, uint figureBegin);
    void AddLines_Unbound();
    void AddBeziers_Unbound();
    [PreserveSig]
    void EndFigure(uint figureEnd);
    [PreserveSig]
    int Close();

    [PreserveSig]
    void AddLine(PointF point);
}