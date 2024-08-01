using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: wcs-id <window_title>");
            return;
        }

        string windowTitle = args[0];

        IntPtr hWnd = FindWindow(null, windowTitle);

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
