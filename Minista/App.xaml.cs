//using Microsoft.Services.Store.Engagement;
using InstagramApiSharp.Classes.Models;
//using Microsoft.Services.Store.Engagement;
using Minista.Helpers;
using MinistaHelper.Push;
using Newtonsoft.Json;
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
        public App()
        {
            InitializeComponent();
            CurrentX = this;
            Suspending += OnSuspending;
            Resuming += OnResuming;
            EnteredBackground += OnEnteredBackground;
            UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Exception.PrintException("App_UnhandledException");
            e.Handled = true;
        }
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            PushHelperX.Register();
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
            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = CreateRootFrame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                if (e.PreviousExecutionState != ApplicationExecutionState.Running)
                {
                    SplashView extendedSplash = new SplashView(e.SplashScreen);
                    rootFrame.Content = extendedSplash;
                    Window.Current.Content = rootFrame;
                }
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (canEnablePrelaunch)
                    TryEnablePrelaunch();
                if (rootFrame.Content == null)
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                Window.Current.Activate();
            }
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
                if (DeviceUtil.IsXbox)
                    FocusVisualKind = FocusVisualKind.Reveal;
            }
            catch { }
            //try
            //{
            //    await Helper.RunInBackground(async () =>
            //    {
            //        StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();

            //        await engagementManager.RegisterNotificationChannelAsync();
            //    });
            //}
            //catch
            //{
            //}
        }
        private void TryEnablePrelaunch() => Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(true);
        public Frame CreateRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }
            return rootFrame;
        }
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Helper.DeleteCachedFolder();

            Helper.DeleteCachedFilesFolder();
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
        private async void OnSuspending(object sender, SuspendingEventArgs e)
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
                    //Helper.InstaApi.PushClient.Shutdown();
                    //await Task.Delay(5500);
                    //await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    //{
                    //    try
                    //    {
                    await Helper.InstaApi.PushClient.TransferPushSocket();
                    await Task.Delay(3500);
                    ////    }
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
        private /*async*/ void OnResuming(object sender, object e)
        {
            try
            {
                //if (Helper.InstaApi?.PushClient is PushClient push && push != null)
                //{
                //    if (string.IsNullOrEmpty(push.LatestErr))
                //        push.Start();
                //    else
                //        await push.StartFresh(true);
                //}
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
        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs e)
        {
            base.OnBackgroundActivated(e);
            ("ONBGAC:     " + e.TaskInstance.TriggerDetails.GetType()).PrintDebug();

            var args = e.TaskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;

            args.PrintDebug();
            Debug.WriteLine("--------------------+OnActivated+------------------");
            Debug.WriteLine(args.Argument);
            Debug.WriteLine("UserInput: ");
            if (args?.UserInput?.Count > 0)
                foreach (var val in args.UserInput)
                    Debug.WriteLine(val.Key + " : " + JsonConvert.SerializeObject(val.Value));
            Debug.WriteLine("--------------------+------------------");

        }   
    }
}
