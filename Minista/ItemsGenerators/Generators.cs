////////MinHeight="430" MinWidth="400"
////////zamani ke tu facebook login be challenge mikhore, panele fb baste nemishe,
////////tu challenge ham bad az inke pass mishe, bazam message no error detected miad:|

//5 new posts in account search?😐 pashmaam
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
    public class MainPostsGenerator : BaseModel, IGenerator
    {
        public bool FirstRun = true;
        public ObservableCollection<InstaPost> Items { get; set; } = new ObservableCollection<InstaPost>();
        //public ObservableCollection<InstaMedia> Items { get; set; } = new ObservableCollection<InstaMedia>();
        public bool HasMoreItems { get; set; } = true;
        public PaginationParameters Pagination { get; private set; }

        ScrollViewer Scroll;
        //PullToRefreshListView LV;
        bool IsLoading = true;
        InstaFeedPaginationSource PaginationSource = InstaFeedPaginationSource.None;
        public MainPostsGenerator() { }
        public void SetLV(/*PullToRefreshListView listView*/ ScrollViewer scrollViewer /*Controls.PullToRefreshPanel pullToRefreshPanel*/)
        {
            //LV = listView;
            Scroll = scrollViewer;
            //Scroll = listView.FindScrollViewer();
            //Scroll = pullToRefreshPanel.FindScrollViewer();
            if (Scroll != null)
                Scroll.ViewChanging += Scroll_ViewChanging;
            HasMoreItems = true;
            IsLoading = true;
            Pagination = PaginationParameters.MaxPagesToLoad(1);
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
            if (!HasMoreItems&& !refresh)
            {
                IsLoading = false;
                return;
            }
            try
            {
                if (refresh)
                {
                    Views.Main.MainView.Current?.ShowTopLoading();
                    Pagination = PaginationParameters.MaxPagesToLoad(1);
                }
                else
                    Views.Main.MainView.Current?.ShowBottomLoading();
                IResult<InstaFeed> result;
                if (refresh || FirstRun)
                {
                    var seenIds = new List<string>();

                    if (Items.Count > 0)
                    {
                        for (int i = 0; i < Items.Count; i++)
                        {
                            var m = Items[i];
                            if (m.Media != null)
                                seenIds.Add(m.Media.InstaIdentifier);
                       }
                    }
                    result = await InstaApi.FeedProcessor
                        .GetUserTimelineFeedAsync(Pagination, seenIds.ToArray(), true,
                        SettingsHelper.Settings.RemoveAds, PaginationSource = InstaFeedPaginationSource.None);
                }
                else
                    result = await InstaApi.FeedProcessor.GetUserTimelineFeedAsync(Pagination, null, false, SettingsHelper.Settings.RemoveAds, PaginationSource);

                FirstRun = false;
                if (!result.Succeeded)
                {
                    IsLoading = false;
                    if (result.Value == null || result.Value?.Medias?.Count == 0)
                    {
                        if (refresh)
                            Views.Main.MainView.Current?.HideTopLoading();
                        else
                            Views.Main.MainView.Current?.HideBottomLoading();
                        refresh = false;
                        return;
                    }
                }

                if (result.Value.NextMaxId == null)
                    HasMoreItems = false;

                Pagination.NextMaxId = result.Value.NextMaxId;
                if (result.Value.Posts?.Count > 0)
                {
                    if (refresh) Items.Clear();
                    InstaAllCatchedUp decorator = null;
                    for (int i = 0; i < result.Value.Posts.Count; i++)
                    {
                        var item = result.Value.Posts[i];
                        if (item.Type == InstaFeedsType.Media && item.Media != null)
                            item.Media.IsMain = true;
                        else if (item.EndOfFeedDemarcator != null && item.Type == InstaFeedsType.EndOfFeedDemarcator)
                            decorator = item.EndOfFeedDemarcator;
                    }
                    if(Pagination.NextMaxId == "feed_recs_head_load" && decorator != null)
                    {
                        if (decorator.GroupSet?.Groups?.Count > 0)
                        {
                            var def = decorator.GroupSet.Groups.FirstOrDefault(x => x.Id == "past_posts");
                            if (def != null)
                            {
                                Pagination.NextMaxId = def.NextMaxId;
                                PaginationSource = InstaFeedPaginationSource.PastPosts;
                                try
                                {
                                    if (def.FeedItems?.Count > 0)
                                    {
                                        for (int i = 0; i < def.FeedItems.Count; i++)
                                            result.Value.Posts.Add(new InstaPost
                                            {
                                                Media = def.FeedItems[i],
                                                Type = InstaFeedsType.Media
                                            });
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    Items.AddRange(result.Value.Posts);

    

                    try
                    {
                        //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async() =>
                        //{
                            //foreach (var item in Items)
                            for(int i = 0; i< Items.Count;i++)
                            {
                                var item = Items[i];
                                if (item.Type == InstaFeedsType.StoriesNetego && item.StoriesNetego != null)
                                {
                                    if (item.Stories?.Count == 0)
                                    {
                                        var usersList = new List<string>();

                                        if (item.StoriesNetego.ReelIds?.Count > 0)
                                            usersList.AddRange(item.StoriesNetego.ReelIds);
                                        else
                                        {
                                            if (string.IsNullOrEmpty(item.StoriesNetego.Title))
                                            {
                                                for (int j = 0; j < result.Value.Medias.Count; j++)
                                                    //foreach (var userItem in result.Value.Medias)
                                                    usersList.Add(result.Value.Medias[j].User.Pk.ToString());
                                                item.StoriesNetego.Title = "Recent stories";
                                            }
                                        }
                                        if (usersList.Count > 0)
                                        {
                                            if (usersList.Count > 4)
                                            {
                                                var list = usersList.ToList();
                                                usersList.Clear();
                                                var take5 = list.Take(5);
                                                for (int k = 0; k < take5.Count(); k++)
                                                    //foreach (var xxxx in list.Take(5))
                                                    usersList.Add(take5.ElementAt(k));
                                            }
                                            var storiesAfterResult = await InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(usersList.ToArray());
                                            if (storiesAfterResult.Succeeded)
                                                item.Stories.AddRange(storiesAfterResult.Value.Items);
                                        }
                                        break;

                                    }
                                }
                            }
                        //});
                    }
                    catch { }

                    if (Items.Count == 1)
                    {
                        if (Items[0].Type == InstaFeedsType.SuggestedUsersCard)
                        {
                            Items[0].SelectedIndex = 0;
                        }
                    }
                }
                await Task.Delay(1000);
                IsLoading = false;

            }
            catch (Exception ex)
            {
                FirstRun =
                   IsLoading = false;
                ex.PrintException("MainPostsGenerator.LoadMoreItemsAsync");
            }
            if (refresh)
                Views.Main.MainView.Current?.HideTopLoading();
            else
                Views.Main.MainView.Current?.HideBottomLoading();
            refresh = false;
        }

        private void Scroll_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
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
                    // await RunLoadMoreAsync();
                    RunLoadMore();
                }
                //    if (Scroll.VerticalOffset > 0 && Scroll.ScrollableHeight> 150)
                //    {
                //        if (Scroll.VerticalOffset >= Scroll.ScrollableHeight - 140 && IsLoading == false && !FirstRun)
                //        {
                //            IsLoading = true;
                //            await RunLoadMoreAsync();
                //        }
                //    }
            }
            catch (Exception ex) { ex.PrintException("Scroll_ViewChanging"); }
        }
        public async void MuteRequested(long userPk)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Items.Count > 0)
                        foreach (var item in Items.ToList())
                        {
                            if (item.Type == InstaFeedsType.Media && item.Media != null)
                            {
                                if (item.Media.User.Pk == userPk)
                                    Items.Remove(item);
                            }
                        }
                });
            }
            catch { }
        }
        public async void MuteHashtagRequested(string tagName)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Items.Count > 0)
                        foreach (var item in Items.ToList())
                        {
                            if (item.Type == InstaFeedsType.Media && item.Media != null)
                            {
                                if (item.Media.FollowHashtagInfo != null)
                                {
                                    if (item.Media.FollowHashtagInfo.Name.ToLower() == tagName.ToLower())
                                        Items.Remove(item);
                                }
                            }
                        }
                });
            }
            catch { }
        }
        public async void RemoveItemRequested(string mediaId)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Items.Count > 0)
                        foreach (var item in Items.ToList())
                        {
                            if (item.Media?.InstaIdentifier == mediaId)
                            {
                                Items.Remove(item);
                                return;
                            }
                        }
                });
            }
            catch { }
        }
        public async void DontShowThisItemRequested(string mediaId)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Items.Count > 0)
                        foreach (var item in Items.ToList())
                        {
                            if (item.Media?.InstaIdentifier == mediaId)
                            {
                                item.Media.DontShowHashtagLikeThis = true;
                                return;
                            }
                        }
                });
            }
            catch { }
        }
    }
}
