using Minista.ContentDialogs;
using Minista.UserControls.Direct;
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

namespace Minista.Views.Direct
{
    public sealed partial class DirectRequestsView : Page
    {
        public static DirectRequestsView Current;
        NavigationMode NavigationMode;
        public DirectRequestsView()
        {
            this.InitializeComponent();
            Current = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationMode = e.NavigationMode;

            KeyDown += OnKeyDownHandler;
            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            KeyDown -= OnKeyDownHandler;
        }
        private void OnKeyDownHandler(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.F5)
                    DirectRequestsVM.RunLoadMore(true);
            }
            catch { }
        }

        private void ItemsLVLoaded(object sender, RoutedEventArgs e)
        {
            if(NavigationMode == NavigationMode.New)
            {
                NavigationCacheMode = NavigationCacheMode.Enabled;
                ResetPanels();
                DirectRequestsVM.RunLoadMore(true);
                DirectRequestsVM?.SetLV(ItemsLV.FindScrollViewer());
            }
        }
        private void SelectionToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectionToggleButton.IsChecked = false;
            }
            catch { }
            ItemsLV.SelectionMode = ListViewSelectionMode.Extended;
            SelectionToggleButton.Visibility = Visibility.Collapsed;
            ExitSelectionToggleButton.Visibility = Visibility.Visible;
        }

        private void ExitSelectionToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ExitSelectionToggleButton.IsChecked = false;
            }
            catch { }
            ResetPanels();
        }

        private async void DeleteAllRequestsToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteAllRequestsToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                await new DirectDeleteAllMessageRequestsDialog().ShowAsync();
            }
            catch { }
        }


        #region LOADINGS
        public void ShowTopLoading() => TopLoading.Start();
        public void HideTopLoading() => TopLoading.Stop();


        public void ShowBottomLoading() => BottomLoading.Start();
        public void HideBottomLoading() => BottomLoading.Stop();
        #endregion LOADINGS

        public void ResetPanels()
        {
            try
            {
                DeleteAllRequestsToggleButton.Visibility = Visibility.Visible;
            }
            catch { }
            try
            {
                SingleGrid.Visibility = Visibility.Collapsed;
            }
            catch { }
            try
            {
                MultipleGrid.Visibility = Visibility.Collapsed;
            }
            catch { }
            try
            {
                ItemsLV.SelectionMode = ListViewSelectionMode.None;
            }
            catch { }

            try
            {
                ItemsLV.SelectionMode = ListViewSelectionMode.None;
                SelectionToggleButton.Visibility = Visibility.Visible;
                ExitSelectionToggleButton.Visibility = Visibility.Collapsed;
            }
            catch { }
        }
        void ShowMultiplePanel()
        {
            try
            {
                DeleteAllRequestsToggleButton.Visibility = Visibility.Collapsed;
            }
            catch { }
            try
            {
                SingleGrid.Visibility = Visibility.Collapsed;
            }
            catch { }
            try
            {
                MultipleGrid.Visibility = Visibility.Visible;
            }
            catch { }
        }
        void ShowSinglePanel()
        {
            try
            {
                DeleteAllRequestsToggleButton.Visibility = Visibility.Collapsed;
            }
            catch { }
            try
            {
                SingleGrid.Visibility = Visibility.Visible;
            }
            catch { }
            try
            {
                MultipleGrid.Visibility = Visibility.Collapsed;
            }
            catch { }
        }
        #region SINGLE
        private async void BlockSingleToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                BlockSingleToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        var uc = ItemsLV.SelectedItems[0] as DirectInboxUc;
                        if (uc != null)
                        {
                            var user = uc.Thread.Users.FirstOrDefault();
                            var result = await Helper.InstaApi.UserProcessor.BlockUserAsync(user.Pk);
                            if (result.Succeeded)
                            {
                                await Helper.InstaApi.MessagingProcessor.DeclineDirectPendingRequestsAsync(uc.Thread.ThreadId);
                                Helper.ShowNotify($"@{user.UserName.ToLower()} blocked...", 3000);
                                DirectRequestsVM.Items.Remove(uc);
                                ResetPanels();
                            }
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 3000);
                                else
                                    Helper.ShowNotify($"Something unexpected happened: {result.Info?.Message}", 3000);
                            }
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }

        private async  void DeleteSingleToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteSingleToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        var uc = ItemsLV.SelectedItems[0] as DirectInboxUc;
                        if (uc != null)
                        {
                            var result = await Helper.InstaApi.MessagingProcessor.DeclineDirectPendingRequestsAsync(uc.Thread.ThreadId);
                            if (result.Succeeded)
                            {
                                Helper.ShowNotify($"'{uc.Thread.Title}' deleted...", 3000);
                                DirectRequestsVM.Items.Remove(uc);
                                ResetPanels();
                            }
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 3000);
                                else
                                    Helper.ShowNotify($"Something unexpected happened: {result.Info?.Message}", 3000);
                            }
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }

        private async void AcceptSingleToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AcceptSingleToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        var uc = ItemsLV.SelectedItems[0] as DirectInboxUc;
                        if (uc != null)
                        {
                            var result = await Helper.InstaApi.MessagingProcessor.ApproveDirectPendingRequestAsync(uc.Thread.ThreadId);
                            if (result.Succeeded)
                            {
                                Helper.ShowNotify($"'{uc.Thread.Title}' approved...", 3000);
                                DirectRequestsVM.Items.Remove(uc);
                                ResetPanels();
                            }
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 3000);
                                else
                                    Helper.ShowNotify($"Something unexpected happened: {result.Info?.Message}", 3000);
                            }
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }
        #endregion SINGLE
        #region MULTIPLE
        private async void DeleteMultipleToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteMultipleToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                if (DirectRequestsVM.Items.Count == 0)
                {
                    Helpers.NavigationService.GoBack();
                    return;
                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        var uc = ItemsLV.SelectedItems[0] as DirectInboxUc;
                        if (uc != null)
                        {
                            var threadIds = new List<string>();
                            var ucList = new List<DirectInboxUc>();
                            for (int i = 0; i < ItemsLV.SelectedItems.Count; i++)
                            {
                                var item = ItemsLV.SelectedItems[i] as DirectInboxUc;
                                if (item != null)
                                {
                                    ucList.Add(item);
                                    threadIds.Add(item.Thread.ThreadId);
                                }
                            }
                            var result = await Helper.InstaApi.MessagingProcessor.DeclineDirectPendingRequestsAsync(threadIds.ToArray());
                            if (result.Succeeded)
                            {
                                Helper.ShowNotify($"'{threadIds.Count}' thread(s) deleted...", 3000);
                                for (int i = 0; i < ucList.Count; i++)
                                    DirectRequestsVM.Items.Remove(ucList[i]);
                                ResetPanels();
                            }
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 3000);
                                else
                                    Helper.ShowNotify($"Something unexpected happened: {result.Info?.Message}", 3000);
                            }
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }
        private async void AcceptMultipleToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AcceptMultipleToggleButton.IsChecked = false;
            }
            catch { }
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        var uc = ItemsLV.SelectedItems[0] as DirectInboxUc;
                        if (uc != null)
                        {
                            var threadIds = new List<string>();
                            var ucList = new List<DirectInboxUc>();
                            for (int i = 0; i < ItemsLV.SelectedItems.Count; i++)
                            {
                                var item = ItemsLV.SelectedItems[i] as DirectInboxUc;
                                if (item != null)
                                {
                                    ucList.Add(item);
                                    threadIds.Add(item.Thread.ThreadId);
                                }
                            }
                            var result = await Helper.InstaApi.MessagingProcessor.ApproveDirectPendingRequestAsync(threadIds.ToArray());
                            if (result.Succeeded)
                            {
                                Helper.ShowNotify($"'{threadIds.Count}' thread(s) approved...", 3000);
                                for (int i = 0; i < ucList.Count; i++)
                                    DirectRequestsVM.Items.Remove(ucList[i]);
                                ResetPanels();
                            }
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 3000);
                                else
                                    Helper.ShowNotify($"Something unexpected happened: {result.Info?.Message}", 3000);
                            }
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }
        #endregion MULTIPLE
        private void ItemsLVSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                (ItemsLV.SelectedIndex + "    " + ItemsLV.SelectedItems.Count).PrintDebug();
                if (ItemsLV.SelectedIndex == -1)
                {

                }
                else
                {
                    if (ItemsLV.SelectionMode == ListViewSelectionMode.Extended)
                    {
                        if(ItemsLV.SelectedItems.Count<=1)
                        {
                            ShowSinglePanel();
                        }
                        else
                        {
                            try
                            {
                                DeleteMultipleToggleButton.Content = $"Delete ({ItemsLV.SelectedItems.Count})";
                            }
                            catch { }
                            try
                            {
                                AcceptMultipleToggleButton.Content = $"Accept ({ItemsLV.SelectedItems.Count})";
                            }
                            catch { }
                            ShowMultiplePanel();

                        }
                    }
                }
            }
            catch { }
        }

        private void ItemsLVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (ItemsLV.SelectionMode == ListViewSelectionMode.None && e.ClickedItem is DirectInboxUc uc && uc != null)
                    Helpers.NavigationService.Navigate(typeof(DirectRequestsThreadView), uc.Thread);
            }
            catch { }
        }
    }
}
