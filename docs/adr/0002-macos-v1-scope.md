# ADR 0002: macOS v1 功能范围与数据兼容

- 状态：已决定

## 决策：v1 范围

**包含**：待办纸片（编辑交互保真：回车新建、多行粘贴一次撤销、拖拽排序、勾选、删除线样式）、折叠胶囊、贴边胶囊、状态栏图标入口、主题（跟随系统/浅/深）、四语言 resx 跟随系统、纸片窗口行为（无边框、置顶可选、拖动、缩放、关闭即隐藏）。

**明确推迟**：笔记与 markdown（含 `i:` 图片、LMDB、AvalonEdit）、插件体系、MCP、WebView2 web 纸片、提醒、全局热键、虚拟桌面适配、独立设置窗（状态栏菜单提供开机启动/主题/隐藏全部等勾选项）。

## 数据兼容硬约束

- mac v1 只渲染待办，但**必须无损往返整个 `data.json`**：`Paper.Type == "note"` 的纸片、`Content` 正文、`BodyProviderId`、插件缓存等字段原样读进写出。否则 Mac 上保存一次，Windows 端的笔记数据即被破坏。
- 启动解析失败不得以空状态覆盖旧文件（沿用上游保存纪律）。
- 数据位置：`~/Library/Application Support/PaperTodo/`。跨机迁移 = 手动拷贝 `data.json`（+ 未来的 `note-assets.lmdb`），不做导入功能、不做同步。
- `CapsuleMonitorDeviceName` 在 mac 端写入 macOS 自己的显示器标识；两端各认各的，互不解析。

## 后果

- v1 不需要编译 macOS 版 LMDB 原生库；该工作随笔记功能整体推迟。
- core 工程包含完整的 `PaperData` 模型（而非裁剪版），无损往返因此天然成立。
