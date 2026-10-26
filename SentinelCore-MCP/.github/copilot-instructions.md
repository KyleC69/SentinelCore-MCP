# Sentinel Core Tools MCP Server - Windows Tools

## Tool Mandates

- All tools must return ToolResult object in all tool activations regaurdless of the results or success/fail status
- Each tool command results must be clean, normalized without ctrl characters and for machine consumption.
- No tool should ever throw or swallow errors, all reasonable possibilities should be accounted for and explained in the ErrorDetails property of the result object and returned.
- SysInternal commands must silence the banner with -nobanner. to suppress eula /accepteula needs to be added.
- Ensure a cancellationtoken is used with a 20 second timout and is propegated throughout all tools.



## Asynchronous Function Pattern

**Recommended** pattern for cmd line utilities
```using System.Diagnostics;

using Process process = new();

process.StartInfo = new ProcessStartInfo
{
    FileName = "ipconfig.exe",
    Arguments = "/all",
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false
};

process.WaitForExitAsync();

Normalize output for machine consumption
```

