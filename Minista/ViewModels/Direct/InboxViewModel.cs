using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Helpers;
using InstagramApiSharp.Enums;
using static Helper;
using System.Collections.ObjectModel;
using Minista.ItemsGenerators;
using Windows.UI.Core;
using Minista.UserControls.Direct;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Minista.ContentDialogs;
using InstagramApiSharp.API.RealTime.Responses.Models;
using Minista.Views.Direct;

namespace Minista.ViewModels.Direct
{
    public class InboxViewModel : BaseModel, IGenerator
    {
        public event EventHandler Updated;
        private static InboxViewModel _inboxViewModel;
        internal static InboxViewModel Instance
        {
            get
            {
                if (_inboxViewModel == null)
                    _inboxViewModel = new InboxViewModel();

                return _inboxViewModel;
            }
        }
        public static void ResetInstance() => _inboxViewModel = null;

        private Visibility _searchVisibility = Visibility.Collapsed;
        public Visibility SearchVisibility { get { return _searchVisibility; } set { _searchVisibility = value; OnPropertyChanged("SearchVisibility"); } }

        private Visibility _directRequestsVisibility = Visibility.Collapsed;
        public Visibility DirectRequestsVisibility { get { return _directRequestsVisibility; } set { _directRequestsVisibility = value; OnPropertyChanged("DirectRequestsVisibility"); } }
        private int _directRequestsCount = 0;
        public int DirectRequestsCount
        {
            get { return _directRequestsCount; }
            set
            {
                _directRequestsCount = value;
                if (value <= 0)
                {
                    DirectRequestsVisibility = Visibility.Collapsed;
                    DirectRequestsCountText = "";
                }
                else
                {
                    if (value == 1)
                        DirectRequestsCountText = $"{value} Request";
                    else
                        DirectRequestsCountText = $"{value} Requests";

                    DirectRequestsVisibility = Visibility.Visible;
                }
            }
        }
        public int SeqId { get; set; } = 0;
        public DateTime SnapshotAt { get; set; }
        private string _directRequestsCountText = "";
        public string DirectRequestsCountText { get { return _directRequestsCountText; } set { _directRequestsCountText = value; OnPropertyChanged("DirectRequestsCountText"); } }
        //public ObservableCollection<InstaDirectInboxThread> Threads { get; set; } = new ObservableCollection<InstaDirectInboxThread>();
        public ObservableCollection<DirectInboxUc> Items { get; set; } = new ObservableCollection<DirectInboxUc>();
        public ObservableCollection<DirectInboxUc> ItemsSearch { get; set; } = new ObservableCollection<DirectInboxUc>();
        public PaginationParameters Pagination { get; set; } = PaginationParameters.MaxPagesToLoad(1);
        public InstaDirectInboxContainer InboxContainer { get; private set; }
        public bool HasMoreItems { get; set; } = true;
        //bool FirstTime = true;
        //ScrollViewer Scroll;
        private bool IsLoading = false;
        public void SetLV(/*ListView listView*/ ScrollViewer scroll)
        {
            //Scroll = listView.FindScrollViewer();
            if (scroll != null)
            {
                //Scroll = scroll;
                scroll.ViewChanging -= ScrollViewChanging;
                scroll.ViewChanging += ScrollViewChanging;
            }
            HasMoreItems = true;
            IsLoading = true;
            if(Pagination == null)
            Pagination = PaginationParameters.MaxPagesToLoad(1);
        }
        public async void RunLoadMore(bool refresh = false)
        {
            await RunLoadMoreAsync(refresh);
        }
        public async Task RunLoadMoreAsync(bool refresh)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await LoadMoreItemsAsync(refresh);
            });
        }
        async Task LoadMoreItemsAsync(bool refresh = false)
        {
            if (Helper.DontUseTimersAndOtherStuff) return;
            if (!HasMoreItems && !refresh)
            {
                IsLoading = false;
                return;
            }
            try
            {
                if (refresh)
                {
                    Pagination = PaginationParameters.MaxPagesToLoad(2);

                    Views.Direct.InboxView.Current?.ShowTopLoading();
                }
                else
                    Views.Direct.InboxView.Current?.ShowBottomLoading();

                var result = await InstaApi.MessagingProcessor.GetDirectInboxAsync(Pagination);
                Pagination.MaximumPagesToLoad = 1;
                if (!result.Succeeded)
                {
                    if (result.Value == null)
                    {
                        IsLoading = false;

                        if (refresh)
                            Views.Direct.InboxView.Current?.HideTopLoading();
                        else
                            Views.Direct.InboxView.Current?.HideBottomLoading();
                        return;
                    }
                }
                DirectRequestsCount = result.Value.PendingRequestsCount;
                HasMoreItems = result.Value.Inbox.HasOlder;
                SeqId = result.Value.SeqId;
                SnapshotAt = result.Value.SnapshotAt;
                if (result.Value?.Inbox?.Threads?.Count > 0)
                {
                    //if (FirstTime)
                    if (refresh)
                    {
                        Items.Clear();
                        InboxContainer = result.Value;
                    }

                    //if()
                    {
                        for (int i = 0; i < result.Value.Inbox.Threads.Count; i++)
                            Append(result.Value.Inbox.Threads[i]);
                    }
                    //else
                    //{
                    //    for (int i = 0; i < result.Value.Inbox.Threads.Count; i++)
                    //    {
                    //        var item = result.Value.Inbox.Threads[i];
                    //        Append(item, false, i);
                    //    }
                    //}
                    MainPage.Current?.SetDirectMessageCount(result.Value);
                }
                GetUserPresense();
                await Task.Delay(1000);
                //refresh  = false;

            }
            catch (Exception ex)
            {
                ex.PrintException("InboxViewModel.LoadMoreItemsAsync");
            }

            if (refresh)
                Views.Direct.InboxView.Current?.HideTopLoading();
            else
                Views.Direct.InboxView.Current?.HideBottomLoading();
            IsLoading = false;
            Updated?.Invoke(this, null);
        }
        public void Refresh()
        {
            RunLoadMore(true);
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
                if (progress > 0.75 && IsLoading == false) 
                {
                    IsLoading = true;
                    RunLoadMore();
                }
            }
            catch (Exception ex) { ex.PrintException("Scroll_ViewChanging"); }
        }
        public async void Search(string searchWord)
        {
            try
            {
                if(string.IsNullOrEmpty(searchWord))
                {
                    ItemsSearch.Clear();
                    SearchVisibility = Visibility.Collapsed;
                    return;
                }
                searchWord = searchWord.ToLower().Trim();
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        if (Items.Count > 0)
                        {
                            ItemsSearch.Clear();
                            var list = Items.Where(x => x.Thread.Users.Any(u => u.UserName.ToLower().Contains(searchWord) ||
                            u.FullName.ToLower().Contains(searchWord)) || x.Thread.Title.ToLower().Contains(searchWord)).ToList();
                            SearchVisibility = Visibility.Visible;
                            if (list.Count > 0)
                            {
                                for (int i = 0; i < list.Count; i++)
                                    ItemsSearch.Add(GetDirectInboxUc(list[i].Thread));
                            }
                        }
                    }
                    catch { }

                });
            }
            catch { }
        }


        internal void RealtimeClientClientTypingChanged(object sender, List<InstaRealtimeTypingEventArgs> e)
        {
            try
            {
                if (Helper.DontUseTimersAndOtherStuff) return;
                //string currentThreadId = ThreadView.Current?.ThreadVM?.CurrentThread?.ThreadId;
                if (e?.Count > 0)
                {
                    var typingList = new List<string>();
                    for (int i = 0; i < e.Count; i++)
                    {
                        var item = e[i];
                        // /direct_v2/threads/340282366841710300949128154931298634193/activity_indicator_id/6685320955800332013
                        var start = "direct_v2/threads/";
                        if (item.RealtimePath?.Contains(start) ?? false)
                        {
                            var threadId = item.RealtimePath.Substring(item.RealtimePath.IndexOf(start) + start.Length);
                            threadId = threadId.Substring(0, threadId.IndexOf("/"));
                            if (ThreadView.Current != null && ThreadView.Current?.ThreadVM?.CurrentThread != null)
                            {
                                if (threadId == ThreadView.Current.ThreadVM.CurrentThread.ThreadId)
                                {
                                    try
                                    {
                                        var user = ThreadView.Current.ThreadVM.CurrentThread
                                            .Users.FirstOrDefault(u => u.Pk == long.Parse(item.SenderId));
                                        typingList.Add(user.UserName.ToLower());
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    if (typingList?.Count > 0)
                        ShowTyping(string.Join(",", typingList));
                }
            }
            catch { }
        }
        async void ShowTyping(string text)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    ThreadView.Current.ThreadVM.TypingText = text + " is typing";
                    ThreadView.Current.ThreadVM.TypingVisibility = Visibility.Visible;
                    await Task.Delay(4000);
                    HideTyping();
                });
            }
            catch { }
        }
        public void HideTyping()=> ThreadView.Current.ThreadVM.TypingVisibility = Visibility.Collapsed;
        internal void RealtimeClientDirectItemChanged(object sender, List<InstaDirectInboxItem> e)
        {
            try
            {
                if (DontUseTimersAndOtherStuff) return;
                if (e?.Count > 0)
                {
                    for (int i = 0; i < e.Count; i++)
                    {
                        try
                        {
                            var item = e[i];
                            ///direct_v2/threads/340282366841710300949128154931298634193/items/
                            var start = "direct_v2/threads/";
                            if (item.RealtimePath?.Contains(start) ?? false)
                            {
                                var threadId = item.RealtimePath.Substring(item.RealtimePath.IndexOf(start) + start.Length);
                                threadId = threadId.Substring(0, threadId.IndexOf("/"));
                                if (item.RealtimePath?.Contains("/items/") ?? false && !item.HideInThread)
                                {
                                    var itemId = item.RealtimePath.Substring(item.RealtimePath.IndexOf("/items/") + "/items/".Length);
                                    if (itemId.Contains("/"))
                                        itemId = itemId.Substring(0, itemId.IndexOf("/"));
                                    if (ThreadView.Current != null && ThreadView.Current?.ThreadVM?.CurrentThread != null)
                                    {
                                        if (threadId == ThreadView.Current.ThreadVM.CurrentThread.ThreadId)
                                        {
                                            if (item.RealtimeOp == "remove")
                                                ThreadView.Current.ThreadVM.RemoveItem(itemId);
                                            else
                                                ThreadView.Current.ThreadVM.AddItem(item);
                                            HideTyping();
                                        }
                                    }
                                    try
                                    {
                                        GoFirst(threadId, item);
                                    }
                                    catch { }
                                }
                                else if (item.RealtimePath?.Contains("/participants/") ?? false)
                                {
                                    if (item.RealtimePath?.Contains("/has_seen") ?? false)
                                    {
                                        if (item.RealtimeOp == "replace")
                                            SetSeenFromRealtime(threadId, item);
                                    }
                                }
                            }
                        } 
                        catch { }
                    }
                }
            }
            catch { }
        }

        public void SetSeenFromRealtime(string threadId, InstaDirectInboxItem inboxItem)
        {
            try
            {
                if (ThreadView.Current != null && ThreadView.Current?.ThreadVM?.CurrentThread != null)
                {
                    if (ThreadView.Current.ThreadVM.CurrentThread.ThreadId == threadId)
                    {
                        //var first = ThreadView.Current.ThreadVM.Items.FirstOrDefault(x => x.ItemId == inboxItem.ItemId);
                        //var findIndex = ThreadView.Current.ThreadVM.Items.IndexOf(first);
                        //if (findIndex != -1)
                        {

                            for (int i = 0; i < ThreadView.Current.ThreadVM.Items.Count; i++)
                            {
                                try
                                {
                                    if (ThreadView.Current.ThreadVM.Items[i].SendingType != InstaDirectInboxItemSendingType.Seen &&
                                       ThreadView.Current.ThreadVM.Items[i].UserId == CurrentUser.Pk)
                                    ThreadView.Current.ThreadVM.Items[i].SendingType = InstaDirectInboxItemSendingType.Seen;
                                }
                                catch { }
                            }
                            try
                            {
                                var first = ThreadView.Current.ThreadVM.CurrentThread.LastSeenAt.FirstOrDefault(x => x.PK != CurrentUser.Pk);

                                if (first != null)
                                {
                                    first.ItemId = inboxItem.ItemId;
                                    first.SeenTime = inboxItem.TimeStamp;
                                }
                                else
                                {
                                    ThreadView.Current.ThreadVM.CurrentThread.LastSeenAt.Add(new InstaLastSeen
                                    {
                                        PK = ThreadView.Current.ThreadVM.CurrentThread.Users[0].Pk,
                                        ItemId = inboxItem.ItemId,
                                        SeenTime = inboxItem.TimeStamp
                                    });
                                }
                            }
                            catch { }
                            try
                            {
                                var thread = Items.FirstOrDefault(x => x.Thread.ThreadId == threadId);
                                if (thread != null)
                                {
                                    var first = thread.Thread.LastSeenAt.FirstOrDefault(x => x.PK != CurrentUser.Pk);

                                    if (first != null)
                                    {
                                        first.ItemId = inboxItem.ItemId;
                                        first.SeenTime = inboxItem.TimeStamp;
                                    }
                                    else
                                    {
                                        thread.Thread.LastSeenAt.Add(new InstaLastSeen
                                        {
                                            PK = ThreadView.Current.ThreadVM.CurrentThread.Users[0].Pk,
                                            ItemId = inboxItem.ItemId,
                                            SeenTime = inboxItem.TimeStamp
                                        });
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                   
            }
            catch { }
        }
        public void GoFirst(string threadId, InstaDirectInboxItem inboxItem)
        {
            try
            {
                var item = Items.FirstOrDefault(x => x.Thread.ThreadId == threadId);
                if (item != null)
                {
                    var findIndex = Items.IndexOf(item);
                    if(findIndex != 0)
                    Items.Move(findIndex, 0);
                    UpdateThread(threadId, inboxItem);
                }
                else
                    RunLoadMore(true);
            }
            catch { }
        }
        public void UpdateThread(string threadId, InstaDirectInboxItem inboxItem)
        {
            try
            {
                if (Helper.DontUseTimersAndOtherStuff) return;
                var thread = Items.FirstOrDefault(x => x.Thread.ThreadId == threadId);
                if (thread != null)
                {
                    thread.Thread.HasUnreadMessage = true;
                    thread.Thread.Items.Add(inboxItem);
                    thread.UpdateItem(inboxItem);
                }
            }
            catch { }
        }
        public static InstaDirectInboxThread GetThread(long pk)
        {
            if (Instance.Items.Count > 0)
            {
                var exists = Instance.Items.FirstOrDefault(x => x.Thread.Users.Count == 1 && x.Thread.Users.FirstOrDefault()?.Pk == pk);
                if (exists != null)
                    return exists.Thread;
            }
            return null;
        }
        void Append(InstaDirectInboxThread thread, bool first = true, int index = -1)
        {
            if(first)
            {
                Items.Add(GetDirectInboxUc(thread));
                return;
            }

            //if (!Items.Any(x => x.Thread.ThreadId.ToLower() == thread.ThreadId.ToLower()))
            //    Items.Insert(0, GetDirectInboxUc(thread));
            //else
            //{
            //    var item = Items.FirstOrDefault(x => x.Thread.ThreadId == thread.ThreadId);
            //    var findIndex = Items.IndexOf(item);
            //    if (index < Items.Count && index != -1 && findIndex != index)
            //        Items.Move(findIndex, index);
            //}
        }

        readonly Random Rnd = new Random();

        DirectInboxUc GetDirectInboxUc(InstaDirectInboxThread thread)
        {
            var uc = new DirectInboxUc
            {
                Name = (Rnd.Next(11111, 999999) + Rnd.Next(10000,999999)).ToString(),
                Thread = thread,
                IsRightTapEnabled = true,
                IsHoldingEnabled = true
            };
            uc.RightTapped += UcRightTapped;
            uc.Holding += UcHolding;
            var deleteMenuFlyoutItem = new MenuFlyoutItem
            {
                DataContext = thread,
                Text = "Delete"
            };
            deleteMenuFlyoutItem.Click += DeleteMenuFlyoutItemClick;
            var mediaFlyout = new MenuFlyout
            {
                Items =
                {
                    deleteMenuFlyoutItem,
                }
            };
            uc.ContextFlyout = mediaFlyout;
            return uc;
        }

        private async void DeleteMenuFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem menu && menu != null && menu.DataContext is InstaDirectInboxThread thread && thread !=null)
                    await new DeleteThreadDialog(thread).ShowAsync();
            }
            catch { }
        }

        private void UcRightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            try
            {
                if (sender is DirectInboxUc uc && uc != null)
                    FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            catch { }
        }

        private void UcHolding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            try
            {
                if (sender is DirectInboxUc uc && uc != null)
                    FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            catch { }
        }


        public async void GetUserPresense()
        {
            try
            {
                if (Helper.DontUseTimersAndOtherStuff) return;
                if (Items?.Count > 0)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        var presences = await InstaApi.MessagingProcessor.GetUsersPresenceAsync();
                        if (presences.Value?.Count > 0)
                        {
                            if (Items.Count > 0)
                            {
                                for (int i = 0; i < presences.Value.Count; i++)
                                {
                                    var item = presences.Value[i];
                                    var single = Items.FirstOrDefault(x => x.Thread.Users.FirstOrDefault()?.Pk == item.Pk);
                                    single?.SetUserPresence(item);
                                }
                            }
                        }
                        else
                        {
                            if (Items.Count > 0)
                                for (int i = 0; i < Items.Count; i++)
                                    Items[i].SetUserPresence(null);
                        }

                    });
                }
            }
            catch { }
        }
    }

}
