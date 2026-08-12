using TMPro;
using UnityEngine;

namespace WPZ0325.EasyDocument
{
    /// <summary>
    /// EasyDocument 文本元素，TMP 自适应高度（由父级 VerticalLayoutGroup 按 preferredHeight 布局）
    /// </summary>
    public class EasyDocumentElementUI_Text : MonoBehaviour
    {
        private TextMeshProUGUI _text;

        /// <summary>
        /// 初始化文本元素样式与内容
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="elementType"></param>
        /// <param name="content"></param>
        public void Init(EasyDocumentSetting setting, EnEasyDocumentElementType elementType, string content)
        {
            if (_text == null)
            {
                _text = this.GetComponent<TextMeshProUGUI>();
                if (_text == null)
                {
                    _text = this.gameObject.AddComponent<TextMeshProUGUI>();
                }
                _text.raycastTarget = false;
            }

            _text.font = GetFontBySetting(setting);
            _text.text = content;
            _text.enableWordWrapping = true;
            _text.overflowMode = TextOverflowModes.Overflow;
            _text.margin = GetTextMargin(setting);

            if (setting == null) return;
            setting.GetTextStyle(elementType, out Color color, out float fontSize, out FontStyles fontStyle, out TextAlignmentOptions align);
            _text.color = color;
            _text.fontSize = fontSize;
            _text.fontStyle = fontStyle;
            _text.alignment = align;
        }

        /// <summary>
        /// 获取文本块内边距：左右两侧由配置控制，上下为 0
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        private static Vector4 GetTextMargin(EasyDocumentSetting setting)
        {
            float paddingX = 0.0f;
            if (setting != null && setting.TextBlockPaddingX > 0.0f)
            {
                paddingX = setting.TextBlockPaddingX;
            }
            return new Vector4(paddingX, 0.0f, paddingX, 0.0f);
        }

        /// <summary>
        /// 获取字体：优先使用配置中的 SDF 字体，其次使用 TMP 默认字体
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static TMP_FontAsset GetFontBySetting(EasyDocumentSetting setting)
        {
            if (setting != null && setting.FontAsset != null)
            {
                return setting.FontAsset;
            }
            return TMP_Settings.defaultFontAsset;
        }
    }
}
