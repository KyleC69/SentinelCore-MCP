// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         FileSystemExtendedReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tools for extended file system operations: computing file hashes,
///     listing alternate data streams, and reading the hosts file.
/// </summary>
[McpServerToolType]
public sealed class FileSystemExtendedReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "File_System_Compute_Hash", ReadOnly = true, Destructive = false)]
    [Description("Computes a cryptographic hash of a file using the specified algorithm (MD5, SHA1, SHA256, SHA384, SHA512).")]
    public static ToolResult FileSystemComputeHash(
        [Description("The full path to the file to hash.")] string filePath,
        [Description("The hash algorithm to use: MD5, SHA1, SHA256, SHA384, or SHA512. Defaults to SHA256.")] string algorithm = "SHA256")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return ToolResult.Fail("filePath is required.");
            }

            if (!File.Exists(filePath))
            {
                return ToolResult.Fail($"File not found: {filePath}");
            }

            using var hashAlgorithm = algorithm.ToUpperInvariant() switch
            {
                "MD5" => (HashAlgorithm)MD5.Create(),
                "SHA1" => SHA1.Create(),
                "SHA256" => SHA256.Create(),
                "SHA384" => SHA384.Create(),
                "SHA512" => SHA512.Create(),
                _ => SHA256.Create()
            };

            using FileStream stream = File.OpenRead(filePath);
            byte[] hashBytes = hashAlgorithm.ComputeHash(stream);
            string hashHex = Convert.ToHexString(hashBytes);

            var result = new
            {
                FilePath = filePath,
                Algorithm = algorithm.ToUpperInvariant(),
                Hash = hashHex,
                FileSize = new FileInfo(filePath).Length
            };

            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"File hash computation failed: {ex.Message}");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "File_System_List_Streams", ReadOnly = true, Destructive = false)]
    [Description("Lists alternate data streams (ADS) for a file or directory on an NTFS volume.")]
    public static ToolResult FileSystemListStreams(
        [Description("The full path to the file or directory to inspect for alternate data streams.")] string path,
        [Description("Maximum number of streams to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ToolResult.Fail("path is required.");
            }

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                return ToolResult.Fail($"Path not found: {path}");
            }

            List<object> results = new();
            System.Diagnostics.ProcessStartInfo psi = new()
            {
                FileName = "cmd",
                Arguments = $"/c \"dir /r \"{path}\"\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return ToolResult.Fail("Unable to start dir command.");
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse dir /r output for ADS entries
            // ADS entries look like: filename:$DATA  or  filename:Zone.Identifier:$DATA
            string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (results.Count >= maxRecords) break;
                string trimmed = line.Trim();
                // Look for lines containing a colon that indicate ADS
                // Format: <date>  <size>  <filename>:<streamname>:$DATA
                int colonIdx = trimmed.IndexOf(':');
                if (colonIdx > 0 && colonIdx < trimmed.Length - 1)
                {
                    // Check if this is an ADS line (contains :$DATA or :Zone.Identifier etc.)
                    if (trimmed.Contains(":$DATA", StringComparison.OrdinalIgnoreCase) ||
                        trimmed.Contains(":Zone.Identifier", StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new { RawLine = trimmed });
                    }
                }
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Alternate data stream listing failed.");
        }
    }








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "File_System_Read_Hosts", ReadOnly = true, Destructive = false)]
    [Description("Reads the contents of the Windows hosts file for DNS redirection detection.")]
    public static ToolResult FileSystemReadHosts()
    {
        try
        {
            string hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");
            if (!File.Exists(hostsPath))
            {
                return ToolResult.Fail($"Hosts file not found at: {hostsPath}");
            }

            string[] lines = File.ReadAllLines(hostsPath);
            List<object> entries = new();
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                {
                    continue;
                }

                // Parse host entries: IP hostname [aliases...]
                string[] parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    entries.Add(new
                    {
                        IPAddress = parts[0],
                        Hostnames = string.Join(", ", parts[1..])
                    });
                }
            }

            string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch
        {
            return ToolResult.Fail("Hosts file read failed.");
        }
    }
}