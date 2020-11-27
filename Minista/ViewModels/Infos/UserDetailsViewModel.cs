using InstagramApiSharp.Classes.Models;
using Minista.Helpers;
using Minista.ItemsGenerators;
using Minista.Models.Main;
using Minista.Views.Infos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using static Helper;
namespace Minista.ViewModels.Infos
{
    public class UserDetailsViewModel : BaseModel
    {
        Visibility _highlightsVisibility = Visibility.Collapsed;
        public Visibility HighlightsVisibility { get { return _highlightsVisibility; } set { _highlightsVisibility = value; OnPropertyChanged("HighlightsVisibility"); } }
        Visibility _igtvVisibility = Visibility.Collapsed;
        public Visibility IGTVVisibility { get { return _igtvVisibility; } set { _igtvVisibility = value; OnPropertyChanged("IGTVVisibility"); } }

        Visibility _privateVisibility = Visibility.Collapsed;
        public Visibility PrivateVisibility { get { return _privateVisibility; } set { _privateVisibility = value; OnPropertyChanged("PrivateVisibility"); } }

        Visibility _chainingVisibility = Visibility.Collapsed;
        public Visibility ChainingVisibility { get { return _chainingVisibility; } set { _chainingVisibility = value; OnPropertyChanged("ChainingVisibility"); } }
        Visibility _noPostsVisibility = Visibility.Collapsed;
        public Visibility NoPostsVisibility { get { return _noPostsVisibility; } set { _noPostsVisibility = value; OnPropertyChanged("NoPostsVisibility"); } }
        Visibility _broadcastVisibility = Visibility.Collapsed;
        public Visibility BroadcastVisibility { get { return _broadcastVisibility; } set { _broadcastVisibility = value; OnPropertyChanged("BroadcastVisibility"); View?.SetBroadcastVisibility(value == Visibility.Collapsed); } }
        private double _storyStrokeThickness = 0;
        public double StoryStrokeThickness { get { return _storyStrokeThickness; } set { _storyStrokeThickness = value; OnPropertyChanged("StoryStrokeThickness"); } }


        public UserDetailsView View;
        private InstaUserShort UserShort;
        private InstaStoryFriendshipStatus _friendshipStatus;
        private InstaUserInfo _user;
        private InstaBroadcast _broadcast;
        //private bool FirstTime = true;
        private string Username = null;
        public InstaUserInfo User { get { return _user; } set { _user = value; OnPropertyChanged("User"); SetBio(); } }
        public InstaStoryFriendshipStatus FriendshipStatus { get { return _friendshipStatus; } set { _friendshipStatus = value; OnPropertyChanged("FriendshipStatus"); } }
        public InstaBroadcast Broadcast { get { return _broadcast; } set { _broadcast = value; OnPropertyChanged("Broadcast"); } }

        public UserDetailsMediasGenerator MediaGeneratror { get; set; } = new UserDetailsMediasGenerator();
        public UserDetailsTaggedMediasGenerator TaggedMediaGeneratror { get; set; } = new UserDetailsTaggedMediasGenerator();
        public UserDetailsTVMediasGenerator TVMediaGeneratror { get; set; } = new UserDetailsTVMediasGenerator();
        public UserDetailsShopMediasGenerator ShopMediaGeneratror { get; set; } = new UserDetailsShopMediasGenerator();

         

        public ObservableCollection<InstaHighlightFeed> Highlights { get; set; } = new ObservableCollection<InstaHighlightFeed>();
        public ObservableCollection<InstaReelFeed> Stories { get; set; } = new ObservableCollection<InstaReelFeed>();
        public ObservableCollection<InstaUserChaining> ChainingSuggestions { get; set; } = new ObservableCollection<InstaUserChaining>();
        InstaTVSelfChannel _tvChannel;
        public InstaTVSelfChannel TVChannel { get { return _tvChannel; } set { _tvChannel = value; OnPropertyChanged("TVChannel"); } }
        public UserDetailsViewModel() { MediaGeneratror.SetVM(this); }
        public RichTextBlock BiographyText;
        public void SetBiographyTextBlock(RichTextBlock textBlock)
        {
            BiographyText = textBlock;
        }
        public void ResetToDefault()
        {
            if(BiographyText != null)
            BiographyText.Blocks.Clear();
            MediaGeneratror.ResetCache();
            UserShort = null;
            FriendshipStatus = null;
            User = null;
            Username = null;
            TVChannel = null;
            Broadcast = null;
            Highlights.Clear();
            Stories.Clear();
            ChainingSuggestions.Clear();
            PrivateVisibility = NoPostsVisibility = ChainingVisibility = IGTVVisibility = HighlightsVisibility = BroadcastVisibility = Visibility.Collapsed;
            View = null;
            StoryStrokeThickness = 0;
        }
        public async void SetUsername(string username)
        {
            try
            {
                if (!string.IsNullOrEmpty(username))
                    Username = username.TrimAnyBullshits();

                if (string.IsNullOrEmpty(Username)) return;

                await MainPage.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    var userInfo = await InstaApi.UserProcessor.GetUserInfoByUsernameAsync(Username);
                    if(userInfo.Succeeded)
                    {
                        UserShort = userInfo.Value?.ToUserShort();
                        View.ScrollableUserPostUc.UpdateUser(UserShort);
                        User = userInfo.Value;
                        MediaGeneratror.UserId = userInfo.Value.Pk;
                        TVMediaGeneratror.UserId = userInfo.Value.Pk;
                        TaggedMediaGeneratror.UserId = userInfo.Value.Pk;
                        ShopMediaGeneratror.UserId = userInfo.Value.Pk;
                        View.CreateTabs(User.TotalIGTVVideos > 0, User.ShoppablePostsCount > 0, User.UsertagsCount > 0);
                        GetUserFriendship();
                        //GetUserInfo();
                        GetHighlights();
                        View.UpdateUserImage(UserShort.ProfilePicture);
                        GetPosts();
                        GetStories();
                    }
                    else
                    {
                        if (userInfo.Info.Message.ToLower().Contains("user not found"))
                        {
                            $"User not found.\r\n@{Username} doesn't exists.".ShowErr();
                        }
                    }
                });
            }
            catch { }
        }
        public void SetUser(InstaUserShort userShort)
        {
            if (userShort == null) return;
            UserShort = userShort;
            User = userShort?.ToUserInfo();
            MediaGeneratror.UserId = userShort.Pk;
            TVMediaGeneratror.UserId = userShort.Pk;
            TaggedMediaGeneratror.UserId = userShort.Pk;
            ShopMediaGeneratror.UserId = userShort.Pk;
            GetUserFriendship();
            GetUserInfo();
            GetHighlights();
            GetStories();
        }
        public void SetUser(InstaUserShortFriendship userShortFriendship)
        {
            if (userShortFriendship == null) return;
            UserShort = userShortFriendship.ToUserShort();
            User = UserShort?.ToUserInfo();
            MediaGeneratror.UserId = UserShort.Pk;
            TVMediaGeneratror.UserId = UserShort.Pk;
            TaggedMediaGeneratror.UserId = UserShort.Pk;
            ShopMediaGeneratror.UserId = UserShort.Pk;
            if (userShortFriendship.FriendshipStatus != null)
                FriendshipStatus = userShortFriendship.FriendshipStatus?.ToStoryFriendshipStatus();

            GetUserFriendship();
            GetUserInfo();
            GetHighlights();
            GetStories();
        }
        public void SetBroadcast(InstaBroadcast broadcast)
        {
            try
            {
                Broadcast = broadcast;
                BroadcastVisibility = broadcast != null ? Visibility.Visible : Visibility.Collapsed;
                StoryStrokeThickness = broadcast != null ? 3 : 0;

                if (broadcast == null)
                    StoryStrokeThickness = Stories.Count > 0 ? 3 : 0;
            }
            catch { }
        }
        //public void SetScrollViewer(ScrollViewer scrollViewer)
        //{
        //    MediaGeneratror.SetLV(scrollViewer);
        //}
        async void GetUserFriendship()
        {
            try
            {
                var status = await InstaApi.UserProcessor.GetFriendshipStatusAsync(UserShort.Pk);
                if (status.Succeeded)
                {
                    FriendshipStatus = status.Value;
                    if (FriendshipStatus != null)
                    {
                        if (FriendshipStatus.IsPrivate && !FriendshipStatus.Following)
                            PrivateVisibility = Visibility.Visible;
                    }
                    else
                        PrivateVisibility = Visibility.Collapsed;
                }
                else
                {
                    if (FriendshipStatus == null)
                    {
                        FriendshipStatus = new InstaStoryFriendshipStatus();
                        PrivateVisibility = Visibility.Collapsed;
                    }
                }
            }
            catch { }
        }
        async void GetUserInfo()
        {
            try
            {
                var userInfo = await InstaApi.UserProcessor.GetUserInfoByIdAsync(UserShort.Pk);
                if (userInfo.Succeeded)
                {
                    ChainingSuggestions.Clear();
                    User = userInfo.Value;
                    View.CreateTabs(User.TotalIGTVVideos > 0, User.ShoppablePostsCount > 0, User.UsertagsCount > 0);
                    if (User != null)
                        GetPosts(true);
                    //if (!FirstTime)
                    View.UpdateUserImage(User.ProfilePicUrl);
                    if (userInfo.Value.ChainingSuggestions?.Count > 0)
                    {
                        ChainingSuggestions.AddRange(User.ChainingSuggestions);
                        ChainingVisibility = Visibility.Visible;
                        await MainPage.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                        {
                            var users = userInfo.Value.ChainingSuggestions.Select(x => x.Pk);
                            var statuses = await InstaApi.UserProcessor.GetFriendshipStatusesAsync(users.ToArray());
                            ($"users count: {users.Count()}").PrintDebug();
                            ($"statuses count: {statuses.Value.Count}").PrintDebug();
                            foreach (var item in statuses.Value)
                            {
                                try
                                {
                                    var u = ChainingSuggestions.SingleOrDefault(s => s.Pk == item.Pk);
                                    if (u != null)
                                        u.FriendshipStatus = item;
                                }
                                catch { }
                            }
                        });
                    }
                    else
                        ChainingVisibility = Visibility.Collapsed;
                }
                else
                {
                    if (userInfo.Info.Message.ToLower().Contains("user not found"))
                    {
                        $"User not found.\r\n@{UserShort.UserName} doesn't exists.".ShowErr();
                    }
                    FriendshipStatus = new InstaStoryFriendshipStatus();
                    ChainingVisibility = Visibility.Collapsed;
                }
            }
            catch { }
            //FirstTime = false;
        }
        async void GetHighlights()
        {
            try
            {
                var hightlights = await InstaApi.StoryProcessor.GetHighlightFeedsAsync(UserShort.Pk);
                if (hightlights.Succeeded)
                {
                    Highlights.Clear();
                    if (hightlights.Value?.Items?.Count > 0)
                    {
                        Highlights.AddRange(hightlights.Value.Items);
                        HighlightsVisibility = Visibility.Visible;
                        try
                        {
                            // ehyanan az inja nist?:|
                            await MainPage.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                var users = new List<string>();
                                foreach (var item in hightlights.Value.Items.Take(5))
                                    users.Add(item.HighlightId);
                                var highlightsAfterResult = await InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(users.ToArray());
                                if (highlightsAfterResult.Succeeded)
                                {
                                    var highlightsAfter = highlightsAfterResult.Value.Items;

                                    foreach (var item in Highlights)
                                    {
                                        try
                                        {
                                            var single = highlightsAfter.SingleOrDefault(ss => ss.Id == item.HighlightId);
                                            if (single != null)
                                            {
                                                item.Items.Clear();
                                                item.Items.AddRange(single.Items);
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            });
                        }
                        catch { }
                    }
                    else
                         HighlightsVisibility = Visibility.Collapsed;
                    TVChannel = hightlights.Value.TVChannel;
                    if (hightlights.Value.TVChannel != null)
                    {
                        IGTVVisibility = Visibility.Visible;
                    }
                    else
                        IGTVVisibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        async void GetStories()
        {
            try
            {
                await MainPage.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    //var stories = await InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(UserShort.Pk.ToString());
                    //if (stories.Succeeded)
                    //{
                    //    Stories.Clear();
                    //    if (stories.Value.Items?.Count > 0)
                    //        Stories.AddRange(stories.Value.Items);
                    //}
                    var st = await InstaApi.StoryProcessor.GetUserStoryAndLivesAsync(UserShort.Pk);
                    if (st.Succeeded)
                    {
                        Stories.Clear();
                        if (st.Value.Reel != null)
                            Stories.Add(st.Value.Reel);
                        SetBroadcast(st.Value.Broadcast);
                    }
                    else
                        SetBroadcast(null);
                });
            }
            catch { }
        }

        public async void GetPosts(bool refresh = false)
        {
            if (FriendshipStatus != null)
                if (FriendshipStatus.IsPrivate && !FriendshipStatus.Following)
                    return;
            await MediaGeneratror.RunLoadMoreAsync(refresh);
        }
        public async void GetTVPosts(bool refresh = false)
        {
            if (FriendshipStatus != null)
                if (FriendshipStatus.IsPrivate && !FriendshipStatus.Following)
                    return;
            await TVMediaGeneratror.RunLoadMoreAsync(refresh);
        }
        public async void GetTaggedPosts(bool refresh = false)
        {
            if (FriendshipStatus != null)
                if (FriendshipStatus.IsPrivate && !FriendshipStatus.Following)
                    return;
            await TaggedMediaGeneratror.RunLoadMoreAsync(refresh);
        }
        public async void GetShoppablePosts(bool refresh = false)
        {
            if (FriendshipStatus != null)
                if (FriendshipStatus.IsPrivate && !FriendshipStatus.Following)
                    return;
            await ShopMediaGeneratror.RunLoadMoreAsync(refresh);
        }
        public void Refresh(bool posts = true, bool tv = false, bool tag = false, bool shop = false)
        {
            if(UserShort == null)
            {
                SetUsername(null);
                return;
            }
            GetUserFriendship();
            GetUserInfo();
            if (posts)
                GetPosts(true);
            else if (tv)
                GetTVPosts(true);
            else if (tag)
                GetTaggedPosts(true);
            else if (shop)
                GetShoppablePosts(true);
            GetHighlights();
            GetStories();
        }


        void SetBio()
        {
            try
            {
                if (BiographyText == null) return;
                BiographyText?.Blocks?.Clear();

                if (User == null) return;

                if (string.IsNullOrEmpty(User.Biography)) return;


                //using (var pg = new PassageHelper())
                //{
                //    var passages = pg.GetParagraph(User.Biography, CaptionHyperLinkClick);
                //    BiographyText.Blocks.Clear();
                //    BiographyText.Blocks.Add(passages);
                //}
                //using (var pg = new WordsHelper())
                //{
                //    var passages = pg.GetParagraph(User.Biography, CaptionHyperLinkClick);
                //    BiographyText.Blocks.Clear();
                //    passages.ForEach(p =>
                //      BiographyText.Blocks.Add(p));
                //}
                using (var pg = new PassageHelperX())
                {
                    var passages = pg.GetInlines(User.Biography, HyperLinkHelper.HyperLinkClick);
                    BiographyText.Blocks.Clear();
                    BiographyText.FlowDirection = passages.Item2 ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                    var p = new Paragraph();
                    passages.Item1.ForEach(item =>
                    p.Inlines.Add(item));
                    BiographyText.Blocks.Add(p);
                }
            }
            catch { }
        }
    }
}
