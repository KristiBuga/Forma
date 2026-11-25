using System.Drawing;
using System.Drawing.Drawing2D;
class NavButton : GlowButton {
 public int Category;
 public NavButton(){Font=new Font("Segoe UI",9,FontStyle.Bold);}
 protected override Rectangle TextBounds{get{return new Rectangle(39,0,Width-46,Height);}}
 protected override void OnPaint(System.Windows.Forms.PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;var state=g.Save();g.TranslateTransform(12,(Height-20)/2f);g.SmoothingMode=SmoothingMode.AntiAlias;Color color=!Enabled?Color.FromArgb(92,98,121):Primary?Color.White:Color.FromArgb(180,170,235);using(var p=new Pen(color,1.6f)){p.StartCap=p.EndCap=LineCap.Round;p.LineJoin=LineJoin.Round;using(var b=new SolidBrush(color))switch(Category){
  case 0:g.DrawLines(p,new[]{new Point(7,15),new Point(7,4),new Point(17,2),new Point(17,13)});g.DrawLine(p,7,7,17,5);g.FillEllipse(b,2,13,6,5);g.FillEllipse(b,12,11,6,5);break;
  case 1:using(var path=Skin.Round(new Rectangle(1,3,19,14),3))g.DrawPath(p,path);g.FillPolygon(b,new[]{new Point(8,6),new Point(8,14),new Point(14,10)});break;
  case 2:using(var path=Skin.Round(new Rectangle(1,2,19,16),3))g.DrawPath(p,path);g.DrawLines(p,new[]{new Point(3,15),new Point(8,10),new Point(12,14),new Point(15,11),new Point(18,15)});g.FillEllipse(b,13,5,3,3);break;
  case 3:g.DrawLines(p,new[]{new Point(4,1),new Point(13,1),new Point(18,6),new Point(18,19),new Point(4,19),new Point(4,1)});g.DrawLines(p,new[]{new Point(13,1),new Point(13,6),new Point(18,6)});g.DrawLine(p,7,10,14,10);g.DrawLine(p,7,14,14,14);break;
  case 4:g.DrawLine(p,2,4,20,4);g.DrawLine(p,2,10,20,10);g.DrawLine(p,2,16,20,16);using(var fill=new SolidBrush(Primary?Color.FromArgb(111,82,238):Color.FromArgb(33,37,56))){g.FillEllipse(fill,5,1,6,6);g.FillEllipse(fill,12,7,6,6);g.FillEllipse(fill,6,13,6,6);}g.DrawEllipse(p,5,1,6,6);g.DrawEllipse(p,12,7,6,6);g.DrawEllipse(p,6,13,6,6);break;
 }}g.Restore(state);}
}
