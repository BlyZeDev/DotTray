namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.Models;
using DotTray.Direct.Internal.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16, Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("b859ee5a-d838-4b5b-a2e8-1adc7d93db48")]
internal partial interface IDWriteFactory
{
    void GetSystemFontCollection_Unbound();
    void CreateCustomFontCollection_Unbound();
    void RegisterFontCollectionLoader_Unbound();
    void UnregisterFontCollectionLoader_Unbound();
    void CreateFontFileReference_Unbound();
    void CreateCustomFontFileReference_Unbound();
    void CreateFontFace_Unbound();
    void CreateRenderingParams_Unbound();
    void CreateMonitorRenderingParams_Unbound();
    void CreateCustomRenderingParams_Unbound();
    void RegisterFontFileLoader_Unbound();
    void UnregisterFontFileLoader_Unbound();

    void CreateTextFormat(string fontFamilyName, nint fontCollection, int fontWeight, int fontStyle, int fontStretch, float fontSize, string localeName, out IDWriteTextFormat textFormat);

    void CreateTypography_Unbound();
    void GetGdiInterop_Unbound();

    void CreateTextLayout(string text, uint length, IDWriteTextFormat textFormat, float maxWidth, float maxHeight, out IDWriteTextLayout textLayout);
}

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16, Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("30572f99-dac6-41db-a16e-0486307e606a")]
internal partial interface IDWriteFactory1 : IDWriteFactory
{
    void GetEudcFontCollection_Unbound();
    void CreateCustomRenderingParams1_Unbound();
}

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16, Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("0439fc60-ca44-4994-8dee-3a9af7b732ec")]
internal partial interface IDWriteFactory2 : IDWriteFactory1
{
    void GetSystemFontFallback_Unbound();
    void CreateFontFallbackBuilder_Unbound();

    [PreserveSig]
    int TranslateColorGlyphRun(float baselineOriginX, float baselineOriginY, nint glyphRun, nint glyphRunDescription, uint measuringMode, nint worldToDeviceTransform, uint colorPaletteIndex, out nint colorLayers);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("9c906818-31d7-4fd3-a151-7c5e225db55a")]
internal partial interface IDWriteTextFormat
{
    void SetTextAlignment(uint alignment);
    void SetParagraphAlignment(uint alignment);
    void SetWordWrapping(uint wrapping);
    void SetReadingDirection_Unbound();
    void SetFlowDirection_Unbound();
    void SetIncrementalTabStop_Unbound();
    void SetTrimming_Unbound();
    void SetLineSpacing_Unbound();
    void GetTextAlignment_Unbound();
    void GetParagraphAlignment_Unbound();
    void GetWordWrapping_Unbound();
    void GetReadingDirection_Unbound();
    void GetFlowDirection_Unbound();
    void GetIncrementalTabStop_Unbound();
    void GetTrimming_Unbound();
    void GetLineSpacing_Unbound();
    void GetFontCollection_Unbound();
    void GetFontFamilyNameLength_Unbound();
    void GetFontFamilyName_Unbound();
    void GetFontWeight_Unbound();
    void GetFontStyle_Unbound();
    void GetFontStretch_Unbound();
    void GetFontSize_Unbound();
    void GetLocaleNameLength_Unbound();
    void GetLocaleName_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("53737037-6d14-410b-9bfe-0b182bb70961")]
internal partial interface IDWriteTextLayout : IDWriteTextFormat
{
    void SetMaxWidth(float maxWidth);
    void SetMaxHeight(float maxHeight);
    void SetFontCollection_Unbound();
    void SetFontFamilyName_Unbound();
    void SetFontWeight_Unbound();
    void SetFontStyle_Unbound();
    void SetFontStretch_Unbound();
    void SetFontSize_Unbound();
    void SetUnderline_Unbound();
    void SetStrikethrough_Unbound();
    void SetDrawingEffect_Unbound();
    void SetInlineObject_Unbound();
    void SetTypography_Unbound();
    void SetLocaleName_Unbound();
    void GetMaxWidth_Unbound();
    void GetMaxHeight_Unbound();
    void LayoutGetFontCollection_Unbound();
    void LayoutGetFontFamilyNameLength_Unbound();
    void LayoutGetFontFamilyName_Unbound();
    void LayoutGetFontWeight_Unbound();
    void LayoutGetFontStyle_Unbound();
    void LayoutGetFontStretch_Unbound();
    void LayoutGetFontSize_Unbound();
    void GetUnderline_Unbound();
    void GetStrikethrough_Unbound();
    void GetDrawingEffect_Unbound();
    void GetInlineObject_Unbound();
    void GetTypography_Unbound();
    void LayoutGetLocaleNameLength_Unbound();
    void LayoutGetLocaleName_Unbound();
    [PreserveSig]
    int Draw(nint clientDrawingContext, IDWriteTextRenderer renderer, float originX, float originY);
    void GetLineMetrics_Unbound();
    void GetMetrics(out TextMetrics metrics);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("5f49804d-7024-4d43-bfa9-d25984f53849")]
internal unsafe partial interface IDWriteFontFace
{
    void GetType_Unbound();
    void GetFiles_Unbound();
    void GetIndex_Unbound();
    void GetSimulations_Unbound();
    void IsSymbolFont_Unbound();
    void GetMetrics_Unbound();
    void GetGlyphCount_Unbound();
    void GetDesignGlyphMetrics_Unbound();
    void GetGlyphIndices_Unbound();
    void TryGetFontTable_Unbound();
    void ReleaseFontTable_Unbound();

    [PreserveSig]
    int GetGlyphRunOutline(float emSize, ushort* glyphIndices, float* glyphAdvances, nint glyphOffsets, uint glyphCount, int isSideways, int isRightToLeft, ID2D1GeometrySink geometrySink);
    void GetRecommendedRenderingMode_Unbound();
    void GetGdiCompatibleMetrics_Unbound();
    void GetGdiCompatibleGlyphMetrics_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("a71efdb4-9fdb-4838-ad90-cfc3be8c3daf")]
internal unsafe partial interface IDWriteFontFace1 : IDWriteFontFace
{
    void GetMetrics1_Unbound();
    void GetGdiCompatibleMetrics1_Unbound();
    void GetCaretMetrics_Unbound();
    void GetUnicodeRanges_Unbound();
    void IsMonospacedFont_Unbound();
    void GetDesignGlyphAdvances_Unbound();
    void GetGdiCompatibleGlyphAdvances_Unbound();
    void GetKerningPairAdjustments_Unbound();
    void HasKerningPairs_Unbound();
    void GetRecommendedRenderingMode1_Unbound();
    void GetVerticalGlyphVariants_Unbound();
    void HasVerticalGlyphVariants_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("d8b768ff-64bc-4e66-982b-ec8e87f693f7")]
internal unsafe partial interface IDWriteFontFace2 : IDWriteFontFace1
{
    [PreserveSig]
    int IsColorFont();
}

[GeneratedComInterface]
[Guid("eaf3a2da-ecf4-4d24-b644-b34f6842024b")]
internal partial interface IDWritePixelSnapping
{
    [PreserveSig]
    int IsPixelSnappingDisabled(nint clientContext, out int isDisabled);
    [PreserveSig]
    int GetCurrentTransform(nint clientContext, out Matrix3x2 transform);
    [PreserveSig]
    int GetPixelsPerDip(nint clientContext, out float pixelsPerDip);
}

[GeneratedComInterface]
[Guid("ef8a8135-5cc6-45fe-8825-c5a0724eb819")]
internal partial interface IDWriteTextRenderer : IDWritePixelSnapping
{
    [PreserveSig]
    int DrawGlyphRun(nint clientContext, float x, float y, uint measuringMode, nint glyphRun, nint glyphRunDescription, nint effect);
    [PreserveSig]
    int DrawUnderline(nint clientContext, float x, float y, nint underline, nint effect);
    [PreserveSig]
    int DrawStrikethrough(nint clientContext, float x, float y, nint strikethrough, nint effect);
    [PreserveSig]
    int DrawInlineObject(nint clientContext, float x, float y, nint inlineObject, int isSideways, int isRightToLeft, nint effect);
}

[GeneratedComClass]
internal sealed unsafe partial class InkBoundsRenderer : IDWriteTextRenderer
{
    private readonly ID2D1Factory _factory;

    public bool HasInk { get; private set; }
    public float Left = float.MaxValue, Top = float.MaxValue, Right = float.MinValue, Bottom = float.MinValue;

    public InkBoundsRenderer(ID2D1Factory factory) => _factory = factory;

    public int IsPixelSnappingDisabled(nint clientContext, out int isDisabled)
    {
        isDisabled = 1;
        return 0;
    }

    public int GetCurrentTransform(nint clientContext, out Matrix3x2 transform)
    {
        transform = new Matrix3x2
        {
            M11 = 1,
            M22 = 1
        };
        return 0;
    }

    public int GetPixelsPerDip(nint clientContext, out float pixelsPerDip)
    {
        pixelsPerDip = 1f;
        return 0;
    }

    public int DrawGlyphRun(nint clientContext, float x, float y, uint measuringMode, nint glyphRun, nint glyphRunDescription, nint effect)
    {
        var run = (GlyphRun*)glyphRun;
        if (run->GlyphCount == 0) return 0;

        var face = ComInterfaceMarshaller<IDWriteFontFace>.ConvertToManaged((void*)run->FontFace)!;
        if (((IDWriteFontFace2)face).IsColorFont() != 0) return 0;

        _factory.CreatePathGeometry(out var geometry);

        try
        {
            geometry.Open(out var sink);
            int hr;

            try
            {
                hr = face.GetGlyphRunOutline(run->FontEmSize, run->GlyphIndices, run->GlyphAdvances, run->GlyphOffsets,
                    run->GlyphCount, run->IsSideways, (int)(run->BidiLevel & 1), sink);
                sink.Close();
            }
            finally
            {
                Com.Release(sink);
            }

            if (hr < 0) return 0;

            geometry.GetBounds(nint.Zero, out var b);

            if (!float.IsFinite(b.Left) || !float.IsFinite(b.Right) || b.Right <= b.Left || b.Bottom <= b.Top) return 0;

            HasInk = true;
            Left = MathF.Min(Left, x + b.Left);
            Top = MathF.Min(Top, y + b.Top);
            Right = MathF.Max(Right, x + b.Right);
            Bottom = MathF.Max(Bottom, y + b.Bottom);
        }
        finally
        {
            Com.Release(geometry);
        }

        return 0;
    }

    public int DrawUnderline(nint clientContext, float x, float y, nint underline, nint effect) => 0;

    public int DrawStrikethrough(nint clientContext, float x, float y, nint strikethrough, nint effect) => 0;

    public int DrawInlineObject(nint clientContext, float x, float y, nint inlineObject, int isSideways, int isRightToLeft, nint effect) => 0;
}