using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: wcs-pid-fuzz <regex_pattern>");
            return;
        }

        string regexPattern = args[0];
        Regex regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
        uint processId = 0;

        EnumWindows(
            delegate(IntPtr hwnd, IntPtr lParam)
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hwnd, sb, sb.Capacity);
                if (regex.IsMatch(sb.ToString()))
                {
                    GetWindowThreadProcessId(hwnd, out processId);
                    return false;
                }
                return true;
            },
            IntPtr.Zero
        );

        Console.Write(processId);
    }
}
