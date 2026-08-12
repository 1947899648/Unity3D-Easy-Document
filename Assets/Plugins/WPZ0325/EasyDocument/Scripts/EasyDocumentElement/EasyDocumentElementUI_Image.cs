using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WPZ0325.EasyDocument
{
    /// <summary>
    /// EasyDocument 图片元素：RawImage 显示图片 + 底部图片标题（图片块的子字段）。
    /// 优先使用 Prefab 中已搭好的 Image / Caption 子物体结构，未找到时自动构建。
    /// 尺寸优先级：json 指定的显示宽高 > 按 ImageMaxWidthRatio 比例自适应
    /// </summary>
    public class EasyDocumentElementUI_Image : MonoBehaviour
    {
        private RawImage _rawImage;
        private TextMeshProUGUI _captionText;
        private LayoutElement _imageLayoutElement;
        private Texture2D _texture;

        /// <summary>
        /// 初始化图片元素，texture 可为 null（加载失败时显示灰色占位）
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="texture"></param>
        /// <param name="caption"></param>
        /// <param name="displayWidth">json 指定的显示宽度（像素），<=0 时按比例自适应</param>
        /// <param name="displayHeight">json 指定的显示高度（像素），<=0 时按宽高比自动计算</param>
        public void Init(EasyDocumentSetting setting, Texture2D texture, string caption, float displayWidth = 0.0f, float displayHeight = 0.0f)
        {
            _texture = texture;
            float contentWidth = GetContentWidth();

            Transform imageTrans = this.transform.Find("Image");
            if (imageTrans == null)
            {
                imageTrans = CreateChild("Image");
                _rawImage = imageTrans.gameObject.AddComponent<RawImage>();
                _imageLayoutElement = imageTrans.gameObject.AddComponent<LayoutElement>();
            }
            else
            {
                _rawImage = imageTrans.GetComponent<RawImage>();
                if (_rawImage == null)
                {
                    _rawImage = imageTrans.gameObject.AddComponent<RawImage>();
                }
                _imageLayoutElement = imageTrans.GetComponent<LayoutElement>();
                if (_imageLayoutElement == null)
                {
                    _imageLayoutElement = imageTrans.gameObject.AddComponent<LayoutElement>();
                }
            }
            _rawImage.raycastTarget = false;
            _rawImage.texture = texture;

            Transform captionTrans = this.transform.Find("Caption");
            if (captionTrans == null)
            {
                captionTrans = CreateChild("Caption");
                _captionText = captionTrans.gameObject.AddComponent<TextMeshProUGUI>();
                captionTrans.gameObject.AddComponent<LayoutElement>();
            }
            else
            {
                _captionText = captionTrans.GetComponent<TextMeshProUGUI>();
                if (_captionText == null)
                {
                    _captionText = captionTrans.gameObject.AddComponent<TextMeshProUGUI>();
                }
            }
            _captionText.raycastTarget = false;

            EnsureCaptionSizeFitter(captionTrans);

            float imageWidth;
            float imageHeight;
            CalculateImageSize(setting, texture, contentWidth, displayWidth, displayHeight, out imageWidth, out imageHeight);

            RectTransform imageRect = imageTrans as RectTransform;
            imageRect.sizeDelta = new Vector2(imageWidth, imageHeight);
            _imageLayoutElement.preferredWidth = imageWidth;
            _imageLayoutElement.preferredHeight = imageHeight;

            InitCaption(setting, caption, imageWidth, captionTrans);
        }

        /// <summary>
        /// 计算图片显示尺寸：json 指定宽高优先，其次按最大宽度比例与图片宽高比
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="texture"></param>
        /// <param name="contentWidth"></param>
        /// <param name="displayWidth"></param>
        /// <param name="displayHeight"></param>
        /// <param name="outWidth"></param>
        /// <param name="outHeight"></param>
        private static void CalculateImageSize(EasyDocumentSetting setting, Texture2D texture, float contentWidth, float displayWidth, float displayHeight, out float outWidth, out float outHeight)
        {
            if (displayWidth > 0.0f && displayHeight > 0.0f)
            {
                outWidth = displayWidth;
                outHeight = displayHeight;
                return;
            }

            if (texture != null)
            {
                float maxRatio = setting != null ? setting.ImageMaxWidthRatio : 0.8f;
                outWidth = displayWidth > 0.0f ? displayWidth : contentWidth * maxRatio;
                outHeight = displayHeight > 0.0f ? displayHeight : outWidth * (float)texture.height / (float)texture.width;
                return;
            }

            outWidth = 100.0f;
            outHeight = 100.0f;
        }

        /// <summary>
        /// 初始化图片标题
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="caption"></param>
        /// <param name="imageWidth"></param>
        /// <param name="captionTrans"></param>
        private void InitCaption(EasyDocumentSetting setting, string caption, float imageWidth, Transform captionTrans)
        {
            LayoutElement captionLayoutElement = captionTrans.GetComponent<LayoutElement>();
            if (captionLayoutElement != null)
            {
                captionLayoutElement.preferredWidth = imageWidth;
            }

            _captionText.font = EasyDocumentElementUI_Text.GetFontBySetting(setting);
            _captionText.text = string.IsNullOrEmpty(caption) ? string.Empty : caption;
            _captionText.enableWordWrapping = true;
            _captionText.overflowMode = TextOverflowModes.Overflow;

            if (setting == null) return;
            _captionText.color = setting.Color_Caption;
            _captionText.fontSize = setting.FontSize_Caption;
            _captionText.fontStyle = setting.FontStyle_Caption;
            _captionText.alignment = setting.Align_Caption;
        }

        /// <summary>
        /// 确保 Caption 子物体挂 ContentSizeFitter（垂直撑高，适配任意 Prefab 结构）
        /// </summary>
        /// <param name="captionTrans"></param>
        private static void EnsureCaptionSizeFitter(Transform captionTrans)
        {
            ContentSizeFitter csf = captionTrans.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = captionTrans.gameObject.AddComponent<ContentSizeFitter>();
            }
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        /// <summary>
        /// 创建子物体
        /// </summary>
        /// <param name="childName"></param>
        /// <returns></returns>
        private Transform CreateChild(string childName)
        {
            GameObject childObj = new GameObject(childName, typeof(RectTransform));
            childObj.transform.SetParent(this.transform, false);
            return childObj.transform;
        }

        /// <summary>
        /// 获取内容宽度（父级 Content 宽度）
        /// </summary>
        /// <returns></returns>
        private float GetContentWidth()
        {
            Transform parent = this.transform.parent;
            if (parent == null) return 500.0f;
            RectTransform parentRect = parent as RectTransform;
            if (parentRect == null) return 500.0f;
            return Mathf.Max(1.0f, parentRect.rect.width);
        }

        protected virtual void OnDestroy()
        {
            if (_texture != null)
            {
                EasyDocumentHandler.DestroyTexture(_texture);
                _texture = null;
            }
        }
    }
}
