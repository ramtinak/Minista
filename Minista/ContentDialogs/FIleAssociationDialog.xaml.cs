using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Minista.Views.Posts;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Minista.ContentDialogs
{
    public sealed partial class FileAssociationDialog : ContentPopup
    {
        private readonly StorageFile File;
        public FileAssociationDialog(StorageFile file) 
        {
            File = file;
            InitializeComponent();

            SetThumbnail();
        }

        private void CancelButtonClick  (object sender, RoutedEventArgs e) => Hide();
        private void PostButtonClick    (object sender, RoutedEventArgs e) => Navigate();
        private void StoryButtonClick   (object sender, RoutedEventArgs e) => Navigate(true);

        void Navigate(bool isStory = false)
        {
            Hide();
            if (isStory)
                Helpers.NavigationService.Navigate(typeof(UploadStoryView), File);
            else
                Helpers.NavigationService.Navigate(typeof(UploadPostView), File);
        }
        async void SetThumbnail()
        {
            try
            {
                if (File.Path.IsSupportedImage())
                {
                    DirectButton.Visibility = Visibility.Visible;
                    DirectColumn.Width = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    DirectButton.Visibility = Visibility.Collapsed;
                    DirectColumn.Width = new GridLength(1, GridUnitType.Auto);
                }
                using (IRandomAccessStream fileStream = await File.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);

                    ThumbnailImage.Source = bitmapImage;
                }
            }
            catch { }
        }

        private async void DirectButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
            await new InboxThreadsDialog(File).ShowAsync();
        }
    }
}
