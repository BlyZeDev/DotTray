namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.Models;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal static partial class COM
{
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
        void GetLineMetrics_Unbound();
        void GetMetrics(out TextMetrics metrics);
    }
}