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
using Minista.Views.Direct;

namespace Minista.ViewModels.Direct
{
    public class ThreadViewModel : BaseModel, IGenerator
    {
        public ThreadViewModel()
        {
            RefreshTimer.Tick += RefreshTimerTick;
        }


        public ObservableCollection<InstaDirectInboxItem> Items { get; set; } = new ObservableCollection<InstaDirectInboxItem>();
        public ObservableCollection<InstaUserShortFriendship> Users { get; set; } = new ObservableCollection<InstaUserShortFriendship>();
        public ObservableCollection<GiphyItem> GiphsItems { get; set; } = new ObservableCollection<GiphyItem>();

        public PaginationParameters Pagination { get; set; } = PaginationParameters.MaxPagesToLoad(1);
        public bool HasMoreItems { get; set; } = true;
        ScrollViewer Scroll;
        private bool IsLoading = false;
        InstaDirectInboxThread _currentThread;
        public InstaDirectInboxThread CurrentThread { get { return _currentThread; } set { _currentThread = value; OnPropertyChanged("CurrentThread"); } }
        public bool IsFakeThreadEnabled = false;
        private Visibility _endChatVisibility = Visibility.Collapsed;
        public Visibility EndChatVisibility { get { return _endChatVisibility; } set { _endChatVisibility = value; OnPropertyChanged("EndChatVisibility"); } }

        private Visibility _addMemberVisibility = Visibility.Collapsed;
        public Visibility AddMemberVisibility { get { return _addMemberVisibility; } set { _addMemberVisibility = value; OnPropertyChanged("AddMemberVisibility"); } }





        private Visibility _leaveChatVisibility = Visibility.Visible;
        public Visibility LeaveChatVisibility { get { return _leaveChatVisibility; } set { _leaveChatVisibility = value; OnPropertyChanged("LeaveChatVisibility"); } }

        private Visibility _approvalAdminVisibility = Visibility.Collapsed;
        public Visibility ApprovalAdminVisibility { get { return _approvalAdminVisibility; } set { _approvalAdminVisibility = value; OnPropertyChanged("ApprovalAdminVisibility"); } }


        private Visibility _typingVisibility = Visibility.Collapsed;
        public Visibility TypingVisibility { get { return _typingVisibility; } set { _typingVisibility = value; OnPropertyChanged("TypingVisibility"); } }

        private string _typingText;
        public string TypingText
        {
            get => _typingText;
            set
            {
                _typingText = value;
                OnPropertyChanged("TypingText");
            }
        }
        public void SetThread(InstaDirectInboxThread directInboxThread)
        {
            CurrentThread = directInboxThread;

            try
            {
                if (CurrentThread.Users?.Count > 0)
                {
                    Users.AddRange(CurrentThread.Users);
                }
            }
            catch { }
            //if (CurrentThread.Items?.Count > 0)
            //{
            //    CurrentThread.Items.Reverse();
            //    CurrentThread.Items.ForEach(x => Items.Add(x));
            //    if (!any)
            //        ListView.ScrollIntoView(Items[Items.Count - 1]);
            //}
        }
        public void ResetCache()
        {
            try
            {
                EndChatVisibility = Visibility.Collapsed;
                ApprovalAdminVisibility = Visibility.Collapsed;
                AddMemberVisibility = Visibility.Collapsed;
                LeaveChatVisibility = Visibility.Collapsed;
                Users.Clear();
                IsLoading = true;
                HasMoreItems = true;
                IsFakeThreadEnabled = false;
                Items.Clear();
                Pagination = PaginationParameters.MaxPagesToLoad(1);
            }
            catch { }
        }
        public static InstaDirectInboxThread CreateFakeThread(InstaUserShortFriendship userShortFriendship)
        {
            var thread = new InstaDirectInboxThread
            {
                Title = userShortFriendship.UserName.ToLower(),
                Users = new List<InstaUserShortFriendship>
                {
                    userShortFriendship
                }
            };
            return thread;
        }
        ListView ListView;
        public void SetLV(ListView listView)
        {
            if (Scroll == null)
            {
                ListView = listView;
                Scroll = listView.FindScrollViewer();
                if (Scroll != null)
                    Scroll.ViewChanging += ScrollViewChanging;
                HasMoreItems = true;
                IsLoading = true;
                Pagination = PaginationParameters.MaxPagesToLoad(1);
            }

            RunLoadMore(true);
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
            if (CurrentThread == null)
                return;
            if(string.IsNullOrEmpty(CurrentThread.ThreadId))
                return;
            //if (refresh) 
            ////RunTimer();
            if (!HasMoreItems && !refresh)
            {
                IsLoading = false;
                return;
            }
            try
            {
                if (Helper.DontUseTimersAndOtherStuff) return;
                if (refresh)
                {
                    Pagination = PaginationParameters.MaxPagesToLoad(1);
                    Items.Clear();
                }
                Views.Direct.ThreadView.Current?.ShowTopLoading();

                var preloadedItems = string.Empty;
                if (refresh && CurrentThread?.Items?.Count > 0)
                {
                    var items = CurrentThread.Items.Select(s => s.ItemId).ToList();
                    preloadedItems = string.Join(",", items);
                }
                var result = await InstaApi.MessagingProcessor.GetDirectInboxThreadAsync(CurrentThread.ThreadId, Pagination, InboxView.Current?.InboxVM?.SeqId ?? 0, preloadedItems);
                Pagination.MaximumPagesToLoad = 1;
                if (!result.Succeeded)
                {
                    if (result.Value == null)
                    {
                        IsLoading = false;

                        ThreadView.Current?.HideTopLoading();
                        return;
                    }
                }

                HasMoreItems = result.Value.HasOlder;
                if (result.Value.IsGroup)
                {
                    if (result.Value.AdminUserIds.Any(x => x == CurrentUser.Pk))
                    {
                        EndChatVisibility = Visibility.Visible;
                        ApprovalAdminVisibility = Visibility.Visible;
                    }
                    AddMemberVisibility = Visibility.Visible;
                    LeaveChatVisibility = Visibility.Visible;
                }
                else
                {
                    AddMemberVisibility = Visibility.Collapsed;
                    EndChatVisibility = Visibility.Collapsed;
                    ApprovalAdminVisibility = Visibility.Collapsed;
                    LeaveChatVisibility = Visibility.Collapsed;
                }
                var any = Items.Any();
                if (result.Value?.Items?.Count > 0)
                {
                    result.Value.Items.Reverse();
                    //if (Items.Count > 0 && !refresh)
                    //{
                    //    if (Items.Any(x =>x.UserId == CurrentUser.Pk && x.SendingType == InstaDirectInboxItemSendingType.Seen))
                    //    { setSeen = true; }
                    //}
                    var seenAt = GetLastSeen();
                    result.Value.Items.ForEach(x =>
                    {
                        if (x.UserId == CurrentUser.Pk)
                            x.SendingType = InstaDirectInboxItemSendingType.Sent;
                        try
                        {
                            if (seenAt != -1)
                                if (long.Parse(x.TimeStampUnix) <= seenAt)
                                    x.SendingType = InstaDirectInboxItemSendingType.Seen;
                        }
                        catch { }
                        Items.Insert(0, x);
                    });
                }
                if (!any)
                {
                    await Task.Delay(1500);
                    ListView.ScrollIntoView(Items[Items.Count - 1], ScrollIntoViewAlignment.Leading);
                }
                if (refresh && result.Value != null)
                    CurrentThread.LastNonSenderItemAt = result.Value.LastNonSenderItemAt;
                if (result.Value.LastPermanentItem != null)
                    CurrentThread.LastPermanentItem = result.Value.LastPermanentItem;

                if (refresh)
                    MarkAsSeen();
                await Task.Delay(1000);
                //refresh = false;

            }
            catch (Exception ex)
            {
                ex.PrintException("InboxViewModel.LoadMoreItemsAsync");
            }
            IsLoading = false;
            Views.Direct.ThreadView.Current?.HideTopLoading();
        }
        public void Refresh()
        {
            RunLoadMore(true);
        }
        long GetLastSeen()
        {
            try
            {
                var lastActivity = CurrentThread.LastActivityUnix;
                if (CurrentThread.LastSeenAt?.Count > 0)
                {
                    var item = CurrentThread.LastSeenAt.FirstOrDefault(x => x.PK != CurrentUser.Pk);
                    if (item != null)
                        return long.Parse(item.SeenTimeUnix);
                }
            }
            catch { }
            return -1;
        }

        async void MarkAsSeen()
        {
            try
            {
                if (Helper.DontUseTimersAndOtherStuff) return;
                //return;
                if (!SettingsHelper.Settings.GhostMode)
                if (Items.Count > 0)
                {
                    var l = Items.ToList();
                        //l.Reverse();
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        bool canMarkAsSeen = false;
                        if (CurrentThread.LastPermanentItem != null)
                        {
                            if(CurrentThread.LastPermanentItem.ItemType == InstaDirectThreadItemType.RavenMedia)
                                await InstaApi.MessagingProcessor.MarkDirectVisualThreadAsSeenAsync(CurrentThread.ThreadId, CurrentThread.LastPermanentItem.ItemId);
                            else
                                await InstaApi.MessagingProcessor.MarkDirectThreadAsSeenAsync(CurrentThread.ThreadId, CurrentThread.LastPermanentItem.ItemId);
                        }
                        for (int i = 0; i < l.Count; i++)
                        {
                            var item = l[i];
                            if (item.TimeStampUnix == CurrentThread.LastNonSenderItemAt)
                                canMarkAsSeen = true;
                            if (canMarkAsSeen && item.UserId != CurrentUser.Pk /*&& i != l.Count -1*/)
                            {
                                CurrentThread.LastNonSenderItemAt = DateTime.UtcNow.ToUnixTimeMiliSeconds().ToString();

                                if (item.ItemType == InstaDirectThreadItemType.RavenMedia)
                                    await InstaApi.MessagingProcessor.MarkDirectVisualThreadAsSeenAsync(CurrentThread.ThreadId, item.ItemId);
                                else
                                    await InstaApi.MessagingProcessor.MarkDirectThreadAsSeenAsync(CurrentThread.ThreadId, item.ItemId);

                                await Task.Delay(500);
                            }
                        }
                        UpdateInbox(Items.LastOrDefault());
                    });
                }
            }
            catch { }
        }


        public async void GetGiphy()
        {
            try
            {
                if (GiphsItems.Count > 0) return;
                if (Helper.DontUseTimersAndOtherStuff) return;
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var giffy = await InstaApi.GetGiphyTrendingAsync();
                    if (giffy.Succeeded)
                    {
                  
                        giffy.Value.Items.ForEach(x => GiphsItems.Add(x));
                    }
                });
            }
            catch { }
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
                if (progress < 0.2 && IsLoading == false)
                {
                    IsLoading = true;
                    RunLoadMore(false);
                }
            }
            catch (Exception ex) { ex.PrintException("Scroll_ViewChanging"); }
        }







        public void UpdateInbox(InstaDirectInboxItem item, bool canGoFirst = false)
        {
            try
            {
                if (Helper.DontUseTimersAndOtherStuff) return;
                if (InboxViewModel.Instance.Items?.Count > 0)
                {
                    DirectInboxUc uc = null;
                    for (int i = 0; i < InboxViewModel.Instance.Items.Count; i++)
                    {
                        var a = InboxViewModel.Instance.Items[i];
                        if (a.Thread.ThreadId == CurrentThread.ThreadId)
                        {
                            a.Thread.Items.Add(item);
                            uc = a;
                            a.Thread.HasUnreadMessage = false;
                            if(a.Thread.LastSeenAt?.Count > 0)
                            a.Thread.LastSeenAt.LastOrDefault().SeenTime = DateTime.UtcNow;
                            a.UpdateItem(item);
                            if (a.HadNewMessages)
                            {
                                //
                                if (InboxViewModel.Instance.InboxContainer != null)
                                {
                                    InboxViewModel.Instance.InboxContainer.Inbox.UnseenCount--;
                                    a.HadNewMessages = false;
                                    MainPage.Current?.SetDirectMessageCount(InboxViewModel.Instance.InboxContainer);
                                }
                            }
                            break;
                        }
                    }
                    if (uc != null && canGoFirst)
                    {
                        var index = InboxViewModel.Instance.Items.IndexOf(uc);
                        if (index != 0)
                            InboxViewModel.Instance.Items.Move(index, 0);
                    }
                }
            }
            catch { }
        }
        public void AddItem(InstaDirectInboxItem item, bool forceToScrollEnd = false, int delay = 150)
        {
            try
            {
                if (Items.Count > 0)
                {
                    var first = Items.FirstOrDefault(s => s.ItemId == item.ItemId);
                    if (first != null) return;
                }
                Items.Add(item);
                if (forceToScrollEnd)
                { 
                    ScrollToEnd(item, delay);
                    return;
                }
                if (Scroll != null)
                {
                    if (Scroll.VerticalOffset >= Scroll.ScrollableHeight - 50)
                        ScrollToEnd(item, delay);
                }
            }
            catch { }
        }
       async void ScrollToEnd(InstaDirectInboxItem item, int delay = 300)
        {
            try
            {
                await Task.Delay(delay);
                ListView.ScrollIntoView(item, ScrollIntoViewAlignment.Leading);
            }
            catch { }
        }
        public void RemoveItem(string itemId)
        {
            try
            {
                if (Items.Count > 0)
                {
                    InstaDirectInboxItem deleteItem = null;
                    for (int i = 0; i < Items.Count; i++)
                    {
                        try
                        {
                            if (Items[i].ItemId.ToLower() == itemId.ToLower())
                            {
                                deleteItem = Items[i];
                                break;
                            }
                        }
                        catch { }
                    }
                    if (deleteItem != null)
                        Items.Remove(deleteItem);

                    deleteItem = null;
                }
            }
            catch { }
        }



        #region Refresh Mode
        public readonly DispatcherTimer RefreshTimer = new DispatcherTimer();
        async void RunTimer()
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await Task.Delay(10000);
                    StartRefreshTimer();
               });
            }
            catch { }
        }
        public void StartRefreshTimer()
        {
            try
            {
                RefreshTimer.Interval = TimeSpan.FromSeconds(5.5);
                try
                {
                    StopRefreshTimer();
                }
                catch { }
                RefreshTimer.Start();
            }
            catch { }
        }
        public void StopRefreshTimer() => RefreshTimer.Stop();
        private bool CanRefreshTimer = true;
        public async void RefreshTimerTick(object sender, object args)
        {
            try
            {
                if (Helper.DontUseTimersAndOtherStuff) return;
                if (CanRefreshTimer)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        CanRefreshTimer = false;
                           var result = await InstaApi.MessagingProcessor
                        .GetDirectInboxThreadAsync(CurrentThread.ThreadId, 
                        PaginationParameters.MaxPagesToLoad(1), InboxView.Current?.InboxVM?.SeqId ?? 0);
                        if (result.Succeeded)
                        {
                            if (result.Value?.Items?.Count > 0)
                            {
                                //result.Value.Items.Reverse();
                                var newList = result.Value.Items;
                                var oldList = Items.ToList();
                                var results = newList.Where(i => !oldList.Any(e => i.ItemId == e.ItemId)).ToList();

                                //var excluded = newList.Except(oldList).ToList();
                                bool flag = false;
                                results.ForEach(x =>
                                {
                                    Items.Add(x);
                                    flag = true;
                                });
                                if (flag)
                                {
                                    if (ThreadView.Current != null)
                                    {
                                        ThreadView.Current.ItemsLV.ScrollIntoView(Items[Items.Count - 1]);
                                    }
                                }

                                CurrentThread.LastNonSenderItemAt = result.Value.LastNonSenderItemAt;
                                await Task.Delay(500);
                                MarkAsSeen();
                            }
                        }
                        await Task.Delay(500);
                        CanRefreshTimer = true;
                    });
                }
            }
            catch(Exception ex) 
            {
                ex.PrintException("RefreshTimerTick");
            }
        }
        #endregion Refresh Mode
    }
}
