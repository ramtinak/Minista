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
using InstagramApiSharp.Classes.Models;
using Minista.ViewModels.Main;
using Minista.Models;
using Minista.Helpers;
using Windows.UI.Xaml.Documents;
using Minista.Converters;
using Minista.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Core;
using InstagramApiSharp.Classes;

namespace Minista.Views.Main
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ActivitiesView : Page
    {
        bool First = true;

        public ActivitiesViewModel ActivitiesVM { get; set; } = new ActivitiesViewModel();
        readonly DateTimeConverter DateConverter = new DateTimeConverter();
        public static ActivitiesView Current;
        public ActivitiesView()
        {
            this.InitializeComponent();
            Current = this;
            NavigationCacheMode = NavigationCacheMode.Enabled;
            Loaded += ActivitiesViewLoaded;
            DataContext = ActivitiesVM;
        }

        private void ActivitiesViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshControl.RefreshRequested -= RefreshControlRefreshRequested;
                RefreshControl.Visualizer.RefreshStateChanged -= RefreshControlRefreshStateChanged;
            }
            catch { }
            RefreshControl.RefreshRequested += RefreshControlRefreshRequested;
            if (RefreshControl.Visualizer != null)
                RefreshControl.Visualizer.RefreshStateChanged += RefreshControlRefreshStateChanged;
            //SetLVs();
            if (First)
            {
                ActivitiesVM.RunLoadMore(true);
                //ActivitiesVM.RunLoadMore2(true);
                First = false;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            KeyDown += OnKeyDownHandler;
            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();
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
                    ActivitiesVM?.Refresh();
            }
            catch { }
        }
        //void SetLVs()
        //{
        //    try
        //    {
        //        ActivitiesVM.SetLV(FollowingItemsLV.FindScrollViewer());
        //    }
        //    catch { }
        //}
        //private void NonFollowersItemsLVRefreshRequested(object sender, EventArgs e)
        //{
        //    ActivitiesVM?.NonFollowersVM?.RunLoadMore(true);
        //}

        private void RefreshControlRefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            using (var RefreshCompletionDeferral = args.GetDeferral())
                ActivitiesVM?.Refresh();
        }
        private void RefreshControlRefreshStateChanged(Microsoft.UI.Xaml.Controls.RefreshVisualizer sender, Microsoft.UI.Xaml.Controls.RefreshStateChangedEventArgs args)
        {
            if (args.NewState == Microsoft.UI.Xaml.Controls.RefreshVisualizerState.Refreshing)
                RefreshButton.IsEnabled = false;
            else
                RefreshButton.IsEnabled = true;
        }
        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            RefreshControl.RequestRefresh();
        }

        private void LikedTaggedGridViewItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem != null &&
                    e.ClickedItem is InstaActivityMedia activityMedia && activityMedia != null)
                {
                    NavigationService.Navigate(typeof(Posts.SinglePostView), activityMedia.Id);
                }
            }
            catch { }
        }

        private async void FollowUnfollowHashtagButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && 
                    btn.DataContext is RecentActivityFeed recentActivity && recentActivity != null)
                {
                    if (recentActivity.StoryType == InstagramApiSharp.Enums.InstaActivityFeedStoryType.LikedTagged &&
                        recentActivity.HashtagFollow != null)
                    {
                        if (recentActivity.HashtagFollow.FollowStatus)
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                            {
                                var result = await Helper.InstaApi.HashtagProcessor.UnFollowHashtagAsync(recentActivity.HashtagFollow.Name);
                                if (result.Succeeded)
                                    recentActivity.HashtagFollow.FollowStatus = false;
                            });
                        }
                        else
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                            {
                                var result = await Helper.InstaApi.HashtagProcessor.FollowHashtagAsync(recentActivity.HashtagFollow.Name);
                                if (result.Succeeded)
                                    recentActivity.HashtagFollow.FollowStatus = true;
                            });
                        }
                    }
                }
            }
            catch { }
        }

        private void TextBlockDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                if (sender is TextBlock textBlock && args.NewValue is RecentActivityFeed recentActivity && recentActivity != null)
                {
                    if (recentActivity.StoryType == InstagramApiSharp.Enums.InstaActivityFeedStoryType.LikedTagged)
                    {
                        using (var pg = new PassageHelperX())
                        {
                            var passages = pg.GetInlines(recentActivity.RichText, HyperLinkHelper.HyperLinkClick);
                            textBlock.Inlines.Clear();
                            passages.Item1.ForEach(item =>
                            textBlock.Inlines.Add(item));
                        }
                    }
                    else
                    {
                        var textStr = recentActivity.Text;
                        var text = recentActivity.Text;

                        if (recentActivity.Links?.Count > 0)
                        {
                            foreach (var link in recentActivity.Links)
                            {
                                try
                                {
                                    var mention = textStr.Substring(link.Start, (link.End - link.Start) -1);
                                    //if (link.Type == InstagramApiSharp.Enums.InstaLinkType.User)
                                    //    text = text.Replace(mention, $"@{mention}");//⥽⍬⥶⍬⥽
                                    //else
                                        text = text.Replace(mention, $"https:\\{mention}".Replace(" ", "⥽⍬⥶⍬⥽"));//⥽⍬⥶⍬⥽
                                }
                                catch { }
                            }
                        }

                        using (var pg = new PassageHelperX())
                        {
                            var passages = pg.GetInlines(text, HyperLinkHelper.HyperLinkClick);
                            textBlock.Inlines.Clear();
                            passages.Item1.ForEach(item =>
                            textBlock.Inlines.Add(item));
                        }
                    }

                    textBlock.Inlines.Add(new LineBreak());
                    var date = "";
                    try
                    {
                        date = (string)DateConverter.Convert(recentActivity.TimeStamp, typeof(DateTime), null, null);
                    }
                    catch { date = ""; }
                    if (!string.IsNullOrEmpty(date))
                    {
                        textBlock.Inlines.Add(new Run
                        {
                            Foreground = (SolidColorBrush)Application.Current.Resources["DefaultInnerForegroundColor"],
                            Text = date,
                           
                        });
                    }
                }
            }
            catch { }
        }
        private void MediaImageExTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is ImageEx imageEx && imageEx.DataContext is RecentActivityFeed recentActivity && recentActivity != null)
                {
                    if (recentActivity.Medias?.Count > 0)
                        NavigationService.Navigate(typeof(Posts.SinglePostView), recentActivity.Medias[0].Id);

                }
            }
            catch { }
        }

        private void UserEllipseTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Ellipse ellipse && ellipse.DataContext is RecentActivityFeed recentActivity && recentActivity != null)
                {
                    if (!string.IsNullOrEmpty(recentActivity.ProfileName))
                        Helper.OpenProfile(recentActivity.ProfileName);
                    else if(recentActivity.ProfileId > 0)
                        Helper.OpenProfile(recentActivity.ProfileId);
                }
            }
            catch { }
        }

        //private void FollowingItemsLVRefreshRequested(object sender, EventArgs e)
        //{
        //    ActivitiesVM?.Refresh2();
        //}

        private void MainPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            //    SetLVs();
            //}
            //catch { }
        }

        //private void FollowingItemsLVLoaded(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        SetLVs();
        //    }
        //    catch { }
        //}

        private void FriendRequestTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(typeof(Infos.FollowRequestsView));
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

        private async void LikeButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is AppBarButton btn && btn != null)
                {
                    btn.DataContext.GetType().PrintDebug();
                    if (btn.DataContext is RecentActivityFeed data && data != null && data.CommentId != null)
                    {
                        if (data.HasLikedComment)
                        {
                            var result = await Helper.InstaApi.CommentProcessor.UnlikeCommentAsync(data.CommentId.Value.ToString());
                            if (result.Succeeded)
                                data.HasLikedComment = false;
                        }
                        else
                        {
                            var result = await Helper.InstaApi.CommentProcessor.LikeCommentAsync(data.CommentId.Value.ToString());
                            if (result.Succeeded)
                                data.HasLikedComment = true;
                        }
                    }
                }
            }
            catch { }
        }

        private void ReplyButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is HyperlinkButton btn && btn != null)
                {
                    if (btn.DataContext is RecentActivityFeed data && data != null)
                    {
                        ActivitiesVM.CommentFeed = data;
                        ShowComments();
                        CommentText.PlaceholderText = $"Reply to @{data.ProfileName ?? string.Empty}'s comment";
                        CommentText.Text = $"@{data.ProfileName ?? string.Empty} ";
                        CommentText.Focus(FocusState.Keyboard);
                    }
                }
            }
            catch { }
        }
        private async void CommentButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(CommentText.Text))
                {
                    CommentText.Focus(FocusState.Keyboard);
                    return;
                }
                var feed = ActivitiesVM.CommentFeed;
                if (feed == null)
                {
                    HideComments();
                    return;
                }

                var result = await Helper.InstaApi.CommentProcessor.ReplyCommentMediaAsync(feed.Medias[0].Id, feed.CommentId.ToString(), CommentText.Text);
                HideComments();
                ActivitiesVM.CommentFeed = null;
                if (result.Succeeded)
                {
                    Helper.ShowNotify("Comment sent successfully.");
                    CommentText.Text = "";
                }
                else
                {
                    switch (result.Info.ResponseType)
                    {
                        case ResponseType.RequestsLimit:
                        case ResponseType.SentryBlock:
                            Helper.ShowNotify(result.Info.Message);
                            break;
                        case ResponseType.ActionBlocked:
                            Helper.ShowNotify("Action blocked.\r\nPlease try again 5 or 10 minutes later");
                            break;
                    }
                }
            }
            catch { }
        }

        public void CloseCommentButtonClick(object sender, RoutedEventArgs e) => HideComments();
        public void ShowComments() => CommentsGrid.Visibility = Visibility.Visible;
        public void HideComments() => CommentsGrid.Visibility = Visibility.Collapsed;
        #region LOADINGS You
        public void ShowTopLoadingYou() => TopLoadingYou.Start();
        public void HideTopLoadingYou() => TopLoadingYou.Stop();


        public void ShowBottomLoadingYou() => BottomLoadingYou.Start();
        public void HideBottomLoadingYou() => BottomLoadingYou.Stop();



        public void ShowTopLoadingFollowers() { /*TopLoadingFollowers.Start(); */}
        public void HideTopLoadingFollowers() { /*TopLoadingFollowers.Stop();*/ }
        #endregion LOADINGS You
         

        //#region LOADINGS Following
        //public void ShowTopLoadingFollowing() => TopLoadingFollowing.Start();
        //public void HideTopLoadingFollowing() => TopLoadingFollowing.Stop();


        //public void ShowBottomLoadingFollowing() => BottomLoadingFollowing.Start();
        //public void HideBottomLoadingFollowing() => BottomLoadingFollowing.Stop();
        //#endregion LOADINGS Following
    }
}
