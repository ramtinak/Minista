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
    public sealed partial class UnfollowHashtagDialog : ContentPopup
    {
        readonly InstaFollowHashtagInfo HashtagInfo;
        public UnfollowHashtagDialog(InstaFollowHashtagInfo hashtagInfo)
        {
            this.InitializeComponent();
            HashtagInfo = hashtagInfo;
            Image.UriSource = new Uri(hashtagInfo.ProfilePicture);
            UnfollowHashtagText.Text = $"Unfollow #{hashtagInfo.Name}?";
        }
        private void CancelButtonClick(object sender, RoutedEventArgs e) => Hide();

        private async void UnfollowButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    Helper.MuteHashtagRequested(HashtagInfo.Name);
                    var result = await Helper.InstaApi.HashtagProcessor.UnFollowHashtagAsync(HashtagInfo.Name.ToLower());
                    if (result.Succeeded)
                        MainPage.Current.ShowInAppNotify($"#{HashtagInfo.Name} unfollowed", 1200);
                });
            }
            catch { }
        }
    }
}
