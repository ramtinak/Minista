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
                //App.CurrentX.InitFrame();
            }
            catch { }

        }
    }
}
