# Convert All Tools to McpServerTool Pattern

## Understanding
Convert all 43+ tool classes from the `AITool` base class pattern (`Microsoft.Extensions.AI`) to the MCP SDK's `[McpServerToolType]` / `[McpServerTool]` attribute pattern (`ModelContextProtocol`). Replace custom `ToolResult` with SDK's `CallToolResult`. Annotate all read-only tools with `ReadOnly = true, Destructive = false`. Use `WithToolsFromAssembly()` for auto-discovery.

## Key Decisions
- **Return type**: `CallToolResult` for all methods (needed for `IsError = true` on failure)
- **Async**: Methods stay synchronous where possible (MCP SDK supports both sync/async)
- **Error pattern**: Return `CallToolResult` with `IsError = true` for failures, never throw
- **Content**: Use `TextContentBlock` for string content in `CallToolResult`
- **Discovery**: `[McpServerToolType]` on every class + `.WithToolsFromAssembly()` in Program.cs
- **Delete**: Remove `ToolResult.cs` after all references are removed

## Key Files
- `SentinelCore-MCP/Tools/ScheduledTaskReadTool.cs` — prototype for conversion pattern
- `SentinelCore-MCP/Program.cs` — update tool registration
- `SentinelCore-MCP/Tools/ToolResult.cs` — to be deleted
- All 43 tool files — bulk conversion

## Risks & Open Questions
- Exact `CallToolResult` constructor/API depends on ModelContextProtocol v1.2.0 package — need to verify namespaces and types at build time
- Some tools may have complex constructor injection (DI) — need to check if any tools use it

## Steps
1. Prototype conversion on ScheduledTaskReadTool.cs — establish the exact pattern
2. Verify the prototype compiles against ModelContextProtocol v1.2.0
3. Bulk-convert remaining tool files (usings, class attributes, method attributes, return types, error handling)
4. Update Program.cs to use WithToolsFromAssembly()
5. Remove ToolResult.cs
6. Build and verify the full solution