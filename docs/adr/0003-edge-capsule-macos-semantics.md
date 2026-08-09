# ADR 0003: 贴边胶囊的 macOS 语义映射

- 状态：已决定

贴边胶囊进入 v1，是 mac 外壳架构的出发点。Windows 版语义逐条"翻译"如下：

| Windows 语义 | macOS 译法 |
|---|---|
| 独占 HWND 钉在物理屏边 | NSWindow 设为浮动 window level，无边框、不进 Mission Control |
| 属于某显示器某条边，垂直成队 | 贴 **visibleFrame 的边**（自动避让 Dock 与菜单栏），不贴物理边缘 |
| Windows 虚拟桌面适配 | 胶囊声明 `canJoinAllSpaces`，**所有 Space 可见**；普通纸片窗口跟随所在 Space |
| 全屏前台检测规避 | 原生全屏（含 Split View）是独立 Space，v1 不抵抗：全屏 Space 内胶囊不出现，返回普通 Space 自动恢复。**Zoom/最大化不是全屏**：窗口仍处普通 Space，胶囊照常浮于其上，无需处理 |
| 悬停展开、关闭区从墙边推出 | 纯自绘，保真 |
| 跨队列拖动（floating drag HWND） | 拖动即普通可移动 NSWindow，保真且实现更简单 |

外形与交互保真；"贴哪、在哪些 Space 出现"按 Mac 习惯翻译。决策者 Dock 位于底部且不自动隐藏，"给 Dock 让位"以 visibleFrame 自然达成。

## Spike 验证结果（2026-08，macOS 27，双屏实测）

最小 Avalonia 验证工程（`~/Code/bigwork/mac-capsule-spike`，非交付代码）实测通过：

1. `NSFloatingWindowLevel(3)` 窗口可浮于 Zoom 满屏的普通窗口之上 ✅
2. `canJoinAllSpaces|stationary|ignoresCycle` 使窗口跟随所有 Space ✅
3. 原生全屏 Space 中该窗口不出现，返回普通 Space 自动恢复——与上表决策一致 ✅
4. 按 Avalonia `WorkingArea` 钉边可精确贴 visibleFrame 边缘并避让 Dock ✅
5. `WorkingArea` 实测：两块屏均排除菜单栏；Dock 未隐藏时只从其当前所在屏扣除（内建屏底部扣 63 DIP），Dock 换屏时自动跟随——贴边逻辑无需自行探测 Dock ✅
6. ⚠️ 裸 `dotnet` 进程（无 .app bundle）被 macOS 视为低分辨率应用，`Scaling` 恒报 1、渲染模糊。正式版与日常自用版都必须打包为 .app（Info.plist `NSHighResolutionCapable`）；物理像素 = DIP × backing scale 的换算需在 bundle 形态下复核。

## 后果

- 胶囊窗口的 NSWindow 互操作需求收敛为：window level、collectionBehavior（canJoinAllSpaces / stationary / ignoresCycle）、visibleFrame 读取、物理指针采样。
- 不做"全屏 Space 内也显示胶囊"（需要极高 window level，违背 Mac 用户预期）。
- 上游 AGENTS.md 中贴边胶囊的各条不变量（planner 一次产出完整 shape plan、geometry 纯函数转换、applied frame 判定悬停等）在 mac 外壳继续成立，平台无关部分进 core 复用（见 ADR 0005）。
