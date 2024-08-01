using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WindowInfoApp
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetClassName(
            IntPtr hWnd,
            StringBuilder lpClassName,
            int nMaxCount
        );

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private DataGridView activeWindowDataGridView;
        private DataGridView selfWindowDataGridView;
        private CheckBox topMostCheckBox;
        private Timer checkForegroundWindowTimer;

        public MainForm()
        {
            InitializeComponent();
            InitializeDataGridViews();
            InitializeTopMostCheckBox();
            InitializeTimer();
            UpdateSelfWindowInfo();
            UpdateWindowInfo(GetForegroundWindow(), activeWindowDataGridView);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Name = "MainForm";
            this.Text = "wcs-inspector-win-info";
            this.ResumeLayout(false);
        }

        private void InitializeDataGridViews()
        {
            activeWindowDataGridView = CreateDataGridView("Active Window Info");
            activeWindowDataGridView.Location = new Point(10, 30);
            activeWindowDataGridView.Size = new Size(580, 150);
            this.Controls.Add(activeWindowDataGridView);

            selfWindowDataGridView = CreateDataGridView("Self Window Info");
            selfWindowDataGridView.Location = new Point(10, 210);
            selfWindowDataGridView.Size = new Size(580, 150);
            this.Controls.Add(selfWindowDataGridView);
        }

        private DataGridView CreateDataGridView(string name)
        {
            DataGridView dgv = new DataGridView();
            dgv.Name = name;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.BackgroundColor = SystemColors.Window;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv.ColumnHeadersVisible = false;
            dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

            DataTable dt = new DataTable();
            dt.Columns.Add("Property", typeof(string));
            dt.Columns.Add("Value", typeof(string));
            dgv.DataSource = dt;

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem copyMenuItem = new ToolStripMenuItem("Copy");
            copyMenuItem.Click += (sender, e) => CopySelectedCells(dgv);
            contextMenu.Items.Add(copyMenuItem);
            dgv.ContextMenuStrip = contextMenu;

            dgv.KeyDown += (sender, e) =>
            {
                if (e.Control && e.KeyCode == Keys.C)
                {
                    CopySelectedCells(dgv);
                    e.Handled = true;
                }
            };

            return dgv;
        }

        private void CopySelectedCells(DataGridView dgv)
        {
            if (dgv.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                Clipboard.SetDataObject(dgv.GetClipboardContent());
            }
        }

        private void InitializeTopMostCheckBox()
        {
            topMostCheckBox = new CheckBox();
            topMostCheckBox.Text = "Pin on Top";
            topMostCheckBox.AutoSize = true;
            topMostCheckBox.Location = new Point(this.ClientSize.Width - 100, 5);
            topMostCheckBox.CheckedChanged += TopMostCheckBox_CheckedChanged;
            this.Controls.Add(topMostCheckBox);
        }

        private void TopMostCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = topMostCheckBox.Checked;
        }

        private void InitializeTimer()
        {
            checkForegroundWindowTimer = new Timer();
            checkForegroundWindowTimer.Interval = 100;
            checkForegroundWindowTimer.Tick += CheckForegroundWindowTimer_Tick;
            checkForegroundWindowTimer.Start();
        }

        private IntPtr lastActiveWindow = IntPtr.Zero;

        private void CheckForegroundWindowTimer_Tick(object sender, EventArgs e)
        {
            IntPtr activeWindow = GetForegroundWindow();
            if (activeWindow != lastActiveWindow && activeWindow != this.Handle)
            {
                UpdateWindowInfo(activeWindow, activeWindowDataGridView);
                lastActiveWindow = activeWindow;
            }
        }

        private void UpdateWindowInfo(IntPtr hWnd, DataGridView dgv)
        {
            StringBuilder title = new StringBuilder(256);
            StringBuilder className = new StringBuilder(256);
            GetWindowText(hWnd, title, title.Capacity);
            GetClassName(hWnd, className, className.Capacity);

            uint processId;
            GetWindowThreadProcessId(hWnd, out processId);

            try
            {
                Process process = Process.GetProcessById((int)processId);
                string fileName = process.MainModule.FileName;

                DataTable dt = (DataTable)dgv.DataSource;
                dt.Rows.Clear();

                dt.Rows.Add("Title", title.ToString());
                dt.Rows.Add("Class", className.ToString());
                dt.Rows.Add("Executable", System.IO.Path.GetFileName(fileName));
                dt.Rows.Add("PID", processId.ToString());
                dt.Rows.Add("HWND", hWnd.ToInt64().ToString());
            }
            catch (Exception)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                dt.Rows.Clear();
                dt.Rows.Add("Error", "Unable to retrieve window information.");
            }
        }

        private void UpdateSelfWindowInfo()
        {
            UpdateWindowInfo(this.Handle, selfWindowDataGridView);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
