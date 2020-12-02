using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;

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
                var backgroundColor = theme.DefaultBackgroundColor.GetColorFromHex();
                var foregroundColor = theme.DefaultForegroundColor.GetColorFromHex();
                var innerForegroundColor = theme.DefaultInnerForegroundColor.GetColorFromHex();

                (Application.Current.Resources["DefaultBackgroundColor"] as SolidColorBrush).Color = backgroundColor;
                (Application.Current.Resources["DefaultItemBackgroundColor"] as SolidColorBrush).Color = theme.DefaultItemBackgroundColor.GetColorFromHex();
                (Application.Current.Resources["DefaultForegroundColor"] as SolidColorBrush).Color = foregroundColor;
                (Application.Current.Resources["DefaultInnerForegroundColor"] as SolidColorBrush).Color = innerForegroundColor;


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
                (Application.Current.Resources["SystemControlHyperlinkTextBrush"] as SolidColorBrush).Color = theme.HyperlinkTextForeroundColor.GetColorFromHex();
                (Application.Current.Resources["SystemControlHyperlinkBaseHighBrush"] as SolidColorBrush).Color = theme.HyperlinkTextForeroundColor.GetColorFromHex();
                (Application.Current.Resources["SystemControlHyperlinkBaseMediumBrush"] as SolidColorBrush).Color = theme.HyperlinkTextForeroundColor.GetColorFromHex();
                (Application.Current.Resources["SystemControlHyperlinkBaseMediumHighBrush"] as SolidColorBrush).Color = theme.HyperlinkTextForeroundColor.GetColorFromHex();
                (Application.Current.Resources["LoadingForegroundColor"] as SolidColorBrush).Color = theme.LoadingForegroundColor.GetColorFromHex();

                (Application.Current.Resources["TextBoxBackgroundColor"] as SolidColorBrush).Color = theme.TextBoxBackgroundColor.GetColorFromHex();

                (Application.Current.Resources["SystemControlHighlightListAccentLowBrush"] as SolidColorBrush).Color = theme.UserSuggestionsCardBackgroundColor.GetColorFromHex();
                (Application.Current.Resources["SystemControlHighlightListAccentMediumBrush"] as SolidColorBrush).Color = theme.UserSuggestionsCardBackgroundColor.GetColorFromHex();
                (Application.Current.Resources["SystemControlHighlightListAccentHighBrush"] as SolidColorBrush).Color = theme.UserSuggestionsCardBackgroundColor.GetColorFromHex();


                // DM
                (Application.Current.Resources["DirectItemBorderBrushColor"] as SolidColorBrush).Color = theme.DirectItemBorderBrushColor.GetColorFromHex();
                (Application.Current.Resources["DirectItemTextForegroundColor"] as SolidColorBrush).Color = theme.DirectItemTextForegroundColor.GetColorFromHex();
                (Application.Current.Resources["DirectItemTimeForegroundColor"] as SolidColorBrush).Color = theme.DirectItemTimeForegroundColor.GetColorFromHex();
                (Application.Current.Resources["DirectItemSelfBackgroundColor"] as SolidColorBrush).Color = theme.DirectItemSelfBackgroundColor.GetColorFromHex();
                (Application.Current.Resources["DirectItemPeopleBackgroundColor"] as SolidColorBrush).Color = theme.DirectItemPeopleBackgroundColor.GetColorFromHex();



                Helper.ShowStatusBar(backgroundColor, foregroundColor);
                Helper.ChangeTileBarTheme(foregroundColor, innerForegroundColor);

                if (NavigationService.Frame?.Content is Page page)
                    page.RequestedTheme = ElementTheme;

                if (Window.Current.Content != null && Window.Current.Content is Page page2)
                    page2.RequestedTheme = ElementTheme;
            }
            catch { }

        }
    }
}
