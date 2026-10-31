// Solution: SentinelCore-MCP
// Project:   SentinelCore-MCP
// File:         PowerShellResult.cs
// Author: Kyle L. Crowder
// Build Num:  102702



/// <summary>
///     Represents the result of a PowerShell command execution, exposing a success flag and the command's output or error
///     text.
///     USAGE: If Success is true, Output contains the result and Error is empty; if Success is false, Error contains the
///     exception message and output can be used for additional details.
/// </summary>
/// <remarks>
///     Immutable, sealed type. Instances are created via the Ok and Fail factory methods: Success = true
///     implies Output contains the result and Error is empty; Success = false implies Error contains the failure message
///     and Output is empty.
/// </remarks>
public sealed class PowerShellResult
{

    private PowerShellResult(bool success, string output, string error)
    {
        Success = success;
        Output = output;
        Error = error;
    }








    public string Error { get; }
    public string Output { get; }
    public bool Success { get; }








    public static PowerShellResult Fail(string error)
    {
        return new PowerShellResult(false, string.Empty, error);
    }








    public static PowerShellResult Ok(string output)
    {
        return new PowerShellResult(true, output, string.Empty);
    }
}





public class ValidationResult
{
    private ValidationResult(bool isValid, string failureReason = "")
    {
        IsValid = isValid;
        FailureReason = failureReason;
    }








    public string FailureReason { get; }
    public bool IsValid { get; }


    public static ValidationResult Fail(string reason) => new ValidationResult(false, reason);


    public static ValidationResult Success() => new ValidationResult(true);
}