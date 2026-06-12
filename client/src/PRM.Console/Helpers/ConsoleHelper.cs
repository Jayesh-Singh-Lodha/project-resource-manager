namespace PRM.Console.Helpers;

/// <summary>
/// Utility methods for console UI rendering.
/// Provides consistent styling across all screens: boxes, separators, colored output, masked input.
/// </summary>
public static class ConsoleHelper
{
    private const int BoxWidth = 48;

    /// <summary>
    /// Draws a bordered box with one or two lines of text.
    /// Matches BRD screen layout: ╔══╗ ║ ║ ╚══╝
    /// </summary>
    public static void DrawBox(string line1, string? line2 = null)
    {
        var border = new string('═', BoxWidth - 2);
        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine($"╔{border}╗");
        System.Console.WriteLine($"║  {line1.PadRight(BoxWidth - 5)}║");
        if (line2 is not null)
        {
            System.Console.WriteLine($"║  {line2.PadRight(BoxWidth - 5)}║");
        }
        System.Console.WriteLine($"╚{border}╝");
        System.Console.ResetColor();
        System.Console.WriteLine();
    }

    /// <summary>
    /// Draws a horizontal separator line.
    /// </summary>
    public static void DrawSeparator()
    {
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.WriteLine(new string('─', BoxWidth - 2));
        System.Console.ResetColor();
    }

    /// <summary>
    /// Writes a success message in green with a checkmark.
    /// </summary>
    public static void WriteSuccess(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"\n{message} ✓");
        System.Console.ResetColor();
    }

    /// <summary>
    /// Writes an error message in red.
    /// </summary>
    public static void WriteError(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"\n✗ {message}");
        System.Console.ResetColor();
    }

    /// <summary>
    /// Writes a warning message in yellow.
    /// </summary>
    public static void WriteWarning(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"\n⚠ {message}");
        System.Console.ResetColor();
    }

    /// <summary>
    /// Prompts the user with a label and returns their input.
    /// </summary>
    public static string Prompt(string label)
    {
        System.Console.Write($"{label}: ");
        return System.Console.ReadLine()?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Reads a password from the console, masking input with asterisks.
    /// Handles backspace for corrections. Fallbacks to ReadLine if input is redirected.
    /// </summary>
    public static string ReadPassword(string label)
    {
        if (System.Console.IsInputRedirected)
        {
            System.Console.Write($"{label}: ");
            return System.Console.ReadLine() ?? string.Empty;
        }

        System.Console.Write($"{label}: ");
        var password = string.Empty;

        try
        {
            while (true)
            {
                var key = System.Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    System.Console.WriteLine();
                    break;
                }

                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[..^1];
                    System.Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password += key.KeyChar;
                    System.Console.Write("*");
                }
            }
        }
        catch
        {
            return System.Console.ReadLine() ?? string.Empty;
        }

        return password;
    }

    /// <summary>
    /// Waits for the user to press any key before continuing.
    /// </summary>
    public static void WaitForKey(string message = "Press any key to continue...")
    {
        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.Write(message);
        System.Console.ResetColor();

        try
        {
            if (!System.Console.IsInputRedirected)
            {
                System.Console.ReadKey(intercept: true);
                while (System.Console.KeyAvailable)
                {
                    System.Console.ReadKey(intercept: true);
                }
            }
            else
            {
                System.Console.ReadLine();
            }
        }
        catch
        {
            try { System.Console.ReadLine(); } catch { }
        }
        System.Console.WriteLine();
    }

    /// <summary>
    /// Clears the console screen.
    /// </summary>
    public static void ClearScreen()
    {
        try
        {
            if (!System.Console.IsInputRedirected && !System.Console.IsOutputRedirected)
            {
                System.Console.Clear();
            }
        }
        catch
        {
            // Ignore if console handle is not available (e.g. redirected output in tests/CI)
        }
    }
}
