using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WPZ0325.EasyDocument
{
    /// <summary>
    /// EasyDocument 文档控制器：挂在任意 RectTransform 上，运行时自动构建 ScrollView 结构并按数据生成文档内容
    /// </summary>
    public class EasyDocumentController : MonoBehaviour
    {
        [SerializeField] EasyDocumentSetting _setting;
        [SerializeField] string _documentFolderName = "示例文档";

        [SerializeField] GameObject _prefabBlockText;
        [SerializeField] GameObject _prefabBlockImage;

        [SerializeField] RectTransform _content;

        /// <summary>
        /// 初始化文档，folderName 为 EasyDocumentData 下的文档文件夹名
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="onFinished">加载与生成完成后回调</param>
        public void Init(string folderName, Action onFinished = null)
        {
            _documentFolderName = folderName;
            StartCoroutine(InitCoroutine(folderName, onFinished));
        }

        private IEnumerator InitCoroutine(string folderName, Action onFinished)
        {
            BuildUIStructureIfNeed();

            EasyDocumentDataModel data = null;
            yield return EasyDocumentHandler.LoadDocumentJson(folderName, (EasyDocumentDataModel loaded) => { data = loaded; });

            ClearBlocks();
            if (data == null)
            {
                Debug.LogError($"[EasyDocument] 文档数据加载失败:{folderName}");
                onFinished?.Invoke();
                yield break;
            }

            List<EasyDocumentElementDataModel> elements = data.Elements;
            if (elements == null || elements.Count == 0)
            {
                Debug.LogWarning($"[EasyDocument] 文档元素列表为空:{folderName}");
                onFinished?.Invoke();
                yield break;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                EasyDocumentElementDataModel elementData = elements[i];
                EnEasyDocumentElementType elementType = ParseElementType(elementData.Type);
                switch (elementType)
                {
                    case EnEasyDocumentElementType.TITLE_1:
                    case EnEasyDocumentElementType.TITLE_2:
                    case EnEasyDocumentElementType.TITLE_3:
                    case EnEasyDocumentElementType.TITLE_4:
                    case EnEasyDocumentElementType.BODY:
                        CreateTextBlock(elementType, elementData.Text);
                        break;
                    case EnEasyDocumentElementType.IMAGE:
                        yield return CreateImageBlock(folderName, elementData);
                        break;
                    default:
                        Debug.LogError($"[EasyDocument] 未知元素类型:{elementData.Type} ,index:{i}");
                        break;
                }
            }

            onFinished?.Invoke();
        }

        /// <summary>
        /// 构建文档显示结构：优先使用已有 ScrollRect 的 Content（用户手动搭建），否则自动构建 ScrollView + Viewport + Content
        /// </summary>
        private void BuildUIStructureIfNeed()
        {
            if (_content != null) return;

            if (!TryFindExistingContent())
            {
                BuildAutoStructure();
            }

            EnsureLayoutComponents();
        }

        /// <summary>
        /// 从自身及子物体查找已有 ScrollRect 的 Content
        /// </summary>
        /// <returns></returns>
        private bool TryFindExistingContent()
        {
            ScrollRect[] scrollRects = this.GetComponentsInChildren<ScrollRect>(true);
            if (scrollRects == null || scrollRects.Length == 0) return false;

            for (int i = 0; i < scrollRects.Length; i++)
            {
                ScrollRect sr = scrollRects[i];
                if (sr != null && sr.content != null)
                {
                    _content = sr.content;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 自动构建 ScrollView + Viewport + Content 结构
        /// </summary>
        private void BuildAutoStructure()
        {
            RectTransform selfRect = this.transform as RectTransform;
            if (selfRect == null)
            {
                Debug.LogError("[EasyDocument] Controller 必须挂在带 RectTransform 的物体上");
                return;
            }

            ScrollRect scrollRect = this.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform));
            viewportObj.transform.SetParent(this.transform, false);
            RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
            SetStretchFull(viewportRect);
            viewportObj.AddComponent<RectMask2D>();

            GameObject contentObj = new GameObject("Content", typeof(RectTransform));
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.0f, 1.0f);
            contentRect.anchorMax = new Vector2(1.0f, 1.0f);
            contentRect.pivot = new Vector2(0.5f, 1.0f);
            contentRect.sizeDelta = new Vector2(0.0f, 0.0f);
            contentRect.anchoredPosition = new Vector2(0.0f, 0.0f);

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            _content = contentRect;
        }

        /// <summary>
        /// 为 Content 补齐并统一布局组件（VerticalLayoutGroup + ContentSizeFitter + 样式间距）
        /// </summary>
        private void EnsureLayoutComponents()
        {
            if (_content == null) return;

            VerticalLayoutGroup vlg = _content.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = _content.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter csf = _content.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = _content.gameObject.AddComponent<ContentSizeFitter>();
            }
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            float spacing = _setting != null ? _setting.BlockSpacing : 12.0f;
            float padding = _setting != null ? _setting.ContentPadding : 20.0f;
            vlg.spacing = spacing;
            vlg.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
        }

        /// <summary>
        /// 将 json 中的字符串类型解析为枚举，解析失败返回 NONE
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static EnEasyDocumentElementType ParseElementType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return EnEasyDocumentElementType.NONE;
            if (Enum.TryParse(typeName, true, out EnEasyDocumentElementType parsedType))
            {
                return parsedType;
            }
            return EnEasyDocumentElementType.NONE;
        }

        /// <summary>
        /// 生成文本块元素：优先实例化配置的 Block_Text prefab，未配置时自动构建
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="content"></param>
        private void CreateTextBlock(EnEasyDocumentElementType elementType, string content)
        {
            GameObject blockObj = null;
            if (_prefabBlockText != null)
            {
                blockObj = Instantiate(_prefabBlockText, _content);
            }
            else
            {
                blockObj = new GameObject("Block_Text", typeof(RectTransform));
                blockObj.transform.SetParent(_content, false);
            }

            EasyDocumentElementUI_Text block = blockObj.GetComponent<EasyDocumentElementUI_Text>();
            if (block == null)
            {
                block = blockObj.AddComponent<EasyDocumentElementUI_Text>();
            }
            block.Init(_setting, elementType, content);
        }

        /// <summary>
        /// 生成图片块元素（图片异步加载完成后初始化）：优先实例化配置的 Block_Image prefab，未配置时自动构建
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="elementData"></param>
        /// <returns></returns>
        private IEnumerator CreateImageBlock(string folderName, EasyDocumentElementDataModel elementData)
        {
            GameObject blockObj = null;
            if (_prefabBlockImage != null)
            {
                blockObj = Instantiate(_prefabBlockImage, _content);
            }
            else
            {
                blockObj = new GameObject("Block_Image", typeof(RectTransform));
                blockObj.transform.SetParent(_content, false);

                VerticalLayoutGroup vlg = blockObj.AddComponent<VerticalLayoutGroup>();
                vlg.childControlWidth = false;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = false;
                vlg.childForceExpandHeight = false;
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.spacing = 4.0f;
            }

            EasyDocumentElementUI_Image block = blockObj.GetComponent<EasyDocumentElementUI_Image>();
            if (block == null)
            {
                block = blockObj.AddComponent<EasyDocumentElementUI_Image>();
            }

            Texture2D texture = null;
            if (!string.IsNullOrEmpty(elementData.ImagePath))
            {
                yield return EasyDocumentHandler.LoadDocumentImage(folderName, elementData.ImagePath, (Texture2D loaded) => { texture = loaded; });
            }
            block.Init(_setting, texture, elementData.Caption, elementData.ImageWidth, elementData.ImageHeight);
        }

        /// <summary>
        /// 生成文档（编辑器面板按钮调用，可在非播放模式下手动迭代执行）
        /// </summary>
        /// <returns></returns>
        public IEnumerator GenerateInEditor()
        {
            return InitCoroutine(_documentFolderName, null);
        }

        /// <summary>
        /// 清空所有已生成的内容块：销毁 Content 下全部子物体
        /// （不依赖内存列表，编辑器模式下脚本重编译后依然有效）
        /// </summary>
        public void ClearBlocks()
        {
            if (_content == null)
            {
                TryFindExistingContent();
            }
            if (_content == null) return;

            for (int i = _content.childCount - 1; i >= 0; i--)
            {
                Transform child = _content.GetChild(i);
                if (child != null)
                {
                    DestroyBlockObject(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 销毁对象：运行时用 Destroy（帧末延迟销毁），编辑器下用 DestroyImmediate（立即销毁）
        /// </summary>
        /// <param name="obj"></param>
        private void DestroyBlockObject(GameObject obj)
        {
            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }

        /// <summary>
        /// 将 RectTransform 拉伸铺满父级
        /// </summary>
        /// <param name="rect"></param>
        private static void SetStretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
