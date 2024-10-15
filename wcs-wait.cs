using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

class Program
{
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    static string GetActiveWindowTitle()
    {
        const int nChars = 256;
        IntPtr handle = GetForegroundWindow();
        var buff = new StringBuilder(nChars);

        if (GetWindowText(handle, buff, nChars) > 0)
        {
            return buff.ToString();
        }
        return null;
    }

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide window title as argument!");
            return;
        }

        string targetTitle = args[0];
        bool useRegex = args.Length > 1 && args[1] == "-r";

        Console.WriteLine(
            $"Waiting for window with title{(useRegex ? " matching regex" : "")}: {targetTitle}"
        );

        string currentTitle = null;

        while (true)
        {
            currentTitle = GetActiveWindowTitle();

            if (currentTitle != null)
            {
                if (useRegex)
                {
                    if (Regex.IsMatch(currentTitle, targetTitle))
                    {
                        break;
                    }
                }
                else
                {
                    if (currentTitle == targetTitle)
                    {
                        break;
                    }
                }
            }

            Thread.Sleep(100);
        }

        Console.WriteLine($"Found active window with title: {currentTitle}");
    }
}
