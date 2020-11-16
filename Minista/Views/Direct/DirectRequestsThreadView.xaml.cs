using InstagramApiSharp.Classes;
using InstagramApiSharp.Helpers;
using InstagramApiSharp.Classes.Models;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Minista.Helpers;
using Minista.UserControls.Direct;
using Minista.ViewModels.Direct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using System.Threading.Tasks;

namespace Minista.Views.Direct
{
    public sealed partial class DirectRequestsThreadView : Page
    {
        InstaDirectInboxItem CurrentDirectInboxItem;
        AppBarButton VoicePlayPauseButton;
        readonly Random Rnd = new Random();

        public InstaDirectInboxThread Thread;
        private bool CanLoadFirstPopUp = false;
        NavigationMode NavigationMode;
        public static DirectRequestsThreadView Current;
        public DirectRequestsThreadView()
        {
            this.InitializeComponent();
            Current = this;
            Loaded += DirectRequestsThreadViewLoaded;
        }
        async void PauseVideo()
        {
            try
            {
                try
                {
                    ME.Source = null;
                    ME.Pause();
                    if (VoicePlayPauseButton != null)
                        VoicePlayPauseButton.Content = Helper.PlayMaterialIcon;
                }
                catch (Exception ex) { ex.PrintException("PauseVideo()"); }
                await Task.Delay(350);
            }
            catch { }
        }

        private async void DirectRequestsThreadViewLoaded(object sender, RoutedEventArgs e)
        {
            if (NavigationMode == NavigationMode.Back && DirectRequestsThreadVM.CurrentThread != null)
            {
                if (DirectRequestsThreadVM.CurrentThread.ThreadId.ToLower() == Thread.ThreadId.ToLower())
                    return;
            }
            else if (NavigationMode == NavigationMode.New)
            {
                GetType().RemovePageFromBackStack();
                NavigationCacheMode = NavigationCacheMode.Enabled;
                CanLoadFirstPopUp = false;
            }
            try
            {
                if (!CanLoadFirstPopUp)
                {
                    PauseVideo();
                    UsernameText.Text = "";
                    UserInfoText.Text = "";
                    await Task.Delay(250);
                    DirectRequestsThreadVM.SetThread(Thread);
                    await Task.Delay(150);
                    try
                    {
                        UserImage.Fill = DirectRequestsThreadVM.CurrentThread.Users[0].ProfilePicture.GetImageBrush();
                    }
                    catch { }
                    try
                    {
                        UsernameText.Text = $"@{DirectRequestsThreadVM.CurrentThread.Users[0].UserName.ToLower()} wants to send you a message.";
                    }
                    catch { }
                    try
                    {
                        UsernameText.Text = $"@{DirectRequestsThreadVM.CurrentThread.Users[0].UserName.ToLower()} wants to send you a message.";
                    }
                    catch { }
                    try
                    {
                        RunUser.Text = $"@{DirectRequestsThreadVM.CurrentThread.Users[0].UserName.ToLower()}";
                    }
                    catch { }
                    CanLoadFirstPopUp = true;
                    try
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                        {
                            var result = await Helper.InstaApi.UserProcessor.GetUserInfoByIdAsync(DirectRequestsThreadVM.CurrentThread.Users[0].Pk);
                            if (result.Succeeded)
                            {
                                UserInfoText.Text = $"{result.Value.FollowerCount} follower{(result.Value.FollowerCount > 1 ? "s" : "")} {Helper.MiddleDot} " +
                                $"{result.Value.MediaCount} post{(result.Value.MediaCount > 1 ? "s" : "")}";
                            }
                        });
                    }
                    catch { }
                }
            }
            catch { }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationMode = e.NavigationMode;
            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();

            if (e.Parameter != null && e.Parameter is InstaDirectInboxThread thread)
                Thread = thread;
        }


        private void UserImageTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                Helper.OpenProfile(DirectRequestsThreadVM.CurrentThread.Users[0]);
            }
            catch { }
        }


        #region Direct Stuff
        private void MenuGridRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            "MenuGridRightTapped".PrintDebug();
            //try
            //{
            //    if (sender is Grid item && item.DataContext is InstaDirectInboxItem data && data.UserId == Helper.CurrentUser.Pk)
            //        FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            //}
            //catch { }
        }

        private void MenuGridHolding(object sender, HoldingRoutedEventArgs e)
        {
            "MenuGridHolding".PrintDebug();
            //try
            //{
            //    if (sender is Grid item && item.DataContext is InstaDirectInboxItem data && data.UserId == Helper.CurrentUser.Pk)
            //        FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            //}
            //catch { }
        }
        private void CopyTextFlyoutClick(object sender, RoutedEventArgs e)
        {
        }
        private void MEMediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (VoicePlayPauseButton != null)
                    VoicePlayPauseButton.Content = Helper.PlayMaterialIcon;
            }
            catch { }
        }

        private void MEMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                if (VoicePlayPauseButton != null)
                    VoicePlayPauseButton.Content = Helper.PlayMaterialIcon;
            }
            catch { }
        }

        private void MEMediaOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                ME.Play();
            }
            catch { }
        }
        private void PlayPauseButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is AppBarButton button && button != null)
                {
                    if (button.DataContext is InstaDirectInboxItem directInboxItem && directInboxItem != null &&
                        directInboxItem.ItemType == InstaDirectThreadItemType.VoiceMedia && directInboxItem.VoiceMedia != null &&
                        directInboxItem.VoiceMedia?.Media?.Audio != null)
                    {

                        if (VoicePlayPauseButton == null)
                            VoicePlayPauseButton = button;
                        else if (VoicePlayPauseButton.Tag.ToString() != button.Tag.ToString())
                        {
                            VoicePlayPauseButton.Content = Helper.PlayMaterialIcon;
                            VoicePlayPauseButton = button;
                        }

                        if (button.Content.ToString() == Helper.PlayMaterialIcon)// play
                        {
                            if (CurrentDirectInboxItem == null)
                            {
                                CurrentDirectInboxItem = directInboxItem;
                                ME.Source = new Uri(directInboxItem.VoiceMedia.Media.Audio.AudioSource);
                            }
                            else if (CurrentDirectInboxItem.ItemId != directInboxItem.ItemId)
                            {
                                CurrentDirectInboxItem = directInboxItem;
                                ME.Source = new Uri(directInboxItem.VoiceMedia.Media.Audio.AudioSource);
                            }
                            else ME.Play();

                            button.Content = Helper.PauseMaterialIcon;
                        }
                        else
                        {
                            ME.Pause();
                            button.Content = Helper.PlayMaterialIcon;
                        }
                    }
                }
            }
            catch { }
        }
        private void MediaElementTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is MediaElement me && me != null)
                {
                    if (me.CurrentState != MediaElementState.Playing)
                        me.Play();
                    else
                        me.Pause();
                }
            }
            catch { }
        }

        private void UnsendMessageFlyoutClick(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    if (sender is MenuFlyoutItem item && item.DataContext is InstaDirectInboxItem data && data != null &&
            //        data.UserId == Helper.CurrentUser.Pk)
            //    {
            //        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            //        {
            //            var result = await Helper.InstaApi.MessagingProcessor.DeleteSelfMessageAsync(ThreadVM.CurrentThread.ThreadId, data.ItemId);
            //            if (result.Succeeded)
            //                ThreadVM.RemoveItem(data.ItemId);
            //            else
            //                Helper.ShowNotify(result.Info.Message, 2200);
            //        });
            //    }
            //}
            //catch { }
        }
        private void MediaShareImageExTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is ImageEx image && image != null)
                {
                    if (image.DataContext is InstaDirectInboxItem threadItem && threadItem?.ItemType == InstaDirectThreadItemType.MediaShare)
                    {
                        NavigationService.Navigate(typeof(Posts.SinglePostView), threadItem.MediaShare);
                    }
                }
            }
            catch { }

        }

        private void ProfileGridTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid grid && grid != null)
                {
                    if (grid.DataContext is InstaDirectInboxItem threadItem && threadItem?.ItemType == InstaDirectThreadItemType.Profile)
                    {
                        Helper.OpenProfile(threadItem.ProfileMedia);
                    }
                }
            }
            catch { }
        }

        private void FelixShareGridTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid grid && grid != null)
                {
                    if (grid.DataContext is InstaDirectInboxItem threadItem && threadItem?.ItemType == InstaDirectThreadItemType.FelixShare)
                    {
                        NavigationService.Navigate(typeof(Posts.SinglePostView), threadItem.FelixShareMedia);
                    }
                }
            }
            catch { }
        }
        private void VoiceMediaGridDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                if (sender is Grid WaveGrid && args.NewValue is InstaDirectInboxItem directInboxItem && directInboxItem != null &&
                        directInboxItem.ItemType == InstaDirectThreadItemType.VoiceMedia && directInboxItem.VoiceMedia != null &&
                        directInboxItem.VoiceMedia?.Media?.Audio != null)
                {
                    WaveGrid.Children.Clear();
                    var waveFromData = directInboxItem.VoiceMedia.Media.Audio.WaveformData;
                    var black = (SolidColorBrush)Application.Current.Resources["DefaultForegroundColor"];
                    var rectwidth = WaveGrid.ActualWidth / waveFromData.Length;
                    if (rectwidth > 2.5)
                        rectwidth = 1.5;
                    var heightList = new double[] { 3.1, 3.2, 3.3, 3.4, 3.5 };
                    for (int i = 0; i < waveFromData.Length; i++)
                    {
                        try
                        {
                            var data = waveFromData[i];
                            var height = (data * WaveGrid.Height);
                            if (height <= 0)
                                height = heightList[Rnd.Next(heightList.Length)];
                            var width = rectwidth;
                            if (width - 0.276 > 0)
                                width -= 0.276;
                            Rectangle rect = new Rectangle() { Fill = black, Height = height, Width = width };
                            if (WaveGrid.ColumnDefinitions.Count <= waveFromData.Length)
                            {
                                WaveGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                            }
                            Grid.SetColumn(rect, i);
                            WaveGrid.Children.Add(rect);
                        }
                        catch (Exception ex)
                        {
                            ex.PrintException();
                        }
                    }
                }
            }
            catch { }
        }

        private void LinkMediaGridTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid grid && grid.DataContext is InstaDirectInboxItem directInboxItem && directInboxItem != null &&
                        directInboxItem.ItemType == InstaDirectThreadItemType.Link && directInboxItem.LinkMedia != null &&
                        directInboxItem.LinkMedia?.LinkContext?.LinkUrl != null)
                    UriHelper.HandleUri(directInboxItem.LinkMedia.LinkContext.LinkUrl);
            }
            catch { }
        }

        private void TextBlockDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                if (sender is TextBlock textBlock && textBlock.DataContext is string str && !string.IsNullOrEmpty(str))
                {
                    using (var pg = new PassageHelperX())
                    {
                        var passages = pg.GetInlines(str, HyperLinkHelper.HyperLinkClick);
                        textBlock.Inlines.Clear();
                        textBlock.FlowDirection = passages.Item2 ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                        passages.Item1.ForEach(item =>
                        textBlock.Inlines.Add(item));
                    }
                }
            }
            catch { }
        }
        private void UserProfileEllipseTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Ellipse elp && elp.DataContext is InstaDirectInboxItem data && data != null)
                {
                    var user = DirectRequestsThreadVM.CurrentThread.Users.FirstOrDefault(x => x.Pk == data.UserId);
                    if (user != null)
                        Helper.OpenProfile(user);
                }
            }
            catch { }
        }

        private void MediaImageExTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid imageEx && imageEx.DataContext is InstaDirectInboxItem data && data != null &&
                    data.ItemType == InstaDirectThreadItemType.Media && data.Media != null)
                    NavigationService.Navigate(typeof(Infos.ImageVideoView), data.Media);
            }
            catch { }
        }

        private void ReelShareImageExTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is ImageEx imageEx && imageEx.DataContext is InstaDirectInboxItem data && data != null &&
                    data.ItemType == InstaDirectThreadItemType.ReelShare && data.ReelShareMedia != null)
                {
                    var reel = new InstaReelFeed
                    {
                        CreatedAt = data.ReelShareMedia.Media.TakenAt,
                        Id = data.ReelShareMedia.Media.Id,
                        User = data.ReelShareMedia.Media.User.ToUserShortFriendshipFull(),
                        CanReply = data.ReelShareMedia.Media.CanReply,

                    };
                    reel.Items.Add(data.ReelShareMedia.Media);

                    NavigationService.Navigate(typeof(Main.StoryView), reel);
                }
            }
            catch { }
        }

        private void VisualMediaTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid imageEx && imageEx.DataContext is InstaDirectInboxItem data && data != null &&
                    data.ItemType == InstaDirectThreadItemType.RavenMedia && data.VisualMedia != null)
                {
                    if (!data.VisualMedia.IsExpired && data.VisualMedia.SeenCount != null)
                        NavigationService.Navigate(typeof(RavenMediaView), new object[] { DirectRequestsThreadVM.CurrentThread, data });
                }
            }
            catch { }
        }


        private void StoryShareGridTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid item && item.DataContext is InstaDirectInboxItem data && data != null &&
                data.ItemType == InstaDirectThreadItemType.StoryShare && data.StoryShare != null && data.StoryShare.Media != null)
                {
                    NavigationService.Navigate(typeof(Main.StoryView),
                        new object[] { data.StoryShare.Media.User.Pk, data.StoryShare.Media.Pk.ToString(),
                            data.StoryShare.Media.Pk.ToString(), data.StoryShare.Media.Pk.ToString(), });
                }
            }
            catch { }
        }

        #endregion Direct Stuff








        #region SINGLE
        private async void BlockSingleToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                BlockSingleToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        var user = DirectRequestsThreadVM.CurrentThread.Users.FirstOrDefault();
                        var result = await Helper.InstaApi.UserProcessor.BlockUserAsync(user.Pk);
                        if (result.Succeeded)
                        {
                            await Helper.InstaApi.MessagingProcessor.DeclineDirectPendingRequestsAsync(DirectRequestsThreadVM.CurrentThread.ThreadId);
                            Helper.ShowNotify($"@{user.UserName.ToLower()} blocked...", 3000);
                            RemoveThreadAndGoBack();
                        }
                        else
                        {
                            if (result.Info.ResponseType == ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 3000);
                            else
                                Helper.ShowNotify($"Something unexpected happened: {result.Info?.Message}", 3000);
                        }

                    }
                    catch { }
                });
            }
            catch { }
        }

        private async void DeleteSingleToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteSingleToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        var result = await Helper.InstaApi.MessagingProcessor.DeclineDirectPendingRequestsAsync(DirectRequestsThreadVM.CurrentThread.ThreadId);
                        if (result.Succeeded)
                        {
                            Helper.ShowNotify($"'{DirectRequestsThreadVM.CurrentThread.Title}' deleted...", 3000);
                            RemoveThreadAndGoBack();
                        }
                        else
                        {
                            if (result.Info.ResponseType == ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 3000);
                            else
                                Helper.ShowNotify($"Something unexpected happened: {result.Info?.Message}", 3000);
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }

        private async void AcceptSingleToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AcceptSingleToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        var result = await Helper.InstaApi.MessagingProcessor.ApproveDirectPendingRequestAsync(DirectRequestsThreadVM.CurrentThread.ThreadId);
                        if (result.Succeeded)
                        {
                            Helper.ShowNotify($"'{DirectRequestsThreadVM.CurrentThread.Title}' approved...", 3000);
                            RemoveThreadAndGoBack();
                        }
                        else
                        {
                            if (result.Info.ResponseType == ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 3000);
                            else
                                Helper.ShowNotify($"Something unexpected happened: {result.Info?.Message}", 3000);
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }

        void RemoveThreadAndGoBack()
        {
            try
            {
                if (DirectRequestsView.Current != null)
                {
                    var l = DirectRequestsView.Current.DirectRequestsVM.Items.ToList();
                    for (int i = 0; i < l.Count; i++)
                    {
                        if (DirectRequestsThreadVM.CurrentThread.ThreadId == l[i].Thread.ThreadId)
                        {
                            DirectRequestsView.Current.DirectRequestsVM.Items.RemoveAt(i);
                            break;
                        }
                    }
                    NavigationService.GoBack();
                    InboxView.Current?.InboxVM.Refresh();
                }
            }
            catch { }
        }
        #endregion SINGLE

    }
}
