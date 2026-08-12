using System;

namespace WPZ0325.EasyDocument
{
    /// <summary>
    /// Json 工具类，默认使用 UnityEngine.JsonUtility
    /// </summary>
    public static class EasyDocumentJsonTool
    {
        /// <summary>
        /// Json 字符串反序列化为文档数据模型
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static EasyDocumentDataModel JsonToObject(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                EasyDocumentDataModel data = UnityEngine.JsonUtility.FromJson<EasyDocumentDataModel>(json);
                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 文档数据模型序列化为 Json 字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="prettyPrint"></param>
        /// <returns></returns>
        public static string ObjectToJson(EasyDocumentDataModel data, bool prettyPrint = true)
        {
            if (data == null) return string.Empty;
            string json = UnityEngine.JsonUtility.ToJson(data, prettyPrint);
            return json;
        }
    }
}
