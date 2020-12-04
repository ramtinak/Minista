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
using static Helper;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using InstagramApiSharp;
using Minista.ItemsGenerators;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Minista.ViewModels.Direct;
using Minista.Views.Main;
using InstagramApiSharp.Helpers;
using InstagramApiSharp.Enums;

namespace Minista.ViewModels.Main
{
    public class MainViewModel : BaseModel
    {
        public Orientation StoryOrientation { get; set; } = Orientation.Horizontal;
        public ObservableCollection<StoryModel> Stories { get; set; } = new ObservableCollection<StoryModel>();
        public ObservableCollection<StoryWithLiveSupportModel> StoriesX { get; set; } = new ObservableCollection<StoryWithLiveSupportModel>();
        public MainPostsGenerator PostsGenerator { get; set; } = new MainPostsGenerator();

        Visibility _storeisVisibility = Visibility.Collapsed;
        public Visibility StoreisVisibility { get { return _storeisVisibility; } set { _storeisVisibility = value; OnPropertyChanged("StoreisVisibility"); } }
        public async void FirstRun(bool refresh = false)
        {
         
            RefreshStories(refresh);
            PostsGenerator.RunLoadMore(refresh);
            InboxViewModel.ResetInstance();
            if (InboxViewModel.Instance != null)
            {
                try
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        if (InboxViewModel.Instance.SeqId <= 0)
                            await InboxViewModel.Instance.RunLoadMoreAsync(refresh);
                        if (MainPage.Current.RealtimeClient == null)
                            MainPage.Current.RealtimeClient = new InstagramApiSharp.API.RealTime.RealtimeClient(InstaApi);
                        var client = MainPage.Current.RealtimeClient;

                        await client.Start(InboxViewModel.Instance.SeqId, InboxViewModel.Instance.SnapshotAt);
                        client.DirectItemChanged += InboxViewModel.Instance.RealtimeClientDirectItemChanged;
                        client.TypingChanged += InboxViewModel.Instance.RealtimeClientClientTypingChanged;
                        client.BroadcastChanged += RealtimeClientBroadcastChanged;

                    });
                }
                catch { }
            }
            //MainView.Current?.MainViewInboxUc?.InboxVM?.RunLoadMore(refresh);
            ActivitiesViewModel.Instance?.RunLoadMore(refresh);
        }

        private void RealtimeClientBroadcastChanged(object sender, InstagramApiSharp.API.RealTime.Handlers.InstaBroadcastEventArgs e)
        {
            try
            {
                var type = (InstaBroadcastStatusType)Enum.Parse(typeof(InstaBroadcastStatusType), e.BroadcastStatus?.Replace("_", ""), true);
                if (type == InstaBroadcastStatusType.Active)
                {
                    if (!StoriesX.Any(x => x.Type == StoryType.Broadcast && x.Broadcast?.Id == e.BroadcastId))
                    {
                        var broadcast = new StoryWithLiveSupportModel
                        {
                            Broadcast = new InstaBroadcast
                            {
                                Id = e.BroadcastId,
                                BroadcastOwner = e.User.ToUserShortFriendshipFull(),
                            },
                            Type = StoryType.Broadcast
                        };

                        StoriesX.Insert(0, broadcast);
                        //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        //{
                        //    var getInfo = await InstaApi.LiveProcessor.GetInfoAsync(e.BroadcastId);
                        //    if (getInfo.Succeeded)
                        //    {
                        //        broadcast.Broadcast.DashManifest = getInfo.Value.DashManifest;
                        //        broadcast.Broadcast.DashPlaybackUrl = getInfo.Value.DashPlaybackUrl;
                        //    }
                        //});
                    }
                }
                else if (type == InstaBroadcastStatusType.Stopped || type == InstaBroadcastStatusType.HardStop)
                {
                    var first = StoriesX.FirstOrDefault(x => x.Type == StoryType.Broadcast && x.Broadcast?.Id == e.BroadcastId);
                    if (first != null)
                        StoriesX.Remove(first);
                }
                if (Helpers.NavigationService.Frame.Content is Views.Broadcast.LiveBroadcastView view && view != null)
                    view.LiveVM?.DetermineLiveStatus(e.BroadcastId, type);
                else if (Helpers.NavigationService.Frame.Content is Views.Infos.UserDetailsView userDetails &&
                    e.User?.Pk == userDetails?.UserDetailsVM?.User?.Pk)
                    userDetails.SetBroadcast(type == InstaBroadcastStatusType.Active ? new InstaBroadcast { Id = e.BroadcastId } : null);
                else if (Helpers.NavigationService.Frame.Content is Views.Infos.ProfileDetailsView profileDetails &&
                    profileDetails != null && e.User?.Pk == CurrentUser?.Pk)
                    profileDetails.SetBroadcast(type == InstaBroadcastStatusType.Active ? new InstaBroadcast { Id = e.BroadcastId } : null);
            }
            catch { }
        }

        public async void RefreshStories(bool refresh = false)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var stories = await InstaApi.StoryProcessor.GetStoryFeedWithPostMethodAsync(refresh);
                    if (stories.Succeeded)
                    {
                        Stories.Clear();
                        StoriesX.Clear();
                        var listX = new List<StoryWithLiveSupportModel>();
                        if (stories.Value.Broadcasts?.Count > 0)
                        {
                            for (int i = 0; i < stories.Value.Broadcasts.Count; i++)
                            {
                                var item = stories.Value.Broadcasts[i];
                                listX.Add(new StoryWithLiveSupportModel
                                {
                                    Broadcast = item,
                                    Type = StoryType.Broadcast
                                });
                            }
                        }
                        if (stories.Value.Items?.Count > 0)
                        {
                            var list = new List<StoryModel>();
                            string id = null;
                            for (int i = 0; i < stories.Value.Items.Count; i++)
                            {
                                var item = stories.Value.Items[i];
                                var m = item.ToStoryModel();

                                if (string.IsNullOrEmpty(id) || !string.IsNullOrEmpty(id) && id != item.Id)
                                {
                                    list.Add(m);
                                    listX.Add(new StoryWithLiveSupportModel
                                    {
                                        Story = item.ToStoryModel(),
                                        Type = StoryType.Story
                                    });
                                }
                                id = item.Id;
                            }
                            id = null;
                            Stories.AddRange(list);
                            if (stories.Value.Items?.Count > 0)
                            {
                                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                                {
                                    var users = new List<string>();
                                    foreach (var item in stories.Value.Items.Take(5))// 5ta ro migirim!
                                                                                     //if (item.IsHashtag)
                                                                                     //    users.Add(item.Owner.Pk.ToString());
                                                                                     //else
                                                                                     //users.Add(item.User.Pk.ToString());
                                    users.Add(item.Id);

                                    var storiesAfterResult = await InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(users.ToArray());
                                    if (storiesAfterResult.Succeeded)
                                    {
                                        var storiesAfter = storiesAfterResult.Value.Items;
                                        for (int i = 0; i < Stories.Count; i++)
                                        {
                                            var item = Stories[i];
                                        //var single = storiesAfter.SingleOrDefault(ss => ss.User.Pk.ToString() == item.User.Pk.ToString());
                                        var single = storiesAfter.SingleOrDefault(ss => ss.Id == item.Id);
                                            if (single != null)
                                            {
                                                item.Items.Clear();
                                                item.Items.AddRange(single.Items);
                                            }
                                        }
                                    }
                                    StoreisVisibility = Visibility.Visible;
                                });

                            }
                        }
                        if (stories.Value.PostLives?.Count > 0)
                        {
                            for (int i = 0; i < stories.Value.PostLives.Count; i++)
                            {
                                var item = stories.Value.PostLives[i];
                                var xtem = new StoryWithLiveSupportModel
                                {
                                    PostLives = item,
                                    Type = StoryType.PostLive
                                };

                                try
                                {
                                    if (listX.Count - 1 > item.RankedPosition && item.RankedPosition != -1)
                                        listX.Insert(item.RankedPosition, xtem);
                                    else
                                        listX.Add(xtem);
                                }
                                catch { listX.Add(xtem); }
                            }
                        }

                        StoriesX.AddRange(listX);
                        if (StoriesX.Count > 0)
                            StoreisVisibility = Visibility.Visible;
                    }
                    else
                    {
                        if (stories.Info.ResponseType == ResponseType.LoginRequired)
                            MainPage.Current.LoggedOut();
                        if (stories.Info.ResponseType == ResponseType.ChallengeRequired ||
                        stories.Info.ResponseType == ResponseType.ChallengeRequiredV2)
                        {
                            InstaApi.SetChallengeInfo(stories.Info.Challenge);

                            MainPage.Current.InAppChallenge.StartChallengeV2(stories.Info.Challenge);
                            return;
                        }
                        //if (Stories.Count == 0)
                        //    StoreisVisibility = Visibility.Collapsed;
                        if (StoriesX.Count == 0)
                            StoreisVisibility = Visibility.Collapsed;
                        if (stories.Info.ResponseType == ResponseType.ConsentRequired)
                        {
                            ShowNotify("Consent is required!\r\nLet Minista fix it for you ;-)\r\nTrying.... Give me 30 seconds maximum...", 3500);
                            await Task.Delay(TimeSpan.FromSeconds(8));

                            var acceptConsent = await InstaApi.AcceptConsentAsync();
                            await Task.Delay(TimeSpan.FromSeconds(15));
                            ShowNotify("Consent is fixed (I think) let me try to refresh feeds and other stuffs for u.", 2500);
                            MainView.Current?.TryToRefresh(true);
                        }
                    }
                });
            }
            catch(Exception ex) { ex.PrintException("MainViewModel.RefreshStories"); }
            refresh = false;
        }

    }
}
