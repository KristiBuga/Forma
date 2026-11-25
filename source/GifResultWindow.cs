using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
class GifResultWindow:Form {
 readonly MemoryStream stream;readonly Image animation;
 public GifResultWindow(string file){Text="Forma · Готовый GIF";Size=new Size(760,600);MinimumSize=new Size(480,360);StartPosition=FormStartPosition.CenterParent;BackColor=Skin.Bg;ForeColor=Skin.Text;Font=new Font("Segoe UI",10);stream=new MemoryStream(File.ReadAllBytes(file));animation=Image.FromStream(stream);var image=new PictureBox{Dock=DockStyle.Fill,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.FromArgb(10,12,20),Image=animation};Controls.Add(image);var footer=new Panel{Dock=DockStyle.Bottom,Height=66,BackColor=Skin.Card};Controls.Add(footer);footer.Controls.Add(Skin.Label("Готовый GIF · "+(stream.Length/1048576d).ToString("0.00")+" МБ · "+animation.Width+" × "+animation.Height,16,20,430,30,10,false));var save=new GlowButton{Text="Сохранить как…",Bounds=new Rectangle(480,15,230,38),Primary=true,Anchor=AnchorStyles.Top|AnchorStyles.Right};footer.Controls.Add(save);save.Click+=(s,e)=>{using(var dialog=new SaveFileDialog{Filter="GIF|*.gif",FileName="Forma.gif"})if(dialog.ShowDialog(this)==DialogResult.OK)try{File.Copy(file,dialog.FileName,true);}catch(Exception ex){MessageBox.Show(this,"Не удалось сохранить GIF: "+ex.Message);}};}
 protected override void Dispose(bool disposing){if(disposing){animation.Dispose();stream.Dispose();}base.Dispose(disposing);}
}

