---
type: agent-handoff
status: active
updated: 2026-08-28
---

# 当前交接卡

## 当前目标

评估 ArrowsGame 的现有功能与手感，规划微信小游戏上线所需的兼容、性能、商业化和可玩性迭代。

## 已确认决策

- 项目基于 Unity UnitFramework，代码主要位于 `Project/Assets/Scripts/Framework/` 和 `Project/Assets/Scripts/Game/`。
- 项目已集成 UniTask，异步逻辑遵循 `.agents/skills/unity-unitask/SKILL.md`。
- 项目已安装 Unity Test Framework，测试遵循 `.agents/skills/unity-testing/SKILL.md`。
- AI 默认不执行 `git commit` 或 `git push`，只有用户明确要求时才执行。

## 当前状态

- 远端仓库为 `https://github.com/Wakis-kaf/ArrowsGame.git`，仓库为私有。
- `.agents/skills/` 已包含 `unity-unitask` 和 `unity-testing`。
- 已使用 `codebase-memory-mcp 0.10.8` 完成索引，生成数据库 `Datebases/code-mermory-base/D-learn-UnityGame-ArrowsGame.db`（44,574 节点、159,079 条边）；后续优先通过 MCP 读取并在代码关系变化后重新索引同步。
- `index_status` 已验证索引状态为 `ready`；当前有 66 个文件存在解析不完整区间，涉及部分第三方/生成/存档文件，查询这些文件时需辅以源码搜索。
- 已完成一次微信小游戏上线与玩法体验分析；结论尚未转化为代码改造任务。
- 已确认下一阶段需求：主线宝箱/金币奖励、三种局内道具、错误点击闪红与可关闭震动、统一可开关的多平台广告中间层；详细计划见 `.agents/plans/item-reward-ad-feel-plan.md`。
- 已开始实现 Progression 基础能力：`ArchiveProgression` 持久化金币、三种局内道具、成就进度和宝箱领取状态；`GameProgressionService` 提供通关奖励和道具 API；`GameAdService` 提供统一激励广告中间层；`GameFeedbackService` 接入错误点击消息和设置控制的震动。
- `LevelVO` 通关记录已调用奖励服务，错误箭头点击已触发统一反馈。
- 道具数据源已统一回归策划导表：`Cehua/Excel/道具配置.xlsx` 的“道具基础配置”新增撤销、清除、提示（20101001-20101003），并同步导出至 `Assets/AddressableResources/Configs/cfg_items.json`；图标字段暂为空，等待美术资源。
- `Cehua/Excel/GlobalConfig.json` 的导表输入和输出路径已修正为当前 `D:\learn\UnityGame\ArrowsGame` 目录。
- 错误点击反馈已改为 Built-in 管线的相机后处理：`ArrowsGameStage` 在显示时为关卡相机挂载 `ArrowsWrongClickPostProcess`，触发消息后以 `ArrowsWrongClickPostProcess.shader` 渲染短促红色边缘；已移除 UI Image 方案。
- 局内 HUD 已接入金币、当前关卡生命和三种道具：`PlayGamePanel` 显示提示、撤销、清除的库存；道具库存优先消耗，库存不足时走广告中间层或 30 金币兜底。提示高亮可解点，清除自动移除可解箭头，撤销恢复最近一次成功移除。
- 下一关切换的 `GameArrowsLoadingOption` 已设置最短显示 0.2 秒；`GameLoading` 以 `minDisplayTime` 通用字段和非缩放时间延迟关闭，消除缓存命中时 Loading 的闪帧。
- 已在 `Project/Packages/manifest.json` 添加 MCP for Unity 9.7.3，Unity 已解析包缓存并编译 `MCPForUnity.Runtime.dll`；仓库 `.mcp.json` 与 Codex 用户配置都已注册 `unityMCP` HTTP 端点 `http://127.0.0.1:8080/mcp`。
- 当前 MCP CLI 因 Windows `C:\Users` ACL 安全校验无法创建协调端点，尚未完成本次代码变更后的重新索引；需修复运行环境后执行 `index_repository`。
- Unity 2022.3.62f2 批处理验证因项目已被另一 Unity 实例锁定而未执行到脚本编译；需关闭编辑器后重试。

## 下一步

1. 使用美术资源绑定局内道具栏、获取弹窗、宝箱弹窗和奖励展示。
2. 将 `cfg_progression.json` 接入现有配置加载链路，并实现宝箱随机奖励、金币购买和广告奖励。
3. 在 Unity 的 `Tools > MCP for Unity` 启动 Bridge，确认本地 8080 端口监听后重启 Codex 以加载 `unityMCP`，并验证运行时红色边缘效果。
4. 修复 MCP CLI ACL 后重新索引同步 `Datebases/code-mermory-base/`。

## 已知限制

- Unity 编辑器运行和完整构建验证需要本机 Unity 环境；未运行时不得声称已通过。
- MCP for Unity 包已解析，但其 HTTP Bridge 当前未监听 8080；该包需要在 Unity 窗口中启动 Bridge（或启用 Auto-Start on Editor Load）后才能完成端到端连接。
- 资源、配置和生成文件可能由 Unity 或导表工具维护，修改前需确认来源。

## 更新规则

- 本卡保持短小，只记录当前工作所需事实，目标长度不超过 200 行。
- 完成独立阶段、连续两次上下文压缩、准备新对话或用户改变目标时必须更新。
- 稳定决策写入 `.agents/knowledge/decisions/`，详细过程写入 `.agents/plans/`，不要复制到本卡。
