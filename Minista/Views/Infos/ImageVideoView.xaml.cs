using InstagramApiSharp.Classes.Models;
using Minista.Classes;
using Minista.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Minista.Views.Infos
{
    public sealed partial class ImageVideoView : Page
    {
        public InstaInboxMedia Media { get; set; }
        public InstaUserInfo User { get; set; }
        public ImageVideoView()
        {
            this.InitializeComponent();
            Loaded += ImageVideoViewLoaded;
        }

        private void ImageVideoViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Media != null)
                {
                    if (Media.MediaType == InstaMediaType.Image)
                    {
                        ImageEX.Source = Media.Images[0].Uri.GetBitmap();
                     /*   ScrollingHost.Visibility =*/ ImageEX.Visibility = Visibility.Visible;
                        MediaElementX.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MediaElementX.Source = Media.Videos[0].Uri.ToUri();
                       /* ScrollingHost.Visibility = */ImageEX.Visibility = Visibility.Collapsed;
                        MediaElementX.Visibility = Visibility.Visible;
                    }
                }
                else if (User != null)
                {
                    ImageEX.Source = User.HdProfilePicUrlInfo.Uri.GetBitmap();
                    /*ScrollingHost.Visibility = */ImageEX.Visibility = Visibility.Visible;
                    MediaElementX.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();
            try
            {
                if (e.Parameter != null && e.Parameter is InstaInboxMedia media)
                    Media = media;
                else if (e.Parameter != null && e.Parameter is InstaUserInfo userInfo)
                    User = userInfo;
            }
            catch { }
        }
        //private async void ScrollViewerDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        //{
        //    try
        //    {
        //        var scrollViewer = sender as ScrollViewer;
        //        var doubleTapPoint = e.GetPosition(scrollViewer);

        //        if (scrollViewer.ZoomFactor != 1)
        //        {
        //            scrollViewer.ZoomToFactor(1);
        //        }
        //        else if (scrollViewer.ZoomFactor == 1)
        //        {
        //            scrollViewer.ZoomToFactor(2);

        //            var dispatcher = Window.Current.CoreWindow.Dispatcher;
        //            await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
        //            {
        //                scrollViewer.ScrollToHorizontalOffset(doubleTapPoint.X);
        //                scrollViewer.ScrollToVerticalOffset(doubleTapPoint.Y);
        //            });
        //        }
        //    }
        //    catch { }
        //}

        private void SaveButtonClick(object sender, RoutedEventArgs e) => StartDownload();

        void StartDownload()
        {
            try
            {
                if (Media != null)
                {
                    switch (Media.MediaType)
                    {
                        case InstaMediaType.Image:
                            {
                                var url = (SettingsHelper.Settings.DownloadQuality == DownloadQuality.HighestQuality ?
                                    Media.Images?[0] : Media.Images.LastOrDefault()).Uri;
                                DownloadHelper.Download(url, Media.Images?.LastOrDefault().Uri, false);
                            }
                            break;
                        case InstaMediaType.Video:
                            {
                                var url = (SettingsHelper.Settings.DownloadQuality == DownloadQuality.HighestQuality ?
                                    Media.Videos?[0] : Media.Videos.LastOrDefault()).Uri;
                                DownloadHelper.Download(url, Media.Images?.LastOrDefault().Uri, true);
                            }
                            break;
                    }
                }
                else if (User != null)
                {
                    DownloadHelper.Download(User.HdProfilePicUrlInfo.Uri, User.ProfilePicture, false, User.UserName);
                }
            }
            catch { }
        }
        private void ScrollingHostViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            Panel.ManipulationMode = ScrollingHost.ZoomFactor == 1 ? ManipulationModes.TranslateY | ManipulationModes.TranslateRailsY | ManipulationModes.System : ManipulationModes.System;
        }

        private async void PanelDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var position = e.GetPosition(Panel);

            if (ScrollingHost.ZoomFactor != 1)
            {
                //ScrollingHost.ZoomToFactor(1);
                ScrollingHost.ChangeView(null, null, 1, true);
            }
            else if (ScrollingHost.ZoomFactor == 1)
            {
                //ScrollingHost.ZoomToFactor(4);

                var dispatcher = Window.Current.CoreWindow.Dispatcher;
                await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    ScrollingHost.ChangeView(position.X, position.Y, 4, true);
                    //ScrollingHost.ScrollToHorizontalOffset(position.X);
                    //ScrollingHost.ScrollToVerticalOffset(position.Y);
                });
            }
        }

    }
}
