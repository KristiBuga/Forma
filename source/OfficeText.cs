using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
class OfficeText {
 const string R="http://schemas.openxmlformats.org/officeDocument/2006/relationships/";
 public static XmlDocument Load(ZipArchive z,string path){var entry=z.GetEntry(path);if(entry==null)throw new Exception("В документе отсутствует "+path);var d=new XmlDocument{XmlResolver=null};using(var s=entry.Open())using(var r=XmlReader.Create(s,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null}))d.Load(r);return d;}
 static string Resolve(string parent,string target){return new Uri(new Uri("http://package/"+parent),target).AbsolutePath.TrimStart('/');}
 static Dictionary<string,string> Links(ZipArchive z,string part){string folder=part.Substring(0,part.LastIndexOf('/')+1);string file=part.Substring(folder.Length);var result=new Dictionary<string,string>();var doc=Load(z,folder+"_rels/"+file+".rels");foreach(XmlElement e in doc.DocumentElement.ChildNodes)if(e.GetAttribute("TargetMode")!="External")result[e.GetAttribute("Id")]=Resolve(part,e.GetAttribute("Target"));return result;}
 public static string Read(string input){using(var zip=ZipFile.OpenRead(input)){
  var sb=new StringBuilder();bool slides=Path.GetExtension(input).Equals(".pptx",StringComparison.OrdinalIgnoreCase);string main=slides?"ppt/presentation.xml":"xl/workbook.xml";var doc=Load(zip,main);var links=Links(zip,main);
  var shared=new List<string>();if(!slides&&zip.GetEntry("xl/sharedStrings.xml")!=null){var strings=Load(zip,"xl/sharedStrings.xml");foreach(XmlNode si in strings.SelectNodes("//*[local-name()='si']"))shared.Add(string.Concat(si.SelectNodes(".//*[local-name()='t']").Cast<XmlNode>().Select(n=>n.InnerText)));}
  foreach(XmlElement item in doc.SelectNodes(slides?"//*[local-name()='sldId']":"//*[local-name()='sheet']")){
   string id=item.GetAttribute("id",R.TrimEnd('/'));string path;if(!links.TryGetValue(id,out path))throw new Exception("Не найдена часть документа: "+id);var part=Load(zip,path);
   if(slides){foreach(XmlNode p in part.SelectNodes("//*[local-name()='txBody']/*[local-name()='p']")){foreach(XmlNode n in p.SelectNodes(".//*[local-name()='t' or local-name()='br']")){if(n.LocalName=="br")sb.AppendLine();else sb.Append(n.InnerText);}sb.AppendLine();}}
   else{sb.AppendLine(item.GetAttribute("name"));foreach(XmlNode row in part.SelectNodes("//*[local-name()='sheetData']/*[local-name()='row']")){int previous=0;foreach(XmlElement c in row.SelectNodes("*[local-name()='c']")){string cell=c.GetAttribute("r");int col=0;foreach(char ch in cell){if(ch<'A'||ch>'Z')break;col=col*26+ch-'A'+1;}if(col==0)col=previous+1;while(previous+1<col){sb.Append('\t');previous++;}if(previous>0)sb.Append('\t');string kind=c.GetAttribute("t");var v=c.SelectSingleNode("*[local-name()='v']");string value=v==null?"":v.InnerText;if(kind=="s"){int ix;if(int.TryParse(value,out ix)&&ix>=0&&ix<shared.Count)value=shared[ix];}else if(kind=="inlineStr")value=string.Concat(c.SelectNodes(".//*[local-name()='t']").Cast<XmlNode>().Select(n=>n.InnerText));else if(kind=="b")value=value=="1"?"TRUE":"FALSE";sb.Append(value);previous=col;}sb.AppendLine();}}
   sb.AppendLine();
  }return sb.ToString();
 }}
 static string E(string value){return System.Security.SecurityElement.Escape(value)??"";}
 static void Put(ZipArchive z,string path,string xml){using(var w=new StreamWriter(z.CreateEntry(path).Open(),new UTF8Encoding(false)))w.Write(xml);}
 static string Rel(string target,string type,string id){return "<Relationship Id=\""+id+"\" Type=\""+R+type+"\" Target=\""+target+"\"/>";}
 static string Rels(string body){return "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"+body+"</Relationships>";}
 public static void Write(string text,string output,string ext){
  if(ext=="txt"){File.WriteAllText(output,text,new UTF8Encoding(true));return;}if(ext=="pdf"){TextPdf.Write(text,output);return;}
  string[] lines=text.Replace("\r","").Split('\n');using(var z=ZipFile.Open(output,ZipArchiveMode.Create)){
   string types="<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/>";
   if(ext=="docx"){
    Put(z,"_rels/.rels",Rels(Rel("word/document.xml","officeDocument","rId1")));
    var body=new StringBuilder();foreach(string line in lines){body.Append("<w:p><w:r><w:t xml:space=\"preserve\">").Append(E(line)).Append("</w:t></w:r></w:p>");}
    Put(z,"word/document.xml","<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>"+body+"<w:sectPr><w:pgSz w:w=\"11906\" w:h=\"16838\"/><w:pgMar w:top=\"1134\" w:right=\"1134\" w:bottom=\"1134\" w:left=\"1134\"/></w:sectPr></w:body></w:document>");types+="<Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/>";
   }else if(ext=="xlsx"){
    Put(z,"_rels/.rels",Rels(Rel("xl/workbook.xml","officeDocument","rId1")));Put(z,"xl/workbook.xml","<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\""+R.TrimEnd('/')+"\"><sheets><sheet name=\"Текст\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");Put(z,"xl/_rels/workbook.xml.rels",Rels(Rel("worksheets/sheet1.xml","worksheet","rId1")));
    var body=new StringBuilder();int row=0;foreach(string line in lines){for(int pos=0;pos<Math.Max(1,line.Length);pos+=32767){if(++row>1048576)throw new Exception("Текст превышает вместимость листа Excel.");string value=line.Substring(pos,Math.Min(32767,line.Length-pos));body.Append("<row r=\"").Append(row).Append("\"><c r=\"A").Append(row).Append("\" t=\"inlineStr\"><is><t xml:space=\"preserve\">").Append(E(value)).Append("</t></is></c></row>");}}
    Put(z,"xl/worksheets/sheet1.xml","<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><cols><col min=\"1\" max=\"1\" width=\"100\" customWidth=\"1\"/></cols><sheetData>"+body+"</sheetData></worksheet>");types+="<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>";
   }else if(ext=="pptx"){
    string ns="xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" xmlns:p=\"http://schemas.openxmlformats.org/presentationml/2006/main\" xmlns:r=\""+R.TrimEnd('/')+"\"";
    Put(z,"_rels/.rels",Rels(Rel("ppt/presentation.xml","officeDocument","rId1")));
    var pages=Wrap(text,70);int total=Math.Max(1,(pages.Count+13)/14);var ids=new StringBuilder();var rels=new StringBuilder();
    string tree="<p:spTree><p:nvGrpSpPr><p:cNvPr id=\"1\" name=\"\"/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr/></p:spTree>";
    Put(z,"ppt/slideLayouts/slideLayout1.xml","<p:sldLayout "+ns+" type=\"blank\" preserve=\"1\"><p:cSld name=\"Blank\">"+tree+"</p:cSld><p:clrMapOvr><a:masterClrMapping/></p:clrMapOvr></p:sldLayout>");
    Put(z,"ppt/slideLayouts/_rels/slideLayout1.xml.rels",Rels(Rel("../slideMasters/slideMaster1.xml","slideMaster","rId1")));
    Put(z,"ppt/slideMasters/slideMaster1.xml","<p:sldMaster "+ns+"><p:cSld>"+tree+"</p:cSld><p:clrMap accent1=\"accent1\" accent2=\"accent2\" accent3=\"accent3\" accent4=\"accent4\" accent5=\"accent5\" accent6=\"accent6\" bg1=\"lt1\" bg2=\"lt2\" folHlink=\"folHlink\" hlink=\"hlink\" tx1=\"dk1\" tx2=\"dk2\"/><p:sldLayoutIdLst><p:sldLayoutId id=\"2147483649\" r:id=\"rId1\"/></p:sldLayoutIdLst><p:txStyles><p:titleStyle/><p:bodyStyle/><p:otherStyle/></p:txStyles></p:sldMaster>");
    Put(z,"ppt/slideMasters/_rels/slideMaster1.xml.rels",Rels(Rel("../slideLayouts/slideLayout1.xml","slideLayout","rId1")));
    rels.Append(Rel("slideMasters/slideMaster1.xml","slideMaster","rIdMaster"));
    types+="<Override PartName=\"/ppt/slideMasters/slideMaster1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.presentationml.slideMaster+xml\"/><Override PartName=\"/ppt/slideLayouts/slideLayout1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.presentationml.slideLayout+xml\"/>";
    for(int i=0;i<total;i++){
     ids.Append("<p:sldId id=\"").Append(256+i).Append("\" r:id=\"rId").Append(i+1).Append("\"/>");rels.Append(Rel("slides/slide"+(i+1)+".xml","slide","rId"+(i+1)));
     var paragraphs=new StringBuilder();foreach(string line in pages.Skip(i*14).Take(14))paragraphs.Append("<a:p><a:pPr/><a:r><a:rPr lang=\"ru-RU\" sz=\"2000\"/><a:t xml:space=\"preserve\">").Append(E(line)).Append("</a:t></a:r><a:endParaRPr sz=\"2000\"/></a:p>");
     Put(z,"ppt/slides/slide"+(i+1)+".xml","<p:sld "+ns+"><p:cSld><p:spTree><p:nvGrpSpPr><p:cNvPr id=\"1\" name=\"\"/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr/><p:sp><p:nvSpPr><p:cNvPr id=\"2\" name=\"Текст\"/><p:cNvSpPr txBox=\"1\"/><p:nvPr/></p:nvSpPr><p:spPr><a:xfrm><a:off x=\"457200\" y=\"457200\"/><a:ext cx=\"11277600\" cy=\"5943600\"/></a:xfrm><a:prstGeom prst=\"rect\"><a:avLst/></a:prstGeom><a:noFill/></p:spPr><p:txBody><a:bodyPr/><a:lstStyle/>"+paragraphs+"</p:txBody></p:sp></p:spTree></p:cSld><p:clrMapOvr><a:masterClrMapping/></p:clrMapOvr></p:sld>");
     types+="<Override PartName=\"/ppt/slides/slide"+(i+1)+".xml\" ContentType=\"application/vnd.openxmlformats-officedocument.presentationml.slide+xml\"/>";
     Put(z,"ppt/slides/_rels/slide"+(i+1)+".xml.rels",Rels(Rel("../slideLayouts/slideLayout1.xml","slideLayout","rId1")));
    }
    Put(z,"ppt/presentation.xml","<p:presentation "+ns+"><p:sldMasterIdLst><p:sldMasterId id=\"2147483648\" r:id=\"rIdMaster\"/></p:sldMasterIdLst><p:sldIdLst>"+ids+"</p:sldIdLst><p:sldSz cx=\"12192000\" cy=\"6858000\"/><p:notesSz cx=\"6858000\" cy=\"9144000\"/></p:presentation>");Put(z,"ppt/_rels/presentation.xml.rels",Rels(rels.ToString()));types+="<Override PartName=\"/ppt/presentation.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml\"/>";
   }else throw new Exception("Неизвестный выходной формат документа.");
   Put(z,"[Content_Types].xml",types+"</Types>");
  }
 }
 public static List<string> Wrap(string text,int width){var result=new List<string>();foreach(var line in text.Replace("\r","").Replace("\t","    ").Split('\n')){if(line.Length==0){result.Add("");continue;}for(int pos=0;pos<line.Length;){int len=Math.Min(width,line.Length-pos);if(pos+len<line.Length){int space=line.LastIndexOf(' ',pos+len-1,len);if(space>pos)len=space-pos+1;}result.Add(line.Substring(pos,len));pos+=len;}}return result;}
}
