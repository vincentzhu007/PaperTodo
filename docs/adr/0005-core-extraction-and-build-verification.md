# ADR 0005: 核心抽取边界、工程结构与构建验证

- 状态：已决定

## 工程结构

```
PaperTodo/
├── PaperTodo.csproj     # Windows 外壳（现有，WPF/Win32，行为不变）
├── PaperTodo.Core/      # 新增，net10.0 无平台 TFM
└── PaperTodo.Mac/       # 新增，net10.0 + Avalonia + objc_msgSend 互操作
```

命名 `PaperTodo.Mac` 而非 `PaperTodo.Avalonia`：工程内含 mac 专属互操作，不假装通用外壳。

## Core 抽取边界（只搬 v1 需要的）

- 数据协议：`Models` / `StateStore` / `TodoRules` / `PaperTitles` / `Strings` + 四语言 resx（已验证零 WPF 依赖）。
- 胶囊大脑：`EdgeCapsule{Model, Reducer, TargetPlanner, Geometry, Presentation, TransitionPolicy, QueueCoordinator, HoverIntent, Layout}` + `ScreenGeometry`（已验证除 `EdgeCapsuleLayout` 的 `SystemParameters.WorkArea` 与少量 WPF 几何胶水外均为纯逻辑；`ScreenGeometry` 的 `DeviceScreenRect` 等原语本就平台无关）。
- 主题：颜色定义进 core，WPF/Avalonia 各自映射 Brush。
- 单实例：core 只定义 `show`/`exit` 语义；Windows 侧 Mutex + 参数转发不动，mac 侧用 named Mutex + Unix socket 实现同语义。
- **不搬**：笔记/LMDB/AvalonEdit、插件、MCP、提醒、虚拟桌面。

## 对 Windows 侧的侵入与验收

- 文件物理移入 `PaperTodo.Core/`；根 csproj `DefaultItemExcludes` 排除该目录（沿用 `PaperTodo.Plugin.Abstractions` 既有模式）。
- `EdgeCapsuleLayout`/`ScreenGeometry` 的 WPF 胶水改为平台接缝，Windows 侧实现接缝，逐行对应原逻辑。
- 验收门槛：Windows 版构建绿 + 贴边胶囊行为不变。Windows 侧改动只有"文件移动"与"平台接缝"两类，按功能边界拆提交，每个提交可独立审查回滚。

## 构建验证三层防线

1. 本地 macOS：`dotnet build PaperTodo.Core` / `PaperTodo.Mac` 全程可验证。
2. fork CI：`debug.yml` 已改为所有分支 push 均在 `windows-latest` 构建 WPF 外壳（含子模块与内置 LMDB DLL）。
3. 行为不变性：CI 只保编译绿；行为靠上述提交纪律 + 接缝代码审查保障。
