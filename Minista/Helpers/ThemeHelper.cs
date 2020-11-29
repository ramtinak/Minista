using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media;

namespace Minista.Helpers
{
    public static class ThemeHelper
    {
        public static ElementTheme ElementTheme { get; set; } = ElementTheme.Dark;
        public static void InitTheme(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                "InitTheme No theme found in provided JSON".PrintDebug();
                return;
            }
            try
            {
                InitTheme(JsonConvert.DeserializeObject<MinistaThemeCore>(json));
            }
            catch { }
        }
        public static void InitTheme(MinistaThemeCore ministaTheme)
        {
            try
            {
                if (ministaTheme == null) return;
                if (ministaTheme.Theme == null) return;
                var theme = ministaTheme.Theme;
                ElementTheme = theme.ElementTheme;
                (Application.Current.Resources["DefaultBackgroundColor"] as SolidColorBrush).Color = theme.DefaultBackgroundColor.GetColorFromHex();
                (Application.Current.Resources["DefaultItemBackgroundColor"] as SolidColorBrush).Color = theme.DefaultItemBackgroundColor.GetColorFromHex();
                (Application.Current.Resources["DefaultForegroundColor"] as SolidColorBrush).Color = theme.DefaultForegroundColor.GetColorFromHex();
                (Application.Current.Resources["DefaultInnerForegroundColor"] as SolidColorBrush).Color = theme.DefaultInnerForegroundColor.GetColorFromHex();


                (Application.Current.Resources["SeperatorColor"] as SolidColorBrush).Color = theme.SeperatorColor.GetColorFromHex();
                (Application.Current.Resources["CategoryColor"] as SolidColorBrush).Color = theme.CategoryColor.GetColorFromHex();
                (Application.Current.Resources["ProfileTextColor"] as SolidColorBrush).Color = theme.ProfileTextColor.GetColorFromHex();
                (Application.Current.Resources["SeeMoreColor"] as SolidColorBrush).Color = theme.SeeMoreColor.GetColorFromHex();


                (Application.Current.Resources["Comment4ButtonBackgroundColor"] as SolidColorBrush).Color = theme.Comment4ButtonBackgroundColor.GetColorFromHex();
                (Application.Current.Resources["Comment4ForegroundColor"] as SolidColorBrush).Color = theme.Comment4ForegroundColor.GetColorFromHex();
                (Application.Current.Resources["RefreshGoUpButtonBackgroundColor"] as SolidColorBrush).Color = theme.RefreshGoUpButtonBackgroundColor.GetColorFromHex();
                (Application.Current.Resources["DirectPaneBackgroundColor"] as SolidColorBrush).Color = theme.DirectPaneBackgroundColor.GetColorFromHex();



                (Application.Current.Resources["DirectTextBoxBorderColor"] as SolidColorBrush).Color = theme.DirectTextBoxBorderColor.GetColorFromHex();
                (Application.Current.Resources["StoryReplyTextBackgroundColor"] as SolidColorBrush).Color = theme.StoryReplyTextBackgroundColor.GetColorFromHex();
                (Application.Current.Resources["StoryButtonBackoundColor"] as SolidColorBrush).Color = theme.StoryButtonBackoundColor.GetColorFromHex();
                (Application.Current.Resources["StoryButtonForeroundColor"] as SolidColorBrush).Color = theme.StoryButtonForeroundColor.GetColorFromHex();
                (Application.Current.Resources["UserSuggestionsCardBackgroundColor"] as SolidColorBrush).Color = theme.UserSuggestionsCardBackgroundColor.GetColorFromHex();

            }
            catch { }

        }
    }
}
