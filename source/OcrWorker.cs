using System;
using System.IO;
using System.Text;
using Tesseract;
class OcrWorker {
 static int Main(string[] args){try{if(args.Length!=2)return 2;string root=AppDomain.CurrentDomain.BaseDirectory;using(var engine=new TesseractEngine(Path.Combine(root,"tessdata"),"rus+eng",EngineMode.LstmOnly))using(var image=Pix.LoadFromFile(args[0]))using(var page=engine.Process(image,PageSegMode.Auto)){File.WriteAllText(args[1],page.GetText(),new UTF8Encoding(true));}return 0;}catch(Exception ex){Console.Error.WriteLine("Не удалось распознать текст: "+ex.Message);return 1;}}
}
