using InstagramApiSharp.Classes.Models;
using Minista.Helpers;
using Minista.ItemsGenerators;
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
    public class ProfileDetailsViewModel : BaseModel
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


        private InstaBroadcast _broadcast;
        public ProfileDetailsView View;
        //private InstaStoryFriendshipStatus _friendshipStatus;
        private InstaUserInfo _user;
        //private bool FirstTime = true;
        //private string Username = null;
        public InstaUserInfo User { get { return _user; } set { _user = value; OnPropertyChanged("User"); SetBio(); } }
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
        public ProfileDetailsViewModel()
        {
            User = CurrentUser;
            MediaGeneratror.SetVM(this);
        }
        public RichTextBlock BiographyText;
        public void SetBiographyTextBlock(RichTextBlock textBlock)
        {
            BiographyText = textBlock;
        }
        public async void SetInfo()
        {
            try
            {
                await MainPage.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    MediaGeneratror.UserId = User.Pk;
                    TVMediaGeneratror.UserId = User.Pk;
                    TaggedMediaGeneratror.UserId = User.Pk;
                    ShopMediaGeneratror.UserId = User.Pk;
                    GetHighlights();
                    View.UpdateUserImage(User.ProfilePicture);
                    GetPosts();
                    GetStories();
                    GetUserInfo();
                    View.SetBusinessProfile();
                });
            }
            catch { }
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
        //    //if (User != null)
        //    //    GetPosts();
        //}
        async void GetUserInfo()
        {
            try
            {
                await MainPage.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    var userInfo = await UserHelper.GetSelfUserAsync();
                    if (userInfo != null)
                    {
                        View.SetBusinessProfile();
                        ChainingSuggestions.Clear();
                        User = userInfo;

                        View.CreateTabs(User.TotalIGTVVideos > 0, User.ShoppablePostsCount > 0, User.UsertagsCount > 0);
                        //if (!FirstTime)
                        View.UpdateUserImage(User.ProfilePicture);
                        if (userInfo.ChainingSuggestions?.Count > 0)
                        {
                            ChainingSuggestions.AddRange(User.ChainingSuggestions);
                            ChainingVisibility = Visibility.Visible;

                            var users = userInfo.ChainingSuggestions.Select(x => x.Pk);
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
                        }
                        else
                            ChainingVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        ChainingVisibility = Visibility.Collapsed;
                    }
                });
            }
            catch { }
            //FirstTime = false;
        }
        async void GetHighlights()
        {
            try
            {
                var hightlights = await InstaApi.StoryProcessor.GetHighlightFeedsAsync(CurrentUser.Pk);
                if (hightlights.Succeeded)
                {
                    Highlights.Clear();
                    if (hightlights.Value.Items?.Count > 0)
                    {
                        Highlights.AddRange(hightlights.Value.Items);
                        HighlightsVisibility = Visibility.Visible;
                        try
                        {
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
                    //var stories = await InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(User.Pk.ToString());
                    //if (stories.Succeeded)
                    //{
                    //    Stories.Clear();
                    //    if (stories.Value.Items?.Count > 0)
                    //        Stories.AddRange(stories.Value.Items);
                    //}
                    var st = await InstaApi.StoryProcessor.GetUserStoryAndLivesAsync(User.Pk);
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
            await MediaGeneratror.RunLoadMoreAsync(refresh);
        }
        public async void GetTVPosts(bool refresh = false)
        {
            await TVMediaGeneratror.RunLoadMoreAsync(refresh);
        }
        public async void GetTaggedPosts(bool refresh = false)
        {
            await TaggedMediaGeneratror.RunLoadMoreAsync(refresh);
        }
        public async void GetShoppablePosts(bool refresh = false)
        {
            await ShopMediaGeneratror.RunLoadMoreAsync(refresh);
        }
        public void Refresh(bool posts = true, bool tv = false, bool tag = false, bool shop = false)
        {
            GetUserInfo();
            if (posts)
                GetPosts(true);
            else if (tv)
                GetTVPosts(true);
            else if (tag)
                GetTaggedPosts(true);
            else if (shop)
                GetShoppablePosts(true); GetHighlights();
            GetStories();
        }


        public void SetBio()
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
