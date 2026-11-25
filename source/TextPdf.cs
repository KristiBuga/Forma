using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
class TextPdf {
 public static void Write(string text,string output){
  string fontPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts),"arial.ttf");if(!File.Exists(fontPath))throw new Exception("Для PDF требуется системный шрифт Arial.");
  var font=new GlyphTypeface(new Uri(fontPath));var map=new byte[131072];var widths=new StringBuilder();var used=new HashSet<char>(text);used.Add(' ');
  foreach(char c in used){ushort gid;if(!font.CharacterToGlyphMap.TryGetValue(c,out gid))gid=0;map[c*2]=(byte)(gid>>8);map[c*2+1]=(byte)gid;double width; if(!font.AdvanceWidths.TryGetValue(gid,out width))width=0.6;widths.Append(((int)c).ToString()).Append(" [").Append((width*1000).ToString("0",CultureInfo.InvariantCulture)).Append("] ");}
  var objects=new List<byte[]>();Func<string,byte[]> ascii=s=>Encoding.ASCII.GetBytes(s);Action<string> add=s=>objects.Add(ascii(s));
  var lines=new List<string>();foreach(string paragraph in text.Replace("\r","").Replace("\t","    ").Split('\n')){var line=new StringBuilder();double measured=0;foreach(char c in paragraph){ushort gid;double advance=0.6;if(font.CharacterToGlyphMap.TryGetValue(c,out gid))font.AdvanceWidths.TryGetValue(gid,out advance);if(measured+advance*11>505&&line.Length>0){lines.Add(line.ToString());line.Clear();measured=0;}line.Append(c);measured+=advance*11;}lines.Add(line.ToString());}int count=Math.Max(1,(lines.Count+47)/48);var kids=new StringBuilder();for(int i=0;i<count;i++)kids.Append(10+i*2).Append(" 0 R ");
  add("<< /Type /Catalog /Pages 2 0 R >>");add("<< /Type /Pages /Count "+count+" /Kids ["+kids+"] >>");
  add("<< /Type /Font /Subtype /Type0 /BaseFont /Arial /Encoding /Identity-H /DescendantFonts [4 0 R] /ToUnicode 8 0 R >>");
  add("<< /Type /Font /Subtype /CIDFontType2 /BaseFont /Arial /CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> /FontDescriptor 5 0 R /CIDToGIDMap 7 0 R /DW 600 /W ["+widths+"] >>");
  add("<< /Type /FontDescriptor /FontName /Arial /Flags 32 /FontBBox [-700 -400 2200 1400] /ItalicAngle 0 /Ascent 905 /Descent -212 /CapHeight 716 /StemV 80 /FontFile2 6 0 R >>");
  var bytes=File.ReadAllBytes(fontPath);objects.Add(Stream(bytes,"/Length1 "+bytes.Length));objects.Add(Stream(map,""));
  objects.Add(Stream(ascii("/CIDInit /ProcSet findresource begin 12 dict begin begincmap /CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def /CMapName /ArialUnicode def /CMapType 2 def 1 begincodespacerange <0000> <FFFF> endcodespacerange 1 beginbfrange <0000> <FFFF> <0000> endbfrange endcmap CMapName currentdict /CMap defineresource pop end end"),""));
  add("<< /Producer (Forma) >>");
  for(int page=0;page<count;page++){
   add("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R >> >> /Contents "+(11+page*2)+" 0 R >>");var content=new StringBuilder("BT /F1 11 Tf 14 TL 45 794 Td ");
   for(int n=page*48;n<Math.Min(lines.Count,(page+1)*48);n++){var encoded=Encoding.BigEndianUnicode.GetBytes(lines[n]);content.Append('<').Append(BitConverter.ToString(encoded).Replace("-","")).Append("> Tj T* ");}content.Append("ET");objects.Add(Stream(ascii(content.ToString()),""));
  }
  using(var file=File.Create(output)){Action<string> put=s=>{byte[] b=ascii(s);file.Write(b,0,b.Length);};put("%PDF-1.4\n");var offsets=new List<long>{0};for(int i=0;i<objects.Count;i++){offsets.Add(file.Position);put((i+1)+" 0 obj\n");file.Write(objects[i],0,objects[i].Length);put("\nendobj\n");}long xref=file.Position;put("xref\n0 "+(objects.Count+1)+"\n0000000000 65535 f \n");for(int i=1;i<offsets.Count;i++)put(offsets[i].ToString("0000000000")+" 00000 n \n");put("trailer << /Size "+(objects.Count+1)+" /Root 1 0 R /Info 9 0 R >>\nstartxref\n"+xref+"\n%%EOF");}
 }
 static byte[] Stream(byte[] content,string attrs){using(var ms=new MemoryStream()){byte[] head=Encoding.ASCII.GetBytes("<< /Length "+content.Length+" "+attrs+" >>\nstream\n");ms.Write(head,0,head.Length);ms.Write(content,0,content.Length);var tail=Encoding.ASCII.GetBytes("\nendstream");ms.Write(tail,0,tail.Length);return ms.ToArray();}}
}
