using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Minista.Helpers;
using Minista.Views;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using Windows.System.Display;

namespace Minista
{
    sealed partial class App
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
        public static App CurrentX { get; private set; }
        protected override async void OnActivated(IActivatedEventArgs e)
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
                        SplashView extendedSplash = new SplashView(e.SplashScreen);
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
                Debug.WriteLine("--------------------+OnActivated+------------------");
                Debug.WriteLine(args.Argument);
                Debug.WriteLine("UserInput: ");
                if (args?.UserInput?.Count > 0)
                    foreach (var val in args.UserInput)
                        Debug.WriteLine(val.Key + " : " + JsonConvert.SerializeObject(val.Value));
                Debug.WriteLine("--------------------+------------------");
                Frame rootFrame = CreateRootFrame();
                bool wait = false;
                if (rootFrame.Content == null)
                {
                    wait = true;
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    if (e.PreviousExecutionState != ApplicationExecutionState.Running)
                    {
                        SplashView extendedSplash = new SplashView(e.SplashScreen);
                        rootFrame.Content = extendedSplash;
                        Window.Current.Content = rootFrame;
                    }
                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }
                else
                {
                    HandleActivation(args, false);
                    MainPage.Current?.HandleUriProtocol();
                }

                rootFrame.Content.GetType().PrintDebug();
                Window.Current.Content.GetType().PrintDebug();
                Window.Current.Activate();

                // wait 
                if (wait)
                {
                    await Task.Delay(3500);
                    HandleActivation(args, true);
                }
            }
        }

        void HandleActivation(ToastNotificationActivatedEventArgs args, bool wait)
        {
            NotificationActivationHelper.HandleActivation(Helper.InstaApi, Helper.InstaApiList, args.Argument,
            args.UserInput, wait, OpenProfile, OpenLive, OpenPendingThreadRequest, OpenPost, OpenTV);
        }


        protected override async void OnShareTargetActivated(ShareTargetActivatedEventArgs e)
        {
            try
            {
                if (DeviceUtil.IsXbox)
                    Helper.FullscreenModeInXbox();
            }
            catch { }
            try
            {
                Frame rootFrame = CreateRootFrame();
                var isNull = false;
                var isRunning = false;
                if (rootFrame.Content == null)
                //if (!(Window.Current.Content is Frame rootFrame))
                {
                    isRunning = isNull = true;
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    if (e.PreviousExecutionState != ApplicationExecutionState.Running)
                    {
                        SplashView extendedSplash = new SplashView(e.SplashScreen);
                        rootFrame.Content = extendedSplash;
                        Window.Current.Content = rootFrame;
                    }
                    else
                    {
                        SplashView extendedSplash = new SplashView(e.SplashScreen);
                        rootFrame.Content = extendedSplash;
                        Window.Current.Content = rootFrame;
                    }
                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }
                //rootFrame.Content.GetType().PrintDebug();
                //Window.Current.Content.GetType().PrintDebug();
                //if(!isRunning)
                Window.Current.Activate();
                if (isNull)
                    await Task.Delay(6500);
                try
                {
                    ShareOperation shareOperation = e.ShareOperation;
                    //Debug.WriteLine(string.Join("\n", shareOperation.Data.AvailableFormats));
                    if (shareOperation.Data.Contains(StandardDataFormats.StorageItems))
                    {
                        var items = await shareOperation.Data.GetStorageItemsAsync();

                        if (items.Count > 0)
                        {
                            if (items[0] is StorageFile file)
                                MainPage.Current?.HandleUriFile(file);
                        }
                    }
                }
                catch { }
                //if (Helper.InstaApi != null)
                //{
                //    if (Helper.InstaApi.IsUserAuthenticated)
                //    {


                //        return;
                //    }
                //}
            }
            catch { }
            // Code to handle activation goes here. 
            //ShareOperation shareOperation = args.ShareOperation;
            //Debug.WriteLine(string.Join("\n", shareOperation.Data.AvailableFormats));
            //if (shareOperation.Data.Contains(StandardDataFormats.StorageItems))
            //{
            //    var items = await shareOperation.Data.GetStorageItemsAsync();

            //    if (items.Count > 0)
            //    {
            //        var ff = items[0] as StorageFile;
            //    }
            //}
            //else if (shareOperation.Data.Contains(StandardDataFormats.ApplicationLink))
            //{
            //    var items = await shareOperation.Data.GetApplicationLinkAsync();

            //}
            //else if (shareOperation.Data.Contains(StandardDataFormats.WebLink))
            //{
            //    var items = await shareOperation.Data.GetWebLinkAsync();

            //}
            //else if (shareOperation.Data.Contains(StandardDataFormats.Bitmap))
            //{
            //    var items = await shareOperation.Data.GetBitmapAsync();

            //}
        }
        protected async override void OnFileActivated(FileActivatedEventArgs e)
        {
            try
            {
                if (DeviceUtil.IsXbox)
                    Helper.FullscreenModeInXbox();
            }
            catch { }
            try
            {
                Frame rootFrame = CreateRootFrame();
                var isNull = false;
                if (rootFrame.Content == null)
                //if (!(Window.Current.Content is Frame rootFrame))
                {
                    isNull = true;
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    if (e.PreviousExecutionState != ApplicationExecutionState.Running)
                    {
                        SplashView extendedSplash = new SplashView(e.SplashScreen);
                        rootFrame.Content = extendedSplash;
                        Window.Current.Content = rootFrame;
                    }
                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                    //await Task.Delay(6500);
                }
                rootFrame.Content.GetType().PrintDebug();
                Window.Current.Content.GetType().PrintDebug();
                Window.Current.Activate();
                if (isNull)
                {
                    if (e.Files?.Count > 0 && e.Files[0] is StorageFile file)
                    {
                        if (file.Path.Contains(".mi-theme"))
                        {
                            var json = await FileIO.ReadTextAsync(file);
                            ThemeHelper.InitTheme(json);
                            return;
                        }
                    }
                    await Task.Delay(6500);
                }
                try
                {
                    MainPage.Current?.HandleUriFile(e);
                }
                catch { }
                //if (Helper.InstaApi != null)
                //{
                //    if (Helper.InstaApi.IsUserAuthenticated)
                //    {


                //        return;
                //    }
                //}
            }
            catch { }
        }
        internal async void OpenProfile(long pk)
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => Helper.OpenProfile(pk));
            }
            catch { }
        }
        internal async void OpenLive(string id)
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => Helper.OpenLive(id));
            }
            catch { }
        }
        internal async void OpenPendingThreadRequest(string threadId, InstaUserShortFriendship userShortFriendship)
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var thread = ViewModels.Direct.ThreadViewModel.CreateFakeThread(userShortFriendship);
                    thread.ThreadId = threadId;
                    NavigationService.Navigate(typeof(Views.Direct.DirectRequestsThreadView), thread);
                });
            }
            catch { }
        }
        internal async void OpenPost(string id)
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        NavigationService.Navigate(typeof(Views.Posts.SinglePostView), id));
            }
            catch { }
        }
        internal async void OpenTV(InstaMedia tv)
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                       NavigationService.Navigate(typeof(Views.TV.TVPlayer), new object[] { new List<InstaMedia> { tv }, 0 }));
            }
            catch { }
        }

    }
}
