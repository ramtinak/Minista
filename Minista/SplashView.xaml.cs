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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SplashView : Page
    {
        private SplashScreen splash; // Variable to hold the splash screen object.
        internal bool dismissed = false; // Variable to track splash screen dismissal status.
        internal Frame rootFrame;
        //private readonly DispatcherTimer _timer;
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
            //StatusBar statusbar = StatusBar.GetForCurrentView();
            //statusbar.HideAsync();
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
                //Application.Current.Resources["DefaultBackgroundColor"] = Helper.GetColorBrush("#c4001d");
                //Application.Current.Resources["DefaultItemBackgroundColor"] = Helper.GetColorBrush("#7f0000");
                //Application.Current.Resources["DefaultForegroundColor"] = Helper.GetColorBrush("#ffffff");
                //Application.Current.Resources["DefaultInnerForegroundColor"] = Helper.GetColorBrush("#ffffff");

                //Application.Current.Resources["DefaultBackgroundColor"] = Helper.GetColorBrush("#000000");
                //Application.Current.Resources["DefaultItemBackgroundColor"] = Helper.GetColorBrush("#170400");
                //Application.Current.Resources["DefaultForegroundColor"] = Helper.GetColorBrush("#ffff00");
                //Application.Current.Resources["DefaultInnerForegroundColor"] = Helper.GetColorBrush("#ffea00");

                //// WHITE THEME
                //Application.Current.Resources["DefaultBackgroundColor"] = Helper.GetColorBrush("#FFFFFFFF");
                //Application.Current.Resources["DefaultItemBackgroundColor"] = Helper.GetColorBrush("#FFE8E8E8");
                //Application.Current.Resources["DefaultForegroundColor"] = Helper.GetColorBrush("#FF2F2F2F");
                //Application.Current.Resources["DefaultInnerForegroundColor"] = Helper.GetColorBrush("#FF575757");
            }
            catch /*(Exception ex)*/{ }
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
            // Place the frame in the current Window
            Window.Current.Content = rootFrame;
        }
        public void ShowLoading()
        {
            LoadingPb.IsActive = true;
            LoadingGrid.Visibility = Visibility.Visible;
        }
        public void HideLoading()
        {
            LoadingPb.IsActive = false;
            LoadingGrid.Visibility = Visibility.Collapsed;
        }

        // Include code to be executed when the system has transitioned from the splash screen to the extended splash screen (application's first view).
        void DismissedEventHandler(SplashScreen sender, object e)
        {
            dismissed = true;
        }
    }
}
