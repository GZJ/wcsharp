using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

//-------------------------------- window manager --------------------------------
public static class WindowManager
{
    // DLL imports
    [DllImport("user32.dll")] static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)] static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindow(IntPtr hWnd);
    [DllImport("user32.dll", SetLastError = true)] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] public static extern IntPtr SetFocus(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern IntPtr SetActiveWindow(IntPtr hWnd);
    [DllImport("user32.dll", SetLastError = true)] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    // Constants
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_SHOWWINDOW = 0x0040;
    public const uint SWP_NOACTIVATE = 0x0010;
    static readonly IntPtr HWND_TOP = new IntPtr(0);
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    const int SW_SHOWNORMAL = 1;
    const int SW_HIDE = 0;
    const UInt32 WM_CLOSE = 0x0010;

    public class WindowInfo
    {
        public string Title { get; set; }
        public IntPtr Handle { get; set; }
    }

    private static CountdownEvent processExitEvent;
    private static List<WindowInfo> WindowInfos = new List<WindowInfo>();
    private static IntPtr PrevHwnd;
    public static string WindowSpecifiers { get; set; }

    private static void UpdatePrevHwnd() => PrevHwnd = GetForegroundWindow();

    private static bool IsWindowFocused() => 
        WindowInfos.Any(wi => wi.Handle == GetForegroundWindow());

    private static void WinFocus(IntPtr hWnd) => SetForegroundWindow(hWnd);
    private static void WinUnFocus() => SetForegroundWindow(PrevHwnd);
    private static void WinHide(IntPtr hWnd) => ShowWindow(hWnd, SW_HIDE);
    private static void WinShow(IntPtr hWnd) => ShowWindow(hWnd, SW_SHOWNORMAL);

    private static string GetWindowTitle(IntPtr hWnd)
    {
        int length = GetWindowTextLength(hWnd);
        if (length == 0) return string.Empty;

        StringBuilder builder = new StringBuilder(length + 1);
        GetWindowText(hWnd, builder, builder.Capacity);
        return builder.ToString();
    }

    static WindowManager() => UpdatePrevHwnd();

    public static void InitHwnd(string arg)
    {
        WindowSpecifiers = arg;
        foreach (string title in arg.Split(','))
        {
            if (int.TryParse(title, out int handle))
            {
                IntPtr hwnd = (IntPtr)handle;
                WindowInfos.Add(new WindowInfo { 
                    Title = IsWindow(hwnd) ? GetWindowTitle(hwnd) : "", 
                    Handle = IsWindow(hwnd) ? hwnd : IntPtr.Zero 
                });
            }
            else
            {
                WindowInfos.Add(new WindowInfo { 
                    Title = title, 
                    Handle = FindWindow(null, title) 
                });
            }
        }
    }

    public static void MonitorWindowProcessExit()
    {
        int count = WindowInfos.Count;
        processExitEvent = new CountdownEvent(count);

        foreach (var wi in WindowInfos)
        {
            if (wi.Handle == IntPtr.Zero) continue;
            
            GetWindowThreadProcessId(wi.Handle, out uint processId);
            Console.WriteLine($"Monitoring window: \"{wi.Title}\", process ID: {processId}");
            
            try
            {
                Process process = Process.GetProcessById((int)processId);
                process.EnableRaisingEvents = true;
                process.Exited += (sender, e) => {
                    Console.WriteLine($"Process ID: {processId} has exited");
                    processExitEvent.Signal();
                };
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Cannot find process ID: {processId}");
                processExitEvent.Signal();
            }
        }
        
        processExitEvent.Wait();
        Console.WriteLine("All monitored processes have exited");
        Application.Exit();
    }

    public static void ToggleWindowState()
    {
        if (IsWindowFocused())
        {
            WinUnFocus();
            foreach (var wi in WindowInfos.Where(w => w.Handle != IntPtr.Zero))
                WinHide(wi.Handle);
        }
        else
        {
            UpdatePrevHwnd();
            foreach (var wi in WindowInfos.AsEnumerable().Reverse().Where(w => w.Handle != IntPtr.Zero))
            {
                WinShow(wi.Handle);
                SetWindowPos(wi.Handle, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                WinFocus(wi.Handle);
            }
        }
    }

    public static void ToggleWindow()
    {
        if (WindowInfos.Any(wi => wi.Handle != IntPtr.Zero && IsWindowVisible(wi.Handle)))
        {
            foreach (var wi in WindowInfos.Where(w => w.Handle != IntPtr.Zero && IsWindowVisible(w.Handle)))
                WinHide(wi.Handle);
        }
        else
        {
            foreach (var wi in WindowInfos.AsEnumerable().Reverse().Where(w => w.Handle != IntPtr.Zero))
            {
                WinShow(wi.Handle);
                WinFocus(wi.Handle);
            }
        }
    }

    public static void CloseWindows()
    {
        foreach (var wi in WindowInfos.Where(w => w.Handle != IntPtr.Zero))
        {
            Console.WriteLine($"Closing window: {wi.Handle}");
            SendMessage(wi.Handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
    }

}

//-------------------------------- hotkey manager --------------------------------
public class HotKeyManager : IDisposable
{
    public const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }

    private IntPtr _windowHandle;
    private int _hotkeyId;
    private bool _disposed = false;

    public HotKeyManager(IntPtr windowHandle, int hotkeyId, uint fsModifiers, uint vk)
    {
        _windowHandle = windowHandle;
        _hotkeyId = hotkeyId;

        if (!RegisterHotKey(_windowHandle, _hotkeyId, fsModifiers, vk))
            throw new InvalidOperationException("Unable to register hotkey.");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                UnregisterHotKey(_windowHandle, _hotkeyId);
            _disposed = true;
        }
    }

    public static bool TryParseHotKey(string hotKey, out uint modifiers, out uint key)
    {
        modifiers = 0;
        key = 0;

        string[] parts = hotKey.Split('+');
        foreach (var part in parts)
        {
            switch (part.Trim().ToLower())
            {
                case "ctrl": modifiers |= (uint)KeyModifiers.Control; break;
                case "alt": modifiers |= (uint)KeyModifiers.Alt; break;
                case "shift": modifiers |= (uint)KeyModifiers.Shift; break;
                case "win": modifiers |= (uint)KeyModifiers.Win; break;
                default:
                    try
                    {
                        key = (uint)(Keys)Enum.Parse(typeof(Keys), part, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing key: {part}. Exception: {ex.Message}");
                        return false;
                    }
                    break;
            }
        }
        return key != 0;
    }
}

//-------------------------------- hotkey hidden window  --------------------------------
public class HiddenWindow : Form
{
    private const int HOTKEY_ID = 1;

    public HiddenWindow()
    {
        Visible = false;
        ShowInTaskbar = false;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == HotKeyManager.WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            WindowManager.ToggleWindowState();
        
        base.WndProc(ref m);
    }
}

//-------------------------------- tray manager  --------------------------------
public class TrayIconManager
{
    private NotifyIcon trayIcon;

    public TrayIconManager()
    {
        var trayMenu = new ContextMenu();
        trayMenu.MenuItems.Add("Exit", (s, e) => {
            WindowManager.CloseWindows();
            Application.Exit();
        });

        string tooltipText = WindowManager.WindowSpecifiers ?? string.Empty;
        if (tooltipText.Length > 63)
        {
            tooltipText = tooltipText.Substring(0, 63);
            Console.WriteLine("Warning: Tray icon tooltip text truncated to 63 characters.");
        }

        trayIcon = new NotifyIcon
        {
            Text = tooltipText,
            Icon = new Icon(SystemIcons.Application, 40, 40),
            ContextMenu = trayMenu,
            Visible = true
        };

        trayIcon.MouseClick += (s, e) => {
            if (e.Button == MouseButtons.Left)
                WindowManager.ToggleWindow();
        };
    }

    public void Dispose()
    {
        trayIcon?.Dispose();
    }
}

//-------------------------------- Program  --------------------------------
public static class Program
{
    private static HotKeyManager _hotKeyManager;
    private static HiddenWindow _hiddenWindow;
    private static TrayIconManager _trayIconManager;
    private const int HOTKEY_ID = 1;

    [STAThread]
    static void Main()
    {
        string[] args = Environment.GetCommandLineArgs();
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: wcs-tray <WindowTitle or Hwnd> <HotKey>");
            return;
        }

        WindowManager.InitHwnd(args[1]);
        
        Thread monitorThread = new Thread(WindowManager.MonitorWindowProcessExit);
        monitorThread.Start();

        Application.ApplicationExit += (s, e) => {
            Console.WriteLine("Application is exiting. Cleaning up...");
            _hotKeyManager?.Dispose();
            _trayIconManager?.Dispose();
        };

        _trayIconManager = new TrayIconManager();
        _hiddenWindow = new HiddenWindow();

        if (HotKeyManager.TryParseHotKey(args[2], out uint modifiers, out uint key))
            _hotKeyManager = new HotKeyManager(_hiddenWindow.Handle, HOTKEY_ID, modifiers, key);
        else
        {
            Console.WriteLine("Invalid hotkey format. Use format like 'Ctrl+Alt+X'.");
            return;
        }

        Application.Run();
    }
}
