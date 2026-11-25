using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
class ChoicePopup : ToolStripDropDown {
 public ChoiceList List;
 public ChoicePopup(){Padding=Padding.Empty;Margin=Padding.Empty;DropShadowEnabled=false;AutoSize=false;BackColor=Color.FromArgb(28,31,48);}
 protected override void OnPaintBackground(PaintEventArgs e){e.Graphics.Clear(BackColor);}
 protected override bool ProcessDialogKey(Keys keyData){if(List!=null&&List.Navigate(keyData))return true;return base.ProcessDialogKey(keyData);}
}
class ChoiceList : Control {
 const int Row=48,Inset=8;readonly IList<object> items;readonly int selected;readonly Action<int> choose;readonly Action close;readonly Timer timer=new Timer();int offset,hover=-1,cursor,edge;Point pointer;
 public int ScrollOffset{get{return offset;}}
 int MaximumOffset{get{return Math.Max(0,items.Count*Row+Inset*2-Height);}}
 public ChoiceList(IList<object> entries,int current,Action<int> picked,Action dismissed){items=new List<object>(entries);selected=current;cursor=Math.Max(0,current);choose=picked;close=dismissed;Font=new Font("Segoe UI",10);BackColor=Color.FromArgb(28,31,48);TabStop=true;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);timer.Interval=35;timer.Tick+=(s,e)=>Step();}
 public void RevealSelection(){Reveal(cursor);}
 void Reveal(int index){int top=Inset+index*Row,bottom=top+Row;if(top<offset+Inset)Scroll(top-Inset-offset);else if(bottom>offset+Height-Inset)Scroll(bottom-(offset+Height-Inset));}
 void Scroll(int amount){offset=Math.Max(0,Math.Min(MaximumOffset,offset+amount));hover=Hit(pointer.Y);Invalidate();}
 int Hit(int y){int n=(y+offset-Inset)/Row;return y>=Inset&&y<Height-Inset&&n>=0&&n<items.Count?n:-1;}
 void Step(){if(!Visible||edge==0){timer.Stop();return;}int previous=offset;Scroll(edge*10);if(previous==offset)timer.Stop();}
 public void StopScrolling(){timer.Stop();edge=0;}
 protected override void OnMouseMove(MouseEventArgs e){pointer=e.Location;hover=Hit(e.Y);edge=MaximumOffset==0?0:e.Y<30?-1:e.Y>Height-30?1:0;if(edge!=0)timer.Start();else timer.Stop();Invalidate();base.OnMouseMove(e);}
 protected override void OnMouseLeave(EventArgs e){StopScrolling();hover=-1;Invalidate();base.OnMouseLeave(e);}
 protected override void OnMouseWheel(MouseEventArgs e){StopScrolling();pointer=e.Location;Scroll(-e.Delta*Row*3/120);base.OnMouseWheel(e);}
 protected override void OnMouseUp(MouseEventArgs e){if(e.Button==MouseButtons.Left){int hit=Hit(e.Y);if(hit>=0){StopScrolling();choose(hit);}}base.OnMouseUp(e);}
 public bool Navigate(Keys key){switch(key&Keys.KeyCode){case Keys.Down:cursor=Math.Min(items.Count-1,cursor+1);break;case Keys.Up:cursor=Math.Max(0,cursor-1);break;case Keys.Home:cursor=0;break;case Keys.End:cursor=items.Count-1;break;case Keys.PageDown:cursor=Math.Min(items.Count-1,cursor+Math.Max(1,(Height-16)/Row));break;case Keys.PageUp:cursor=Math.Max(0,cursor-Math.Max(1,(Height-16)/Row));break;case Keys.Enter:case Keys.Space:StopScrolling();if(cursor>=0&&cursor<items.Count)choose(cursor);return true;case Keys.Escape:close();return true;default:return false;}StopScrolling();Reveal(cursor);hover=cursor;Invalidate();return true;}
 protected override bool IsInputKey(Keys key){return true;}
 protected override void OnKeyDown(KeyEventArgs e){if(Navigate(e.KeyData)){e.Handled=true;e.SuppressKeyPress=true;}base.OnKeyDown(e);}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(BackColor);var state=g.Save();g.SetClip(new Rectangle(Inset,Inset,Width-Inset*2,Height-Inset*2));for(int n=Math.Max(0,(offset-Inset)/Row);n<items.Count;n++){int y=Inset+n*Row-offset;if(y>=Height-Inset)break;var rect=new Rectangle(Inset,y+2,Width-Inset*2-6,Row-4);if(n==hover||n==selected){using(var path=Skin.Round(rect,10))using(var b=new SolidBrush(n==hover?Color.FromArgb(54,52,82):Color.FromArgb(38,39,62)))g.FillPath(b,path);}TextRenderer.DrawText(g,items[n].ToString(),Font,new Rectangle(Inset+12,y,Width-66,Row),n==selected?Color.FromArgb(189,173,255):Skin.Text,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.PreserveGraphicsClipping);if(n==selected)TextRenderer.DrawText(g,"✓",Font,new Rectangle(Width-43,y,20,Row),Color.FromArgb(165,138,255),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.PreserveGraphicsClipping);}
  g.Restore(state);if(MaximumOffset>0){float h=Math.Max(26,(Height-20)*(Height-20)/(float)(items.Count*Row)),y=10+(Height-20-h)*offset/MaximumOffset;using(var b=new SolidBrush(Color.FromArgb(112,101,162)))g.FillRectangle(b,Width-6,y,2,h);}
  using(var path=Skin.Round(new Rectangle(0,0,Width-1,Height-1),15))using(var pen=new Pen(Color.FromArgb(66,65,98)))g.DrawPath(pen,path);
 }
 protected override void Dispose(bool disposing){if(disposing)timer.Dispose();base.Dispose(disposing);}
}

