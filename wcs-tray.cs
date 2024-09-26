using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public static class WindowManager
{
    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    const int SW_SHOW = 5;
    const int SW_HIDE = 0;

    public class WindowInfo
    {
        public string Title { get; set; }
        public IntPtr Handle { get; set; }
    }

    private static CountdownEvent processExitEvent;
    private static List<WindowInfo> WindowInfos = new List<WindowInfo>();
    private static IntPtr PrevHwnd { get; set; }
    public static string Args1 { get; set; }

    static WindowManager()
    {
        UpdatePrevHwnd();
    }

    public static void InitHwnd(string arg)
    {
        Args1 = arg;
        string[] titles = arg.Split(',');

        foreach (string title in titles)
        {
            if (int.TryParse(title, out int handle))
            {
                if (IsWindow((IntPtr)handle))
                {
                    string t = GetWindowTitle((IntPtr)handle);
                    WindowInfos.Add(new WindowInfo { Title = t, Handle = (IntPtr)handle });
                }
                else
                {
                    WindowInfos.Add(new WindowInfo { Title = "", Handle = IntPtr.Zero });
                }
            }
            else
            {
                IntPtr hwnd = FindWindow(null, title);
                WindowInfos.Add(new WindowInfo { Title = title, Handle = hwnd });
            }
        }
    }

    public static void MonitorWindowProcessExit()
    {
        int count = 0;
        foreach (var wi in WindowInfos)
        {
            GetWindowThreadProcessId(wi.Handle, out uint processId);
            Console.WriteLine(
                $"MonitorWindowProcessExit: Found window with title \"{wi.Title}\", process ID: {processId}"
            );
            count += 1;
            try
            {
                Process process = Process.GetProcessById((int)processId);
                process.EnableRaisingEvents = true;
                process.Exited += (sender, e) =>
                {
                    Console.WriteLine(
                        $"MonitorWindowProcessExit: Process ID: {processId} has exited."
                    );
                    processExitEvent.Signal();
                };
            }
            catch (ArgumentException)
            {
                Console.WriteLine(
                    $"MonitorWindowProcessExit: Unable to find process corresponding to process ID {processId}."
                );
            }
        }
        Console.WriteLine(
            "MonitorWindowProcessExit: Waiting for all monitored processes to exit..."
        );
        processExitEvent = new CountdownEvent(count);
        processExitEvent.Wait();
        Console.WriteLine("MonitorWindowProcessExit: All monitored processes have exited.");
        Application.Exit();
    }

    public static void ToggleWindowState()
    {
        if (IsWindowFocused())
        {
            WinUnFocus();
            foreach (var windowInfo in WindowInfos)
            {
                IntPtr hwnd = windowInfo.Handle;
                if (hwnd != IntPtr.Zero)
                {
                    WinHide(hwnd);
                }
            }
        }
        else
        {
            UpdatePrevHwnd();
            foreach (var windowInfo in WindowInfos.AsEnumerable().Reverse())
            {
                IntPtr hwnd = windowInfo.Handle;
                if (hwnd != IntPtr.Zero)
                {
                    WinShow(hwnd);
                    WinFocus(hwnd);
                }
            }
        }
    }

    public static void ToggleWindow()
    {
        bool anyWindowHidden = false;

        foreach (var windowInfo in WindowInfos)
        {
            IntPtr hwnd = windowInfo.Handle;
            if (hwnd != IntPtr.Zero && IsWindowVisible(hwnd))
            {
                WinHide(hwnd);
                anyWindowHidden = true;
            }
        }

        if (anyWindowHidden)
        {
            return;
        }

        foreach (var windowInfo in WindowInfos.AsEnumerable().Reverse())
        {
            IntPtr hwnd = windowInfo.Handle;
            if (hwnd != IntPtr.Zero)
            {
                WinShow(hwnd);
                WinFocus(hwnd);
            }
        }
    }

    private static void UpdatePrevHwnd()
    {
        PrevHwnd = GetForegroundWindow();
    }

    private static bool IsWindowFocused()
    {
        IntPtr foregroundWindowHandle = GetForegroundWindow();
        return WindowInfos.Any(wi => wi.Handle == foregroundWindowHandle);
    }

    private static void WinFocus(IntPtr hWnd)
    {
        SetForegroundWindow(hWnd);
    }

    private static void WinUnFocus()
    {
        SetForegroundWindow(PrevHwnd);
    }

    private static void WinHide(IntPtr hWnd)
    {
        ShowWindow(hWnd, SW_HIDE);
    }

    private static void WinShow(IntPtr hWnd)
    {
        ShowWindow(hWnd, SW_SHOW);
    }

    private static string GetWindowTitle(IntPtr hWnd)
    {
        int length = GetWindowTextLength(hWnd);
        if (length == 0)
            return string.Empty;

        StringBuilder builder = new StringBuilder(length + 1);
        GetWindowText(hWnd, builder, builder.Capacity);
        return builder.ToString();
    }
}

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
        {
            throw new InvalidOperationException("Unable to register hotkey.");
        }
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
            {
                UnregisterHotKey(_windowHandle, _hotkeyId);
            }
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
                case "ctrl":
                    modifiers |= (uint)KeyModifiers.Control;
                    break;
                case "alt":
                    modifiers |= (uint)KeyModifiers.Alt;
                    break;
                case "shift":
                    modifiers |= (uint)KeyModifiers.Shift;
                    break;
                case "win":
                    modifiers |= (uint)KeyModifiers.Win;
                    break;
                default:
                    try
                    {
                        key = (uint)(Keys)Enum.Parse(typeof(Keys), part, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Error parsing key part: {part}. Exception: {ex.Message}"
                        );
                        return false;
                    }
                    break;
            }
        }
        return key != 0;
    }
}

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
        {
            WindowManager.ToggleWindowState();
        }
        base.WndProc(ref m);
    }
}

public class TrayIconManager
{
    private NotifyIcon trayIcon;
    private ContextMenu trayMenu;

    public TrayIconManager()
    {
        trayMenu = new ContextMenu();
        trayMenu.MenuItems.Add("Exit", OnExit);

        trayIcon = new NotifyIcon
        {
            Text = WindowManager.Args1,
            Icon = new Icon(SystemIcons.Application, 40, 40),
            ContextMenu = trayMenu,
            Visible = true
        };

        trayIcon.MouseClick += TrayIcon_MouseClick;
    }

    private void OnExit(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            WindowManager.ToggleWindow();
        }
    }

    public void Dispose()
    {
        if (trayIcon != null)
        {
            trayIcon.Dispose();
        }
    }
}

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
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: wcs-tray <WindowTitle or Hwnd> <HotKey>");
            return;
        }
        WindowManager.InitHwnd(args[1]);
        Thread monitorThread = new Thread(WindowManager.MonitorWindowProcessExit);
        monitorThread.Start();

        Application.ApplicationExit += OnApplicationExit;

        _trayIconManager = new TrayIconManager();
        _hiddenWindow = new HiddenWindow();

        if (HotKeyManager.TryParseHotKey(args[2], out uint modifiers, out uint key))
        {
            _hotKeyManager = new HotKeyManager(_hiddenWindow.Handle, HOTKEY_ID, modifiers, key);
        }
        else
        {
            Console.WriteLine("Invalid hotkey format. Use format like 'Ctrl+Alt+X'.");
            return;
        }

        Application.Run();
    }

    private static void OnApplicationExit(object sender, EventArgs e)
    {
        Console.WriteLine("Application is exiting. Cleaning up...");

        _hotKeyManager.Dispose();
        _trayIconManager.Dispose();
    }
}
