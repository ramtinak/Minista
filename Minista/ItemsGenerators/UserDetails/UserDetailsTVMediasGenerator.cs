using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
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
    public class UserDetailsTVMediasGenerator : BaseModel, IGenerator, IIsMine
    {
        public bool IsMine { get; set; } = false;

        public bool FirstRun = true;
        public ObservableCollection<InstaMedia> Items { get; set; } = new ObservableCollection<InstaMedia>();
        public ObservableCollection<InstaMedia> ItemsX { get; set; } = new ObservableCollection<InstaMedia>();
        public bool HasMoreItems { get; set; } = true;
        public PaginationParameters Pagination { get; private set; }
        private long _userId;
        public long UserId
        {
            get { return _userId; }
            set
            {
                if (value != _userId)
                {
                    ResetCache();
                }
                _userId = value;
            }
        }


        private int PageCount = 1;
        bool IsLoading = true;
        public void ResetCache()
        {
            try
            {
                Items.Clear();
                ItemsX.Clear();
                HasMoreItems = true;
                IsLoading = true;
                FirstRun = true;
                Pagination = PaginationParameters.MaxPagesToLoad(2);
            }
            catch { }
        }
        public void SetLV( ScrollViewer scrollViewer )
        {
            if (scrollViewer == null) return;
            scrollViewer.ViewChanging += Scroll_ViewChanging;
            HasMoreItems = true;
            IsLoading = true;
            Pagination = PaginationParameters.MaxPagesToLoad(3);
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
                    Pagination = PaginationParameters.MaxPagesToLoad(2);
                    try
                    {
                        Views.Infos.UserDetailsView.Current?.ShowTopLoading();
                    }
                    catch { }
                    try
                    {
                        Views.Infos.ProfileDetailsView.Current?.ShowTopLoading();
                    }
                    catch { }
                    //try
                    //{
                    //    Views.Posts.ScrollableUserPostView.Current?.ShowTopLoading();
                    //}
                    //catch { }
                }
                else
                {
                    try
                    {
                        Views.Infos.UserDetailsView.Current?.ShowBottomLoading();
                    }
                    catch { }
                    try
                    {
                        Views.Infos.ProfileDetailsView.Current?.ShowBottomLoading();
                    }
                    catch { }
                    //try
                    //{
                    //    Views.Posts.ScrollableUserPostView.Current?.ShowBottomLoading();
                    //}
                    //catch { }
                }
                var result = await InstaApi.TVProcessor.GetChannelByIdAsync(UserId, Pagination);
                PageCount++;
                FirstRun = false;
                Pagination.MaximumPagesToLoad = 2;
                if (!result.Succeeded)
                {
                    IsLoading = false;
                    if (result.Value == null || result.Value.Items.Count == 0)
                    {
                        Hide(refresh);
                        return;
                    }
                }

                    HasMoreItems = result.Value.MaxId!= null;

                Pagination.NextMaxId = result.Value.MaxId;
                if (refresh)
                {
                    Items.Clear();
                    ItemsX.Clear();
                }
                if (result.Value != null)
                    if( result.Value.Items.Count > 0)
                {
                    Items.AddRange(result.Value.Items);
                    //ItemsX.AddRange(result.Value);
                }
                await Task.Delay(1000);
                IsLoading = false;

            }
            catch (Exception ex)
            {
                FirstRun =
                   IsLoading = false;
                ex.PrintException("UserDetailsTVMediasGenerator.LoadMoreItemsAsync");
            }
            Hide(refresh);
        }
        void Hide(bool refresh)
        {
            if (refresh)
            {
                try
                {
                    Views.Infos.UserDetailsView.Current?.HideTopLoading();
                }
                catch { }
                try
                {
                    Views.Infos.ProfileDetailsView.Current?.HideTopLoading();
                }
                catch { }
                //try
                //{
                //    Views.Posts.ScrollableUserPostView.Current?.HideTopLoading();
                //}
                //catch { }
            }
            else
            {
                try
                {
                    Views.Infos.UserDetailsView.Current?.HideBottomLoading();
                }
                catch { }
                try
                {
                    Views.Infos.ProfileDetailsView.Current?.HideBottomLoading();
                }
                catch { }
                //try
                //{
                //    Views.Posts.ScrollableUserPostView.Current?.HideBottomLoading();
                //}
                //catch { }
            }
        }
        public void Scroll_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                if (Items == null)
                    return;
                if (!Items.Any())
                    return;
                if (!IsMine) return;

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
    }
}
