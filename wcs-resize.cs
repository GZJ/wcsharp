using System;
using System.Runtime.InteropServices;
using System.Text;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool MoveWindow(
        IntPtr hWnd,
        int X,
        int Y,
        int nWidth,
        int nHeight,
        bool bRepaint
    );

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
        if (args.Length != 3)
        {
            Console.WriteLine("Usage: wcs-resize <windowTitle> <width> <height>");
            return;
        }

        if (!int.TryParse(args[1], out int newWidth) || !int.TryParse(args[2], out int newHeight))
        {
            Console.WriteLine("Invalid parameters. Ensure that width and height are integers.");
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
            Console.WriteLine($"Could not find a window with title: {windowTitle}");
            return;
        }

        if (GetWindowRect(hWnd, out RECT rect))
        {
            int newX = rect.Left;
            int newY = rect.Top;

            bool result = MoveWindow(hWnd, newX, newY, newWidth, newHeight, true);

            if (!result)
            {
                Console.WriteLine("Failed to resize the window.");
            }
            else
            {
                Console.WriteLine("Window resized successfully.");
            }
        }
        else
        {
            Console.WriteLine("Failed to get the current position of the window.");
        }
    }
}
