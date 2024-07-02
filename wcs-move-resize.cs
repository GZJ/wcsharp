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

    static void Main(string[] args)
    {
        if (args.Length != 5)
        {
            Console.WriteLine("Usage: Program <windowTitle> <x> <y> <width> <height>");
            return;
        }

        string windowTitle = args[0];
        if (
            !int.TryParse(args[1], out int newX)
            || !int.TryParse(args[2], out int newY)
            || !int.TryParse(args[3], out int newWidth)
            || !int.TryParse(args[4], out int newHeight)
        )
        {
            Console.WriteLine(
                "Invalid parameters. Ensure that x, y, width, and height are integers."
            );
            return;
        }

        IntPtr hWnd = FindWindow(null, windowTitle);

        if (hWnd == IntPtr.Zero)
        {
            Console.WriteLine($"Could not find a window with title: {windowTitle}");
            return;
        }

        bool result = MoveWindow(hWnd, newX, newY, newWidth, newHeight, true);

        if (!result)
        {
            Console.WriteLine("Failed to move and resize the window.");
        }
        else
        {
            Console.WriteLine("Window moved and resized successfully.");
        }
    }
}
