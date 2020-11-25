using InstagramApiSharp.Classes;
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
using Windows.UI.Core;
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
        CompositeTransform LastCompositeTransform;
        private InstaBroadcast Broadcast;
        public static LiveBroadcastView Current;
        public LiveBroadcastView()
        {
            this.InitializeComponent();
            Current = this;
            Loaded += LiveBroadcastView_Loaded;
        }

        private async void LiveBroadcastView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LiveVM.Reset();
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
        private void OnPointerWheelChanged(CoreWindow sender, PointerEventArgs e)
        {
            // Changing volume is not working in LibVLC
            //try
            //{
            //    if (e.CurrentPoint.Properties.MouseWheelDelta > 0)
            //    {
            //        if (LiveVM.MediaPlayer != null)
            //        {
            //            if (LiveVM.MediaPlayer.Volume < 100)
            //            {
            //                LiveVM.MediaPlayer.Volume = LiveVM.MediaPlayer.Volume + 1;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (LiveVM.MediaPlayer != null)
            //        {
            //            if (LiveVM.MediaPlayer.Volume > 0)
            //            {
            //                LiveVM.MediaPlayer.Volume = LiveVM.MediaPlayer.Volume - 1;
            //            }
            //        }
            //    }
            //}
            //catch { }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MainPage.Current?.HideHeaders();
            Helper.HideStatusBar();
            if (e.Parameter is InstaBroadcast broadcast && broadcast != null)
                Broadcast = broadcast;
            Window.Current.CoreWindow.PointerWheelChanged += OnPointerWheelChanged;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MainPage.Current?.ShowHeaders();
            Helper.ShowStatusBar();
            Window.Current.CoreWindow.PointerWheelChanged -= OnPointerWheelChanged;

            NavigationService.HideSystemBackButton();
            if (MainPage.Current != null)
                NavigationService.ShowBackButton();
            try
            {
                LiveVM.Stop();
            }
            catch { }
            try
            {
                LiveVM.EndBroadcast();
            }
            catch { }
        }

        private void RotateButtonClick(object sender, RoutedEventArgs e)
        {
            if (LastCompositeTransform == null)
            {
                LastCompositeTransform = new CompositeTransform { Rotation = -90 };
                VlcVideoView.Width = double.NaN;
                VlcVideoView.Height = double.NaN;
            }
            else
            {
                LastCompositeTransform = null;
                VlcVideoView.Width = double.NaN;
                VlcVideoView.Height = double.NaN;
            }
            VlcVideoView.RenderTransformOrigin = new Point(0.5, 0.5);
            VlcVideoView.RenderTransform = LastCompositeTransform;
        }


        private async void CommentButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LiveVM.Broadcast == null) return;
                if (string.IsNullOrEmpty(CommentText.Text))
                {
                    CommentText.Focus(FocusState.Keyboard);
                    return;
                }
                if (LiveVM.CommentsVisibility == Visibility.Collapsed)
                    return;
                var result = await Helper.InstaApi.LiveProcessor.CommentAsync(Broadcast.Id, CommentText.Text);
                if (result.Succeeded)
                {
                    // no need to notify, comment will be visible after a few seconds
                    CommentText.Text = "";
                }
                else
                {
                    switch (result.Info.ResponseType)
                    {
                        case ResponseType.RequestsLimit:
                        case ResponseType.SentryBlock:
                            Helper.ShowNotify(result.Info.Message);
                            break;
                        case ResponseType.ActionBlocked:
                            Helper.ShowNotify("Action blocked.\r\nPlease try again 5 or 10 minutes later");
                            break;
                    }
                }
            }
            catch { }
        }
        public bool IsInLoading => LoadingUc.Visibility == Visibility.Visible;
        public void ShowLoading() => LoadingUc.Start();
        public void HideLoading() => LoadingUc.Stop();
    }
}
