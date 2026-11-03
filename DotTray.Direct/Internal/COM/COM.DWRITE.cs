namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.Models;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16, Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("b859ee5a-d838-4b5b-a2e8-1adc7d93db48")]
internal partial interface IDWriteFactory
{
    [PreserveSig]
    int GetSystemFontCollection(out IDWriteFontCollection fontCollection, int checkForUpdates);
    void CreateCustomFontCollection_Unbound();
    void RegisterFontCollectionLoader_Unbound();
    void UnregisterFontCollectionLoader_Unbound();
    [PreserveSig]
    int CreateFontFileReference(string filePath, nint lastWriteTime, out IDWriteFontFile fontFile);
    void CreateCustomFontFileReference_Unbound();
    void CreateFontFace_Unbound();
    void CreateRenderingParams_Unbound();
    void CreateMonitorRenderingParams_Unbound();
    void CreateCustomRenderingParams_Unbound();
    void RegisterFontFileLoader_Unbound();
    void UnregisterFontFileLoader_Unbound();

    void CreateTextFormat(string fontFamilyName, IDWriteFontCollection? fontCollection, int fontWeight, int fontStyle, int fontStretch, float fontSize, string localeName, out IDWriteTextFormat textFormat);

    void CreateTypography_Unbound();
    void GetGdiInterop_Unbound();

    void CreateTextLayout(string text, uint length, IDWriteTextFormat textFormat, float maxWidth, float maxHeight, out IDWriteTextLayout textLayout);
    void CreateGdiCompatibleTextLayout_Unbound();
    void CreateEllipsisTrimmingSign_Unbound();
    void CreateTextAnalyzer_Unbound();
    void CreateNumberSubstitution_Unbound();
    void CreateGlyphRunAnalysis_Unbound();
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
    void TranslateColorGlyphRun_Unbound();
    void CreateCustomRenderingParams2_Unbound();
    void CreateGlyphRunAnalysis2_Unbound();
}

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16, Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("9a1b41c3-d3bb-466a-87fc-fe67556a3b65")]
internal partial interface IDWriteFactory3 : IDWriteFactory2
{
    void CreateGlyphRunAnalysis3_Unbound();
    void CreateCustomRenderingParams3_Unbound();
    void CreateFontFaceReferenceFromFile_Unbound();

    [PreserveSig]
    int CreateFontFaceReference(string filePath, nint lastWriteTime, uint faceIndex, uint simulations, out IDWriteFontFaceReference fontFaceReference);

    void GetSystemFontSet_Unbound();

    [PreserveSig]
    int CreateFontSetBuilder(out IDWriteFontSetBuilder fontSetBuilder);

    [PreserveSig]
    int CreateFontCollectionFromFontSet(IDWriteFontSet fontSet, out IDWriteFontCollection fontCollection);
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
    void Draw_Unbound();
    [PreserveSig]
    unsafe int GetLineMetrics(LineMetrics* lineMetrics, uint maxLineCount, out uint actualLineCount);
    void GetMetrics(out TextMetrics metrics);
}

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16, Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("a84cee02-3eea-4eee-a827-87c1a02a0fcc")]
internal partial interface IDWriteFontCollection
{
    [PreserveSig]
    uint GetFontFamilyCount();

    [PreserveSig]
    int GetFontFamily(uint index, out IDWriteFontFamily fontFamily);

    [PreserveSig]
    int FindFamilyName(string familyName, out uint index, out int exists);

    void GetFontFromFontFace_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("1a0d8438-1d97-4ec1-aef9-a2fb86ed6acb")]
internal partial interface IDWriteFontList
{
    void GetFontCollection_Unbound();
    void GetFontCount_Unbound();
    void GetFont_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("da20d8ef-812a-4c43-9802-62ec4abd7add")]
internal partial interface IDWriteFontFamily : IDWriteFontList
{
    [PreserveSig]
    int GetFamilyNames(out IDWriteLocalizedStrings names);

    [PreserveSig]
    int GetFirstMatchingFont(uint weight, uint stretch, uint style, out IDWriteFont font);

    void GetMatchingFonts_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("acd16696-8c14-4f5d-877e-fe3fc1d32737")]
internal partial interface IDWriteFont
{
    void GetFontFamily_Unbound();
    void GetWeight_Unbound();
    void GetStretch_Unbound();
    void GetStyle_Unbound();
    void IsSymbolFont_Unbound();
    void GetFaceNames_Unbound();
    void GetInformationalStrings_Unbound();
    void GetSimulations_Unbound();

    [PreserveSig]
    void GetMetrics(out DWriteFontMetrics fontMetrics);

    void HasCharacter_Unbound();
    void CreateFontFace_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("739d886a-cef5-47dc-8769-1a8b41bebbb0")]
internal partial interface IDWriteFontFile
{
    void GetReferenceKey_Unbound();
    void GetLoader_Unbound();

    [PreserveSig]
    int Analyze(out int isSupportedFontType, out uint fontFileType, out uint fontFaceType, out uint numberOfFaces);
}

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16, Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("08256209-099a-4b34-b86d-c22b110e7771")]
internal unsafe partial interface IDWriteLocalizedStrings
{
    void GetCount_Unbound();

    [PreserveSig]
    int FindLocaleName(string localeName, out uint index, out int exists);

    void GetLocaleNameLength_Unbound();
    void GetLocaleName_Unbound();

    [PreserveSig]
    int GetStringLength(uint index, out uint length);

    [PreserveSig]
    int GetString(uint index, char* stringBuffer, uint size);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("5e7fa7ca-dde3-424c-89f0-9fcd6fed58cd")]
internal partial interface IDWriteFontFaceReference
{
    void CreateFontFace_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("53585141-d9f8-4095-8321-d73cf6bd116b")]
internal partial interface IDWriteFontSet
{
    void GetFontCount_Unbound();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("2f642afe-9c68-4f40-b8be-457401afcb3d")]
internal partial interface IDWriteFontSetBuilder
{
    void AddFontFaceReferenceWithProperties_Unbound();

    [PreserveSig]
    int AddFontFaceReference(IDWriteFontFaceReference fontFaceReference);

    void AddFontSet_Unbound();

    [PreserveSig]
    int CreateFontSet(out IDWriteFontSet fontSet);
}