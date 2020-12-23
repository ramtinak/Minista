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
namespace Minista.Views.Posts
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadStoryView : Page
    {
        private StorageFile FileToUpload;
        private const double DefaultAspectRatio = 0.562d;
        public UploadStoryView()
        {
            this.InitializeComponent();
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
                var editNeeded = false;
                double width = 0, height = 0;
                var decoder = await BitmapDecoder.CreateAsync(await file.OpenReadAsync());
                width = decoder.PixelWidth;
                height = decoder.PixelHeight;
                var wRatio = AspectRatioHelper.GetAspectRatioForMedia(width, height);
                var hRatio = AspectRatioHelper.GetAspectRatioForMedia(height, width);
                //if (wRatio > 1.91 && wRatio < 0.8)
                //    editNeeded = true;
                //else
                //{
                //    if (hRatio > 1.91 && hRatio < 0.8)
                //        editNeeded = true;
                //}
                //if (height > width)
                editNeeded = true;

                if (!editNeeded)
                {
                    CropGrid.Visibility = Visibility.Visible;
                    ImageView.Visibility = Visibility.Collapsed;
                    UploadButton.IsEnabled = false;

                    AspectRatioSlider.Value = wRatio;
                    ImageCropper.AspectRatio = wRatio;
                    ImageCropper.CropShape = CropShape.Rectangular;
                    await ImageCropper.LoadImageFromFile(file);
                }
                else
                {
                    CropGrid.Visibility = Visibility.Visible;
                    ImageView.Visibility = Visibility.Collapsed;
                    UploadButton.IsEnabled = false;
                    //Helper.ShowNotify("Your photo is not in a acceptable aspect ratio." +
                    //    "\r\nYou need to crop it and then you can upload it.", 4500);


                    AspectRatioSlider.Value = DefaultAspectRatio;
                    ImageCropper.AspectRatio = DefaultAspectRatio;
                    ImageCropper.CropShape = CropShape.Rectangular;
                    await ImageCropper.LoadImageFromFile(file);
                }
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
            //var result = await Helper.InstaApi.StoryProcessor.UploadStoryPhotoAsync(img, "");

            uploader.UploadSinglePhoto(FileToUpload, "CaptionText.Text");
            MainPage.Current?.ShowMediaUploadingUc();
            if (NavigationService.Frame.CanGoBack)
                NavigationService.GoBack();
        }
        private void AspectRatioSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            try
            {
                if (AspectRatioSlider.Value != -1)
                    ImageCropper.AspectRatio = AspectRatioSlider.Value;
            }
            catch { }
        }

        private async void CropButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Helper.CreateCachedFolder();
                Helper.CreateCachedFolder();
                Helper.CreateCachedFolder();
                var cacheFolder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
                var file = await cacheFolder.CreateFileAsync(15.GenerateRandomStringStatic() + ".jpg");
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None))
                    await ImageCropper.SaveAsync(fileStream, BitmapFileFormat.Jpeg);
                FileToUpload = await new PhotoHelper().SaveToImage(file, false);

                ShowImagePreview(FileToUpload);
                UploadButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ex.PrintException("CropButtonClick");
            }
        }
    }
}
