using TMPro;
using UnityEngine;

namespace WPZ0325.EasyDocument
{
    /// <summary>
    /// EasyDocument 样式配置，用户可拖入 SDF 字体并预设各元素类型样式
    /// </summary>
    [CreateAssetMenu(fileName = "EasyDocumentSetting", menuName = "WPZ0325/Create SO/EasyDocument/EasyDocumentSetting")]
    public class EasyDocumentSetting : ScriptableObject
    {
        #region 字体与排版

        [Header("------SDF字体（可空，空则使用TMP默认字体）------")]
        public TMP_FontAsset FontAsset;

        [Header("------排版------")]
        [Tooltip("元素之间的垂直间距")]
        public float BlockSpacing = 12.0f;
        [Tooltip("内容四周内边距")]
        public float ContentPadding = 20.0f;
        [Tooltip("文本块内文字与块左右两侧的距离")]
        public float TextBlockPaddingX = 10.0f;
        [Tooltip("图片最大宽度占内容宽度的比例")]
        public float ImageMaxWidthRatio = 0.8f;

        #endregion

        #region 标题样式

        [Header("------1级标题------")]
        public Color Color_Title_1 = Color.black;
        public float FontSize_Title_1 = 40.0f;
        public FontStyles FontStyle_Title_1 = FontStyles.Bold;
        public TextAlignmentOptions Align_Title_1 = TextAlignmentOptions.Left;

        [Header("------2级标题------")]
        public Color Color_Title_2 = Color.black;
        public float FontSize_Title_2 = 32.0f;
        public FontStyles FontStyle_Title_2 = FontStyles.Bold;
        public TextAlignmentOptions Align_Title_2 = TextAlignmentOptions.Left;

        [Header("------3级标题------")]
        public Color Color_Title_3 = Color.black;
        public float FontSize_Title_3 = 26.0f;
        public FontStyles FontStyle_Title_3 = FontStyles.Bold;
        public TextAlignmentOptions Align_Title_3 = TextAlignmentOptions.Left;

        [Header("------4级标题------")]
        public Color Color_Title_4 = Color.black;
        public float FontSize_Title_4 = 22.0f;
        public FontStyles FontStyle_Title_4 = FontStyles.Bold;
        public TextAlignmentOptions Align_Title_4 = TextAlignmentOptions.Left;

        #endregion

        #region 正文与图片标题样式

        [Header("------正文------")]
        public Color Color_Body = new Color(0.15f, 0.15f, 0.15f, 1.0f);
        public float FontSize_Body = 18.0f;
        public FontStyles FontStyle_Body = FontStyles.Normal;
        public TextAlignmentOptions Align_Body = TextAlignmentOptions.Left;

        [Header("------图片标题------")]
        public Color Color_Caption = Color.gray;
        public float FontSize_Caption = 14.0f;
        public FontStyles FontStyle_Caption = FontStyles.Normal;
        public TextAlignmentOptions Align_Caption = TextAlignmentOptions.Center;

        #endregion

        #region 样式查询

        /// <summary>
        /// 根据元素类型获取对应文本样式
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="outColor"></param>
        /// <param name="outFontSize"></param>
        /// <param name="outFontStyle"></param>
        /// <param name="outAlign"></param>
        public void GetTextStyle(EnEasyDocumentElementType elementType, out Color outColor, out float outFontSize, out FontStyles outFontStyle, out TextAlignmentOptions outAlign)
        {
            outColor = Color_Body;
            outFontSize = FontSize_Body;
            outFontStyle = FontStyle_Body;
            outAlign = Align_Body;
            switch (elementType)
            {
                case EnEasyDocumentElementType.TITLE_1:
                    outColor = Color_Title_1;
                    outFontSize = FontSize_Title_1;
                    outFontStyle = FontStyle_Title_1;
                    outAlign = Align_Title_1;
                    break;
                case EnEasyDocumentElementType.TITLE_2:
                    outColor = Color_Title_2;
                    outFontSize = FontSize_Title_2;
                    outFontStyle = FontStyle_Title_2;
                    outAlign = Align_Title_2;
                    break;
                case EnEasyDocumentElementType.TITLE_3:
                    outColor = Color_Title_3;
                    outFontSize = FontSize_Title_3;
                    outFontStyle = FontStyle_Title_3;
                    outAlign = Align_Title_3;
                    break;
                case EnEasyDocumentElementType.TITLE_4:
                    outColor = Color_Title_4;
                    outFontSize = FontSize_Title_4;
                    outFontStyle = FontStyle_Title_4;
                    outAlign = Align_Title_4;
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
