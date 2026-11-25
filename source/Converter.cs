using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;

partial class Converter : ChromeForm {
 ListBox files = new ListBox(); Choice type = new Choice(), format = new Choice();
 Button add = new GlowButton(), run = new GlowButton(), cancel = new GlowButton(), folder = new GlowButton();
 Label status = new Label(); TextBox dest = new TextBox(); Meter progress = new Meter(); Action refreshFiles; ToolPanel toolPanel=new ToolPanel();
 NavButton[] categoryButtons=new NavButton[5]; Process active; bool stopping; string root = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
 public Converter() {InitializeWorkspace();}
 void AddFiles(string[] paths){if(!run.Enabled)return;foreach(var p in paths)if(File.Exists(p)&&!files.Items.Contains(p))files.Items.Add(p);if(refreshFiles!=null)refreshFiles();}
 void Stop(){stopping=true;try{if(active!=null&&!active.HasExited)active.Kill();}catch{} }
 static string Q(string s){return "\""+s+"\"";}
 public static string Options(string f,int group){
  if(group==2)return "-frames:v 1 -update 1 ";
  switch(f){case "mp3":return "-vn -c:a libmp3lame -b:a 192k ";case "m4a":return "-vn -c:a aac -b:a 192k ";case "wav":return "-vn -c:a pcm_s16le ";case "flac":return "-vn -c:a flac ";case "ogg":return "-vn -c:a libvorbis -q:a 5 ";case "opus":return "-vn -c:a libopus -b:a 160k ";case "webm":return "-map 0:v:0 -map 0:a:0? -c:v libvpx-vp9 -crf 32 -b:v 0 -c:a libopus ";default:return "-map 0:v:0 -map 0:a:0? -c:v libx264 -pix_fmt yuv420p -vf pad=ceil(iw/2)*2:ceil(ih/2)*2 -crf 23 -c:a aac ";}
 }
 async Task ConvertFiles(){
  if(files.Items.Count==0){ShowNotice("Сначала добавьте файлы.");return;}
  string engine=Path.Combine(root,"ffmpeg.exe");if(type.SelectedIndex!=3&&!File.Exists(engine)){ShowNotice("Рядом с программой отсутствует ffmpeg.exe. Распакуйте архив целиком.");return;}
  string ext=format.Text.ToLowerInvariant();int group=type.SelectedIndex;string outputDir=dest.Text;ToolRequest tool=null;if(group==4){try{tool=toolPanel.Snapshot(format.SelectedIndex);}catch(Exception ex){ShowNotice(ex.Message);return;}}
  PrepareWorkspaceBatch();SavePreferences();var inputs=new string[files.Items.Count];files.Items.CopyTo(inputs,0);int batch=++batchVersion;
  run.Enabled=add.Enabled=folder.Enabled=type.Enabled=format.Enabled=toolPanel.Enabled=presetChoice.Enabled=false;foreach(var button in categoryButtons)button.Enabled=false;cancel.Enabled=true;stopping=false;progress.Value=0;progress.Maximum=1000;
  int done=0,failed=0;string errors="";
  try{
   Directory.CreateDirectory(outputDir);int index=0;
   foreach(string input in inputs){
    if(stopping)break;int fileIndex=index++;status.Text="Обработка: "+Path.GetFileName(input);var watch=Stopwatch.StartNew();
    if(tool!=null)ext=tool.Extension(input);
    string name=Path.GetFileNameWithoutExtension(input), output=Path.Combine(outputDir,name+"."+ext);int n=1;while(File.Exists(output))output=Path.Combine(outputDir,name+" ("+(n++)+")."+ext);
    string temp=Path.Combine(outputDir,".forma-"+Guid.NewGuid().ToString("N")+"."+ext);
    try{
     await Task.Run(()=>{
      Func<bool> cancelled=()=>stopping;Action<Process> track=p=>active=p;Action<double> report=f=>ReportFile(batch,fileIndex,inputs.Length,f,Path.GetFileName(input),watch);
      if(group==3){DocumentPipeline.Convert(input,temp,ext,root,cancelled,track,(f,message)=>ReportFile(batch,fileIndex,inputs.Length,f,message,watch));return;}
      if(tool!=null&&tool.Gif!=null){GifRender.Render(tool.Gif,input,temp,root,cancelled,track,report);return;}
      double duration=MediaJob.Duration(input,root);if(tool!=null){if(tool.Operation==2)duration=Math.Max(0,Math.Min(tool.B,duration-tool.A));if(tool.Operation==4)duration/=tool.A;}
      string args=tool!=null?tool.Arguments(input,temp):ConvertArguments(input,temp,ext,group);MediaJob.Run(root,args,duration,cancelled,track,report);
     });
     if(stopping)break;File.Move(temp,output);done++;RecordWorkspaceOutput(output);
    }catch(Exception ex){if(stopping)break;failed++;errors+=Path.GetFileName(input)+"\r\n"+ex.Message+"\r\n\r\n";}
    finally{if(File.Exists(temp))try{File.Delete(temp);}catch{} }
    progress.Value=(int)(1000d*(done+failed)/inputs.Length);
   }
   if(failed>0){string log=Path.Combine(outputDir,"Ошибки-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".txt");File.WriteAllText(log,errors);ShowNotice("Не удалось обработать файлов: "+failed+". Подробности: "+log);}
   if(done>0)OpenOutputDirectory(outputDir);
   status.Text=(stopping?"Остановлено. ":"Готово. ")+"Сохранено: "+done+". Ошибок: "+failed;
  }catch(Exception ex){ShowNotice(ex.Message);status.Text="Обработка не завершена";}
  finally{active=null;run.Enabled=add.Enabled=folder.Enabled=type.Enabled=format.Enabled=toolPanel.Enabled=presetChoice.Enabled=true;cancel.Enabled=false;foreach(var button in categoryButtons)button.Enabled=true;CompleteWorkspaceBatch();}
 }

 [STAThread] static void Main(){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new Converter());}
}

















