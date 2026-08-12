using System;
using System.Collections.Generic;

namespace WPZ0325.EasyDocument
{
    /// <summary>
    /// EasyDocument 文档数据根模型，对应 document.json 文件
    /// </summary>
    [Serializable]
    public class EasyDocumentDataModel
    {
        /// <summary>
        /// 文档名称
        /// </summary>
        public string DocumentName;

        /// <summary>
        /// 文档元素列表（JsonUtility 不支持顶层 List，故挂在根对象下）
        /// </summary>
        public List<EasyDocumentElementDataModel> Elements = new List<EasyDocumentElementDataModel>();
    }

    /// <summary>
    /// EasyDocument 文档元素数据模型
    /// </summary>
    [Serializable]
    public class EasyDocumentElementDataModel
    {
    /// <summary>
    /// 元素类型，json 中以字符串记录（如 TITLE_1/BODY/IMAGE），运行时解析为 EnEasyDocumentElementType
    /// </summary>
    public string Type;

        /// <summary>
        /// 文本内容（文字类型使用；IMAGE 类型可为空）
        /// </summary>
        public string Text;

        /// <summary>
        /// 图片相对本文档文件夹的路径（IMAGE 类型使用，如 images/xxx.png）
        /// </summary>
        public string ImagePath;

        /// <summary>
        /// 图片标题（IMAGE 类型的子字段，样式独立配置）
        /// </summary>
        public string Caption;

        /// <summary>
        /// 图片显示宽度（像素单位），>0 时生效；0 或省略时按 ImageMaxWidthRatio 比例自适应
        /// </summary>
        public float ImageWidth;

        /// <summary>
        /// 图片显示高度（像素单位），>0 时生效；0 或省略时按图片宽高比自动计算
        /// </summary>
        public float ImageHeight;
    }
}
