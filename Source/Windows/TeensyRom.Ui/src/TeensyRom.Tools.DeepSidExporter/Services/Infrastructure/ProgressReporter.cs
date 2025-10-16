namespace TeensyRom.Tools.DeepSidExporter.Services.Infrastructure;

/// <summary>
/// Service for displaying progress updates on a single line (modern CLI style)
/// </summary>
public interface IProgressReporter
{
    void Report(string message);
    void Complete(string message);
    void Clear();
}

public class ProgressReporter : IProgressReporter
{
    private int _lastLineLength = 0;
    private readonly object _lock = new();

    /// <summary>
    /// Report progress on the same line (updates in place)
    /// </summary>
    public void Report(string message)
    {
        lock (_lock)
        {
            // Clear the previous line
            if (_lastLineLength > 0)
            {
                Console.Write('\r' + new string(' ', _lastLineLength) + '\r');
            }

            // Write the new message in progress color
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(message);
            Console.ResetColor();
            _lastLineLength = message.Length;
        }
    }

    /// <summary>
    /// Complete progress with a final message and newline
    /// </summary>
    public void Complete(string message)
    {
        lock (_lock)
        {
            // Clear the progress line
            if (_lastLineLength > 0)
            {
                Console.Write('\r' + new string(' ', _lastLineLength) + '\r');
            }

            // Write the completion message
            Console.WriteLine(message);
            _lastLineLength = 0;
        }
    }

    /// <summary>
    /// Clear the current progress line
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            if (_lastLineLength > 0)
            {
                Console.Write('\r' + new string(' ', _lastLineLength) + '\r');
                _lastLineLength = 0;
            }
        }
    }
}
