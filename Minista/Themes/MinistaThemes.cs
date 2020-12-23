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

            DefaultBackgroundColor = "#FFFFFFFF";
            DefaultItemBackgroundColor = "#FFF0E3E4";
            DefaultForegroundColor = "#FF000000";
            DefaultInnerForegroundColor = "#FF171717";
            SeperatorColor = "#FF363636";
            CategoryColor = "#FF666666";
            ProfileTextColor = "#FFD4D4D4";
            SeeMoreColor = "#45000000";
            Comment4ButtonBackgroundColor = "#FF1F1F1F";
            Comment4ForegroundColor = "#FFFFFFFF";
            RefreshGoUpButtonBackgroundColor = "#FFEBDEDF";
            DirectPaneBackgroundColor = "#E5858585";
            DirectTextBoxBorderColor = "#FF6E6E6E";
            StoryReplyTextBackgroundColor = "#7FB4B4B4";
            StoryButtonBackoundColor = "#7FB4B4B4";
            StoryButtonForeroundColor = "#FF232323";
            UserSuggestionsCardBackgroundColor = "#FFF0E3E4";
            HyperlinkTextForeroundColor = "#FF5278FF";
            TextBoxBackgroundColor = "#BFB5B1B1";

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



            DirectVoiceRecoderEllipseForegroundColor = "#FFFA2929";
            DirectVoiceRecoderTimeForegroundColor = "#FF000000";
            DirectVoiceRecordForegroundColor = "#FF000000";
            DirectHeartButtonForegroundColor = "#FF0068FF";
            DirectItemText2ForegroundColor = "#FF2E2E2E";
            DirectVoicePlayerBackgroundColor = "#FFEDEDED";

        }
    }


}
