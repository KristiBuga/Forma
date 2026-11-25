using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
class MediaJob {
 public static double Duration(string input,string root){try{string value=DocumentPipeline.Run(Path.Combine(root,"ffprobe.exe"),"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "+DocumentPipeline.Q(input),()=>false,p=>{});double n;if(double.TryParse(value.Trim(),NumberStyles.Float,CultureInfo.InvariantCulture,out n)&&n>0&&!double.IsInfinity(n))return n;}catch{}return 0;}
 public static void Run(string root,string args,double duration,Func<bool> cancelled,Action<Process> track,Action<double> progress){using(var p=new Process()){if(cancelled())throw new OperationCanceledException();p.StartInfo=new ProcessStartInfo(Path.Combine(root,"ffmpeg.exe"),"-progress pipe:1 -nostats "+args){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};track(p);p.Start();if(cancelled())try{p.Kill();}catch{}var stderr=p.StandardError.ReadToEndAsync();string line;while((line=p.StandardOutput.ReadLine())!=null){if(cancelled())try{p.Kill();}catch{}if(line.StartsWith("out_time_us=")&&duration>0){double micro;if(double.TryParse(line.Substring(12),NumberStyles.Float,CultureInfo.InvariantCulture,out micro))progress(Math.Max(0,Math.Min(0.995,micro/1000000/duration)));}}p.WaitForExit();if(cancelled())throw new OperationCanceledException();if(p.ExitCode!=0)throw new Exception(stderr.Result);progress(1);}}
}
class GifRender {
 public static GifOptions Render(GifOptions original,string input,string output,string root,Func<bool> cancelled,Action<Process> track,Action<double> progress){var options=original.Copy();options.Cancelled=cancelled;options.Track=track;for(int attempt=0;attempt<12;attempt++){MediaJob.Run(root,options.Arguments(input,output),options.End-options.Start,cancelled,track,progress);if(options.MaxBytes<=0||new FileInfo(output).Length<=options.MaxBytes)return options;if(options.Size<=16&&options.Fps<=1)break;options.Size=Math.Max(16,(int)(options.Size*0.78));options.Fps=Math.Max(1,(int)(options.Fps*0.85));}throw new Exception("Не удалось уложить GIF в выбранный размер. Сократите фрагмент.");}
}

