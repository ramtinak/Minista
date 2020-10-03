using Minista.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Minista.ContentDialogs
{
    public sealed partial class DirectDeleteAllMessageRequestsDialog : ContentPopup
    {
        public DirectDeleteAllMessageRequestsDialog() => InitializeComponent();
        
        private async void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        var result = await Helper.InstaApi.MessagingProcessor.DeclineAllDirectPendingRequestsAsync();
                        if (result.Succeeded)
                        {
                            ClearItems();
                            Hide();
                        }
                        else
                        {
                            if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 3000);
                            else
                                Helper.ShowNotify($"Something unexpected happened: {result.Info?.Message}", 3000);

                            Hide();
                        }
                    }
                    catch { Hide(); }
                });
            }
            catch { Hide(); }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Hide();
            }
            catch { }
        }
        public static void ClearItems()
        {
            try
            {
                if (NavigationService.Frame.Content is Views.Direct.DirectRequestsView view && view != null)
                {
                    view.DirectRequestsVM.Items.Clear();
                    Views.Direct.InboxView.Current?.InboxVM.RunLoadMore(true);
                    NavigationService.GoBack();
                }
            }
            catch { }
        }
    }
}
