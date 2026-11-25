using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

partial class Converter {
    NotifyIcon trayIcon;
    bool trayExitPending;

    [DllImport("user32.dll")]
    static extern bool DestroyIcon(IntPtr handle);

    void InitializeTray() {
        var menu = new ContextMenuStrip {
            RenderMode = ToolStripRenderMode.System,
            ShowImageMargin = true,
            AccessibleName = "Forma"
        };
        var badge = new Bitmap(16, 16);
        var logoPath = Path.Combine(root, "Forma-logo.png");
        using (var graphics = Graphics.FromImage(badge)) {
            if (File.Exists(logoPath)) {
                using (var logo = Image.FromFile(logoPath))
                    graphics.DrawImage(logo, new Rectangle(0, 0, badge.Width, badge.Height));
            } else {
                graphics.DrawIcon(SystemIcons.Application, new Rectangle(0, 0, badge.Width, badge.Height));
            }
        }
        IntPtr iconHandle = badge.GetHicon();
        var icon = System.Drawing.Icon.FromHandle(iconHandle);
        var heading = new ToolStripLabel("Forma", badge);
        var open = new ToolStripMenuItem("Открыть Forma");
        var results = new ToolStripMenuItem("Открыть папку результатов");
        var exit = new ToolStripMenuItem("Выйти из Forma");
        menu.Items.AddRange(new ToolStripItem[] {
            heading, new ToolStripSeparator(), open, results, new ToolStripSeparator(), exit
        });
        menu.Opening += (sender, args) => {
            heading.Text = appBusy ? "Forma  ·  Идёт обработка" : "Forma  ·  Готово к работе";
            exit.Enabled = !trayExitPending;
        };
        open.Click += (sender, args) => RestoreFromTray();
        results.Click += (sender, args) => OpenResultsFromTray();
        exit.Click += (sender, args) => BeginInvoke(new Action(ExitFromTray));
        trayIcon = new NotifyIcon { Icon = icon, Text = "Forma", ContextMenuStrip = menu };
        trayIcon.MouseDoubleClick += (sender, args) => {
            if (args.Button == MouseButtons.Left) RestoreFromTray();
        };
        Shown += (sender, args) => trayIcon.Visible = true;
        Disposed += (sender, args) => {
            trayIcon.Dispose();
            menu.Dispose();
            icon.Dispose();
            DestroyIcon(iconHandle);
            badge.Dispose();
        };
    }

    void RestoreFromTray() {
        if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
        Activate();
    }

    void OpenResultsFromTray() {
        var directory = dest.Text;
        try {
            Directory.CreateDirectory(directory);
            OpenOutputDirectory(directory);
        } catch (Exception ex) {
            RestoreFromTray();
            ShowNotice("Не удалось открыть папку результатов. " + ex.Message);
        }
    }

    void ExitFromTray() {
        if (appBusy || !run.Enabled) {
            trayExitPending = true;
            Stop();
            status.Text = "Останавливаем обработку…";
        } else {
            Close();
        }
    }

    void FinishPendingTrayExit() {
        if (trayExitPending) BeginInvoke(new Action(Close));
    }
}
