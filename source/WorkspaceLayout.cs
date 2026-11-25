using System;
using System.Drawing;
using System.Windows.Forms;
partial class Converter {
 void ChangeLayout(int mode,bool initialLayout){
  if(mode<0||mode>2||appBusy)return;
  if(layoutMode==2&&wasInlineGif){
   try{CommitInlineGif();}catch(Exception ex){ShowNotice(ex.Message);return;}
  }
  layoutMode=mode;
  layoutSwitcher.SelectMode(mode);
  if(mode==2&&type.SelectedIndex==4&&format.SelectedIndex==0)gifInspector.LoadOptions(toolPanel.Snapshot(0).Gif);
  wizardStep=0;
  mediaPreview.Stop();
  if(initialLayout){
   var area=Screen.FromPoint(Cursor.Position).WorkingArea;
   var openingBounds=GetOpeningBounds(mode,area);
   MinimumSize=new Size(Math.Min(640,openingBounds.Width),Math.Min(640,openingBounds.Height));
   Bounds=openingBounds;
  }else{
   KeepWindowOnScreen();
  }
  body.MoveTo(0);
  LayoutWorkspace();
  if(mode==2){var ignored=UpdateSelection();}
 }
 static Rectangle GetOpeningBounds(int mode,Rectangle area){
  const int margin=12;
  var desired=mode==0?new Size(780,840):mode==1?new Size(960,900):new Size(1380,880);
  int width=Math.Min(desired.Width,Math.Max(1,area.Width-2*margin));
  int height=Math.Min(desired.Height,Math.Max(1,area.Height-2*margin));
  return new Rectangle(area.Left+(area.Width-width)/2,area.Top+(area.Height-height)/2,width,height);
 }
 protected override void OnLoad(EventArgs e){
  base.OnLoad(e);
  KeepWindowOnScreen();
 }
 void KeepWindowOnScreen(){
  if(WindowState!=FormWindowState.Normal)return;
  var area=Screen.FromRectangle(Bounds).WorkingArea;
  if(area.Contains(Bounds))return;
  int width=Math.Min(Width,area.Width),height=Math.Min(Height,area.Height);
  MinimumSize=new Size(Math.Min(MinimumSize.Width,width),Math.Min(MinimumSize.Height,height));
  Bounds=new Rectangle(Math.Max(area.Left,Math.Min(Left,area.Right-width)),
   Math.Max(area.Top,Math.Min(Top,area.Bottom-height)),width,height);
 }
 void LayoutWorkspace(){if(buildingUI||layoutInProgress||body==null||canvas==null)return;layoutInProgress=true;try{
  int w=ClientSize.Width;layoutSwitcher.Place(w-158,w<680);workspaceHeader.Controls["WindowMinimize"].SetBounds(w-142,15,40,36);workspaceHeader.Controls["WindowMaximize"].SetBounds(w-96,15,40,36);workspaceHeader.Controls["WindowClose"].SetBounds(w-50,15,40,36);
  progress.SetBounds(24,8,Math.Max(1,w-48),4);status.SetBounds(24,20,Math.Max(220,w-300),37);presetHost.SetBounds(24,62,220,36);if(presetChoice!=null)presetChoice.SetBounds(0,0,220,36);run.SetBounds(w-248,60,224,40);cancel.SetBounds(w-390,60,130,40);wizardBack.SetBounds(w-390,60,130,40);cancel.Visible=appBusy;wizardBack.Visible=layoutMode==1&&wizardStep>0&&!appBusy;run.Text=appBusy?"Обработка…":layoutMode==1?wizardStep==0?"Далее →":wizardStep==2?"Новая обработка":type.SelectedIndex==4&&format.SelectedIndex==0?"Создать GIF":"Конвертировать":layoutMode==2?type.SelectedIndex==4&&format.SelectedIndex==0?"Создать GIF":"Экспорт":type.SelectedIndex==4?"Применить →":"Конвертировать";
  for(int i=0;i<5;i++){categoryButtons[i].Text=layoutMode==2?"":shortCategories[i];categoryButtons[i].Primary=i==type.SelectedIndex;categoryButtons[i].Invalidate();}
  screenTitle.Text=type.SelectedIndex==4?format.Text:type.SelectedIndex==3?"Текст из документов":"Конвертер файлов";screenSubtitle.Text=type.SelectedIndex==3?"Извлечение текста и OCR · русский + английский":"Локально. Без загрузки файлов в облако.";noteLabel.Text=type.SelectedIndex==3?"Сканы и фото распознаются автоматически. Проверьте важные слова и числа.":"Исходные файлы сохраняются. Папка откроется после обработки.";
  wizardOperations.Visible=layoutMode==1&&wizardStep==1;for(int i=0;i<3;i++){operationButtons[i].Chosen=i==(type.SelectedIndex==4&&format.SelectedIndex==0?1:type.SelectedIndex==0?2:0);operationButtons[i].Invalidate();}steps.Visible=layoutMode==1;steps.Step=wizardStep;steps.Invalidate();mediaCard.Visible=layoutMode==2;summaryCard.Visible=layoutMode==1&&wizardStep==1;resultCard.Visible=layoutMode==1&&wizardStep==2;navigation.Visible=layoutMode!=1||wizardStep==1;queueCard.Visible=layoutMode!=1||wizardStep==0;settingsCard.Visible=layoutMode!=1||wizardStep==1;screenTitle.Visible=screenSubtitle.Visible=layoutMode!=1||wizardStep!=2;
  int cw=Math.Max(600,body.ClientSize.Width-12),contentHeight=0;
  if(layoutMode==0){screenTitle.Visible=screenSubtitle.Visible=false;
   navigation.SetBounds(20,14,cw-40,44);LayoutNavigation(false);screenTitle.SetBounds(24,78,cw-48,34);screenSubtitle.SetBounds(24,117,cw-48,24);queueCard.SetBounds(24,78,cw-48,310);LayoutQueue();int height=LayoutInspector(Math.Max(1,cw-48),false);settingsCard.SetBounds(24,404,cw-48,height);LayoutInspector(settingsCard.Width,false);contentHeight=404+height+16;
  }else if(layoutMode==1){
   steps.SetBounds(30,8,cw-60,76);
   if(wizardStep==0){screenTitle.Text="Добавьте файлы";screenSubtitle.Text="Можно выбрать сразу несколько файлов";screenTitle.SetBounds(30,102,cw-60,38);screenSubtitle.SetBounds(30,145,cw-60,24);queueCard.SetBounds(30,185,cw-60,Math.Max(320,body.Height-215));LayoutQueue();contentHeight=queueCard.Bottom+24;}
   else if(wizardStep==1){screenTitle.Text="Настройте результат";screenTitle.SetBounds(30,100,cw-60,36);screenSubtitle.SetBounds(30,141,cw-60,24);summaryCard.SetBounds(30,179,cw-60,88);summaryTitle.SetBounds(20,15,summaryCard.Width-40,28);summaryDetail.SetBounds(20,48,summaryCard.Width-40,24);wizardOperations.SetBounds(30,283,cw-60,92);for(int i=0;i<3;i++)operationButtons[i].SetBounds(i*((wizardOperations.Width-24)/3+12),0,(wizardOperations.Width-24)/3,92);navigation.SetBounds(30,389,cw-60,44);LayoutNavigation(false);int height=LayoutInspector(cw-60,false);settingsCard.SetBounds(30,451,cw-60,height);LayoutInspector(settingsCard.Width,false);contentHeight=settingsCard.Bottom+24;}
   else{resultCard.SetBounds(30,115,cw-60,Math.Max(365,body.Height-145));resultTitle.SetBounds(26,24,resultCard.Width-52,42);outputFiles.SetBounds(26,90,resultCard.Width-52,Math.Max(80,resultCard.Height-180));openResults.SetBounds(26,resultCard.Height-66,290,40);contentHeight=resultCard.Bottom+24;}
  }else{
   const int mainX=84,inspectorWidth=344;
   bool stacked=cw-mainX-inspectorWidth-36<350;
   int mainW=cw-mainX-(stacked?18:inspectorWidth+36);
   navigation.SetBounds(10,22,50,300);
   LayoutNavigation(true);
   screenTitle.SetBounds(mainX,18,mainW,36);
   screenSubtitle.SetBounds(mainX,61,mainW,24);
   int previewHeight=Math.Max(235,body.Height-356);
   mediaCard.SetBounds(mainX,103,mainW,previewHeight);
   mediaPreview.SetBounds(1,1,mainW-2,previewHeight-2);
   queueCard.SetBounds(mainX,mediaCard.Bottom+16,mainW,166);
   LayoutQueue();
   bool gif=type.SelectedIndex==4&&format.SelectedIndex==0;
   gifPreview.Visible=gifEstimate.Visible=gif;
   gifPreview.SetBounds(mainX,queueCard.Bottom+12,Math.Min(230,(mainW-12)/2),38);
   gifEstimate.SetBounds(gifPreview.Right+12,queueCard.Bottom+12,Math.Min(200,(mainW-12)/2),38);
   int mainBottom=gif?gifPreview.Bottom:queueCard.Bottom;
   if(stacked){
    int height=LayoutInspector(mainW,true);
    settingsCard.SetBounds(mainX,mainBottom+16,mainW,height);
   }else{
    settingsCard.SetBounds(mainX+mainW+18,20,inspectorWidth,Math.Max(320,body.Height-40));
   }
   LayoutInspector(settingsCard.Width,true);
   contentHeight=Math.Max(mainBottom,settingsCard.Bottom)+16;
  }
  if(layoutMode!=2){gifPreview.Visible=gifEstimate.Visible=false;}
  body.SetExtent(Math.Max(body.Height,contentHeight));
 }finally{layoutInProgress=false;}}
 void LayoutNavigation(bool vertical){for(int i=0;i<5;i++){var button=categoryButtons[i];if(vertical)button.SetBounds(0,i*55,48,44);else{int width=(navigation.Width-32)/5;button.SetBounds(i*(width+8),0,width,44);}}}
 void LayoutQueue(){int qw=queueCard.Width;queueTitle.SetBounds(18,18,Math.Max(80,qw-350),26);add.SetBounds(Math.Max(120,qw-294),12,174,36);removeFile.SetBounds(qw-108,12,90,36);files.MultiColumn=layoutMode==2;files.ColumnWidth=Math.Max(180,(qw-28)/3);files.SetBounds(14,64,qw-28,Math.Max(60,queueCard.Height-78));emptyQueue.Bounds=files.Bounds;emptyTitle.SetBounds(5,Math.Max(4,(emptyQueue.Height-74)/2),Math.Max(1,emptyQueue.Width-10),36);emptyDetail.SetBounds(5,emptyTitle.Bottom+8,Math.Max(1,emptyQueue.Width-10),25);}
 int LayoutInspector(int width,bool tall){
  bool tool=type.SelectedIndex==4,quality=type.SelectedIndex==0&&format.Text=="MP3";int inner=Math.Max(1,width-46);formatTitle.Text=tool?"Инструмент":"Формат";formatTitle.SetBounds(8,16,inner,25);format.SetBounds(8,48,inner,38);int y=104;qualityChoice.Visible=qualityTitle.Visible=quality;
  if(quality){if(!tall){int half=(inner-16)/2;formatTitle.Width=format.Width=half;qualityTitle.SetBounds(half+24,16,half,25);qualityChoice.SetBounds(half+24,48,half,38);}else{qualityTitle.SetBounds(8,y,inner,23);qualityChoice.SetBounds(8,y+28,inner,38);y+=86;}}
  bool inline=tall&&tool&&format.SelectedIndex==0;wasInlineGif=inline;gifInspector.Visible=inline;toolPanel.Visible=tool&&!inline;if(inline){gifInspector.SetBounds(8,y,inner,gifInspector.RequiredHeight);y+=gifInspector.RequiredHeight+14;}else if(tool){toolPanel.SetBounds(8,y,inner,toolPanel.Height);int wanted=toolPanel.RequiredHeight;toolPanel.Height=wanted;y+=wanted+14;}
  noteLabel.SetBounds(8,y,inner,tall?48:32);y+=tall?62:44;folderTitle.SetBounds(8,y,inner,24);if(tall){dest.SetBounds(8,y+30,Math.Max(1,inner-4),24);folder.SetBounds(8,y+62,Math.Min(180,inner),38);y+=118;}else{dest.SetBounds(8,y+34,Math.Max(1,inner-184),24);folder.SetBounds(inner-166,y+26,174,38);y+=82;}
  int desired=y+12;inspector.SetBounds(14,1,Math.Max(1,width-24),tall?Math.Max(1,settingsCard.Height-2):desired);inspector.SetExtent(desired);return desired+2;
 }
}
