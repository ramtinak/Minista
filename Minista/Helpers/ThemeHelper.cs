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


                //(Application.Current.Resources["DefaultBackgroundColor"] as SolidColorBrush).Color = backgroundColor;
                //(Application.Current.Resources["DefaultItemBackgroundColor"] as SolidColorBrush).Color = theme.DefaultItemBackgroundColor.GetColorFromHex();
                //(Application.Current.Resources["DefaultForegroundColor"] as SolidColorBrush).Color = foregroundColor;
                //(Application.Current.Resources["DefaultInnerForegroundColor"] as SolidColorBrush).Color = innerForegroundColor;

                SetColorOrCurrentColor(nameof(theme.DefaultBackgroundColor), theme.DefaultBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.DefaultItemBackgroundColor), theme.DefaultItemBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.DefaultForegroundColor), theme.DefaultForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DefaultInnerForegroundColor), theme.DefaultInnerForegroundColor);


                //(Application.Current.Resources["SeperatorColor"] as SolidColorBrush).Color = theme.SeperatorColor.GetColorFromHex();
                //(Application.Current.Resources["CategoryColor"] as SolidColorBrush).Color = theme.CategoryColor.GetColorFromHex();
                //(Application.Current.Resources["ProfileTextColor"] as SolidColorBrush).Color = theme.ProfileTextColor.GetColorFromHex();
                //(Application.Current.Resources["SeeMoreColor"] as SolidColorBrush).Color = theme.SeeMoreColor.GetColorFromHex();


                SetColorOrCurrentColor(nameof(theme.SeperatorColor), theme.SeperatorColor);
                SetColorOrCurrentColor(nameof(theme.CategoryColor), theme.CategoryColor);
                SetColorOrCurrentColor(nameof(theme.ProfileTextColor), theme.ProfileTextColor);
                SetColorOrCurrentColor(nameof(theme.SeeMoreColor), theme.SeeMoreColor);



                
                //(Application.Current.Resources["Comment4ButtonBackgroundColor"] as SolidColorBrush).Color = theme.Comment4ButtonBackgroundColor.GetColorFromHex();
                //(Application.Current.Resources["Comment4ForegroundColor"] as SolidColorBrush).Color = theme.Comment4ForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["RefreshGoUpButtonBackgroundColor"] as SolidColorBrush).Color = theme.RefreshGoUpButtonBackgroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectPaneBackgroundColor"] as SolidColorBrush).Color = theme.DirectPaneBackgroundColor.GetColorFromHex();

                SetColorOrCurrentColor(nameof(theme.Comment4ButtonBackgroundColor), theme.Comment4ButtonBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.Comment4ForegroundColor), theme.Comment4ForegroundColor);
                SetColorOrCurrentColor(nameof(theme.RefreshGoUpButtonBackgroundColor), theme.RefreshGoUpButtonBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectPaneBackgroundColor), theme.DirectPaneBackgroundColor);

                //(Application.Current.Resources["DirectTextBoxBorderColor"] as SolidColorBrush).Color = theme.DirectTextBoxBorderColor.GetColorFromHex();
                //(Application.Current.Resources["StoryReplyTextBackgroundColor"] as SolidColorBrush).Color = theme.StoryReplyTextBackgroundColor.GetColorFromHex();
                //(Application.Current.Resources["StoryButtonBackoundColor"] as SolidColorBrush).Color = theme.StoryButtonBackoundColor.GetColorFromHex();
                //(Application.Current.Resources["StoryButtonForeroundColor"] as SolidColorBrush).Color = theme.StoryButtonForeroundColor.GetColorFromHex();

                SetColorOrCurrentColor(nameof(theme.DirectTextBoxBorderColor), theme.DirectTextBoxBorderColor);
                SetColorOrCurrentColor(nameof(theme.StoryReplyTextBackgroundColor), theme.StoryReplyTextBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.StoryButtonBackoundColor), theme.StoryButtonBackoundColor);
                SetColorOrCurrentColor(nameof(theme.StoryButtonForeroundColor), theme.StoryButtonForeroundColor);



                //(Application.Current.Resources["UserSuggestionsCardBackgroundColor"] as SolidColorBrush).Color = theme.UserSuggestionsCardBackgroundColor.GetColorFromHex();
                //(Application.Current.Resources["SystemControlHyperlinkTextBrush"] as SolidColorBrush).Color = theme.HyperlinkTextForeroundColor.GetColorFromHex();
                //(Application.Current.Resources["SystemControlHyperlinkBaseHighBrush"] as SolidColorBrush).Color = theme.HyperlinkTextForeroundColor.GetColorFromHex();
                //(Application.Current.Resources["SystemControlHyperlinkBaseMediumBrush"] as SolidColorBrush).Color = theme.HyperlinkTextForeroundColor.GetColorFromHex();
                //(Application.Current.Resources["SystemControlHyperlinkBaseMediumHighBrush"] as SolidColorBrush).Color = theme.HyperlinkTextForeroundColor.GetColorFromHex();

                SetColorOrCurrentColor(nameof(theme.UserSuggestionsCardBackgroundColor), theme.UserSuggestionsCardBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.HyperlinkTextForeroundColor), theme.HyperlinkTextForeroundColor, "SystemControlHyperlinkTextBrush");
                SetColorOrCurrentColor(nameof(theme.HyperlinkTextForeroundColor), theme.HyperlinkTextForeroundColor, "SystemControlHyperlinkBaseHighBrush");
                SetColorOrCurrentColor(nameof(theme.HyperlinkTextForeroundColor), theme.HyperlinkTextForeroundColor, "SystemControlHyperlinkBaseMediumBrush");
                SetColorOrCurrentColor(nameof(theme.HyperlinkTextForeroundColor), theme.HyperlinkTextForeroundColor, "SystemControlHyperlinkBaseMediumHighBrush");



                //(Application.Current.Resources["LoadingForegroundColor"] as SolidColorBrush).Color = theme.LoadingForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["TextBoxBackgroundColor"] as SolidColorBrush).Color = theme.TextBoxBackgroundColor.GetColorFromHex();
                //(Application.Current.Resources["SystemControlHighlightListAccentLowBrush"] as SolidColorBrush).Color = theme.UserSuggestionsCardBackgroundColor.GetColorFromHex();
                //(Application.Current.Resources["SystemControlHighlightListAccentMediumBrush"] as SolidColorBrush).Color = theme.UserSuggestionsCardBackgroundColor.GetColorFromHex();
                //(Application.Current.Resources["SystemControlHighlightListAccentHighBrush"] as SolidColorBrush).Color = theme.UserSuggestionsCardBackgroundColor.GetColorFromHex();
                SetColorOrCurrentColor(nameof(theme.LoadingForegroundColor), theme.LoadingForegroundColor);
                SetColorOrCurrentColor(nameof(theme.TextBoxBackgroundColor), theme.TextBoxBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.UserSuggestionsCardBackgroundColor), theme.UserSuggestionsCardBackgroundColor, "SystemControlHighlightListAccentLowBrush");
                SetColorOrCurrentColor(nameof(theme.UserSuggestionsCardBackgroundColor), theme.UserSuggestionsCardBackgroundColor, "SystemControlHighlightListAccentMediumBrush");
                SetColorOrCurrentColor(nameof(theme.UserSuggestionsCardBackgroundColor), theme.UserSuggestionsCardBackgroundColor, "SystemControlHighlightListAccentHighBrush");

                // DM
                //(Application.Current.Resources["DirectItemBorderBrushColor"] as SolidColorBrush).Color = theme.DirectItemBorderBrushColor.GetColorFromHex();
                //(Application.Current.Resources["DirectItemTextForegroundColor"] as SolidColorBrush).Color = theme.DirectItemTextForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectItemTimeForegroundColor"] as SolidColorBrush).Color = theme.DirectItemTimeForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectItemSelfBackgroundColor"] as SolidColorBrush).Color = theme.DirectItemSelfBackgroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectItemPeopleBackgroundColor"] as SolidColorBrush).Color = theme.DirectItemPeopleBackgroundColor.GetColorFromHex();

                SetColorOrCurrentColor(nameof(theme.DirectItemBorderBrushColor), theme.DirectItemBorderBrushColor);
                SetColorOrCurrentColor(nameof(theme.DirectItemTextForegroundColor), theme.DirectItemTextForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectItemTimeForegroundColor), theme.DirectItemTimeForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectItemSelfBackgroundColor), theme.DirectItemSelfBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectItemPeopleBackgroundColor), theme.DirectItemPeopleBackgroundColor);


                //(Application.Current.Resources["DirectBlockAllForegroundColor"] as SolidColorBrush).Color = theme.DirectBlockAllForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectBlockPeopleForegroundColor"] as SolidColorBrush).Color = theme.DirectBlockPeopleForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectAcceptForegroundColor"] as SolidColorBrush).Color = theme.DirectAcceptForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectUploadProgressBackgroundColor"] as SolidColorBrush).Color = theme.DirectUploadProgressBackgroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectUploadProgressForegroundColor"] as SolidColorBrush).Color = theme.DirectUploadProgressForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectUploadProgressRingForegroundColor"] as SolidColorBrush).Color = theme.DirectUploadProgressRingForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectVoiceProgressForegroundColor"] as SolidColorBrush).Color = theme.DirectVoiceProgressForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectLinkMediaForegroundColor"] as SolidColorBrush).Color = theme.DirectLinkMediaForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectLeaveChatForegroundColor"] as SolidColorBrush).Color = theme.DirectLeaveChatForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectIsTypingForegroundColor"] as SolidColorBrush).Color = theme.DirectIsTypingForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectVoiceRecoderEllipseForegroundColor"] as SolidColorBrush).Color = theme.DirectVoiceRecoderEllipseForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectVoiceRecoderTimeForegroundColor"] as SolidColorBrush).Color = theme.DirectVoiceRecoderTimeForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectVoiceRecordForegroundColor"] as SolidColorBrush).Color = theme.DirectVoiceRecordForegroundColor.GetColorFromHex();
                //(Application.Current.Resources["DirectHeartButtonForegroundColor"] as SolidColorBrush).Color = theme.DirectHeartButtonForegroundColor.GetColorFromHex();

                SetColorOrCurrentColor(nameof(theme.DirectBlockAllForegroundColor), theme.DirectBlockAllForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectBlockPeopleForegroundColor), theme.DirectBlockPeopleForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectAcceptForegroundColor), theme.DirectAcceptForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectUploadProgressBackgroundColor), theme.DirectUploadProgressBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectUploadProgressForegroundColor), theme.DirectUploadProgressForegroundColor);


                SetColorOrCurrentColor(nameof(theme.DirectUploadProgressRingForegroundColor), theme.DirectUploadProgressRingForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectVoiceProgressForegroundColor), theme.DirectVoiceProgressForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectLinkMediaForegroundColor), theme.DirectLinkMediaForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectLeaveChatForegroundColor), theme.DirectLeaveChatForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectIsTypingForegroundColor), theme.DirectIsTypingForegroundColor);

                SetColorOrCurrentColor(nameof(theme.DirectVoiceRecoderEllipseForegroundColor), theme.DirectVoiceRecoderEllipseForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectVoiceRecoderTimeForegroundColor), theme.DirectVoiceRecoderTimeForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectVoiceRecordForegroundColor), theme.DirectVoiceRecordForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectHeartButtonForegroundColor), theme.DirectHeartButtonForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectItemText2ForegroundColor), theme.DirectItemText2ForegroundColor);
                Helper.ShowStatusBar(backgroundColor, foregroundColor);
                Helper.ChangeTileBarTheme(foregroundColor, innerForegroundColor);

                if (NavigationService.Frame?.Content is Page page)
                    page.RequestedTheme = ElementTheme;

                if (Window.Current.Content != null && Window.Current.Content is Page page2)
                    page2.RequestedTheme = ElementTheme;
            }
            catch { }
        }


        static void SetColorOrCurrentColor(string resourceKey, string color, string resourceKey2 = null)
        {
            try
            {
                if (string.IsNullOrEmpty(resourceKey2))
                    resourceKey.SetColorToResource(color.GetNullableColorFromHex() ?? resourceKey.GetColorFromResource().Color);
                else
                    resourceKey2.SetColorToResource(color.GetNullableColorFromHex() ?? resourceKey.GetColorFromResource().Color);
            }
            catch { }

        }

    }
}
