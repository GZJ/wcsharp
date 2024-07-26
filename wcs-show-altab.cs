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
        SetWindowStyle(hWnd);
    }

    static void SetWindowStyle(IntPtr hWnd)
    {
        if (hWnd != IntPtr.Zero)
        {
            SetWindowLong(
                hWnd,
                GWL_EXSTYLE,
                (GetWindowLong(hWnd, GWL_EXSTYLE) | WS_EX_APPWINDOW) & ~WS_EX_TOOLWINDOW
            );
        }
        else
        {
            Console.WriteLine("Window not found.");
        }
    }
}
