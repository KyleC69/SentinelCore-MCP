



using System.Text.RegularExpressions;
namespace SentinelCoreMCP.Tools;
public static class Helpers
{


    public static string NormalizeToolOutput(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // Remove ANSI escape sequences
        text = Regex.Replace(text, @"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])", string.Empty);

        // Remove non-useful control chars
        text = new string(text.Where(c => !char.IsControl(c) || c == '\r' || c == '\n' || c == '\t').ToArray());

        // Normalize line endings
        text = text.Replace("\r\n", Environment.NewLine);
        text = text.Replace("\n", Environment.NewLine);
        text = text.Replace("\r", Environment.NewLine);
        //text = text.Replace("\r\n", "\n").Replace('\r', '\n');

        return text.Trim();
    }
}
