using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

sealed class LayoutSwitcher : Panel {
    readonly LayoutButton[] buttons = new LayoutButton[3];
    static readonly int[] WideWidths = { 122, 101, 84 };

    public LayoutSwitcher(ToolTip tips, Action<int> chooseMode) {
        BackColor = Color.FromArgb(24, 26, 34);
        AccessibleName = "Вид Forma";
        string[] labels = { "Компактный", "По шагам", "Студия" };
        for (int i = 0; i < buttons.Length; i++) {
            int mode = i;
            var button = new LayoutButton(mode, labels[i]);
            button.Name = "LayoutMode" + i;
            button.Click += (sender, args) => chooseMode(mode);
            tips.SetToolTip(button, labels[i]);
            buttons[i] = button;
            Controls.Add(button);
        }
    }

    public void Place(int right, bool iconsOnly) {
        int x = 3;
        for (int i = 0; i < buttons.Length; i++) {
            int width = iconsOnly ? 38 : WideWidths[i];
            buttons[i].SetIconsOnly(iconsOnly);
            buttons[i].SetBounds(x, 3, width, 34);
            x += width + 4;
        }
        int totalWidth = x - 1;
        SetBounds(right - totalWidth, 13, totalWidth, 40);
    }

    public void SelectMode(int mode) {
        for (int i = 0; i < buttons.Length; i++) buttons[i].SetSelected(i == mode);
    }

    protected override void OnSizeChanged(EventArgs e) {
        base.OnSizeChanged(e);
        if (Width < 20 || Height < 20) return;
        using (var path = Skin.Round(ClientRectangle, 10)) {
            var previous = Region;
            Region = new Region(path);
            if (previous != null) previous.Dispose();
        }
    }

    sealed class LayoutButton : Button {
        readonly int mode;
        bool selected, iconsOnly, hover;

        public LayoutButton(int mode, string label) {
            this.mode = mode;
            AccessibleName = Text = label;
            Font = new Font("Segoe UI", 9.5f);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        public void SetSelected(bool value) {
            if (selected == value) return;
            selected = value;
            AccessibleDescription = value ? "Активный вид" : null;
            Invalidate();
        }

        public void SetIconsOnly(bool value) {
            if (iconsOnly == value) return;
            iconsOnly = value;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hover = false; Invalidate(); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e) {
            var graphics = e.Graphics;
            graphics.Clear(Parent.BackColor);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (selected || (Enabled && hover)) {
                using (var path = Skin.Round(ClientRectangle, 7))
                using (var fill = new SolidBrush(selected ? Color.FromArgb(67, 54, 90) : Color.FromArgb(48, 48, 61)))
                    graphics.FillPath(fill, path);
            }
            Color ink = !Enabled ? Skin.Muted : selected ? Color.FromArgb(224, 204, 255) : Skin.Text;
            int x = iconsOnly ? (Width - 12) / 2 : 11;
            int y = (Height - 12) / 2;
            using (var pen = new Pen(ink, 1.25f)) {
                if (mode == 2) {
                    graphics.DrawRectangle(pen, x + 1, y + 1, 10, 10);
                    graphics.DrawLine(pen, x + 6, y + 1, x + 6, y + 11);
                    graphics.DrawLine(pen, x + 1, y + 6, x + 11, y + 6);
                } else {
                    for (int row = 2; row <= 10; row += 4) {
                        graphics.DrawLine(pen, x + 1, y + row, x + (mode == 0 ? 11 : 3), y + row);
                        if (mode == 1) graphics.DrawLine(pen, x + 5, y + row, x + 11, y + row);
                    }
                }
            }
            if (!iconsOnly) TextRenderer.DrawText(graphics, Text, Font, new Rectangle(29, 0, Width - 33, Height),
                ink, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            if (Focused && ShowFocusCues) {
                using (var path = Skin.Round(Rectangle.Inflate(ClientRectangle, -2, -2), 6))
                using (var pen = new Pen(ink))
                    graphics.DrawPath(pen, path);
            }
        }
    }
}
