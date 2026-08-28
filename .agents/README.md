# ArrowsGame AI Workspace

`.agents` 是项目内 AI Agent 共用的工作区。

- `skills/`：可复用的技术工作流和技能。
- `knowledge/`：项目事实、架构决策和长期有效知识。
- `../Datebases/code-mermory-base/`：`codebase-memory-mcp` 生成的代码知识图谱数据库；代码任务优先通过 MCP 读取，并在关系变化后重新索引同步。
- `.mcp.json`：项目级 `codebase-memory` MCP 连接配置。
- `handoff/`：新对话或上下文压缩后的当前状态交接卡。
- `plans/`：具体任务的详细实施计划。

开始工作前阅读仓库根目录的 `AGENTS.md`，再根据任务需要读取 `.agents/handoff/CURRENT.md` 和知识库中的相关文件。

代码修改后检查 `../Datebases/code-mermory-base/` 中的 MCP 图谱是否需要同步；不要把源码细节、任务计划或未经确认的推测混入图谱。
