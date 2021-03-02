//using Lumia.Imaging;
using Minista.Helpers;
using Minista.UI.Controls;
using Minista.Views.MediaConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Microsoft.Graphics.Canvas;
using Windows.Storage.Streams;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Core;

namespace Minista.Views.Posts
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadPostView : Page
    {
        private StorageFile FileToUpload, ThumbnailFile;
        private const double DefaultAspectRatio = 1.6201d;
        public UploadPostView()
        {
            this.InitializeComponent();
            Loaded += OnPageLoaded;
            Unloaded += UploadPostView_Unloaded;
            Window.Current.SizeChanged += CurrentSizeChanged;
        }

        private void UploadPostView_Unloaded(object sender, RoutedEventArgs e)
        {
            Editor.Completed -= Editor_Completed;
            Window.Current.SizeChanged -= CurrentSizeChanged;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            SetCanvas();
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
                openPicker.FileTypeFilter.Add(".mp4");
                openPicker.FileTypeFilter.Add(".mkv");
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

        bool IsVideo = false;
        async void ImportFile(StorageFile file)
        {
            try
            {
                try
                {
                    CropGrid.Visibility = Visibility.Collapsed;
                }
                catch { }
                if (Path.GetExtension(file.Path).ToLower() == ".mp4" || Path.GetExtension(file.Path).ToLower() == ".mkv")
                {
                    IsVideo = true;
                    FileToUpload = file;
                    ThumbnailFile = await file.GetSnapshotFromD3D(TimeSpan.Zero, true);
                    ThumbnailFile = await new PhotoHelper().SaveToImage(ThumbnailFile, false);
                    CropGrid.Opacity = 1;
                    CropGrid.Visibility = Visibility.Visible;
                    await Editor.SetFileAsync(ThumbnailFile);
                    MainCanvas.Visibility = Visibility.Collapsed;
                    UploadButton.IsEnabled = false;

                    Editor.AspectRatio = DefaultAspectRatio;
                }
                else
                {
                    IsVideo = false;
                    ThumbnailFile = null;
                    CropGrid.Opacity = 1;
                    CropGrid.Visibility = Visibility.Visible;
                    MainCanvas.Visibility = Visibility.Collapsed;
                    UploadButton.IsEnabled = false;
                    Editor.AspectRatio = DefaultAspectRatio;
                    await Editor.SetFileAsync(file);

                }
            }
            catch { }
        }
        void ShowImagePreview(StorageFile file)
        {
            try
            {
                Show(file);
                MainCanvas.Visibility =Visibility.Visible;
                CropGrid.Visibility = Visibility.Collapsed;

            }
            catch { }
        }
        private void UploadButtonClick(object sender, RoutedEventArgs e)
        {
            if (Path.GetExtension(FileToUpload.Path).ToLower() == ".mp4")
            {
                var uploader = new VideoUploader();
                Helper.ShowNotify("We will notify you once your video uploaded...", 3000);
                uploader.UploadVideo(FileToUpload, ThumbnailFile, CaptionText.Text, Editor.ScaledCropRect);
            }
            else
            {
                var uploader = new PhotoUploaderHelper();
                Helper.ShowNotify("We will notify you once your photo uploaded...", 3000);
                uploader.UploadSinglePhoto(FileToUpload, CaptionText.Text, UserTags);
                MainPage.Current?.ShowMediaUploadingUc();
                if (NavigationService.Frame.CanGoBack)
                    NavigationService.GoBack();
            }
        }
        public List<InstaUserTagUpload> UserTags { get; set; } = new List<InstaUserTagUpload>();

        private async void Editor_Completed(object sender, EventArgs e)
        {
            try
            {
                if (IsVideo)
                {
                    var cacheFolder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
                    var file = await cacheFolder.CreateFileAsync(15.GenerateRandomStringStatic() + ".jpg");

                    await Editor.SaveToFileAsync(file);
                    ThumbnailFile = await new PhotoHelper().SaveToImage(file, false);
                    ShowImagePreview(ThumbnailFile);
                    UploadButton.IsEnabled = true;
                }
                else
                {
                    var cacheFolder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
                    var file = await cacheFolder.CreateFileAsync(15.GenerateRandomStringStatic() + ".jpg");

                    await Editor.SaveToFileAsync(file);
                    FileToUpload = await new PhotoHelper().SaveToImage(file, false, false);
                    ShowImagePreview(FileToUpload);
                    UploadButton.IsEnabled = true;
                }
            }
            catch(Exception ex)
            {
                ex.PrintException("Editor_Completed");
            }
        }

        #region Canvas

        private void CurrentSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            SetCanvas();
            MainCanvas.Invalidate();
        }
        private CanvasBitmap _image; 
        private Stretch _stretch = Stretch.Uniform;
        public async void Show(StorageFile image)
        {
            try
            {
                Show();
                CanvasDevice cd = CanvasDevice.GetSharedDevice();
                var stream = await image.OpenAsync(FileAccessMode.Read);
                _image = await CanvasBitmap.LoadAsync(cd, stream);
                MainCanvas.Invalidate();
            }
            catch { }
        }
        private void Show()
        {
            SetCanvas();
        }
        private void MainCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            var target = GetDrawings(true); 
            if (target != null)
                args.DrawingSession.DrawImage(target);
        }
        private void SetCanvas()
        {
            try
            {
                var w = MainWorkSapce.ActualWidth - 40;
                var h = MainWorkSapce.ActualHeight - 40;

                MainCanvas.Width = w;
                MainCanvas.Height = h;
                MainCanvas.Invalidate();
            }
            catch { }
        }
        private async void GenerateResultImage()
        {
            var img = GetDrawings(false);
            if (img != null)
            {
                IRandomAccessStream stream = new InMemoryRandomAccessStream();
                await img.SaveAsync(stream, CanvasBitmapFileFormat.Jpeg);
                BitmapImage result = new BitmapImage();
                stream.Seek(0);
                await result.SetSourceAsync(stream);
            }
        }
        private Rect GetImageDrawingRect()
        {
            Rect des;

            var image_w = _image.Size.Width;
            var image_h = _image.Size.Height;

            if (_stretch == Stretch.Uniform)
            {
                var w = MainCanvas.Width - 20;
                var h = MainCanvas.Height - 20;
                if (image_w / image_h > w / h)
                {
                    var left = 10;

                    var width = w;
                    var height = (image_h / image_w) * width;

                    var top = (h - height) / 2 + 10;

                    des = new Rect(left, top, width, height);
                }
                else
                {
                    var top = 10;
                    var height = h;
                    var width = (image_w / image_h) * height;
                    var left = (w - width) / 2 + 10;
                    des = new Rect(left, top, width, height);
                }
            }
            else
            {
                var w = MainCanvas.Width;
                var h = MainCanvas.Height;
                var left = 0;
                var top = 0;
                if (image_w / image_h > w / h)
                {
                    var height = h;
                    var width = (image_w / image_h) * height;
                    des = new Rect(left, top, width, height);
                }
                else
                {
                    var width = w;
                    var height = (image_h / image_w) * width;

                    des = new Rect(left, top, width, height);
                }
            }
            return des;
        }
        private CanvasRenderTarget GetDrawings(bool edit)
        {
            double w, h;
            if (edit)
            {
                w = MainCanvas.ActualWidth;
                h = MainCanvas.ActualHeight;
            }
            else 
            {
                Rect des = GetImageDrawingRect();

                w = (_image.Size.Width / des.Width) * MainCanvas.Width;
                h = (_image.Size.Height / des.Height) * MainCanvas.Height;
            }
            var scale = edit ? 1 : w / MainCanvas.Width; 

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget target = new CanvasRenderTarget(device, (float)w, (float)h, 96);
            using (CanvasDrawingSession graphics = target.CreateDrawingSession())
                DrawBackImage(graphics, scale);

            return target;
        }
        private void DrawBackImage(CanvasDrawingSession graphics, double scale)
        {
            if (_image != null)
            {
                Rect des = GetImageDrawingRect();
                des.X *= scale;
                des.Y *= scale;
                des.Width *= scale;
                des.Height *= scale;

                ICanvasImage image = GetBrightnessEffect(_image);
                image = ApplyFilterTemplate(image);

                graphics.DrawImage(image, des, _image.Bounds);
            }
        }
        private ICanvasImage GetBrightnessEffect(ICanvasImage source)
        {
            var t = 50 / 500 * 2;
            var exposureEffect = new ExposureEffect
            {
                Source = source,
                Exposure = (float)t
            };

            return exposureEffect;
        }

        private ICanvasImage ApplyFilterTemplate(ICanvasImage source) => source;

        #endregion
    }
}
