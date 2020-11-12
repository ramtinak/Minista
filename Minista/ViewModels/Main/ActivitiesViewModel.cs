using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minista.Models;
using Minista.Models.Main;
using System.Diagnostics;
using System.Threading;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using InstagramApiSharp;
using Minista.ItemsGenerators;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Minista.ViewModels.Direct;
using static Helper;
namespace Minista.ViewModels.Main
{
    public class ActivitiesViewModel : BaseModel, IGenerator
    {
        public static ActivitiesViewModel Instance => new ActivitiesViewModel();
        public NonFollowersViewModel NonFollowersVM { get; set; } = new NonFollowersViewModel();
        public bool FirstRun = true/*, FirstRun2 = true*/;
        public RecentActivityFeedCollection RecentItems { get; set; } = new RecentActivityFeedCollection();
        //public ObservableCollection<RecentActivityFeed> FollowingItems { get; set; } = new ObservableCollection<RecentActivityFeed>();

        public bool HasMoreItems { get; set; } = true;
        //public bool HasMoreItems2 { get; set; } = true;
        public PaginationParameters Pagination { get; private set; }
        public RecentActivityFeed CommentFeed { get; set; } = null;
        //public PaginationParameters Pagination2 { get; private set; }

        //ScrollViewer Scroll;
        //bool IsLoading = true;
        //bool IsLoading2 = true;
        //bool LVSet = false;
        public void SetLV(ScrollViewer scrollViewer)
        {
            if(Pagination == null)
            {
                Pagination = PaginationParameters.MaxPagesToLoad(1);
                //Pagination2 = PaginationParameters.MaxPagesToLoad(1);
            }
            //if (!LVSet && Scroll == null)
            //{
            //    Scroll = scrollViewer;

            //    if (Scroll != null)
            //    {
            //        Scroll.ViewChanging += ScrollViewChanging;
    
            
            //        LVSet = true;
            //    }
            //}
            //if (IsLoading)
            //{
            //}
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
                //IsLoading = false;
                return;
            }
            try
            {
                if (refresh)
                {
                    Pagination = PaginationParameters.MaxPagesToLoad(1);
                    Views.Main.ActivitiesView.Current?.ShowTopLoadingYou();
                }
                else
                    Views.Main.ActivitiesView.Current?.ShowBottomLoadingYou();
                Views.Main.ActivitiesView.Current?.HideComments();
                CommentFeed = null;
                var result = await InstaApi.UserProcessor.GetRecentActivityFeedAsync(Pagination);
                 FirstRun = false;
                Pagination.MaximumPagesToLoad = 1;
                if (!result.Succeeded)
                {
                    //IsLoading = false;
                    if (result.Value == null || result.Value?.Items?.Count == 0)
                    {
                        if (refresh)
                            Views.Main.ActivitiesView.Current?.HideTopLoadingYou();
                        else
                            Views.Main.ActivitiesView.Current?.HideBottomLoadingYou();
                        return;
                    }
                }
                MainPage.Current?.ShowActivityNotify(result.Value.Counts);
                if (string.IsNullOrEmpty(result.Value.NextMaxId))
                    HasMoreItems = false;

                if (result.Value?.Items?.Count > 0)
                {
                    if (refresh)
                    {
                        RecentItems.Clear();
                        RecentItems.CurrentList = null;
                    }

                    result.Value.Items.ForEach(item => RecentItems.AddWithColumns(item.ToRecentActivityFeed()));
                }
                await Task.Delay(1000);
                //IsLoading = false;
                await InstaApi.UserProcessor.MarkNewsInboxSeenAsync();
                await InstaApi.UserProcessor.MarkDiscoverMarkSuSeenAsync();

            }
            catch (Exception ex)
            {
                FirstRun =
                  /* IsLoading =*/ false;
                ex.PrintException("ActivitiesViewModel.LoadMoreItemsAsync");
            }
            if (refresh)
                Views.Main.ActivitiesView.Current?.HideTopLoadingYou();
            else
                Views.Main.ActivitiesView.Current?.HideBottomLoadingYou();
            refresh = false;
        }
        public void Refresh(bool flag = false)
        {
            RunLoadMore(true);
            //if (flag)
                NonFollowersVM.RunLoadMore(true);
        }


        //public async void RunLoadMore2(bool refresh = false)
        //{
        //    await RunLoadMoreAsync2(refresh);
        //}
        //public async Task RunLoadMoreAsync2(bool refresh = false)
        //{
        //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        //    {
        //        await LoadMoreItemsAsync2(refresh);
        //    });
        //}
        //async Task LoadMoreItemsAsync2(bool refresh = false)
        //{
        //    if (!HasMoreItems2 && !refresh)
        //    {
        //        IsLoading2 = false;
        //        return;
        //    }
        //    try
        //    {
        //        if (refresh)
        //        {
        //            Pagination2 = PaginationParameters.MaxPagesToLoad(1);
        //            Views.Main.ActivitiesView.Current?.ShowTopLoadingFollowing();
        //        }
        //        else
        //            Views.Main.ActivitiesView.Current?.ShowBottomLoadingFollowing();

        //        var result = await InstaApi.UserProcessor.GetFollowingRecentActivityFeedAsync(Pagination2);
        //        FirstRun2 = false;
        //        Pagination2.MaximumPagesToLoad = 1;
        //        if (!result.Succeeded)
        //        {
        //            IsLoading2 = false;
        //            if (result.Value == null || result.Value?.Items?.Count == 0)
        //            {
        //                if (refresh)
        //                    Views.Main.ActivitiesView.Current?.HideTopLoadingFollowing();
        //                else
        //                    Views.Main.ActivitiesView.Current?.HideBottomLoadingFollowing();
        //                return;
        //            }
        //        }

        //        if (string.IsNullOrEmpty(result.Value.NextMaxId))
        //            HasMoreItems2 = false;

        //        if (result.Value?.Items?.Count > 0)
        //        {
        //            if (refresh)
        //                FollowingItems.Clear();

        //            result.Value.Items.ForEach(item => FollowingItems.Add(item.ToRecentActivityFeed()));
        //        }
        //        await Task.Delay(1000);
        //        IsLoading2 = false;

        //    }
        //    catch (Exception ex)
        //    {
        //        FirstRun2 =
        //           IsLoading2 = false;
        //        ex.PrintException("ActivitiesViewModel.LoadMoreItemsAsync2");
        //    }
        //    if (refresh)
        //        Views.Main.ActivitiesView.Current?.HideTopLoadingFollowing();
        //    else
        //        Views.Main.ActivitiesView.Current?.HideBottomLoadingFollowing();
        //    refresh = false;
        //}
        //public void Refresh2()
        //{
        //    RunLoadMore2(true);
        //}
        //private void ScrollViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        //{
        //    try
        //    {
        //        if (FollowingItems == null)
        //            return;
        //        if (!FollowingItems.Any())
        //            return;
        //        ScrollViewer view = sender as ScrollViewer;

        //        double progress = view.VerticalOffset / view.ScrollableHeight;
        //        if (progress > 0.95 && IsLoading2 == false && !FirstRun2)
        //        {
        //            IsLoading2 = true;
        //            RunLoadMore2();
        //        }
        //    }
        //    catch (Exception ex) { ex.PrintException("Scroll_ViewChanging"); }
        //}

    }
}
