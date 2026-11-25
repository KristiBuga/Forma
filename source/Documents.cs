using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Windows.Forms;
class Documents {
 public static void Extract(string input,string output){
  string ext=Path.GetExtension(input).ToLowerInvariant(),text;
  if(ext==".pptx"||ext==".xlsx"){text=OfficeText.Read(input);}
  else if(ext==".docx" || ext==".odt"){
   using(var zip=ZipFile.OpenRead(input)){
    var part=zip.GetEntry(ext==".docx"?"word/document.xml":"content.xml");if(part==null)throw new Exception("В документе не найден текст.");
    var builder=new StringBuilder();var settings=new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null};
    using(var stream=part.Open())using(var reader=XmlReader.Create(stream,settings)){
     while(reader.Read()){
      if(reader.NodeType==XmlNodeType.Element && reader.LocalName=="tab")builder.Append('\t');
      if(reader.NodeType==XmlNodeType.Element && (reader.LocalName=="br"||reader.LocalName=="line-break"))builder.AppendLine();
      if(reader.NodeType==XmlNodeType.Text||reader.NodeType==XmlNodeType.SignificantWhitespace)builder.Append(reader.Value);
      if(reader.NodeType==XmlNodeType.EndElement && (reader.LocalName=="p"||reader.LocalName=="h"))builder.AppendLine();
     }
    }text=builder.ToString();
   }
  }else if(ext==".rtf"){string value=null;Exception error=null;var thread=new System.Threading.Thread(()=>{try{using(var box=new RichTextBox()){box.LoadFile(input);value=box.Text;}}catch(Exception ex){error=ex;}});thread.SetApartmentState(System.Threading.ApartmentState.STA);thread.Start();thread.Join();if(error!=null)throw error;text=value;}
  else if(ext==".txt"||ext==".csv"||ext==".md"||ext==".log"){using(var reader=new StreamReader(input,Encoding.UTF8,true))text=reader.ReadToEnd();}
  else throw new Exception("Поддерживаются PDF, DOCX, PPTX, XLSX, ODT, RTF, TXT, CSV, MD. Старый DOC сохраните как DOCX.");
  if(string.IsNullOrWhiteSpace(text))throw new Exception("Текст не найден. Для изображений и сканов нужно OCR-распознавание.");
  File.WriteAllText(output,text,new UTF8Encoding(true));
 }
}

