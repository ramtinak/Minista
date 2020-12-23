using InstagramApiSharp;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstagramApiSharp.Enums;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using InstagramApiSharp.Classes;
using Minista.Models;
using Windows.UI.Xaml;

namespace Minista
{
    public class GenerateCommentItems : BaseModel
    {
        public ObservableCollection<InstaComment> Items { get; set; }
        public bool HasMoreItems { get; set; }
        private int _LastPage = 1;
        public PaginationParameters Pagination { get; private set; }
        bool HasMore;
        readonly ScrollViewer Scroll;
        readonly ListView LV;
        public InstaMedia Media { get; private set; }
        private InstaComment _replyComment;
        public InstaComment ReplyComment
        {
            get { return _replyComment; }
            set
            {
                _replyComment = value;
                OnPropertyChanged("ReplyComment");
                ReplyVisibility = value == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        private Visibility _replyVisibility = Visibility.Collapsed;
        public Visibility ReplyVisibility
        {
            get { return _replyVisibility; }
            set { _replyVisibility = value; OnPropertyChanged("ReplyVisibility"); }
        }
        
        public GenerateCommentItems(InstaMedia instaMedia, ListView listView)
        {
            Media = instaMedia;
            Items = new ObservableCollection<InstaComment>();
            LV = listView;
            Scroll = listView.FindScrollViewer();
            if (Scroll != null)
                Scroll.ViewChanging += Scroll_ViewChanging;
            HasMoreItems = true;
            Pagination = PaginationParameters.MaxPagesToLoad(1);
        }
        public async Task RunLoadMoreAsync()
        {
            await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await LoadMoreItemsAsync();
            });
        }
        async Task LoadMoreItemsAsync()
        {
            if (!HasMoreItems)
            {
                IsLoading = false;
                return;
            }
            try
            {
                var result = await Helper.InstaApi.CommentProcessor.GetMediaCommentsAsync(Media.InstaIdentifier, Pagination);

                if (!result.Succeeded)
                {
                    IsLoading = false;
                    return;
                }


                if (!string.IsNullOrEmpty(result.Value.NextMinId) && result.Value.MoreHeadLoadAvailable &&
                    result.Value.NextMaxId == null && !result.Value.MoreCommentsAvailable)
                {
                    HasMore = HasMoreItems = true;
                    Pagination.NextMinId = result.Value.NextMinId;
                }
                else if (!string.IsNullOrEmpty(result.Value.NextMaxId) && result.Value.MoreCommentsAvailable &&
                    result.Value.NextMinId == null && !result.Value.MoreHeadLoadAvailable)
                {
                    HasMore = HasMoreItems = true;
                    Pagination.NextMaxId = result.Value.NextMaxId;
                }
                else
                {
                    HasMore = HasMoreItems = false;
                    Pagination.NextMaxId = null;
                    Pagination.NextMinId = null;
                }

                if (!string.IsNullOrEmpty(Media.CommentsCount))
                {
                    try
                    {
                        int count = int.Parse(Media.CommentsCount);
                        if (count <= Items.Count)
                        {
                            HasMore = HasMoreItems = false;
                            Pagination.NextMaxId = null;
                            Pagination.NextMinId = null;
                        }
                    }
                    catch { }
                }

                if (result.Value.Comments != null && result.Value.Comments.Any())
                    Items.AddRange(result.Value.Comments);

                _LastPage++;
                IsLoading = false;

            }
            catch (Exception ex)
            {
                IsLoading = false;
                ex.PrintException("GenerateCommentItems.LoadMoreItemsAsync");
            }
        }

        bool IsLoading = false;
        private async void Scroll_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                if (Items == null)
                    return;
                if (!Items.Any())
                    return;
                if (Scroll.VerticalOffset > 0 && Scroll.ScrollableHeight > 150)
                {
                    if (Scroll.VerticalOffset >= Scroll.ScrollableHeight - 140 && IsLoading == false)
                    {
                        IsLoading = true;
                        await RunLoadMoreAsync();
                    }
                }
            }
            catch (Exception ex) { ex.PrintException("Scroll_ViewChanging"); }
        }
    }

    public class GenerateLikersItems : BaseModel
    {
        public ObservableCollection<InstaUserShortTV> Items { get; set; }

        InstaMedia _instaMedia;
        public InstaMedia Media
        {
            get { return _instaMedia; }
            set { _instaMedia = value; OnPropertyChanged("Media"); }
        }
        public GenerateLikersItems(InstaMedia instaMedia)
        {
            Media = instaMedia;
            Items = new ObservableCollection<InstaUserShortTV>();
        }
        public async Task RunLoadMoreAsync()
        {
            await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await LoadMoreItemsAsync();
            });
        }
        async Task LoadMoreItemsAsync()
        {
            try
            {
                var result = await Helper.InstaApi.MediaProcessor.GetMediaLikersAsync(Media.InstaIdentifier);

                Items.Clear();
                if (!result.Succeeded)
                    return;
                if (result.Value != null && result.Value.Any())
                {
                    var convertedUsers = result.Value.Select(x => new InstaUserShortTV
                    {
                        FullName = x.FullName,
                        IsPrivate = x.IsPrivate,
                        IsVerified = x.IsVerified,
                        Pk = x.Pk,
                        ProfilePicture = x.ProfilePicture,
                        ProfilePictureId = x.ProfilePictureId,
                        ProfilePicUrl = x.ProfilePicUrl,
                        UserName = x.UserName
                    });
                    Items.AddRange(convertedUsers.ToList());
                }
                else
                    return;
               
                var users = result.Value.Select(x => x.Pk);
                var statuses = await Helper.InstaApi.UserProcessor.GetFriendshipStatusesAsync(users.ToArray());
                ($"users count: {users.Count()}").PrintDebug();
                ($"statuses count: {statuses.Value.Count}").PrintDebug();
                for (int i = 0; i < statuses.Value.Count; i++)
                {
                    try
                    {
                        // code
                        Items[i].Friendship = statuses.Value[0];
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                ex.PrintException("GenerateLikersItems.LoadMoreItemsAsync");
            }
        }
    }

    public class GenerateUserItems : BaseModel
    {
        public bool IsFirstRun = false;
        public ObservableCollection<InstaMedia> Items { get; set; }
        InstaTVUser _userDetail;
        public InstaTVUser UserDetail
        {
            get { return _userDetail; }
            set { _userDetail = value; OnPropertyChanged("UserDetail"); }
        }
        InstaUserShortTV _user;
        public InstaUserShortTV User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }
        string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged("Title"); }
        }
        public bool HasMoreItems { get; set; }
        private bool IsUserSets = false;
        private int _LastPage = 1;
        public PaginationParameters Pagination { get; private set; }
        bool HasMore;
        readonly ScrollViewer Scroll;
        readonly ListView LV;

        public GenerateUserItems(InstaUserShortTV user, ListView listView)
        {
            Items = new ObservableCollection<InstaMedia>();
            User = user;
            LV = listView;
            Scroll = listView.FindScrollViewer();
            if (Scroll != null)
                Scroll.ViewChanging += Scroll_ViewChanging;
            HasMoreItems = true;
            Pagination = PaginationParameters.MaxPagesToLoad(1);
        }
        public async Task RunLoadMoreAsync()
        {
            await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await LoadMoreItemsAsync();
            });
        }
        async Task LoadMoreItemsAsync()
        {
            if (!HasMoreItems)
            {
                IsLoading = false;
                return;
            }
            try
            {
                try
                {
                    if (User.Friendship == null)
                    {
                        var status = await Helper.InstaApi.UserProcessor.GetFriendshipStatusAsync(User.Pk);
                        if (status.Succeeded)
                        {
                            var x = status.Value;
                            User.Friendship = new InstaFriendshipShortStatus
                            {
                                Following = x.Following,
                                IncomingRequest = x.IncomingRequest,
                                IsPrivate = x.IsPrivate,
                                OutgoingRequest = x.OutgoingRequest
                            };
                        }
                    }
                }
                catch { }
                var result = await Helper.InstaApi.TVProcessor.GetChannelByIdAsync(User.Pk, Pagination);

                if (!result.Succeeded)
                {
                    IsLoading = false;
                    return;
                }
                if (!IsUserSets)
                {
                    IsUserSets = true;
                    UserDetail = result.Value.UserDetail;
                }
                if (result.Value.MaxId == null || !result.Value.HasMoreAvailable)
                    HasMore = HasMoreItems = false;

                Pagination.NextMaxId = result.Value.MaxId;
                if (result.Value.Items != null && result.Value.Items.Any())
                    Items.AddRange(result.Value.Items);
                Title = result.Value.Title;
                try
                {
                    if (IsFirstRun)
                    {
                        IsFirstRun = false;
                        //MainPage.Current.MediaUc.SetMedia(Items[0]);
                    }
                }
                catch { }
                _LastPage++;
                IsLoading = false;

            }
            catch (Exception ex)
            {
                IsLoading = false;
                ex.PrintException("GenerateUserItems.LoadMoreItemsAsync");
            }
        }

        bool IsLoading = false;
        private async void Scroll_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                if (Items == null)
                    return;
                if (!Items.Any())
                    return;
                if (Scroll.HorizontalOffset > 0 && Scroll.ScrollableWidth > 150)
                {
                    if (Scroll.HorizontalOffset >= Scroll.ScrollableWidth - 140 && IsLoading == false)
                    {
                        IsLoading = true;
                        await RunLoadMoreAsync();
                    }
                }
            }
            catch (Exception ex) { ex.PrintException("Scroll_ViewChanging"); }
        }
    }

    public class GenerateSearchItems : BaseModel
    {
        public ObservableCollection<InstaTVSearchResult> Items { get; set; }
        string _searchWord;
        public string SearchWord
        {
            get { return _searchWord; }
            set { _searchWord = value; OnPropertyChanged("SearchWord"); }
        }
        public GenerateSearchItems()
        {
            Items = new ObservableCollection<InstaTVSearchResult>();
        }
        public async Task RunLoadMoreAsync()
        {
            await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await LoadMoreItemsAsync();
            });
        }
        async Task LoadMoreItemsAsync()
        {
            try
            {
                IResult<InstaTVSearch> result;
                if (string.IsNullOrEmpty(SearchWord))
                    result = await Helper.InstaApi.TVProcessor.GetSuggestedSearchesAsync();
                else
                    result = await Helper.InstaApi.TVProcessor.SearchAsync(SearchWord);

                Items.Clear();
                if (!result.Succeeded)
                    return;
                if (result.Value.Results != null && result.Value.Results.Any())
                    Items.AddRange(result.Value.Results);


            }
            catch (Exception ex)
            {
                ex.PrintException("GenerateSearchItems.LoadMoreItemsAsync");
            }
        }
    }

    public class GenerateChannelItems : BaseModel
    {
        public bool IsFirstRun = false;
        public ObservableCollection<InstaMedia> Items { get; set; }
        string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged("Title"); }
        }
        public bool HasMoreItems { get; set; }
        public readonly InstaTVChannelType ChannelType;
        private int _LastPage = 1;
        public PaginationParameters Pagination { get; private set; }
        bool HasMore;
        readonly ScrollViewer Scroll;
        readonly ListView LV;

        public GenerateChannelItems(InstaTVChannelType type, ListView listView)
        {
            Items = new ObservableCollection<InstaMedia>();
            LV = listView;
            Scroll = listView.FindScrollViewer();
            if(Scroll!= null)
                Scroll.ViewChanging += Scroll_ViewChanging;
            HasMoreItems = true; 
            ChannelType = type;
            Pagination = PaginationParameters.MaxPagesToLoad(1);
        }
        public async Task RunLoadMoreAsync()
        {
            await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async()=>
            {
                await LoadMoreItemsAsync();
            });
        }
        async Task LoadMoreItemsAsync()
        {
            if (!HasMoreItems)
            {
                IsLoading = false;
                return;
            }
            try
            {
                var result = await Helper.InstaApi.TVProcessor.GetChannelByTypeAsync(ChannelType, Pagination);

                if (!result.Succeeded)
                {
                    IsLoading = false;
                    return;
                }

                if (result.Value.MaxId == null || !result.Value.HasMoreAvailable)
                    HasMore = HasMoreItems = false;

                Pagination.NextMaxId = result.Value.MaxId;
                if (result.Value.Items != null && result.Value.Items.Any())
                    Items.AddRange(result.Value.Items);
                Title = result.Value.Title;
                try
                {
                    if (IsFirstRun)
                    {
                        IsFirstRun = false;
                        //MainPage.Current.MediaUc.SetMedia(Items[0]);
                    }
                }
                catch { }
                _LastPage++;
                IsLoading = false;

            }
            catch (Exception ex)
            {
                IsLoading = false;
                ex.PrintException("GenerateChannelItems.LoadMoreItemsAsync");
            }
        }

        bool IsLoading = false;
        private async void Scroll_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                if (Items == null)
                    return;
                if (!Items.Any())
                    return;
                if (Scroll.HorizontalOffset > 0 && Scroll.ScrollableWidth > 150)
                {
                    if (Scroll.HorizontalOffset >= Scroll.ScrollableWidth - 140 && IsLoading == false)
                    {
                        IsLoading = true;
                        await RunLoadMoreAsync();
                    }
                }
            }
            catch (Exception ex) { ex.PrintException("Scroll_ViewChanging"); }
        }
    }
}
