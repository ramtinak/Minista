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

namespace Minista
{
    sealed partial class App
    {
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
                    await Task.Delay(6500);
                try
                {
                    MainPage.Current?.HandleUriFile(e as FileActivatedEventArgs);
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
    }
}
