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
                var theme = ministaTheme.Theme;
                if (theme == null) return;
                ElementTheme = theme.ElementTheme;
                var backgroundColor = theme.DefaultBackgroundColor.GetColorFromHex();
                var foregroundColor = theme.DefaultForegroundColor.GetColorFromHex();
                var innerForegroundColor = theme.DefaultInnerForegroundColor.GetColorFromHex();
                SetColorOrCurrentColor(nameof(theme.DefaultBackgroundColor), theme.DefaultBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.DefaultItemBackgroundColor), theme.DefaultItemBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.DefaultForegroundColor), theme.DefaultForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DefaultInnerForegroundColor), theme.DefaultInnerForegroundColor);
                SetColorOrCurrentColor(nameof(theme.SeperatorColor), theme.SeperatorColor);
                SetColorOrCurrentColor(nameof(theme.CategoryColor), theme.CategoryColor);
                SetColorOrCurrentColor(nameof(theme.ProfileTextColor), theme.ProfileTextColor);
                SetColorOrCurrentColor(nameof(theme.SeeMoreColor), theme.SeeMoreColor);
                SetColorOrCurrentColor(nameof(theme.Comment4ButtonBackgroundColor), theme.Comment4ButtonBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.Comment4ForegroundColor), theme.Comment4ForegroundColor);
                SetColorOrCurrentColor(nameof(theme.RefreshGoUpButtonBackgroundColor), theme.RefreshGoUpButtonBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectPaneBackgroundColor), theme.DirectPaneBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectTextBoxBorderColor), theme.DirectTextBoxBorderColor);
                SetColorOrCurrentColor(nameof(theme.StoryReplyTextBackgroundColor), theme.StoryReplyTextBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.StoryButtonBackoundColor), theme.StoryButtonBackoundColor);
                SetColorOrCurrentColor(nameof(theme.StoryButtonForeroundColor), theme.StoryButtonForeroundColor);
                SetColorOrCurrentColor(nameof(theme.UserSuggestionsCardBackgroundColor), theme.UserSuggestionsCardBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.HyperlinkTextForeroundColor), theme.HyperlinkTextForeroundColor, "SystemControlHyperlinkTextBrush");
                SetColorOrCurrentColor(nameof(theme.HyperlinkTextForeroundColor), theme.HyperlinkTextForeroundColor, "SystemControlHyperlinkBaseHighBrush");
                SetColorOrCurrentColor(nameof(theme.HyperlinkTextForeroundColor), theme.HyperlinkTextForeroundColor, "SystemControlHyperlinkBaseMediumBrush");
                SetColorOrCurrentColor(nameof(theme.HyperlinkTextForeroundColor), theme.HyperlinkTextForeroundColor, "SystemControlHyperlinkBaseMediumHighBrush");


                SetColorOrCurrentColor(nameof(theme.LoadingForegroundColor), theme.LoadingForegroundColor);
                SetColorOrCurrentColor(nameof(theme.TextBoxBackgroundColor), theme.TextBoxBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.UserSuggestionsCardBackgroundColor), theme.UserSuggestionsCardBackgroundColor, "SystemControlHighlightListAccentLowBrush");
                SetColorOrCurrentColor(nameof(theme.UserSuggestionsCardBackgroundColor), theme.UserSuggestionsCardBackgroundColor, "SystemControlHighlightListAccentMediumBrush");
                SetColorOrCurrentColor(nameof(theme.UserSuggestionsCardBackgroundColor), theme.UserSuggestionsCardBackgroundColor, "SystemControlHighlightListAccentHighBrush");

                // DM
            
                SetColorOrCurrentColor(nameof(theme.DirectItemBorderBrushColor), theme.DirectItemBorderBrushColor);
                SetColorOrCurrentColor(nameof(theme.DirectItemTextForegroundColor), theme.DirectItemTextForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectItemTimeForegroundColor), theme.DirectItemTimeForegroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectItemSelfBackgroundColor), theme.DirectItemSelfBackgroundColor);
                SetColorOrCurrentColor(nameof(theme.DirectItemPeopleBackgroundColor), theme.DirectItemPeopleBackgroundColor);
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
                SetColorOrCurrentColor(nameof(theme.DirectVoicePlayerBackgroundColor), theme.DirectVoicePlayerBackgroundColor);
                Helper.ShowStatusBar(backgroundColor, foregroundColor);
                Helper.ChangeTileBarTheme(foregroundColor, innerForegroundColor);

                if (MainPage.Current?.MyFrame?.Content is Page page)
                    page.RequestedTheme = ElementTheme;

                if (Window.Current.Content != null && Window.Current.Content is Page page2)
                    page2.RequestedTheme = ElementTheme;


                SettingsHelper.Settings.CurrentTheme = ministaTheme;
                SettingsHelper.SaveSettings();
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
