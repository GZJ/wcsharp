using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide window title as argument!");
            return;
        }

        string windowTitle = args[0];
        IntPtr hWnd = FindWindow(null, windowTitle);

        if (hWnd != IntPtr.Zero)
        {
            SetForegroundWindow(hWnd);
            Console.WriteLine($"Successfully focused on window: {windowTitle}");
        }
        else
        {
            Console.WriteLine($"Window not found: {windowTitle}");
        }
    }
}
