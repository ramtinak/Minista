using Minista.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Minista
{
    public sealed partial class SplashView : Page
    {
        private readonly SplashScreen splash;
        internal bool dismissed = false; 
        internal Frame rootFrame;
        public SplashView(bool canLoaded =false)
        {
            App.ActivateDisplayRequest();
            CanLoaded = canLoaded;
            Loaded += SplashViewLoaded;
            rootFrame = new Frame();
        }
        public SplashView(SplashScreen splashscreen)
        {
            this.InitializeComponent();
            splash = splashscreen;
            if (splash != null)
                splash.Dismissed += new TypedEventHandler<SplashScreen, object>(DismissedEventHandler);
            Loaded += SplashViewLoaded;
            rootFrame = new Frame();
        }
        public bool CanLoaded = true;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                if (DeviceUtil.IsXbox)
                    Helper.FullscreenModeInXbox();
            }
            catch { }
            if (e != null && e.Parameter != null && e.Parameter is bool b)
                CanLoaded = b;
        }
        private async void SplashViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            { 
                ShowLoading();
                if (CanLoaded)
                    SettingsHelper.LoadSettings();

                Helper.CreateCachedFolder();

                await Task.Delay(100);
                if (CanLoaded)
                    await SessionHelper.LoadSessionsAsync();
                await Task.Delay(100);

                ThemeHelper.InitTheme(SettingsHelper.Settings.CurrentTheme);
            }
            catch { }
            try
            {
                HideLoading();
            }
            catch { }
            try
            {
                Dismiss();
            }
            catch { }
        }
        private void Dismiss()
        {
            rootFrame.Navigate(typeof(MainPage));
            Window.Current.Content = rootFrame;
        }
        public void ShowLoading()
        {
            LoadingUc.Start();
            //LoadingPb.IsActive = true;
            //LoadingGrid.Visibility = Visibility.Visible;
        }
        public void HideLoading()
        {
            LoadingUc.Stop();
            //LoadingPb.IsActive = false;
            //LoadingGrid.Visibility = Visibility.Collapsed;
        }
        void DismissedEventHandler(SplashScreen sender, object e)
        {
            dismissed = true;
        }
    }
}
