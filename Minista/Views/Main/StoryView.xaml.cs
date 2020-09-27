using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using InstagramApiSharp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Minista.ContentDialogs;
using Minista.Helpers;
using Minista.UserControls.Main;
using Minista.Views.Infos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Minista.UserControls.Story;
using Minista.Controls;
using InstagramApiSharp.Classes;
using UICompositionAnimations;
using UICompositionAnimations.Animations;
using UICompositionAnimations.AttachedProperties;
using UICompositionAnimations.Behaviours;
using UICompositionAnimations.Helpers;
using UICompositionAnimations.Enums;

using System.Numerics;
namespace Minista.Views.Main
{
    public sealed partial class StoryView : Page, INotifyPropertyChanged
    {
        public ObservableCollection<string> ReactionItems { get; set; } = new ObservableCollection<string>
        {
            "😂", "😮", "😍","😢", "👏", "🔥", "🎉", "💯"
        };
        public static List<InstaReelFeed> FeedListStatic = new List<InstaReelFeed>();
        public List<InstaReelFeed> FeedList = new List<InstaReelFeed>();
        public int FeedListIndex = 0;
        public DispatcherTimer Timer = new DispatcherTimer();
        public int StoryIndex { get; set; } = 0;
        int IntervalTimer { get; set; } = 0;
        const int MaxIntervalForImage = 5;

        readonly List<ProgressBar> ProgressBarList = new List<ProgressBar>();
        public Dictionary<int, StoryFeedItemUc> Items { get; set; } = new Dictionary<int, StoryFeedItemUc>();
        public bool IsHolding { get; set; } = false;
        bool IsHighlight { get; set; } = false;
        public string Title { get; set; } 

        readonly GestureRecognizer GestureRecognizer = new GestureRecognizer();


        public InstaReelFeed StoryFeed
        {
            get
            {
                return (InstaReelFeed)GetValue(StoryFeedProperty);
            }
            set
            {
                SetValue(StoryFeedProperty, value);
                this.DataContext = value;
                OnPropertyChanged2("StoryFeed");

            }
        }
        public static readonly DependencyProperty StoryFeedProperty =
            DependencyProperty.Register("StoryFeed",
                typeof(InstaReelFeed),
                typeof(StoryView),
                new PropertyMetadata(null));
        readonly GestureHelper GestureHelper;
        public StoryView()
        {
            this.InitializeComponent();
            Timer.Interval = TimeSpan.FromSeconds(1);
            Timer.Tick += TimerTick;
            DataContextChanged += StoryFeedUcDataContextChanged;
            GestureRecognizer.GestureSettings = GestureSettings.HoldWithMouse;
            ManipulationMode = ManipulationModes.TranslateX;
            ManipulationCompleted += StoryViewManipulationCompleted;
            //GestureRecognizer.Holding += GestureRecognizerHolding;
            PointerPressed += GridPointerPressed;
            PointerMoved += GridPointerMoved;
            PointerReleased += GridPointerReleased;
            GestureHelper = new GestureHelper(this, GestureMode.LeftRight);
            GestureHelper.LeftSwipe += GestureHelperLeftSwipe;
            GestureHelper.RightSwipe += GestureHelperRightSwipe;
            Loaded += StoryViewLoaded;
            SetUpPageAnimation();
        }

        private void GestureHelperRightSwipe(object sender, EventArgs e)
        {
            if (MainStoryViewerUc.Visibility == Visibility.Visible) return;
            //SkipPrevious();
            SkipPreviousFeedUser();
        }

        private void GestureHelperLeftSwipe(object sender, EventArgs e)
        {
            if (MainStoryViewerUc.Visibility == Visibility.Visible) return;
            //SkipNext();
            SkipNextFeedUser();
        }
        private void StoryViewManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            //if (e.Cumulative.Translation.X < -50)
            //{
            //    "-50".PrintDebug();
            //}

            //if (e.Cumulative.Translation.X > 50)
            //{
            //    "50".PrintDebug();
            //}
        }

        void SetUpPageAnimation()
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new ContinuumNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;
        }
        private void StoryViewLoaded(object sender, RoutedEventArgs e)
        {
            if(FeedList?.Count > 0)
            PlayFeedUser();
            ReactionGV.ItemsSource = ReactionItems;
        }
        private bool WasItBackButtonShown = false;
        private string SelectedStoryId = null;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MainPage.Current?.HideHeaders();
            Helper.HideStatusBar();

            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();

            if (MainPage.Current != null)
            {
                if(MainPage.Current.BackButton.Visibility == Visibility.Visible)
                {
                    WasItBackButtonShown = true;
                    NavigationService.HideBackButton();
                }
                NavigationService.ShowSystemBackButton();
            }
            try
            {
                FeedListStatic = new List<InstaReelFeed>();
                if (e.Parameter is object[] objArr && objArr != null)
                {
                    if (objArr.Length == 2)
                    {
                        if (objArr[0] is List<InstaReelFeed> reels)
                        {
                            FeedList = reels;
                            FeedListIndex = (int)objArr[1];
                        }
                        else if (objArr[0] is List<InstaHighlightFeed> highlights)
                        {
                            var reelFeeds = new List<InstaReelFeed>();
                            foreach (var f in highlights)
                                reelFeeds.Add(f.ToReelFeed());
                            FeedList = reelFeeds;
                            FeedListIndex = (int)objArr[1];
                        }
                    }
                    else if (objArr.Length == 3)
                    {
                        var user = objArr[0] as string;
                        var storyId = objArr[1] as string;
                        //var url = objArr[3] as string; // in dekorie ke faghat lengthemon beshe 3ta
                        user = user.Trim();
                        SelectedStoryId = storyId.Trim();
                        var userResult = await Helper.InstaApi.UserProcessor.GetUserInfoByUsernameAsync(user);
                        if (userResult.Succeeded)
                        {
                            var stories = await Helper.InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(userResult.Value.Pk.ToString());
                            if (stories.Succeeded)
                            {
                                FeedList = stories.Value.Items;
                                FeedListIndex = 0;
                                PlayFeedUser();
                            }
                        }
                    }
                    else if (objArr.Length == 5)
                    {
                        var user = objArr[0] as InstaUserInfo;
                        var storyId = objArr[1] as string;
                        //var url = objArr[3] as string; // in dekorie ke faghat lengthemon beshe 3ta
                        SelectedStoryId = storyId.Trim();

                        var stories = await Helper.InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(user.Pk.ToString());
                        if (stories.Succeeded)
                        {
                            FeedList = stories.Value.Items;
                            FeedListIndex = 0;
                            PlayFeedUser();
                        }
                    }
                    else if (objArr.Length == 4)
                    {
                        var userId = (long)objArr[0];
                        var storyId = objArr[1] as string;
                        SelectedStoryId = storyId.Trim();

                        var stories = await Helper.InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(userId.ToString());
                        if (stories.Succeeded)
                        {
                            FeedList = stories.Value.Items;
                            FeedListIndex = 0;
                            PlayFeedUser();
                        }
                    }
                }
                else if (e.Parameter is InstaReelFeed reel && reel != null)
                {
                    FeedList = new List<InstaReelFeed> { reel };
                    FeedListIndex = 0;
                }
                else
                {
                    long pk = -1;
                    if (e.Parameter is InstaUserShort userShort)
                        pk = userShort.Pk;
                    else if (e.Parameter is long userId)
                        pk = userId;

                    if (pk != -1)
                    {
                        var stories = await Helper.InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(pk.ToString());
                        if (stories.Succeeded)
                        {
                            FeedList = stories.Value.Items;
                            FeedListIndex = 0;
                            PlayFeedUser();
                        }
                    }
                }
            }
            catch { }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MainPage.Current?.ShowHeaders();
            Helper.ShowStatusBar();

            NavigationService.HideSystemBackButton();
            if (MainPage.Current != null && WasItBackButtonShown)
                NavigationService.ShowBackButton();
            WasItBackButtonShown = false;
            try
            {
                FeedListStatic = FeedList;
                StopEverything();
            }
            catch { }
        }
        private void GridPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (MainStoryViewerUc.Visibility == Visibility.Visible) return;
                var ps = e.GetIntermediatePoints(null);
                if (ps != null && ps.Count > 0)
                {
                    GestureRecognizer.ProcessDownEvent(ps[0]);
                    e.Handled = true;
                    if (!IsHolding)
                    {
                        IsHolding = true;
                        HideHoldingPanels();
                    }
                    //else
                    //    HideStoryInnerPanels();
                }
            }
            catch { }
        }
        private void GridPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (MainStoryViewerUc.Visibility == Visibility.Visible) return;
                GestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(null));
                e.Handled = true;
            }
            catch { }
        }

        private void GridPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (MainStoryViewerUc.Visibility == Visibility.Visible) return;
                var ps = e.GetIntermediatePoints(null);
                if (ps != null && ps.Count > 0)
                {
                    GestureRecognizer.ProcessUpEvent(ps[0]);
                    e.Handled = true;
                    GestureRecognizer.CompleteGesture();
                    IsHolding = false;
                    WasPaused = false;
                    ShowHoldingPanels();
                    if (Items[StoryIndex]?.StoryItem.MediaType == InstaMediaType.Video)
                    {
                        Items[StoryIndex].MediaElement.Play();
                    }
                }
            }
            catch { }
        }

        private async void StoryFeedUcDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                //args.NewValue.GetType().PrintDebug();
                if (args.NewValue is InstaReelFeed reel && reel != null)
                {
                    Items.Clear();
                    ChildGrid.Children.Clear();
                    ProgressGrid.Children.Clear();
                    ProgressGrid.ColumnDefinitions.Clear();
                    try
                    {
                        ProgressBarList.Clear();
                    }
                    catch { }
                    StoryIndex = 0;
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        try
                        {
                            if (StoryFeed.Items.Count == 0)
                            {
                                var users = new List<string>
                            {
                                StoryFeed.Id
                            };

                                var storiesAfterResult = await Helper.InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(users.ToArray());
                                if (storiesAfterResult.Succeeded)
                                {
                                    var storiesAfter = storiesAfterResult.Value.Items;

                                    StoryFeed.Items.AddRange(storiesAfter.FirstOrDefault().Items);
                                    FeedList[FeedListIndex].Items.Clear();
                                    FeedList[FeedListIndex].Items.AddRange(storiesAfter.FirstOrDefault().Items);

                                    if (MainView.Current?.MainVM?.Stories?.Count > 0)
                                    {
                                        try
                                        {
                                            foreach (var item in MainView.Current.MainVM.Stories)
                                            {
                                                //var single = storiesAfter.SingleOrDefault(ss => ss.User.Pk.ToString() == item.User.Pk.ToString());
                                                var single = storiesAfter.SingleOrDefault(ss => ss.Id == item.Id);
                                                if (single != null)
                                                {
                                                    item.Items.Clear();
                                                    item.Items.AddRange(single.Items);
                                                    break;
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }


                            foreach (var item in StoryFeed.Items)
                                ProgressGrid.ColumnDefinitions.Add(GenerateColumn());
                            int ix = 0;
                            var margin = new Thickness(2.5, 5, 2.5, 5);
                            if (StoryFeed.Items.Count > 14 && StoryFeed.Items.Count < -20)
                                margin = new Thickness(2.0, 5, 2.0, 5);
                            else if (StoryFeed.Items.Count > 20)
                                margin = new Thickness(.4, 5, .4, 5);

                            foreach (var item in StoryFeed.Items)
                            {
                                try
                                {
                                    ProgressBar p = GenerateProgress(margin);

                                    //var shadow = GenerateShadowPanel();
                                    //shadow.Content = p;
                                    if (item.MediaType == InstaMediaType.Video)
                                        p.Maximum = item.VideoDuration;
                                    else
                                        p.Maximum = MaxIntervalForImage;

                                    Grid.SetColumn(p, ix);
                                    ProgressBarList.Add(p);
                                    ProgressGrid.Children.Add(p);

                                    var feedUc = GenerateFeedItem(item);
                                    //feedUc.Holding += FeedUcHolding;
                                    Items.Add(ix, feedUc);
                                    ix++;
                                }
                                catch { }
                            }


                            //var seens = FeedList.Select((item,i) => new { Seen = item, Index = i });

                            int index = -1;
                            if (Items?.Count > 0)
                            {
                                //if (StoryFeed.Seen > 1 && StoryFeed.Items.LastOrDefault()?.TakenAt.Year > 2009)
                                //{
                                //    StoryIndex++;
                                //}
                                //else

                                for (int i = 0; i < Items.Count; i++)
                                {
                                    if (string.IsNullOrEmpty(SelectedStoryId))
                                    {
                                        if (Items[i].StoryItem.TakenAt.ToUnixTime() == StoryFeed.Seen)
                                        {
                                            //if (i + 1 < Items.Count - 1)
                                            index = i + 1;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (Items[i].StoryItem.Id == SelectedStoryId ||
                                        Items[i].StoryItem.Pk.ToString() == SelectedStoryId)
                                        {
                                            index = i;
                                            SelectedStoryId = null;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (index != -1)
                            {
                                if (index > Items.Count - 1)
                                    index = 0;
                                StoryIndex = index;
                                for (int i = 0; i < StoryIndex; i++)
                                {
                                    try
                                    {
                                        ProgressBarList[i].Value = ProgressBarList[i].Maximum;
                                    }
                                    catch { }
                                }
                            }

                            Play();
                            SeenStory();
                            StartTimer();
                        }
                        catch { }
                    });
                        
                }

            }
            catch { }
        }

        //private void FeedUcHolding(object sender, HoldingRoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (e.HoldingState == HoldingState.Started)
        //        {
        //            IsHolding = true;
        //            HideHoldingPanels();
        //        }
        //        else
        //        {
        //            IsHolding = false;
        //            WasPaused = false;
        //            ShowHoldingPanels();
        //            if (Items[StoryIndex]?.StoryItem.MediaType == InstaMediaType.Video)
        //                Items[StoryIndex].MediaElement.Play();
        //        }
        //    }
        //    catch { }
        //}


        private void UserButtonClick(object sender, RoutedEventArgs e)
        {
            if (MainStoryViewerUc.Visibility == Visibility.Visible) return;
            try
            {
                Helper.OpenProfile(Items[StoryIndex]?.StoryItem.User);
            }
            catch { }
        }

        public void StartTimer() => Timer.Start();
        public void StopTimer() => Timer.Stop();
        bool WasPaused = false;
        private void TimerTick(object sender, object e)
        {
            try
            {
                try
                {
                    Items[StoryIndex]?.MediaElement.CurrentState.PrintDebug();
                }
                catch { }
                if (Items[StoryIndex]?.StoryItem.MediaType == InstaMediaType.Image && !Items[StoryIndex].IsImageOpened)
                    return;
     
                if (Items[StoryIndex]?.StoryItem.MediaType == InstaMediaType.Video)
                {
                    if (Items[StoryIndex]?.MediaElement != null && Items[StoryIndex]?.MediaElement.CurrentState != MediaElementState.Playing &&
                         Items[StoryIndex]?.MediaElement.CurrentState != MediaElementState.Closed)
                        return;
                }
                if (IsHolding)
                {
                    $"Holding is on".PrintDebug();

                    if (Items[StoryIndex]?.StoryItem != null)
                        if (Items[StoryIndex]?.StoryItem.MediaType == InstaMediaType.Video)
                        {
                            Items[StoryIndex].MediaElement.Pause();
                            WasPaused = true;
                        }
                    return;
                }
                if(UserGrid.Visibility == Visibility.Collapsed)
                    ShowHoldingPanels();
                if (WasPaused)
                    if (Items[StoryIndex]?.StoryItem != null)
                        if (Items[StoryIndex]?.StoryItem.MediaType == InstaMediaType.Video)
                        /* Items[StoryIndex].MediaElement*/
                        {
                            WasPaused = false;
                            LatestMediaElement.Play();
                        }
                //var uc = Items[StoryIndex];
                //var item = uc.StoryItem;
                //int IntervalTimer { get; set; } = 0;
                //const int MaxIntervalForImage = 5;
                //Holding    //StoryIndex
                if (Items[StoryIndex].StoryItem != null)
                {
                    if (Items[StoryIndex].StoryItem.MediaType == InstaMediaType.Image)
                    {
                        if (IntervalTimer >= MaxIntervalForImage)
                        {
                            SkipNext();
                        }
                        else
                        {
                            IntervalTimer++;
                            UpdateProgress();
                        }
                    }
                    else
                    {
                        if (IntervalTimer > Items[StoryIndex].StoryItem.VideoDuration)
                        {
                            SkipNext();
                        }
                        else
                        {
                            if (VideoIsPlaying)
                            {
                                IntervalTimer++;
                                UpdateProgress();
                            }
                        }
                    }
                }
            }
            catch { }
        }
        /*async*/ void UpdateProgress()
        {
            try
            {
                //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                //{
                //    for (int i = 0; i <= 100; i++)
                //    {
                //        try
                //        {
                //            ProgressBarList[StoryIndex].Value += .01;
                //            await Task.Delay(5);
                //        }
                //        catch { }
                //    }
                //});
                ProgressBarList[StoryIndex].Value += 1;
            }
            catch { }
            //ProgressBarList[StoryIndex].Value += 1;
        }
        void UpdateOuterUser()
        {
            try
            {
                if (StoryFeed == null) return;
                if(StoryFeed.IsHashtag)
                {
                    InnerUserText.Text = Items[StoryIndex].StoryItem.User.UserName;
                    InnerUserText.Visibility = Visibility.Visible;
                }
                else
                {
                    InnerUserText.Text = string.Empty;
                    InnerUserText.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }
        void PlayFeedUser()
        {
            try
            {
                StoryFeed = FeedList[FeedListIndex];
                if (!string.IsNullOrEmpty(StoryFeed.Title))
                {
                    TitleText.Text = StoryFeed.Title;
                    TitleCover.Source = StoryFeed.HighlightCoverMedia.CroppedImage.Uri.GetBitmap();
                    IsHolding = true;
                    ShowTitle();
                }
            }
            catch { }
        }
        void SkipPreviousFeedUser()
        {
            try
            {
                FeedListIndex.PrintDebug();
                if (FeedListIndex - 1 < 0)
                {
                    StopEverything();
                    return;
                }
                IntervalTimer = 0;
                FeedListIndex--;
                PlayFeedUser();
            }
            catch { }
        }
        void SkipNextFeedUser()
        {
            try
            {
                if (FeedListIndex + 1 > FeedList.Count - 1)
                {
                    StopEverything();
                    return;
                }
                IntervalTimer = 0;
                FeedListIndex++;
                PlayFeedUser();
            }
            catch { }
        }
        public void SkipNext()
        {
            try
            {
                if (StoryIndex + 1 > Items.Count - 1)
                {
                    SkipNextFeedUser();
                    return;
                }

                //if (StoryIndex == 0)
                //{
                //    //var seens = FeedList.Select((item,i) => new { Seen = item, Index = i });

                //    int index = -1;
                //    if (Items?.Count > 0)
                //    {
                //        //if (StoryFeed.Seen > 1 && StoryFeed.Items.LastOrDefault()?.TakenAt.Year > 2009)
                //        //{
                //        //    StoryIndex++;
                //        //}
                //        //else

                //        for (int i = 0; i < Items.Count; i++)
                //        {
                //            if (Items[i].StoryItem.TakenAt.ToUnixTime() == StoryFeed.Seen)
                //            {
                //                index = i;
                //                break;
                //            }
                //        }
                //    }

                //    if (index != -1)
                //    {
                //        if (index >= Items.Count - 1)
                //            index = 0;
                //        StoryIndex = index;
                //    }
                //    else
                //        StoryIndex++;
                //    WasPaused = false;
                //    IntervalTimer = 0;
                //    Play();
                //}
                //else
                {
                    WasPaused = false;
                    IntervalTimer = 0;
                    StoryIndex++;

                    if (StoryIndex > 0)
                    {
                        for (int i = 0; i < StoryIndex; i++)
                        {
                            try
                            {
                                ProgressBarList[i].Value = ProgressBarList[i].Maximum;
                            }
                            catch { }
                        }
                    }
                    Play();
                    //else
                    //    Items[StoryIndex].MediaElement.Stop();
                }
            }
            catch { }
        }
        void SkipPrevious()
        {
            try
            {
                if (StoryIndex - 1 < 0)
                {
                    SkipPreviousFeedUser();
                    return;
                }
                WasPaused = false;
                IntervalTimer = 0;
                StoryIndex--;
                if (Items.Count > 0)
                {
                    for (int i = StoryIndex; i < Items.Count; i++)
                    {
                        try
                        {
                            ProgressBarList[i].Value = 0;
                        }
                        catch { }
                    }
                }
                Play();
                //else
                //    Items[StoryIndex].MediaElement.Stop();
            }
            catch { }
        }
        public async void Play()
        {
            try
            {
                ChildGrid.Children.Clear();
                ChildGrid.Children.Add(Items[StoryIndex]);
                await Task.Delay(250);
                DateText.Text = Convert.ToString(new Converters.DateTimeConverter().Convert(Items[StoryIndex].StoryItem.TakenAt, null, null, null));
                if (Items[StoryIndex].StoryItem.MediaType == InstaMediaType.Video)
                {
                    VideoIsPlaying = false;
                    Items[StoryIndex].MediaElement.Source = new Uri(Items[StoryIndex].StoryItem.Videos[0].Uri);
                    try
                    {
                        Items[StoryIndex].MediaElement.MediaOpened -= MediaElementMediaOpened;
                    }
                    catch { }
                    try
                    {
                        Items[StoryIndex].MediaElement.CurrentStateChanged -= MediaElementCurrentStateChanged;
                    }
                    catch { }
                    try
                    {
                        Items[StoryIndex].MediaElement.MediaEnded -= MediaElementMediaEnded;
                    }
                    catch { }
                    Items[StoryIndex].MediaElement.MediaOpened += MediaElementMediaOpened;
                    Items[StoryIndex].MediaElement.CurrentStateChanged += MediaElementCurrentStateChanged;
                    Items[StoryIndex].MediaElement.MediaEnded += MediaElementMediaEnded;
                }
                else VideoIsPlaying = false;

                SeeMoreButton.Visibility = Items[StoryIndex].StoryItem.StoryCTA?.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

                ReplyText.Text = string.Empty;
                if (StoryFeed != null && StoryFeed?.User?.Pk == Helper.CurrentUser?.Pk)
                {
                    var itemX = Items[StoryIndex].StoryItem; 
                    ReplyText.Visibility = ReplyButton.Visibility = ReactionGV.Visibility = Visibility.Collapsed;
                    if ((int)itemX.ViewerCount > 0)
                    {
                        SeenByButton.Content = "Seen by " + (int)itemX.ViewerCount;
                        SeenByButton.Visibility = Visibility.Visible;
                    }
                    else
                        SeenByButton.Visibility = Visibility.Collapsed;
                    var dateNow = DateTime.UtcNow.ToUnixTime();
                    var expiringAt = itemX.ExpiringAt.ToUnixTime();
                    if (dateNow > expiringAt)
                    {
                        SeenByButton.Content = "Viewers" ;

                        SeenByButton.Visibility = Visibility.Visible;
                    }

                }
                else
                {
                    SeenByButton.Visibility = Visibility.Collapsed;
                    ReplyText.Visibility = /*ReactionGV.Visibility =*/ Items[StoryIndex].StoryItem.CanReply ? Visibility.Visible : Visibility.Collapsed;
                }
                UpdateOuterUser();
                SeenStory();
                SetStuff(Items[StoryIndex]);
            }
            catch { }
        }

        private void MediaElementMediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                SkipNext();
            }
            catch { }
        }

        private void MediaElementCurrentStateChanged(object sender, RoutedEventArgs e)
        {

        }

        async void SeenStory()
        {
            try
            {
                if (!SettingsHelper.Settings.GhostMode)
                {
                    try
                    {
                        if (FeedList[FeedListIndex].Seen == Items[StoryIndex].StoryItem.TakenAt.ToUnixTime())
                            return;
                    }
                    catch { return; }
                    //Items[StoryIndex].StoryItem
                    //var seen = await Helper.InstaApi
                    //    .StoryProcessor.MarkStoryAsSeenAsync(Items[StoryIndex].StoryItem.Id, Items[StoryIndex].StoryItem.TakenAt.ToUnixTime());
                    if (FeedList[FeedListIndex].IsElection)
                    {
                        var dic = new List<InstaStoryElectionKeyValue>
                        {
                            //{Items[StoryIndex].StoryItem.Id, Items[StoryIndex].StoryItem.TakenAt.ToUnixTime()}
                           new InstaStoryElectionKeyValue
                           {
                               StoryId = FeedList[FeedListIndex].Id,
                               StoryItemId = Items[StoryIndex].StoryItem.Id,
                               TakenAtUnix = Items[StoryIndex].StoryItem.TakenAt.ToUnixTime().ToString()
                           }
                        };
                        //foreach (var item in Items)
                        //    dic.Add(item.Value.StoryItem.Id, item.Value.StoryItem.TakenAt.ToUnixTime());

                        var XXX = FeedList[FeedListIndex];
                        var YYY = Items[StoryIndex];
                        var seen = await Helper.InstaApi
                            .StoryProcessor.MarkMultipleStoriesAsSeen2Async(dic);
                        XXX.Seen = YYY.StoryItem.TakenAt.ToUnixTime();

                        MainView.Current?.SetSeens(XXX.Id, YYY.StoryItem.TakenAt.ToUnixTime());
                    }
                    else
                    {

                        var dic = new Dictionary<string, long>
                        {
                            {Items[StoryIndex].StoryItem.Id, Items[StoryIndex].StoryItem.TakenAt.ToUnixTime()}
                        };
                        //var aaab = Items[StoryIndex];
                        //foreach (var item in Items)
                        //    dic.Add(item.Value.StoryItem.Id, item.Value.StoryItem.TakenAt.ToUnixTime());

                        var XXX = FeedList[FeedListIndex];
                        var YYY = Items[StoryIndex];
                        var seen = await Helper.InstaApi
                            .StoryProcessor.MarkMultipleStoriesAsSeenAsync(dic);
                        XXX.Seen = YYY.StoryItem.TakenAt.ToUnixTime();

                        MainView.Current?.SetSeens(XXX.Id, YYY.StoryItem.TakenAt.ToUnixTime());
                    }
                }
            }
            catch { }
        }
        MediaElement LatestMediaElement = null;
        bool VideoIsPlaying = false;
        private void MediaElementMediaOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                LatestMediaElement = (sender as MediaElement);
                LatestMediaElement.Play();
                VideoIsPlaying = true;
                SetStuff();
            }
            catch { }
        }

        public void StopEverything()
        {
            try
            {
                DateText.Text = "";
                WasPaused = false;
                LatestMediaElement?.Stop();
                StopTimer();
                IntervalTimer = 0;
                //DataContext = null;
                StoryFeed = null;
                if (NavigationService.Frame.CanGoBack)
                {
                    //if (NavigationService.Frame.BackStack[NavigationService.Frame.BackStack.Count - 1]
                    //    .SourcePageType.Name.Contains("MainView"))
                    //    try
                    //    {
                    //        if (NavigationService.Frame.Content is MainView view && view != null)
                    //            view.SetSeens();
                    //    }
                    //    catch { }
                    NavigationService.GoBack();
                }
            }
            catch { }
        }
        private void LeftGridTapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsHolding) return;
            SkipPrevious();
        }

        private void RightGridTapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsHolding) return;
            SkipNext();
        }



        private async void ShareButtonClick(object sender, RoutedEventArgs e)
        {
            if (MainStoryViewerUc.Visibility == Visibility.Visible) return;
            try
            {
                IsHolding = true;
                await new UsersDialog(Items[StoryIndex].StoryItem, StoryFeed).ShowAsync();
            }
            catch { }
            IsHolding = false;
            try
            {
                if (Items[StoryIndex]?.StoryItem.MediaType == InstaMediaType.Video)
                {
                    Items[StoryIndex].MediaElement.Play();
                }
            }
            catch { }
        }









        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged2(string memberName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }

        #region Control generators
        DropShadowPanel GenerateShadowPanel()
        {
            return new DropShadowPanel
            {
                Color = Colors.Black,
                OffsetX = 5.0,
                OffsetY = 5.0,
                ShadowOpacity = 0.80,
                BlurRadius = 15.0
            };
        }
        StoryFeedItemUc GenerateFeedItem(InstaStoryItem item)
        {
            return new StoryFeedItemUc
            {
                StoryItem = item,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                IsHoldingEnabled = true
            };
        }
        ColumnDefinition GenerateColumn()
        {
            return new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
        }

        ProgressBar GenerateProgress(Thickness? margin = null)
        {
            if (margin == null)
                margin = new Thickness(2.5, 5, 2.5, 5);
            //<ProgressBar Value="10" Grid.Column="1" Height="2" Margin="5" Background="#41FFFFFF" Foreground="#B5C3C3C3" />
            var p = new ProgressBar
            {
                Height = 1.6,
                Margin = margin.Value,
                Background = /*"#41FFFFFF"*/"#74FFFFFF".GetColorBrush(),
                Foreground = new SolidColorBrush(Colors.White),
                LargeChange = .01,
                SmallChange = .01
                //Foreground = "#B5C3C3C3".GetColorBrush()
            };
            try
            {
                if (DeviceUtil.OverRS2OS)
                    p.CornerRadius = new CornerRadius(0.8);
            }
            catch { }
            return p;
        }


        #endregion Control generators


        private async void ReplyButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        //Items[StoryIndex].StoryItem, StoryFeed
                        var type = InstaSharingType.Photo;
                        if (Items[StoryIndex].StoryItem.MediaType == InstaMediaType.Video)
                            type = InstaSharingType.Video;
                        IsHolding = false;
                        var reply = await Helper.InstaApi.StoryProcessor.ReplyToStoryAsync(Items[StoryIndex].StoryItem.Id, Items[StoryIndex].StoryItem.User.Pk,
                            ReplyText.Text, type);

                        if (reply.Succeeded)
                        {
                            ReplyText.Text = string.Empty;
                            Helper.ShowNotify($"Reply sent.");
                        }

                        if (Items[StoryIndex]?.StoryItem.MediaType == InstaMediaType.Video)
                        {
                            IsHolding = false;
                            Items[StoryIndex].MediaElement.Play();
                        }
                    }
                    catch { }
                });
             
            }
            catch { }
        }
        private void ReplyTextGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                IsHolding = true;
                ReactionGV.Visibility = Visibility.Visible;
            }
            catch { }
        }

        private void ReplyTextLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                IsHolding = false;
                ReactionGV.Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        #region Animate 

        void ShowTitle()
        {
            try
            {
                IsHolding = true;
                //var story = new Storyboard();
                //var da = new DoubleAnimation
                //{
                //    To = 1,
                //    Duration = TimeSpan.FromSeconds(1)                  
                //};
                TitleGrid.Visibility = Visibility.Visible;
                ShowTitleStoryboard.Begin();
                //Storyboard.SetTargetName(da, "TitleGrid");
                //Storyboard.SetTargetProperty(da, "Opacity");
                //story.Children.Add(da);
                //story.Completed += ShowTitleCompleted;
                //story.Begin();
            }
            catch { }
        }

        private async void ShowTitleCompleted(object sender, object e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await Task.Delay(2000);
                    HideTitle();
                });
            }
            catch { }
        }
        void HideTitle()
        {
            try
            {
                HideTitleStoryboard.Begin();
                //var story = new Storyboard();
                //var da = new DoubleAnimation
                //{
                //    To = 0,
                //    Duration = TimeSpan.FromMilliseconds(650)
                //};
                //Storyboard.SetTargetName(da, "TitleGrid");
                //Storyboard.SetTargetProperty(da, "Opacity");
                //story.Children.Add(da);
                //story.Completed += HideTitleCompleted;
                //story.Begin();
            }
            catch { }
        }

        private void HideTitleCompleted(object sender, object e)
        {
            IsHolding = false;
            try
            {
                TitleGrid.Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        void ShowHoldingPanels()
        {
            try
            {
                ProgressGrid.Visibility = UserGrid.Visibility = BottomStuffGrid.Visibility = Visibility.Visible;
                ShowHoldingStoryboard.Begin();
            }
            catch { }
        }
        void HideHoldingPanels()
        {
            try
            {
                HideHoldingStoryboard.Begin();
            }
            catch { }
        }

        private void HideHoldingStoryboardCompleted(object sender, object e)
        {
            try
            {
                ProgressGrid.Visibility = UserGrid.Visibility = BottomStuffGrid.Visibility = Visibility.Collapsed;
            }
            catch { }
        }
        #endregion Animate


        #region Reaction
        private async void ReactionGVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is string str && !string.IsNullOrEmpty(str))
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        try
                        {
                            var user = Items[StoryIndex].StoryItem.User.UserName;
                            ShowReaction(str);
                            var result = await Helper.InstaApi.StoryProcessor
                            .SendReactionToStoryAsync(Items[StoryIndex].StoryItem.User.Pk, Items[StoryIndex].StoryItem.Id, str);
                            if (result.Succeeded)
                                Helper.ShowNotify($"Reaction sent to {user}");
                        }
                        catch { }
                    });
                }
            }
            catch { }
        }
        void ShowReaction(string emoji)
        {
            try
            {
                ReactionGrid.Children.Clear();
                var rnd = new Random();
                //ReactionGrid.Height = rnd.Next(150, 200);
                ReactionCompositeTransform.TranslateY = 0;
                ReactionGrid.RowDefinitions.Clear();
                ReactionGrid.ColumnDefinitions.Clear();
                ReactionGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                ReactionGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                ReactionGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                for (int i = 0; i < 9; i++)
                {
                    var text = new TextBlock
                    {
                        Text = emoji,
                        FontSize = rnd.Next(15, 20),
                        Margin = new Thickness(rnd.Next(5, 15), rnd.Next(5, 15), rnd.Next(7, 13), rnd.Next(7, 15))
                    };
                    ReactionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                    Grid.SetColumn(text, (ReactionGrid.ColumnDefinitions.Count - 1));
                    ReactionGrid.Children.Add(text);
                }
                for (int i = 0; i < 9; i++)
                {
                    var text = new TextBlock
                    {
                        Text = emoji,
                        FontSize = rnd.Next(18, 28),
                        Margin = new Thickness(rnd.Next(5, 9), rnd.Next(10, 18), rnd.Next(5, 14), rnd.Next(8, 16))
                    };
                    Grid.SetColumn(text, i);
                    Grid.SetRow(text, 1);
                    ReactionGrid.Children.Add(text);
                }
                for (int i = 0; i < 9; i++)
                {
                    var text = new TextBlock
                    {
                        Text = emoji,
                        FontSize = rnd.Next(18, 28),
                        Margin = new Thickness(rnd.Next(6, 10), rnd.Next(10, 18), rnd.Next(6, 12), rnd.Next(10, 18))
                    };
                    Grid.SetColumn(text, i);
                    Grid.SetRow(text, 2);
                    ReactionGrid.Children.Add(text);
                }
                ReactionGrid.Visibility = Visibility.Visible;
                ShowReactionStoryboard.Begin();
                //CreateStoryBoardAnimation();
            }
            catch { }
        }
        private async void ShowReactionStoryboardCompleted(object sender, object e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await Task.Delay(750);
                    try
                    {
                        ReactionGrid.Visibility = Visibility.Collapsed;
                        ReactionCompositeTransform.TranslateY = 150;
                    }
                    catch { }
                });
            }
            catch { }
        }
        #endregion Raction

        public void SetStuff() => SetStuff(Items[StoryIndex]);
        void SetStuff(StoryFeedItemUc storyUc)
        {
            try
            {
                IsStoryInnerShowing = false;
                StorySuffItems.Children.Clear();
                StorySuffItems.Visibility = Visibility.Visible;
               var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
                var scaleFactor = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                var actwidth = storyUc.MediaElement.ActualWidth == 0 ? storyUc.Image.ActualWidth : storyUc.MediaElement.ActualWidth;
                var actheight = storyUc.MediaElement.ActualHeight == 0 ? storyUc.Image.ActualHeight : storyUc.MediaElement.ActualHeight;
                //var actwidth = storyUc.Image.ActualWidth;
                //var actheight = storyUc.Image.ActualHeight;
                var size = AspectRatioHelper.CalculateSizeInBox(storyUc.StoryItem.OriginalWidth, storyUc.StoryItem.OriginalHeight, actheight, actwidth);
                //var size = new Size(actwidth, actheight);
                StorySuffItems.Width = size.Width;
                StorySuffItems.Height = size.Height;

                if (storyUc.StoryItem.StoryFeedMedia?.Count > 0)
                {
                    foreach (var item in storyUc.StoryItem.StoryFeedMedia)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);

                        var rndName = 8.GenerateRandomStringStatic();
                        var rect = new Rectangle()
                        {
                            Fill = new SolidColorBrush(Colors.Black),
                            Opacity = 0,
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            DataContext = item.MediaId.ToString(),
                            RenderTransform = trans,
                            Width = item.Width * bounds.Width,
                            Height = item.Height * bounds.Height,
                            Name = "StoryFeedMedia"+ rndName
                        };

                        rect.Tapped += ShowPanel;
                        StorySuffItems.Children.Add(rect);
                    }
                }


                if (storyUc.StoryItem.ReelMentions?.Count > 0)
                {
                    foreach (var item in storyUc.StoryItem.ReelMentions)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var rect = new Rectangle()
                        {
                            Fill = new SolidColorBrush(Colors.Black),
                            Opacity = 0,
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            DataContext = item.User.UserName.ToLower(),
                            RenderTransform = trans,
                            Width = item.Width * bounds.Width,
                            Height = item.Height * bounds.Height,
                            Name = "UserMention" + rndName,
                            Tag = item.User.FullName
                        };
                        rect.Tapped += ShowPanel;
                        StorySuffItems.Children.Add(rect);
                    }
                }


                if (storyUc.StoryItem.StoryHashtags?.Count > 0)
                {
                    foreach (var item in storyUc.StoryItem.StoryHashtags)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var rect = new Rectangle()
                        {
                            Fill = new SolidColorBrush(Colors.Black),
                            Opacity = 0,
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            DataContext = item.Hashtag.Name.ToLower(),
                            RenderTransform = trans,
                            Width = item.Width * bounds.Width,
                            Height = item.Height * bounds.Height,
                            Name = "Hashtag" + rndName
                        };
                        rect.Tapped += ShowPanel;
                        try
                        {
                        }
                        catch { }
                        StorySuffItems.Children.Add(rect);
                    }
                }

                // NOT COMPLETE
                if (storyUc.StoryItem.StoryLocations?.Count > 0)
                {
                    foreach (var item in storyUc.StoryItem.StoryLocations)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var rect = new Rectangle()
                        {
                            Fill = new SolidColorBrush(Colors.Black),
                            Opacity = 0,
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            DataContext = item.Location.Name.ToLower(),
                            RenderTransform = trans,
                            Width = item.Width * bounds.Width,
                            Height = item.Height * bounds.Height,
                            Name = "Location" + rndName
                        };
                        rect.Tapped += ShowPanel;
                        StorySuffItems.Children.Add(rect);
                    }
                }

                // COMPLETED
                if (storyUc.StoryItem.StoryPolls?.Count > 0)
                {
                    foreach (var item in storyUc.StoryItem.StoryPolls)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var storyPoll = new StoryPollUc()
                        {
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            //DataContext = item.Location.Name.ToLower(),
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "Poll" + rndName
                        };
                        storyPoll.SetItem(item, storyUc.StoryItem);
                        StorySuffItems.Children.Add(storyPoll);
                    }
                }

                // COMPLETED
                if (storyUc.StoryItem.StoryQuestions?.Count > 0)
                {
                    foreach (var item in storyUc.StoryItem.StoryQuestions)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var storyQuestionControl = new StoryQuestionControl()
                        {
                            Background = "#A5EEF900".GetColorBrush(),
                            Opacity = 0,
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            //DataContext = item.Location.Name.ToLower(),
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "Questions" + rndName,
                            StoryId = storyUc.StoryItem.Id,
                            StoryQuestionItem = item
                        };
                        storyQuestionControl.Tapped += StoryQuestionControlTapped;
                        StorySuffItems.Children.Add(storyQuestionControl);
                    }
                }

                // COMPLETED
                if (storyUc.StoryItem.StoryQuizs?.Count > 0)
                {
                    foreach (var item in storyUc.StoryItem.StoryQuizs)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var storyQuiz = new StoryQuizUc()
                        {
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            //DataContext = item.Location.Name.ToLower(),
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "Quizs" + rndName
                        };
                        storyQuiz.SetQuiz(item, storyUc.StoryItem);
                        StorySuffItems.Children.Add(storyQuiz);
                    }
                }

                // COMPLETED
                if (storyUc.StoryItem.StorySliders?.Count > 0)
                {
                    foreach (var item in storyUc.StoryItem.StorySliders)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var storySliderUc = new StorySliderUc()
                        {
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            //DataContext = item.User.UserName.ToLower(),
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "StorySliders" + rndName,
                        };
                        storySliderUc.SetItem(item, storyUc.StoryItem);
                        StorySuffItems.Children.Add(storySliderUc);
                    }
                }
                
                // COMPLETED
                if (storyUc.StoryItem.Countdowns?.Count > 0)
                {
                    foreach (var item in storyUc.StoryItem.Countdowns)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var storyCountdown = new StoryCountdownUc()
                        {
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "Countdowns" + rndName
                        };
                        storyCountdown.SetItem(item, storyUc.StoryItem);
                        StorySuffItems.Children.Add(storyCountdown);
                    }
                }

                if (StorySuffItems.Children.Any())
                {
                    foreach (var xItem in StorySuffItems.Children)
                    {
                        var item = xItem as Rectangle;
                        if (item != null)
                        {
                            try
                            {
                                var randomName = 8.GenerateRandomStringStatic();
                                var name = "";
                                var innerType = StoryInnerUc.StoryInnerItem.Hashtag;
                                var title = "";
                                if (item.Name.Contains("StoryFeedMedia"))
                                {
                                    name = "MediaFeed" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.MediaFeed;
                                    title = "See Post";
                                }
                                else if (item.Name.Contains("UserMention"))
                                {
                                    name = "UserMention" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.UserMention;
                                    title = item.Tag.ToString();
                                }
                                else if (item.Name.Contains("Hashtag"))
                                {
                                    name = "Hashtag" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.Hashtag;
                                    title = "See Hashtag";
                                }
                                else if (item.Name.Contains("Location"))
                                {
                                    name = "Location" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.Location;
                                    title = "See Location";
                                }

                                else if (item.Name.Contains("Poll"))
                                {
                                    name = "Poll" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.Hashtag;
                                    title = "See Poll";
                                }
                                else if (item.Name.Contains("Questions"))
                                {
                                    name = "Questions" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.Hashtag;
                                    title = "See Questions";
                                }
                                else if (item.Name.Contains("Quizs"))
                                {
                                    name = "Quizs" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.Hashtag;
                                    title = "See Quizs";
                                }
                                item.Tag = name;
                                //                  var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                                //((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                                var margin = new Thickness
                                {
                                    Bottom = item.Margin.Bottom,
                                    Left = item.Margin.Left + 20,
                                    Right = item.Margin.Right,
                                    Top = item.Margin.Top - 25
                                };
                                var innerUc = new StoryInnerUc(innerType, title, item.DataContext.ToString())
                                {
                                    Visibility = Visibility.Visible,
                                    Opacity = 0,
                                    VerticalAlignment = VerticalAlignment.Top,
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    Margin = margin,
                                    RenderTransform = item.RenderTransform,
                                    Name = name
                                };
                                StorySuffItems.Children.Add(innerUc);


                                innerUc.Visibility = Visibility.Collapsed;
                            }
                            catch { }
                        }
                    }
                }
            }
            catch(Exception ex) { ex.PrintException("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"); }
        }

        private async void StoryQuestionControlTapped(object sender, TappedRoutedEventArgs e)
        {
            IsHolding = true;
            try
            {
                if (sender is StoryQuestionControl storyQuestion && storyQuestion != null)
                    if (storyQuestion.StoryQuestionItem.QuestionSticker.ViewerCanInteract)
                        await new StoryQuestionDialog(storyQuestion).ShowAsync();
            }
            catch { }
            IsHolding = false;
        }

        private void Rect_Tapped(object sender, TappedRoutedEventArgs e)
        {
            "Rect_Tapped".PrintDebug();
        }

        bool IsStoryInnerShowing = false;
        private async void ShowPanel(object sender, TappedRoutedEventArgs e)
        {
            if (sender is Rectangle rect /*&& rect.DataContext is string str && !string.IsNullOrEmpty(str)*/)
            {
                try
                {
                    //var hasAnyPanelShown = false;
                    if (StorySuffItems.Children.Count > 0)
                    {
                        for (int i = 0; i < StorySuffItems.Children.Count; i++)
                        {
                            var item = StorySuffItems.Children[i] as StoryInnerUc;
                            if (item != null)
                            {
                                if (item.Name == rect.Tag.ToString())
                                {
                                    IsHolding = true;
                                    IsStoryInnerShowing = true;
                                    //if (item.Visibility == Visibility.Visible)
                                    //{
                                    //    await item.Animation(FrameworkLayer.Xaml)
                                    //            .Scale(1, 1.2, Easing.QuadraticEaseInOut)
                                    //            .Duration(80)
                                    //            .StartAsync();


                                    //    await item.Animation(FrameworkLayer.Xaml)
                                    //          .Opacity(1, 0, Easing.CircleEaseOut)
                                    //          .Scale(1.2, 0, Easing.QuadraticEaseInOut)
                                    //          .Duration(250)
                                    //          .StartAsync();
                                    //    item.Visibility = Visibility.Collapsed;
                                    //}
                                    //else
                                    {
                                        await item.Animation(FrameworkLayer.Xaml)
                                              .Scale(1, 0, Easing.QuadraticEaseInOut)
                                              .Duration(0)
                                              //.Delay(250)
                                              .StartAsync();
                                        item.Visibility = Visibility.Visible;

                                        await item.Animation(FrameworkLayer.Xaml)
                                              .Opacity(0, 1, Easing.CircleEaseOut)
                                              .Scale(0, 1.2, Easing.QuadraticEaseInOut)
                                              .Duration(250)
                                              .StartAsync();


                                        await item.Animation(FrameworkLayer.Xaml)
                                                .Scale(1.2, 1, Easing.QuadraticEaseInOut)
                                                .Duration(80)
                                                .StartAsync();
                                    }
                                    //item.Opacity = 1;
                                    //item.Visibility = Visibility.Visible;
                                    break;
                                }
                                //hasAnyPanelShown = item.Visibility == Visibility.Visible;
                            }
                        }
                        //if(!hasAnyPanelShown)
                        //{

                        //    IsHolding = false;
                        //    IsStoryInnerShowing = false;
                        //}
                    }
                }
                catch { }
            }
        }

        void HideStoryInnerPanels()
        {
            try
            {
                if (IsStoryInnerShowing)
                {
                    if (StorySuffItems.Children.Count > 0)
                    {
                        for (int i = 0; i < StorySuffItems.Children.Count; i++)
                        {
                            if (StorySuffItems.Children[i] is StoryInnerUc item)
                            {
                                item.Visibility = Visibility.Collapsed;
                                item.Opacity = 0;
                            }
                        }
                        IsHolding = false;
                        IsStoryInnerShowing = false;
                    }
                }
            }
            catch { }

        }

        private void MainGridTapped(object sender, TappedRoutedEventArgs e)
        {
            //HideStoryInnerPanels();
        }

        private void MainGridKTapped(object sender, TappedRoutedEventArgs e)
        {
            HideStoryInnerPanels();
        }

        private void SeeMoreButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Items[StoryIndex] != null &&
                    Items[StoryIndex].StoryItem != null &&
                    Items[StoryIndex].StoryItem.StoryCTA?.Count > 0)
                {
                    if (Items[StoryIndex].StoryItem.StoryCTA.FirstOrDefault().WebUri.Contains("instagram.com/"))
                        UriHelper.HandleUri(Items[StoryIndex].StoryItem.StoryCTA.FirstOrDefault().WebUri);
                    else
                        Items[StoryIndex].StoryItem.StoryCTA.FirstOrDefault().WebUri.OpenUrl();
                }
            }
            catch { }
        }
        //private void CopyUrlButtonClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var url = ExtensionHelper.GetUrl(StoryFeed.User.UserName.ToLower(), Items[StoryIndex].StoryItem.Pk);
        //        url.CopyText();
        //        Helper.ShowNotify("Url copied ;-)");
        //    }
        //    catch { }
        //}

        private void DownloadButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = Items[StoryIndex].StoryItem;
                if (item.MediaType == InstaMediaType.Image)
                {
                    var url = item.Images.FirstOrDefault().Uri;
                    DownloadHelper.Download(url, item.Images.LastOrDefault().Uri, false, item.User.UserName, null, true, true);
                }
                else
                {
                    var url = item.Videos.FirstOrDefault().Uri;
                    DownloadHelper.Download(url, item.Images.LastOrDefault().Uri, true, item.User.UserName, null, true, true);
                }
            }
            catch { }
        }
        private async void MoreOptionsButtonClick(object sender, RoutedEventArgs e)
        {
            IsHolding = true;
            try
            {
                await new StoryMenuDialog(StoryFeed, Items[StoryIndex].StoryItem, this).ShowAsync();
            }
            catch { }
            IsHolding = false;

        }

        private void ReplyTextTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if(string.IsNullOrEmpty(ReplyText.Text))
                {
                    ReplyButton.Visibility = Visibility.Collapsed;
                    ShareButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ReplyButton.Visibility = Visibility.Visible;
                    ShareButton.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        private void SeenByButtonClick(object sender, RoutedEventArgs e)
        {
            if (Items.Count > StoryIndex)
            {
                IsHolding = true;
                StorySuffItems.Visibility = Visibility.Collapsed;
                MainStoryViewerUc.SetStoryItem(Items[StoryIndex].StoryItem);
            }
        }
    }
}
