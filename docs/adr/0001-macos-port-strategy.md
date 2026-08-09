# ADR 0001: macOS 移植总体策略

- 状态：已决定（2026-08，grill-with-docs 访谈）
- 开发仓：`github.com/vincentzhu007/PaperTodo`（fork，origin）；上游 `snownico0722/PaperTodo`（upstream）

## 背景

PaperTodo 的 UI/外壳层完全是 Windows 专属：WPF + WinForms + WebView2 + WinRT（TFM `net10.0-windows`），贴边胶囊、托盘、全局热键、虚拟桌面等大量 Win32 互操作。"移植 macOS"本质是保留数据协议与领域逻辑、重写外壳层。

## 决策

1. **定位：先自用，后产品化**（"先 A 后 B"）。决策者是 Mac 重度用户，先做出自己每天用的版本；做得好再考虑合回上游或独立维护 mac 版给别人用。初期不背签名、公证、分发的包袱。
2. **技术栈：C# + Avalonia**，与上游同仓库同源（fork 开发）。mac 外壳为新的 Avalonia 工程，WPF 外壳不动。
3. **保留合流可能**：因为上游是 C# 工程，只有 mac 版同为 C#，"合回上游"才是真实的合并；这排除了原生 Swift 重写（那等于独立产品）。
4. **不用 Avalonia 替换 WPF**：两端不共用 UI 层，Windows 版外壳保持原样，避免第一步就触碰最高风险区。

## 后果

- 全局热键、状态栏图标、桌面层级窗口等 macOS 平台能力需要自行编写原生互操作（objc_msgSend P/Invoke），与 Windows 侧写 Win32 interop 同理，无需编译原生 dylib。
- fork 与 upstream 通过 git remote 保持双向同步路径；合流形式为"核心抽取重构 + 新增工程"的增量提交。
- 验证依赖 CI（见 ADR 0005）；开发者本机只有 macOS 环境。
