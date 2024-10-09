using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class Program
{
    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags
    );

    private static readonly IntPtr HWND_TOP = IntPtr.Zero;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;

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
        if (args.Length < 2)
        {
            Console.WriteLine(
                "Please provide window title/handle and monitor number as arguments!"
            );
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
            if (
                int.TryParse(args[1], out int monitorIndex)
                && monitorIndex >= 0
                && monitorIndex < Screen.AllScreens.Length
            )
            {
                if (GetWindowRect(hWnd, out RECT windowRect))
                {
                    Screen currentScreen = Screen.FromHandle(hWnd);
                    int offsetX = windowRect.Left - currentScreen.Bounds.Left;
                    int offsetY = windowRect.Top - currentScreen.Bounds.Top;
                    Screen targetScreen = Screen.AllScreens[monitorIndex];

                    int newX = targetScreen.Bounds.Left + offsetX;
                    int newY = targetScreen.Bounds.Top + offsetY;

                    if (args.Length > 2 && int.TryParse(args[2], out int providedX))
                    {
                        newX = targetScreen.Bounds.Left + providedX;
                    }
                    if (args.Length > 3 && int.TryParse(args[3], out int providedY))
                    {
                        newY = targetScreen.Bounds.Top + providedY;
                    }

                    SetWindowPos(hWnd, HWND_TOP, newX, newY, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
                    SetForegroundWindow(hWnd);

                    Console.WriteLine(
                        $"Successfully moved window '{windowTitle}' to monitor {monitorIndex} at position ({newX - targetScreen.Bounds.Left}, {newY - targetScreen.Bounds.Top})."
                    );
                }
                else
                {
                    Console.WriteLine("Failed to get the window rectangle.");
                }
            }
            else
            {
                Console.WriteLine("Invalid monitor index. Please provide a valid monitor number.");
            }
        }
        else
        {
            Console.WriteLine($"Window not found: {windowTitle}");
        }
    }
}
