using InstagramApiSharp.Classes.Models;
using Minista.UserControls.Direct;
using Minista.ViewModels.Direct;
using Minista.Views.Direct;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Minista.ContentDialogs
{
    public sealed partial class InboxThreadsDialog : ContentPopup
    {
        public ObservableCollection<DirectInboxUc> Items { get; set; } = new ObservableCollection<DirectInboxUc>();
        private readonly StorageFile File;
        public InboxThreadsDialog(StorageFile file)
        {
            File = file;
            this.InitializeComponent();
            DataContext = this;
            Loaded += InboxThreadsDialogLoaded;
        }

        private void InboxThreadsDialogLoaded(object sender, RoutedEventArgs e) => SetThumbnail();

        private void CancelButtonClick(object sender, RoutedEventArgs e) => Hide();
        private void ItemsLVLoaded(object sender, RoutedEventArgs e)
        {
            var inbox = InboxViewModel.Instance?.Items;
            if (inbox?.Count > 0)
                for (int i = 0; i < inbox.Count; i++)
                    Items.Add(GetDirectInboxUc(inbox[i].Thread));
        }
        private void ItemsLVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem != null && e.ClickedItem is DirectInboxUc directInboxUc && directInboxUc != null)
                {
                    Hide();

                    Helpers.NavigationService.Navigate(typeof(ThreadView), new object[] { directInboxUc.Thread, File });
                }
            }
            catch { }
        }
        async void SetThumbnail()
        {
            try
            {
                using (IRandomAccessStream fileStream = await File.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);

                    ThumbnailImage.Source = bitmapImage;
                }
            }
            catch { }
        }
        readonly Random Rnd = new Random();

        DirectInboxUc GetDirectInboxUc(InstaDirectInboxThread thread)
        {
            var uc = new DirectInboxUc
            {
                Name = (Rnd.Next(11111, 999999) + Rnd.Next(10000, 999999)).ToString(),
                Thread = thread,
                IsRightTapEnabled = true,
                IsHoldingEnabled = true
            };
            return uc;
        }
    }
}
