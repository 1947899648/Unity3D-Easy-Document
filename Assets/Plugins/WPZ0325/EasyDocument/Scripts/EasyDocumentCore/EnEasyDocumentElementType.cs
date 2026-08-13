namespace WPZ0325.EasyDocument
{
    /// <summary>
    /// EasyDocument 元素类型枚举，需与 document.json 中 type 字段相互对应
    /// </summary>
    public enum EnEasyDocumentElementType
    {
        NONE = 0,
        //文字类型
        BODY = 10,
        TITLE_1 = 11,
        TITLE_2 = 12,
        TITLE_3 = 13,
        TITLE_4 = 14,
        //资源类型
        IMAGE = 100,
    }
}
