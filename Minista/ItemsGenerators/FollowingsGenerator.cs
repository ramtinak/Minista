using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Minista.ViewModels.Infos;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using static Helper;
#pragma warning disable IDE0044	
#pragma warning disable IDE0051
#pragma warning disable IDE0052
#pragma warning disable IDE0060	
#pragma warning disable CS0618	

namespace Minista.ItemsGenerators
{
    public class FollowingsGenerator : BaseModel, IGenerator
    {
        public bool FirstRun = true;
        public ObservableCollection<InstaUserShortFriendship> Items { get; set; } = new ObservableCollection<InstaUserShortFriendship>();
        public ObservableCollection<InstaUserShortFriendship> SearchItems { get; set; } = new ObservableCollection<InstaUserShortFriendship>();

        public bool HasMoreItems { get; set; } = true;
        public bool HasMoreItems2 { get; set; } = true;

        public PaginationParameters Pagination { get; private set; } = PaginationParameters.MaxPagesToLoad(1);
        public PaginationParameters Pagination2 { get; private set; } = PaginationParameters.MaxPagesToLoad(1);

        private string searchWord_ = null;
        public string SearchWord { get { return searchWord_; } set { searchWord_ = value; OnPropertyChanged("SearchWord"); } }
        public long UserId { get; private set; }
        private int PageCount = 1;
        ScrollViewer Scroll;
        bool IsLoading = true;
        ScrollViewer Scroll2;
        bool IsLoading2 = true;

        private InstaFollowOrderType orderType_;
        public InstaFollowOrderType OrderType { get { return orderType_; } set { orderType_ = value; RunLoadMore(true); } }

        int selectedIndex_ = 0;
        public int SelectedIndex
        {
            get { return selectedIndex_; }
            set
            {
                if (value != selectedIndex_)
                    Pagination = PaginationParameters.MaxPagesToLoad(1);
                else return;
                selectedIndex_ = value;
                OnPropertyChanged("SelectedIndex");
                switch(value)
                {
                    case 0:
                        OrderType = InstaFollowOrderType.Default;
                        break;
                    case 1:
                        OrderType = InstaFollowOrderType.DateFollowedEarliest;
                        break;
                    case 2:
                        OrderType = InstaFollowOrderType.DateFollowedLatest;
                        break;
                }
            }
        }
        public void SetUserId(long pk)
        {
            if (pk != UserId)
            {
                Items.Clear();
                SearchItems.Clear();
                FirstRun = true;
                SearchWord = null;
                Pagination = PaginationParameters.MaxPagesToLoad(1);
                Pagination2 = PaginationParameters.MaxPagesToLoad(1);
                HasMoreItems = true;
                IsLoading = true;

                HasMoreItems2 = true;
                IsLoading2 = true;
            }
            UserId = pk;
        }
        public void SetLV(ScrollViewer scrollViewer)
        {
            Scroll = scrollViewer;

            if (Scroll != null)
            {
                Scroll.ViewChanging += ScrollViewChanging;
                HasMoreItems = true;
                IsLoading = true;
            }
        }
        public void SetSearchLV(ScrollViewer scrollViewer)
        {
            Scroll2 = scrollViewer;

            if (Scroll2 != null)
            {
                Scroll2.ViewChanging += Scroll2ViewChanging;
                HasMoreItems2 = true;
                IsLoading2 = true;
            }
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
                    PageCount = 1;
                    Pagination = PaginationParameters.MaxPagesToLoad(1);
                    Views.Infos.FollowView.Current?.ShowTopLoadingFollowing();
                }
                else
                    Views.Infos.FollowView.Current?.ShowBottomLoadingFollowing();
                var result = await InstaApi.UserProcessor.GetUserFollowingByIdAsync(UserId, Pagination);
         
                PageCount++;
                FirstRun = false;
                Pagination.MaximumPagesToLoad = 2;
                if (!result.Succeeded)
                {
                    IsLoading = false;
                    if (result.Value == null || result.Value?.Count == 0)
                    {
                        if (refresh)
                            Views.Infos.FollowView.Current?.HideTopLoadingFollowing();
                        else
                            Views.Infos.FollowView.Current?.HideBottomLoadingFollowing();
                        return;
                    }
                }

                if (string.IsNullOrEmpty(result.Value.NextMaxId))
                    HasMoreItems = false;

                Pagination.NextMaxId = result.Value.NextMaxId;
                if (refresh) Items.Clear();
                var userIds = new List<long>();
                if (result.Value?.Count > 0)
                {
                    result.Value.ForEach(x =>
                    {
                        userIds.Add(x.Pk);
                        Items.Add(x.ToUserShortFriendship());
                    });
                }
                try
                {
                    if (userIds.Count > 0)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            var friendshipStatuses = await InstaApi.UserProcessor.GetFriendshipStatusesAsync(userIds.ToArray());
                            if (friendshipStatuses.Succeeded)
                            {
                                var friends = friendshipStatuses.Value;
                                friends.ForEach(x =>
                                {
                                    var t = Items.FirstOrDefault(u => u.Pk == x.Pk);
                                    if (t != null)
                                        t.FriendshipStatus = x;
                                });
                            }
                        });
                    }
                }
                catch { }
                await Task.Delay(1000);
                IsLoading = false;
            }
            catch (Exception ex)
            {
                FirstRun =
                   IsLoading = false;
                ex.PrintException("FollowingGenerator.LoadMoreItemsAsync");
            }

            if (refresh)
                Views.Infos.FollowView.Current?.HideTopLoadingFollowing();
            else
                Views.Infos.FollowView.Current?.HideBottomLoadingFollowing();
        }

        private void ScrollViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                if (Items == null)
                    return;
                if (!Items.Any())
                    return;
                ScrollViewer view = sender as ScrollViewer;

                double progress = view.VerticalOffset / view.ScrollableHeight;
                if (progress > 0.95 && IsLoading == false && !FirstRun)
                {
                    IsLoading = true;
                    RunLoadMore();
                }
            }
            catch (Exception ex) { ex.PrintException("Scroll_ViewChanging"); }
        }














        public async void RunLoadMore2(bool refresh = false)
        {
            await RunLoadMoreAsync2(refresh);
        }
        public async Task RunLoadMoreAsync2(bool refresh = false)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await LoadMoreItemsAsync2(refresh);
            });
        }
        async Task LoadMoreItemsAsync2(bool refresh = false)
        {
            if (!HasMoreItems2 && !refresh)
            {
                IsLoading2 = false;
                return;
            }
            try
            {
                if (refresh)
                {
                    Pagination2 = PaginationParameters.MaxPagesToLoad(1);

                    Views.Infos.FollowView.Current?.ShowTopLoadingFollowing();
                }
                else
                    Views.Infos.FollowView.Current?.ShowBottomLoadingFollowing();
                var result = await InstaApi.UserProcessor.GetUserFollowingByIdAsync(UserId, Pagination2, SearchWord);
                Pagination2.MaximumPagesToLoad = 2;
                if (!result.Succeeded)
                {
                    IsLoading2 = false;
                    if (result.Value == null || result.Value?.Count == 0)
                    {
                        if (refresh)
                            Views.Infos.FollowView.Current?.HideTopLoadingFollowing();
                        else
                            Views.Infos.FollowView.Current?.HideBottomLoadingFollowing();
                        return;
                    }
                }

                if (string.IsNullOrEmpty(result.Value.NextMaxId))
                    HasMoreItems2 = false;

                Pagination2.NextMaxId = result.Value.NextMaxId;
                if (refresh) SearchItems.Clear();
                if (result.Value?.Count > 0)
                    result.Value.ForEach(x => SearchItems.Add(x.ToUserShortFriendship()));

                await Task.Delay(1000);
                IsLoading2 = false;
            }
            catch (Exception ex)
            {
                IsLoading2 = false;
                ex.PrintException("FollowersGenerator.LoadMoreItemsAsync");
            }

            if (refresh)
                Views.Infos.FollowView.Current?.HideTopLoadingFollowing();
            else
                Views.Infos.FollowView.Current?.HideBottomLoadingFollowing();
        }

        private void Scroll2ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                if (SearchItems == null)
                    return;
                if (!SearchItems.Any())
                    return;
                ScrollViewer view = sender as ScrollViewer;

                double progress = view.VerticalOffset / view.ScrollableHeight;
                if (progress > 0.95 && IsLoading2 == false && !FirstRun)
                {
                    IsLoading2 = true;
                    RunLoadMore2();
                }
            }
            catch (Exception ex) { ex.PrintException("Scroll_ViewChanging"); }
        }
    }
}
