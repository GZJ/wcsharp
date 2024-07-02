using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public static class WindowManager
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    public static string WindowTitle { get; set; }

    public static void ToggleWindowState()
    {
        IntPtr hWnd = FindWindow(null, WindowTitle);
        if (hWnd != IntPtr.Zero)
        {
            bool isMinimized = IsIconic(hWnd);
            if (isMinimized)
            {
                WinRestore();
                WinFocuse();
            }
            else if (IsWindowFocused(hWnd))
            {
                WinMin();
            }
            else {
                WinFocuse();
            }
        }
    }

    public static bool IsWindowFocused(IntPtr hWnd)
    {
        IntPtr foregroundWindowHandle = GetForegroundWindow();
        return foregroundWindowHandle == hWnd;
    }

    public static void WinFocuse()
    {
        RunCommand("cmd.exe", $"/c wcs-focuse.exe \"{WindowTitle}\"");
    }

    public static void WinMin()
    {
        RunCommand("cmd.exe", $"/c wcs-min.exe \"{WindowTitle}\"");
    }

    public static void WinRestore()
    {
        RunCommand("cmd.exe", $"/c wcs-restore.exe \"{WindowTitle}\"");
    }

    public static void RunCommand(string processName, string arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo(processName, arguments)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.Start();
            process.WaitForExit();
        }
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
            Text = WindowManager.WindowTitle,
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
            WindowManager.ToggleWindowState();
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
            Console.WriteLine("Usage: wcs-tray <WindowTitle> <HotKey>");
            return;
        }
        WindowManager.WindowTitle = args[1];

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

