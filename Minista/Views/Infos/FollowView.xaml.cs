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
using Minista.ViewModels.Infos;
using InstagramApiSharp.Classes.Models;
using Windows.UI.Core;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using System.Threading.Tasks;
using System.Numerics;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Minista.ContentDialogs;

namespace Minista.Views.Infos
{
    public sealed partial class FollowView : Page
    {
        readonly Compositor _compositor;

        Visual _baseGridVisual;
        ScrollViewer ScrollView, ScrollView2, ScrollView3, ScrollViewFollowersSearch;
        public static FollowView Current;
        public FollowView()
        {
            this.InitializeComponent();
            Current = this;
            Loaded += FollowViewLoaded;
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        }
        private int SelectIndex = 1;
        private InstaUserShort User;
        bool CanLoadFirstPopUp = false;
        NavigationMode NavigationMode;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            KeyDown += OnKeyDownHandler;
            ($"===================================== NAV MODE: {e.NavigationMode} ========================").PrintDebug();

            NavigationMode = e.NavigationMode;

            if (e.Parameter != null)
            {
                if (e.Parameter is InstaUserShort userShort)
                    User = userShort;
                else if (e.Parameter is object[] obj)
                {
                    if (obj[0] is InstaUserShort user)
                    {
                        User = user;
                        SelectIndex = (int)obj[1] + 1;
                    }
                }
            }
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
                {
                    FollowVM.FollowersGenerator.RunLoadMore(true);
                    FollowVM.FollowingsGenerator.RunLoadMore(true);
                }
            }
            catch { }
        }
        private void FollowViewLoaded(object sender, RoutedEventArgs e)
        {

            if (NavigationMode == NavigationMode.Back && FollowVM.User != null)
            {
                if (FollowVM.User.Pk == User.Pk) 
                return;
            }
            else if(NavigationMode == NavigationMode.New)
            {
                GetType().RemovePageFromBackStack();
                NavigationCacheMode = NavigationCacheMode.Enabled;
                CanLoadFirstPopUp = false;
            }
            SetFollowersLV();
            SetFollowingsLV();
            FollowVM.SetUser(User);

            try
            {
                if (!CanLoadFirstPopUp)
                {
                    FollowingsSearchText.Text = "";
                    FollowersSearchText.Text = "";
                    try
                    {
                        MainPivot.SelectedIndex = SelectIndex;
                    }
                    catch { }



                    try
                    {
                        if (SelectIndex == 1)
                        {
                            FollowVM.FollowersGenerator.RunLoadMore(true);
                            FollowVM.FollowingsGenerator.RunLoadMore(true);
                        }
                        else if (SelectIndex == 2)
                        {
                            FollowVM.FollowingsGenerator.RunLoadMore(true);
                            FollowVM.FollowersGenerator.RunLoadMore(true);
                        }
                    }
                    catch { }
                        FollowVM.MutualFriendsGenerator.RunLoadMore(true);
                    CanLoadFirstPopUp = true;
                }
            }
            catch { }
        }

        private void FollowersItemsLVLoaded(object sender, RoutedEventArgs e)
        {
            SetFollowersLV();
            ScrollView = FollowersItemsLV.FindScrollViewer();
            if (ScrollView != null)
                ScrollView.ViewChanging += ScrollViewViewChanging;


            ScrollView2 = FollowingItemsLV.FindScrollViewer();
            if (ScrollView2 != null)
                ScrollView2.ViewChanging += ScrollViewViewChanging;
        }

        private void FollowersItemsLVRefreshRequested(object sender, EventArgs e)
        {
            FollowVM.FollowersGenerator.RunLoadMore(true);
        }

        private void FollowingItemsLVLoaded(object sender, RoutedEventArgs e)
        {
            SetFollowingsLV();
        }
        private void FollowingItemsLVRefreshRequested(object sender, EventArgs e)
        {
            FollowVM.FollowingsGenerator.RunLoadMore(true);
        }
        void SetFollowersLV()
        {
            try
            {
                FollowVM.FollowersGenerator.SetLV(FollowersItemsLV.FindScrollViewer());
            }
            catch { }
        }
        void SetFollowingsLV()
        {
            try
            {
                FollowVM.FollowingsGenerator.SetLV(FollowingItemsLV.FindScrollViewer());
            }
            catch { }
        }

        private async void FollowUnFollowMainButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn != null && btn.DataContext is InstaUserShortFriendship user)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        if (btn.Content.ToString() == "Follow")
                        {
                            var result = await Helper.InstaApi.UserProcessor.FollowUserAsync(user.Pk);
                            if (result.Succeeded)
                                user.FriendshipStatus = result.Value?.ToFriendshipShortStatus();
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                                else
                                    Helper.ShowNotify($"Error while sending follow request to  @{user.UserName}.\r\n" +
                                      $"Error message: {result.Info?.Message}", 2000);
                            }
                        }
                        else if (btn.Content.ToString() == "Unfollow" || btn.Content.ToString() == "Requested")
                        {
                            var result = await Helper.InstaApi.UserProcessor.UnFollowUserAsync(user.Pk);
                            if (result.Succeeded)
                                user.FriendshipStatus = result.Value?.ToFriendshipShortStatus();
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                                else
                                    Helper.ShowNotify($"Error while sending follow request to  @{user.UserName}.\r\n" +
                                     $"Error message: {result.Info?.Message}", 2000);
                            }
                        }
                    });
                }
            }
            catch { }
        }

        private void UserTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid gid && gid.DataContext is InstaUserShortFriendship item && item != null)
                {
                    Helper.OpenProfile(item.ToUserShort());
                }
            }
            catch { }
        }



        Grid BaseGrid;

        private void GridLoaded(object sender, RoutedEventArgs e)
        {
            BaseGrid = sender as Grid;
            _baseGridVisual = BaseGrid.GetVisual();
        }



        //private double _lastVerticalOffset;
        //private bool _isHideTitleGrid;




        private void ScrollViewViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                //var scrollViewer = sender as ScrollViewer;
                //if ((scrollViewer.VerticalOffset - _lastVerticalOffset) > 5 && !_isHideTitleGrid)
                //{
                //    _isHideTitleGrid = true;
                //    ToggleGoUpButtonAnimation(false);
                //}
                //else if (scrollViewer.VerticalOffset < _lastVerticalOffset && _isHideTitleGrid)
                //{
                //    _isHideTitleGrid = false;
                //    ToggleGoUpButtonAnimation(true);
                //}
                //_lastVerticalOffset = scrollViewer.VerticalOffset;
            }
            catch { }
        }

        private async void ToggleGoUpButtonAnimation(bool show)
        {
            if (_baseGridVisual == null)
                return;
            if (show)
                BaseGrid.Visibility = Visibility.Visible;

            var scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(1f, show ? 1f : 0);
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);

            _baseGridVisual.CenterPoint = new Vector3((float)BaseGrid.ActualWidth / 2f, (float)BaseGrid.ActualHeight / 2f, 0f);
            _baseGridVisual.StartAnimation("Scale.X", scaleAnimation);
            _baseGridVisual.StartAnimation("Scale.Y", scaleAnimation);
            try
            {
                if (!show)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await Task.Delay(510);
                        BaseGrid.Visibility = Visibility.Collapsed;
                    });

                }
            }
            catch { }
        }



        //////////////////////////////////
        ////////// TAB GENERATOR /////////
        //private PullToRefreshListView MutualFriendsItemsLV;
        //PivotItem GetPivotItem()
        //{
        //    var pItem = new PivotItem
        //    {
        //        Margin = new Thickness(0),
        //        Padding = new Thickness(0),
        //        Header = "Mutual"
        //    };

        //    var listView = new PullToRefreshListView
        //    {
        //        Foreground = "{StaticResource DefaultInnerForegroundColor}".GetColorBrush(),
        //        Margin = new Thickness(5),
        //        SelectionMode = ListViewSelectionMode.None,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        VerticalAlignment = VerticalAlignment.Stretch,
        //        IsItemClickEnabled = true,
        //        PullThreshold = 140,
        //        IsPullToRefreshWithMouseEnabled = true
        //    };
        //    listView.ItemTemplate = Resources["UserTemplate"] as DataTemplate;
        //    listView.ItemsSource = FollowVM.MutualFriendsGenerator.Items;
        //    listView.RefreshRequested += MutualFriendsItemsLVRefreshRequested;
        //    listView.Loaded += MutualFriendsItemsLVLoaded;
        //    pItem.Content = listView;

        //    MutualFriendsItemsLV = listView;
        //    return pItem;
        //}

        private async void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is AppBarButton btn && btn.DataContext is InstaUserShortFriendship user)
                {
                    await new RemoveUserDialog(user).ShowAsync();
                }
            }
            catch { }
        }

        private void MutualFriendsItemsLVLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ScrollView3 = MutualFriendsItemsLV.FindScrollViewer();
                if (ScrollView3 != null)
                    ScrollView3.ViewChanging += ScrollViewViewChanging;
            }
            catch { }
        }

        private void MutualFriendsItemsLVRefreshRequested(object sender, EventArgs e)
        {
            FollowVM.MutualFriendsGenerator.RunLoadMore(true);
        }


        private async void FollowButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is InstaSuggestionItem item && item != null)
                {
                    if (btn.Content.ToString().ToLower() == "follow")
                    {
                        var result = await Helper.InstaApi.UserProcessor.FollowUserAsync(item.User.Pk);
                        if (result.Succeeded)
                        {
                            var data = result.Value;
                            if (data.Following)
                                btn.Content = "Unfollow";
                            else if (data.OutgoingRequest)
                                btn.Content = "Requested";
                            else
                                btn.Content = "Follow";
                        }
                        else
                        {
                            if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                            else
                                Helper.ShowNotify($"Error while sending follow request to  @{item.User.UserName}.\r\n" +
                                  $"Error message: {result.Info?.Message}", 2000);
                        }
                    }
                    else if (btn.Content.ToString() == "unfollow" || btn.Content.ToString() == "requested")
                    {
                        var result = await Helper.InstaApi.UserProcessor.UnFollowUserAsync(item.User.Pk);
                        if (result.Succeeded)
                        {
                            var data = result.Value;
                            if (data.Following)
                                btn.Content = "Unfollow";
                            else if (data.OutgoingRequest)
                                btn.Content = "Requested";
                            else
                                btn.Content = "Follow";
                        }
                        else
                        {
                            if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                            else
                                Helper.ShowNotify($"Error while sending unfollow request to  @{item.User.UserName}.\r\n" +
                                  $"Error message: {result.Info?.Message}", 2000);
                        }
                    }
                }
            }
            catch { }
        }


        private void SuggestionTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid grid && grid.DataContext is InstaSuggestionItem item && item != null)
                    Helper.OpenProfile(item.User);
            }
            catch { }
        }

        #region followings search
        private void FollowingsSearchButtonClick(object sender, RoutedEventArgs e) => DoFollowingsSearch();

        private void FollowingsSearchTextTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (FollowingsSearchText.Text.Length == 0)
                {
                    FollowingSearchItemsLV.Visibility = Visibility.Collapsed;
                    FollowingItemsLV.Visibility = Visibility.Visible;
                    FollowVM.FollowingsGenerator.SearchItems.Clear();
                }
                else
                {
                    DoFollowingsSearch();
                }
            }
            catch { }
        }
        private void FollowingsSearchTextKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                    DoFollowingsSearch();
            }
            catch { }
        }
        void DoFollowingsSearch()
        {
            try
            {
                if (string.IsNullOrEmpty(FollowingsSearchText.Text)) return;

                FollowingSearchItemsLV.Visibility = Visibility.Visible;
                FollowingItemsLV.Visibility = Visibility.Collapsed;
                FollowVM.FollowingsGenerator.SearchItems.Clear();
                var list = FollowVM.FollowingsGenerator.Items.Where(x => x.UserName.ToLower().Contains(FollowingsSearchText.Text.ToLower())).ToList();
                if(list?.Count >0)
                FollowVM.FollowingsGenerator.SearchItems.AddRange(list);
            }
            catch { }
        }
        #endregion

        #region followers search
        private void FollowersSearchButtonClick(object sender, RoutedEventArgs e) => DoFollowersSearch();

        private void FollowersSearchTextTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (FollowersSearchText.Text.Length == 0)
                {
                    FollowersSearchItemsLV.Visibility = Visibility.Collapsed;
                    FollowersItemsLV.Visibility = Visibility.Visible;
                    FollowVM.FollowersGenerator.SearchItems.Clear();
                }
                else
                {
                    DoFollowersSearch();
                }
            }
            catch { }
        }
        private void FollowersSearchTextKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                    DoFollowersSearch();
            }
            catch { }
        }
        void DoFollowersSearch()
        {
            try
            {
                if (string.IsNullOrEmpty(FollowersSearchText.Text)) return;

                FollowersSearchItemsLV.Visibility = Visibility.Visible;
                FollowersItemsLV.Visibility = Visibility.Collapsed;
                FollowVM.FollowersGenerator.SearchItems.Clear();
                FollowVM.FollowersGenerator.SearchWord = FollowersSearchText.Text.ToLower();
                FollowVM.FollowersGenerator.RunLoadMore2(true);
            }
            catch { }
        }
        private void FollowersSearchItemsLVLoaded(object sender, RoutedEventArgs e)
        {
            if (FollowVM?.FollowersGenerator != null)
            {
                ScrollViewFollowersSearch = FollowersSearchItemsLV.FindScrollViewer();
                if (ScrollViewFollowersSearch != null)
                    FollowVM?.FollowersGenerator.SetSearchLV(ScrollViewFollowersSearch);
            }
        }

        #endregion






        #region LOADINGS Mutual
        public void ShowTopLoadingMutual() => TopLoadingMutual.Start();
        public void HideTopLoadingMutual() => TopLoadingMutual.Stop();


        public void ShowBottomLoadingMutual() => BottomLoadingMutual.Start();
        public void HideBottomLoadingMutual() => BottomLoadingMutual.Stop();
        #endregion LOADINGS Mutual



        #region LOADINGS Follower
        public void ShowTopLoadingFollower() => TopLoadingFollower.Start();
        public void HideTopLoadingFollower() => TopLoadingFollower.Stop();


        public void ShowBottomLoadingFollower() => BottomLoadingFollower.Start();
        public void HideBottomLoadingFollower() => BottomLoadingFollower.Stop();
        #endregion LOADINGS Follower



        #region LOADINGS Following
        public void ShowTopLoadingFollowing() => TopLoadingFollowing.Start();
        public void HideTopLoadingFollowing() => TopLoadingFollowing.Stop();


        public void ShowBottomLoadingFollowing() => BottomLoadingFollowing.Start();
        public void HideBottomLoadingFollowing() => BottomLoadingFollowing.Stop();
        #endregion LOADINGS Following




    }
}
