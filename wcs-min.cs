using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    const int SW_MINIMIZE = 6;

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide window title or window handle as argument!");
            return;
        }

        string windowTitle = args[0];
        IntPtr hWnd;

        if (int.TryParse(windowTitle, out int handle))
        {
            hWnd = (IntPtr)handle;
        }
        else
        {
            hWnd = FindWindow(null, windowTitle);
        }

        if (hWnd != IntPtr.Zero)
        {
            ShowWindow(hWnd, SW_MINIMIZE);
            Console.WriteLine($"Successfully minimized window: {windowTitle}");
        }
        else
        {
            Console.WriteLine($"Window not found: {windowTitle}");
        }
    }
}
