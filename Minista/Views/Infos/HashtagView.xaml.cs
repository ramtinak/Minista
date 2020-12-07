using InstagramApiSharp.Classes.Models;
using Minista.ViewModels.Infos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Minista.Views.Infos
{
    public sealed partial class HashtagView : Page
    {
        private string Hashtag;


        readonly Compositor _compositor;
        readonly ImplicitAnimationCollection _elementImplicitAnimation;
        private readonly Visual _refreshButtonVisual;
        private readonly Visual _goUpButtonVisual;
        Visual _baseGridVisual;
        Grid BaseGrid;

        private bool CanLoadFirstPopUp = false;
        NavigationMode NavigationMode;
        public static HashtagView Current;
        public HashtagView()
        {
            InitializeComponent();
            Current = this;
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            _elementImplicitAnimation = _compositor.CreateImplicitAnimationCollection();

            _refreshButtonVisual = RefreshButton.GetVisual();
            _goUpButtonVisual = GoUpButton.GetVisual();
            _elementImplicitAnimation["Opacity"] = CreateOpacityAnimation();
            Loaded += HashtagViewLoaded;
        }

        private void GridLoaded(object sender, RoutedEventArgs e)
        {
            BaseGrid = sender as Grid;
            _baseGridVisual = BaseGrid.GetVisual();
        }
        private async void HashtagViewLoaded(object sender, RoutedEventArgs e)
        {
            if (NavigationMode == NavigationMode.Back && HashtagVM.Hashtag != null)
            {
                if (HashtagVM.HashtagText.ToLower().TrimAnyBullshits() == Hashtag.ToLower().TrimAnyBullshits())
                    return;
            }
            else if (NavigationMode == NavigationMode.New)
            {
                GetType().RemovePageFromBackStack();
                NavigationCacheMode = NavigationCacheMode.Enabled;
                CanLoadFirstPopUp = false;
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
            if (!CanLoadFirstPopUp)
            {
                HashtagVM.ResetCache();
                await Task.Delay(500);
                HashtagVM.HashtagsRecentGenerator.IsMine = false;
                HashtagVM.HashtagsTopGenerator.IsMine = true;
                HashtagVM.SetHashtag(Hashtag);
                CanLoadFirstPopUp = true;
                //if (Scroll == null)
                {
                    //var scroll = MainLV.FindScrollViewer();
                    //if (scroll != null)
                    //{
                    //    Scroll = scroll;
                    //    scroll.ViewChanging += ScrollViewViewChanging;
                    //}
                    HashtagVM.HashtagsTopGenerator.SetLV2(SCMain);
                    HashtagVM.HashtagsRecentGenerator.SetLV2(SCMain);


                }
                ScrollableHashtagPostUc.SetData(HashtagVM.HashtagsTopGenerator, -1);
                ScrollableRecentHashtagPostUc.SetData(HashtagVM.HashtagsRecentGenerator, -1);
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            KeyDown += OnKeyDownHandler;
            NavigationMode = e.NavigationMode;
            try
            {
                if (e.Parameter != null && e.Parameter is string str)
                    Hashtag = str?.Replace("#", "").Replace(" ", "");
            }
            catch { }
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
                    RefreshControl.RequestRefresh();
            }
            catch { }
        }
        private void RefreshControlRefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            using (var RefreshCompletionDeferral = args.GetDeferral())
            {
                try
                {
                    if(MainPivot.SelectedIndex == 0)
                    {
                        if (HashtagVM.HashtagsTopGenerator != null)
                            HashtagVM.HashtagsTopGenerator.RunLoadMore(true);
                    }
                    else
                    {
                        if (HashtagVM.HashtagsRecentGenerator != null)
                            HashtagVM.HashtagsRecentGenerator.RunLoadMore(true);
                    }
                }
                catch { }
            }
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

        private async void FollowStatusButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is InstaHashtag hashtag)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        if (hashtag.FollowStatus)
                        {
                            var result = await Helper.InstaApi.HashtagProcessor.UnFollowHashtagAsync(hashtag.Name.Replace("#", ""));
                            if (result.Succeeded)
                                hashtag.FollowStatus = false;
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                                else
                                    Helper.ShowNotify($"Error while sending unfollow request.\r\nError message: {result.Info?.Message}", 2000);
                            }
                        }
                        else
                        {
                            var result = await Helper.InstaApi.HashtagProcessor.FollowHashtagAsync(hashtag.Name.Replace("#", ""));
                            if (result.Succeeded)
                                hashtag.FollowStatus = true;
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                                else
                                    Helper.ShowNotify($"Error while sending unfollow request.\r\nError message: {result.Info?.Message}", 2000);
                            }
                        }
                    });
                }
            }
            catch { }
        }

        private void LVRelativesItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem != null && e.ClickedItem is InstaRelatedHashtag hashtag && hashtag != null)
                    Helpers.NavigationService.Navigate(typeof(HashtagView), hashtag.Name);
            }
            catch { }
        }


        //private void TopItemsLVLoaded(object sender, RoutedEventArgs e)
        //{
        //    var scroll = MainLV/*TopItemsLV*/.FindScrollViewer();
        //    if (scroll != null)
        //        scroll.ViewChanging += ScrollViewViewChanging;
        //    HashtagVM.HashtagsTopGenerator.SetLV(scroll);
        //}
        private void TopItemsLVRefreshRequested(object sender, EventArgs e)
        {
            HashtagVM.HashtagsTopGenerator.RunLoadMore(true);
        }

        //private void RecentItemsLVLoaded(object sender, RoutedEventArgs e)
        //{
        //    var scroll = MainLV/*RecentItemsLV*/.FindScrollViewer();
        //    if (scroll != null)
        //        scroll.ViewChanging += ScrollViewViewChanging;
        //    HashtagVM.HashtagsRecentGenerator.SetLV(scroll);
        //}

        private void RecentItemsLVRefreshRequested(object sender, EventArgs e)
        {
            HashtagVM.HashtagsRecentGenerator.RunLoadMore(true);
        }

        private void MainPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (MainPivot.SelectedIndex == 1)
                {
                    HashtagVM.HashtagsRecentGenerator.IsMine = true;
                    HashtagVM.HashtagsTopGenerator.IsMine = false;

                    if (HashtagVM.HashtagsRecentGenerator.Items.Count == 0)
                        HashtagVM.HashtagsRecentGenerator.RunLoadMore(true);

                    //try
                    //{
                    //    txtTop.Foreground = "#FFD5D5D5".GetColorBrush();
                    //    txtRecent.Foreground = new SolidColorBrush(Colors.White);
                    //}
                    //catch { }
                }
                else
                {
                    HashtagVM.HashtagsRecentGenerator.IsMine = false;
                    HashtagVM.HashtagsTopGenerator.IsMine = true;
                    //try
                    //{
                    //    txtRecent.Foreground = "#FFD5D5D5".GetColorBrush();
                    //    txtTop.Foreground = new SolidColorBrush(Colors.White);
                    //}
                    //catch { }
                }
            }
            catch { }
        }

        private async void LVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is InstaMedia media)
                {
                    var index = HashtagVM.HashtagsTopGenerator.Items.IndexOf(media);
                    //Helpers.NavigationService.Navigate(typeof(Posts.ScrollableHashtagPostView),
                    //    new object[] { HashtagVM.Hashtag, HashtagVM.HashtagsTopGenerator, index });
                    ////Helpers.NavigationService.Navigate(typeof(Posts.SinglePostView), media);
                    ///
                    ScrollableHashtagPostUc.Visibility = Visibility.Visible;
                    await Task.Delay(350);
                    ScrollableHashtagPostUc.ScrollTo(index);
                }
            }
            catch { }
        }


        private async void LVItemXClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is InstaMedia media)
                {
                    var index = HashtagVM.HashtagsRecentGenerator.Items.IndexOf(media);
                    //Helpers.NavigationService.Navigate(typeof(Posts.ScrollableHashtagPostView),
                    //    new object[] { HashtagVM.Hashtag, HashtagVM.HashtagsRecentGenerator, index });
                    ////Helpers.NavigationService.Navigate(typeof(Posts.SinglePostView), media);
                    ScrollableRecentHashtagPostUc.Visibility = Visibility.Visible;
                    await Task.Delay(350);
                    ScrollableRecentHashtagPostUc.ScrollTo(index);
                }
            }
            catch { }
        }









        private double _lastVerticalOffset;
        private bool _isHideTitleGrid;




        private async void ScrollViewViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                var sc = sender as ScrollViewer;
                HandleGoUpRefreshButtons(sc);
                //if (sc.VerticalOffset >= GridMainScrollViewer.ActualHeight && !tryingEnableSCs)
                //{
                //    tryingEnableSCs = true;
                //    sc.DisableScroll();
                //    S1?.EnableScroll();
                //    S2?.EnableScroll();
                //    //isMainScrollEnabled = false;
                //    ("DISABELING SC MAIN").PrintDebug();
                //    GridMainScrollViewer.Height = 0;
                //    //SCMain.ChangeView(null, GridMainScrollViewer.ActualHeight, null);
                //    try
                //    {
                //        await Task.Delay(40);

                //        S1?.ChangeView(null, lastSC1Offset, null);
                //        S2?.ChangeView(null, lastSC2Offset, null);

                //    }
                //    catch { }
                //    await Task.Delay(500);
                //    tryingEnableSCs = false;
                //}
            }
            catch { }
        }
        void HandleGoUpRefreshButtons(ScrollViewer scrollViewer)
        {
            try
            {
                if ((scrollViewer.VerticalOffset - _lastVerticalOffset) > 5 && !_isHideTitleGrid)
                {
                    _isHideTitleGrid = true;
                    ToggleGoUpButtonAnimation(false);
                    ToggleRefreshButtonAnimation(false);
                }
                else if (scrollViewer.VerticalOffset < _lastVerticalOffset && _isHideTitleGrid)
                {
                    _isHideTitleGrid = false;
                    ToggleGoUpButtonAnimation(true);
                    ToggleRefreshButtonAnimation(true);
                }
                _lastVerticalOffset = scrollViewer.VerticalOffset;
            }
            catch { }
        }

        private void ToggleGoUpButtonAnimation(bool show)
        {
            try
            {
                var scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
                scaleAnimation.InsertKeyFrame(1f, show ? 1f : 0);
                scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);

                _goUpButtonVisual.CenterPoint = new Vector3((float)GoUpButton.ActualWidth / 2f, (float)GoUpButton.ActualHeight / 2f, 0f);
                _goUpButtonVisual.StartAnimation("Scale.X", scaleAnimation);
                _goUpButtonVisual.StartAnimation("Scale.Y", scaleAnimation);
            }
            catch { }
        }


        private void GoUpButtonClick(object sender, RoutedEventArgs e)
        {
            SCMain?.ScrollToElement(0);
        }

        private void ToggleRefreshButtonAnimation(bool show)
        {
            try
            {
                var scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
                scaleAnimation.InsertKeyFrame(1f, show ? 1f : 0);
                scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);

                _refreshButtonVisual.CenterPoint = new Vector3((float)RefreshButton.ActualWidth / 2f, (float)RefreshButton.ActualHeight / 2f, 0f);
                _refreshButtonVisual.StartAnimation("Scale.X", scaleAnimation);
                _refreshButtonVisual.StartAnimation("Scale.Y", scaleAnimation);
            }
            catch { }
        }














        private void LVContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            try
            {
                var elementVisual = ElementCompositionPreview.GetElementVisual(args.ItemContainer);
                if (args.InRecycleQueue)
                {
                    elementVisual.ImplicitAnimations = null;
                }
                else
                {
                    //Add implicit animation to each visual 
                    elementVisual.ImplicitAnimations = _elementImplicitAnimation;
                }
            }
            catch { }
        }

        private void ProfileEllipseTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (HashtagVM.Stories?.Count > 0)
                {
                    var list = new List<InstaReelFeed> { HashtagVM.Reel};
                    Helpers.NavigationService.Navigate(typeof(Main.StoryView), new object[] { list, 0 });
                }
            }
            catch { }
        }

        private void MediaElementTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (HashtagVM.HashtagsTopGenerator.Channel != null)
                    Helpers.NavigationService.Navigate(typeof(Posts.MultiplePostView), HashtagVM.HashtagsTopGenerator.Channel);
            }
            catch { }
        }
        bool Repeated = false;
        private void MediaElementMediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Repeated)
                {
                    (sender as MediaElement).Play();
                    Repeated = true;
                }
            }
            catch { }
        }

        private void MainLVRefreshRequested(object sender, EventArgs e)
        {
            HashtagVM.Refresh();
            HashtagVM.HashtagsRecentGenerator.RunLoadMore(true);
        }

        private void TopButtonClick(object sender, RoutedEventArgs e)
        {
            MainPivot.SelectedIndex = 0;
        }

        private void RecentButtonClick(object sender, RoutedEventArgs e)
        {
            MainPivot.SelectedIndex = 1;
            //if(HashtagVM.HashtagsRecentGenerator.Items.Count == 0)
            //    ToggleHeaderGridAnimation(false);
        }

        private void HeaderGridLoaded(object sender, RoutedEventArgs e)
        {
            //ToggleHeaderGridAnimation(false);
        }
        #region Animation

        private CompositionAnimationGroup CreateOpacityAnimation()
        {
            ScalarKeyFrameAnimation fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1.0f, 0.0f);
            fadeAnimation.Duration = TimeSpan.FromSeconds(.35);
            fadeAnimation.Target = "Opacity";
            //Add Animations to Animation group. 
            CompositionAnimationGroup animationGroup = _compositor.CreateAnimationGroup();
            animationGroup.Add(fadeAnimation);

            return animationGroup;
        }
        private CompositionAnimationGroup CreateOffsetAnimation()
        {

            //Define Offset Animation for the ANimation group
            Vector3KeyFrameAnimation offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromSeconds(.4);

            //Define Animation Target for this animation to animate using definition. 
            offsetAnimation.Target = "Offset";


            ScalarKeyFrameAnimation fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1f, 0.8f);
            fadeAnimation.Duration = TimeSpan.FromSeconds(.4);
            fadeAnimation.Target = "Opacity";




            //Define Rotation Animation for Animation Group. 
            ScalarKeyFrameAnimation rotationAnimation = _compositor.CreateScalarKeyFrameAnimation();
            rotationAnimation.InsertKeyFrame(.5f, 0.160f);
            rotationAnimation.InsertKeyFrame(1f, 0f);
            rotationAnimation.Duration = TimeSpan.FromSeconds(.4);

            //Define Animation Target for this animation to animate using definition. 
            rotationAnimation.Target = "RotationAngle";

            //Add Animations to Animation group. 
            CompositionAnimationGroup animationGroup = _compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);
            animationGroup.Add(rotationAnimation);
            animationGroup.Add(fadeAnimation);

            return animationGroup;
        }
        #endregion




        #region LOADINGS
        public void ShowTopLoading() => TopLoading.Start();
        public void HideTopLoading() => TopLoading.Stop();


        public void ShowBottomLoading() => BottomLoading.Start();
        public void HideBottomLoading() => BottomLoading.Stop();
        #endregion LOADINGS
        #region Scroll
        private ScrollViewer S1, S2;

        //private bool tryingEnableSCs = false; 
        //bool isMainScrollEnabled = true;

        //private double _lastMainVerticalOffset;
        //private bool _isMainHideTitleGrid;
        //private bool _triedFirst = false;
        //private double lastSC1Offset = 0;
        //private double lastSC2Offset = 0;
        private void OnLVTopLoaded(object sender, RoutedEventArgs e)
        {
            S1 = LVTop.FindScrollViewer();
            if (S1 != null)
            { 
                S1.ViewChanging += OnSCViewChanging;
                S1.ViewChanged += InnerLVsScrollViewViewChanged;
            }
            HashtagVM.HashtagsTopGenerator.SetLV(S1);
        }

        private void OnLVRecentLoaded(object sender, RoutedEventArgs e)
        {
            S2 = LVRecent.FindScrollViewer();
            if (S2 != null)
            {
                //if (SCMain.VerticalScrollMode == ScrollMode.Disabled)
                //    S2.EnableScroll();
                S2.ViewChanging += OnSCViewChanging;
                S2.ViewChanged += InnerLVsScrollViewViewChanged;
            }
            HashtagVM.HashtagsRecentGenerator.SetLV(S2);
        }
        private /*async*/ void OnSCViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;

            HandleGoUpRefreshButtons(scrollViewer);
            //if ((scrollViewer.VerticalOffset - _lastMainVerticalOffset) > 5 && !_isMainHideTitleGrid)
            //{
            //    _isMainHideTitleGrid = true;
            //}
            //else if (scrollViewer.VerticalOffset < _lastMainVerticalOffset && _isMainHideTitleGrid)
            //{
            //    _isMainHideTitleGrid = false;
            //    "2".PrintDebug();
            //    if (scrollViewer.VerticalOffset < 3.0)
            //    {
            //        return;
            //        //SCMain.EnableScroll();
            //    }
            //}
            //if (scrollViewer.VerticalOffset < 3.0)
            //{
            //    "3".PrintDebug();

            //    _isMainHideTitleGrid = true;

            //}
            //if (_lastMainVerticalOffset > scrollViewer.VerticalOffset)
            //{
            //    if (scrollViewer.VerticalOffset < 3.0 && _isMainHideTitleGrid && !_triedFirst)
            //    {
            //        _triedFirst = true; 
            //        ("DISABELING SC1 SC2").PrintDebug();
            //        lastSC1Offset = S1.VerticalOffset;
            //        if (S2 != null)
            //            lastSC2Offset = S2.VerticalOffset;
            //        scrollViewer.DisableScroll();
            //        await Task.Delay(150);
            //        SCMain.EnableScroll();
            //        GridMainScrollViewer.Height = double.NaN;
            //        await Task.Delay(10);
            //        SCMain.EnableScroll();
            //        SCMain.ChangeView(null, GridMainScrollViewer.ActualHeight - 10, null);
            //        await Task.Delay(750);
            //        _triedFirst = false;

            //    }
            //}
            //_lastMainVerticalOffset = scrollViewer.VerticalOffset;
        }
        #endregion Scroll

        #region Scroll v2
        private double _scrollingHost;
        private bool _scrollingHostDisabled = false;

        private double _innerScrolls;
        private bool _innerScrollsDisabled = true;
        private void ScrollViewViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (_scrollingHostDisabled)
            {
                if (!e.IsIntermediate)
                {
                    scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null, true);
                }

                _scrollingHost = scrollViewer.VerticalOffset;
                return;
            }

            if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 12 && _scrollingHost < scrollViewer.VerticalOffset)
            {
                _scrollingHostDisabled = true;
                SetScrollMode(false);

                SetInnerScrollMode(true);
                _innerScrollsDisabled = false;
            }

            _scrollingHost = scrollViewer.VerticalOffset;
        }

        private void InnerLVsScrollViewViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer2 = sender as ScrollViewer;
            if (_innerScrollsDisabled)
            {
                if (!e.IsIntermediate)
                {
                    scrollViewer2.ChangeView(null, 12, null, false);
                }

                _innerScrolls = scrollViewer2.VerticalOffset;
                return;
            }

            if (scrollViewer2.VerticalOffset <= 12 && _innerScrolls > scrollViewer2.VerticalOffset)
            {
                SetScrollMode(true);
                _scrollingHostDisabled = false;

                _innerScrollsDisabled = true;
                SetInnerScrollMode(false);
            }

            _innerScrolls = scrollViewer2.VerticalOffset;
        }
        private IEnumerable<ScrollViewer> GetScrollViewers()
        {
            if (S1 != null)
                yield return S1;

            if (S2 != null)
                yield return S2;
        }

        public void SetInnerScrollMode(bool enable)
        {
            foreach (var scrollViewer in GetScrollViewers())
            {
                if (enable)
                {
                    scrollViewer.VerticalScrollMode = ScrollMode.Auto;
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scrollViewer.ChangeView(null, 12, null, true);
                }
                else
                {
                    scrollViewer.ChangeView(null, 12, null, true);
                    scrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
            }
        }
        private void SetScrollMode(bool enable)
        {
            if (enable)
            {
                SCMain.VerticalScrollMode = ScrollMode.Auto;
                SCMain.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                SCMain.ChangeView(null, SCMain.ScrollableHeight - 48, null, false);
            }
            else
            {
                SCMain.ChangeView(null, SCMain.ScrollableHeight, null, true);
                SCMain.VerticalScrollMode = ScrollMode.Disabled;
                SCMain.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }
        }
        #endregion
    }
}
