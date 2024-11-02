using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll")]
    static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    const uint WM_CLOSE = 0x0010;

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

        if (hWnd != IntPtr.Zero && IsWindow(hWnd))
        {
            uint processId;
            GetWindowThreadProcessId(hWnd, out processId);

            Console.Write(processId);
        }
        else
        {
            Console.Write(0);
        }
    }
}
