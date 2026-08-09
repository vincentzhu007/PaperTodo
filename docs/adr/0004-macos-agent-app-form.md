# ADR 0004: macOS app 形态——Agent app

- 状态：已决定

## 决策

mac 版为 **agent app（`LSUIElement = true`）**：无 Dock 图标、无顶部菜单栏，唯一入口是系统状态栏图标（对应 Windows 托盘）。开机自启用 `SMAppService` 登录项。

## 理由

- PaperTodo 的产品形态是"桌面上的几张纸 + 托盘入口"，Dock 常驻图标对 Mac 重度用户是干扰；原则是尽量不打扰用户工作流。
- agent app 没有"激活到前台"概念，纸片窗口的焦点行为更干净；Windows 版托盘菜单关闭后的焦点清理难题在此形态下大多不存在。
- 代价（无顶部菜单栏，设置入口全走状态栏菜单/纸片自身）与 Windows 版现状一致，无损失。
