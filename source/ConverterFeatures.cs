using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
partial class Converter {
 Preferences preferences;int currentCategory,activePreset=-1,batchVersion;bool restoring,applyingPreset;Choice presetChoice;
 void InitializeFeatures(Panel sidebar){
  presetChoice=new Choice{Bounds=new Rectangle(18,404,174,40),Font=new Font("Segoe UI",8.5f,FontStyle.Bold)};presetChoice.Items.AddRange(new object[]{"GIF до 10 МБ","Видео для Telegram","MP3 · 320 кбит/с"});presetChoice.Text="Быстрые настройки";sidebar.Controls.Add(presetChoice);
  presetChoice.SelectedIndexChanged+=(s,e)=>{if(presetChoice.SelectedIndex>=0&&!restoring&&run.Enabled)ApplyPreset(presetChoice.SelectedIndex);};
  preferences=Preferences.Load(Preferences.DefaultPath);restoring=true;toolPanel.Gif=preferences.Gif.Copy();currentCategory=preferences.Category;type.SelectedIndex=currentCategory;format.SelectedIndex=Math.Max(0,Math.Min(format.Items.Count-1,preferences.Formats[currentCategory]));if(!string.IsNullOrWhiteSpace(preferences.Folder))dest.Text=preferences.Folder;if(preferences.Preset>=0&&preferences.Preset<3){activePreset=preferences.Preset;presetChoice.SelectedIndex=activePreset;}restoring=false;
  type.SelectedIndexChanged+=(s,e)=>{if(restoring)return;currentCategory=type.SelectedIndex;restoring=true;format.SelectedIndex=Math.Max(0,Math.Min(format.Items.Count-1,preferences.Formats[currentCategory]));restoring=false;if(!applyingPreset)ClearPreset();};
  format.SelectedIndexChanged+=(s,e)=>{if(!restoring&&currentCategory==type.SelectedIndex){preferences.Formats[currentCategory]=format.SelectedIndex;if(!applyingPreset)ClearPreset();}};
  FormClosing+=(s,e)=>{if(!e.Cancel&&run.Enabled)SavePreferences();};
 }
 void ClearPreset(){activePreset=-1;presetChoice.SelectedIndex=-1;presetChoice.Text="Быстрые настройки";}
 protected virtual void OpenOutputDirectory(string directory){try{Process.Start("explorer.exe",Q(directory));}catch{ShowNotice("Файлы сохранены в: "+directory);}}
 void ApplyPreset(int index){applyingPreset=true;try{if(index==0){type.SelectedIndex=4;format.SelectedIndex=0;toolPanel.Gif.MaxBytes=10*1024*1024;toolPanel.Gif.Size=640;toolPanel.Gif.Fps=15;toolPanel.Gif.End=toolPanel.Gif.Start+Math.Min(10,toolPanel.Gif.MaxDuration);toolPanel.Configure(0);if(gifInspector!=null)gifInspector.LoadOptions(toolPanel.Snapshot(0).Gif);}else if(index==1){type.SelectedIndex=1;format.SelectedIndex=0;}else{type.SelectedIndex=0;format.SelectedIndex=0;}activePreset=index;if(index==2)qualityChoice.SelectedIndex=4;status.Text="Пресет: "+presetChoice.Items[index];}finally{applyingPreset=false;}}
 void SavePreferences(){try{preferences.Category=type.SelectedIndex;preferences.LayoutMode=layoutMode;preferences.Mp3Kbps=GetMp3Bitrate();preferences.Preset=activePreset;preferences.Folder=dest.Text;preferences.Formats[type.SelectedIndex]=format.SelectedIndex;try{preferences.Gif=type.SelectedIndex==4&&format.SelectedIndex==0?toolPanel.Snapshot(0).Gif:toolPanel.Gif.Copy();}catch{preferences.Gif=toolPanel.Gif.Copy();}preferences.Save(Preferences.DefaultPath);}catch{}}
 string ConvertArguments(string input,string temp,string ext,int group){string options=Options(ext,group);if(group==0&&ext=="mp3")options=options.Replace("192k",preferences.Mp3Kbps+"k");if(activePreset==2&&group==0&&ext=="mp3")options=options.Replace("192k","320k");if(activePreset==1&&group==1)options="-map 0:v:0 -map 0:a:0? -vf \"scale='min(1280,iw)':'min(1280,ih)':force_original_aspect_ratio=decrease:force_divisible_by=2,pad=ceil(iw/2)*2:ceil(ih/2)*2\" -c:v libx264 -pix_fmt yuv420p -crf 23 -c:a aac -b:a 160k -movflags +faststart ";return "-hide_banner -nostdin -y -i "+Q(input)+" "+options+Q(temp);}
 void ReportFile(int batch,int index,int count,double fraction,string name,Stopwatch clock){if(!IsHandleCreated||IsDisposed)return;BeginInvoke(new Action(()=>{if(batch!=batchVersion||run.Enabled||IsDisposed)return;fraction=Math.Max(0,Math.Min(1,fraction));progress.Value=(int)(1000*(index+fraction)/count);string eta=fraction>0.03&&fraction<1?" · ещё ~"+TimeSpan.FromSeconds(Math.Min(359999,clock.Elapsed.TotalSeconds*(1-fraction)/fraction)).ToString(@"hh\:mm\:ss"):"";status.Text=name+"\n"+(int)(fraction*100)+"%"+eta+" · "+(index+1)+"/"+count;}));}
}



