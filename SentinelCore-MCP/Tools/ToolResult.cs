// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         ToolResult.cs
// Author: Kyle L. Crowder
// Build Num:  080801



namespace SentinelCoreMCP.Tools;





/// <summary>
///     a universal result object for tool operations. It contains information about the success or failure of the
///     operation, as well as any relevant results or failure reasons.
///     Every AITool in the Sentinel Core Platform *must* return a ToolResult object to indicate the outcome of its
///     operation. This allows for consistent handling of tool results across the system.
/// </summary>
public class ToolResult
{








    /// <summary>
    ///     Indicates the reason for the failure of the tool operation, if any.
    ///     THIS MUST BE USED FOR ALL FAILURES AND CONTAIN THE EXCEPTION MESSAGE AND ANY ADDITIONAL CONTEXTUAL INFORMATION ABOUT THE FAILURE. This is critical for debugging and understanding why a tool operation failed.
    ///    ///     If the tool operation was successful, this property should be null.
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    ///     Indicates the results of the tool operation. This is oftype object to allow for flexibility in the type of results that can be returned. It can be a string, a complex object, or any other type that represents the results of the tool operation.
    /// </summary>
    public object? Results { get; set; }

    /// <summary>
    ///     Indicates whether the tool operation was successful.
    /// </summary>
    public bool Success { get; set; }

    ///<summary>
    ///      Human readable message providing additional context about the result of the tool operation. This can be used to explain the outcome of the operation in a more user-friendly way, or to provide additional information about the result. It can be used for logging, debugging, or providing feedback to the user. If the tool operation was successful, this property can contain a success message. If the operation failed, it can contain a message explaining the failure.
    /// </summary>
    public string? Message { get; set; }





    public static ToolResult Fail(string errorDetails, string message)
       => new ToolResult
       {
           Success = false,
           Message = message,
           ErrorDetails = errorDetails,
           Results = null
       };








    public static ToolResult Ok(object results, string message)
    {
        return new ToolResult
        {
            Success = true,
            ErrorDetails = null,
            Results = results,
            Message = message
        };
    }
}
