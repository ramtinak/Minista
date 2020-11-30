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
        }
    }


}
