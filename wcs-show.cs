using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    const int GWL_EXSTYLE = -20;
    const int WS_EX_APPWINDOW = 0x00040000;
    const int WS_EX_TOOLWINDOW = 0x00000080;

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a window title as an argument.");
            return;
        }

        IntPtr hWnd = FindWindow(null, args[0]);
        SetWindowStyle(hWnd);
    }

    static void SetWindowStyle(IntPtr hWnd)
    {
        if (hWnd != IntPtr.Zero)
        {
            SetWindowLong(hWnd, GWL_EXSTYLE, (GetWindowLong(hWnd, GWL_EXSTYLE) | WS_EX_APPWINDOW) & ~WS_EX_TOOLWINDOW);
        }
        else
        {
            Console.WriteLine("Window not found.");
        }
    }
}
