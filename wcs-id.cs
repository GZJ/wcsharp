using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    static void Main(string[] args)
    {
        IntPtr hWnd;

        if (args.Length == 0)
        {
            hWnd = GetForegroundWindow();
        }
        else
        {
            string windowTitle = args[0];
            hWnd = FindWindow(null, windowTitle);
        }

        if (hWnd != IntPtr.Zero)
        {
            Console.Write($"{hWnd}");
        }
        else
        {
            Console.Write(0);
        }
    }
}
