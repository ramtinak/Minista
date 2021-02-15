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
        private const double DefaultAspectRatio = 1.6200d;
        public UploadPostView()
        {
            this.InitializeComponent();
            Loaded += OnPageLoaded;
            Unloaded += UploadPostView_Unloaded;
            Window.Current.SizeChanged += CurrentSizeChanged;
        }

        private void UploadPostView_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= CurrentSizeChanged;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            SetCanvas();
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
                //openPicker.FileTypeFilter.Add(".gif");
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

        BitmapDecoder VideoBitmapDecoder;
        Rect CurrentCroppedRectForVideo;
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

                    double width = 0, height = 0;
                    var decoder = await BitmapDecoder.CreateAsync(await ThumbnailFile.OpenReadAsync());
                    width = decoder.PixelWidth;
                    height = decoder.PixelHeight;
                    var wRatio = AspectRatioHelper.GetAspectRatioForMedia(width, height);
                    var hRatio = AspectRatioHelper.GetAspectRatioForMedia(height, width);



                    MainCanvas.Visibility = /*ImageView.Visibility =*/  Visibility.Collapsed;
                    UploadButton.IsEnabled = false;

                    AspectRatioSlider.Value = wRatio;
                    ImageCropper.AspectRatio = wRatio;
                    ImageCropper.CropShape = CropShape.Rectangular;
                    await ImageCropper.LoadImageFromFile(ThumbnailFile);
                    AspectRatioSlider.Value = DefaultAspectRatio;
                    ImageCropper.AspectRatio = DefaultAspectRatio;
                    //await Task.Delay(2500);
                    //using (var fileStream = await ThumbnailFile.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None))
                    //    await ImageCropper.SaveAsync(fileStream, BitmapFileFormat.Jpeg);
                    //ThumbnailFile = await new PhotoHelper().SaveToImage(ThumbnailFile, false);
                    //decoder = await BitmapDecoder.CreateAsync(await ThumbnailFile.OpenReadAsync());
                    //VideoBitmapDecoder = decoder;
                    //CurrentCroppedRectForVideo = ImageCropper.CurrentCroppedRect;
                    //ShowImagePreview(ThumbnailFile);
                    //CropGrid.Visibility = Visibility.Collapsed;
                    //MainCanvas.Visibility = /*ImageView.Visibility =*/  Visibility.Visible;
                    //UploadButton.IsEnabled = true;

                    var editNeeded = false;

                    if (wRatio > 1.91 && wRatio < 0.8)
                        editNeeded = true;
                    else
                    {
                        if (hRatio > 1.91 && hRatio < 0.8)
                            editNeeded = true;
                    }
                    if (height > width)
                        editNeeded = true;

                    if (!editNeeded)
                    {
                        FileToUpload = await new PhotoHelper().SaveToImage(file, false);

                        CropGrid.Visibility = Visibility.Visible;

                        CropGrid.Opacity = 1;
                        MainCanvas.Visibility = /*ImageView.Visibility =*/ Visibility.Collapsed;
                        UploadButton.IsEnabled = false;

                        AspectRatioSlider.Value = wRatio;
                        ImageCropper.AspectRatio = wRatio;
                        ImageCropper.CropShape = CropShape.Rectangular;
                        await ImageCropper.LoadImageFromFile(file);
                    }
                    else
                    {
                        CropGrid.Opacity = 1;
                        CropGrid.Visibility = Visibility.Visible;
                        MainCanvas.Visibility = /*ImageView.Visibility =*/ Visibility.Collapsed;
                        UploadButton.IsEnabled = false;

                        AspectRatioSlider.Value = DefaultAspectRatio;
                        ImageCropper.AspectRatio = DefaultAspectRatio;
                        ImageCropper.CropShape = CropShape.Rectangular;
                        await ImageCropper.LoadImageFromFile(file);
                    }
                }
                else
                {
                    IsVideo = false;
                    ThumbnailFile = null;
                    VideoBitmapDecoder = null;
                    var editNeeded = false;
                    double width = 0, height = 0;
                    var decoder = await BitmapDecoder.CreateAsync(await file.OpenReadAsync());
                    width = decoder.PixelWidth;
                    height = decoder.PixelHeight;
                    var wRatio = AspectRatioHelper.GetAspectRatioForMedia(width, height);
                    var hRatio = AspectRatioHelper.GetAspectRatioForMedia(height, width);
                    if (wRatio > 1.91 && wRatio < 0.8)
                        editNeeded = true;
                    else
                    {
                        if (hRatio > 1.91 && hRatio < 0.8)
                            editNeeded = true;
                    }
                    if (height > width)
                        editNeeded = true;

                    if (!editNeeded)
                    {
                        //CropGrid.Visibility = Visibility.Collapsed;

                        //ImageView.Visibility = Visibility.Visible;
                        //UploadButton.IsEnabled = true;
                        //ShowImagePreview(file);

                        //in paeini comment bod
                        FileToUpload = await new PhotoHelper().SaveToImage(file, false);




                        CropGrid.Visibility = Visibility.Visible;

                        CropGrid.Opacity = 1;
                        MainCanvas.Visibility = /*ImageView.Visibility =*/ Visibility.Collapsed;
                        UploadButton.IsEnabled = false;

                        AspectRatioSlider.Value = wRatio;
                        ImageCropper.AspectRatio = wRatio;
                        ImageCropper.CropShape = CropShape.Rectangular;
                        await ImageCropper.LoadImageFromFile(file);
                    }
                    else
                    {
                        CropGrid.Opacity = 1;
                        CropGrid.Visibility = Visibility.Visible;
                        MainCanvas.Visibility = /*ImageView.Visibility =*/ Visibility.Collapsed;
                        UploadButton.IsEnabled = false;
                        //Helper.ShowNotify("Your photo is not in a acceptable aspect ratio." +
                        //    "\r\nYou need to crop it and then you can upload it.", 4500);


                        AspectRatioSlider.Value = DefaultAspectRatio;
                        ImageCropper.AspectRatio = DefaultAspectRatio;
                        ImageCropper.CropShape = CropShape.Rectangular;
                        await ImageCropper.LoadImageFromFile(file);
                    }
                    //using (var source = new StorageFileImageSource(file))
                    //using (var renderer = new JpegRenderer(source, JpegOutputColorMode.Yuv420, OutputOption.PreserveAspectRatio))
                    //{
                    //    var info = await source.GetInfoAsync();
                    //    width = info.ImageSize.Width;
                    //    height = info.ImageSize.Height;
                    //}
                }
            }
            catch { }
        }
        /*async*/ void ShowImagePreview(StorageFile file)
        {
            try
            {
                Show(file);
                //var bitmap = new BitmapImage();
                //bitmap.SetSource((await file.OpenStreamForReadAsync()).AsRandomAccessStream());

                //ImageView.Source = bitmap;
                MainCanvas.Visibility = /*ImageView.Visibility =*/ Visibility.Visible;
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
                uploader.UploadVideo(FileToUpload, ThumbnailFile, CaptionText.Text, VideoBitmapDecoder, CurrentCroppedRectForVideo);
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
            //using (var photo = new PhotoHelper())
            //{
            //    var fileToUpload = await photo.SaveToImageForPost(files[0]);
            //    Random rnd = new Random();
            //    Uploader.UploadSinglePhoto(fileToUpload, "TEEEEEEEEEEST\r\n\r\n\r\n" + DateTime.Now.ToString());
            //}
        }
        private void AspectRatioSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            try
            {
                if (AspectRatioSlider.Value != -1 && CropGrid.Visibility == Visibility.Visible)
                    ImageCropper.AspectRatio = AspectRatioSlider.Value;
            }
            catch { }
        }
        public List<InstaUserTagUpload> UserTags { get; set; } = new List<InstaUserTagUpload>();

        private void ImageViewTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                //var p = e.GetPosition(ImageView);
                //var x = p.X;
                //var y = p.Y;
                //var hh = ImageView.ActualHeight;
                //var ww = ImageView.ActualWidth;

                //var w2 = (x / ww);
                //var w3 = (y / hh);
                //UserTags.Clear();
                //UserTags.Add(new InstaUserTagUpload
                //{
                //    Username = "rmtjokar1373",
                //    X = w2,
                //    Y = w3
                //});
            }
            catch { }
        }

        //private double AspectRatio = 1.91d;
        //private void ComboAspectRatioSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        if(ComboAspectRatio.SelectedIndex != -1)
        //        {
        //            switch(ComboAspectRatio.SelectedIndex)
        //            {
        //                case 0:
        //                    AspectRatio = 1.91d;
        //                    ImageCropper.AspectRatio = AspectRatio;
        //                    break;

        //                case 1:
        //                    AspectRatio = 1d;
        //                    ImageCropper.AspectRatio = AspectRatio;
        //                    break;

        //                case 2:
        //                    AspectRatio = 0.8d;
        //                    ImageCropper.AspectRatio = AspectRatio;
        //                    break;

        //            }
        //        }
        //    }
        //    catch { }
        //}

        private async void CropButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsVideo)
                {
                    using (var fileStream = await ThumbnailFile.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None))
                        await ImageCropper.SaveAsync(fileStream, BitmapFileFormat.Jpeg);
                    ThumbnailFile = await new PhotoHelper().SaveToImage(ThumbnailFile, false);
                    var decoder = await BitmapDecoder.CreateAsync(await ThumbnailFile.OpenReadAsync());
                    VideoBitmapDecoder = decoder;
                    CurrentCroppedRectForVideo = ImageCropper.CurrentCroppedRect;
                    ShowImagePreview(ThumbnailFile);
                    UploadButton.IsEnabled = true;
                }
                else
                {
                    var cacheFolder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
                    var file = await cacheFolder.CreateFileAsync(15.GenerateRandomStringStatic() + ".jpg");
                    //await ImageCropper.CroppedImage.SaveAsync(file);
                    using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None))
                        await ImageCropper.SaveAsync(fileStream, BitmapFileFormat.Jpeg);
                    FileToUpload = await new PhotoHelper().SaveToImage(file, false, false);

                    //CropGrid.Visibility = Visibility.Collapsed;
                    ShowImagePreview(FileToUpload);
                    UploadButton.IsEnabled = true;
                }



                //using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None))
                //    await ImageCropper.SaveAsync(fileStream, BitmapFileFormat.Jpeg);
                //ThumbnailFile = await new PhotoHelper().SaveToImage(file, false);

                ////CropGrid.Visibility = Visibility.Collapsed;
                //ShowImagePreview(ThumbnailFile);
                //var converter = new VideoConverter();
                //var decoder = await BitmapDecoder.CreateAsync(await ThumbnailFile.OpenReadAsync());

                //var converted = await converter.ConvertFiles(new List<StorageFile> { FileToUpload }, false, new Size(decoder.PixelWidth, decoder.PixelHeight));
            }
            catch(Exception ex)
            {
                ex.PrintException("CropButtonClick");
            }
        }










        #region Canvas

        private void CurrentSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {

            //var height = ApplicationView.GetForCurrentView().VisibleBounds.Height;
            //var width = ApplicationView.GetForCurrentView().VisibleBounds.Width;

            //Height = height;
            //Width = width;
            //SetCanvas();
            //MainCanvas.Invalidate();
            //await Task.Delay(10);
            SetCanvas();
            MainCanvas.Invalidate();
        }
        private CanvasBitmap _image;  //Base map
        private Stretch _stretch = Stretch.Uniform;
        public async void Show(StorageFile image)
        {
            try
            {
                Show();
                //WaitLoading.IsActive = true;
                CanvasDevice cd = CanvasDevice.GetSharedDevice();
                var stream = await image.OpenAsync(FileAccessMode.Read);
                _image = await CanvasBitmap.LoadAsync(cd, stream);
                //WaitLoading.IsActive = false;
                MainCanvas.Invalidate();
            }
            catch
            {
            }
        }
        private void Show()
        {
            SetCanvas();
        }
        /// <summary>
        /// Canvas drawing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MainCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            var target = GetDrawings(true); 
            if (target != null)
            {
                args.DrawingSession.DrawImage(target);
            }
        }
        /// <summary>
        /// Click on the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var p = e.GetPosition(MainCanvas);
            if (!ClickEmpty(p))
            {

                //if (_cropUI != null)
                //{
                //    if ((_cropUI as CropUI).RightTopRegion.Contains(p))  //OK
                //    {
                //        double w, h;  //画布大小

                //        Rect des = GetImageDrawingRect();

                //        w = (_image.Size.Width / des.Width) * MainCanvas.Width;
                //        h = (_image.Size.Height / des.Height) * MainCanvas.Height;

                //        var scale = w / MainCanvas.Width;  //缩放比例

                //        CanvasDevice device = CanvasDevice.GetSharedDevice();
                //        CanvasRenderTarget target = new CanvasRenderTarget(device, (float)w, (float)h, 96);
                //        using (CanvasDrawingSession graphics = target.CreateDrawingSession())
                //        {

                //            graphics.Transform = Matrix3x2.CreateTranslation((float)(_cropUI as CropUI).Region.X, (float)(_cropUI as CropUI).Region.Y);
                //        }
                //    }
                //}
                return;
            }
        }
        private void MainCanvas_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

        }
        private void MainCanvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {

        }
        private void MainCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

        }        /// </summary>
        private void SetCanvas()
        {
            try
            {

                var w = MainWorkSapce.ActualWidth - 40;  //
                var h = MainWorkSapce.ActualHeight - 40;  //

                MainCanvas.Width = w;
                MainCanvas.Height = h;
                MainCanvas.Invalidate();
            }
            catch { }
        }
        private bool ClickEmpty(Point p)
        {
            return false;
            //if (_cropUI != null)
            //{
            //    if ((_cropUI as CropUI).LeftTopRegion.Contains(p))  //لغو
            //    {
            //        _cropUI = null;
            //        MainCanvas.Invalidate();
            //        return false;
            //    }
            //    if ((_cropUI as CropUI).RightTopRegion.Contains(p))  //OK
            //    {

            //        //
            //        return false;
            //    }
            //    if ((_cropUI as CropUI).Region.Contains(p))  //点击的是 剪切对象 区域
            //    {
            //        return false;
            //    }
            //}
            //if (_wall_paperUI != null)
            //{
            //    if ((_wall_paperUI as WallPaperUI).RightTopRegion.Contains(p)) //取消墙纸
            //    {
            //        _wall_paperUI = null;
            //        MainCanvas.Invalidate();
            //        return false;
            //    }
            //    if ((_wall_paperUI as WallPaperUI).Region.Contains(p)) //点击墙纸
            //    {
            //        (_wall_paperUI as WallPaperUI).Editing = !(_wall_paperUI as WallPaperUI).Editing;
            //        MainCanvas.Invalidate();
            //        return false;
            //    }
            //}
            //if (_tagsUIs != null)
            //{
            //    foreach (var tag in _tagsUIs)
            //    {
            //        if ((tag as TagUI).Region.Contains(p))  //点击的是 tag 区域
            //        {
            //            (tag as TagUI).ShowCloseBtn = !(tag as TagUI).ShowCloseBtn;
            //            MainCanvas.Invalidate();

            //            return false;
            //        }

            //        if ((tag as TagUI).CloseRegion.Contains(p) && (tag as TagUI).ShowCloseBtn) //点击的是tag 关闭按钮
            //        {
            //            _tagsUIs.Remove(tag);
            //            MainCanvas.Invalidate();
            //            return false;
            //        }
            //    }
            //}
            ////点击空白区域  所有元素失去编辑（选中）状态
            //if (_wall_paperUI != null)
            //{
            //    (_wall_paperUI as WallPaperUI).Editing = false;
            //}
            //if (_tagsUIs != null)
            //{
            //    _tagsUIs.ForEach((tag) => { (tag as TagUI).ShowCloseBtn = false; });
            //}

            //return true;
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
            double w, h;  //画布大小
            if (edit)  //编辑状态
            {
                w = MainCanvas.ActualWidth;
                h = MainCanvas.ActualHeight;
            }
            else  //最终生成图片  有一定的scale
            {
                Rect des = GetImageDrawingRect();

                w = (_image.Size.Width / des.Width) * MainCanvas.Width;
                h = (_image.Size.Height / des.Height) * MainCanvas.Height;
            }
            var scale = edit ? 1 : w / MainCanvas.Width;  //缩放比例

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget target = new CanvasRenderTarget(device, (float)w, (float)h, 96);
            using (CanvasDrawingSession graphics = target.CreateDrawingSession())
            {
                ////绘制背景
                //graphics.Clear(_back_color);

                ////绘制底图
                DrawBackImage(graphics, scale);
                ////绘制涂鸦
                //if (_doodleUIs != null && _doodleUIs.Count > 0)
                //{
                //    var list = _doodleUIs.ToList(); list.Reverse();
                //    list.ForEach((d) => { d.Draw(graphics, (float)scale); });
                //}
                //if (_current_editing_doodleUI != null)
                //{
                //    _current_editing_doodleUI.Draw(graphics, (float)scale); //正在涂鸦对象 在上面
                //}
                ////绘制贴图
                //if (_wall_paperUI != null)
                //{
                //    _wall_paperUI.Draw(graphics, (float)scale);
                //}
                ////绘制Tag
                //if (_tagsUIs != null)
                //{
                //    _tagsUIs.ForEach((t) => { t.Draw(graphics, (float)scale); });
                //}
                ////绘制Crop裁剪工具
                //if (_cropUI != null && edit)
                //{
                //    _cropUI.Draw(graphics, (float)scale);
                //}
            }

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
                // جلوه های فیلتر

                ICanvasImage image = GetBrightnessEffect(_image);
                //image = GetSharpenEffect(image);
                //image = GetBlurEffect(image);

                //image = GetStraightenEffect(image);
                //از الگوی فیلتر استفاده کنید
                image = ApplyFilterTemplate(image);

                graphics.DrawImage(image, des, _image.Bounds);
            }
        }
        private ICanvasImage GetBrightnessEffect(ICanvasImage source)
        {
            var t = /*Slider1.Value / 500 * 2*/ 50 / 500 * 2;
            var exposureEffect = new ExposureEffect
            {
                Source = source,
                Exposure = (float)t
            };

            return exposureEffect;
        }

        private void MainCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

        }

        private ICanvasImage ApplyFilterTemplate(ICanvasImage source)
        {
            return source;
        }

        #endregion
    }
}
