using InstagramApiSharp.Classes.Models;
using LibVLCSharp.Shared;
using Minista.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
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
        private bool CanChangePlayerSize = false;
        CompositeTransform LastCompositeTransform;
        private InstaBroadcast Broadcast;
        public LiveBroadcastView()
        {
            this.InitializeComponent();
            Loaded += LiveBroadcastView_Loaded;
            SizeChanged += LiveBroadcastView_SizeChanged;
        }

        private void LiveBroadcastView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (CanChangePlayerSize)
                {
                    //if (LastCompositeTransform != null)
                    //{

                    //    VlcVideoView.Width = ActualHeight;
                    //    VlcVideoView.Height = ActualWidth;
                    //}
                    //else
                    //{
                    //    VlcVideoView.Width = double.NaN;
                    //    VlcVideoView.Height = double.NaN;
                    //}

                }
            }
            catch { }
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
            try
            {
                LiveVM.Stop();
            }
            catch { }
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            if (LastCompositeTransform == null)
            {
                LastCompositeTransform = new CompositeTransform { Rotation = -90 };
                VlcVideoView.Width = double.NaN;
                VlcVideoView.Height = double.NaN;
            }
            else
            {
                if (LastCompositeTransform.Rotation == -90)
                {
                    LastCompositeTransform = null;
                    VlcVideoView.Width = double.NaN;
                    VlcVideoView.Height = double.NaN;
                }
                else
                {
                    LastCompositeTransform = new CompositeTransform { Rotation = -90 };

                    VlcVideoView.Width = double.NaN;
                    VlcVideoView.Height = ActualWidth;
                    //VlcVideoView.Width = ActualHeight;
                    //VlcVideoView.Height = ActualWidth;
                }
            }
            CanChangePlayerSize = LastCompositeTransform != null;
            VlcVideoView.RenderTransformOrigin = new Point(0.5, 0.5);
            VlcVideoView.RenderTransform = LastCompositeTransform;
        }
    }
}
