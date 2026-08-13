using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WPZ0325.EasyDocument
{
    /// <summary>
    /// 章节平滑滚动缓动类型
    /// </summary>
    public enum EnEaseType
    {
        Linear = 0,
        EaseIn = 1,
        EaseOut = 2,
        EaseInOut = 3,
    }

    /// <summary>
    /// 章节导航器：挂载于含 ScrollRect 的对象上，提供按章节平滑滚动定位能力。
    /// 自动订阅 EasyDocumentController 的生成/清理事件刷新章节索引。
    /// ExecuteAlways：编辑器非播放模式下也订阅事件，保证面板章节索引与生成按钮联动。
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(ScrollRect))]
    public class EasyDocumentChapterNavigator : MonoBehaviour
    {
        /// <summary>
        /// 章节信息
        /// </summary>
        [Serializable]
        public class ChapterInfo
        {
            /// <summary>
            /// 章节标题文本
            /// </summary>
            public string Title;

            /// <summary>
            /// 章节标题级别（TITLE_1~TITLE_4）
            /// </summary>
            public EnEasyDocumentElementType Level;

            /// <summary>
            /// 章节块挂载点
            /// </summary>
            public RectTransform BlockRect;
        }

        /// <summary>
        /// 章节按钮绑定：用户拖入的章节跳转按钮，顺序对应章节 index
        /// </summary>
        [Serializable]
        public class ChapterButtonBinding
        {
            /// <summary>
            /// 章节跳转按钮（可空，空则跳过绑定）
            /// </summary>
            public Button ButtonRef;
        }

        #region 序列化字段

        [Tooltip("EasyDocumentController 引用（用于订阅文档生成/清理事件）")]
        [SerializeField] EasyDocumentController _controller;
        [Tooltip("平滑滚动时长（秒）")]
        [SerializeField] float _smoothDuration = 0.5f;
        [Tooltip("平滑滚动缓动类型")]
        [SerializeField] EnEaseType _smoothType = EnEaseType.EaseInOut;
        [Tooltip("章节跳转按钮绑定列表（顺序对应章节 index，可空）")]
        [SerializeField] List<ChapterButtonBinding> _chapterButtonBindings = new List<ChapterButtonBinding>();

        #endregion

        #region 私有字段

        private ScrollRect _scrollRect;
        private readonly List<ChapterInfo> _chapters = new List<ChapterInfo>();
        private readonly Dictionary<Button, UnityAction> _buttonActions = new Dictionary<Button, UnityAction>();
        private IEnumerator _scrollCoroutine;

        #endregion

        #region 生命周期

        private void Awake()
        {
            _scrollRect = this.GetComponent<ScrollRect>();
        }

        private void OnEnable()
        {
            if (_controller != null)
            {
                _controller.OnDocumentGenerated.AddListener(RefreshChapters);
                _controller.OnDocumentCleared.AddListener(ClearChapters);
            }

            RefreshChapters();

            if (Application.isPlaying)
            {
                BindChapterButtons();
            }
        }

        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.OnDocumentGenerated.RemoveListener(RefreshChapters);
                _controller.OnDocumentCleared.RemoveListener(ClearChapters);
            }

            if (Application.isPlaying)
            {
                UnbindAllButtons();
            }
        }

        /// <summary>
        /// 移除全部按钮监听（OnDisable 或清空索引时调用）
        /// </summary>
        private void UnbindAllButtons()
        {
            foreach (KeyValuePair<Button, UnityAction> pair in _buttonActions)
            {
                if (pair.Key != null)
                {
                    pair.Key.onClick.RemoveListener(pair.Value);
                }
            }
            _buttonActions.Clear();
        }

        #endregion

        #region 章节索引

        /// <summary>
        /// 章节列表（只读，供编辑器面板展示层级结构）
        /// </summary>
        public IReadOnlyList<ChapterInfo> Chapters
        {
            get
            {
                return _chapters;
            }
        }

        /// <summary>
        /// 扫描 Content 下所有 TITLE_1~TITLE_4 文本块，重建章节索引
        /// （文档生成事件触发时自动调用，也可手动调用）
        /// </summary>
        public void RefreshChapters()
        {
            _chapters.Clear();
            if (_scrollRect == null || _scrollRect.content == null) return;

            Canvas.ForceUpdateCanvases();

            EasyDocumentElementUI_Text[] textBlocks = _scrollRect.content.GetComponentsInChildren<EasyDocumentElementUI_Text>(true);
            if (textBlocks == null || textBlocks.Length == 0) return;

            for (int i = 0; i < textBlocks.Length; i++)
            {
                EasyDocumentElementUI_Text textBlock = textBlocks[i];
                EnEasyDocumentElementType level = textBlock.ElementType;
                if (level != EnEasyDocumentElementType.TITLE_1 &&
                    level != EnEasyDocumentElementType.TITLE_2 &&
                    level != EnEasyDocumentElementType.TITLE_3 &&
                    level != EnEasyDocumentElementType.TITLE_4)
                {
                    continue;
                }

                ChapterInfo info = new ChapterInfo();
                info.Level = level;
                info.BlockRect = textBlock.transform as RectTransform;

                TextMeshProUGUI tmpText = textBlock.GetComponent<TextMeshProUGUI>();
                info.Title = tmpText != null ? tmpText.text : string.Empty;

                _chapters.Add(info);
            }

            SyncBindingList();
        }

        /// <summary>
        /// 同步绑定列表长度与章节数量一致（保留已拖入的按钮引用，尾部补齐/裁剪）
        /// </summary>
        private void SyncBindingList()
        {
            while (_chapterButtonBindings.Count < _chapters.Count)
            {
                _chapterButtonBindings.Add(new ChapterButtonBinding());
            }
            while (_chapterButtonBindings.Count > _chapters.Count)
            {
                _chapterButtonBindings.RemoveAt(_chapterButtonBindings.Count - 1);
            }
        }

        /// <summary>
        /// 清空章节索引与按钮绑定（文档清理事件触发时自动调用，彻底清除后重新生成）
        /// </summary>
        private void ClearChapters()
        {
            _chapters.Clear();
            _chapterButtonBindings.Clear();
            UnbindAllButtons();
        }

        /// <summary>
        /// 为章节绑定列表中的按钮添加点击监听：有引用则绑定 ScrollToChapter(index)，无引用则跳过
        /// </summary>
        private void BindChapterButtons()
        {
            for (int i = 0; i < _chapterButtonBindings.Count; i++)
            {
                ChapterButtonBinding binding = _chapterButtonBindings[i];
                if (binding == null || binding.ButtonRef == null) continue;
                if (_buttonActions.ContainsKey(binding.ButtonRef)) continue;

                int capturedIndex = i;
                UnityAction action = () => ScrollToChapter(capturedIndex);
                binding.ButtonRef.onClick.AddListener(action);
                _buttonActions.Add(binding.ButtonRef, action);
            }
        }

        #endregion

        #region 滚动

        /// <summary>
        /// 按章节序号滚动到对应章节（从 0 开始）
        /// </summary>
        /// <param name="chapterIndex">章节序号</param>
        /// <param name="onFinished">滚动完成回调</param>
        public void ScrollToChapter(int chapterIndex, Action onFinished = null)
        {
            if (chapterIndex < 0 || chapterIndex >= _chapters.Count)
            {
                Debug.LogWarning($"[EasyDocument] 章节序号越界:{chapterIndex}");
                onFinished?.Invoke();
                return;
            }

            ScrollToRect(_chapters[chapterIndex].BlockRect, onFinished);
        }

        /// <summary>
        /// 按标题模糊匹配滚动到对应章节（互相包含即命中，如传入“第一章”匹配“第一章 简介”）
        /// </summary>
        /// <param name="chapterTitle">章节标题关键字</param>
        /// <param name="onFinished">滚动完成回调</param>
        public void ScrollToChapterByTitle(string chapterTitle, Action onFinished = null)
        {
            if (string.IsNullOrEmpty(chapterTitle))
            {
                onFinished?.Invoke();
                return;
            }

            for (int i = 0; i < _chapters.Count; i++)
            {
                ChapterInfo info = _chapters[i];
                if (info.Title.Contains(chapterTitle) || chapterTitle.Contains(info.Title))
                {
                    ScrollToRect(info.BlockRect, onFinished);
                    return;
                }
            }

            Debug.LogWarning($"[EasyDocument] 未找到章节:{chapterTitle}");
            onFinished?.Invoke();
        }

        /// <summary>
        /// 停止当前滚动并跳转到指定章节块
        /// </summary>
        /// <param name="blockRect"></param>
        /// <param name="onFinished"></param>
        private void ScrollToRect(RectTransform blockRect, Action onFinished)
        {
            if (_scrollRect == null || blockRect == null)
            {
                onFinished?.Invoke();
                return;
            }

            if (_scrollCoroutine != null)
            {
                StopCoroutine(_scrollCoroutine);
                _scrollCoroutine = null;
            }

            float targetNormalized = CalculateTargetNormalized(blockRect);
            _scrollCoroutine = ScrollCoroutine(targetNormalized, onFinished);
            StartCoroutine(_scrollCoroutine);
        }

        /// <summary>
        /// 计算将章节块滚动到视口顶部所需的 verticalNormalizedPosition
        /// </summary>
        /// <param name="blockRect"></param>
        /// <returns></returns>
        private float CalculateTargetNormalized(RectTransform blockRect)
        {
            Canvas.ForceUpdateCanvases();

            RectTransform contentRect = _scrollRect.content;
            RectTransform viewportRect = _scrollRect.viewport;
            if (viewportRect == null)
            {
                viewportRect = this.transform as RectTransform;
            }

            Vector3 blockWorldPos = blockRect.position;
            Vector3 contentLocalPos = contentRect.InverseTransformPoint(blockWorldPos);

            float blockTop = contentLocalPos.y + blockRect.rect.height * (1.0f - blockRect.pivot.y);
            float contentTop = contentRect.rect.height * (1.0f - contentRect.pivot.y);
            float distanceFromTop = Mathf.Max(0.0f, contentTop - blockTop);

            float contentHeight = contentRect.rect.height;
            float viewportHeight = viewportRect.rect.height;
            float maxScroll = Mathf.Max(0.0f, contentHeight - viewportHeight);
            float targetScroll = Mathf.Min(distanceFromTop, maxScroll);

            if (maxScroll <= 0.0f) return 1.0f;
            return 1.0f - targetScroll / maxScroll;
        }

        /// <summary>
        /// 平滑滚动协程
        /// </summary>
        /// <param name="targetNormalized"></param>
        /// <param name="onFinished"></param>
        /// <returns></returns>
        private IEnumerator ScrollCoroutine(float targetNormalized, Action onFinished)
        {
            float startNormalized = _scrollRect.verticalNormalizedPosition;
            float duration = Mathf.Max(0.01f, _smoothDuration);
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float eased = ApplyEase(progress, _smoothType);
                _scrollRect.verticalNormalizedPosition = Mathf.Lerp(startNormalized, targetNormalized, eased);
                yield return null;
            }

            _scrollRect.verticalNormalizedPosition = targetNormalized;
            onFinished?.Invoke();
        }

        /// <summary>
        /// 应用缓动曲线
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="easeType"></param>
        /// <returns></returns>
        private static float ApplyEase(float progress, EnEaseType easeType)
        {
            switch (easeType)
            {
                case EnEaseType.EaseIn:
                    return progress * progress;
                case EnEaseType.EaseOut:
                    return 1.0f - (1.0f - progress) * (1.0f - progress);
                case EnEaseType.EaseInOut:
                    return progress < 0.5f
                        ? 2.0f * progress * progress
                        : 1.0f - Mathf.Pow(-2.0f * progress + 2.0f, 2.0f) / 2.0f;
                case EnEaseType.Linear:
                default:
                    return progress;
            }
        }

        #endregion
    }
}
