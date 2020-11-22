using InstagramApiSharp.Classes.Models;
using LibVLCSharp.Shared;
using Minista.Helpers;
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

namespace Minista.Views.Broadcast
{
    public sealed partial class LiveBroadcastView : Page
    {
        private InstaBroadcast Broadcast;
        public LiveBroadcastView()
        {
            this.InitializeComponent();
            Loaded += LiveBroadcastView_Loaded;
        }

        private async void LiveBroadcastView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Delay(1500);
                if (Broadcast != null)
                {
                    LiveVM.Broadcast = Broadcast;
                    await Task.Delay(500);
                    LiveVM.Play();
                }
            }
            catch { }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MainPage.Current?.HideHeaders();
            Helper.HideStatusBar();
            if (e.Parameter is InstaBroadcast broadcast && broadcast != null)
                Broadcast = broadcast;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MainPage.Current?.ShowHeaders();
            Helper.ShowStatusBar();

            NavigationService.HideSystemBackButton();
            if (MainPage.Current != null)
                NavigationService.ShowBackButton();
        }
    }
}
