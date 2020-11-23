//using Microsoft.Services.Store.Engagement;
using Microsoft.Services.Store.Engagement;
using Minista.Helpers;
using MinistaHelper.Push;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Display;
using Windows.System.Profile;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Minista
{
    sealed partial class App : Application
    {
        private static DisplayRequest appDisplayRequest;
        private static bool isDisplayRequestActive = false;

        internal static void ActivateDisplayRequest()
        {
            if (!isDisplayRequestActive)
            {
                if (appDisplayRequest == null)
                {
                    appDisplayRequest = new DisplayRequest();
                }
                appDisplayRequest.RequestActive();
                isDisplayRequestActive = true;
            }
        }

        private static Guid id = Guid.NewGuid();
        public static Guid Id { get { return id; } }
        protected override void OnActivated(IActivatedEventArgs e)
        {
            try
            {
                if (DeviceUtil.IsXbox)
                    Helper.FullscreenModeInXbox();
            }
            catch { }
            if (e.Kind == ActivationKind.Protocol)
            {
                var uriArgs = e as ProtocolActivatedEventArgs;
                System.Diagnostics.Debug.WriteLine("URI>>>>>> " + uriArgs.Uri.ToString());
                MainPage.NavigationUriProtocol = uriArgs.Uri.ToString();

                // Ensure the current window is active
                //Window.Current.Activate();

                Frame rootFrame = CreateRootFrame();
                if (rootFrame.Content == null)
                //if (!(Window.Current.Content is Frame rootFrame))
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    if (e.PreviousExecutionState != ApplicationExecutionState.Running)
                    {
                        bool loadState = (e.PreviousExecutionState == ApplicationExecutionState.Terminated);
                        SplashView extendedSplash = new SplashView(e.SplashScreen, loadState);
                        rootFrame.Content = extendedSplash;
                        Window.Current.Content = rootFrame;
                    }
                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }
                else
                    MainPage.Current?.HandleUriProtocol();

                rootFrame.Content.GetType().PrintDebug();
                Window.Current.Content.GetType().PrintDebug();
                Window.Current.Activate();
            }
            else if (e.Kind == ActivationKind.ToastNotification)
            {
                e.GetType().PrintDebug();
                var args = e as ToastNotificationActivatedEventArgs;
                args.PrintDebug();
                Frame rootFrame = CreateRootFrame();
                if (rootFrame.Content == null)
                //if (!(Window.Current.Content is Frame rootFrame))
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    if (e.PreviousExecutionState != ApplicationExecutionState.Running)
                    {
                        bool loadState = (e.PreviousExecutionState == ApplicationExecutionState.Terminated);
                        SplashView extendedSplash = new SplashView(e.SplashScreen, loadState);
                        rootFrame.Content = extendedSplash;
                        Window.Current.Content = rootFrame;
                    }
                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }
                else
                    MainPage.Current?.HandleUriProtocol();

                rootFrame.Content.GetType().PrintDebug();
                Window.Current.Content.GetType().PrintDebug();
                Window.Current.Activate();
            }

        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
            this.EnteredBackground += OnEnteredBackground;
            this.UnhandledException += App_UnhandledException;
            //ExtensionHelper.GetAppVersion().PrintDebug();
            DeviceHelper.GetDeviceInfo();
            try
            {
                if (DeviceUtil.IsXbox)
                {
                    Helper.FullscreenModeInXbox();
                    RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
                }
            }
            catch { }
            try
            {
                UserAgentHelper.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 Safari/537.36");
            }
            catch { }
            try
            {
                if (/*AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox"*/ DeviceUtil.IsXbox)
                {
                    FocusVisualKind = FocusVisualKind.Reveal;
                }
            }
            catch { }
            //try
            //{
            //    Application.Current.Resources["DefaultBackgroundColor"] = Helper.GetColorBrush("#18227c");
            //}
            //catch (Exception ex)
            //{
            //}
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            //var ex = e.Exception;
            //var stac = Environment.StackTrace;
            //var fuc = ex.PrintException("App_UnhandledException");
            e.Handled = true;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            bool canEnablePrelaunch = Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Windows.ApplicationModel.Core.CoreApplication", "EnablePrelaunch");

            if (e != null)
            {
                if (e.PreviousExecutionState == ApplicationExecutionState.Running)
                {
                    Window.Current.Activate();
                    return;
                }
            }
            Helper.ChangeAppMinSize(540, 744);

            try
            {

                //StoreServicesNotificationChannelParameters parameters =
                //    new StoreServicesNotificationChannelParameters();
                //parameters.CustomNotificationChannelUri = "Assign your channel URI here";

                //StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
                //await engagementManager.RegisterNotificationChannelAsync(parameters);

                await Helper.RunInBackground(async () =>
                {
                    StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();

                    await engagementManager.RegisterNotificationChannelAsync();
                });

            }
            catch
            {
                //
            }

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = CreateRootFrame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }
                if (e.PreviousExecutionState != ApplicationExecutionState.Running)
                {
                    bool loadState = (e.PreviousExecutionState == ApplicationExecutionState.Terminated);
                    SplashView extendedSplash = new SplashView(e.SplashScreen, loadState);
                    rootFrame.Content = extendedSplash;
                    //rootFrame.Content = new Views.BlankPage1();

                    Window.Current.Content = rootFrame;
                }
                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (canEnablePrelaunch)
                {
                    TryEnablePrelaunch();
                }
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Encapsulates the call to CoreApplication.EnablePrelaunch() so that the JIT
        /// won't encounter that call (and prevent the app from running when it doesn't
        /// find it), unless this method gets called. This method should only
        /// be called when the caller determines that we are running on a system that
        /// supports CoreApplication.EnablePrelaunch().
        /// </summary>
        private void TryEnablePrelaunch()
        {
            Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(true);
        }
        public Frame CreateRootFrame()
        {
#pragma warning disable IDE0019 // Use pattern matching
            Frame rootFrame = Window.Current.Content as Frame;
#pragma warning restore IDE0019 // Use pattern matching

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame
                {

                    //// Set the default language
                    //Language = Windows.Globalization.ApplicationLanguages.Languages[0]
                };
                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            return rootFrame;
        }
        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Helper.DeleteCachedFolder();

            Helper.DeleteCachedFilesFolder();
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private /*async*/ void OnSuspending(object sender, SuspendingEventArgs e)
        {
            Helper.DeleteCachedFolder();

            Helper.DeleteCachedFilesFolder();
            SettingsHelper.SaveSettings();
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            try
            {
                if(Helper.InstaApi?.PushClient != null)
                {
                    Helper.InstaApi.PushClient.Shutdown();
                    //await Task.Delay(5500);
                    //await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    //{
                    //    try
                    //    {
                    //        await Task.Delay(5500);
                    //        await Helper.InstaApi.PushClient.TransferPushSocket();
                    //    }
                    //    catch { }

                    //    });

                }
                //if (MultiApiHelper.Pushs.Count > 0)
                //    for (int i = 0; i < MultiApiHelper.Pushs.Count; i++)
                //    {
                //        var push = MultiApiHelper.Pushs[i];
                //        await push.TransferPushSocket();
                //    }
            }
            catch (Exception exception)
            {
                exception.PrintException();
            }
            finally
            {
                deferral.Complete();
            }
        }
        private async void OnResuming(object sender, object e)
        {
            try
            {
                if (Helper.InstaApi?.PushClient is PushClient push && push != null)
                {
                    if (string.IsNullOrEmpty(push.LatestErr))
                        push.Start();
                    else
                        await push.StartFresh(true);
                }
                //if (MultiApiHelper.Pushs.Count > 0)
                //    for (int i = 0; i < MultiApiHelper.Pushs.Count; i++)
                //    {
                //        var push = MultiApiHelper.Pushs[i];
                //        push.Start();
                //    }
            }
            catch (Exception exception)
            {
                exception.PrintException();
            }
        }

        private void OnEnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
        }

        protected override /*async*/ void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            _ = args.TaskInstance.GetDeferral();
            ("ONBGAC:     " + args.TaskInstance.TriggerDetails.GetType()).PrintDebug();
        }   
    }
}
