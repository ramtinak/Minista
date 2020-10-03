using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Minista.Controls;
namespace Minista.ContentDialogs
{
    public sealed partial class StoryQuestionDialog : ContentPopup
    {
        private readonly StoryQuestionControl StoryQuestion;
        public StoryQuestionDialog(StoryQuestionControl storyControl)
        {
            InitializeComponent();
            StoryQuestion = storyControl;
            Loaded += StoryQuestionDialogLoaded;
        }

        private void StoryQuestionDialogLoaded(object sender, RoutedEventArgs e)
        {
            if (StoryQuestion.StoryQuestionItem == null) return;
            if (StoryQuestion.StoryQuestionItem.QuestionSticker == null) return;
            try
            {
                txtQuestion.Text = StoryQuestion.StoryQuestionItem.QuestionSticker.Question;
                txtQuestion.Foreground = ("#ff" + StoryQuestion.StoryQuestionItem.QuestionSticker.TextColor.Replace("#", "")).GetColorBrush();
                var bgColor = ("#ff" + StoryQuestion.StoryQuestionItem.QuestionSticker.BackgroundColor.Replace("#", "")).GetColorBrush();
                MainGrid.Background = bgColor;
                UserImage.Stroke = bgColor;
                UserImage.Fill = StoryQuestion.StoryQuestionItem.QuestionSticker.ProfilePicUrl.GetImageBrush();
            }
            catch { }
            try
            {
                txtQuestion.Focus(FocusState.Keyboard);
            }
            catch { }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private async void SendButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    SetHolding(true);

                    var result = await Helper.InstaApi.StoryProcessor
                    .AnswerToStoryQuestionAsync(StoryQuestion.StoryId,
                    StoryQuestion.StoryQuestionItem.QuestionSticker.QuestionId,
                    MessageText.Text.Trim().Replace("\r", ""));

                    if (result.Succeeded)
                        Helper.ShowNotify($"Your respond to {StoryQuestion.StoryUsername}'s question has been sent successfully.");
                    txtQuestion.Text = MessageText.Text = string.Empty;
                    Hide();
                    SetHolding(false);
                });
            }
            catch { SetHolding(false); Hide(); }
        }
        void SetHolding(bool flag)
        {
            try
            {
                if (Helpers.NavigationService.Frame.Content is Views.Main.StoryView story && story != null)
                    story.IsHolding = flag;
            }
            catch { }
        }
    }
}
