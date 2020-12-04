using InstagramApiSharp.Classes;
using InstagramApiSharp.Helpers;
using Minista.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

#pragma warning disable IDE0060	
namespace Minista.Views.Sign
{
    public sealed partial class SignInView : Page
    {

        public SignInView()
        {
            this.InitializeComponent();
            SignInVM.View = this;
            Loaded += SignInViewLoaded;
            DataContextChanged += SignInView_DataContextChanged;
        }

        private void SignInView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs e)
        {
            e.NewValue.ToString().PrintDebug();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ThemeHelper.InitTheme(SettingsHelper.Settings.CurrentTheme);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                MainPage.Current?.ShowHeaders();

            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();
        }
        private void SignInViewLoaded(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    Visibility = Visibility.Collapsed;
            //    try
            //    {
            //        MainPage.Current.ShowLoading("Gathering information...");
            //    }
            //    catch { }
            //    await Task.Delay(2000);
            //    await SessionHelper.LoadSession();

            //    SignInVM.Connected();
            //}
            //catch { }
            //try
            //{
            //    MainPage.Current.HideLoading();
            //}
            //catch { }
            SignInVM.BeforeLogin();
        }

        private void UsernameTextKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                try
                {
                    if (string.IsNullOrEmpty(UsernameText.Text))
                        UsernameText.Focus(FocusState.Keyboard);
                    else
                        PasswordText.Focus(FocusState.Keyboard);
                }
                catch { }
            }
        }

        private void PasswordTextKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                try
                {
                    if (string.IsNullOrEmpty(UsernameText.Text))
                        UsernameText.Focus(FocusState.Keyboard);
                    else if (string.IsNullOrEmpty(PasswordText.Password))
                        PasswordText.Focus(FocusState.Keyboard);
                    else
                        SignInVM.LoginAsync();
                }
                catch { }
            }
        }

        #region Facebook
        const string FacebookBlockedMsg = "It seems there's a network problem or Facebook website is blocked(filtered) in your area.\r\nPlease connect to vpn and try again.";

        private void FacebookLoginButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                FbStart();
            }
            catch { }
        }
        private void FacebookCloseButtonClick(object sender, RoutedEventArgs e)
        {
            FacebookGrid.Visibility = Visibility.Collapsed;
            SignInVM.LoadingOff();
        }

        private async void FbStart()
        {
            try
            {
                FacebookGrid.Visibility = Visibility.Visible;
                SignInVM.LoadingOn();
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(InstaFbHelper.FacebookAddress);
                    if (!response.IsSuccessStatusCode)
                    {
                        FacebookGrid.Visibility = Visibility.Collapsed;
                        SignInVM.LoadingOff();
                        FacebookBlockedMsg.ShowMsg();
                        return;
                    }
                }
            }
            catch
            {
                FacebookGrid.Visibility = Visibility.Collapsed;
                SignInVM.LoadingOff();
                FacebookBlockedMsg.ShowMsg();
                return;
            }
            try
            {
                UserAgentHelper.SetUserAgent("Mozilla/5.0 (Linux; Android 4.4; Nexus 5 Build/_BuildID_) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/30.0.0.0 Mobile Safari/537.36");
            }
            catch { }
            try
            {
                DeleteFacebookCookies();
            }
            catch { }
            FacebookGrid.Visibility = Visibility.Visible;
            SignInVM.LoadingOn();
            await Task.Delay(1500);
            var facebookLogin = InstaFbHelper.GetFacebookLoginUri();

            FacebookWebView.Navigate(facebookLogin);
        }

        private void FacebookWebViewPermissionRequested(WebView sender, WebViewPermissionRequestedEventArgs args)
        {
            args.PermissionRequest.Deny();
        }

        private void WebViewFacebookNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            SignInVM.LoadingOn();
        }
        private async void WebViewFacebookDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            try
            {
                SignInVM.LoadingOff();
                try
                {
                    string html = await FacebookWebView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });

                    if (InstaFbHelper.IsLoggedIn(html))
                    {
                        var cookies = GetBrowserCookie(args.Uri);
                        var sbCookies = new StringBuilder();
                        foreach (var item in cookies)
                        {
                            sbCookies.Append($"{item.Name}={item.Value}; ");
                        }

                        var fbToken = InstaFbHelper.GetAccessToken(html);

                        Helper.InstaApiTrash = Helper.BuildApi();
                        await Helper.InstaApiTrash.SendRequestsBeforeLoginAsync();
                        SignInVM.LoadingOn();
                        var loginResult = await Helper.InstaApiTrash.LoginWithFacebookAsync(fbToken, sbCookies.ToString());

                        FacebookGrid.Visibility = Visibility.Collapsed;
                        SignInVM.HandleLogin(loginResult);

                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                ex.PrintException("WebViewFacebookDOMContentLoaded");
            }
        }
        private void WebViewFacebookNewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            try
            {
                sender.Navigate(args.Uri);
                args.Handled = true;
            }
            catch { }
        }
        private void FacebookWebViewUnsupportedUriSchemeIdentified(WebView sender, WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
        {
            if (args.Uri.Scheme.ToLower() == "fbconnect")
                args.Handled = true;
        }


        internal static void DeleteFacebookCookies()
        {
            try
            {

                HttpBaseProtocolFilter myFilter = new HttpBaseProtocolFilter();
                var cookieManager = myFilter.CookieManager;

                HttpCookieCollection myCookieJar = cookieManager.GetCookies(new Uri("https://facebook.com/"));
                foreach (HttpCookie cookie in myCookieJar)
                {
                    cookieManager.DeleteCookie(cookie);
                }
            }
            catch { }
            try
            {

                HttpBaseProtocolFilter myFilter = new HttpBaseProtocolFilter();
                var cookieManager = myFilter.CookieManager;

                HttpCookieCollection myCookieJar = cookieManager.GetCookies(new Uri("https://www.facebook.com/"));
                foreach (HttpCookie cookie in myCookieJar)
                {
                    cookieManager.DeleteCookie(cookie);
                }
            }
            catch { }
            try
            {

                HttpBaseProtocolFilter myFilter = new HttpBaseProtocolFilter();
                var cookieManager = myFilter.CookieManager;

                HttpCookieCollection myCookieJar = cookieManager.GetCookies(new Uri("https://m.facebook.com/"));
                foreach (HttpCookie cookie in myCookieJar)
                {
                    cookieManager.DeleteCookie(cookie);
                }
            }
            catch { }
            try
            {

                HttpBaseProtocolFilter myFilter = new HttpBaseProtocolFilter();
                var cookieManager = myFilter.CookieManager;

                HttpCookieCollection myCookieJar = cookieManager.GetCookies(new Uri("https://instagram.com/"));
                foreach (HttpCookie cookie in myCookieJar)
                {
                    cookieManager.DeleteCookie(cookie);
                }
            }
            catch { }
        }
        internal static HttpCookieCollection GetBrowserCookie(Uri targetUri)
        {
            var httpBaseProtocolFilter = new HttpBaseProtocolFilter();
            var cookieManager = httpBaseProtocolFilter.CookieManager;
            var cookieCollection = cookieManager.GetCookies(targetUri);
            return cookieCollection;
        }

        #endregion Facebook

        private void SignInHelpButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.ShowBackButton();
            NavigationService.Navigate(typeof(RecoverView));
        }

        private async void ImportFromLocalButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    CommitButtonText = "Open and load"
                };
                filePicker.FileTypeFilter.Add(Helper.SessionFileType);

                var file = await filePicker.PickSingleFileAsync();

                if (file == null)
                    return;
                try
                {
                    var json = await FileIO.ReadTextAsync(file);
                    if (string.IsNullOrEmpty(json))
                    {
                        ShowError();
                        return;
                    }
                    var content = CryptoHelper.Decrypt(json);
                    if (string.IsNullOrEmpty(content))
                    {
                        ShowError();
                        return;
                    }
                    Helper.InstaApiTrash = Helper.BuildApi();
                    await Helper.InstaApiTrash.LoadStateDataFromStringAsync(content);

                    if (!Helper.InstaApiTrash.IsUserAuthenticated)
                    {
                        ShowError();
                    }
                    else
                    {
                        SignInVM.LoadingOn();
                        var result = await Helper.InstaApiTrash.UserProcessor.GetCurrentUserAsync();
                        SignInVM.LoadingOff();
                        if (result.Succeeded)
                        {
                            Helper.InstaApi = Helper.InstaApiTrash;
                            Helper.AddInstaApi(Helper.InstaApi);
                            Helper.InstaApiSelectedUsername = Helper.InstaApiTrash.GetLoggedUser().LoggedInUser.UserName.ToLower();
                            Helper.InstaApiTrash = null;
                            MultiApiHelper.SetupPushNotification(Helper.InstaApiList);

                            SessionHelper.SaveCurrentSession();
                            //NavigationService.Navigate(typeof(MainPage));
                            MainPage.Current?.NavigateToMainView();
                        }
                        else
                            ShowError();
                    }
                }
                catch
                {
                    ShowError();
                }
            }
            catch { }
        }

        void ShowError()
        {
            "It seems something isn't right, maybe session is corrupted.\r\nPlease try Login option".ShowErr();
        }

        public void ShowResendCodeGrid(bool flag = false)
        {
            try
            {
                TwoFactorResendGrid.Visibility = Visibility.Visible;
                SignInVM.StartTimer(flag);
            }
            catch { }
        }
        public void HideResendCodeGrid()
        {
            try
            {
                TwoFactorResendGrid.Visibility = Visibility.Collapsed;
                SignInVM.StopTimer();
            }
            catch { }
        }
        private bool TwoFactorManualSet = false;
        public void AddTwoFactorOptions(bool smsCode, bool authAppToo = false)
        {
            //<ComboBoxItem Content="Use Text(sms) Message" />
            //    <ComboBoxItem Content="Use Recovery Code" />
            //    <ComboBoxItem Content="Use Authentication App"/>
            try
            {
                TwoFactorOptionsCmb.Items.Clear();
                if (smsCode)
                {
                    TwoFactorOptionsCmb.Items.Add(new TextBlock
                    {
                        FontFamily = Application.Current.Resources["VazirFont"] as FontFamily,
                        FontSize = (double)Application.Current.Resources["TinyFontSize"],
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(5),
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Text = "Use Text(sms) Message"
                    });
                    SignInVM.TwoFactorVerifyOptions = InstagramApiSharp.Enums.InstaTwoFactorVerifyOptions.SmsCode;
                }
                TwoFactorOptionsCmb.Items.Add(new TextBlock
                {
                    FontFamily = Application.Current.Resources["VazirFont"] as FontFamily,
                    FontSize = (double)Application.Current.Resources["TinyFontSize"],
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Text = "Use Recovery Code"
                });
                int index = 0;
                TwoFactorManualSet = true;
                if (authAppToo)
                {
                    TwoFactorOptionsCmb.Items.Add(new TextBlock
                    {
                        FontFamily = Application.Current.Resources["VazirFont"] as FontFamily,
                        FontSize = (double)Application.Current.Resources["TinyFontSize"],
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(5),
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Text = "Use Authentication App"
                    });
                    if (smsCode)
                        index = 2;
                    else
                        index = 1;
                    SignInVM.TwoFactorVerifyOptions = InstagramApiSharp.Enums.InstaTwoFactorVerifyOptions.AuthenticationApp;
                }
                TwoFactorOptionsCmb.SelectedIndex = index;

            }
            catch { }
        }


        private async void TwoFactorOptionsCmbSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (TwoFactorOptionsCmb.SelectedIndex != -1 /*&& !TwoFactorManualSet*/)
                {
                    var item = TwoFactorOptionsCmb.SelectedItem as TextBlock;
                    if (item.Text.ToLower().Contains("authen"))
                    {
                        HideResendCodeGrid();
                        TwoFactorText.Text = "Enter the 6-digit code generated by your authentication app.";
                        TwoFactorVerificationCodeText.PlaceholderText = "Authentication app code";
                        SignInVM.TwoFactorVerifyOptions = InstagramApiSharp.Enums.InstaTwoFactorVerifyOptions.AuthenticationApp;
                    }
                    else if (item.Text.ToLower().Contains("text") || item.Text.ToLower().Contains("sms"))
                    {
                        if (TwoFactorManualSet)
                            ShowResendCodeGrid(true);
                        else
                            ShowResendCodeGrid();
                        SignInVM.TwoFactorVerifyOptions = InstagramApiSharp.Enums.InstaTwoFactorVerifyOptions.SmsCode;
                        var result = await Helper.InstaApiTrash.GetTwoFactorInfoAsync();
                        TwoFactorText.Text = $"Enter the 6-digit code we sent to your number ending in {result.Value?.ObfuscatedPhoneNumber}.";
                        TwoFactorVerificationCodeText.PlaceholderText = "Two factor sms code";
                    }
                    else
                    {
                        HideResendCodeGrid();
                        SignInVM.TwoFactorVerifyOptions = InstagramApiSharp.Enums.InstaTwoFactorVerifyOptions.RecoveryCode;
                        TwoFactorText.Text = "Enter the 8-digit code provided during two-factor authentication set-up.";
                        TwoFactorVerificationCodeText.PlaceholderText = "Recovery code";
                    }
                }

                TwoFactorManualSet = false;
            }
            catch { }
        }
        #region Challenge V2
        public async void StartChallengeV2(InstaChallengeLoginInfo challengeLoginInfo)
        {
            try
            {
                try
                {
                    //UserAgentHelper.SetUserAgent("Mozilla/5.0 (Linux; Android 4.4; Nexus 5 Build/_BuildID_) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/30.0.0.0 Mobile Safari/537.36");
                    UserAgentHelper.SetUserAgent($"Mozilla/5.0 (Linux; Android 10; SM-A205U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.86 Mobile Safari/537.36");
                }
                catch { }
                try
                {
                    DeleteFacebookCookies();
                }
                catch { }
                ChallengeV2Grid.Visibility = Visibility.Visible;
                SignInVM.ChallengeV2LoadingOn();
                await Task.Delay(1500);
                Uri baseUri = new Uri(challengeLoginInfo.Url);
                HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
                var cookies = Helper.InstaApiTrash.HttpRequestProcessor.HttpHandler.CookieContainer
                    .GetCookies(Helper.InstaApiTrash.HttpRequestProcessor.Client.BaseAddress);
                foreach (System.Net.Cookie c in cookies)
                {
                    HttpCookie cookie = new HttpCookie(c.Name, baseUri.Host, "/")
                    {
                        Value = c.Value
                    };
                    filter.CookieManager.SetCookie(cookie, false);
                }

                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, baseUri);
                ChallengeV2kWebView.NavigateWithHttpRequestMessage(httpRequestMessage);
            }
            catch(Exception ex)
            {
                SignInVM.ChallengeV2LoadingOff();
                Helper.ShowErr("Something unexpected happened", ex);
            }
        }
        private async void ChallengeV2kWebViewDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            // {"location": "instagram://checkpoint/dismiss", "type": "CHALLENGE_REDIRECTION", "status": "ok"}

            //SignInVM.LoadingOff();
            SignInVM.ChallengeV2LoadingOff();
            try
            {
                string html = await ChallengeV2kWebView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });

                if (html.Contains("\"instagram://checkpoint/dismiss\"") || html.Contains("instagram://checkpoint/dismiss"))
                {
                    ChallengeV2CloseButtonClick(null, null);

                    SignInVM.Login(null);
                }
            }
            catch { }
        }

        private void ChallengeV2kWebViewNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            SignInVM.ChallengeV2LoadingOn();
        }

        private void ChallengeV2kWebViewUnsupportedUriSchemeIdentified(WebView sender, WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
        {
            //if (args.Uri.Scheme.ToLower() == "fbconnect")
            //    args.Handled = true;
        }

        private void ChallengeV2CloseButtonClick(object sender, RoutedEventArgs e)
        {
            ChallengeV2Grid.Visibility = Visibility.Collapsed;
            SignInVM.ChallengeV2LoadingOff();
        }

        #endregion Challenge V2
    }
}
