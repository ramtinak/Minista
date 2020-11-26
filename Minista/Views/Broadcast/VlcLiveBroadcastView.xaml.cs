using InstagramApiSharp.Classes;
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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VlcLiveBroadcastView : Page
    {
        CompositeTransform LastCompositeTransform;
        private InstaBroadcast Broadcast;
        private string BroadcastId;
        public static VlcLiveBroadcastView Current;
        public VlcLiveBroadcastView()
        {
            InitializeComponent();
            Current = this;
            Loaded += VlcLiveBroadcastViewLoaded;
        }

        private async void VlcLiveBroadcastViewLoaded(object sender, RoutedEventArgs e)
        {
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
                    LiveVM.Play();
                }
                else if (!string.IsNullOrEmpty(BroadcastId))
                {
                    ShowLoading();
                    await LiveVM.SetBroadcast(BroadcastId);
                    HideLoading();
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
            {
                Broadcast = broadcast;
                BroadcastId = null;
            }
            else if (e.Parameter is string broadcastId && !string.IsNullOrEmpty(broadcastId))
            {
                BroadcastId = broadcastId;
                Broadcast = null;
            }
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
