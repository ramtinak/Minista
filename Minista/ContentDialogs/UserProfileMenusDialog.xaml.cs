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
    public sealed partial class UserProfileMenusDialog : ContentPopup
    {
        enum CommandType
        {
            Block,
            Unblock,
            ShowYourStory,
            HideYourStory,
            DownloadProfilePicture,
            CopyUsername,
            CopyUsernameAddress,
            ShareThisProfile,
            Mute,
            UnMute,
            SendMessage,
            Cancel
        }
        class MenuClass : BaseModel
        {
            public CommandType Command { get; set; }
            string text_;
            public string Text { get { return text_; } set { text_ = value; OnPropertyChanged("Text"); } }
        }
        private readonly InstaUserInfo User;
        private readonly InstaStoryFriendshipStatus FriendshipStatus;
        public UserProfileMenusDialog(InstaUserInfo userInfo, InstaStoryFriendshipStatus friendshipStatus)
        {
            this.InitializeComponent();
            User = userInfo;
            FriendshipStatus = friendshipStatus;
            LVMenu.Items.Add(new MenuClass { Command = CommandType.SendMessage, Text = "Send message" });
            if (FriendshipStatus != null)
            {
                if (FriendshipStatus.Blocking)
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.Unblock, Text = "Unblock" });
                else
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.Block, Text = "Block" });
            }
            LVMenu.Items.Add(new MenuClass { Command = CommandType.DownloadProfilePicture, Text = "Download profile picture" });
            if (FriendshipStatus != null)
            {
                if (FriendshipStatus.IsBlockingReel)
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.ShowYourStory, Text = "Show Your Story" });
                else
                    LVMenu.Items.Add(new MenuClass { Command = CommandType.HideYourStory, Text = "Hide Your Story" });
                if (FriendshipStatus.FollowedBy || FriendshipStatus.Following)
                {
                    if (FriendshipStatus.Muting)
                        LVMenu.Items.Add(new MenuClass { Command = CommandType.UnMute, Text = "Unmute" });
                    else
                        LVMenu.Items.Add(new MenuClass { Command = CommandType.Mute, Text = "Mute" });
                }
            }

            LVMenu.Items.Add(new MenuClass { Command = CommandType.CopyUsername, Text = "Copy username" });
            LVMenu.Items.Add(new MenuClass { Command = CommandType.CopyUsernameAddress, Text = "Copy username address" });
            LVMenu.Items.Add(new MenuClass { Command = CommandType.ShareThisProfile, Text = "Share this profile" });


            LVMenu.Items.Add(new MenuClass { Command = CommandType.Cancel, Text = "Cancel" });

        }

        private async void LVMenuItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem != null && e.ClickedItem is MenuClass menu && menu != null)
                {
                    try
                    {
                        switch (menu.Command)
                        {
                        
                            case CommandType.CopyUsername:
                                User.UserName.CopyText();
                                MainPage.Current.ShowInAppNotify($"{User.UserName} copied ;)", 1500);
                                Hide();
                                break;
                            case CommandType.CopyUsernameAddress:
                                var uAddress = $"https://instagram.com/{User.UserName.ToLower()}";
                                uAddress.CopyText();
                                MainPage.Current.ShowInAppNotify($"{uAddress} copied ;)", 1500);
                                Hide();
                                break;
                            case CommandType.DownloadProfilePicture:
                                StartDownload();
                                Hide();
                                break;
                            case CommandType.ShareThisProfile:
                                Hide();
                                await new UsersDialog(User.ToUserShort()).ShowAsync();
                                break;
                            case CommandType.Block:
                                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    var result = await Helper.InstaApi.UserProcessor.BlockUserAsync(User.Pk);
                                    if (result.Succeeded)
                                    {
                                        if (FriendshipStatus != null)
                                            FriendshipStatus.Blocking = result.Value.Blocking;
                                        Helper.ShowNotify($"@{User.UserName.ToLower()} blocked...", 1500);
                                    }
                                });
                                break;
                            case CommandType.Unblock:
                                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    var result = await Helper.InstaApi.UserProcessor.UnBlockUserAsync(User.Pk);
                                    if (result.Succeeded)
                                    {
                                        if (FriendshipStatus != null)
                                            FriendshipStatus.Blocking = result.Value.Blocking;
                                        Helper.ShowNotify($"@{User.UserName.ToLower()} unblocked...", 1500);
                                    }
                                });
                                break;
                            case CommandType.ShowYourStory:
                                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    var result = await Helper.InstaApi.UserProcessor.UnHideMyStoryFromUserAsync(User.Pk);
                                    if (result.Succeeded)
                                    {
                                        if (FriendshipStatus != null)
                                            FriendshipStatus.IsBlockingReel = result.Value.IsBlockingReel;

                                        Helper.ShowNotify($"Your stories is now visibile for @{User.UserName.ToLower()}...", 1500);
                                    }
                                });
                                break;
                            case CommandType.HideYourStory:
                                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    var result = await Helper.InstaApi.UserProcessor.HideMyStoryFromUserAsync(User.Pk);
                                    if (result.Succeeded)
                                    {
                                        if (FriendshipStatus != null)
                                            FriendshipStatus.IsBlockingReel = result.Value.IsBlockingReel;
                                        Helper.ShowNotify($"Your stories is now hidden for @{User.UserName.ToLower()}...", 1500);
                                    }
                                });
                                break;
                            case CommandType.Mute:
                                Hide();
                                await new MuteDialog(User.ToUserShort()).ShowAsync();
                                break;
                            case CommandType.UnMute:
                                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    var result = await Helper.InstaApi.UserProcessor.UnMuteUserMediaAsync(User.Pk, InstagramApiSharp.Enums.InstaMuteOption.All);
                                    if (result.Succeeded && FriendshipStatus != null)
                                    {
                                        if (FriendshipStatus != null)
                                            FriendshipStatus.Muting = result.Value.Muting;
                                        Helper.ShowNotify($"@{User.UserName.ToLower()} unmuted...", 1500);
                                    }
                                    
                                });
                                break;
                            case CommandType.SendMessage:
                                try
                                {
                                    var userFriendShip = new InstaUserShortFriendship
                                    {
                                        UserName = User.UserName,
                                        Pk = User.Pk,
                                        FullName = User.FullName,
                                        ProfilePicUrl = User.ProfilePicUrl,
                                        ProfilePicture = User.ProfilePicture,
                                        HasAnonymousProfilePicture = User.HasAnonymousProfilePicture,
                                        IsBestie = User.IsBestie,
                                        IsPrivate = User.IsPrivate,
                                        IsVerified = User.IsVerified,
                                        ProfilePictureId = User.ProfilePictureId,
                                        FriendshipStatus = new InstaFriendshipShortStatus
                                        {
                                            Following = FriendshipStatus.Following,
                                            IncomingRequest = FriendshipStatus.IncomingRequest,
                                            IsBestie = FriendshipStatus.IsBestie,
                                            IsPrivate = FriendshipStatus.IsPrivate,
                                            OutgoingRequest = FriendshipStatus.OutgoingRequest,
                                            Pk = User.Pk
                                        }
                                    };
                                    NavigationService.Navigate(typeof(Views.Direct.ThreadView), userFriendShip);
                                }
                                catch { }
                                break;
                        }

                        Hide();
                    }
                    catch { Hide(); }
                }
            }
            catch { Hide(); }
        }

        void StartDownload()
        {
            try
            {
                var url = User.HdProfilePicUrlInfo.Uri;
                DownloadHelper.Download(url, url, false, User.UserName,null,false,false,true);
            }
            catch { }
        }
    }
}
