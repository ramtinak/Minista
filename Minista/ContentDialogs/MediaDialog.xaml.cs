using InstagramApiSharp.Classes.Models;
using Minista.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Minista.ContentDialogs
{
    public sealed partial class MediaDialog : ContentPopup
    {
        enum CommandType
        {
            CopyCaption,
            CopyUrl,
            Download,
            DownloadCurrent,
            Mute,
            Unfollow,
            Follow,
            Cancel,

            CopyProfile,
            CopyProfileAddress,
            CopyHashtag,
            // hashtag
            DontShowHashtag,
            UnfollowHashtag,
            Commenting,
            EditPost,
            Archive,
            UnArchive,
            Delete,
            Repost,
            ShareAsStory
        }
        class MenuClass : BaseModel
        {
            public CommandType Command { get; set; }
            string text_;
            public string Text { get { return text_; } set { text_ = value; OnPropertyChanged("Text"); } }
        }
        public InstaMedia Media;
        public int SelectedIndex = -1;
        public MediaDialog(InstaMedia media, bool isArchivePost = false, int selectedIndex =-1)
        {
            InitializeComponent();
            Media = media;
            SelectedIndex = selectedIndex;
            LVMenu.Items.Add(new MenuClass { Command = CommandType.CopyUrl, Text = "Copy share link" });
            LVMenu.Items.Add(new MenuClass { Command = CommandType.Download, Text = "Download this post" });
            try
            {
                if (selectedIndex != -1 && media.MediaType == InstaMediaType.Carousel)
                {
                    var carousel = media.Carousel[selectedIndex];
                    if (carousel != null)
                    {
                        var title = "Download current " + (carousel.MediaType == InstaMediaType.Video ? "video" : "photo");
                        LVMenu.Items.Add(new MenuClass { Command = CommandType.DownloadCurrent, Text = title });
                    }
                }
            }
            catch { }
            try
            {
                if (media.Caption != null && media.Caption.Text?.Length > 0)
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.CopyCaption, Text = "Copy caption" });
            }
            catch { }

            if (!isArchivePost)
            {
                if (media.FollowHashtagInfo == null)
                {
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.CopyProfile, Text = "Copy username" });
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.CopyProfileAddress, Text = "Copy username address" });

                    if (media.User.Pk != Helper.CurrentUser.Pk)
                    {
                        if (media.IsMain)
                            LVMenu.Items.Add(new MenuClass { Command = CommandType.Unfollow, Text = "Unfollow" });
                        else
                        {
                            if (!media.User.FriendshipStatus?.Following ?? false)
                                LVMenu.Items.Add(new MenuClass { Command = CommandType.Follow, Text = "Follow" });
                            else
                                LVMenu.Items.Add(new MenuClass { Command = CommandType.Unfollow, Text = "Unfollow" });
                        }
                        LVMenu.Items.Add(new MenuClass { Command = CommandType.Mute, Text = "Mute" });
                    }
                }
                else
                {
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.CopyHashtag, Text = "Copy hashtag" });
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.DontShowHashtag, Text = "Don't show posts for this Hashtag" });
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.UnfollowHashtag, Text = "Unfollow Hashtag" });
                }
                if (media.User.Pk == Helper.CurrentUser.Pk)
                {
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.EditPost, Text = "Edit post" });

                    LVMenu.Items.Add(new MenuClass { Command = CommandType.Archive, Text = "Archive" });

                    if (media.IsCommentsDisabled)
                        LVMenu.Items.Add(new MenuClass { Command = CommandType.Commenting, Text = "Turn On Commenting" });
                    else
                        LVMenu.Items.Add(new MenuClass { Command = CommandType.Commenting, Text = "Turn Off Commenting" });
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.Delete, Text = "Delete" });
                }
                else
                {
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.Repost, Text = "Re-post" });
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.ShareAsStory, Text = "Share as story" });

                }
            }
            else
            {
                LVMenu.Items.Add(new MenuClass { Command = CommandType.UnArchive, Text = "Show on profile" });
                LVMenu.Items.Add(new MenuClass { Command = CommandType.Delete, Text = "Delete" });

            }
            LVMenu.Items.Add(new MenuClass { Command = CommandType.Cancel, Text = "Cancel" });
        }

        private async void LVMenuItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem != null && e.ClickedItem is MenuClass menu && menu != null)
            {
                try
                {
                    switch (menu.Command)
                    {
                        case CommandType.CopyUrl:
                            try
                            {
                                Hide();
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    var url = await Helper.InstaApi.MediaProcessor.GetShareLinkFromMediaIdAsync(Media.InstaIdentifier);
                                    if (url.Succeeded)
                                    {
                                        url.Value.ToString().CopyText();
                                        MainPage.Current.ShowInAppNotify("Url copied ;)", 1500);
                                    }
                                });
                            }
                            catch { }
                            break;
                        case CommandType.CopyCaption:
                            if (Media.Caption != null && !string.IsNullOrEmpty(Media.Caption.Text))
                            {
                                Media.Caption.Text.CopyText();
                                MainPage.Current.ShowInAppNotify("Caption copied ;)", 1500);
                            }
                            else
                                MainPage.Current.ShowInAppNotify("No caption found...", 1500);
                            break;
                        case CommandType.CopyHashtag:
                            if (Media.FollowHashtagInfo != null)
                            {
                                Media.FollowHashtagInfo.Name.CopyText();
                                MainPage.Current.ShowInAppNotify("Hashtag copied ;)", 1500);
                            }
                            break;
                        case CommandType.CopyProfile:
                                Media.User.UserName.CopyText();
                                MainPage.Current.ShowInAppNotify($"{Media.User.UserName} copied ;)", 1500);
                            break;
                        case CommandType.CopyProfileAddress:
                            var uAddress = $"https://instagram.com/{Media.User.UserName.ToLower()}";
                            uAddress.CopyText();
                            MainPage.Current.ShowInAppNotify($"{uAddress} copied ;)", 1500);
                            break;
                        case CommandType.Mute:
                            Hide();
                            await new MuteDialog(Media.User).ShowAsync();
                            break;
                        case CommandType.Unfollow:
                            Hide();
                            await new UnfollowUserDialog(Media.User).ShowAsync();
                            break;
                        case CommandType.Follow:
                            Hide();
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                var followResult = await Helper.InstaApi.UserProcessor.FollowUserAsync(Media.User.Pk);
                                if (followResult.Succeeded)
                                {
                                    if (followResult.Value.OutgoingRequest)
                                        Helper.ShowNotify($"Follow requested to @{Media.User.UserName.ToLower()} .", 2500);
                                    else if (followResult.Value.Following)
                                        Helper.ShowNotify($"@{Media.User.UserName.ToLower()} followed successfully.", 2500);
                                }
                            });
                            break;
                        case CommandType.Download:
                            StartDownload();
                            break;
                        case CommandType.DownloadCurrent:
                            StartDownloadCurrent();
                            break;

                        //hashtag
                        case CommandType.UnfollowHashtag:
                            Hide();
                            await new UnfollowHashtagDialog(Media.FollowHashtagInfo).ShowAsync();
                            break;
                        case CommandType.DontShowHashtag:
                            try
                            {
                                Hide();
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    Helper.DontShowThisItemRequested(Media.InstaIdentifier);
                                    //var dismissSuggestion = await Helper.InstaApi.DiscoverProcessor
                                    //.DismissSuggestionAsync(Media.FollowHashtagInfo.Id.ToString());
                                    var reportHashtag = await Helper.InstaApi.HashtagProcessor
                                    .ReportHashtagMediaAsync(Media.FollowHashtagInfo.Name.ToLower(), Media.FollowHashtagInfo.Id.ToString(), Media.InstaIdentifier);
                                }); 
                            }
                            catch { }
                            break;
                        case CommandType.Commenting:
                            try
                            {
                                if (Media?.User.Pk == Helper.CurrentUser.Pk)
                                {
                                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                    {
                                        if (menu.Text.ToLower().Contains("on"))
                                        {
                                            var result = await Helper.InstaApi.CommentProcessor.EnableMediaCommentAsync(Media.InstaIdentifier);
                                            if (result.Succeeded)
                                            {
                                                Media.IsCommentsDisabled = false;
                                                menu.Text = "Turn Off Commenting";
                                            }
                                        }
                                        else
                                        {
                                            var result = await Helper.InstaApi.CommentProcessor.DisableMediaCommentAsync(Media.InstaIdentifier);
                                            if (result.Succeeded)
                                            {
                                                Media.IsCommentsDisabled = false;
                                                menu.Text = "Turn On Commenting";
                                            }
                                        }
                                    });
                                }
                            }
                            catch { }
                            break;
                        case CommandType.Archive:
                            try
                            {
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    var archive = await Helper.InstaApi.MediaProcessor.ArchiveMediaAsync(Media.InstaIdentifier);
                                    if (!archive.Succeeded)
                                        archive.Info.Message.ShowErr(archive.Info.Exception);
                                    else
                                    {
                                        try
                                        {
                                            if (NavigationService.Frame.Content is Views.Infos.ProfileDetailsView prof)
                                            {
                                                if (prof.ProfileDetailsVM.MediaGeneratror.Items.Count > 0)
                                                {
                                                    for (int i = 0; i < prof.ProfileDetailsVM.MediaGeneratror.Items.Count; i++)
                                                    {
                                                        try
                                                        {
                                                            if (prof.ProfileDetailsVM.MediaGeneratror.Items[i].InstaIdentifier == Media.InstaIdentifier)
                                                            {
                                                                prof.ProfileDetailsVM.MediaGeneratror.Items.RemoveAt(i);
                                                                break;
                                                            }
                                                        }
                                                        catch { }
                                                    }
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                });
                            }
                            catch { }
                            break;
                        case CommandType.UnArchive:
                            try
                            {
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    var unArchive = await Helper.InstaApi.MediaProcessor.UnArchiveMediaAsync(Media.InstaIdentifier);
                                    if (!unArchive.Succeeded)
                                        unArchive.Info.Message.ShowErr(unArchive.Info.Exception);
                                    else
                                    {
                                        try
                                        {
                                            if (NavigationService.Frame.Content is Views.Infos.ArchiveView arch)
                                            {
                                                if(arch.ArchiveVM.Items.Count > 0)
                                                {
                                                    for (int i = 0; i < arch.ArchiveVM.Items.Count; i++)
                                                    {
                                                        try
                                                        {
                                                            if (arch.ArchiveVM.Items[i].InstaIdentifier == Media.InstaIdentifier)
                                                            {
                                                                arch.ArchiveVM.Items.RemoveAt(i);
                                                                break;
                                                            }
                                                        }
                                                        catch { }
                                                    }
                                                    if (arch.ArchiveVM.Items.Count == 0)
                                                    {
                                                        arch.ArchiveVM.NoArchivedPostsVisibility = Visibility.Visible;
                                                        arch.ScrollableArchivePostUc.Visibility = Visibility.Collapsed;
                                                    }
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                });
                            }
                            catch { }
                            break;
                        case CommandType.Delete:
                            try
                            {
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    var delete = await Helper.InstaApi.MediaProcessor.DeleteMediaAsync(Media.InstaIdentifier, Media.MediaType);
                                    if (!delete.Succeeded)
                                        delete.Info.Message.ShowErr(delete.Info.Exception);
                                    else
                                    {
                                        try
                                        {
                                            if (NavigationService.Frame.Content is Views.Infos.ArchiveView arch)
                                            {
                                                if (arch.ArchiveVM.Items.Count > 0)
                                                {
                                                    for (int i = 0; i < arch.ArchiveVM.Items.Count; i++)
                                                    {
                                                        try
                                                        {
                                                            if (arch.ArchiveVM.Items[i].InstaIdentifier == Media.InstaIdentifier)
                                                            {
                                                                arch.ArchiveVM.Items.RemoveAt(i);
                                                                break;
                                                            }
                                                        }
                                                        catch { }
                                                    }
                                                    if (arch.ArchiveVM.Items.Count == 0)
                                                    {
                                                        arch.ArchiveVM.NoArchivedPostsVisibility = Visibility.Visible;
                                                        arch.ScrollableArchivePostUc.Visibility = Visibility.Collapsed;
                                                    }
                                                }
                                            }
                                            else if (NavigationService.Frame.Content is Views.Infos.ProfileDetailsView prof)
                                            {
                                                if (prof.ProfileDetailsVM.MediaGeneratror.Items.Count > 0)
                                                {
                                                    for (int i = 0; i < prof.ProfileDetailsVM.MediaGeneratror.Items.Count; i++)
                                                    {
                                                        try
                                                        {
                                                            if (prof.ProfileDetailsVM.MediaGeneratror.Items[i].InstaIdentifier == Media.InstaIdentifier)
                                                            {
                                                                prof.ProfileDetailsVM.MediaGeneratror.Items.RemoveAt(i);
                                                                break;
                                                            }
                                                        }
                                                        catch { }
                                                    }
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                });
                            }
                            catch { }
                            break;
                        case CommandType.EditPost:
                            Hide();
                            NavigationService.Navigate(typeof(Views.Posts.EditPostView), Media);
                            break;
                        case CommandType.Repost:
                        case CommandType.ShareAsStory:
                            "This feature is not available for you, only available for the DEVELOPER".ShowMsg("Not available");
                            break;
                        default:
                            break;
                    }
                    Hide();
                }
                catch { }
            }
        }
        void StartDownloadCurrent()
        {
            try
            {
                var album = Media.Carousel[SelectedIndex];
                if (album.MediaType == InstaMediaType.Image)
                {
                    var url = album.Images?[0].Uri;
                    DownloadHelper.Download(url, url, false, Media.User.UserName, Media.Caption?.Text);
                }
                else
                {
                    var url = album.Videos?[0].Uri;
                    DownloadHelper.Download(url, album.Images?[0].Uri, true, Media.User.UserName, Media.Caption?.Text);
                }
                var caption = "";
                if (Media.Caption != null && !string.IsNullOrEmpty(Media.Caption.Text))
                {
                    caption = Media.Caption.Text;
                    if (caption.Length > 45)
                        caption = caption.Substring(0, 45);
                }
                MainPage.Current.ShowInAppNotify($"{caption} download started", 1500);
            }
            catch { }
        }
        void StartDownload()
        {
            try
            {
                switch (Media.MediaType)
                {
                    case InstaMediaType.Image:
                        {
                            var url = Media.Images?[0].Uri;
                            DownloadHelper.Download(url, url,false,Media.User.UserName, Media.Caption?.Text);
                        }
                        break;
                    case InstaMediaType.Video:
                        {
                            var url = Media.Videos?[0].Uri;
                            DownloadHelper.Download(url, Media.Images?[0].Uri, true, Media.User.UserName, Media.Caption?.Text);
                        }
                        break;
                    case InstaMediaType.Carousel:
                        foreach (var album in Media.Carousel)
                        {
                            if (album.MediaType == InstaMediaType.Image)
                            {
                                var url = album.Images?[0].Uri;
                                DownloadHelper.Download(url, url, false, Media.User.UserName, Media.Caption?.Text);
                            }
                            else
                            {
                                var url = album.Videos?[0].Uri;
                                DownloadHelper.Download(url, album.Images?[0].Uri, true, Media.User.UserName, Media.Caption?.Text);
                            }
                        }
                        break;
                }
                var caption = "";
                if (Media.Caption != null && !string.IsNullOrEmpty(Media.Caption.Text))
                {
                    caption = Media.Caption.Text;
                    if (caption.Length > 45)
                        caption = caption.Substring(0, 45);
                }
                MainPage.Current.ShowInAppNotify($"{caption} download started", 1500);
            }
            catch { }
        }


        private async void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is MenuClass menu && menu != null)
            {
                try
                {
                    switch (menu.Command)
                    {
                        case CommandType.CopyUrl:
                            Media.Url.CopyText();
                            MainPage.Current.ShowInAppNotify("Url copied ;)", 1500);
                            break;
                        case CommandType.CopyCaption:
                            if (Media.Caption != null && !string.IsNullOrEmpty(Media.Caption.Text))
                            {
                                Media.Caption.Text.CopyText();
                                MainPage.Current.ShowInAppNotify("Caption copied ;)", 1500);
                            }
                            else
                                MainPage.Current.ShowInAppNotify("No caption found...", 1500);

                            break;
                        case CommandType.Mute:
                            Hide();
                            await new MuteDialog(Media.User).ShowAsync();
                            break;
                        case CommandType.Unfollow:
                            try
                            {
                                Helper.MuteRequested(Media.User.Pk);
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    var unfollow = await Helper.InstaApi.UserProcessor.UnFollowUserAsync(Media.User.Pk);
                                    if (unfollow.Succeeded)
                                        MainPage.Current.ShowInAppNotify($"{Media.User.UserName} unfollowed...", 1000);
                                });
                            }
                            catch { }
                            break;
                        case CommandType.Download:
                            StartDownload();
                            break;


                        //hashtag
                        case CommandType.UnfollowHashtag:
                            Hide();
                            await new UnfollowHashtagDialog(Media.FollowHashtagInfo).ShowAsync();
                            break;
                        case CommandType.DontShowHashtag:
                            try
                            {
                                Hide();
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    Helper.DontShowThisItemRequested(Media.InstaIdentifier);
                                    //var dismissSuggestion = await Helper.InstaApi.DiscoverProcessor
                                    //.DismissSuggestionAsync(Media.FollowHashtagInfo.Id.ToString());
                                    var reportHashtag = await Helper.InstaApi.HashtagProcessor
                                    .ReportHashtagMediaAsync(Media.FollowHashtagInfo.Name.ToLower(), Media.FollowHashtagInfo.Id.ToString(), Media.InstaIdentifier);
                                });
                            }
                            catch { }
                            break;

                        default:
                            break;
                    }
                    Hide();
                }
                catch { }
            }

        }
    }
}
