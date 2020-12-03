using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minista.Themes
{
    /// <summary>
    ///     Default theme
    /// </summary>
    public class MinistaDarkTheme : MinistaTheme 
    {  
        // Default value of MinistaTheme is for Dark theme, so no need to change anything in it. 
    }

    public class MinistaWhiteTheme : MinistaTheme 
    {
        public MinistaWhiteTheme()
        {
            //<SolidColorBrush x:Key="DefaultBackgroundColor">#FFFFFFFF</SolidColorBrush>
            //<SolidColorBrush x:Key="DefaultItemBackgroundColor">#FFF0E3E4</SolidColorBrush>
            //<SolidColorBrush x:Key="DefaultForegroundColor">#FF000000</SolidColorBrush>
            //<SolidColorBrush x:Key="DefaultInnerForegroundColor">#FF171717</SolidColorBrush>
            //<SolidColorBrush x:Key="SeperatorColor">#FF787878</SolidColorBrush>
            //<SolidColorBrush x:Key="CategoryColor">#FF666666</SolidColorBrush>
            //<SolidColorBrush x:Key="ProfileTextColor">#FF474747</SolidColorBrush>
            //<SolidColorBrush x:Key="SeeMoreColor">#45000000</SolidColorBrush>
            //<SolidColorBrush x:Key="Comment4ButtonBackgroundColor">#FF1F1F1F</SolidColorBrush>
            //<SolidColorBrush x:Key="Comment4ForegroundColor">#FFFFFFFF</SolidColorBrush>
            //<SolidColorBrush x:Key="RefreshGoUpButtonBackgroundColor">#FFEBDEDF</SolidColorBrush>
            //<SolidColorBrush x:Key="DirectPaneBackgroundColor">#E5858585</SolidColorBrush>
            //<SolidColorBrush x:Key="DirectTextBoxBorderColor">#FF252525</SolidColorBrush>
            //<SolidColorBrush x:Key="StoryReplyTextBackgroundColor">#FF666666</SolidColorBrush>
            //<SolidColorBrush x:Key="StoryButtonBackoundColor">#FF666666</SolidColorBrush>
            //<SolidColorBrush x:Key="StoryButtonForeroundColor">#FF171717</SolidColorBrush>
            //<SolidColorBrush x:Key="UserSuggestionsCardBackgroundColor">#FFF0E3E4</SolidColorBrush>

            DefaultBackgroundColor = "#FFFFFFFF";
            DefaultItemBackgroundColor = "#FFF0E3E4";
            DefaultForegroundColor = "#FF000000";
            DefaultInnerForegroundColor = "#FF171717";
            SeperatorColor = "#FF787878";
            CategoryColor = "#FF666666";
            ProfileTextColor = "#FFD4D4D4";
            SeeMoreColor = "#45000000";
            Comment4ButtonBackgroundColor = "#FF1F1F1F";
            Comment4ForegroundColor = "#FFFFFFFF";
            RefreshGoUpButtonBackgroundColor = "#FF333333";
            DirectPaneBackgroundColor = "#E5141414";
            DirectTextBoxBorderColor = "#FF252525";
            StoryReplyTextBackgroundColor = "#7FB4B4B4";
            StoryButtonBackoundColor = "#7FB4B4B4";
            StoryButtonForeroundColor = "#FF232323";
            UserSuggestionsCardBackgroundColor = "#FF252525";
            HyperlinkTextForeroundColor = "#FF5278FF";
            TextBoxBackgroundColor = "#FF171717";

            LoadingForegroundColor = "#FF2C42FF";
            DirectItemBorderBrushColor = "#FFB8B1B1";
            DirectItemTextForegroundColor = "#FF000000";
            DirectItemTimeForegroundColor = "#FF3D3D3D";
            DirectItemSelfBackgroundColor = "#FFDED6D6";
            DirectItemPeopleBackgroundColor = "#00FFF6F6";
            DirectBlockAllForegroundColor = "#FFFF6C6C";
            DirectBlockPeopleForegroundColor = "#FFFF2727";
            DirectAcceptForegroundColor = "#FF34C08B";
            DirectUploadProgressBackgroundColor = "#4DFFFFFF";
            DirectUploadProgressForegroundColor = "#B6292828";
            DirectUploadProgressRingForegroundColor = "#D8575757";
            DirectVoiceProgressForegroundColor = "#FF007ACC";
            DirectLinkMediaForegroundColor = "#FFA6A6A6";
            DirectLeaveChatForegroundColor = "#FFFA5E5E";
            DirectIsTypingForegroundColor = "#FF919191";



            //<SolidColorBrush x:Key="DirectVoiceRecoderEllipseForegroundColor">#FFFA2929</SolidColorBrush>
            //<SolidColorBrush x:Key="DirectVoiceRecoderTimeForegroundColor">#FFFFFFFF</SolidColorBrush>
            //<SolidColorBrush x:Key="DirectVoiceRecordForegroundColor">#FFFFFFFF</SolidColorBrush>
            //<SolidColorBrush x:Key="DirectHeartButtonForegroundColor">#FF0068FF</SolidColorBrush>
            //<SolidColorBrush x:Key="DirectItemText2ForegroundColor">#FF2E2E2E</SolidColorBrush>
            DirectVoiceRecoderEllipseForegroundColor = "#FFFA2929";
            DirectVoiceRecoderTimeForegroundColor = "#FF000000";
            DirectVoiceRecordForegroundColor = "#FF000000";
            DirectHeartButtonForegroundColor = "#FF0068FF";
            DirectItemText2ForegroundColor = "#FF2E2E2E";


        }
    }


}
