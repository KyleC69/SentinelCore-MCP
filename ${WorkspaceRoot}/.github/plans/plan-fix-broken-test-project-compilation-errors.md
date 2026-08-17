# Fix Broken Test Project Compilation Errors

## Understanding
The test project `SentinelCore.Tests` has 263+ compilation errors because types/namespaces have been renamed or moved in the production code. The tests reference types like `AgentBuilder`, `AIAgent`, `AgentRole`, `AgentProfile`, `AgentPersona`, `IAgentSpecBuilder`, `AgentSpecBuilder`, `CoreAgentFactory`, `IDomainAgentFactory`, `AIFunction`, `AIFunctionFactory`, `AITool`, and `ModelBuilder` that no longer exist or have different names. Need to map old type names to current types and update all test files.

## Approach
1. Read all production code types to establish the current API surface
2. Read all test files to understand what they reference
3. Fix each test file by updating type references, constructors, and method calls to match current production API
4. Build and verify

## Key Files
- SentinelCore\projects\SentinelCore.Tests\AgentBuilderTests.cs - Uses old AgentBuilder, AIAgent, AgentRole, AgentProfile, etc.
- SentinelCore\projects\SentinelCore.Tests\AgentFactoryTests.cs - Uses old IAgentSpecBuilder, AgentSpecBuilder, CoreAgentFactory, IDomainAgentFactory
- SentinelCore\projects\SentinelCore.Tests\EventPublishingChatClientTests.cs
- SentinelCore\projects\SentinelCore.Tests\CaseFlow\*.cs
- SentinelCore\projects\SentinelCore.Tests\ToolsTests.cs
- SentinelCore\projects\SentinelCore.Tests\SentinelCoreEventsTests.cs
- SentinelCore\projects\SentinelCore.Tests\TestInfrastructure\*.cs

## Steps
1. Read production code types (AgentProfile, AgentRole, AgentProfileBuilder, SentinelAgentFactory, etc.) to map current API
2. Read all test files and test infrastructure files
3. Read test project csproj to understand package references
4. Fix AgentBuilderTests.cs - update to use current types
5. Fix AgentFactoryTests.cs - update to use current types
6. Fix remaining test files (EventPublishingChatClient, SentinelCoreEvents, CaseFlow, ToolsTests)
7. Fix TestInfrastructure files (CapturingAgentBuilder, EventCapture, etc.)
8. Build and verify all tests compile