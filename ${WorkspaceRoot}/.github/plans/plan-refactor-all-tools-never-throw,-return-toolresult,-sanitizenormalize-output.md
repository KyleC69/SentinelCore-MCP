# Refactor All Tools: Never Throw, Return ToolResult, Sanitize/Normalize Output

## Understanding
Analyze all ~40 tool files in the project for violations of three rules:
1. **Tools must never throw** — any unhandled exception paths need try/catch wrapping
2. **Tools must return ToolResult** — all return types must be ToolResult
3. **All results must be sanitized and normalized** — no raw exception messages, no unsanitized paths/data, consistent formatting

## Approach
1. First read ToolResult.cs to understand the contract
2. Use subagent to scan all tool files for violations
3. Categorize violations and apply systematic fixes across all files
4. Verify build succeeds

## Key Files
- All `SentinelCore-MCP\Tools\*.cs` files
- `SentinelCore-MCP\Tools\ToolResult.cs` — the result contract
- `SentinelCore-MCP\SentinelCore-MCP.csproj` — package references

## Steps
1. Read ToolResult.cs and understand the API
2. Scan all tool files for rule violations (throwing, missing try/catch, raw exception messages, missing sanitization)
3. Fix violations in each tool file
4. Build and verify