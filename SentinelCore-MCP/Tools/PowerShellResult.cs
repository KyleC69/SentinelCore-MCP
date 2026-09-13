// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num:  ${CurrentDate.Month}${CurrentDate.Day}${CurrentDate.Hour}
//


/// <summary>
/// Represents the result of a PowerShell command execution, exposing a success flag and the command's output or error
/// text.
/// USAGE: If Success is true, Output contains the result and Error is empty; if Success is false, Error contains the exception message and output can be used for additional details.
/// </summary>
/// <remarks>Immutable, sealed type. Instances are created via the Ok and Fail factory methods: Success = true
/// implies Output contains the result and Error is empty; Success = false implies Error contains the failure message
/// and Output is empty.</remarks>
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
    public bool IsValid { get; }
    public string FailureReason { get; }
    private ValidationResult(bool isValid, string failureReason = "")
    {
        IsValid = isValid;
        FailureReason = failureReason;
    }
    public static ValidationResult Success() => new ValidationResult(true);
    public static ValidationResult Fail(string reason) => new ValidationResult(false, reason);
}
