using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace WPZ0325.EasyDocument
{
    /// <summary>
    /// EasyDocument 数据加载句柄，双通道读取：Editor/PC 使用 File 同步读取，Android 使用 UnityWebRequest 异步读取
    /// </summary>
    public static class EasyDocumentHandler
    {
        #region 属性

        /// <summary>
        /// StreamingAssets 下 EasyDocumentData 根目录
        /// </summary>
        public static string RootPath
        {
            get
            {
                string rootPath = Path.Combine(Application.streamingAssetsPath, "EasyDocumentData");
                return rootPath;
            }
        }

        /// <summary>
        /// 是否为 Android 平台（Android 无法用 File 直接读 StreamingAssets）
        /// </summary>
        private static bool IsAndroid
        {
            get
            {
                return Application.platform == RuntimePlatform.Android;
            }
        }

        #endregion

        #region 数据加载

        /// <summary>
        /// 加载文档 json（协程），folderName 为 EasyDocumentData 下的文档文件夹名
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="onLoaded">加载完成后回调，失败时返回 null</param>
        /// <returns></returns>
        public static IEnumerator LoadDocumentJson(string folderName, Action<EasyDocumentDataModel> onLoaded)
        {
            string jsonPath = Path.Combine(Path.Combine(RootPath, folderName), "document.json");
            string json = null;

            if (IsAndroid)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(jsonPath))
                {
                    yield return request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        json = request.downloadHandler.text;
                    }
                    else
                    {
                        Debug.LogError($"[EasyDocument] 加载文档json失败:{jsonPath} ,error:{request.error}");
                    }
                }
            }
            else
            {
                if (File.Exists(jsonPath))
                {
                    json = File.ReadAllText(jsonPath, Encoding.UTF8);
                }
                else
                {
                    Debug.LogError($"[EasyDocument] 文档json不存在:{jsonPath}");
                }
                yield return null;
            }

            EasyDocumentDataModel data = EasyDocumentJsonTool.JsonToObject(json);
            onLoaded?.Invoke(data);
        }

        /// <summary>
        /// 加载文档图片（协程），relativePath 为相对文档文件夹的路径（如 images/xxx.png）
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="relativePath"></param>
        /// <param name="onLoaded">加载完成后回调，失败时返回 null</param>
        /// <returns></returns>
        public static IEnumerator LoadDocumentImage(string folderName, string relativePath, Action<Texture2D> onLoaded)
        {
            Texture2D result = null;
            string fullPath = Path.Combine(Path.Combine(RootPath, folderName), relativePath);

            if (IsAndroid)
            {
                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullPath))
                {
                    yield return request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        DownloadHandlerTexture handler = (DownloadHandlerTexture)request.downloadHandler;
                        result = handler.texture;
                    }
                    else
                    {
                        Debug.LogError($"[EasyDocument] 加载文档图片失败:{fullPath} ,error:{request.error}");
                    }
                }
            }
            else
            {
                if (File.Exists(fullPath))
                {
                    byte[] bytes = File.ReadAllBytes(fullPath);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(bytes))
                    {
                        result = tex;
                    }
                    else
                    {
                        DestroyTexture(tex);
                        Debug.LogError($"[EasyDocument] 图片解码失败:{fullPath}");
                    }
                }
                else
                {
                    Debug.LogError($"[EasyDocument] 图片不存在:{fullPath}");
                }
                yield return null;
            }

            onLoaded?.Invoke(result);
        }

        #endregion

        #region 纹理管理

        /// <summary>
        /// 销毁运行时创建的纹理：运行时用 Destroy，编辑器下用 DestroyImmediate
        /// </summary>
        /// <param name="texture"></param>
        public static void DestroyTexture(Texture2D texture)
        {
            if (texture == null) return;
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(texture);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        #endregion
    }
}
