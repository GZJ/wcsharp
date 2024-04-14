using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class Program
{
    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    const int SW_MAXIMIZE = 3;

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
            ShowWindow(hWnd, SW_MAXIMIZE);
            Console.WriteLine($"Successfully maximized window: {windowTitle}");
        }
        else
        {
            Console.WriteLine($"Window not found: {windowTitle}");
        }
    }
}
