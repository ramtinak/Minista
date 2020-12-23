using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Helpers;
using Minista.Helpers;
using Minista.Models.Main;
using Minista.Views.Stories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace Minista.Views.Main
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page
    {
        private readonly Compositor _compositor;
        private readonly Visual _goUpButtonVisual;
        ScrollViewer Scroll;
        static bool IsPageLoaded = false;
        //ListView LatestStoriesLV = null;

        public static MainView Current;
        async void OPEN()
        {
            try
            {
                Helper.DontUseTimersAndOtherStuff = true;
                FileOpenPicker openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".bmp");
                //openPicker.FileTypeFilter.Add(".gif");
                openPicker.FileTypeFilter.Add(".png");
                openPicker.FileTypeFilter.Add(".mp4");
                //openPicker.FileTypeFilter.Add(".mkv");
                var files = await openPicker.PickMultipleFilesAsync();
                Helper.DontUseTimersAndOtherStuff = false;
                Helper.ShowMsg("FILES SUCCESSFULLY OPEN");
            }
            catch (Exception ex)
            {
                ex.PrintException("ImportButtonClick");
                Helper.ShowErr("ImportButtonClick", ex);
                Helper.DontUseTimersAndOtherStuff = false;
            }
        }
        public MainView()
        {
            this.InitializeComponent();
            Current = this;
            Loaded += MainView_Loaded;
            this.NavigationCacheMode = NavigationCacheMode.Required;
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _goUpButtonVisual = GoUpButton.GetVisual();
            SetUpPageAnimation();
            //Window.Current.SizeChanged += Current_SizeChanged;
            
        }

        public static /*async*/ void OnLogReceived(object sender, object e)
        {
            //try
            //{
            //    if (Current != null)
            //        await Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //        {
            //            Current.TXT.Text += e.ToString() + Environment.NewLine;
            //        });
            //}
            //catch { }
        }

        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsPageLoaded)
                return;
            var apis = Helper.InstaApiList.ToList();
            if (Helper.InstaApi != null)
                apis.AddInstaApiX(Helper.InstaApi);
            Helpers.MultiApiHelper.SetupPushNotification(apis);
            IsPageLoaded = true;
            ToggleGoUpButtonAnimation(false);
            if (Scroll == null)
            {
                Scroll = LVPosts.FindScrollViewer();
                if (Scroll != null)
                    Scroll.ViewChanging += ScrollViewViewChanging;
                MainVM.PostsGenerator.SetLV(Scroll);
                TryToRefresh(true);
            }

            try
            {
                RefreshControl.RefreshRequested -= RefreshControlRefreshRequested;
                RefreshControl.Visualizer.RefreshStateChanged -= RefreshControlRefreshStateChanged;
            }
            catch { }
            RefreshControl.RefreshRequested += RefreshControlRefreshRequested;
            if (RefreshControl.Visualizer != null)
                RefreshControl.Visualizer.RefreshStateChanged += RefreshControlRefreshStateChanged;
            InitTheme();
        }
        async void InitTheme()
        {
            try
            {
                await Task.Delay(10000);
                ThemeHelper.InitTheme(SettingsHelper.Settings.CurrentTheme);
            }
            catch { }
        }

        private void RefreshControlRefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            using (var RefreshCompletionDeferral = args.GetDeferral())
                MainVM.FirstRun(true);
        }
        private void RefreshControlRefreshStateChanged(Microsoft.UI.Xaml.Controls.RefreshVisualizer sender, Microsoft.UI.Xaml.Controls.RefreshStateChangedEventArgs args)
        {
            if (args.NewState == Microsoft.UI.Xaml.Controls.RefreshVisualizerState.Refreshing)
            {
                RefreshButton.IsEnabled = false;
            }
            else
            {
                RefreshButton.IsEnabled = true;
            }
        }
        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            RefreshControl.RequestRefresh();
        }

        public async void TryToRefresh(bool all = false)
        {
            if (all) 
            {
                await UserHelper.GetBanyanAsync();
                UserHelper.GetSelfUser();
            }
            MainVM.FirstRun(true);
        }
        private void LVPostsRefreshRequested(object sender, EventArgs e)
        {
            TryToRefresh(false);
        }

        private async void FollowUnfollowButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn != null && btn.DataContext is InstaSuggestionItem user)
                {
                    //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    //{
                        if (user.FollowText == "Follow")
                        {
                            var result = await Helper.InstaApi.UserProcessor.FollowUserAsync(user.User.Pk);
                            if (result.Succeeded)
                            {
                                if (result.Value.OutgoingRequest)
                                    user.FollowText = "Requested";
                                else if (result.Value.Following)
                                    user.FollowText = "Unfollow";
                            }
                        }
                        else if (user.FollowText == "Unfollow")
                        {
                            var result = await Helper.InstaApi.UserProcessor.UnFollowUserAsync(user.User.Pk);
                            if (result.Succeeded)
                                user.FollowText = "Follow";
                        }
                        btn.DataContext = user;
                    //});
                  
                }
            }
            catch { }
        }












        private double _lastVerticalOffset;
        private bool _isHideTitleGrid;





        private void ScrollViewViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                var scrollViewer = sender as ScrollViewer;

                if ((scrollViewer.VerticalOffset - _lastVerticalOffset) > 5 && !_isHideTitleGrid)
                {
                    _isHideTitleGrid = true;
                    ToggleGoUpButtonAnimation(false);
                }
                else if (scrollViewer.VerticalOffset < _lastVerticalOffset && _isHideTitleGrid)
                {
                    _isHideTitleGrid = false;
                    ToggleGoUpButtonAnimation(true);
                }
                if(scrollViewer.VerticalOffset >= 0 && scrollViewer.VerticalOffset <= 1)
                {
                    _isHideTitleGrid = true;
                    ToggleGoUpButtonAnimation(false);
                }
                _lastVerticalOffset = scrollViewer.VerticalOffset;
            }
            catch { }
        }
        private void ToggleGoUpButtonAnimation(bool show)
        {

            var scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(1f, show ? 1f : 0);
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);

            _goUpButtonVisual.CenterPoint = new Vector3((float)GoUpButton.ActualWidth / 2f, (float)GoUpButton.ActualHeight / 2f, 0f);
            _goUpButtonVisual.StartAnimation("Scale.X", scaleAnimation);
            _goUpButtonVisual.StartAnimation("Scale.Y", scaleAnimation);
        }

        private void GoUpButtonClick(object sender, RoutedEventArgs e)
        {
            Scroll.ScrollToElement(0);
        }
        public UserControls.Main.MediaMainUc MediaMainUc;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            KeyDown += OnKeyDownHandler;
            if (e.NavigationMode == NavigationMode.New || e.NavigationMode == NavigationMode.Refresh)
            {
                if (Helper.UserChanged)
                    try
                    {
                        MainVM.Stories.Clear();
                        MainVM.PostsGenerator.Items.Clear();
                        if (NavigationService.Frame.BackStack.Count > 0)
                            NavigationService.Frame.BackStack.Clear();
                        Helper.UserChanged = false;
                        MainPage.Current?.HideBackButton();
                        MainVM.FirstRun(true);
                    }
                    catch { }
            }
            try
            {
                try
                {
                    if (Helpers.NavigationService.Frame.BackStack.Count > 0)
                        Helpers.NavigationService.Frame.BackStack.Clear();
                }
                catch { }
                if (MediaMainUc == null)
                    return;
                ConnectedAnimation userPictureUserDetailsView = ConnectedAnimationService.GetForCurrentView().GetAnimation("UserPictureUserDetailsView");
                if (userPictureUserDetailsView != null)
                {

                    userPictureUserDetailsView.Completed += ImageAnimationClose_Completed;
                    //if (LatestGrid?.Name != InfoGrid.Name)
                    //    LatestGrid.Background = new SolidColorBrush(Helper.GetColorFromHex("#FF2E2E2E"));

                    userPictureUserDetailsView.TryStart(MediaMainUc.UserImage);
                    //GridShadow.Visibility = GVSHOW.Visibility = Visibility.Collapsed;
                }
                
            }
            catch { }
            //SetSeens();
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            KeyDown -= OnKeyDownHandler;
        }

        private void OnKeyDownHandler(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.F5)
                    MainVM.FirstRun(true);
            }
            catch { }
        }

        private async void ImageAnimationClose_Completed(ConnectedAnimation sender, object args)
        {
            try
            {
                //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                //{
                    await Task.Delay(100);
                    MediaMainUc = null;
                //});
            }
            catch { }
        }

        private void LVStoriesItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (sender is ListView listView && listView != null)
                {
                    if (e.ClickedItem is StoryWithLiveSupportModel storyWithLiveSupportModel && storyWithLiveSupportModel != null)
                    {
                        if (storyWithLiveSupportModel.Type == StoryType.Story)
                        {
                            var list = new List<InstaReelFeed>();
                            for (int i = 0; i < MainVM.StoriesX.Count; i++)
                            {
                                var item = MainVM.StoriesX[i];
                                if (item.Type == StoryType.Story)
                                    list.Add(item.Story.ToReelFeed());
                            }
                            var first = list.FirstOrDefault(x => x.Id == storyWithLiveSupportModel.Story.Id);
                            int index = 0;
                            if (first != null)
                                index = list.IndexOf(first);
                            else
                                index = LVStories.Items.IndexOf(storyWithLiveSupportModel);
                            NavigationService.Navigate(SettingsHelper.GetStoryView(), new object[] { list, index });
                        }
                        else if (storyWithLiveSupportModel.Type == StoryType.Broadcast)
                        {
                            Helper.OpenLive(storyWithLiveSupportModel.Broadcast);
                        }
                    }
                    // old code
                    //if (e.ClickedItem is InstaReelFeed reelFeed && reelFeed != null)
                    //{
                    //    //LatestStoriesLV = listView;
                    //    var index = LVStories.Items.IndexOf(reelFeed);
                    //    var list = new List<InstaReelFeed>();
                    //    foreach (var item in MainVM.Stories)
                    //        list.Add(item.ToReelFeed());

                    //    Helpers.NavigationService.Navigate(typeof(StoryView), new object[] { list, index });
                    //    //StoryMain.StoryFeed = reelFeed;
                    //    //StoryMain.Visibility = Visibility.Visible;
                    //    //StoryMain.StartTimer();
                    //    //try
                    //    //{
                    //    //    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, /*async*/ () =>
                    //    //    {
                    //    //        //await Task.Delay(7000);
                    //    //        //StoryMain.Visibility = Visibility.Collapsed;
                    //    //    });
                    //    //}
                    //    //catch { }
                    //}
                }
            }
            catch { }
        }
        ListView LatestInnerStoriesLV = null;
        private void InnerStoriesItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (sender is ListView listView && listView != null)
                {
                    LatestInnerStoriesLV = listView;
                    //if (e.ClickedItem is InstaReelFeed reelFeed && reelFeed != null)
                    //{
                    //    //LatestStoriesLV = listView;
                    //    var index = listView.Items.IndexOf(reelFeed);
                    //    var list = listView.ItemsSource as ObservableCollection<InstaReelFeed>;

                    //    Helpers.NavigationService.Navigate(typeof(StoryView), new object[] { list.ToList(), index });
                    //}
                    "InnerStoriesItemClick".PrintDebug();
                }
            }
            catch { }
        }

        private void InnerStoriesItemGridTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (LatestInnerStoriesLV != null)
                {
                    if (sender is Grid grid && grid.DataContext is InstaReelFeed reelFeed && reelFeed != null)
                    {
                        //LatestStoriesLV = listView;
                        var index = LatestInnerStoriesLV.Items.IndexOf(reelFeed);
                        var list = LatestInnerStoriesLV.ItemsSource as ObservableCollection<InstaReelFeed>;

                        NavigationService.Navigate(SettingsHelper.GetStoryView(), new object[] { list.ToList(), index });
                    }
                }
                "InnerStoriesItemGridTapped".PrintDebug();
            }
            catch { }
        }

        private void InnerStoriesUserEllipseTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Ellipse elp && elp != null && elp.DataContext is InstaReelFeed reelFeed && reelFeed != null)
                    Helper.OpenProfile(reelFeed.User);
            }
            catch { }
        }

        public void SetSeens(string storyId,long seen)
        {
            try
            {
                //if (LatestStoriesLV == null)
                //    return;
                //if (StoryView.FeedListStatic == null)
                //    return;
                //if (StoryView.FeedListStatic.Count == 0)
                //    return;

                //if (LatestStoriesLV.ItemsSource is ObservableCollection<InstaReelFeed> source)
                //{
                //    //foreach (var item in StoryView.FeedListStatic)
                //    {
                //        try
                //        {
                //            var reel = source.Single(x => x.Id == storyId);
                //            reel.Seen = seen;
                //        }
                //        catch { }
                //    }
                //}
                //else 
                if (MainVM.Stories?.Count > 0)
                {
                    //foreach (var item in StoryView.FeedListStatic)
                    {
                        try
                        {
                            var reel = MainVM.Stories.Single(x => x.Id == storyId);

                            if (reel != null)
                            {
                                var index = MainVM.Stories.IndexOf(reel);
                                MainVM.Stories[index].Seen = seen;
                                    if (reel.Seen == reel.LatestReelMedia || reel.LatestReelMedia == seen)
                                        reel.IsSeen = true;
                                //if (reel.Seen > 1 && reel.Items.LastOrDefault()?.TakenAt.Year > 2009)
                                //    if ((int)reel.Seen == (int)reel.Items.LastOrDefault()?.TakenAt.ToUnixTime())
                                //        reel.IsSeen = true;
                            }
                        }
                        catch { }
                    }
                }


                StoryView.FeedListStatic = new List<InstaReelFeed>();
            }
            catch { }

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

        #region User cards
        private void ProfilePictureOrMediaInfosTapped(object sender, TappedRoutedEventArgs e)
        {
            //InstaSuggestionItem
            try
            {
                if (sender is Grid grid && grid.DataContext is InstaSuggestionItem item && item != null)
                    Helper.OpenProfile(item.User);
            }
            catch { }
        }

        private async void FollowUnfollowButtonForUserCardsClick(object sender, RoutedEventArgs e)
        {
            // Button
            try
            {
                if (sender is Button btn && btn.DataContext is InstaSuggestionItem item && item != null)
                {
                    var user = item.User;
                    if (btn.Content.ToString() == "Follow")
                    {
                        var result = await Helper.InstaApi.UserProcessor.FollowUserAsync(user.Pk);
                        if (result.Succeeded)
                        {
                            if (result.Value.OutgoingRequest)
                                btn.Content = "Requested";
                            else if (result.Value.Following)
                                btn.Content = "Unfollow";
                        }
                    }
                    else if (btn.Content.ToString() == "Unfollow")
                    {
                        var result = await Helper.InstaApi.UserProcessor.UnFollowUserAsync(user.Pk);
                        if (result.Succeeded)
                            btn.Content = "Follow";
                    }
                }
            }
            catch { }
        }

        private void DismissUserClick(object sender, RoutedEventArgs e)
        {
            //AppBarButton
            try
            {
                if (sender is AppBarButton btn && btn.DataContext is InstaSuggestionItem item && item != null)
                {
                    var first = MainVM.PostsGenerator.Items.FirstOrDefault();
                    if (first.Type == InstagramApiSharp.Enums.InstaFeedsType.SuggestedUsersCard &&
                        first.SuggestedUserItems?.Count > 0)
                    {
                        foreach (var x in first.SuggestedUserItems.ToList())
                        {
                            if (x.User.Pk == item.User.Pk)
                            {
                                first.SuggestedUserItems.Remove(x);
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        #endregion User cards

        private void Suggested4YouGridTapped (object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid grid && grid != null && grid.DataContext is InstaSuggestionItem user)
                    Helper.OpenProfile(user.User);
            }
            catch { }
        }

        private void LIVEGRIDLoaded(object sender, RoutedEventArgs e)
        {
            try 
            {
                if (sender is Grid grid && grid != null)
                {
                    if (grid.Resources["AnimationStory"] is Storyboard story && story != null)
                        story.Begin();
                }
            }
            catch { }
        }






        #region LOADINGS
        public void ShowTopLoading() => TopLoading.Start();
        public void HideTopLoading() => TopLoading.Stop();


        public void ShowBottomLoading() => BottomLoading.Start();
        public void HideBottomLoading() => BottomLoading.Stop();
        #endregion LOADINGS

    }
}
