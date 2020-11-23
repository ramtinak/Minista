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
    public class HashtagsRecentGenerator : BaseModel, IGenerator
    {
        public bool IsMine { get; set; } = false;
        public InstaHashtagSectionType HashtagType = InstaHashtagSectionType.Recent;

        public bool FirstRun = true;
        public ObservableCollection<InstaMedia> Items { get; set; } = new ObservableCollection<InstaMedia>();
        public ObservableCollection<InstaMedia> ItemsX { get; set; } = new ObservableCollection<InstaMedia>();
        public bool HasMoreItems { get; set; } = true;
        public PaginationParameters Pagination { get; private set; } = PaginationParameters.MaxPagesToLoad(1);

        private int PageCount = 1;
        ScrollViewer Scroll;
        bool IsLoading = true;
        public string Hashtag { get; set; }
        public HashtagViewModel HashtagVM { get; set; }
        public void SetVM(HashtagViewModel model)
        {
            HashtagVM = model;
        }
        public void SetLV(ScrollViewer scrollViewer)
        {
            if (Scroll == null)
            {
                Scroll = scrollViewer;
                if (Scroll == null) return;
                Scroll.ViewChanging += ScrollViewChanging;
                HasMoreItems = true;
                IsLoading = true;
            }
        }
        public void SetLV2(ScrollViewer scrollViewer)
        {
            if (scrollViewer == null) return;

            scrollViewer.ViewChanging += ScrollViewChanging;
            HasMoreItems = true;
            IsLoading = true;
        }
        public void ResetCache()
        {
            Hashtag = null;
            PageCount = 1;
            Pagination = PaginationParameters.MaxPagesToLoad(1);
            IsLoading = true;
            HasMoreItems = true;
            Items.Clear();
            ItemsX.Clear();
            FirstRun = true;
            IsMine = false;
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
            if(!IsMine) return;
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
                    try
                    {
                        Views.Infos.HashtagView.Current?.ShowTopLoading();
                    }
                    catch { }
                    //try
                    //{
                    //    Views.Posts.ScrollableHashtagPostView.Current?.ShowTopLoading();
                    //}
                    //catch { }
                }
                else
                {
                    try
                    {
                        Views.Infos.HashtagView.Current?.ShowBottomLoading();
                    }
                    catch { }
                    //try
                    //{
                    //    Views.Posts.ScrollableHashtagPostView.Current?.ShowBottomLoading();
                    //}
                    //catch { }
                }
                var result = await InstaApi.HashtagProcessor.GetHashtagsSectionsAsync(Hashtag, Pagination, HashtagType);

                PageCount++;
                FirstRun = false;
                Pagination.MaximumPagesToLoad = 1;
                if (!result.Succeeded)
                {
                    IsLoading = false;
                    if (result.Value == null || result.Value.Medias?.Count == 0)
                    {
                        Hide(refresh);
                        return;
                    }
                }

                HasMoreItems = result.Value.MoreAvailable;

                Pagination.NextMaxId = result.Value.NextMaxId;
                if (refresh) Items.Clear();
                if (result.Value.Medias?.Count > 0)
                {
                    Items.AddRange(result.Value.Medias);
                    //ItemsX.AddRange(result.Value.Medias);
                }

                await Task.Delay(1000);
                IsLoading = false;
            }
            catch (Exception ex)
            {
                FirstRun =
                   IsLoading = false;
                ex.PrintException("HashtagsRecentGenerator.LoadMoreItemsAsync");
            }
            Hide(refresh);
        }
        void Hide(bool refresh)
        {
            if (refresh)
            {
                try
                {
                    Views.Infos.HashtagView.Current?.HideTopLoading();
                }
                catch { }
                //try
                //{
                //    Views.Posts.ScrollableHashtagPostView.Current?.HideTopLoading();
                //}
                //catch { }
            }
            else
            {
                try
                {
                    Views.Infos.HashtagView.Current?.HideBottomLoading();
                }
                catch { }
                //try
                //{
                //    Views.Posts.ScrollableHashtagPostView.Current?.HideBottomLoading();
                //}
                //catch { }
            }
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
                if (progress > 0.95 && IsLoading == false && !FirstRun && IsMine)
                {
                    IsLoading = true;
                    RunLoadMore();
                }
            }
            catch (Exception ex) { ex.PrintException("Scroll_ViewChanging"); }
        }


    }
}
