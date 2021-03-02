using Minista.Helpers;
using Minista.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using System.Threading.Tasks;

namespace Minista.Views.Posts
{
    public sealed partial class UploadStoryView : Page
    {
        private StorageFile FileToUpload;
        private const double DefaultAspectRatio = 0.62d/*0.562d*/;
        public UploadStoryView()
        {
            this.InitializeComponent();
            Loaded += UploadStoryView_Loaded;
        }

        private async void UploadStoryView_Loaded(object sender, RoutedEventArgs e)
        {
            Editor.Completed += Editor_Completed;
            await Helper.RunOnUI(async () =>
            {
                try
                {
                    CropGrid.Visibility = Visibility.Visible;
                    await Task.Delay(2000);
                    CropGrid.Visibility = Visibility.Collapsed;
                }
                catch { }
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();
            try
            {
                if (e.Parameter != null && e.Parameter is StorageFile file && file != null)
                    ImportFile(file);
            }
            catch { }
        }

        private async void ImportButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Helper.DontUseTimersAndOtherStuff = true;
                FileOpenPicker openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".bmp");
                openPicker.FileTypeFilter.Add(".png");
                var file = await openPicker.PickSingleFileAsync();
                Helper.DontUseTimersAndOtherStuff = false;
                if (file == null) return;
                ImportFile(file);
            }
            catch
            {
                Helper.DontUseTimersAndOtherStuff = false;
            }
        }
        async void ImportFile(StorageFile file)
        {
            try
            {
                CropGrid.Opacity = 1;
                CropGrid.Visibility = Visibility.Visible;
                UploadButton.IsEnabled = false;
                Editor.LargeChangeRatio = 0.05;
                Editor.MaximumRatio = 0.9;
                Editor.MinimumRatio = 0.50;
                Editor.SmallChangeRatio = 0.05;
                Editor.StepFrequencyRatio = 0.05;
                Editor.AspectRatio = DefaultAspectRatio;
                var editedFile = await new PhotoHelper().SaveToImage(file, false, false);
                await Editor.SetFileAsync(editedFile);
            }
            catch { }
        }
        async void ShowImagePreview(StorageFile file)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.SetSource((await file.OpenStreamForReadAsync()).AsRandomAccessStream());

                ImageView.Source = bitmap;
                ImageView.Visibility = Visibility.Visible;
                CropGrid.Visibility = Visibility.Collapsed;
            }
            catch { }
        }
        private async void UploadButtonClick(object sender, RoutedEventArgs e)
        {
            var uploader = new StoryPhotoUploaderHelper();
            Helper.ShowNotify("We will notify you once your photo story uploaded...", 3000);
            var fileBytes = (await FileIO.ReadBufferAsync(FileToUpload)).ToArray();
            var img = new InstaImage
            {
                ImageBytes = fileBytes
            };
            uploader.UploadSinglePhoto(FileToUpload, "");
            MainPage.Current?.ShowMediaUploadingUc();
            if (NavigationService.Frame.CanGoBack)
                NavigationService.GoBack();
        }
        private async void Editor_Completed(object sender, EventArgs e)
        {
            try
            {
                var cacheFolder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
                var file = await cacheFolder.CreateFileAsync(15.GenerateRandomStringStatic() + ".jpg");

                await Editor.SaveToFileAsync(file);
                FileToUpload = await new PhotoHelper().SaveToImage(file, false, false);
                ShowImagePreview(FileToUpload);
                UploadButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ex.PrintException("Editor_Completed");
            }
        }
    }
}
