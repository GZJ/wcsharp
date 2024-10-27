using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    static extern bool IsWindow(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: get-window-rect <windowTitle>");
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

        if (hWnd == IntPtr.Zero || !IsWindow(hWnd))
        {
            return;
        }

        if (GetWindowRect(hWnd, out RECT rect))
        {
            Console.Write($"{rect.Left},{rect.Top},{rect.Right - rect.Left},{rect.Bottom - rect.Top}");
        }
    }
}
