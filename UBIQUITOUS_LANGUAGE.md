# Ubiquitous Language

PaperTodo 跨平台（Windows 上游 + macOS 移植）统一语言。代码是真相；本表钉死词汇在**两端**的含义。

## 纸片体系

| Term | Definition | Aliases to avoid |
|---|---|---|
| **纸片 (Paper)** | 桌面上的一个浮动窗口单元，是产品的一切载体 | 窗口、便签、便条 |
| **待办纸片 (Todo paper)** | `Type == "todo"` 的纸片，承载勾选项列表 | 任务列表、清单 |
| **笔记纸片 (Note paper)** | `Type == "note"` 的纸片，承载 markdown 正文（mac v1 不渲染但必须无损往返） | 文档、备忘 |
| **data.json** | 用户数据协议文件，含全部纸片与设置；不是内部缓存 | 存档、缓存、配置 |
| **删除 (Delete)** | 从 `Papers` 移除纸片，数据真正消失 | 关闭、清理 |
| **隐藏 (Hide)** | 纸片保留但不可见（`IsVisible = false`） | 关闭、最小化 |
| **折叠 (Collapse)** | 纸片仍可见，呈胶囊形态（`IsCollapsed = true`） | 最小化、收起 |

## 胶囊体系

| Term | Definition | Aliases to avoid |
|---|---|---|
| **胶囊 (Capsule)** | 纸片的胶囊形态 UI：图标 + 标题 + 关闭区的一条圆角条 | 胶囊按钮、标签 |
| **折叠胶囊 (Collapsed capsule)** | 桌面上自由浮动的胶囊，位置即纸片几何 | 普通胶囊、悬浮胶囊 |
| **贴边胶囊 (Edge capsule)** | 钉在某显示器某条屏边、垂直成队的胶囊；代码中历史命名为 `DeepCapsule*` | Deep capsule（见歧义节）、dock 胶囊 |
| **队列 (Queue)** | 同一 (显示器, 边) 上所有贴边胶囊组成的垂直栈 | 组、列 |
| **槽位 (Slot)** | 队列中的一个位置索引；slot 0 固定为主胶囊 | 格子、坑位 |
| **主胶囊 (Master pill)** | 队列 slot 0 的"折叠全部"胶囊，不参与水平伸缩 | 主控胶囊、队长 |
| **激发态 (Expanded-on-edge)** | 从贴边胶囊展开纸片后，胶囊仍显示并占槽位的持久外移状态 | 展开态、激活态 |
| **可见宽度 (Visible width)** | 贴边胶囊当前帧真实胶囊宽度；透明预留合成面不属于胶囊 | 宿主宽度、窗口宽度 |

## 平台翻译（macOS）

| Term | Definition | Aliases to avoid |
|---|---|---|
| **Space** | macOS 虚拟桌面；全屏 app 独占一个 Space | 虚拟桌面（仅指 Windows 概念时用）、workspace |
| **Zoom（最大化）** | 窗口放大填满当前 Space，仍是普通窗口；胶囊照常浮于其上 | 全屏（禁用此义） |
| **原生全屏 (Native fullscreen)** | 绿灯/Ctrl-Cmd-F 进入的独立 Space（含 Split View）；v1 胶囊在其中不出现 | 最大化 |
| **visibleFrame** | NSWindow 排除 Dock 与菜单栏后的可用屏幕区域；贴边胶囊贴它的边 | 工作区（Windows WorkArea 的对应物） |
| **Agent app** | `LSUIElement` 应用：无 Dock 图标无菜单栏，仅状态栏图标 | 后台 app、守护进程 |
| **状态栏图标 (Status item)** | macOS 右上角菜单栏图标，对应 Windows 托盘图标 | 托盘（mac 语境避免） |

## 工程结构

| Term | Definition | Aliases to avoid |
|---|---|---|
| **Core** | `PaperTodo.Core`：无平台 TFM 类库，含数据协议与胶囊大脑 | 共享层、公共库 |
| **外壳 (Shell)** | 平台专属工程：Windows 外壳（WPF）或 `PaperTodo.Mac`（Avalonia） | UI 层、前端 |
| **平台接缝 (Platform seam)** | core 中依赖平台能力的抽象点（工作区来源、显示器标识、窗口层级），各外壳实现 | 适配器（过宽）、桥 |
| **无损往返 (Lossless round-trip)** | mac 端读取并重新保存 `data.json` 时，未渲染的字段（笔记、插件数据）原样保留 | 兼容、容错 |

## Relationships

- 一张 **纸片** 是 **待办纸片** 或 **笔记纸片**（`Type` 判别），二者共用几何与胶囊语义。
- **折叠** 的纸片呈现为一个 **折叠胶囊**；加入 **队列** 的呈现为 **贴边胶囊** 并占据一个 **槽位**。
- 一个 **队列** 恰好有一个 **主胶囊**（slot 0）和零或多个真实纸片 **槽位**。
- **删除**、**隐藏**、**折叠** 是三种互斥语义，任何实现不得混用。
- **Core** 不含任何平台类型；**外壳** 通过 **平台接缝** 向 core 供给自己平台的现实（如 **visibleFrame**）。

## Example dialogue

> **Dev:** "mac 版把笔记纸片渲染成'不支持'提示行吗？"
> **Domain expert:** "不行。**笔记纸片**在 v1 不渲染，但 **data.json** 必须**无损往返**——它的 `Content` 要原样写回去，否则用户在 Mac 上保存一次，Windows 端的笔记就没了。"
> **Dev:** "那**贴边胶囊**贴在屏幕物理左边缘？"
> **Domain expert:** "贴 **visibleFrame** 的边。Dock 在左边时胶囊从 Dock 右侧起算，不和 Dock 抢边。"
> **Dev:** "用户把窗口 Zoom 到满屏，胶囊要躲吗？"
> **Domain expert:** "不用。**Zoom** 只是普通 Space 里的大窗口，胶囊照常浮在上面；只有进入**原生全屏**的独立 Space 才暂时不出现。"
> **Dev:** "这张纸用户点关闭，是**删除**吗？"
> **Domain expert:** "是**隐藏**。**删除**才会从 `Papers` 移除数据；**隐藏**和**折叠**都保留纸片。"

## Flagged ambiguities

- **"Deep capsule" vs "贴边胶囊"**：代码文件名与字段（`DeepCapsuleSlotWindow`、`DeepCapsuleExpandedX`）沿用旧名 "Deep"，而 AGENTS.md 与产品语言已统一为"贴边/Edge"。规范用语是**贴边胶囊 (Edge capsule)**；`DeepCapsule*` 仅为历史代码标识符，新代码（尤其 mac 外壳）一律用 Edge，不在文档和 UI 文案中 resurrect "Deep"。
- **"胶囊" 单独使用**时可能指折叠胶囊或贴边胶囊。两者共用同一套胶囊 UI 但生命周期完全不同（自由几何 vs 队列槽位），正式讨论中必须带限定词。
- **"全屏"**：中文语境常把 Zoom 最大化也叫全屏。本项目内**全屏**仅指 macOS 原生全屏 Space（或 Windows 的全屏前台窗口）；放大窗口一律说 **Zoom/最大化**。
- **"关闭"**：用户视角的"关掉一张纸"在数据语义上是**隐藏**而非**删除**，实现与文案中不得混用。
- **"工作区 (WorkArea)"**：Windows 的 `SystemParameters.WorkArea` 与 macOS 的 `visibleFrame` 是同一概念的两个平台实例，core 中统一称**工作区**，经**平台接缝**供给。
