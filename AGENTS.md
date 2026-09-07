# ArrowsGame AI 工作规范

本文件是本仓库中 AI Agent 的强制工作约束。开始任务前必须先阅读本文件，并按需读取 `Datebases/code-mermory-base/` 与 `.agents/handoff/CURRENT.md`。

## 代码知识图谱优先级

- 若 `Datebases/code-mermory-base/` 中存在 `codebase-memory-mcp` 生成的数据库，开始代码分析或修改前必须优先通过 MCP 读取该图谱，再定位源码。
- MCP 配置位于仓库根目录 `.mcp.json`；首次使用先执行 `list_projects`/`index_status`，确认项目索引后再查询。
- 修改代码后，必须检查图谱描述的模块、入口、依赖或流程是否受到影响；受到影响时在同一任务内同步更新图谱的 `updated` 日期和相关节点。
- 图谱是导航和关系索引，不是源码事实的替代品；发现图谱与源码不一致时以源码为准，并在同一任务结束前重新运行 MCP 索引，将数据库同步到 `Datebases/code-mermory-base/`。

## 交接与上下文

- `.agents/handoff/CURRENT.md` 是当前工作的唯一交接入口。
- 开始新对话、连续两次上下文压缩、完成独立阶段或用户改变目标时，先更新交接卡。
- 交接卡只记录当前目标、已确认事实、当前状态、下一步、风险和精确文件入口；详细过程写入 `.agents/plans/`，稳定决策写入 `.agents/knowledge/decisions/`。
- 不默认读取整个知识库或旧对话记录，只加载当前任务所需的文件。

## 修改与 Git

- 先检查工作区状态，保留用户已有修改，不回滚无关变更。
- 修改范围应紧贴用户请求，遵循现有 Framework + Game 架构和命名风格。
- 默认只修改文件并报告 `git status`/`git diff`；未经用户明确要求，不执行 `git commit`、`git push`、重置或覆盖远端历史。
- 删除、覆盖或批量移动前必须确认目标范围，并优先采用可恢复方式。

## Unity 开发

- 异步代码遵循 `.agents/skills/unity-unitask/SKILL.md`。
- 测试相关任务遵循 `.agents/skills/unity-testing/SKILL.md`。
- 优先复用项目现有框架、模块、配置导表和资源工作流，不引入未经确认的新基础设施。
- 完成代码修改后按风险运行针对性的验证，并明确记录未执行的验证及原因。
- 代码关系发生变化时，验证结果应同时记录在交接卡或对应计划中。
- MCP 全量索引必须串行执行；优先复用已有索引，仅在代码实际变化或图谱过期时重新运行 `index_repository`。

## 交付

- 最终说明修改内容、验证结果、未解决风险和工作区是否干净。
- 若用户明确要求提交或推送，再执行对应 Git 操作并报告提交号或远端结果。
