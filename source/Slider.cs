using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
class Slider : Control {
 int min=0,max=100,value=0;public event EventHandler ValueChanged;
 public int Minimum{get{return min;}set{min=value;Value=this.value;Invalidate();}}public int Maximum{get{return max;}set{max=value;Value=this.value;Invalidate();}}
 public int Value{get{return value;}set{int next=Math.Max(min,Math.Min(max,value));if(this.value==next)return;this.value=next;Invalidate();if(ValueChanged!=null)ValueChanged(this,EventArgs.Empty);}}
 public Slider(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.Selectable,true);TabStop=true;Cursor=Cursors.Hand;BackColor=Skin.Card;Height=36;}
 void UpdateValue(int x){Value=min+(int)Math.Round(Math.Max(0,Math.Min(1,(x-10d)/Math.Max(1,Width-20)))*(max-min));}
 protected override void OnMouseDown(MouseEventArgs e){if(e.Button==MouseButtons.Left){Focus();Capture=true;UpdateValue(e.X);}base.OnMouseDown(e);}
 protected override void OnMouseMove(MouseEventArgs e){if(Capture&&e.Button==MouseButtons.Left)UpdateValue(e.X);base.OnMouseMove(e);}
 protected override void OnMouseUp(MouseEventArgs e){Capture=false;base.OnMouseUp(e);}
 protected override bool IsInputKey(Keys key){return key==Keys.Left||key==Keys.Right||key==Keys.Home||key==Keys.End||base.IsInputKey(key);}
 protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Left)Value--;if(e.KeyCode==Keys.Right)Value++;if(e.KeyCode==Keys.Home)Value=min;if(e.KeyCode==Keys.End)Value=max;base.OnKeyDown(e);}
 protected override void OnGotFocus(EventArgs e){Invalidate();base.OnGotFocus(e);}protected override void OnLostFocus(EventArgs e){Invalidate();base.OnLostFocus(e);}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(BackColor);float x=10+(Width-20)*(value-min)/(float)Math.Max(1,max-min),y=Height/2f;using(var track=new Pen(Color.FromArgb(57,63,88),6)){track.StartCap=track.EndCap=LineCap.Round;g.DrawLine(track,10,y,Width-10,y);}if(x>10)using(var fill=new Pen(Color.FromArgb(89,108,250),6)){fill.StartCap=fill.EndCap=LineCap.Round;g.DrawLine(fill,10,y,x,y);}if(Focused)using(var b=new SolidBrush(Color.FromArgb(52,51,88)))g.FillEllipse(b,x-11,y-11,22,22);using(var b=new SolidBrush(Color.FromArgb(148,122,255)))g.FillEllipse(b,x-7,y-7,14,14);}
}
