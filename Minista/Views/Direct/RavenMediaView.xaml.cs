using InstagramApiSharp.Classes.Models;
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


namespace Minista.Views.Direct
{
    public sealed partial class RavenMediaView : Page
    {
        private InstaDirectInboxItem InboxItem;
        private InstaDirectInboxThread CurrentThread;
        public RavenMediaView()
        {
            this.InitializeComponent();
            Loaded += ImageVideoViewLoaded;
        }
     
        private void ImageVideoViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (InboxItem == null) return;
                if (InboxItem.VisualMedia != null && !InboxItem.VisualMedia.IsExpired && InboxItem.VisualMedia.SeenCount != null)
                {
                    if (InboxItem.VisualMedia.Media.MediaType == InstaMediaType.Image)
                    {
                        ImageEX.Source = InboxItem.VisualMedia.Media.Images[0].Uri.GetBitmap();
                        /*ScrollingHost.Visibility =*/ ImageEX.Visibility = Visibility.Visible;
                        MediaElementX.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MediaElementX.Source = InboxItem.VisualMedia.Media.Videos[0].Uri.ToUri();
                        /*ScrollingHost.Visibility = */ImageEX.Visibility = Visibility.Collapsed;
                        MediaElementX.Visibility = Visibility.Visible;
                    }
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
                if (e.Parameter != null && e.Parameter is object[] obj)
                {
                    CurrentThread = obj[0] as InstaDirectInboxThread;

                    InboxItem = obj[1] as InstaDirectInboxItem;
                }
            }
            catch { }
        }
        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            try
            {
                await Helper.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                 {
                     var result = await Helper.InstaApi.MessagingProcessor.MarkDirectThreadAsSeenAsync(CurrentThread.ThreadId, InboxItem.ItemId);
                 });
            }
            catch { }
        }
        private async void ScrollViewerDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
                var scrollViewer = sender as ScrollViewer;
                var doubleTapPoint = e.GetPosition(scrollViewer);

#pragma warning disable CS0618 // Type or member is obsolete
                if (scrollViewer.ZoomFactor != 1)
                {
                    scrollViewer.ZoomToFactor(1);
                }
                else if (scrollViewer.ZoomFactor == 1)
                {
                    scrollViewer.ZoomToFactor(2);

                    var dispatcher = Window.Current.CoreWindow.Dispatcher;
                    await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        scrollViewer.ScrollToHorizontalOffset(doubleTapPoint.X);
                        scrollViewer.ScrollToVerticalOffset(doubleTapPoint.Y);
                    });
                }
#pragma warning restore CS0618 // Type or member is obsolete
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
                ScrollingHost.ChangeView(null, null, 1, true);
            }
            else if (ScrollingHost.ZoomFactor == 1)
            {
                var dispatcher = Window.Current.CoreWindow.Dispatcher;
                await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    ScrollingHost.ChangeView(position.X, position.Y, 4, true);
                });
            }
        }
        private void SaveButtonClick(object sender, RoutedEventArgs e) => StartDownload();

        void StartDownload()
        {
            try
            {
                switch (InboxItem.VisualMedia.Media.MediaType)
                {
                    case InstaMediaType.Image:
                        {
                            var url = InboxItem.VisualMedia.Media.Images?[0].Uri;
                            DownloadHelper.Download(url, url, false);
                        }
                        break;
                    case InstaMediaType.Video:
                        {
                            var url = InboxItem.VisualMedia.Media.Videos?[0].Uri;
                            DownloadHelper.Download(url, InboxItem.VisualMedia.Media.Images?[0].Uri, true);
                        }
                        break;
                }
            }
            catch { }
        }
    }
}
