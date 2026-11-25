using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class ChromeForm:Form {
 [DllImport("user32.dll")]static extern bool ReleaseCapture();
 [DllImport("user32.dll")]static extern IntPtr SendMessage(IntPtr h,int message,IntPtr w,IntPtr l);
 public void DragWindow(){ReleaseCapture();SendMessage(Handle,0xA1,(IntPtr)2,IntPtr.Zero);}
 public void ToggleMaximize(){MaximizedBounds=Screen.FromControl(this).WorkingArea;WindowState=WindowState==FormWindowState.Maximized?FormWindowState.Normal:FormWindowState.Maximized;}
 protected override void WndProc(ref Message m){base.WndProc(ref m);if(m.Msg==0x84&&WindowState==FormWindowState.Normal){long p=m.LParam.ToInt64();var c=PointToClient(new Point((short)(p&65535),(short)((p>>16)&65535)));bool left=c.X<6,right=c.X>=ClientSize.Width-6,top=c.Y<6,bottom=c.Y>=ClientSize.Height-6;if(left&&top)m.Result=(IntPtr)13;else if(right&&top)m.Result=(IntPtr)14;else if(left&&bottom)m.Result=(IntPtr)16;else if(right&&bottom)m.Result=(IntPtr)17;else if(left)m.Result=(IntPtr)10;else if(right)m.Result=(IntPtr)11;else if(top)m.Result=(IntPtr)12;else if(bottom)m.Result=(IntPtr)15;}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);using(var pen=new Pen(Skin.Line))e.Graphics.DrawRectangle(pen,0,0,ClientSize.Width-1,ClientSize.Height-1);}
}
class StepIndicator:Control {
 public int Step;public bool HasResults;public event Action<int> Navigate;
 public StepIndicator(){DoubleBuffered=true;Height=76;Cursor=Cursors.Hand;}
 protected override void OnMouseUp(MouseEventArgs e){int n=Math.Max(0,Math.Min(2,e.X*3/Math.Max(1,Width)));if(n<2||HasResults)if(Navigate!=null)Navigate(n);base.OnMouseUp(e);}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(BackColor);string[] names={"Файлы","Настройки","Результат"};for(int n=0;n<3;n++){int x=(int)(Width*(n+.5)/3d);if(n<2)using(var line=new Pen(n<Step?Color.FromArgb(145,111,221):Skin.Line,2))g.DrawLine(line,x+20,23,(int)(Width*(n+1.5)/3d)-20,23);using(var b=new SolidBrush(n==Step?Color.FromArgb(151,116,232):n<Step?Color.FromArgb(68,56,95):Skin.Card))g.FillEllipse(b,x-16,7,32,32);TextRenderer.DrawText(g,n<Step?"✓":(n+1).ToString(),Font,new Rectangle(x-16,7,32,32),n<=Step?Skin.Text:Skin.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);TextRenderer.DrawText(g,names[n],Font,new Rectangle(x-70,47,140,24),n==Step?Skin.Text:Skin.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);}}
}
class Glyph {
 public static void File(Graphics g,Rectangle r,string ext,Color color){using(var b=new SolidBrush(Color.FromArgb(58,55,75)))using(var path=Skin.Round(r,9))g.FillPath(b,path);using(var font=new Font("Segoe UI",8,FontStyle.Bold))TextRenderer.DrawText(g,ext,font,r,color,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);}
}
class OperationButton:GlowButton {
 public int Operation;public bool Chosen;
 protected override Rectangle TextBounds{get{return new Rectangle(4,51,Width-8,Height-53);}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;Color c=Chosen?Color.FromArgb(188,156,242):Skin.Muted;int x=Width/2,y=27;using(var p=new Pen(c,1.8f)){p.StartCap=p.EndCap=LineCap.Round;if(Operation==0){g.DrawLine(p,x-12,y-5,x+12,y-5);g.DrawLines(p,new[]{new Point(x+7,y-10),new Point(x+12,y-5),new Point(x+7,y)});g.DrawLine(p,x+12,y+7,x-12,y+7);g.DrawLines(p,new[]{new Point(x-7,y+2),new Point(x-12,y+7),new Point(x-7,y+12)});}else if(Operation==1){g.DrawRectangle(p,x-18,y-13,36,26);using(var f=new Font("Segoe UI",8,FontStyle.Bold))TextRenderer.DrawText(g,"GIF",f,new Rectangle(x-18,y-13,36,26),c,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);}else{g.DrawLines(p,new[]{new Point(x,y+9),new Point(x,y-11),new Point(x+10,y-14),new Point(x+10,y+5)});g.DrawEllipse(p,x-6,y+7,7,5);g.DrawEllipse(p,x+4,y+3,7,5);}}if(Chosen)using(var path=Skin.Round(new Rectangle(1,1,Width-3,Height-3),8))using(var p=new Pen(c,1.8f))g.DrawPath(p,path);}
}
