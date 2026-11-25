using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
partial class Converter {
 async Task PreviewWorkspaceGif(bool estimate){if(appBusy)return;string input=SelectedSource();if(string.IsNullOrEmpty(input)){ShowNotice("Добавьте видео для предпросмотра.");return;}GifOptions options;try{CommitInlineGif();options=toolPanel.Snapshot(0).Gif;}catch(Exception ex){ShowNotice(ex.Message);return;}string temp=Path.Combine(Path.GetTempPath(),"Forma-studio-"+Guid.NewGuid().ToString("N")+".gif");double whole=options.End-options.Start;if(estimate){options.MaxBytes=0;options.End=options.Start+Math.Min(2,whole);}stopping=false;int batch=++batchVersion;var watch=Stopwatch.StartNew();run.Enabled=add.Enabled=folder.Enabled=type.Enabled=format.Enabled=toolPanel.Enabled=presetChoice.Enabled=false;foreach(var button in categoryButtons)button.Enabled=false;cancel.Enabled=true;SetWorkspaceBusy(true);
  try{double sourceDuration=await Task.Run(()=>MediaJob.Duration(input,root));if(sourceDuration>options.Start){double duration=Math.Min(whole,sourceDuration-options.Start);if(!estimate)options.End=options.Start+duration;whole=duration;}
   await Task.Run(()=>GifRender.Render(options,input,temp,root,()=>stopping,p=>active=p,f=>ReportFile(batch,0,1,f,estimate?"Оценка размера GIF":"Предпросмотр GIF",watch)));if(stopping)return;if(estimate){double bytes=new FileInfo(temp).Length*whole/(options.End-options.Start);status.Text="Оценка GIF: ~"+(bytes/1048576d).ToString("0.00")+" МБ\nПо короткому образцу · итог может отличаться";}else{status.Text="Готовый GIF: "+(new FileInfo(temp).Length/1048576d).ToString("0.00")+" МБ";using(var viewer=new GifResultWindow(temp))viewer.ShowDialog(this);}
  }catch(Exception ex){if(!stopping)ShowNotice(ex.Message,"Предпросмотр GIF");else status.Text="Предпросмотр остановлен";}finally{try{File.Delete(temp);}catch{}active=null;run.Enabled=add.Enabled=folder.Enabled=type.Enabled=format.Enabled=toolPanel.Enabled=presetChoice.Enabled=true;foreach(var button in categoryButtons)button.Enabled=true;cancel.Enabled=false;SetWorkspaceBusy(false);}
 }
}
