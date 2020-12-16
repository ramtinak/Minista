using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Minista.Helpers;
using Minista.Views.Stories;

namespace Minista.UserControls.Story.StoryViews
{
    public sealed partial class MainStoryViewerUc : UserControl
    {
        private StoryItemUc StoryItemUc;
        private UserStoryUc UserStoryUc;
        public MainStoryViewerUc()
        {
            this.InitializeComponent();
        }
        void GoBack()
        {
            if (NavigationService.Frame.Content is Views.Main.StoryView view && view != null)
            {
                Visibility = Visibility.Collapsed;
                view.StorySuffItems.Visibility = Visibility.Visible;
                view.IsHolding = false;
            }
            else if (typeof(StoryViewX).IsThisType())
            {
                Visibility = Visibility.Collapsed;
                if (StoryItemUc != null)
                    StoryItemUc.StorySuffItems.Visibility = Visibility.Visible;
                if (UserStoryUc != null)
                    UserStoryUc.ControlPanels(false);
            }
        }
        public async void SetStoryItem(InstaStoryItem storyItem)
        {
            ShowGrid(true);
            Visibility = Visibility.Visible;
            InsightsButton.Visibility = UserHelper.IsBusiness ? Visibility.Visible : Visibility.Collapsed;
            await Task.Delay(420); 

            StoryVM.SetStoryItem(storyItem, MainLV?.FindScrollViewer());
            if (UserHelper.IsBusiness)
            {
                StoryInsightUc.SetItem(storyItem);
            }
        }
        public void SetStoryItemUc(StoryItemUc storyItemUc, UserStoryUc userStoryUc)
        {
            StoryItemUc = storyItemUc;
            UserStoryUc = userStoryUc;

            if (storyItemUc != null)
                StoryItemUc.StorySuffItems.Visibility = Visibility.Collapsed;
            UserStoryUc?.ControlPanels(true);
        }

        private void GridLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void BackButtonClick(object sender, RoutedEventArgs e) => CanPressBack();

        private async void SeeMoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                if (sender is Button btn && btn != null)
                {
                    if (btn.DataContext is StoryViewObject obj && obj != null)
                    {
                        if (obj.Type == StoryViewObjectType.QuestionsResponses)
                        {
                            ShowResponsedGrid();
                            await Task.Delay(450);
                            if (SecondText2Block != null)
                                SecondText2Block.Text = obj.Title;
                            StoryVM.ResponderVM.SetItem(StoryVM.StoryItem, obj.StoryQuestionInfo, QuestionRespondersGV.FindScrollViewer());
                        }
                        else if (obj.Type == StoryViewObjectType.PollResults)
                        {
                            ShowPollGrid();
                            try
                            {
                                MainPivot.SelectedIndex = 1;
                            }
                            catch
                            {
                                MainPivot.SelectedIndex = 0;
                            }
                            await Task.Delay(450);
                            if (SecondText2Block != null)
                                SecondText2Block.Text = obj.Title;
                            MainPivot.SelectedIndex = 0;
                            StoryVM.PollVotersVM.SetItem(StoryVM.StoryItem, obj.StoryPollSticker, LVYes?.FindScrollViewer(), LVNo?.FindScrollViewer());
                        }
                        else if (obj.Type == StoryViewObjectType.QuizResults)
                        {
                            ShowQuizGrid();
                            await Task.Delay(450);
                            if (SecondText2Block != null)
                                SecondText2Block.Text = obj.Title;
                            StoryVM.QuizAnswersVM.SetItem(StoryVM.StoryItem, obj.StoryQuiz, LVQuizAnswers?.FindScrollViewer());
                        }
                        else if (obj.Type == StoryViewObjectType.SliderResults)
                        {
                            ShowSliderVoteGrid();
                            await Task.Delay(450);
                            if (SecondText2Block != null)
                                SecondText2Block.Text = obj.Title;
                            StoryVM.SliderVotersVM.SetItem(StoryVM.StoryItem, obj.SliderStickerItem, LVSliderVote?.FindScrollViewer());
                        }
                        //ShowSliderVoteGrid
                    }
                }
            }
        }

        private void ViewerMenuButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void UserTapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void ShareMenuButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void SliderTextLoaded(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                if (sender is TextBlock textBlock)
                    textBlock.Text = StoryVM.SliderThumbEmoji;
            }
        }
        public bool CanPressBack()
        {
            if (SliderVoteGrid.Visibility == Visibility.Visible || PollVotersGrid.Visibility == Visibility.Visible ||
                QuizAnswersGrid.Visibility == Visibility.Visible || QuestionRespondersGrid.Visibility == Visibility.Visible ||
                QuestionRespondersGrid.Visibility == Visibility.Visible)
            {
                ShowMainGrid();
                return false;
            }
            if (Visibility == Visibility.Visible)
            {
                GoBack();
                return false;
            }
            return true;
        }
        void ShowMainGrid()
        {
            MainTextBlock.Visibility = MainGrid.Visibility = Visibility.Visible;
            QuestionRespondersGrid.Visibility = Visibility.Collapsed;
            QuizAnswersGrid.Visibility = SliderVoteGrid.Visibility = PollVotersGrid.Visibility = SecondTextBlock.Visibility = Visibility.Collapsed;
        }
        void ShowResponsedGrid()
        {
            SecondTextBlock.Visibility = Visibility.Visible;
            QuestionRespondersGrid.Visibility = Visibility.Visible;
            QuizAnswersGrid.Visibility = PollVotersGrid.Visibility = MainTextBlock.Visibility = MainGrid.Visibility = Visibility.Collapsed;
        }
        void ShowPollGrid()
        {
            SecondTextBlock.Visibility = Visibility.Visible;
            PollVotersGrid.Visibility = Visibility.Visible;
            QuizAnswersGrid.Visibility = QuestionRespondersGrid.Visibility = MainTextBlock.Visibility = MainGrid.Visibility = Visibility.Collapsed;
        }

        void ShowQuizGrid()
        {
            SecondTextBlock.Visibility = Visibility.Visible;
            QuizAnswersGrid.Visibility = Visibility.Visible;
            PollVotersGrid.Visibility = QuestionRespondersGrid.Visibility = MainTextBlock.Visibility = MainGrid.Visibility = Visibility.Collapsed;
        }
        void ShowSliderVoteGrid() 
        {
            SecondTextBlock.Visibility = Visibility.Visible;
            SliderVoteGrid.Visibility = Visibility.Visible;
            QuizAnswersGrid.Visibility = PollVotersGrid.Visibility = QuestionRespondersGrid.Visibility = MainTextBlock.Visibility = MainGrid.Visibility = Visibility.Collapsed;
        }

        void ShowGrid(bool viewer)
        {
            try
            {
                if (viewer)
                {
                    Grid1.Visibility = Visibility.Visible;
                    StoryInsightUc.Visibility = Visibility.Collapsed;
                }
                else
                {
                    StoryInsightUc.Visibility = Visibility.Visible;
                    Grid1.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }
        private void MainTextBlockClick(object sender, RoutedEventArgs e) => ShowGrid(true);
        private void InsightsButtonClick(object sender, RoutedEventArgs e) => ShowGrid(false);

    }
}
