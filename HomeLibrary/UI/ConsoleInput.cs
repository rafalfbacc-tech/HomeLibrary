namespace HomeLibrary.UI;


public class EndOfInputException : Exception;

public static class ConsoleInput
{
    public static string ReadLine(string prompt)
    {
        Console.Write("  " + prompt);
        return (Console.ReadLine() ?? throw new EndOfInputException()).Trim();
    }

    public static string? ReadText(string prompt, string? current = null)
    {
        var suffix = current is null ? " (Enter = cancel): " : $" [{current}] (Enter = no change): ";
        var value = ReadLine(prompt + suffix);
        return value.Length == 0 ? current : value;
    }

    public static int? ReadInt(string prompt, int min, int max, int? defaultValue = null)
    {
        while (true)
        {
            var value = ReadLine(prompt);
            if (value.Length == 0) return defaultValue;
            if (int.TryParse(value, out int n) && n >= min && n <= max) return n;
            Error($"Enter a number between {min} and {max}.");
        }
    }

    public static bool Confirm(string question)
    {
        var answer = ReadLine($"{question} (y/n): ").ToLowerInvariant();
        return answer is "y" or "yes";
    }

    public static void Pause()
    {
        Console.WriteLine();
        ReadLine("[press Enter]");
    }

    public static void Error(string message) => Console.WriteLine($"  ERROR: {message}");

    public static void Success(string message) => Console.WriteLine($"  OK: {message}");

    public static void WriteColored(string text, ConsoleColor color, bool newLine = true)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = color;
        if (newLine) Console.WriteLine(text); else Console.Write(text);
        Console.ForegroundColor = old;
    }

    public static void Clear()
    {
        try { if (!Console.IsOutputRedirected) Console.Clear(); }
        catch (IOException) { /* no real console */ }
    }
}
