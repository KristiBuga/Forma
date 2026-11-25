using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

enum WindowIcon { Minimize, Maximize, Restore, Close }

sealed class WindowButton : GlowButton {
    WindowIcon icon;

    public WindowButton(WindowIcon icon) {
        this.icon = icon;
    }

    public void SetIcon(WindowIcon value) {
        if (icon == value) return;
        icon = value;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);
        var g = e.Graphics;
        float x = Width / 2f;
        float y = Height / 2f;
        var color = icon == WindowIcon.Close ? Color.FromArgb(255, 162, 174) : Skin.Text;
        using (var pen = new Pen(color, 1.75f)) {
            pen.StartCap = pen.EndCap = LineCap.Round;
            pen.LineJoin = LineJoin.Round;
            switch (icon) {
                case WindowIcon.Minimize:
                    g.DrawLine(pen, x - 6, y, x + 6, y);
                    break;
                case WindowIcon.Maximize:
                    g.DrawRectangle(pen, x - 5.5f, y - 5.5f, 11, 11);
                    break;
                case WindowIcon.Restore:
                    g.DrawLines(pen, new PointF[] {
                        new PointF(x - 2, y - 2), new PointF(x - 2, y - 6),
                        new PointF(x + 6, y - 6), new PointF(x + 6, y + 2),
                        new PointF(x + 2, y + 2)
                    });
                    g.DrawRectangle(pen, x - 6, y - 2, 8, 8);
                    break;
                case WindowIcon.Close:
                    g.DrawLine(pen, x - 4.5f, y - 4.5f, x + 4.5f, y + 4.5f);
                    g.DrawLine(pen, x + 4.5f, y - 4.5f, x - 4.5f, y + 4.5f);
                    break;
            }
        }
    }
}
