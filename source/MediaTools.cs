using System;
using System.IO;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
class ToolPanel : Panel {
 public TextBox A=new TextBox(),B=new TextBox();Label first,second,help;GlowButton details;public GifOptions Gif=new GifOptions();public event EventHandler EditGif;int activeOperation;
 public int RequiredHeight{get{return Width>=500?136:activeOperation==0?250:activeOperation==1?94:activeOperation==2||activeOperation==3?218:152;}}
 protected override void OnResize(EventArgs e){base.OnResize(e);Reflow();}
 void Reflow(){if(first==null||A.Parent==null||B.Parent==null||help==null||details==null)return;int width=Math.Max(100,Width);if(width<500){int y=0;foreach(var pair in new[]{Tuple.Create(first,A),Tuple.Create(second,B)}){var label=pair.Item1;var box=pair.Item2;if(activeOperation==1||(label==second&&activeOperation!=0&&activeOperation!=2&&activeOperation!=3))continue;label.SetBounds(0,y,width,22);box.Parent.SetBounds(0,y+26,width,38);box.SetBounds(14,9,Math.Max(1,width-28),22);y+=76;}help.SetBounds(0,y,width,activeOperation==0?48:64);details.SetBounds(0,y+54,width,40);}else{int half=(width-16)/2;first.SetBounds(0,0,half,22);second.SetBounds(half+16,0,half,22);A.Parent.SetBounds(0,26,half,38);B.Parent.SetBounds(half+16,26,half,38);A.SetBounds(14,9,Math.Max(1,half-28),22);B.SetBounds(14,9,Math.Max(1,half-28),22);help.SetBounds(0,78,activeOperation==0?width-230:width,52);details.SetBounds(width-214,78,214,40);}}
 public ToolPanel(){BackColor=Skin.Card;first=Skin.Label("",0,0,360,22,9,false);second=Skin.Label("",390,0,360,22,9,false);first.ForeColor=second.ForeColor=Skin.Muted;Controls.Add(first);Controls.Add(second);
  int i=0;foreach(var box in new[]{A,B}){var frame=new InputFrame{BackColor=Skin.Card,Bounds=new Rectangle(i*390,25,370,38)};box.BorderStyle=BorderStyle.None;box.BackColor=Skin.Card;box.ForeColor=Skin.Text;box.Font=new Font("Segoe UI",11);box.AutoSize=false;box.Bounds=new Rectangle(18,9,334,22);frame.Controls.Add(box);Controls.Add(frame);i++;}
  help=Skin.Label("",0,69,760,30,9,false);help.ForeColor=Skin.Muted;Controls.Add(help);
  details=new GlowButton{Text="Настройки GIF…",Bounds=new Rectangle(520,68,240,32),Visible=false};details.Click+=(s,e)=>{if(EditGif!=null)EditGif(this,EventArgs.Empty);};Controls.Add(details);
 }
 public void Configure(int operation){activeOperation=operation;string[] titles={"Видео → GIF","GIF → MP4","Обрезать по времени","Изменить размер","Изменить скорость","Повернуть","Сжать видео"};
  string[] labels={"Ширина GIF, px","","Начало, секунд","Ширина, px (0 — автоматически)","Скорость (0,25–4)","Поворот по часовой стрелке (90 / 180 / 270)","Степень сжатия (CRF, 18–40)"};
  string[] other={"Кадров в секунду (1–50)","","Длительность, секунд","Высота, px (0 — автоматически)","","",""};
  string[] defaults={"640","","0","640","2","90","30"};string[] defaultsB={"15","","5","0","","",""};
  first.Text=labels[operation];second.Text=other[operation];A.Text=defaults[operation];B.Text=defaultsB[operation];A.Parent.Visible=first.Visible=operation!=1;B.Parent.Visible=second.Visible=other[operation]!="";
  string[] hints={"Фрагмент, обрезка, пропорции и повторы →","GIF → MP4, один проход анимации, без звука.","Подходит для видео и аудио. Аудиофайлы сохраняются в M4A, видео — в MP4.","Пропорции сохраняются, если одно измерение равно 0. Фото → PNG, GIF → GIF, видео → MP4.","Видео → MP4. Звук ускоряется или замедляется вместе с видео.","Фото → PNG, GIF → GIF, видео → MP4. Звук видео сохраняется.","Выше число — меньше размер и ниже качество. Итоговый размер не гарантируется."};help.Text=hints[operation];details.Visible=operation==0;help.Width=operation==0?500:760;
  if(operation==0){A.Text=Gif.Size.ToString();B.Text=Gif.Fps.ToString();first.Text=Gif.Dimension==0?"Ширина GIF, px":"Высота GIF, px";}Reflow();
 }
 public ToolRequest Snapshot(int op){var r=ToolRequest.Create(op,A.Text,B.Text);if(op==0){r.Gif=Gif.Copy();r.Gif.Size=(int)r.A;r.Gif.Fps=(int)r.B;r.Gif.End=Math.Min(r.Gif.End,r.Gif.Start+r.Gif.MaxDuration);r.Gif.Validate();}return r;}
}
class ToolRequest {
 public int Operation;public double A,B;public GifOptions Gif;
 static double Number(string text,double min,double max,string name){double n;if(!double.TryParse(text.Trim().Replace(',','.'),NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out n)||double.IsNaN(n)||double.IsInfinity(n)||n<min||n>max)throw new Exception(name+": введите число от "+min+" до "+max+".");return n;}
 public static ToolRequest Create(int op,string a,string b){var r=new ToolRequest{Operation=op};
  switch(op){case 0:r.A=Number(a,16,1920,"Ширина");r.B=Number(b,1,50,"Частота кадров");break;
   case 2:r.A=Number(a,0,86400,"Начало");r.B=Number(b,0.01,86400,"Длительность");break;
   case 3:r.A=Number(a,0,7680,"Ширина");r.B=Number(b,0,7680,"Высота");if(r.A==0&&r.B==0)throw new Exception("Укажите ширину или высоту.");if((r.A>0&&r.A<2)||(r.B>0&&r.B<2))throw new Exception("Размер должен быть не меньше 2 px.");break;
   case 4:r.A=Number(a,0.25,4,"Скорость");break;
   case 5:r.A=Number(a,90,270,"Угол");if(r.A!=90&&r.A!=180&&r.A!=270)throw new Exception("Укажите 90, 180 или 270 градусов.");break;
   case 6:r.A=Number(a,18,40,"Степень сжатия");break;
  }if((op==0||op==3||op==5||op==6)&&(r.A!=Math.Floor(r.A)||r.B!=Math.Floor(r.B)))throw new Exception("Для размеров, кадров и степени сжатия используйте целые числа.");return r;
 }
 static string N(double n){return n.ToString("0.########",CultureInfo.InvariantCulture);}
 static bool Photo(string input){return Array.IndexOf(new[]{".png",".jpg",".jpeg",".bmp",".tif",".tiff",".webp"},Path.GetExtension(input).ToLowerInvariant())>=0;}
 static bool Audio(string input){return Array.IndexOf(new[]{".m4a",".mp3",".wav",".aac",".flac",".ogg",".opus",".wma"},Path.GetExtension(input).ToLowerInvariant())>=0;}
 public string Extension(string input){if(Operation==0)return "gif";if(Operation==2&&Audio(input))return "m4a";if(Operation==3||Operation==5){if(Photo(input))return "png";if(Path.GetExtension(input).Equals(".gif",StringComparison.OrdinalIgnoreCase))return "gif";}return "mp4";}
 public string Arguments(string input,string output){string ext=Extension(input);if((Operation==0||Operation==1||Operation>=4&&Operation!=5)&&Photo(input))throw new Exception("Для этого инструмента выберите видео или GIF.");if(Operation==0&&Gif!=null)return Gif.Arguments(input,output);if(Operation==1&&!Path.GetExtension(input).Equals(".gif",StringComparison.OrdinalIgnoreCase))throw new Exception("Инструмент GIF → MP4 принимает GIF-файлы.");
  string filter="",before="",after="";
  switch(Operation){case 0:filter="fps="+N(B)+",scale="+N(A)+":-1:flags=lanczos";break;
   case 2:before="-ss "+N(A)+" ";after="-t "+N(B)+" ";break;
   case 3:filter="scale="+(A==0?"-2":N(A))+":"+(B==0?"-2":N(B))+":flags=lanczos";break;
   case 4:filter="setpts=(PTS-STARTPTS)/"+N(A);double tempo=A;var af="";while(tempo>2){af+="atempo=2,";tempo/=2;}while(tempo<0.5){af+="atempo=0.5,";tempo/=0.5;}after="-af "+af+"atempo="+N(tempo)+" ";break;
   case 5:filter=A==90?"transpose=1":A==270?"transpose=2":"hflip,vflip";break;
  }
  string encode;
  if(ext=="gif"){if(filter=="")filter="null";encode="-filter_complex \"[0:v]"+filter+",split[a][b];[a]palettegen=stats_mode=single[p];[b][p]paletteuse=new=1\" -an -loop 0 ";}
  else if(ext=="png")encode="-vf \""+filter+"\" -frames:v 1 -update 1 -an ";
  else if(ext=="m4a")encode="-vn -c:a aac -b:a 192k ";
  else {if(filter!="")filter+=",";filter+="pad=ceil(iw/2)*2:ceil(ih/2)*2";encode="-map 0:v:0 -map 0:a:0? -vf \""+filter+"\" -c:v libx264 -pix_fmt yuv420p -preset medium -crf "+(Operation==6?N(A):"23")+" -c:a aac -b:a 160k -movflags +faststart ";}
  return "-hide_banner -nostdin -y -abort_on empty_output "+before+"-i \""+input+"\" "+after+encode+"\""+output+"\"";
 }
}

