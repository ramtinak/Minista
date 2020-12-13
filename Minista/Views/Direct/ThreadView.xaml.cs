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
using Minista.Controls;
using Minista.ContentDialogs;
using Windows.Storage;
using Windows.Media.Capture;
using Windows.UI.Core;
using Windows.System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Minista.Views.Main;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using Windows.UI.Composition;

namespace Minista.Views.Direct
{

    public sealed partial class ThreadView : Page
    {
        public static ThreadView Current;
        readonly Random Rnd = new Random();

        private InstaDirectInboxThread Thread;
        private InstaUserShortFriendship UserShortFriendship;
        private bool CanLoadFirstPopUp = false;
        NavigationMode NavigationMode;
        private Compositor _compositor;

        StorageFile ShareFile = null;

        public ThreadView()
        {
            this.InitializeComponent(); 

            Current = this;
            Loaded += ThreadViewLoaded;
            _ellipseVisual = ElementCompositionPreview.GetElementVisual(Ellipse);
            _elapsedVisual = ElementCompositionPreview.GetElementVisual(ElapsedPanel);
            _slideVisual = ElementCompositionPreview.GetElementVisual(SlidePanel);
            _recordVisual = ElementCompositionPreview.GetElementVisual(ButtonRecord);
            _rootVisual = ElementCompositionPreview.GetElementVisual(TextMessage);
            _compositor = _slideVisual.Compositor;

            _elapsedTimer = new DispatcherTimer();
            _elapsedTimer.Interval = TimeSpan.FromMilliseconds(100);
            _elapsedTimer.Tick += (s, args) =>
            {
                ElapsedLabel.Text = btnVoiceMessage.Elapsed.ToString("m\\:ss\\.ff");
            };

        }
        private /*async*/ void OnKeyboards(CoreWindow sender, KeyEventArgs args)
        {
            try
            {
                CoreVirtualKeyStates controlKeyState = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                var isCtrlDown = (controlKeyState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                CoreVirtualKeyStates shiftKeyState = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
                var isShiftDown = (shiftKeyState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                var shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
                if (DeviceUtil.IsDesktop)
                {
                    if (isShiftDown && args.VirtualKey == VirtualKey.Enter)
                        args.Handled = true;
                    else if (args.VirtualKey == VirtualKey.Enter)
                    {
                        args.Handled = true;
                        SendButtonClick(null, null);
                    }
                }
            }
            catch { }
        }
        async void PauseVideo()
        {
            try
            {
                try
                {
                    ME.Source = null;
                    ME.Pause();
                }
                catch(Exception ex) { ex.PrintException("PauseVideo()"); }
                await Task.Delay(350);
            }
            catch { }
        }

        private async void ThreadViewLoaded(object sender, RoutedEventArgs e)
        {
            if (NavigationMode == NavigationMode.Back && ThreadVM.CurrentThread != null && Thread != null)
            {
                if (ThreadVM.CurrentThread.ThreadId.ToLower() == Thread.ThreadId.ToLower())
                {
                    ThreadVM.StartRefreshTimer();
                    return;
                }
            }
            else if (NavigationMode == NavigationMode.New)
            {
                GetType().RemovePageFromBackStack();
                NavigationCacheMode = NavigationCacheMode.Enabled;
                CanLoadFirstPopUp = false;
            }
            if (!CanLoadFirstPopUp)
            {
                ThreadVM.ResetCache();
                PauseVideo();

                await Task.Delay(250);
                if (Thread != null)
                {
                    ThreadVM.SetThread(Thread);
                    if(btnVoiceMessage != null)
                    btnVoiceMessage.CurrentThread = Thread;
                }
                if (ItemsLV != null)
                    ThreadVM.SetLV(ItemsLV);
                CanLoadFirstPopUp = true;
                await Task.Delay(350);

                if(ShareFile != null)
                UploadFile(ShareFile);

                ShareFile = null;
            }
            try
            {
                AttachExpression();
            }
            catch { }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                if (MainPage.Current != null && MainPage.Current?.RealtimeClient != null)
                    MainPage.Current.RealtimeClient.OnDisconnect += RealtimeClient_OnDisconnect;
            }
            catch { }
            try
            {
                Window.Current.CoreWindow.KeyDown += OnKeyboards;
            }
            catch { }
            NavigationMode = e.NavigationMode;
            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();
            try
            {
                if (RecordDirectVoiceUc.Visibility == Visibility.Visible)
                    RecordDirectVoiceUc.Hide();
            }
            catch { }
            if (e.Parameter != null && e.Parameter is InstaDirectInboxThread thread)
            {
                Thread = thread;
                ShareFile = null;
            }
            else if (e.Parameter != null && e.Parameter is InstaUserShortFriendship userShortFriendship)
            {
                UserShortFriendship = userShortFriendship;
                var myThread = InboxViewModel.GetThread(userShortFriendship.Pk);
                if (myThread != null)
                    Thread = myThread;
                else
                {
                    "NOT FOUND....".PrintDebug();
                    Thread = ThreadViewModel.CreateFakeThread(userShortFriendship);
                }
                ShareFile = null;
            }
            else if (e.Parameter != null && e.Parameter is object[] obj)
            {
                if (obj?.Length == 2)
                {
                    Thread = obj[0] as InstaDirectInboxThread;
                    ShareFile = obj[1] as StorageFile;
                }
                else
                    ShareFile = null;
            }
        }

        private void RealtimeClient_OnDisconnect(object sender, object e)
        {
            ThreadVM.RefreshTimerTick(null, null);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            try
            {
                if (MainPage.Current != null && MainPage.Current?.RealtimeClient != null)
                    MainPage.Current.RealtimeClient.OnDisconnect -= RealtimeClient_OnDisconnect;
            }
            catch { }
            try
            {
                Window.Current.CoreWindow.KeyDown -= OnKeyboards;
            }
            catch { }
            try
            {
                ThreadVM.StopRefreshTimer();
            }
            catch { }
            try
            {
                PauseVideo();
            }
            catch { }
        }

        private /*async*/ void VoiceButtonClick(object sender, RoutedEventArgs e)
        {
            RecordDirectVoiceUc.SetThreadId(ThreadVM.CurrentThread.ThreadId);
            if (RecordDirectVoiceUc.Visibility == Visibility.Visible)
                RecordDirectVoiceUc.Visibility = Visibility.Collapsed;
            else
                RecordDirectVoiceUc.Visibility = Visibility.Visible;
        }


        private async void CameraButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CameraCaptureUI dialog = new CameraCaptureUI();
                StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.PhotoOrVideo);
                if (file != null)
                    UploadFile(file);
            }
            catch { }
        }
        InstaUploader Uploader = new InstaUploader();
        private async void FileSelectButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    CommitButtonText = "Open"
                };

                filePicker.FileTypeFilter.Add(".jpg");
                filePicker.FileTypeFilter.Add(".jpeg");
                filePicker.FileTypeFilter.Add(".png");
                //filePicker.FileTypeFilter.Add(".mp4");
                //filePicker.FileTypeFilter.Add(".mov");

                var file = await filePicker.PickSingleFileAsync();

                if (file == null)
                    return;
                UploadFile(file);
            }
            catch { }
        }
        public async void UploadFile(StorageFile file)
        {
            try
            {
                using (var photo = new PhotoHelper())
                {
                    var fileToUpload = await photo.SaveToImageX(file/*, false*/);

                    var mediaShare = new InstaInboxMedia()
                    {
                        MediaType = InstaMediaType.Image,
                    };
                    mediaShare.Images.Add(new InstaImage
                    {
                        Uri = fileToUpload/*copied*/.Path
                    });
                    var itemId = Guid.NewGuid().ToString();
                    InstaDirectInboxItem item = new InstaDirectInboxItem
                    {
                        ItemType = InstaDirectThreadItemType.Media,
                        Media = mediaShare,
                        UserId = Helper.InstaApi.GetLoggedUser().LoggedInUser.Pk,
                        TimeStamp = DateTime.UtcNow,
                        ItemId = itemId,
                        SendingType = InstagramApiSharp.Enums.InstaDirectInboxItemSendingType.Pending
                    };
                    ThreadVM.AddItem(item, true, 1000);
                    ThreadVM.UpdateInbox(item, true);
                    var pks = new List<long>();
                    for (int i = 0; i < ThreadVM.CurrentThread.Users.Count; i++)
                        pks.Add(ThreadVM.CurrentThread.Users[i].Pk);
                    Uploader.UploadSinglePhoto(fileToUpload, ThreadVM.CurrentThread.ThreadId,
                        pks.EncodeRecipients(), mediaShare, ThreadVM.CurrentThread, item);

                }
            }
            catch { }
        }

        private void PlusButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            catch { }
        }

        private void GiphyMenuFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            ThreadVM.GetGiphy();
            if (LVGiphy.Visibility == Visibility.Visible)
                LVGiphy.Visibility = Visibility.Collapsed;
            else
                LVGiphy.Visibility = Visibility.Visible;
        }

        private async void HeartMenuFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    InstaDirectInboxItem item = new InstaDirectInboxItem
                    {
                        ItemType = InstaDirectThreadItemType.Like,
                        Text = "❤️",
                        UserId = Helper.InstaApi.GetLoggedUser().LoggedInUser.Pk,
                        TimeStamp = DateTime.UtcNow,
                        ItemId = Guid.NewGuid().ToString(),
                        SendingType = InstagramApiSharp.Enums.InstaDirectInboxItemSendingType.Pending
                    };

                    ThreadVM.AddItem(item, true);
                    ThreadVM.UpdateInbox(item, true);
                    var result = await Helper.InstaApi.MessagingProcessor.SendDirectLikeAsync(ThreadVM.CurrentThread.ThreadId);
                    if (!result.Succeeded)
                    {
                        ThreadVM.Items.Remove(item);
                        ItemsLV.UpdateLayout();
                    }
                    else
                        item.SendingType = InstagramApiSharp.Enums.InstaDirectInboxItemSendingType.Sent;

                });
            }
            catch { }
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
        bool singleTap;

        private async void MediaShareGridTapped(object sender, TappedRoutedEventArgs e)
        {
            this.singleTap = true;
            await Task.Delay(200);
            if (this.singleTap)
                ExecSingleTap();
        }
        private void MediaShareGridDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.singleTap = false;
            ExecDoubleTap();
        }
        private void ExecDoubleTap()
        {
            "MediaShareGridDoubleTapped".PrintDebug();
        }
        private void ExecSingleTap()
        {
            "MediaShareGridTapped".PrintDebug();
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

        private void TextMessageTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            try
            {
                if (TextMessage.Text.Length > 0)
                {
                    RightGrid.Visibility = Visibility.Collapsed;
                    SendButton.Visibility = Visibility.Visible;
                    Indicate(true);
                }
                else
                {
                    Indicate(false, true);
                    SendButton.Visibility = Visibility.Collapsed;
                    RightGrid.Visibility = Visibility.Visible;
                }
            }
            catch { }
        }
        private void TextMessageLostFocus(object sender, RoutedEventArgs e)
        {
            Indicate(false, true);
        }
        private bool IndicateIsWorking = false;
        async void Indicate(bool typing, bool forceIndicate= false)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (!string.IsNullOrEmpty(ThreadVM.CurrentThread.ThreadId) && MainPage.Current.RealtimeClient != null)
                    {
                        if (!forceIndicate)
                            if (IndicateIsWorking) return;
                        IndicateIsWorking = true;
                        await Task.Delay(1500);
                        await MainPage.Current.RealtimeClient?.IndicateActivityAsync(ThreadVM.CurrentThread.ThreadId, typing);
                        IndicateIsWorking = false;
                    }
                });
            }
            catch { }
        }

        private async void SendButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (TextMessage.Text.Trim().Length == 0)
                    {
                        TextMessage.Text = string.Empty;
                        return;
                    }
                    InstaDirectInboxItem item = new InstaDirectInboxItem
                    {
                        ItemType = InstaDirectThreadItemType.Text,
                        Text = TextMessage.Text.Trim(),
                        UserId = Helper.InstaApi.GetLoggedUser().LoggedInUser.Pk,
                        SendingType = InstagramApiSharp.Enums.InstaDirectInboxItemSendingType.Pending,
                        TimeStamp = DateTime.Now
                    };
                    ThreadVM.AddItem(item, true, 1000);
                    ThreadVM.UpdateInbox(item, true);
                    if (TextMessage.Text.Trim().StartsWith("http:") || TextMessage.Text.Trim().StartsWith("https:") ||
                    TextMessage.Text.Trim().StartsWith("www."))
                    {
                        item.ItemType = InstaDirectThreadItemType.Link;
                        item.LinkMedia = new InstaWebLink
                        {
                            Text = TextMessage.Text,
                            LinkContext = new InstaWebLinkContext
                            {
                                LinkUrl = TextMessage.Text,
                                LinkSummary = TextMessage.Text
                            }
                        };
                        IResult<InstaDirectRespondPayload> result;
                        if(!string.IsNullOrEmpty(ThreadVM.CurrentThread.ThreadId))
                        result = await Helper.InstaApi.MessagingProcessor.SendDirectLinkAsync(TextMessage.Text.Trim(),
                            TextMessage.Text.Trim(), ThreadVM.CurrentThread.ThreadId);
                        else
                        {
                            var pks = new List<long>();
                            for (int i = 0; i < ThreadVM.CurrentThread.Users.Count; i++)
                                pks.Add(ThreadVM.CurrentThread.Users[i].Pk);
                            result = await Helper.InstaApi.MessagingProcessor.SendDirectLinkToRecipientsAsync(TextMessage.Text.Trim(),
                          TextMessage.Text.Trim(), pks.EncodeRecipients());
                        }
                        if (result.Succeeded)
                        {
                            item.SendingType = InstagramApiSharp.Enums.InstaDirectInboxItemSendingType.Sent;
                            TextMessage.Text = string.Empty;
                            item.ItemId = result.Value?.ItemId;
                            item.TimeStamp = DateTime.UtcNow;
                            ThreadVM.CurrentThread.ThreadId = result.Value.ThreadId;
                        }
                    }
                    else
                    {
                        try
                        {
                            Indicate(false, true);
                        }
                        catch { }
                        IResult<InstaDirectRespondPayload> result;
                        if (!string.IsNullOrEmpty(ThreadVM.CurrentThread.ThreadId))
                            result = await Helper.InstaApi.MessagingProcessor.SendDirectTextAsync(null, ThreadVM.CurrentThread.ThreadId, 
                                TextMessage.Text.Trim());
                        else
                        {
                            var pks = new List<long>();
                            for (int i = 0; i < ThreadVM.CurrentThread.Users.Count; i++)
                                pks.Add(ThreadVM.CurrentThread.Users[i].Pk);
                            result = await Helper.InstaApi.MessagingProcessor.SendDirectTextAsync(pks.EncodeRecipients(),null,
                                 TextMessage.Text.Trim());
                        }
                        if (result.Succeeded)
                        {
                            item.SendingType = InstagramApiSharp.Enums.InstaDirectInboxItemSendingType.Sent;
                            TextMessage.Text = string.Empty;
                            item.ItemId = result.Value?.ItemId;
                            item.TimeStamp = DateTime.Now;
                            ThreadVM.CurrentThread.ThreadId = result.Value.ThreadId;
                        }
                    }
                    // direct response
                    // {"action": "item_ack", "status_code": "200", "payload": {"client_context": "da5efe77-194a-405e-85a0-80f5551cdfcc", "item_id": "28772851342289354032285805617086464", "timestamp": "1559779396695629", "thread_id": "340282366841710300949128112965048690517"}, "status": "ok"}

                });
            }
            catch { }
        }

        private void PlayPauseButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is ToggleButton/*AppBarButton*/ button && button != null)
                {
                    try
                    {
                        button.IsChecked = false;
                    }
                    catch { }
                    if (button.DataContext is InstaDirectInboxItem directInboxItem && directInboxItem != null &&
                        directInboxItem.ItemType == InstaDirectThreadItemType.VoiceMedia && directInboxItem.VoiceMedia != null &&
                        directInboxItem.VoiceMedia?.Media?.Audio != null)
                    {
                        var parent = button.Parent;
                        if (parent is Grid grid)
                        {
                            try
                            {
                                var pVooice = grid.FindChild<ProgressVoice>();
                                if(pVooice != null)
                                    MainPage.Current.VoicePlayerUc.SetDirectItem(ThreadVM.CurrentThread, directInboxItem, pVooice, button);
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }



        private void ProgressVoiceDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                if (sender is ProgressVoice pVoice && args.NewValue is InstaDirectInboxItem directInboxItem && directInboxItem != null &&
                        directInboxItem.ItemType == InstaDirectThreadItemType.VoiceMedia && directInboxItem.VoiceMedia != null &&
                        directInboxItem.VoiceMedia?.Media?.Audio != null)
                {
                    pVoice.UpdateWave(directInboxItem.VoiceMedia.Media.Audio.WaveformData.ToList());
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
                    //WaveGrid.ColumnDefinitions.Clear();
                    var waveFromData = directInboxItem.VoiceMedia.Media.Audio.WaveformData;
                    var black = (SolidColorBrush)Application.Current.Resources["DefaultForegroundColor"];
                    //WaveGrid.ColumnDefinitions.Count.PrintDebug();
                     var rectwidth = WaveGrid.ActualWidth / waveFromData.Length;
                    if (rectwidth > 2.5)
                        rectwidth = 1.5;
                    var heightList = new double[] { 3.1, 3.2, 3.3, 3.4, 3.5};
                    for(int i = 0; i< waveFromData.Length; i++)
                    {
                        try
                        {
                            var data = waveFromData[i];
                            var height = (data * WaveGrid.Height);
                            if (height <= 0)
                            {
                                height = heightList[Rnd.Next(heightList.Length)];
                                //height = 3.5;
                            }
                            var width = rectwidth;
                            if (width - 0.276 > 0)
                                width -= 0.276;
                            //if (width > 2.5)
                            //    width = 2.2;
                            //else width = 1.32;
                            Rectangle rect = new Rectangle() { Fill = black, Height = height, Width = width };
                            if (WaveGrid.ColumnDefinitions.Count <= waveFromData.Length)
                            {
                                //WaveGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(rectwidth) });
                                WaveGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                            }
                            Grid.SetColumn(rect, i);
                            //Grid.SetColumn(rect, (WaveGrid.ColumnDefinitions.Count - 1));
                            WaveGrid.Children.Add(rect);
                        }
                        catch(Exception ex )
                        {
                            ex.PrintException();
                        }
                    }
                }
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

        private void MEMediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (VoicePlayPauseButton != null)
                //    VoicePlayPauseButton.Content = Helper.PlayMaterialIcon;
            }
            catch { }
        }

        private void MEMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                //if (VoicePlayPauseButton != null)
                //    VoicePlayPauseButton.Content = Helper.PlayMaterialIcon;
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

        private async void LVGiphyItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is GiphyItem giphy && giphy != null)
                {
                    var animatedMedia = new InstaAnimatedImage
                    {
                        User = new InstaAnimatedImageUser
                        {
                            Username = Helper.InstaApi.GetLoggedUser().LoggedInUser.UserName,
                        },
                        Media = new InstaAnimatedImageMedia
                        {
                            Height = double.Parse(giphy.Images.FixedWidth.Height ?? "0"),
                            Width = double.Parse(giphy.Images.FixedWidth.Width ?? "0"),
                            Url = giphy.Images.Original.Url,
                            Mp4Size = double.Parse(giphy.Images.Original.Mp4Size ?? "0"),
                            Mp4Url = giphy.Images.Original.Mp4,
                            WebpUrl = giphy.Images.Original.Webp
                        }
                    };

                    InstaDirectInboxItem item = new InstaDirectInboxItem
                    {
                        ItemType = InstaDirectThreadItemType.AnimatedMedia,
                        AnimatedMedia = animatedMedia,
                        UserId = Helper.InstaApi.GetLoggedUser().LoggedInUser.Pk,
                        SendingType = InstagramApiSharp.Enums.InstaDirectInboxItemSendingType.Pending
                    };

                    ThreadVM.AddItem(item, true, 1000);
                    ThreadVM.UpdateInbox(item, true);
                    IResult<InstaDirectInboxThread> result;
                    if (!string.IsNullOrEmpty(ThreadVM.CurrentThread.ThreadId))
                        result = await Helper.InstaApi.MessagingProcessor.SendDirectAnimatedMediaAsync(giphy.Id, ThreadVM.CurrentThread.ThreadId);
                    else
                    {
                        var pks = new List<long>();
                        for (int i = 0; i < ThreadVM.CurrentThread.Users.Count; i++)
                            pks.Add(ThreadVM.CurrentThread.Users[i].Pk);
                        result = await Helper.InstaApi.MessagingProcessor.SendDirectAnimatedMediaToRecipientsAsync(giphy.Id, pks.EncodeRecipients());
                    }
                    if (result.Succeeded)
                    {
                        item.SendingType = InstagramApiSharp.Enums.InstaDirectInboxItemSendingType.Sent;
                        item.ItemId = result.Value.Items.FirstOrDefault()?.ItemId;
                        item.TimeStamp = DateTime.UtcNow;
                        ThreadVM.CurrentThread.ThreadId = result.Value.ThreadId;
                    }
                }
            }
            catch { }
            LVGiphy.Visibility = Visibility.Collapsed;

        }



        private void UserProfileEllipseTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Ellipse elp && elp.DataContext is InstaDirectInboxItem data && data != null)
                {
                    var user = ThreadVM.CurrentThread.Users.FirstOrDefault(x => x.Pk == data.UserId);
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

                    NavigationService.Navigate(SettingsHelper.GetStoryView(), reel);
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
                        NavigationService.Navigate(typeof(RavenMediaView), new object[] { ThreadVM.CurrentThread, data });
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

        private void MenuGridRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            "MenuGridRightTapped".PrintDebug();
            try
            {
                if (sender is Grid item && item.DataContext is InstaDirectInboxItem data && data.UserId == Helper.CurrentUser.Pk)
                    FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            catch { }
        }

        private void MenuGridHolding(object sender, HoldingRoutedEventArgs e)
        {
            "MenuGridHolding".PrintDebug();
            try
            {
                if (sender is Grid item && item.DataContext is InstaDirectInboxItem data && data.UserId == Helper.CurrentUser.Pk)
                    FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            catch { }
        }

        private async void UnsendMessageFlyoutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem item && item.DataContext is InstaDirectInboxItem data && data != null &&
                    data.UserId == Helper.CurrentUser.Pk)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        var result = await Helper.InstaApi.MessagingProcessor.DeleteSelfMessageAsync(ThreadVM.CurrentThread.ThreadId, data.ItemId);
                        if (result.Succeeded)
                            ThreadVM.RemoveItem(data.ItemId);
                        else
                            Helper.ShowNotify(result.Info.Message, 2200);
                    });
                }
            }
            catch { }
        }

        private void CopyTextFlyoutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem item && item.DataContext is InstaDirectInboxItem data && data != null &&
                    data.UserId == Helper.CurrentUser.Pk)
                {
                    var type = data.ItemType;
                    var last = data;
                    if (type == InstaDirectThreadItemType.FelixShare && last.FelixShareMedia != null)
                        CopyCaption(last.FelixShareMedia.Caption?.Text);
                    else if (type == InstaDirectThreadItemType.Hashtag && last.HashtagMedia != null)
                        CopyHashtag(last.HashtagMedia.Name);
                    else if (type == InstaDirectThreadItemType.Like && last.Text != null)
                        CopyText(last.Text);
                    else if (type == InstaDirectThreadItemType.Link && last.LinkMedia != null)
                        CopyLink(last.LinkMedia.LinkContext.LinkUrl);
                    else if (type == InstaDirectThreadItemType.Location && last.LocationMedia != null)
                        CopyLocation(last.LocationMedia.Name);
                    else if (type == InstaDirectThreadItemType.Media && last.Media != null)
                        CopyText(last.Text);
                    else if (type == InstaDirectThreadItemType.MediaShare && last.MediaShare != null)
                        CopyCaption(last.MediaShare.Caption?.Text);
                    else if (type == InstaDirectThreadItemType.Profile && last.ProfileMedia != null)
                        CopyProfile(last.ProfileMedia.UserName);
                    else if (type == InstaDirectThreadItemType.ReelShare && last.ReelShareMedia != null)
                        CopyText(last.ReelShareMedia.Text);
                    else if (type == InstaDirectThreadItemType.StoryShare && last.StoryShare != null)
                        CopyText(last.StoryShare.Text);
                    else if (type == InstaDirectThreadItemType.Text && last.Text != null)
                        CopyText(last.Text);
                }
            }
            catch { }
        }

        void CopyText(string text)
        {
            if(!string.IsNullOrEmpty(text))
            {
                text.CopyText();
                Helper.ShowNotify("Text copied ;-)");
            }
        }
        void CopyCaption(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text.CopyText();
                Helper.ShowNotify("Caption copied ;-)");
            }
        }
        void CopyProfile(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text.CopyText();
                Helper.ShowNotify("Profile copied ;-)");
            }
        }
        void CopyHashtag(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text.CopyText();
                Helper.ShowNotify("Hashtag copied ;-)");
            }
        }
        void CopyLink(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text.CopyText();
                Helper.ShowNotify("Url copied ;-)");
            }
        }
        void CopyLocation(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text.CopyText();
                Helper.ShowNotify("Location copied ;-)");
            }
        }

        private void StoryShareGridTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid item && item.DataContext is InstaDirectInboxItem data && data != null &&
                data.ItemType == InstaDirectThreadItemType.StoryShare && data.StoryShare != null && data.StoryShare.Media != null)
                {
                    NavigationService.Navigate(SettingsHelper.GetStoryView(), 
                        new object[] { data.StoryShare.Media.User.Pk, data.StoryShare.Media.Pk.ToString(),
                            data.StoryShare.Media.Pk.ToString(), data.StoryShare.Media.Pk.ToString(), });
                }
            }
            catch { }
        }

        #region LOADINGS
        public void ShowTopLoading() => TopLoading.Start();
        public void HideTopLoading() => TopLoading.Stop();
        #endregion LOADINGS

        private async void LikeButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is ToggleButton btn && btn.DataContext is InstaDirectInboxItem item && item != null)
                {
                    try
                    {
                        btn.IsChecked = false;
                    }
                    catch { }
                    var amILiked = item.Reactions.Likes.Any(x => x.SenderId == Helper.CurrentUser.Pk);

                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        var parent = btn.Parent as Grid;
                        var sp = parent.FindChild<StackPanel>();//name=> LVReactionUsers
                        if (amILiked)
                        {
                            item.Reactions.Liked = false;
                            var result = await Helper.InstaApi.MessagingProcessor.UnLikeThreadMessageAsync(ThreadVM.CurrentThread.ThreadId, item.ItemId);
                            if (result.Succeeded)
                            {
                                if (sp != null)
                                {
                                    try
                                    {
                                        for (int i = 0; i < sp.Children.Count; i++)
                                        {
                                            try
                                            {
                                                var it = sp.Children[i] as Ellipse;
                                                if (it != null && it.DataContext is InstaDirectLikeReaction context && context != null)
                                                {
                                                    if (context.SenderId == Helper.CurrentUser.Pk)
                                                    {
                                                        sp.Children.RemoveAt(i);
                                                        break;
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                    catch { }
                                }

                                try
                                {
                                    for (int i = 0; i < item.Reactions.Likes.Count; i++)
                                    {
                                        if (item.Reactions.Likes[i].SenderId == Helper.CurrentUser.Pk)
                                        {
                                            item.Reactions.Likes.RemoveAt(i);
                                            break;
                                        }
                                    }
                                }
                                catch { }
                            }
                            else
                                item.Reactions.Liked = true;
                        }
                        else
                        {
                            item.Reactions.Liked = true;
                            var result = await Helper.InstaApi.MessagingProcessor.LikeThreadMessageAsync(ThreadVM.CurrentThread.ThreadId, item.ItemId);
                            if (result.Succeeded)
                            {
                                if (sp != null)
                                {
                                    try
                                    {
                                        var amILiked2 = item.Reactions.Likes.Any(x => x.SenderId == Helper.CurrentUser.Pk);
                                        if (!amILiked2)
                                        {
                                            var reaction = new InstaDirectLikeReaction
                                            {
                                                SenderId = Helper.CurrentUser.Pk,
                                                Timestamp = DateTime.UtcNow,
                                                User = Helper.CurrentUser.ToUserShort()
                                            };
                                            sp.Children.Insert(0, GetEllipse(reaction));
                                            item.Reactions.Likes.Add(reaction);
                                        }
                                    }
                                    catch { }
                                }
                                // add kon be list...
                            }
                            else
                                item.Reactions.Liked = false;
                        }
                    });

                }
            }
            catch { }
        }

        private async void LikeButtonDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                if (sender is ToggleButton btn && args.NewValue is InstaDirectInboxItem data && data != null)
                {
                    var amILiked = data.Reactions.Likes.Any(x => x.SenderId == Helper.CurrentUser.Pk);
                    data.Reactions.Liked = amILiked;
                    if (data.UserId == Helper.CurrentUser.Pk)
                    {
                        if (data.Reactions.Likes.Any())
                            data.Reactions.Visibility = true;
                        else
                            data.Reactions.Visibility = false;
                    }
                    else
                        data.Reactions.Visibility = true;
                    var parent = btn.Parent;
                    if (data.Reactions.Likes.Any())
                    {
                        if (parent is Grid grid && grid != null)
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                var list = new List<long>();
                                data.Reactions.Likes.ForEach(x =>
                                {
                                    if (x.User == null) list.Add(x.SenderId);
                                });
                                if (list.Any())
                                {
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        var result = await Helper.InstaApi.UserProcessor.GetUserInfoByIdAsync(list[i]);
                                        if (result.Succeeded)
                                        {
                                            var first = data.Reactions.Likes.FirstOrDefault(a => a.SenderId == list[i]);
                                            if (first != null)
                                                first.User = result.Value.ToUserShort();
                                        }
                                    }

                                }
                                var sp = grid.FindChild<StackPanel>();//name=> LVReactionUsers
                                if (sp != null)
                                {
                                    if (sp.Children.Count == 0)
                                    {
                                        for (int i = 0; i < data.Reactions.Likes.Count; i++)
                                            sp.Children.Add(GetEllipse(data.Reactions.Likes[i]));
                                    }
                                }
                            });
                        }
                    }
                }
            }
            catch { }
        }

        private async void LikedUsersGridTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid grid && grid.DataContext is InstaDirectInboxItem item && item != null)
                {
                    if(item.Reactions?.Likes?.Count > 0)
                    {
                        var list = new List<InstaUserShort>();
                        item.Reactions.Likes.ForEach(x => list.Add(x.User));
                        await new DirectMessageLikersDialog(list).ShowAsync();
                    }
                }
            }
            catch { }
        }
        Ellipse GetEllipse(InstaDirectLikeReaction reaction)
        {
            return new Ellipse
            {
                Width = 19,
                Height = 19,
                Margin = new Thickness(2),
                DataContext = reaction,
                Fill = reaction?.User?.ProfilePicture?.GetImageBrush()
            };
        }

        private void ToggleApprovalGridToggled(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch { }
        }

        private void AddPeopleToggleButtonButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddPeopleToggleButton.IsChecked = false;
            }
            catch { }

        }

        private async void LeaveChatToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                LeaveChatToggleButton.IsChecked = false;
            }
            catch { }

            try
            {
                await new DirectLeaveChatDialog(ThreadVM.CurrentThread).ShowAsync();
            }
            catch { }
        }

        private void EndChatToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                EndChatToggleButton.IsChecked = false;
            }
            catch { }

        }

        private void MenuButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuButton.IsChecked = false;
            }
            catch { }
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private void UsersLVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem != null && e.ClickedItem is InstaUserShortFriendship user)
                    Helper.OpenProfile(user);
            }
            catch { }
        }































        private readonly DispatcherTimer _elapsedTimer;

        private readonly Visual _rootVisual;
        private readonly Visual _ellipseVisual;
        private readonly Visual _elapsedVisual;
        private readonly Visual _slideVisual;
        private readonly Visual _recordVisual;

        private void ElapsedPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var point = _elapsedVisual.Offset;
            point.X = (float)-e.NewSize.Width;

            _elapsedVisual.Offset = point;
            _elapsedVisual.Size = e.NewSize.ToVector2();
        }

        private void SlidePanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var point = _slideVisual.Offset;
            point.X = (float)e.NewSize.Width /*+ 36*/;

            _slideVisual.Opacity = 0;
            _slideVisual.Offset = point;
            _slideVisual.Size = e.NewSize.ToVector2();
        }

        private void VoiceButton_RecordingStarted(object sender, EventArgs e)
        {
            RecordInfoBorder.Visibility = Ellipse.Visibility = Visibility.Visible;
            HideButtons();
            var slideWidth = (float)SlidePanel.ActualWidth;
            var elapsedWidth = (float)ElapsedPanel.ActualWidth;

            _slideVisual.Opacity = 1;

            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            var slideAnimation = _compositor.CreateScalarKeyFrameAnimation();
            slideAnimation.InsertKeyFrame(0, slideWidth );
            slideAnimation.InsertKeyFrame(1, 0);
            slideAnimation.Duration = TimeSpan.FromMilliseconds(300);

            var elapsedAnimation = _compositor.CreateScalarKeyFrameAnimation();
            elapsedAnimation.InsertKeyFrame(0, -elapsedWidth);
            elapsedAnimation.InsertKeyFrame(1, 0);
            elapsedAnimation.Duration = TimeSpan.FromMilliseconds(300);

            var ellipseAnimation = _compositor.CreateVector3KeyFrameAnimation();
            ellipseAnimation.InsertKeyFrame(0, new Vector3(56f / 96f));
            ellipseAnimation.InsertKeyFrame(1, new Vector3(1));
            ellipseAnimation.Duration = TimeSpan.FromMilliseconds(200);

            _elapsedVisual.StartAnimation("Offset.X", elapsedAnimation);
            _ellipseVisual.StartAnimation("Scale", ellipseAnimation);

            batch.Completed += (s, args) =>
            {
                _elapsedTimer.Start();

                AttachExpression();
            };
            batch.End();

        }

        private void VoiceButton_RecordingStopped(object sender, EventArgs e)
        {
            AttachExpression();
            RecordInfoBorder.Visibility = Visibility.Collapsed;
            ShowButtons();
            var slidePosition = (float)(ActualWidth - 48 - 36);
            var difference = (float)(slidePosition - ElapsedPanel.ActualWidth);

            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            var slideAnimation = _compositor.CreateScalarKeyFrameAnimation();
            slideAnimation.InsertKeyFrame(0, _slideVisual.Offset.X);
            slideAnimation.InsertKeyFrame(1, -slidePosition);
            slideAnimation.Duration = TimeSpan.FromMilliseconds(200);

            var messageAnimation = _compositor.CreateScalarKeyFrameAnimation();
            messageAnimation.InsertKeyFrame(0, 48);
            messageAnimation.InsertKeyFrame(1, 0);
            messageAnimation.Duration = TimeSpan.FromMilliseconds(200);

            _slideVisual.StartAnimation("Offset.X", slideAnimation);

            batch.Completed += (s, args) =>
            {
                _elapsedTimer.Stop();

                DetachExpression();

                ButtonCancelRecording.Visibility = Visibility.Collapsed;
                ElapsedLabel.Text = "0:00,0";

                var point = _slideVisual.Offset;
                point.X = _slideVisual.Size.X + 36;

                _slideVisual.Opacity = 0;
                _slideVisual.Offset = point;

                point = _elapsedVisual.Offset;
                point.X = -_elapsedVisual.Size.X;

                _elapsedVisual.Offset = point;

                point = _recordVisual.Offset;
                point.Y = 0;

                _recordVisual.Offset = point;
            };
            batch.End();


        }

        private void VoiceButton_RecordingLocked(object sender, EventArgs e)
        {
            DetachExpression();

            var ellipseAnimation = _compositor.CreateScalarKeyFrameAnimation();
            ellipseAnimation.InsertKeyFrame(0, -57);
            ellipseAnimation.InsertKeyFrame(1, 0);

            _recordVisual.StartAnimation("Offset.Y", ellipseAnimation);

            ButtonCancelRecording.Visibility = Visibility.Visible;
            btnVoiceMessage.Focus(FocusState.Programmatic);

            var point = _slideVisual.Offset;
            point.X = _slideVisual.Size.X + 36;

            _slideVisual.Opacity = 0;
            _slideVisual.Offset = point;
        }

        private void VoiceButton_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Vector3 point;
            if (btnVoiceMessage.IsLocked || !btnVoiceMessage.IsRecording)
            {
                point = _slideVisual.Offset;
                point.X = 0;

                _slideVisual.Offset = point;

                point = _recordVisual.Offset;
                point.Y = 0;

                _recordVisual.Offset = point;

                return;
            }

            var cumulative = e.Cumulative.Translation.ToVector2();
            point = _slideVisual.Offset;
            point.X = Math.Min(0, cumulative.X);

            _slideVisual.Offset = point;

            if (point.X < -80)
            {
                e.Complete();
                btnVoiceMessage.CancelRecording();
                return;
            }

            point = _recordVisual.Offset;
            point.Y = Math.Min(0, cumulative.Y);

            _recordVisual.Offset = point;

            if (point.Y < -57)
            {
                e.Complete();
                btnVoiceMessage.LockRecording();
            }
        }

        private void ButtonCancelRecording_Click(object sender, RoutedEventArgs e)
        {
            btnVoiceMessage.CancelRecording();
            ShowButtons();
        }
        void ShowButtons()
        {
            FileSelectButton.Visibility = PlusButton.Visibility = Visibility.Visible;
        }
        void HideButtons()
        {
            FileSelectButton.Visibility = PlusButton.Visibility = Visibility.Collapsed;
        }
        private void AttachExpression()
        {
            var elapsedExpression = _compositor.CreateExpressionAnimation("min(0, slide.Offset.X + ((root.Size.X - 48 - 36 - slide.Size.X) - elapsed.Size.X))");
            elapsedExpression.SetReferenceParameter("slide", _slideVisual);
            elapsedExpression.SetReferenceParameter("elapsed", _elapsedVisual);
            elapsedExpression.SetReferenceParameter("root", _rootVisual);

            var ellipseExpression = _compositor.CreateExpressionAnimation("Vector3(max(0, 1 + slide.Offset.X / (root.Size.X - 48 - 36)), max(0, 1 + slide.Offset.X / (root.Size.X - 48 - 36)), 1)");
            ellipseExpression.SetReferenceParameter("slide", _slideVisual);
            ellipseExpression.SetReferenceParameter("elapsed", _elapsedVisual);
            ellipseExpression.SetReferenceParameter("root", _rootVisual);

            _elapsedVisual.StopAnimation("Offset.X");
            _elapsedVisual.StartAnimation("Offset.X", elapsedExpression);

            _ellipseVisual.StopAnimation("Scale");
            _ellipseVisual.StartAnimation("Scale", ellipseExpression);
        }

        private void DetachExpression()
        {
            _elapsedVisual.StopAnimation("Offset.X");
            _ellipseVisual.StopAnimation("Scale");
        }

    }
}
