using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

static class Skin {
 public static Color Bg=Color.FromArgb(25,27,34), Card=Color.FromArgb(38,40,49), Text=Color.FromArgb(241,241,246), Muted=Color.FromArgb(157,159,174), Line=Color.FromArgb(60,62,76);
 public static GraphicsPath Round(Rectangle r,int radius){var p=new GraphicsPath();int d=radius*2;p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
 public static Label Label(string text,int x,int y,int width,int height,float size,bool bold){return new Label{Text=text,Location=new Point(x,y),Size=new Size(width,height),ForeColor=Text,BackColor=Color.Transparent,Font=new Font("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular)};}
}
class Card : Panel {
 public Card(){DoubleBuffered=true;SetStyle(ControlStyles.ResizeRedraw,true);BackColor=Skin.Bg;}
 protected override void OnSizeChanged(EventArgs e){base.OnSizeChanged(e);Invalidate(true);}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var path=Skin.Round(new Rectangle(0,0,Width-1,Height-1),8)){using(var b=new SolidBrush(Skin.Card))e.Graphics.FillPath(b,path);using(var p=new Pen(Skin.Line))e.Graphics.DrawPath(p,path);}base.OnPaint(e);}
}
class GlowButton : Button {
 public bool Primary;bool hover;
 protected virtual Rectangle TextBounds{get{return ClientRectangle;}}
 public GlowButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Cursor=Cursors.Hand;Font=new Font("Segoe UI",10,FontStyle.Bold);BackColor=Skin.Card;ForeColor=Skin.Text;}
 protected override void OnResize(EventArgs e){base.OnResize(e);if(Width>24&&Height>24)using(var path=Skin.Round(new Rectangle(0,0,Width,Height),8)){var old=Region;Region=new Region(path);if(old!=null)old.Dispose();}}
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Parent is Card?Skin.Card:(Parent==null?Skin.Bg:Parent.BackColor));g.SmoothingMode=SmoothingMode.AntiAlias;using(var path=Skin.Round(new Rectangle(0,0,Width-1,Height-1),8)){if(Primary&&Enabled){using(var b=new LinearGradientBrush(ClientRectangle,Color.FromArgb(151,116,226),hover?Color.FromArgb(174,139,242):Color.FromArgb(122,87,194),15))g.FillPath(b,path);}else{using(var b=new SolidBrush(hover&&Enabled?Color.FromArgb(65,63,80):Color.FromArgb(47,49,60)))g.FillPath(b,path);using(var p=new Pen(Skin.Line))g.DrawPath(p,path);}}TextRenderer.DrawText(g,Text,Font,TextBounds,Enabled?Skin.Text:Color.FromArgb(92,98,121),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);if(Focused&&ShowFocusCues){using(var focus=Skin.Round(new Rectangle(3,3,Width-7,Height-7),9))using(var pen=new Pen(Color.FromArgb(174,158,255)))g.DrawPath(pen,focus);}}
}
class InputFrame : Card {
 protected override void OnResize(EventArgs e){base.OnResize(e);if(Width>36&&Height>36)using(var path=Skin.Round(new Rectangle(0,0,Width,Height),8)){var old=Region;Region=new Region(path);if(old!=null)old.Dispose();}}
}
class Meter : Control {
 int val,max=1;public int Value{get{return val;}set{val=value;Invalidate();}}public int Maximum{get{return max;}set{max=Math.Max(1,value);Invalidate();}}
 public Meter(){DoubleBuffered=true;}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(Skin.Card);using(var b=new SolidBrush(Skin.Line))e.Graphics.FillRectangle(b,ClientRectangle);int w=(int)(Width*Math.Min(1,(double)val/max));if(w>0)using(var b=new LinearGradientBrush(ClientRectangle,Color.FromArgb(66,110,255),Color.FromArgb(148,80,244),0f))e.Graphics.FillRectangle(b,0,0,w,Height);}
}
class Choice : GlowButton {
 protected override Rectangle TextBounds{get{return new Rectangle(8,0,Width-40,Height);}}
 ChoicePopup menu;
 protected override void Dispose(bool disposing){if(disposing&&menu!=null){menu.Dispose();menu=null;}base.Dispose(disposing);}
 public System.Collections.Generic.List<object> Items=new System.Collections.Generic.List<object>();int index=-1;
 public event EventHandler SelectedIndexChanged;
 public int SelectedIndex {get{return index;}set{index=value;Text=value>=0&&value<Items.Count?Items[value].ToString():"";Invalidate();if(SelectedIndexChanged!=null)SelectedIndexChanged(this,EventArgs.Empty);}}
 protected override void OnClick(EventArgs e){base.OnClick(e);if(Items.Count==0)return;if(menu!=null){menu.Dispose();menu=null;}var popup=new ChoicePopup();menu=popup;var list=new ChoiceList(Items,index,picked=>{SelectedIndex=picked;popup.Close();Focus();},()=>{popup.Close();Focus();});int popupWidth=Width;using(var measureFont=new Font("Segoe UI",10)){foreach(var entry in Items)popupWidth=Math.Max(popupWidth,TextRenderer.MeasureText(entry.ToString(),measureFont).Width+70);}popupWidth=Math.Min(popupWidth,Screen.FromControl(this).WorkingArea.Width-32);list.Size=new Size(popupWidth,Math.Min(Items.Count*48+16,Math.Min(420,Screen.FromControl(this).WorkingArea.Height-40)));popup.List=list;var host=new ToolStripControlHost(list){Margin=Padding.Empty,Padding=Padding.Empty,AutoSize=false,Size=list.Size};popup.Items.Add(host);popup.Size=list.Size;popup.Closed+=(s,args)=>list.StopScrolling();popup.Opened+=(s,args)=>{using(var path=Skin.Round(new Rectangle(0,0,popup.Width,popup.Height),15))popup.Region=new Region(path);list.RevealSelection();list.Focus();};popup.Show(this,new Point(0,Height+8));}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;float cx=Width-22,cy=Height/2f;using(var pen=new Pen(Enabled?Skin.Muted:Color.FromArgb(80,85,105),1.7f)){pen.StartCap=pen.EndCap=System.Drawing.Drawing2D.LineCap.Round;pen.LineJoin=System.Drawing.Drawing2D.LineJoin.Round;e.Graphics.DrawLines(pen,new PointF[]{new PointF(cx-4,cy-2),new PointF(cx,cy+2),new PointF(cx+4,cy-2)});}}
 class ChoiceRenderer:ToolStripProfessionalRenderer {
  protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e){e.Graphics.Clear(Color.FromArgb(28,31,48));}
  protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e){e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;using(var path=Skin.Round(new Rectangle(0,0,e.ToolStrip.Width-1,e.ToolStrip.Height-1),15))using(var pen=new Pen(Color.FromArgb(66,65,98)))e.Graphics.DrawPath(pen,path);}
  protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e){bool current=e.Item.Tag is bool&&(bool)e.Item.Tag;if(!e.Item.Selected&&!current)return;e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;using(var path=Skin.Round(new Rectangle(1,1,e.Item.Width-3,e.Item.Height-2),10))using(var b=new SolidBrush(e.Item.Selected?Color.FromArgb(54,52,82):Color.FromArgb(38,39,62)))e.Graphics.FillPath(b,path);}
  protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e){bool current=e.Item.Tag is bool&&(bool)e.Item.Tag;var bounds=new Rectangle(15,0,e.Item.Width-56,e.Item.Height);TextRenderer.DrawText(e.Graphics,e.Text,e.TextFont,bounds,current?Color.FromArgb(189,173,255):Skin.Text,TextFormatFlags.VerticalCenter|TextFormatFlags.Left|TextFormatFlags.EndEllipsis);if(current)TextRenderer.DrawText(e.Graphics,"✓",e.TextFont,new Rectangle(e.Item.Width-34,0,24,e.Item.Height),Color.FromArgb(165,138,255),TextFormatFlags.VerticalCenter|TextFormatFlags.HorizontalCenter);}

 }
}











