using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
class DocumentPipeline {
 public static string Q(string s){return "\""+s+"\"";}
 public static string Run(string exe,string args,Func<bool> cancelled,Action<Process> track){using(var p=new Process()){if(cancelled())throw new OperationCanceledException();p.StartInfo=new ProcessStartInfo(exe,args){UseShellExecute=false,CreateNoWindow=true,RedirectStandardError=true,RedirectStandardOutput=true};track(p);p.Start();if(cancelled())try{p.Kill();}catch{}var stderr=p.StandardError.ReadToEndAsync();string output=p.StandardOutput.ReadToEnd();p.WaitForExit();if(cancelled())throw new OperationCanceledException();if(p.ExitCode!=0)throw new Exception(stderr.Result);return output;}}
 public static void Convert(string input,string output,string ext,string root,Func<bool> cancelled,Action<Process> track,Action<double,string> progress){
  string suffix=Path.GetExtension(input).ToLowerInvariant();string scratch=Path.Combine(Path.GetTempPath(),"Forma-OCR-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(scratch);
  try{
   string text;
   if(suffix==".pdf"){
    string info=Run(Path.Combine(root,"pdf","pdfinfo.exe"),Q(input),cancelled,track);var match=Regex.Match(info,@"(?m)^Pages:\s+(\d+)");if(!match.Success)throw new Exception("Не удалось определить число страниц PDF.");int pages=int.Parse(match.Groups[1].Value);var result=new StringBuilder();
    for(int page=1;page<=pages;page++){
     progress((page-1d)/pages,"Страница "+page+" из "+pages);string plain=Path.Combine(scratch,"page.txt");
     Run(Path.Combine(root,"pdf","pdftotext.exe"),"-f "+page+" -l "+page+" -layout -enc UTF-8 "+Q(input)+" "+Q(plain),cancelled,track);string content=File.ReadAllText(plain);
     if(string.IsNullOrWhiteSpace(content)){string image=Path.Combine(scratch,"page");progress((page-1d)/pages,"Распознавание страницы "+page+" из "+pages);Run(Path.Combine(root,"pdf","pdftoppm.exe"),"-f "+page+" -l "+page+" -singlefile -r 180 -scale-to 3500 -png "+Q(input)+" "+Q(image),cancelled,track);Run(Path.Combine(root,"OcrWorker.exe"),Q(image+".png")+" "+Q(plain),cancelled,track);content=File.ReadAllText(plain);}
     result.AppendLine(content.TrimEnd());result.AppendLine();progress((double)page/pages,"Страница "+page+" из "+pages);
    }text=result.ToString();
   }else if(Array.IndexOf(new[]{".png",".jpg",".jpeg",".bmp",".tif",".tiff"},suffix)>=0){string plain=Path.Combine(scratch,"image.txt");progress(0,"Распознавание фото · русский + английский");Run(Path.Combine(root,"OcrWorker.exe"),Q(input)+" "+Q(plain),cancelled,track);text=File.ReadAllText(plain);}
   else{string plain=Path.Combine(scratch,"document.txt");progress(0,"Извлечение текста");Documents.Extract(input,plain);text=File.ReadAllText(plain);}
   if(cancelled())throw new OperationCanceledException();if(string.IsNullOrWhiteSpace(text))throw new Exception("Текст не найден. Проверьте, что на изображении есть разборчивый печатный текст.");OfficeText.Write(text,output,ext);progress(1,"Текст сохранён");
  }finally{try{Directory.Delete(scratch,true);}catch{}}
 }
}
