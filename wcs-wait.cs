using System;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

    static string GetActiveWindowTitle()
    {
        const int nChars = 256;
        IntPtr handle = GetForegroundWindow();
        var buff = new System.Text.StringBuilder(nChars);

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
        Console.WriteLine($"Waiting for window with title: {targetTitle}");

        while (true)
        {
            string currentTitle = GetActiveWindowTitle();

            if (currentTitle != null && currentTitle == targetTitle)
            {
                break;
            }

            Thread.Sleep(100);
        }
    }
}
