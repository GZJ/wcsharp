using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;

    static void Main(string[] args)
    {
        string windowTitle = args[0];

        IntPtr hWnd = FindWindow(null, windowTitle);

        if (hWnd == IntPtr.Zero)
        {
            Console.WriteLine("Window not found!");
        }
        else
        {
            ShowWindow(hWnd, SW_HIDE);
            Console.WriteLine("Window hidden!");
        }
    }
}
