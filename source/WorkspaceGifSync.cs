using System;
partial class Converter {
 void CommitInlineGif(){if(layoutMode==2&&wasInlineGif&&gifInspector!=null){var value=gifInspector.ReadOptions();toolPanel.Gif=value;toolPanel.Configure(0);}}
 void RememberInlineGif(){try{CommitInlineGif();}catch{}}
}
