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
- 当前没有已登记的进行中改造任务。

## 下一步

1. 根据用户指定的玩法目标定位相关模块和场景。
2. 优先通过 MCP 读取 `Datebases/code-mermory-base/` 中的代码图谱，再阅读匹配的知识、技能和现有代码，进行最小范围修改。
3. 修改后重新运行 `index_repository`，将最新 MCP 数据库同步到 `Datebases/code-mermory-base/`，并执行针对性 Unity 验证。
4. 更新本卡的状态和风险。

## 已知限制

- Unity 编辑器运行和完整构建验证需要本机 Unity 环境；未运行时不得声称已通过。
- 资源、配置和生成文件可能由 Unity 或导表工具维护，修改前需确认来源。

## 更新规则

- 本卡保持短小，只记录当前工作所需事实，目标长度不超过 200 行。
- 完成独立阶段、连续两次上下文压缩、准备新对话或用户改变目标时必须更新。
- 稳定决策写入 `.agents/knowledge/decisions/`，详细过程写入 `.agents/plans/`，不要复制到本卡。
