# Unity3D-Easy-Document

基于 TMP（TextMeshPro）的 Unity UI 文档浏览插件。以 StreamingAssets 下的文件夹为文档壳，支持多级标题、正文、图片与图片标题的展示，布局自适应，样式由 ScriptableObject 统一配置。

## 功能特性

- 支持 1~4 级标题、正文、图片、图片标题六种元素类型
- 图片显示尺寸可在 json 中按像素指定，未指定时按比例自适应
- 文档数据以文件夹为壳（json + 图片资源），便于外部维护与热替换
- 双通道加载：Editor/PC 使用 File 同步读取，Android 使用 UnityWebRequest 异步读取
- 样式统一配置（颜色、字号、字重、对齐、SDF 字体），运行时生效
- 布局自适应：文本块按内容撑高，图片按宽高比缩放，Content 高度随内容增长

## 目录结构

```
Assets/
├── Plugins/WPZ0325/EasyDocument/     # 插件本体
│   ├── Scripts/                      # 核心脚本
│   │   └── EasyDocumentElement/      # 内容块元素脚本
│   └── Prefabs/                      # Scroll View 整件 + 内容块预制体
├── Demo/                             # 演示场景与样式配置资源
└── StreamingAssets/EasyDocumentData/ # 文档数据目录（<文档名>/document.json + 图片）
```

## 文档数据格式

文档存放于 `StreamingAssets/EasyDocumentData/` 下，每个文档一个文件夹，文件夹名即文档名：

```
EasyDocumentData/
└── 我的文档/
    ├── document.json
    └── images/xxx.png
```

document.json 结构：

```json
{
    "DocumentName": "我的文档",
    "Elements": [
        { "Type": "TITLE_1", "Text": "第一章 简介" },
        { "Type": "BODY", "Text": "正文内容……" },
        { "Type": "IMAGE", "Text": "", "ImagePath": "images/xxx.png",
          "Caption": "图1 示例", "ImageWidth": 800, "ImageHeight": 480 }
    ]
}
```

| 字段 | 说明 |
|---|---|
| `DocumentName` | 文档名称 |
| `Type` | 元素类型：`TITLE_1` / `TITLE_2` / `TITLE_3` / `TITLE_4` / `BODY` / `IMAGE` |
| `Text` | 文本内容（IMAGE 类型可为空） |
| `ImagePath` | 图片相对本文档文件夹的路径（IMAGE 类型使用） |
| `Caption` | 图片标题（IMAGE 类型的子字段） |
| `ImageWidth` / `ImageHeight` | 图片显示宽高（像素），`>0` 生效；`0`/省略时按比例自适应 |

## 快速开始

1. 在 `StreamingAssets/EasyDocumentData/` 下创建文档文件夹（含 document.json 与图片资源）
2. 创建样式配置：菜单 `Assets > Create > WPZ0325 > Create SO > EasyDocument > EasyDocumentSetting`，可选拖入 SDF 字体
3. 将 `Scroll View-EasyDocument` 预制体拖入场景（或挂载 `EasyDocumentController` 到任意 ScrollRect 物体上）
4. Inspector 配置：样式资源 `_setting`、文档文件夹名 `_documentFolderName`、内容块预制体
5. 运行时调用：

```csharp
EasyDocumentController controller = GetComponent<EasyDocumentController>();
controller.Init("我的文档");
```

> 提示：Inspector 中右键组件，点击 `InitByInspector` 可直接加载预览。

## 样式配置说明

| 配置项 | 说明 |
|---|---|
| `FontAsset` | SDF 字体（TMP_FontAsset），留空回退 TMP 默认字体 |
| `BlockSpacing` / `ContentPadding` | 元素间距与内容内边距 |
| `TextBlockPaddingX` | 文本块内文字与块左右两侧的距离（像素） |
| `ImageMaxWidthRatio` | 图片最大宽度占内容宽度比例（未指定宽高时生效） |
| 各级标题/正文/图片标题 | 颜色、字号、字重、对齐方式 |

## 注意事项

- 中文内容需要拖入支持中文的 SDF 字体（在 TMP Font Asset Creator 中用微软雅黑等字体生成）
- 修改文档数据后，重新调用 `Init` 即可刷新内容
- 图片块预制体的 `VerticalLayoutGroup` 采用子元素自撑高布局，请勿将 `Child Control Height` 打开
