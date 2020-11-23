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
using Minista.ItemsGenerators;
using Minista.ViewModels.Infos;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using static Helper;
#pragma warning disable IDE0044	
#pragma warning disable IDE0051
#pragma warning disable IDE0052
#pragma warning disable IDE0060	
#pragma warning disable CS0618	

namespace Minista.ViewModels.TV
{
    public class FooterViewModel : BaseModel, IGenerator
    {
        public bool FirstRun = true;
        public ObservableCollection<InstaMedia> Items { get; set; } = new ObservableCollection<InstaMedia>();
        public bool HasMoreItems { get; set; } = true;
        public PaginationParameters Pagination { get; private set; } = PaginationParameters.MaxPagesToLoad(1);
        private int PageCount = 1;
        bool IsLoading = true;
        public InstaUserShort User = null;
        public InstaMedia CurrentMedia = null;
        public int SelectedIndex = -1;
        public void SetData(List<InstaMedia> medias, int selectedIndex = -1)
        {
            try
            {
                User = null;
                Items.Clear();
                Items.AddRange(medias); 
                Pagination = PaginationParameters.MaxPagesToLoad(1);
                SelectedIndex = selectedIndex;
            }
            catch { }
        }
        public void SetData(InstaUserShort user, List<InstaMedia> medias, int selectedIndex = -1)
        {
            try
            {
                User = user;
                Items.Clear();
                Items.AddRange(medias);
                Pagination = PaginationParameters.MaxPagesToLoad(1);
                SelectedIndex = selectedIndex;
            }
            catch { }
        }
        public void SetLV(ScrollViewer scrollViewer)
        {
            if (scrollViewer != null)
            {

                scrollViewer.ViewChanging += ScrollViewChanging;
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
            User = null;
            CurrentMedia = null;
            SelectedIndex = -1;
            PageCount = 1;
            Pagination = PaginationParameters.MaxPagesToLoad(1);
            IsLoading = true;
            HasMoreItems = true;
            Items.Clear();
            FirstRun = true;
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
                    //try
                    //{
                    //    Views.Infos.HashtagView.Current?.ShowTopLoading();
                    //}
                    //catch { }
                    //try
                    //{
                    //    Views.Posts.ScrollableHashtagPostView.Current?.ShowTopLoading();
                    //}
                    //catch { }
                }
                else
                {
                    //try
                    //{
                    //    Views.Infos.HashtagView.Current?.ShowBottomLoading();
                    //}
                    //catch { }
                    //try
                    //{
                    //    Views.Posts.ScrollableHashtagPostView.Current?.ShowBottomLoading();
                    //}
                    //catch { }
                }
                if (User != null)
                {
                    var result = await InstaApi.TVProcessor.GetChannelByIdAsync(User.Pk, Pagination);

                    PageCount++;
                    FirstRun = false;
                    if (!result.Succeeded)
                    {
                        IsLoading = false;
                        if (result.Value == null || result.Value.Items?.Count == 0)
                        {
                            Hide(refresh);
                            return;
                        }
                    }

                    HasMoreItems = result.Value.HasMoreAvailable;

                    Pagination.NextMaxId = result.Value.MaxId;
                    if (refresh) Items.Clear();
                    if (result.Value.Items?.Count > 0)
                    {
                        Items.AddRange(result.Value.Items);
                    }
                }
                else
                {
                    var result = await InstaApi.TVProcessor.BrowseFeedAsync(Pagination);

                    PageCount++;
                    FirstRun = false;
                    if (!result.Succeeded)
                    {
                        IsLoading = false;
                        if (result.Value == null || result.Value.BrowseItems?.Count == 0)
                        {
                            Hide(refresh);
                            return;
                        }
                    }

                    HasMoreItems = result.Value.MoreAvailable;

                    Pagination.NextMaxId = result.Value.MaxId;
                    if (refresh) Items.Clear();
                    if (result.Value.BrowseItems?.Count > 0)
                    {
                        Items.AddRange(result.Value.BrowseItems);
                    }
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
                double progress = view.HorizontalOffset / view.ScrollableWidth;
                if (progress > 0.90 && IsLoading == false && !FirstRun)
                {
                    IsLoading = true;
                    RunLoadMore();
                }
            }
            catch (Exception ex) { ex.PrintException("Scroll_ViewChanging"); }
        }
    }
}
