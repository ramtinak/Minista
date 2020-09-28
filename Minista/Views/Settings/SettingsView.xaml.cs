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
using System.Threading.Tasks;
using Minista.Helpers;
using Minista.Classes;
using Windows.UI.Xaml.Documents;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;

namespace Minista.Views.Settings
{
    public sealed partial class SettingsView : Page
    {
        private bool IsPageLoaded = false;
        private bool GhostMode = false;
        private int MenuIndex = 0;

        private bool CanDoThings = false;
        public SettingsView()
        {
            InitializeComponent();
            Loaded += SettingsViewLoaded;
        }

        private async void SettingsViewLoaded(object sender, RoutedEventArgs e)
        {
            if (IsPageLoaded) return;
            AboutText.Text = "About " + ExtensionHelper.GetAppVersion();
            try
            {
                comboAppMenu.SelectedIndex = MenuIndex;

                toggleGhostMode.IsOn = GhostMode;

                togglePrivateAccount.IsOn = Helper.CurrentUser.IsPrivate;

                LogoutToggleButton.Content = $"Log Out {Helper.CurrentUser.UserName.ToLower()}";

                toggleElementSound.IsOn = SettingsHelper.Settings.ElementSound;
                toggleRemoveAds.IsOn = SettingsHelper.Settings.RemoveAds;
                toggleOpenTelegramLinks.IsOn = SettingsHelper.Settings.HandleTelegramLinks;

                ShowSavedLocationFolder();
                CalculateCacheSize();
            }
            catch { }
            IsPageLoaded = true;
            CanDoThings = true;
            await Task.Delay(500);
        }

        private async void ShowSavedLocationFolder()
        {
            try
            {
                StorageFolder folder = null;
                try
                {
                    folder = await Helper.GetPictureFolder();

                    if (!SettingsHelper.Settings.DownloadLocationChanged)
                        txtLocation.Text = $"{(DeviceUtil.IsMobile ? "Phone/Pictures/Minista" : folder.Path)}";
                    else
                        txtLocation.Text = folder.Path;
                }
                catch { }
            }
            catch { }
        }

        async void CalculateCacheSize()
        {
            try
            {
                var f1 = await ApplicationData.Current.LocalFolder.GetFolderAsync("CachedFiles");
                var cache1 = await f1.GetFolderSize();

                var f2 = await ApplicationData.Current.LocalFolder.GetFolderAsync("Cache");
                var cache2 = await f2.GetFolderSize();

                var size = cache1 + cache2;
                if(size == 0)
                {
                    toggleClearCache.IsEnabled = false;
                    txtCache.Text = "Cache size: 0";
                }
                else
                {
                    toggleClearCache.IsEnabled = true;
                    txtCache.Text = "Cache size: " + size.CalculateBytes();
                }
            }
            catch { }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SettingsHelper.SaveSettings();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();
            if (!IsPageLoaded)
            {
                MenuIndex = SettingsHelper.Settings.HeaderPosition == HeaderPosition.Top ? 0 : 1;
                GhostMode = SettingsHelper.Settings.GhostMode;
            }
        }

        private void ComboAppMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!CanDoThings) return;
            try
            {
                if (comboAppMenu.SelectedIndex == -1) return;
                switch(comboAppMenu.SelectedIndex)
                {
                    case 0:
                        MenuIndex = 0;
                        SettingsHelper.Settings.HeaderPosition = HeaderPosition.Top;
                        MainPage.Current?.ShowHeaders();
                        break;
                    case 1:
                        SettingsHelper.Settings.HeaderPosition = HeaderPosition.Bottom;
                        MenuIndex = 1;
                        MainPage.Current?.ShowHeaders();
                        break;
                }
            }
            catch { }
        }

        private void ToggleGhostModeToggled(object sender, RoutedEventArgs e)
        {
            if (!CanDoThings) return;
            try
            {
                SettingsHelper.Settings.GhostMode = toggleGhostMode.IsOn;
            }
            catch { }
        }
        private void ToggleRemoveAdsToggled(object sender, RoutedEventArgs e)
        {
            if (!CanDoThings) return;
            try
            {
                SettingsHelper.Settings.RemoveAds = toggleRemoveAds.IsOn;
            }
            catch { }

        }

        private void ToggleOpenTelegramLinksToggled(object sender, RoutedEventArgs e)
        {
            if (!CanDoThings) return;

            try
            {
                SettingsHelper.Settings.HandleTelegramLinks = toggleOpenTelegramLinks.IsOn;
            }
            catch { }
        }
        private void ToggleElementSoundToggled(object sender, RoutedEventArgs e)
        {
            if (!CanDoThings) return;
            try
            {
                SettingsHelper.Settings.ElementSound = toggleElementSound.IsOn;
                if(toggleElementSound.IsOn)
                    ElementSoundPlayer.State = ElementSoundPlayerState.On;
                else
                    ElementSoundPlayer.State = ElementSoundPlayerState.Off;
            }
            catch { }
        }
        private bool ImTheOneWhoChangingPrivateToggle = false;
        private async void TogglePrivateAccountToggled(object sender, RoutedEventArgs e)
        {
            if (!CanDoThings) return;
            try
            {
                if (!ImTheOneWhoChangingPrivateToggle)
                {
                    if (!toggleGhostMode.IsOn)
                    {
                        var result = await Helper.InstaApi.AccountProcessor.SetAccountPrivateAsync();
                        if (result.Succeeded)
                        {
                            ImTheOneWhoChangingPrivateToggle = true;
                            toggleGhostMode.IsOn = Helper.CurrentUser.IsPrivate = result.Value.IsPrivate;
                        }
                        else
                        {
                            Helper.ShowNotify("Error: " + result.Info.Message);
                            ImTheOneWhoChangingPrivateToggle = true;
                            toggleGhostMode.IsOn = false;
                            return;
                        }
                    }
                    else
                    {
                        var result = await Helper.InstaApi.AccountProcessor.SetAccountPublicAsync();
                        if (result.Succeeded)
                        {
                            ImTheOneWhoChangingPrivateToggle = true;
                            toggleGhostMode.IsOn = Helper.CurrentUser.IsPrivate = result.Value.IsPrivate;
                        }
                        else
                        {
                            Helper.ShowNotify("Error: " + result.Info.Message);
                            ImTheOneWhoChangingPrivateToggle = true;
                            toggleGhostMode.IsOn = true;
                            return;
                        }
                    }
                }
            }
            catch { }
            ImTheOneWhoChangingPrivateToggle = false;
        }

        private void BlockedAccountToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                BlockedAccountToggleButton.IsChecked = false;
            }
            catch { }
            NavigationService.Navigate(typeof(Infos.BlockedView));
        }

        private void CloseFriendsToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CloseFriendsToggleButton.IsChecked = false;
            }
            catch { }
            NavigationService.Navigate(typeof(Infos.CloseFriendsView));
        }

        private void ToggleTransparentMenuToggled(object sender, RoutedEventArgs e)
        {
            //Hyperink
        }


        private async void AllAccountsToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AllAccountsToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                if (Helper.Passcode.IsEnabled)
                {
                    var psd = new ContentDialogs.PasscodeDialog(true)
                    {
                        CallMeAnAction = async () => { await new ContentDialogs.AddOrChooseUserDialog().ShowAsync(); }
                    };
                    await psd.ShowAsync();
                    return;
                }
            }
            catch { }
            try
            {
                await new ContentDialogs.AddOrChooseUserDialog().ShowAsync();
            }
            catch { }
        }
        private async void AddAccountToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddAccountToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                if (Helper.Passcode.IsEnabled)
                {
                    var psd = new ContentDialogs.PasscodeDialog(true)
                    {
                        CallMeAnAction = () =>
                        {
                            NavigationService.Navigate(typeof(Sign.SignInView));
                            MainPage.Current?.HideHeaders();
                        }
                    };
                    await psd.ShowAsync();
                    return;
                }
            }
            catch { }
            NavigationService.Navigate(typeof(Sign.SignInView));
            MainPage.Current?.HideHeaders();
        }

        private async void LogoutToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                LogoutToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                if (Helper.Passcode.IsEnabled)
                {
                    var psd = new ContentDialogs.PasscodeDialog(true)
                    {
                        CallMeAnAction = async () =>
                        {
                            await new ContentDialogs.LogoutDialog().ShowAsync();
                        }
                    };
                    await psd.ShowAsync();
                    return;
                }
            }
            catch { }
            try
            {
                await new ContentDialogs.LogoutDialog().ShowAsync();
            }
            catch { }
        }

        private async void ToggleClearCacheClick(object sender, RoutedEventArgs e)
        {
            try
            {
                toggleClearCache.IsChecked = false;
            }
            catch { }
            try
            {
                Helper.DeleteCachedFilesFolder();
                Helper.DeleteCachedFolder();
                await Task.Delay(500);
                Helper.CreateCachedFilesFolder();
                Helper.CreateCachedFolder();
                await Task.Delay(500);
                CalculateCacheSize();
            }
            catch { }
        }
        private void HyperlinkClick(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            try
            {
                var inline = sender.Inlines.FirstOrDefault();
                if (inline != null)
                {
#pragma warning disable IDE0019
                    Run run = inline as Run;
#pragma warning restore IDE0019
                    if (run != null)
                        MainPage.Current?.NavigateToUrl(run.Text.ToLower());
                }

            }
            catch { }
        }
        private void HyperlinkX2Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            try
            {
                var inline = sender.Inlines.FirstOrDefault();
                if (inline != null)
                {
#pragma warning disable IDE0019
                    Run run = inline as Run;
#pragma warning restore IDE0019
                    if (run != null)
                    {
                        var text = run.Text;
                        if (text.Contains("Win2d"))
                            "https://github.com/Microsoft/Win2D".OpenUrl();
                        else if (text.Contains("Toolkit"))
                            "https://github.com/windows-toolkit/WindowsCommunityToolkit".OpenUrl();
                        else if (text.Contains("WinUI"))
                            "https://github.com/microsoft/microsoft-ui-xaml".OpenUrl();
                        else if (text.Contains("Json"))
                            "https://www.newtonsoft.com/json".OpenUrl();
                        else if (text.Contains("XamlAnimatedGif"))
                            "https://github.com/XamlAnimatedGif/XamlAnimatedGif".OpenUrl(); 
                        else if (text.Contains("Telerik"))
                            "https://github.com/telerik/UI-For-UWP".OpenUrl();
                        //else if (text.Contains(""))
                        //    "".OpenUrl();
                    }
                }

            }
            catch { }
        }

        private void SavedToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SavedToggleButton.IsChecked = false;
            }
            catch { }
            NavigationService.Navigate(typeof(Infos.SavedPostsView));
        }

        private void ArchiveToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ArchiveToggleButton.IsChecked = false;
            }
            catch { }
            NavigationService.Navigate(typeof(Infos.ArchiveView));
        }

        private void LikedPostsToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                LikedPostsToggleButton.IsChecked = false;
            }
            catch { }
            NavigationService.Navigate(typeof(Infos.LikedPostView));
        }

        private void TaggedYouToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TaggedYouToggleButton.IsChecked = false;
            }
            catch { }
            NavigationService.Navigate(typeof(Infos.TaggedYouView));
        }

        private async void ToggleChangeLocationClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Helper.IsAppPurchasedForKharejia)
                {
                    Helper.PurchaseMessage();
                    return;
                }
                var picker = new FolderPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                };
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".bmp");
                var folder = await picker.PickSingleFolderAsync();
                if (folder != null)
                {
                    SettingsHelper.Settings.DownloadLocationChanged = true;
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(Helper.FolderToken, folder);
                    ShowSavedLocationFolder();
                    await Helper.CreatePictureFolder();
                }
            }
            catch { }
        }

        private async void AddPassLockToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddPassLockToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                if (Helper.Passcode.IsEnabled)
                {
                    await new ContentDialogs.PasscodeDialog().ShowAsync();
                }
                else
                    await new ContentDialogs.PasscodeSettingDialog().ShowAsync();
            }
            catch { }
        }

        private void NotificationsToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NotificationsToggleButton.IsChecked = false;
            }
            catch { }
            "Notification settings will be available in next releases...".ShowMsg();
            //NavigationService.Navigate(typeof(NotificationsView));
        }

    }
}
