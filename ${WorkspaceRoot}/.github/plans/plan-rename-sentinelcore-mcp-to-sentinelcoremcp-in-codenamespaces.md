# Rename SentinelCore-MCP to SentinelCoreMCP in code/namespaces

## Understanding
Remove the hyphen from `SentinelCore-MCP` in all project code, namespaces, types, and configuration — but keep the hyphen in documentation (README, comments, etc.). The project directory itself keeps the hyphen since the user only asked for namespaces/types.

## Approach
1. Search all source files for namespace/type references containing `SentinelCore-MCP`
2. Rename in `.csproj` (AssemblyName, PackageId, Description)
3. Rename in `.mcp/server.json` (name, identifier)
4. Rename namespaces in all `.cs` files
5. Rename `Program.cs` ServerInfo Name/WebsiteUrl
6. Update `AssemblyInfo1.cs` if needed
7. Build to verify

## Key Files
- `SentinelCore-MCP\SentinelCore-MCP.csproj` — PackageId, Description
- `SentinelCore-MCP\Program.cs` — ServerInfo
- `SentinelCore-MCP\.mcp\server.json` — name, identifier
- All `Tools\*.cs` — namespace declarations
- `SentinelCore-MCP\AssemblyInfo1.cs` — assembly metadata

## Steps
1. Search all source files for `SentinelCore-MCP` references in code/config
2. Update `.csproj` — rename PackageId, Description references
3. Update `.mcp/server.json` — name and identifier fields
4. Update `Program.cs` — ServerInfo properties
5. Update `AssemblyInfo1.cs` — if it contains references
6. Update all Tool `.cs` files — namespace declarations
7. Update any other code references found
8. Build and verify