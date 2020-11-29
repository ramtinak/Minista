using Windows.UI.Xaml;

namespace Minista
{
    public class MinistaThemeCore
    {
        public MinistaPublisher Publisher { get; set; }
        public MinistaTheme Theme { get; set; }
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
        public string DefaultBackgroundColor { get; set; }
        public string DefaultItemBackgroundColor { get; set; } 
        public string DefaultForegroundColor { get; set; }
        public string DefaultInnerForegroundColor { get; set; } 
        public string IconsColor { get; set; }
        public string HighlightColor { get; set; }
        public string ProfileCircleColor { get; set; }
        public string LikedColor { get; set; }
        public string SeperatorColor { get; set; }
        public string DirectBackgroundColor { get; set; }
        public string DirectMyMessageBackgroundColor { get; set; }
        public string DirectMyMessageForegroundColor { get; set; }
        public string DirectPeopleMessageBackgroundColor { get; set; }
        public string DirectPeopleMessageForegroundColor { get; set; }
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