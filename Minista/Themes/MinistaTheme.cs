using Windows.UI.Xaml;

namespace Minista
{
    public class MinistaThemeCore
    {
        public MinistaPublisher Publisher { get; set; }
        public MinistaTheme Theme { get; set; } = new MinistaTheme();
        public string Font { get; set; }
    }

    public class MinistaPublisher
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
        public string PublisherInstagram { get; set; }
    }
     
    public class MinistaTheme
    {
        public ElementTheme ElementTheme { get; set; } = ElementTheme.Dark;
        public string DefaultBackgroundColor { get; set; } = "#FF151515";
        public string DefaultItemBackgroundColor { get; set; } = "#FF232323";
        public string DefaultForegroundColor { get; set; } = "#FFB4B4B4";
        public string DefaultInnerForegroundColor { get; set; } = "#FFC1C1C1";
        public string SeperatorColor { get; set; } = "#FF363636";
        public string CategoryColor { get; set; } = "#FF666666";
        public string ProfileTextColor { get; set; } = "#FFD4D4D4";
        public string SeeMoreColor { get; set; } = "#45000000";
        public string Comment4ButtonBackgroundColor { get; set; } = "#FF1F1F1F";
        public string Comment4ForegroundColor { get; set; } = "#FFFFFFFF";
        public string RefreshGoUpButtonBackgroundColor { get; set; } = "#FF333333";
        public string StoryReplyTextBackgroundColor { get; set; } = "#7F282828";
        public string StoryButtonBackoundColor { get; set; } = "#7F282828";
        public string StoryButtonForeroundColor { get; set; } = "#FFFFFFFF";
        public string UserSuggestionsCardBackgroundColor { get; set; } = "#FF252525";
        public string HyperlinkTextForeroundColor { get; set; } = "#FF5278FF";
        public string TextBoxBackgroundColor { get; set; } = "#BF292828";
        public string LoadingForegroundColor { get; set; } = "#FF2C42FF";


        public string DirectPaneBackgroundColor { get; set; } = "#E5141414";
        public string DirectTextBoxBorderColor { get; set; } = "#FF252525";
        public string DirectItemBorderBrushColor { get; set; } = "#FFA60019";
        public string DirectItemTextForegroundColor { get; set; } = "#FFFFFFFF";
        public string DirectItemTimeForegroundColor { get; set; } = "#FF858585";
        public string DirectItemSelfBackgroundColor { get; set; } = "#FF373737";
        public string DirectItemPeopleBackgroundColor { get; set; } = "#FF1D1C1C";
        public string DirectBlockAllForegroundColor { get; set; } = "#FFFF6C6C";
        public string DirectBlockPeopleForegroundColor { get; set; } = "#FFFF2727";
        public string DirectAcceptForegroundColor { get; set; } = "#FF34C08B";
        public string DirectUploadProgressBackgroundColor { get; set; } = "#4DFFFFFF";
        public string DirectUploadProgressForegroundColor { get; set; } = "#B6292828";
        public string DirectUploadProgressRingForegroundColor { get; set; } = "#D8575757";
        public string DirectVoiceProgressForegroundColor { get; set; } = "#FF007ACC";
        public string DirectLinkMediaForegroundColor { get; set; } = "#FFA6A6A6";
        public string DirectLeaveChatForegroundColor { get; set; } = "#FFFA5E5E";
        public string DirectIsTypingForegroundColor { get; set; } = "#FF919191";
        public string DirectVoiceRecoderEllipseForegroundColor { get; set; } = "#FFFA2929";
        public string DirectVoiceRecoderTimeForegroundColor { get; set; } = "#FFFFFFFF";
        public string DirectVoiceRecordForegroundColor { get; set; } = "#FFFFFFFF";
        public string DirectHeartButtonForegroundColor { get; set; } = "#FF8A0015";
        public string DirectItemText2ForegroundColor { get; set; } = "#FFCDCDCD";




        //public string IconsColor { get; set; }
        //public string HighlightColor { get; set; }
        //public string ProfileCircleColor { get; set; }
        //public string LikedColor { get; set; }
        //public string SeperatorColor { get; set; }
        //public string DirectBackgroundColor { get; set; }
        //public string DirectMyMessageBackgroundColor { get; set; }
        //public string DirectMyMessageForegroundColor { get; set; }
        //public string DirectPeopleMessageBackgroundColor { get; set; }
        //public string DirectPeopleMessageForegroundColor { get; set; }
    }
}
///////////////////////////////////// DEFAULT THEME /////////////////////////////////////
//<SolidColorBrush x:Key="DefaultBackgroundColor">#FF151515</SolidColorBrush>
//<SolidColorBrush x:Key="DefaultItemBackgroundColor">#FF232323</SolidColorBrush>
//<SolidColorBrush x:Key="DefaultForegroundColor"></SolidColorBrush>
//<SolidColorBrush x:Key="IconsColor"></SolidColorBrush>
//<SolidColorBrush x:Key="HighlightColor"></SolidColorBrush>
//<SolidColorBrush x:Key="ProfileCircleColor"></SolidColorBrush>
//<SolidColorBrush x:Key="LikedColor"></SolidColorBrush>
//<SolidColorBrush x:Key="SeperatorColor"></SolidColorBrush>
//<SolidColorBrush x:Key="DirectBackgroundColor"></SolidColorBrush>
//<SolidColorBrush x:Key="DirectMyMessageBackgroundColor"></SolidColorBrush>
//<SolidColorBrush x:Key="DirectMyMessageForegroundColor"></SolidColorBrush>
//<SolidColorBrush x:Key="DirectPeopleMessageBackgroundColor"></SolidColorBrush>
//<SolidColorBrush x:Key="DirectPeopleMessageForegroundColor"></SolidColorBrush>




///////////////////////////////////// DEFAULT THEME /////////////////////////////////////