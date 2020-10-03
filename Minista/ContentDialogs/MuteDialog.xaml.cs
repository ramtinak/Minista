using InstagramApiSharp.Classes.Models;
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
    public sealed partial class MuteDialog : ContentPopup
    {
        readonly InstaUserShort User;
        public MuteDialog(InstaUserShort user)
        {
            this.InitializeComponent();
            User = user;
            MuteUser.Text = $"Mute @{user.UserName.ToLower()}?";
        }
        private async void MutePostsButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    Helper.MuteRequested(User.Pk);
                    var result = await Helper.InstaApi.UserProcessor.MuteUserMediaAsync(User.Pk, InstagramApiSharp.Enums.InstaMuteOption.Post);
                    if (result.Succeeded)
                        MainPage.Current.ShowInAppNotify($"{User.UserName}'s posts is muted now", 1200);
                });
            }
            catch { }
        }

        private async void MutePostsAndStoryButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    Helper.MuteRequested(User.Pk);
                    var result = await Helper.InstaApi.UserProcessor.MuteUserMediaAsync(User.Pk, InstagramApiSharp.Enums.InstaMuteOption.All);
                    if (result.Succeeded)
                        MainPage.Current.ShowInAppNotify($"{User.UserName}'s posts and stories is muted now", 1200);
                });
            }
            catch { }
        }
        
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

    }
}
