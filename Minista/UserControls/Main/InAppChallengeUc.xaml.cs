using InstagramApiSharp.Classes;
using Minista.Helpers;
using Minista.Views.Sign;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Minista.UserControls.Main
{
    public sealed partial class InAppChallengeUc : UserControl
    {
        public bool ChallengeV2LoadingRingIsActive { get; set; } = false;
        public Visibility ChallengeV2LoadingGridVisibility { get; set; } = Visibility.Collapsed;
        public InAppChallengeUc()
        {
            this.InitializeComponent();
        }
        public bool IsMe() => Visibility == Visibility.Visible;
        public async void StartChallengeV2(InstaChallengeLoginInfo challengeLoginInfo)
        {
            try
            {
                try
                {
                    var device = Helper.InstaApi.GetCurrentDevice();
                    //UserAgentHelper.SetUserAgent("Mozilla/5.0 (Linux; Android 4.4; Nexus 5 Build/_BuildID_) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/30.0.0.0 Mobile Safari/537.36");
                    UserAgentHelper.SetUserAgent($"Mozilla/5.0 (Linux; Android {device.AndroidVer.VersionNumber}; {device.DeviceModelIdentifier}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.86 Mobile Safari/537.36");
                }
                catch
                {
                    try
                    {
                        UserAgentHelper.SetUserAgent($"Mozilla/5.0 (Linux; Android 10; SM-A205U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.86 Mobile Safari/537.36");
                    }
                    catch { }
                }
                try
                {
                    SignInView.DeleteFacebookCookies();
                }
                catch { }
                Visibility = Visibility.Visible;
                ChallengeV2LoadingOn();
                await Task.Delay(1500);
                Uri baseUri = new Uri(challengeLoginInfo.Url);
                HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
                var cookies = Helper.InstaApi.HttpRequestProcessor.HttpHandler.CookieContainer
                    .GetCookies(Helper.InstaApi.HttpRequestProcessor.Client.BaseAddress);
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
            catch (Exception ex)
            {
                ChallengeV2LoadingOff();
                Helper.ShowErr("Something unexpected happened", ex);
            }
        }
        private async void ChallengeV2kWebViewDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            // {"location": "instagram://checkpoint/dismiss", "type": "CHALLENGE_REDIRECTION", "status": "ok"}

            ChallengeV2LoadingOff();
            try
            {
                string html = await ChallengeV2kWebView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });

                html.PrintDebug();
                if (html.Contains("\"instagram://checkpoint/dismiss\"") || html.Contains("instagram://checkpoint/dismiss"))
                {
                    ChallengeV2CloseButtonClick(null, null);

                    //SignInVM.Login(null);
                }
            }
            catch { }
        }

        private void ChallengeV2kWebViewNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            ChallengeV2LoadingOn();
        }

        private void ChallengeV2kWebViewUnsupportedUriSchemeIdentified(WebView sender, WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
        {
            //if (args.Uri.Scheme.ToLower() == "fbconnect")
            //    args.Handled = true;
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
        private void FacebookWebViewPermissionRequested(WebView sender, WebViewPermissionRequestedEventArgs args)
        {
            args.PermissionRequest.Deny();
        }
        private void ChallengeV2CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            ChallengeV2LoadingOff();
        }
        public void ChallengeV2LoadingOn()
        {
            //ChallengeV2LoadingRingIsActive = true;
            //ChallengeV2LoadingGridVisibility = Visibility.Visible;
        }
        public void ChallengeV2LoadingOff()
        {
            //ChallengeV2LoadingRingIsActive = false;
            //ChallengeV2LoadingGridVisibility = Visibility.Collapsed;
        }
    }
}
