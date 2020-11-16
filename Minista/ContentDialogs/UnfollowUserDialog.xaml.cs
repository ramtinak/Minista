using InstagramApiSharp.Classes.Models;
using Minista.Views.Infos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Minista.ContentDialogs
{
    public sealed partial class UnfollowUserDialog : ContentPopup
    {
        readonly InstaUserShort User;
        public UnfollowUserDialog(InstaUserShortFriendship userShortFriendship)
            : this(userShortFriendship.ToUserShort()) { }

        public UnfollowUserDialog(InstaUserShort userShort)
        {
            this.InitializeComponent();
            User = userShort;
            Image.UriSource = new Uri(userShort.ProfilePicture);
            UnfollowUserText.Text = $"Unfollow @{userShort.UserName}?";
        }
        private void CancelButtonClick(object sender, RoutedEventArgs e) => Hide();

        private async void UnfollowButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        Helper.MuteRequested(User.Pk);
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                        {
                            var unfollow = await Helper.InstaApi.UserProcessor.UnFollowUserAsync(User.Pk);
                            if (unfollow.Succeeded)
                                MainPage.Current.ShowInAppNotify($"@{User.UserName} unfollowed.");
                        });
                    }
                    catch { }
                });
            }
            catch { }
        }

        private void ImageTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                Hide();
                Helper.OpenProfile(User.ToUserShortFriendship().ToListExt());
            }
            catch { }
        }
    }
}
