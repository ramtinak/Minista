using InstagramApiSharp.Classes.Models;
using Minista.Helpers;
using Minista.ViewModels.Direct;
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
    public sealed partial class DirectLeaveChatDialog : ContentPopup
    {
        readonly InstaDirectInboxThread Thread;
        public DirectLeaveChatDialog(InstaDirectInboxThread thread)
        {
            this.InitializeComponent();
            Thread = thread;
            txtTitle.Text = thread.Title;
            UserImage.Fill = thread.Users[0].ProfilePicture.GetImageBrush();
        }

        private async void LeaveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        var result = await Helper.InstaApi.MessagingProcessor.LeaveGroupThreadAsync(Thread.ThreadId);
                        if (result.Succeeded)
                        {
                            ClearThread();
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
        void ClearThread()
        {
            try
            {
                return;
                //if (Helpers.NavigationService.Frame.Content is Views.Direct.InboxView inbox)
                //    for (int i = 0; i < inbox.InboxVM.Items.Count; i++)
                //    {
                //        var item = inbox.InboxVM.Items[i];
                //        if (item != null && item.Thread.ThreadId == Thread.ThreadId)
                //        {
                //            inbox.InboxVM.Items.RemoveAt(i);
                //            break;
                //        }
                //    }
            }
            catch { }
        }
    }
}
