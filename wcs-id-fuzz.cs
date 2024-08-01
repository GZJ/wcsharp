using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: wcs-id-fuzz <regex_pattern>");
            return;
        }

        string regexPattern = args[0];
        Regex regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
        IntPtr hWnd = IntPtr.Zero;

        EnumWindows(
            delegate(IntPtr hwnd, IntPtr lParam)
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hwnd, sb, sb.Capacity);
                if (regex.IsMatch(sb.ToString()))
                {
                    hWnd = hwnd;
                    return false;
                }
                return true;
            },
            IntPtr.Zero
        );

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
