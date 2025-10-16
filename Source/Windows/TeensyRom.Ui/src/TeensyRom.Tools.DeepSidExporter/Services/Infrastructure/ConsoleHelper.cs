namespace TeensyRom.Tools.DeepSidExporter.Services.Infrastructure;

/// <summary>
/// Helper for consistent colored console output
/// </summary>
public static class ConsoleHelper
{
    // Color scheme
    private static readonly ConsoleColor StepColor = ConsoleColor.Cyan;
    private static readonly ConsoleColor SuccessColor = ConsoleColor.Green;
    private static readonly ConsoleColor WarningColor = ConsoleColor.Yellow;
    private static readonly ConsoleColor ErrorColor = ConsoleColor.Red;
    private static readonly ConsoleColor InfoColor = ConsoleColor.Gray;
    private static readonly ConsoleColor HighlightColor = ConsoleColor.White;
    private static readonly ConsoleColor ProgressColor = ConsoleColor.DarkCyan;

    public static void WriteStep(string message)
    {
        Console.ForegroundColor = StepColor;
        Console.Write(message);
        Console.ResetColor();
    }

    public static void WriteStepLine(string message)
    {
        Console.ForegroundColor = StepColor;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteSuccess(string message)
    {
        Console.ForegroundColor = SuccessColor;
        Console.Write(message);
        Console.ResetColor();
    }

    public static void WriteSuccessLine(string message)
    {
        Console.ForegroundColor = SuccessColor;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = WarningColor;
        Console.Write(message);
        Console.ResetColor();
    }

    public static void WriteWarningLine(string message)
    {
        Console.ForegroundColor = WarningColor;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteError(string message)
    {
        Console.ForegroundColor = ErrorColor;
        Console.Write(message);
        Console.ResetColor();
    }

    public static void WriteErrorLine(string message)
    {
        Console.ForegroundColor = ErrorColor;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteInfo(string message)
    {
        Console.ForegroundColor = InfoColor;
        Console.Write(message);
        Console.ResetColor();
    }

    public static void WriteInfoLine(string message)
    {
        Console.ForegroundColor = InfoColor;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteHighlight(string message)
    {
        Console.ForegroundColor = HighlightColor;
        Console.Write(message);
        Console.ResetColor();
    }

    public static void WriteHighlightLine(string message)
    {
        Console.ForegroundColor = HighlightColor;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteProgress(string message)
    {
        Console.ForegroundColor = ProgressColor;
        Console.Write(message);
        Console.ResetColor();
    }

    public static void WriteSeparator()
    {
        Console.ForegroundColor = InfoColor;
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.ResetColor();
    }

    public static void WriteSubSeparator()
    {
        Console.ForegroundColor = InfoColor;
        Console.WriteLine("───────────────────────────────────────────────────────────");
        Console.ResetColor();
    }
}
