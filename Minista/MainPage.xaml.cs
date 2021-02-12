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

using Minista.Helpers;
using Windows.ApplicationModel.Core;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using InstagramApiSharp.Classes.Models;
using Windows.UI.Xaml.Media.Imaging;
using Windows.System;
using Windows.Media.Editing;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core;
using UICompositionAnimations.Enums;
using Windows.UI.Popups;
using Windows.Graphics.Imaging;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel.Background;
using Thrift.Collections;
using Windows.UI.Notifications.Management;
using InstagramApiSharp.API.RealTime;
using Minista.Themes;
using MinistaHelper.Push;
using Minista.Views.Main;
using InstagramApiSharp.API;
using Newtonsoft.Json;
using static Helper;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System.Net;

namespace Minista
{
    public sealed partial class MainPage : Page
    {
        public static string NavigationUriProtocol;
        public static MainPage Current;
        public MediaElement ME => mediaElement;

        //Windows.System.Display.DisplayRequest ScreenOnRequest;
        public RealtimeClient RealtimeClient { get;  set; }
        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            Loaded += MainPageLoaded;
            Window.Current.CoreWindow.KeyDown += OnKeyboards;
            //KeyDown += MainPageKeyDown;
            CreateConfig();
            try
            {
                if (CheckLogin())
                {
                    AllowDrop = true;
                    try
                    {
                        DragOver -= OnDragOver;
                        Drop -= OnDrop;
                    }
                    catch { }
                    try
                    {
                        DragOver += OnDragOver;
                        Drop += OnDrop;
                    }
                    catch { }
                }
            }
            catch { }
        }
        
        private async void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            ThemeHelper.InitTheme(SettingsHelper.Settings.CurrentTheme);
            CreateConfig();
            try
            {
                Focus(FocusState.Pointer);
            }
            catch { }
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBarLayoutMetricsChanged;
            UpdateTitleBarLayout(coreTitleBar);
            Window.Current.SetTitleBar(AppTitleBarORG);
            if (InstaApi == null || InstaApi != null && !InstaApi.IsUserAuthenticated)
            {
                SetStackPanelTitleVisibility(Visibility.Collapsed);
                NavigationService.Navigate(typeof(Views.Sign.SignInView));
            }
            else
                NavigateToMainView();

            if (!SettingsHelper.Settings.AskedAboutPosition)
            {
                SettingsGrid.Visibility = Visibility.Visible;
            }
            //if (App.ScreenOnRequest != null)
            //    App.ScreenOnRequest.RequestActive();
            CheckLicense();
            try
            {
                if (Passcode.IsEnabled)
                {
                    PassCodeView.Visibility = Visibility.Visible;
                    LockControl.Visibility = Visibility.Visible;
                }
                else
                {
                    PassCodeView.Visibility = Visibility.Collapsed;
                    LockControl.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
            try
            {
                await BackgroundExecutionManager.RequestAccessAsync();
            }
            catch { }
            try
            {
                UserNotificationListener listener = UserNotificationListener.Current;
                await listener.RequestAccessAsync();
            }
            catch { }
        }
        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Drop here to upload";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
        }
        async private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                try
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    if (items.Count > 0)
                    {
                        if (items[0] is StorageFile file)
                        {
                            if (file.Path.IsSupportedImage()) // IsSupportedVideo ?
                            {
                                if (NavigationService.Frame.Content is Views.Direct.ThreadView thread)
                                {
                                    thread.UploadFile(file);
                                }
                                else
                                    await new ContentDialogs.FileAssociationDialog(file).ShowAsync();
                            }
                            else if(file.Path.Contains(".mi-theme"))
                            {
                                var json = await FileIO.ReadTextAsync(file);
                                ThemeHelper.InitTheme(json);
                            }
                            else
                                ShowNotify("This file is not supported.\r\n" + file.Path, 3000);
                        }
                    }
                }
                catch { }
            }
        }
        private async void OnKeyboards(CoreWindow sender, KeyEventArgs args)
        {
            try
            {
                CoreVirtualKeyStates controlKeyState = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                var isCtrlDown = (controlKeyState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                CoreVirtualKeyStates shiftKeyState = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
                var isShiftDown = (shiftKeyState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                var shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
                switch (args.VirtualKey)
                {
                    case VirtualKey.Escape:
                        //NavigationService.GoBack();
                        break;
                }
                if (isCtrlDown && args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.Escape)
                    NavigationService.GoBack();

                if (isCtrlDown && args.VirtualKey == VirtualKey.V || isShiftDown && args.VirtualKey == VirtualKey.Insert)
                {
                    if (InstaApi != null && InstaApi.IsUserAuthenticated)
                    {
                        DataPackageView dataPackageView = Clipboard.GetContent();
                        if (dataPackageView.Contains(StandardDataFormats.StorageItems))
                        {
                            var items = await dataPackageView.GetStorageItemsAsync();
                            if (items.Count > 0)
                                if (items[0] is StorageFile file)
                                    if (file.Path.IsSupportedImage()) // IsSupportedVideo ?
                                    {
                                        if (NavigationService.Frame.Content is Views.Direct.ThreadView thread)
                                        {
                                            thread.UploadFile(file);
                                        }
                                        else
                                            await new ContentDialogs.FileAssociationDialog(file).ShowAsync();
                                    }
                                    else
                                        ShowNotify("This file is not supported.\r\n" + file.Path, 3000);
                        }
                        else if (dataPackageView.Contains(StandardDataFormats.Bitmap))
                        {
                            var bitmap = await dataPackageView.GetBitmapAsync();
                            var decoder = await BitmapDecoder.CreateAsync(await bitmap.OpenReadAsync());
                            var file = await GenerateRandomOutputFile();
                            var encoder = await BitmapEncoder.CreateForTranscodingAsync(await file.OpenAsync(FileAccessMode.ReadWrite), decoder);
                            await encoder.FlushAsync();
                            if (NavigationService.Frame.Content is Views.Direct.ThreadView thread)
                            {
                                thread.UploadFile(file);
                            }
                            else
                                await new ContentDialogs.FileAssociationDialog(file).ShowAsync();
                        }
                    }
                }
            }
            catch { }
        }

        public void HideHeaders()
        {
            GridFrame.Margin = new Thickness(0);
            SplitViewPaneGrid.Margin = new Thickness(0);
            StackPanelTitle.Visibility = Visibility.Collapsed;
        }
        public void ShowHeaders()
        {
            if (NavigationService.Frame.Content is Views.Sign.SignInView && !CheckLogin())
            {
                HideHeaders();
                return;
            }
            if (SettingsHelper.Settings.HeaderPosition == Classes.HeaderPosition.Top)
            {
                GridFrame.Margin = new Thickness(0, 52, 0, 0);
                SplitViewPaneGrid.Margin = new Thickness(0, 52, 0, 0);
                AppTitleBarORG.Height = 52;
                AppTitleBar.VerticalAlignment = VerticalAlignment.Top;
                if (DeviceUtil.IsDesktop)
                {
                    AppTitleBar.HorizontalAlignment = HorizontalAlignment.Stretch;
                }
                else
                    AppTitleBar.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else
            {
                if (DeviceUtil.IsMobile)
                {
                    GridFrame.Margin = new Thickness(0, 0, 0, 52);
                    SplitViewPaneGrid.Margin = new Thickness(0, 0, 0, 52);
                    AppTitleBarORG.Height = 0;
                }
                else
                { 
                    GridFrame.Margin = new Thickness(0, 42, 0, 52);
                    SplitViewPaneGrid.Margin = new Thickness(0, 42, 0, 52);
                    AppTitleBarORG.Height = 42;
                }
                AppTitleBar.VerticalAlignment = VerticalAlignment.Bottom;
                AppTitleBar.HorizontalAlignment = HorizontalAlignment.Center;
            }
            SetTitleBarLayout();
            //AppTitleBarORG
            if(NavigationService.Frame.Content is Views.Sign.SignInView)
                StackPanelTitle.Visibility = Visibility.Visible;
            else
            StackPanelTitle.Visibility = Visibility.Visible;
        }

        public void SetStackPanelTitleVisibility(Visibility visibility = Visibility.Collapsed)
        {
            StackPanelTitle.Visibility = visibility;
        }
        public void NavigateToMainView(bool flag = false)
        {
            SetStackPanelTitleVisibility(Visibility.Visible);
            ShowHeaders();
            if (!string.IsNullOrEmpty(NavigationUriProtocol))
            {
                HandleUriProtocol();
                return;
            }
            try
            {
                if (flag)
                {
                    if (MainView.Current != null)
                    {
                        MainView.Current.ResetPageCache();
                    }
                }
            }
            catch { }
            NavigationService.Navigate(typeof(MainView));
            try
            {
                if (flag)
                {
                    NavigateToMainViewAsync();
                }
            }
            catch { }
        }
        async void NavigateToMainViewAsync()
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, /*async*/ () =>
                {
                    if (Views.Direct.InboxView.Current != null)
                    {
                        Views.Direct.InboxView.Current.ResetPageCache();
                        //ViewModels.Direct.InboxViewModel.ResetInstance();
                    }
                    UserHelper.GetSelfUser();
                    UserHelper.GetBanyan();

                    if (Views.Direct.ThreadView.Current != null)
                    {
                        Views.Direct.ThreadView.Current.ResetPageCache();
                    }
                    if (Views.Direct.DirectRequestsThreadView.Current != null)
                    {
                        Views.Direct.DirectRequestsThreadView.Current.ResetPageCache();
                    }
                    if (Views.Direct.DirectRequestsView.Current != null)
                    {
                        Views.Direct.DirectRequestsView.Current.ResetPageCache();
                    }
                    if (ActivitiesView.Current != null)
                    {
                        ActivitiesView.Current.ResetPageCache();
                        //ActivitiesView.Current.NavigationCacheMode = NavigationCacheMode.Disabled;
                        //ActivitiesView.Current.NavigationCacheMode = NavigationCacheMode.Enabled;
                    }
                    if (ExploreView.Current != null)
                    {
                        ExploreView.Current.ResetPageCache();
                        //ExploreView.Current.NavigationCacheMode = NavigationCacheMode.Disabled;
                        //ExploreView.Current.NavigationCacheMode = NavigationCacheMode.Enabled;
                    }
                    if (ExploreView.Current != null)
                    {
                        ExploreView.Current.ResetPageCache();
                    }
                    if (ExploreClusterView.Current != null)
                    {
                        ExploreClusterView.Current.ResetPageCache();
                    }
                    if (LikersView.Current != null)
                    {
                        LikersView.Current.ResetPageCache();
                    }
                    if (Views.Searches.SearchView.Current != null)
                    {
                        Views.Searches.SearchView.Current.ResetPageCache();
                        //Views.Searches.SearchView.Current.NavigationCacheMode = NavigationCacheMode.Disabled;
                        //Views.Searches.SearchView.Current.NavigationCacheMode = NavigationCacheMode.Enabled;
                    }
                    if (Views.Infos.SavedPostsView.Current != null)
                    {
                        Views.Infos.SavedPostsView.Current.ResetPageCache();
                    }
                    if (Views.Infos.CloseFriendsView.Current != null)
                    {
                        Views.Infos.CloseFriendsView.Current.ResetPageCache();
                    }
                    if (Views.Infos.ArchiveView.Current != null)
                    {
                        Views.Infos.ArchiveView.Current.ResetPageCache();
                    }
                    if (Views.Infos.ProfileDetailsView.Current != null)
                    {
                        Views.Infos.ProfileDetailsView.Current.ResetPageCache();
                        //Views.Searches.SearchView.Current.NavigationCacheMode = NavigationCacheMode.Disabled;
                        //Views.Searches.SearchView.Current.NavigationCacheMode = NavigationCacheMode.Enabled;
                    }
                    if (Views.Infos.UserDetailsView.Current != null)
                    {
                        Views.Infos.UserDetailsView.Current.ResetPageCache();
                    }
                    if (Views.Infos.FollowRequestsView.Current != null)
                    {
                        Views.Infos.FollowRequestsView.Current.ResetPageCache();
                        //ActivitiesView.Current.NavigationCacheMode = NavigationCacheMode.Disabled;
                        //ActivitiesView.Current.NavigationCacheMode = NavigationCacheMode.Enabled;
                    }
                    if (Views.Infos.FollowView.Current != null)
                    {
                        Views.Infos.FollowView.Current.ResetPageCache();
                    }
                    if (Views.Infos.HashtagView.Current != null)
                    {
                        Views.Infos.HashtagView.Current.ResetPageCache();
                    }
                    if (Views.Infos.RecentFollowersView.Current != null)
                    {
                        Views.Infos.RecentFollowersView.Current.ResetPageCache();
                    }

                    //if (Views.Posts.ScrollableUserPostView.Current != null)
                    //{
                    //    Views.Posts.ScrollableUserPostView.Current.ResetPageCache();
                    //}
                    if (Views.Posts.MultiplePostView.Current != null)
                    {
                        Views.Posts.MultiplePostView.Current.ResetPageCache();
                    }
                    if (Views.Posts.SinglePostView.Current != null)
                    {
                        Views.Posts.SinglePostView.Current.ResetPageCache();
                    }
                    if (Views.Posts.CommentView.Current != null)
                    {
                        Views.Posts.CommentView.Current.ResetPageCache();
                    }
                    //if (Views.Posts.ScrollableExplorePostView.Current != null)
                    //{
                    //    Views.Posts.ScrollableExplorePostView.Current.ResetPageCache();
                    //}
                    if (Views.TV.TVView.Current != null)
                    {
                        Views.TV.TVView.Current.ResetPageCache();
                    }
                    try
                    {
                        if (RealtimeClient != null)
                        {
                            try
                            {
                                RealtimeClient.Shutdown();
                            }
                            catch { }
                        }
                        RealtimeClient = new RealtimeClient(InstaApi);
                    }
                    catch { }
                    SessionHelper.DontSaveSettings = false;
                });
            }
            catch { }
        }


        private void CoreTitleBarLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }
        private double SystemOverlayLeftInset = 0;
        private double SystemOverlayRightInset = 0;

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            try
            {
                SystemOverlayLeftInset = coreTitleBar.SystemOverlayLeftInset;
                SystemOverlayRightInset = coreTitleBar.SystemOverlayRightInset;
                SetTitleBarLayout();
            }
            catch { }
        }
        void SetTitleBarLayout()
        {
            try
            {
                if (SettingsHelper.Settings.HeaderPosition == Classes.HeaderPosition.Top)
                {
                    LeftPaddingColumn.Width = new GridLength(SystemOverlayLeftInset);
                    RightPaddingColumn.Width = new GridLength(SystemOverlayRightInset);
                }
                else
                {
                    LeftPaddingColumn.Width = new GridLength(0);
                    RightPaddingColumn.Width = new GridLength(0);
                }
            }
            catch { }
        }
        async void CheckLicense()
        {
            try
            {
                await GetLicenseState();
            }
            catch { }
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                if (DeviceUtil.IsXbox)
                    FullscreenModeInXbox();
            }
            catch { }
            ChangeTileBarTheme();
            CreateCachedFilesFolder();
            NavigationService.StartService();
            //App.ScreenOnRequest = new Windows.System.Display.DisplayRequest();
            if (!DeviceHelper.IsThisMinista())
            {
                //await new MessageDialog("Oops, It seems you changed my app package to crack it.\r\n\r\n" +
                //    "You can work with Minista!!!").ShowAsync();
                //try
                //{
                //    CoreApplication.Exit();
                //}
                //catch
                //{
                //    try
                //    {
                //        Application.Current.Exit();
                //    }
                //    catch { }
                //}
                //return;
            }
            try
            {
                var flag = await GetLicenseState();
                if (!flag)
                    PurchaseMessage();
            }
            catch { }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ResetMainPage();
        }
        public void ResetMainPage()
        {
            NavigationService.StopService();
            try
            {
                if (App.ScreenOnRequest != null)
                    App.ScreenOnRequest.RequestRelease();
            }
            catch { }
            try
            {
                DragOver -= OnDragOver;
                Drop -= OnDrop;
            }
            catch { }
        }
        public void ShowInAppNotify(string text, int duration = 1800)
        {
            try
            {
                InAppNotify.Show(text, duration);
            }
            catch { }
        }
        public void ShowLoading(string text = null)
        {
            if (text == null)
                text = string.Empty;
            LoadingText.Text = text;
            LoadingPb.IsActive = true;
            LoadingGrid.Visibility = Visibility.Visible;
        }
        public void HideLoading()
        {
            LoadingPb.IsActive = false;
            LoadingGrid.Visibility = Visibility.Collapsed;
        }
        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(Views.Searches.SearchView));
        }
         
        private void DirectButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(Views.Direct.InboxView));
        }
        private void ActivityButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(ActivitiesView));
        }

        private  void ProfileButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(Views.Infos.ProfileDetailsView));
        }

        private void ExploreButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(ExploreView));
        }

        public void SetDirectMessageCount(InstaDirectInboxContainer inbox)
        {
            try
            {
                if (inbox == null)
                {
                    DirectMessageCountGrid.Visibility = Visibility.Collapsed;
                    return;
                }

                if(inbox.Inbox.UnseenCount > 0)
                {
                    DirectMessageCountText.Text = inbox.Inbox.UnseenCount.ToString();
                    DirectMessageCountGrid.Visibility = Visibility.Visible;
                }
                else
                    DirectMessageCountGrid.Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        public void LoggedOut() // logout is completed?
        {
            var switchToAnotherAccount = false;
            if (InstaApiList.Count > 0)
            {
                InstaApiList.RemoveInstaApi(InstaApi);
                InstaApi = null;
                if (InstaApiList.Count > 0)
                {
                    InstaApi = InstaApiList[0];
                    switchToAnotherAccount = true;
                }
                SessionHelper.DontSaveSettings = true;
            }

            if (!switchToAnotherAccount)
            {
                SetStackPanelTitleVisibility(Visibility.Collapsed);
                NavigationService.Navigate(typeof(Views.Sign.SignInView));
            
            }
            else
            {
                InstaApiSelectedUsername = InstaApi.GetLoggedUser().UserName.ToLower();
                SettingsHelper.SaveSettings();
                try
                {
                    NavigationService.HideBackButton();
                }
                catch { }
                UserChanged = true;
                try
                {
                    Current.NavigateToMainView(true);
                }
                catch { }
            }
            "You've Been Logged Out.".ShowMsg();
            NavigationService.RemoveAllBackStack();
        }

        private void HomeButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(MainView));
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        public void ShowBackButton() => BackButton.Visibility = Visibility.Visible;
        public void HideBackButton() => BackButton.Visibility = Visibility.Collapsed;

        private void SubmitButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BottomRadio.IsChecked == true)
                {
                    SettingsHelper.Settings.HeaderPosition = Classes.HeaderPosition.Bottom;
                    SettingsHelper.Settings.AskedAboutPosition = true;
                    SettingsHelper.SaveSettings();
                }
                else
                {
                    SettingsHelper.Settings.HeaderPosition = Classes.HeaderPosition.Top;
                    SettingsHelper.Settings.AskedAboutPosition = true;
                    SettingsHelper.SaveSettings();
                }
                ShowHeaders();
            }
            catch { }

            SettingsGrid.Visibility = Visibility.Collapsed;
        }

        //private async void AddUserButtonClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        await new ContentDialogs.AddOrChooseUserDialog().ShowAsync();
        //    }
        //    catch { }
        //}
        public void ShowMediaUploadingUc()
        {
            try
            {
                MediaUploadingUc.Visibility = Visibility.Visible;
                MediaUploadingUc.Start();
            }
            catch { }
        }
        public void HideMediaUploadingUc()
        {
            try
            {
                MediaUploadingUc.Visibility = Visibility.Collapsed;
                MediaUploadingUc.Stop();
            }
            catch { }
        }
        //PhotoUploaderHelper Uploader = new PhotoUploaderHelper();
        //PhotoAlbumUploader AlbumUploader = new PhotoAlbumUploader();
        private /*async*/ void UploadButtonClick(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(typeof(Views.Uploads.UploadView));
            //NavigationService.Navigate(typeof(Views.Posts.UploadPostView));
            //NavigationService.Navigate(typeof(Views.Posts.UploadStoryView));
            //return;
            //FileOpenPicker openPicker = new FileOpenPicker
            //{
            //    ViewMode = PickerViewMode.Thumbnail,
            //    SuggestedStartLocation = PickerLocationId.PicturesLibrary
            //};
            //openPicker.FileTypeFilter.Add(".jpg");
            //openPicker.FileTypeFilter.Add(".bmp");
            ////openPicker.FileTypeFilter.Add(".gif");
            //openPicker.FileTypeFilter.Add(".png");
            //var files = await openPicker.PickMultipleFilesAsync();
            //if (files != null && files.Count > 0)
            //{
            //    if(files.Count == 1)
            //    {
            //        using (var photo = new PhotoHelper())
            //        {
            //            var fileToUpload = await photo.SaveToImageForPost(files[0]);
            //            Random rnd = new Random();
            //            Uploader.UploadSinglePhoto(fileToUpload, "TEEEEEEEEEEST\r\n\r\n\r\n" + DateTime.Now.ToString(), null);
            //        }
            //    }
            //    else
            //    {
            //        List<StorageFile> list = new List<StorageFile>();
            //        foreach (var f in files)
            //        {
            //            using (var photo = new PhotoHelper())
            //            {
            //                var fileToUpload = await photo.SaveToImageForPost(f);
            //                list.Add(fileToUpload);
            //            }
            //        }
            //        AlbumUploader.SetFiles(list.ToArray(), $"ALBUM UPPPPP\r\n\r\n\r\n{DateTime.Now}\r\n\r\n\r\n" +
            //            $"RMT");
            //    }
            //}
        }

        private void UploadNewStoryButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Focus(FocusState.Pointer);
            }
            catch { }
            NavigationService.Navigate(typeof(Views.Uploads.UploadStoryView));
        }
        private void UploadStoryButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Focus(FocusState.Pointer);
            }
            catch { }
            NavigationService.Navigate(typeof(Views.Posts.UploadStoryView));
        }
        private void UploadPostButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Focus(FocusState.Pointer);
            }
            catch { }
            NavigationService.Navigate(typeof(Views.Uploads.UploadView));
        }
        private void UploadOldPostButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Focus(FocusState.Pointer);
            }
            catch { }

            NavigationService.Navigate(typeof(Views.Posts.UploadPostView));
        }
        public async void HandleUriFile(FileActivatedEventArgs e)
        {
            try
            {
                if (e.Files.Any())
                {
                    if (e.Files[0] is StorageFile file)
                    {
                        if (file.Path.Contains(".mi-theme"))
                        {
                            var json = await FileIO.ReadTextAsync(file);
                            ThemeHelper.InitTheme(json);
                        }
                        else
                            HandleUriFile(file);
                    }
                } 
            }
            catch {}
        }
        public async void HandleUriFile(StorageFile file)
        {
            try
            {
                await new ContentDialogs.FileAssociationDialog(file).ShowAsync();
            }
            catch { }
        }
        public void HandleUriProtocol()
        {
            if (string.IsNullOrEmpty(NavigationUriProtocol)) return;
            var uri = NavigationUriProtocol;
            UriHelper.HandleUri(uri);
            NavigationUriProtocol = null;
        }
        public void NavigateToUrl(string urlProtocol)
        {
            try
            {
                if (string.IsNullOrEmpty(urlProtocol)) return;
                var url = urlProtocol;
                if (url.ToLower().Contains("instagram.com/"))
                {
                    var n = url.Substring(url.IndexOf("instagram.com/") + "instagram.com/".Length);

                    OpenProfile(n);
                }
                else if (url.ToLower().Contains("ramtinak@live"))
                    ($"mailto:{url}").OpenUrl();
                else
                    url.OpenUrl();
            }
            catch { }
        }
        

        public void ShowActivityNotify(InstaActivityCount count)
        {
            try
            {
                if (ActivityNotifyUc == null) return;

                var pos = ActivityButton.GetPosition();

                double x = pos.X + 5;
                double y = 0;
                if (AppTitleBar.VerticalAlignment == VerticalAlignment.Top)
                     y = pos.Y + ActivityButton.ActualHeight - 14;
                else
                {
                    if (!DeviceUtil.IsMobile)
                        y = pos.Y - ActivityButton.ActualHeight + 18;
                    else
                        y = pos.Y - ActivityButton.ActualHeight ;
                }
                var transform = new CompositeTransform
                {
                    TranslateX = x,
                    TranslateY = y
                };

                ActivityNotifyUc.Show(transform, count, AppTitleBar.VerticalAlignment == VerticalAlignment.Bottom);
         
            }
            catch { }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (InstaApi.PushClient is PushClient push)
            {
                push.LogReceived-= MainView.OnLogReceived;
                push.LogReceived += MainView.OnLogReceived;
                push.OpenNow();

                push.Start();
            }
            return;
            var ab = new MinistaWhiteTheme();

            await Task.Delay(1500);
            var json2 = JsonConvert.SerializeObject(ab);
            json2.PrintDebug();
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".mi-theme");

            var file = await openPicker.PickSingleFileAsync();
            if (file == null)
                return;
            var json = await FileIO.ReadTextAsync(file);
            ThemeHelper.InitTheme(json);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(MainView));
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //{
            //  "Title": null,
            //  "Message": "Minista App (@ministaapp) has requested to follow you.",
            //  "TickerText": null,
            //  "IgAction": "user?username=ministaapp",
            //  "CollapseKey": "private_user_follow_request",
            //  "OptionalImage": null,
            //  "OptionalAvatarUrl": "https://instagram.fevn1-1.fna.fbcdn.net/v/t51.2885-19/s150x150/62231151_371563176829566_537134867805110272_n.jpg?_nc_ht=instagram.fevn1-1.fna.fbcdn.net&_nc_ohc=_S4Ei3Q3ZDsAX-HVjOo&tp=1&oh=384bd91ead130669a6eba37288050aaa&oe=5FF226E3",
            //  "Sound": null,
            //  "PushId": "5b59a0b8cd64eHa61202a11H5b59a5522d920H4b",
            //  "PushCategory": "private_user_follow_request",
            //  "IntendedRecipientUserId": "44579170833",
            //  "SourceUserId": "1647718432",
            //  "IgActionOverride": null,
            //  "BadgeCount": {
            //    "Direct": 0,
            //    "Ds": 0,
            //    "Activities": 0
            //  },
            //  "InAppActors": null
            //}

            //var p = new PushNotification
            //{
            //    Message = "ministaapp just posted an IGTV video.",//"2449256854614833779_1647718432"
            //    IgAction = "tv_viewer?id=2457508981540087540",
            //    CollapseKey = "subscribed_igtv_post",
            //    OptionalAvatarUrl = "https://instagram.fevn1-1.fna.fbcdn.net/v/t51.2885-19/s150x150/62231151_371563176829566_537134867805110272_n.jpg?_nc_ht=instagram.fevn1-1.fna.fbcdn.net&_nc_ohc=_S4Ei3Q3ZDsAX-HVjOo&tp=1&oh=384bd91ead130669a6eba37288050aaa&oe=5FF226E3",
            //    PushId = "5b5b4a70e31a8Ha61202a11H5b5b4f0a4347aH23",
            //    PushCategory = "subscribed_igtv_post",
            //    IntendedRecipientUserId = "44579170833",
            //    SourceUserId = "1647718432",
            //};
            //PushHelper.HandleNotify(p, InstaApiList);
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".json");

            var file = await openPicker.PickSingleFileAsync();
            if (file == null)
                return; 
            var json = await FileIO.ReadTextAsync(file);
            var list = JsonConvert.DeserializeObject<cc>(json);
            foreach (var p in list)
            {
                PushNotification notification = new PushNotification
                {
                    Sound = p.Sound,
                    SourceUserId = p.SourceUserId,
                    BadgeCount = new BadgeCount
                    {
                        Activities = p.BadgeCount.Activities,
                        Direct = p.BadgeCount.Direct,
                        Ds = p.BadgeCount.Ds
                    },
                    CollapseKey = p.CollapseKey,
                    PushCategory = p.PushCategory,
                    IgAction = p.IgAction,
                    IgActionOverride = p.IgActionOverride,
                    InAppActors = p.InAppActors,
                    IntendedRecipientUserId = p.IntendedRecipientUserId,
                    Message = p.Message,
                    OptionalAvatarUrl = p.OptionalAvatarUrl,
                    OptionalImage = p.OptionalImage,
                    PushId = p.PushId,
                    TickerText = p.TickerText,
                    Title = p.Title,

                };
                PushHelper.HandleNotify(notification, InstaApiList.ToList());
            }
        }
        public class cc : List<PushNotification2> { }

        private async void aaaaaaaaaaaaabc_Click(object sender, RoutedEventArgs e)
        {
            var abc = await InstaApi.SeenMediaAsync(URLText.Text);
        }

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            // GetMediaIdFromUrlAsync
            //GetMediaByIdAsync
            var api = InstaApiBuilder.CreateBuilder()
                .SetUser(UserSessionData.ForUsername("FAKEUSER").WithPassword("FAKEPASS"))
                  .UseLogger(new DebugLogger(InstagramApiSharp.Logger.LogLevel.All))
                .Build();

            api.Invalidate();
            //{ds_user=ire_bori; $Port}
            //{csrftoken=Uw0qLl6txUewwV9LYcQvUmHXRgPM9ulV}
            //{urlgen="{\"51.68.192.171\": 16276\054 \"178.131.20.96\": 50810\054 \"69.197.155.150\": 32097\054 \"62.102.128.176\": 50810}:1kwuGe:M2XfUKO7a1grxY-cvBNIWMTJkbU"}
            //{ds_user_id=44579170833}
            //{sessionid=44579170833%3AiORxCjSrsIJ2Mm%3A20; $Port}
            //{shbid=13238; $Port}
            //{shbts=1609709996.3197112; $Port}

            //; csrftoken=
            //var id = await api.MediaProcessor.GetMediaIdFromUrlAsync(new Uri("https://www.instagram.com/p/CJrWuBolTnb/?igshid=1x63rbrz5d3p2"));
            var cookies = InstaApi.HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(new Uri(InstaApiConstants.INSTAGRAM_URL));
            var user = await api.UserProcessor.GetUserAsync("instagram");


            var userInfo = await api.UserProcessor.GetUserInfoByIdAsync(user.Value.Pk);
            foreach (Cookie cookie in cookies)
            {
                if (cookie.Name == "sessionid")
                //if(cookie.Name == "csrftoken")
                {
                    //
                    cookie.Value = cookie.Value.Replace("sIJ2Mm", "J2sMIm");
                    api.HttpRequestProcessor.HttpHandler.CookieContainer.Add(new Uri(InstaApiConstants.INSTAGRAM_URL), cookie);
                }
            }

            var userStory1 = await api.StoryProcessor.GetUserStoryAndLivesAsync(user.Value.Pk);
            var userStory2 = await api.StoryProcessor.GetUserStoryAsync(user.Value.Pk);

            var medias = await api.UserProcessor.GetUserMediaByIdAsync(user.Value.Pk, InstagramApiSharp.PaginationParameters.MaxPagesToLoad(1));
            //var media = await api.MediaProcessor.GetMediaByIdAsync(id.Value);

        }
        //private void SettingsButtonClick(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(typeof(Views.Settings.SettingsView));
        //}
    }
}
