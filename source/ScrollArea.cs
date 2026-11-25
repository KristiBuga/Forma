using System;
using System.Drawing;
using System.Windows.Forms;
class ScrollArea:Panel,IMessageFilter {
 public readonly Panel Content=new Panel();readonly ScrollGrip grip;int offset,extent;bool registered;
 public int Offset{get{return offset;}}
 public ScrollArea(){DoubleBuffered=true;BackColor=Skin.Bg;Content.BackColor=BackColor;Controls.Add(Content);grip=new ScrollGrip(this);Controls.Add(grip);}
 public void SetExtent(int height){extent=Math.Max(0,height);Content.Size=new Size(Math.Max(1,ClientSize.Width-12),extent);MoveTo(offset);}
 public void MoveTo(int value){offset=Math.Max(0,Math.Min(Math.Max(0,extent-ClientSize.Height),value));Content.Location=new Point(0,-offset);grip.Visible=extent>ClientSize.Height;grip.SetBounds(Width-9,4,7,Math.Max(1,Height-8));grip.BringToFront();grip.Invalidate();}
 protected override void OnResize(EventArgs e){base.OnResize(e);Content.Width=Math.Max(1,Width-12);MoveTo(offset);}
 protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);Application.AddMessageFilter(this);registered=true;}
 public bool PreFilterMessage(ref Message m){if(m.Msg!=0x20A||!Visible||extent<=Height)return false;Control owner=Control.FromChildHandle(m.HWnd);for(Control c=owner;c!=null;c=c.Parent){if(c==this){int delta=(short)((m.WParam.ToInt64()>>16)&65535);MoveTo(offset-delta*96/120);return true;}}return false;}
 protected override void Dispose(bool disposing){if(disposing&&registered){Application.RemoveMessageFilter(this);registered=false;}base.Dispose(disposing);}
 class ScrollGrip:Control {readonly ScrollArea area;bool dragging;int last;public ScrollGrip(ScrollArea p){area=p;DoubleBuffered=true;Cursor=Cursors.SizeNS;}
  protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(area.BackColor);int total=Math.Max(area.extent,area.Height);int size=Math.Max(26,Height*area.Height/Math.Max(1,total));int y=(int)((Height-size)*(area.offset/(double)Math.Max(1,area.extent-area.Height)));using(var b=new SolidBrush(Color.FromArgb(89,83,110)))e.Graphics.FillRectangle(b,2,y,3,size);}
  protected override void OnMouseDown(MouseEventArgs e){dragging=true;Capture=true;last=PointToScreen(e.Location).Y;base.OnMouseDown(e);}
  protected override void OnMouseMove(MouseEventArgs e){if(dragging){int y=PointToScreen(e.Location).Y;area.MoveTo(area.offset+(int)((y-last)*area.extent/(double)Math.Max(1,Height)));last=y;}base.OnMouseMove(e);}
  protected override void OnMouseUp(MouseEventArgs e){dragging=false;Capture=false;base.OnMouseUp(e);}
 }
}
