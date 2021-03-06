﻿using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Minista.ContentDialogs;
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
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Storage.Streams;
using System.Xml.Serialization;
using System.Xml;

namespace Minista.Views.Broadcast
{
    public sealed partial class LiveBroadcastView : Page
    {
        CompositeTransform LastCompositeTransform;
        private InstaBroadcast Broadcast;
        private string BroadcastId;
        public static LiveBroadcastView Current;

        const double MaxVolume = 1; 
        double indexVolume = 1; 
        double LastPositionVolume = 0; 

        public LiveBroadcastView()
        {
            InitializeComponent();
            Current = this;
            Loaded += LiveBroadcastViewLoaded;

        }

        private async void LiveBroadcastViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                mediaElement.ManipulationMode = ManipulationModes.TranslateY;
                mediaElement.ManipulationDelta -= OnManipulationDelta;
                mediaElement.ManipulationDelta += OnManipulationDelta;
            }
            catch { }

            try
            {
                LiveVM.Reset();
                await Task.Delay(1500);
                if (Broadcast != null)
                {
                    ShowLoading();
                    await LiveVM.SetBroadcast(Broadcast);
                    HideLoading();
                    await Task.Delay(500);
                    LiveVM.Play(mediaElement);
                }
                else if (!string.IsNullOrEmpty(BroadcastId))
                {
                    ShowLoading();
                    await LiveVM.SetBroadcast(BroadcastId);
                    HideLoading();
                    await Task.Delay(500);
                    LiveVM.Play(mediaElement);
                }
            }
            catch { }
        }
        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (LastPositionVolume < e.Position.Y)
            {
                if (indexVolume > 0)
                {
                    indexVolume -= .005;
                    VolumeResult(indexVolume);
                    string.Format("Volume: {0}%", (int)(indexVolume * 100));
                }
                else
                {
                    VolumeResult(indexVolume = 0);
                }
            }
            else
            {
                if (indexVolume < MaxVolume)
                {
                    indexVolume += .005;
                    VolumeResult(indexVolume);
                }
                else
                    VolumeResult(indexVolume = MaxVolume);
            }
            LastPositionVolume = e.Position.Y;
        }
        private void OnPointerWheelChanged(CoreWindow sender, PointerEventArgs e)
        {
            // Changing volume is not working in LibVLC
            try
            {
                if (LiveVM.MinistaPlayer.MediaPlayer != null)
                {
                    if (e.CurrentPoint.Properties.MouseWheelDelta > 0)
                    {
                        if (LiveVM.MinistaPlayer.MediaPlayer.Volume < 1.0)
                            VolumeResult(LiveVM.MinistaPlayer.MediaPlayer.Volume + .005);
                    }
                    else
                    {
                        if (LiveVM.MinistaPlayer.MediaPlayer.Volume > 0)
                            VolumeResult(LiveVM.MinistaPlayer.MediaPlayer.Volume - .005);
                    }
                }
            }
            catch { }
        }
        void VolumeResult(double volume)
        {
            try
            {
                if (LiveVM.MinistaPlayer?.MediaPlayer != null)
                    LiveVM.MinistaPlayer.MediaPlayer.Volume = volume;
            }
            catch { }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MainPage.Current?.HideHeaders();
            Helper.HideStatusBar();
            if (e.Parameter is InstaBroadcast broadcast && broadcast != null)
            {
                Broadcast = broadcast;
                BroadcastId = null;
            }
            else if (e.Parameter is string broadcastId && !string.IsNullOrEmpty(broadcastId))
            {
                BroadcastId = broadcastId;
                Broadcast = null;
            }
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
                Broadcast = null;
                BroadcastId = null;
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
                mediaElement.Width = double.NaN;
                mediaElement.Height = double.NaN;
            }
            else
            {
                LastCompositeTransform = null;
                mediaElement.Width = double.NaN;
                mediaElement.Height = double.NaN;
            }
            mediaElement.RenderTransformOrigin = new Point(0.5, 0.5);
            mediaElement.RenderTransform = LastCompositeTransform;
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
        private async void ForwardButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LiveVM.Broadcast == null) return;
                await new UsersDialog(LiveVM.Broadcast).ShowAsync();
            }
            catch { }
        }
        public bool IsInLoading => LoadingUc.Visibility == Visibility.Visible;
        public void ShowLoading() => LoadingUc.Start();
        public void HideLoading() => LoadingUc.Stop();

    }
}
