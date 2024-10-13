using System;
using System.Text;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll")]
    static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    static extern bool IsWindowVisible(IntPtr hWnd);

    delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    static void Main()
    {
        EnumWindows(EnumerateWindow, IntPtr.Zero);
    }

    static bool EnumerateWindow(IntPtr hWnd, IntPtr lParam)
    {
        const int nChars = 256;
        StringBuilder windowTitle = new StringBuilder(nChars);

        if (IsWindowVisible(hWnd) && GetWindowText(hWnd, windowTitle, nChars) > 0)
        {
            Console.WriteLine($"{windowTitle},{hWnd}");
        }

        return true;
    }
}
