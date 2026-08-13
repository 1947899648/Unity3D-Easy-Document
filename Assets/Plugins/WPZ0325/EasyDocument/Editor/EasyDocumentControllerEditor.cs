using System.Collections;
using UnityEditor;
using UnityEngine;

namespace WPZ0325.EasyDocument
{
    /// <summary>
    /// EasyDocumentController 编辑器面板：字段分“文档操作”与“配置”两组，并提供“生成文档”与“清空文档”按钮
    /// </summary>
    [CustomEditor(typeof(EasyDocumentController))]
    public class EasyDocumentControllerEditor : Editor
    {
        #region 序列化属性引用

        private SerializedProperty _settingProperty;
        private SerializedProperty _documentFolderNameProperty;
        private SerializedProperty _prefabBlockTextProperty;
        private SerializedProperty _prefabBlockImageProperty;
        private SerializedProperty _contentProperty;

        #endregion

        #region 生命周期

        private void OnEnable()
        {
            _settingProperty = serializedObject.FindProperty("_setting");
            _documentFolderNameProperty = serializedObject.FindProperty("_documentFolderName");
            _prefabBlockTextProperty = serializedObject.FindProperty("_prefabBlockText");
            _prefabBlockImageProperty = serializedObject.FindProperty("_prefabBlockImage");
            _contentProperty = serializedObject.FindProperty("_content");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDocumentGroup();
            DrawConfigGroup();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region 面板绘制

        /// <summary>
        /// 文档操作组：文档文件夹名 + 生成/清空按钮
        /// </summary>
        private void DrawDocumentGroup()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(_documentFolderNameProperty);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("生成文档"))
            {
                EasyDocumentController controller = (EasyDocumentController)target;
                IEnumerator enumerator = controller.GenerateInEditor();
                IterateCoroutine(enumerator);
            }
            if (GUILayout.Button("清空文档"))
            {
                EasyDocumentController controller = (EasyDocumentController)target;
                controller.ClearBlocks();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 配置组：样式设置、内容块 Prefab、内容挂载点（_content 必须手动指定）
        /// </summary>
        private void DrawConfigGroup()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(_settingProperty);
            EditorGUILayout.PropertyField(_prefabBlockTextProperty);
            EditorGUILayout.PropertyField(_prefabBlockImageProperty);
            EditorGUILayout.PropertyField(_contentProperty);
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region 协程工具

        /// <summary>
        /// 手动迭代协程，嵌套协程一并执行（编辑器非播放模式下无法使用 StartCoroutine）
        /// </summary>
        /// <param name="enumerator"></param>
        private static void IterateCoroutine(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                IEnumerator child = enumerator.Current as IEnumerator;
                if (child != null)
                {
                    IterateCoroutine(child);
                }
            }
        }

        #endregion
    }
}
