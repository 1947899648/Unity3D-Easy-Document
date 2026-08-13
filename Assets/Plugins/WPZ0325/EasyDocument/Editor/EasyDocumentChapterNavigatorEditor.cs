using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WPZ0325.EasyDocument
{
    /// <summary>
    /// EasyDocumentChapterNavigator 编辑器面板：滚动参数配置 + 当前文档章节层级结构可视化
    /// </summary>
    [CustomEditor(typeof(EasyDocumentChapterNavigator))]
    public class EasyDocumentChapterNavigatorEditor : Editor
    {
        #region 序列化属性引用

        private SerializedProperty _controllerProperty;
        private SerializedProperty _smoothDurationProperty;
        private SerializedProperty _smoothTypeProperty;

        #endregion

        #region 生命周期

        private int _lastChapterCount = -1;

        private void OnEnable()
        {
            _controllerProperty = serializedObject.FindProperty("_controller");
            _smoothDurationProperty = serializedObject.FindProperty("_smoothDuration");
            _smoothTypeProperty = serializedObject.FindProperty("_smoothType");

            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        /// <summary>
        /// 每帧检测章节数量变化，文档重新生成后自动重绘面板
        /// </summary>
        private void OnEditorUpdate()
        {
            if (target == null) return;
            EasyDocumentChapterNavigator navigator = (EasyDocumentChapterNavigator)target;
            int count = navigator.Chapters.Count;
            if (count != _lastChapterCount)
            {
                _lastChapterCount = count;
                Repaint();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesGroup();
            DrawChapterTreeGroup();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region 面板绘制

        /// <summary>
        /// 滚动参数组：Controller 引用 + 时长 + 缓动类型
        /// </summary>
        private void DrawPropertiesGroup()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(_controllerProperty);
            EditorGUILayout.PropertyField(_smoothDurationProperty);
            EditorGUILayout.PropertyField(_smoothTypeProperty);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 章节表格组：复制按钮 + 章节表格（章节级别 / Index / 标题 / 章节按钮）
        /// 索引自动刷新：OnEnable 无条件刷新 + Controller 生成/清理事件联动，无需手动按钮
        /// </summary>
        private void DrawChapterTreeGroup()
        {
            EasyDocumentChapterNavigator navigator = (EasyDocumentChapterNavigator)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("章节索引", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            if (GUILayout.Button("复制调用代码"))
            {
                CopyCallCodeToClipboard(navigator);
            }

            IReadOnlyList<EasyDocumentChapterNavigator.ChapterInfo> chapters = navigator.Chapters;
            if (chapters == null || chapters.Count == 0)
            {
                EditorGUILayout.HelpBox("Content 下暂无章节内容，请先生成文档", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            DrawChapterTable(chapters);

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制章节表格：表头（章节/Index/string/章节按钮）+ 数据行（含按钮拖放槽）
        /// </summary>
        /// <param name="chapters"></param>
        private void DrawChapterTable(IReadOnlyList<EasyDocumentChapterNavigator.ChapterInfo> chapters)
        {
            SerializedProperty bindingsProperty = serializedObject.FindProperty("_chapterButtonBindings");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("章节", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.LabelField("Index", EditorStyles.boldLabel, GUILayout.Width(40));
            EditorGUILayout.LabelField("string", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("章节按钮", EditorStyles.boldLabel, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < chapters.Count; i++)
            {
                EasyDocumentChapterNavigator.ChapterInfo info = chapters[i];
                SerializedProperty buttonRefProperty = null;
                if (bindingsProperty != null && i < bindingsProperty.arraySize)
                {
                    SerializedProperty element = bindingsProperty.GetArrayElementAtIndex(i);
                    buttonRefProperty = element.FindPropertyRelative("ButtonRef");
                }
                DrawChapterRow(i, info, buttonRefProperty);
            }
        }

        /// <summary>
        /// 绘制章节表格行（交替底色便于阅读，含按钮拖放槽）
        /// </summary>
        /// <param name="index"></param>
        /// <param name="info"></param>
        /// <param name="buttonRefProperty">按钮引用序列化属性（可能为 null）</param>
        private static void DrawChapterRow(int index, EasyDocumentChapterNavigator.ChapterInfo info, SerializedProperty buttonRefProperty)
        {
            if (index % 2 == 0)
            {
                GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 0.15f);
                EditorGUILayout.BeginHorizontal("Box");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
            }

            EditorGUILayout.LabelField(info.Level.ToString(), GUILayout.Width(60));
            EditorGUILayout.LabelField(index.ToString(), GUILayout.Width(40));
            EditorGUILayout.LabelField(info.Title, EditorStyles.label);

            if (buttonRefProperty != null)
            {
                EditorGUILayout.PropertyField(buttonRefProperty, GUIContent.none, GUILayout.Width(150));
            }
            else
            {
                EditorGUILayout.LabelField("-", GUILayout.Width(150));
            }

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// 复制章节调用代码到剪贴板（ScrollToChapter 序号 + ScrollToChapterByTitle 标题 两套）
        /// </summary>
        /// <param name="navigator"></param>
        private static void CopyCallCodeToClipboard(EasyDocumentChapterNavigator navigator)
        {
            IReadOnlyList<EasyDocumentChapterNavigator.ChapterInfo> chapters = navigator.Chapters;
            if (chapters == null || chapters.Count == 0)
            {
                EditorUtility.DisplayDialog("复制调用代码", "暂无章节内容，请先刷新章节索引", "确定");
                return;
            }

            StringBuilder builder = new StringBuilder();

            builder.AppendLine("// 按章节序号滚动（index 从 0 开始）");
            for (int i = 0; i < chapters.Count; i++)
            {
                EasyDocumentChapterNavigator.ChapterInfo info = chapters[i];
                builder.AppendLine($"navigator.ScrollToChapter({i}); // [{info.Level}] {info.Title}");
            }

            builder.AppendLine();
            builder.AppendLine("// 按标题模糊匹配滚动");
            for (int i = 0; i < chapters.Count; i++)
            {
                EasyDocumentChapterNavigator.ChapterInfo info = chapters[i];
                builder.AppendLine($"navigator.ScrollToChapterByTitle(\"{info.Title}\");");
            }

            GUIUtility.systemCopyBuffer = builder.ToString();
            EditorUtility.DisplayDialog("复制调用代码", "已复制到剪贴板", "确定");
        }

        #endregion
    }
}
