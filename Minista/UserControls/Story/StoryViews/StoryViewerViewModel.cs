using InstagramApiSharp;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using static Helper;
namespace Minista.UserControls.Story.StoryViews
{
    public class StoryViewerViewModel : BaseModel
    {
        public QuestionRespondersViewModel ResponderVM { get; set; } = new QuestionRespondersViewModel();
        public PollVotersViewModel PollVotersVM { get; set; } = new PollVotersViewModel();
        public QuizAnswerViewModel QuizAnswersVM { get; set; } = new QuizAnswerViewModel();
        public SliderVotersViewModel SliderVotersVM { get; set; } = new SliderVotersViewModel();

         

        public ObservableCollection<StoryViewObject> Items { get; set; } = new ObservableCollection<StoryViewObject>();
        public bool FirstRun = true;
        public bool HasMoreItems { get; set; } = true;
        public PaginationParameters Pagination { get; private set; } = PaginationParameters.MaxPagesToLoad(1);
        ScrollViewer Scroll;
        bool IsLoading = true;

        private InstaStoryItem _StoryItem;
        public InstaStoryItem StoryItem { get => _StoryItem; set { _StoryItem = value; OnPropertyChanged("StoryItem"); } }
        bool IsUpperItemsAdded = false;
        StoryViewObject StoryViewObjectViewers = new StoryViewObject();
        public string SliderThumbEmoji { get; set; } = "😂";


        public string ViewerCount => StoryItem == null || StoryItem?.ViewerCount == 0 ? "" : StoryItem?.ViewerCount.ToString();
        public void SetStoryItem(InstaStoryItem storyItem, ScrollViewer scrollViewer)
        {
            StoryItem = storyItem;
            ResetCache();

            RunLoadMore(true);
            SetLV(scrollViewer);
        }
        public void SetLV(ScrollViewer scrollViewer)
        {
            if (Scroll == null)
            {
                Scroll = scrollViewer;
                if (Scroll == null) return;
                Scroll.ViewChanging += ScrollViewChanging;
            }
            else
            {
                try
                {
                    Scroll.ViewChanging -= ScrollViewChanging;
                }
                catch { }
            }
        }
        public void ResetCache()
        {
            Pagination = PaginationParameters.MaxPagesToLoad(1);
            IsLoading = true;
            HasMoreItems = true;
            Items.Clear();
            FirstRun = true;
            IsUpperItemsAdded = false;
        }
        public async void RunLoadMore(bool refresh = false)
        {
            await RunLoadMoreAsync(refresh);
        }
        public async Task RunLoadMoreAsync(bool refresh = false)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await LoadMoreItemsAsync(refresh);
            });
        }
        async Task LoadMoreItemsAsync(bool refresh = false)
        {
            if (!HasMoreItems && !refresh)
            {
                IsLoading = false;
                return;
            }
            try
            {
                if (refresh)
                {
                    Pagination = PaginationParameters.MaxPagesToLoad(1);
                    IsUpperItemsAdded = false;
                    StoryViewObjectViewers = new StoryViewObject
                    {
                        Title = "Viewers",
                        SeeMoreText = "",
                        Type = StoryViewObjectType.Viewers,
                    };
                }
                var result = await InstaApi.StoryProcessor.GetStoryMediaViewersAsync(StoryItem.Pk.ToString(), Pagination);

                FirstRun = false;
                Pagination.MaximumPagesToLoad = 1;
                if (!result.Succeeded)
                {
                    IsLoading = false;
                    Hide(refresh);
                    return;

                }

                HasMoreItems = !string.IsNullOrEmpty(result.Value.NextMaxId);

                Pagination.NextMaxId = result.Value.NextMaxId;
                if (refresh) Items.Clear();
                if (!IsUpperItemsAdded)
                {
                    if (result.Value.UpdatedMedia != null)
                    {
                        if (result.Value.UpdatedMedia.StoryPollVoters.Count > 0)
                        {
                            if (result.Value.UpdatedMedia.StoryPollVoters[0].Voters.Count > 0)
                            {
                                int noCount = 0, yesCount = 0;
                                for (int i = 0; i < result.Value.UpdatedMedia.StoryPollVoters[0].Voters.Count; i++)
                                {
                                    if ((int)result.Value.UpdatedMedia.StoryPollVoters[0].Voters[i].Vote == 0)
                                        yesCount++;
                                    else
                                        noCount++;
                                }
                                var yesText = result.Value.UpdatedMedia.StoryPolls[0].PollSticker.Tallies[0].Text;
                                var noText = result.Value.UpdatedMedia.StoryPolls[0].PollSticker.Tallies[1].Text;
                                var storyPollResults = new StoryViewObject
                                {
                                    Title = "Poll Results",
                                    SeeMoreText = "See Voters >",
                                    Type = StoryViewObjectType.PollResults,
                                    YesVote = yesCount.ToString(),
                                    NoVote = noCount.ToString(),
                                    YesVoteText = yesText,
                                    NoVoteText = noText,
                                    StoryPollSticker = result.Value.UpdatedMedia.StoryPolls[0].PollSticker
                                };
                                for (int i = 0; i < result.Value.UpdatedMedia.StoryPollVoters.Count; i++)
                                {
                                    for (int j = 0; j < result.Value.UpdatedMedia.StoryPollVoters[i].Voters.Count; j++)
                                    {
                                        result.Value.UpdatedMedia.StoryPollVoters[i].Voters[j].VoteYesText = yesText;
                                        result.Value.UpdatedMedia.StoryPollVoters[i].Voters[j].VoteNoText = noText;
                                    }
                                }
                                storyPollResults.StoryPollVoters.AddRange(result.Value.UpdatedMedia.StoryPollVoters[0].Voters);

                                Items.Add(storyPollResults);
                                var storyObject = new StoryViewObject
                                {
                                    Title = "Voters",
                                    SeeMoreText = "",
                                    Type = StoryViewObjectType.PollVoters,
                                    YesVoteText = yesText,
                                    NoVoteText = noText,
                                };
                                storyObject.StoryPollVoters.AddRange(result.Value.UpdatedMedia.StoryPollVoters[0].Voters);

                                Items.Add(storyObject);
                            }
                        }

                        if (result.Value.UpdatedMedia.StoryQuestionsResponderInfos.Count > 0)
                        {
                            var storyResponses = new StoryViewObject
                            {
                                Title = "Responses",
                                SeeMoreText = "See All >",
                                Type = StoryViewObjectType.QuestionsResponses,
                            };
                            for(int i =0; i< result.Value.UpdatedMedia.StoryQuestionsResponderInfos[0].Responders.Count;i++)
                            {
                                result.Value.UpdatedMedia.StoryQuestionsResponderInfos[0].Responders[i].BackgroundColor = result.Value.UpdatedMedia.StoryQuestionsResponderInfos[0].BackgroundColor;
                                result.Value.UpdatedMedia.StoryQuestionsResponderInfos[0].Responders[i].TextColor = result.Value.UpdatedMedia.StoryQuestionsResponderInfos[0].TextColor;
                            }
                            storyResponses.StoryQuestionsResponders.AddRange(result.Value.UpdatedMedia.StoryQuestionsResponderInfos[0].Responders);
                            storyResponses.StoryQuestionInfo = result.Value.UpdatedMedia.StoryQuestionsResponderInfos[0];
                            Items.Add(storyResponses);
                        }
                        if (result.Value.UpdatedMedia.StoryQuizsParticipantInfos.Count > 0)
                        {
                            var obj = new StoryViewObject
                            {
                                Title = "Quiz Results",
                                SeeMoreText = "See Answers >",
                                Type = StoryViewObjectType.QuizResults,
                            };
                            obj.StoryQuizsParticipants.AddRange(result.Value.UpdatedMedia.StoryQuizsParticipantInfos[0].Participants);
                            obj.StoryQuiz = result.Value.UpdatedMedia.StoryQuizs[0].QuizSticker;
                            for (int i = 0; i < obj.StoryQuiz.Tallies.Count; i++)
                            {
                                obj.StoryQuiz.Tallies[i].GradientBackgroundColor = obj.StoryQuiz.StartBackgroundColor+ "|"+ obj.StoryQuiz.EndBackgroundColor;
                                obj.StoryQuiz.Tallies[i].TextColor = obj.StoryQuiz.TextColor;
                                if (i == obj.StoryQuiz.CorrectAnswer)
                                    obj.StoryQuiz.Tallies[i].CorrectAnswer = true;
                                else
                                    obj.StoryQuiz.Tallies[i].CorrectAnswer = false;
                            }
                            obj.StoryQuizTallies.AddRange(obj.StoryQuiz.Tallies);
                            Items.Add(obj);

                            var objAnswers = new StoryViewObject
                            {
                                Title = "Answers",
                                SeeMoreText = "",
                                Type = StoryViewObjectType.QuizAnswers,
                            };
                            for (int i = 0; i < result.Value.UpdatedMedia.StoryQuizsParticipantInfos[0].Participants.Count; i++)
                            {
                                var index = result.Value.UpdatedMedia.StoryQuizsParticipantInfos[0].Participants[i].Answer;
                                result.Value.UpdatedMedia.StoryQuizsParticipantInfos[0].Participants[i].AnswerText = obj.StoryQuiz.Tallies[index].Text;
                            }
                            objAnswers.StoryQuizsParticipants.AddRange(result.Value.UpdatedMedia.StoryQuizsParticipantInfos[0].Participants);
                            Items.Add(objAnswers);
                        }
                        if (result.Value.UpdatedMedia.StorySliderVoters.Count > 0)
                        {
                            var obj = new StoryViewObject
                            {
                                Title = "Slider Results",
                                SeeMoreText = "See Answers >",
                                Type = StoryViewObjectType.SliderResults,
                                SliderStickerItem = result.Value.UpdatedMedia.StorySliders[0].SliderSticker
                            };
                            obj.StorySliderVoters.AddRange(result.Value.UpdatedMedia.StorySliderVoters[0].Voters);
                            SliderThumbEmoji = result.Value.UpdatedMedia.StorySliders[0].SliderSticker.Emoji;
                            Items.Add(obj);

                            var objAnswers = new StoryViewObject
                            {
                                Title = "Answers",
                                SeeMoreText = "",
                                Type = StoryViewObjectType.SliderAnswers,
                            };
                            objAnswers.StorySliders.AddRange(result.Value.UpdatedMedia.StorySliders);
                            objAnswers.StorySliderVoters.AddRange(result.Value.UpdatedMedia.StorySliderVoters[0].Voters);

                            Items.Add(objAnswers);
                        }
                    }
                }
                if (result.Value?.Users?.Count > 0)
                    StoryViewObjectViewers.Viewers.AddRange(result.Value.Users);
                if (!IsUpperItemsAdded && StoryViewObjectViewers.Viewers.Count > 0)
                {
                    IsUpperItemsAdded = true;
                    Items.Add(StoryViewObjectViewers);
                }
                if (!IsUpperItemsAdded && result.Value?.UpdatedMedia != null)
                {
                    var dateNow = DateTime.UtcNow.ToUnixTime();
                    var expiringAt = result.Value.UpdatedMedia.ExpiringAt.ToUnixTime();
                    if (dateNow > expiringAt)
                    {
                        IsUpperItemsAdded = true;
                        Items.Add(new StoryViewObject
                        {
                            Type = StoryViewObjectType.ViewersNotAvailable
                        });
                    }
                }
                await Task.Delay(1000);
                IsLoading = false;
            }
            catch (Exception ex)
            {
                FirstRun =
                   IsLoading = false;
                ex.PrintException("StoryViewerViewModel.LoadMoreItemsAsync");
            }
            Hide(refresh);
        }
        void Hide(bool refresh)
        {
          
        }
        public void ScrollViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                if (Items == null)
                    return;
                if (!Items.Any())
                    return;
                ScrollViewer view = sender as ScrollViewer;

                double progress = view.VerticalOffset / view.ScrollableHeight;
                if (progress > 0.95 && IsLoading == false && !FirstRun )
                {
                    IsLoading = true;
                    RunLoadMore();
                }
            }
            catch (Exception ex) { ex.PrintException("StoryViewerViewModel.Scroll_ViewChanging"); }
        }

    }
    public class StoryViewObject : BaseModel
    {
        public string Title { get; set; }
        public string SeeMoreText { get; set; }
        public StoryViewObjectType Type { get; set; }
        private ObservableCollection<InstaUserShort> _Viewers = new ObservableCollection<InstaUserShort>();
        public ObservableCollection<InstaUserShort> Viewers { get => _Viewers; set { _Viewers = value; OnPropertyChanged("Viewers"); } }

        public InstaStoryPollStickerItem StoryPollSticker { get; set; }
        public InstaStoryQuestionInfo StoryQuestionInfo { get; set; }
        public ObservableCollection<InstaStoryQuestionResponder> StoryQuestionsResponders { get; set; } = new ObservableCollection<InstaStoryQuestionResponder>();
        public List<InstaStoryVoterItem> StoryPollVoters { get; set; } = new List<InstaStoryVoterItem>();

        public List<InstaStorySliderItem> StorySliders { get; set; } = new List<InstaStorySliderItem>();
        public InstaStorySliderStickerItem SliderStickerItem { get; set; }
        public ObservableCollection<InstaStoryVoterItem> StorySliderVoters { get; set; } = new ObservableCollection<InstaStoryVoterItem>();

        public ObservableCollection<InstaStoryQuizAnswer> StoryQuizsParticipants { get; set; } = new ObservableCollection<InstaStoryQuizAnswer>();
        public InstaStoryQuizStickerItem StoryQuiz { get; set; }
        public ObservableCollection<InstaStoryTalliesItem> StoryQuizTallies { get; set; } = new ObservableCollection<InstaStoryTalliesItem>();

        public string YesVote { get; set; }
        public string NoVote { get; set; }
        public string YesVoteText { get; set; }
        public string NoVoteText { get; set; } 
    }
    public enum StoryViewObjectType
    {
        PollResults,
        PollVoters,// male polle
        Viewers,
        ViewersNotAvailable,
        QuizResults,
        QuizAnswers,// male quize
        QuestionsResponses,
        SliderResults ,
        SliderAnswers,// male quize
    }
}
