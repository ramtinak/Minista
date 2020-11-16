using InstagramApiSharp.Classes.Models;
using Minista.Helpers;
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
    public sealed partial class RemoveUserDialog : ContentPopup
    {
        readonly InstaUserShort User;
        public RemoveUserDialog(InstaUserShortFriendship userShortFriendship)
            : this(userShortFriendship.ToUserShort()) { }
        public RemoveUserDialog(InstaUserShort userShort)
        {
            this.InitializeComponent();
            User = userShort;
            Image.UriSource = new Uri(userShort.ProfilePicture);
            RemoveUserText.Text = $"We won't tell @{userShort.UserName} they were removed from your followers.";
        }
        private void CancelButtonClick(object sender, RoutedEventArgs e) => Hide();

        private async void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        RemoveUser(User.Pk);
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                        {
                            var unfollow = await Helper.InstaApi.UserProcessor.RemoveFollowerAsync(User.Pk);
                            if (unfollow.Succeeded)
                                MainPage.Current.ShowInAppNotify($"@{User.UserName} removed from your followers.");
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
        public static void RemoveUser(long userPk)
        {
            try
            {
                if (NavigationService.Frame.Content is FollowView view && view != null)
                {
                    var list = view.FollowVM.FollowersGenerator.Items.ToList();
                    for(int i =0; i<list.Count;i++)
                    {
                        if(list[i].Pk == userPk)
                        {
                            view.FollowVM.FollowersGenerator.Items.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
            catch { }
        }

    }
}
