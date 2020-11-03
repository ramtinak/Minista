//using Lumia.Imaging;
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
using Minista.Classes;
using Minista.Helpers;
using InstagramApiSharp.Enums;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using System.Collections.ObjectModel;
using Minista.UserControls;
using Minista.UserControls.Main;
using Minista.ContentDialogs;

namespace Minista.Views.Uploads
{
    public sealed partial class UploadUc : UserControl
    {
        //public ImageCropper ImageCropper;
        public bool Editing { get; set; } = true;
        public StorageFile FileToUpload, ThumbnailFile;
        private const double DefaultAspectRatio = 1.62d;
        public StorageUploadItem UploadItem => GetUploadItem();
        BitmapDecoder BitmapDecoder;
        Rect CurrentCroppedRectForVideo;
        public bool IsVideo = false;
        private bool _canTagPeople = false;
        public bool CanTagPeople
        {
            get => _canTagPeople;
            set
            {
                _canTagPeople = value;
                UserTags.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public UploadUc()
        {
            this.InitializeComponent();
            Loaded += OnPageLoaded;
            Unloaded += UploadPostView_Unloaded;
            Window.Current.SizeChanged += CurrentSizeChanged;
        }


        private void QualityComboLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (QualityCombo.Items.Count > 0)
                    QualityCombo.SelectedIndex = 0;
            }
            catch { }
        }

        StorageUploadItem GetUploadItem()
        {
            var quality = VideoEncodingQuality.Auto;
            if (QualityCombo.SelectedItem != null)
                quality = (VideoEncodingQuality)Enum.Parse(typeof(VideoEncodingQuality), ((ComboBoxItem)QualityCombo.SelectedItem).Content.ToString());
            else
                quality = VideoEncodingQuality.Auto;
            var upItem = new StorageUploadItem
            {
                VideoEncodingQuality = quality,
                //VideoToUpload = FileToUpload,
                //ImageToUpload = ThumbnailFile ?? FileToUpload,
                PixelHeight = BitmapDecoder.PixelHeight,
                PixelWidth = BitmapDecoder.PixelWidth,
                StartTime = TimeSpan.FromSeconds(TrimControl.RangeMin),
                EndTime = TimeSpan.FromSeconds(TrimControl.RangeMax),
                //AudioMuted = ToggleMuteAudio.IsChecked ?? false,
               Rect = ImageCropper.CurrentCroppedRect
            };
            upItem.UserTags = GetMediaTagUcs();
            upItem.ThisIsVideo(IsVideo);
            if (IsVideo)
            {
                upItem.VideoToUpload = FileToUpload;
                upItem.ImageToUpload = ThumbnailFile;
            }
            else
            {
                upItem.VideoToUpload = null;
                upItem.ImageToUpload = FileToUpload;
            }    
            try
            {
                if (IsVideo)
                {
                    var start = TimeSpan.FromSeconds(TrimControl.RangeMin);
                    var end = TimeSpan.FromSeconds(TrimControl.RangeMax);

                    upItem.StartTime = start;
                    upItem.EndTime = end;

                }
            }
            catch(Exception ex)
            {
                ex.PrintException("GetUploadItemAsync");
            }
            return upItem;
        }
        bool CanCrop = false;
        public async void SetFile(StorageFile file)
        {
            try
            {
                Editing = true; 
                CanCrop = false;
                LoadingUc.Start();
                if (file.IsVideo())
                {
                    VideoButton.Visibility = Visibility.Visible;
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
                    ImageX.Visibility = Visibility.Collapsed;

                    AspectRatioSlider.Value = wRatio;
                    ImageCropper.AspectRatio = wRatio;
                    ImageCropper.CropShape = CropShape.Rectangular;
                    await ImageCropper.LoadImageFromFile(ThumbnailFile);

                    CropGrid.Visibility = Visibility.Visible;

                    await Task.Delay(1500);
                    ImageCropper.AspectRatio = DefaultAspectRatio;
                    AspectRatioSlider.Value = DefaultAspectRatio;
                    CanCrop = true;
                    LoadingUc.Stop();
                }
                else
                {
                    VideoButton.Visibility = Visibility.Collapsed;
                    IsVideo = false;
                    ThumbnailFile = null;
                    BitmapDecoder = null;
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

                    //in paeini comment bod
                    FileToUpload = await new PhotoHelper().SaveToImage(file, false);

                    if (!editNeeded)
                    {


                        CropGrid.Visibility = Visibility.Visible;

                        ImageX.Visibility = Visibility.Collapsed;

                        AspectRatioSlider.Value = wRatio;
                        ImageCropper.AspectRatio = wRatio;
                        ImageCropper.CropShape = CropShape.Rectangular;
                        await ImageCropper.LoadImageFromFile(file);
                    }
                    else
                    {
                        CropGrid.Visibility = Visibility.Visible;
                        ImageX.Visibility = Visibility.Collapsed;
                        AspectRatioSlider.Value = DefaultAspectRatio;
                        ImageCropper.AspectRatio = DefaultAspectRatio;
                        ImageCropper.CropShape = CropShape.Rectangular;
                        await ImageCropper.LoadImageFromFile(file);
                    }
                    await Task.Delay(1500);
                    CanCrop = true;
                    LoadingUc.Stop();
                }
            }
            catch { }
        }

        public async Task SetFileAsync(StorageFile file)
        {
            try
            {
                Editing = true;
                CanCrop = false;
                LoadingUc.Start();
                if (file.IsVideo())
                {
                    VideoButton.Visibility = Visibility.Visible;
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
                    ImageX.Visibility = Visibility.Collapsed;

                    AspectRatioSlider.Value = wRatio;
                    ImageCropper.AspectRatio = wRatio;
                    ImageCropper.CropShape = CropShape.Rectangular;
                    await ImageCropper.LoadImageFromFile(ThumbnailFile);

                    CropGrid.Visibility = Visibility.Visible;

                    await Task.Delay(1500);
                    ImageCropper.AspectRatio = DefaultAspectRatio;
                    AspectRatioSlider.Value = DefaultAspectRatio;
                    CanCrop = true;
                    LoadingUc.Stop();
                }
                else
                {
                    VideoButton.Visibility = Visibility.Collapsed;
                    IsVideo = false;
                    ThumbnailFile = null;
                    BitmapDecoder = null;
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

                    //in paeini comment bod
                    FileToUpload = await new PhotoHelper().SaveToImage(file, false);

                    if (!editNeeded)
                    {


                        CropGrid.Visibility = Visibility.Visible;

                        ImageX.Visibility = Visibility.Collapsed;

                        AspectRatioSlider.Value = wRatio;
                        ImageCropper.AspectRatio = wRatio;
                        ImageCropper.CropShape = CropShape.Rectangular;
                        await ImageCropper.LoadImageFromFile(file);
                    }
                    else
                    {
                        CropGrid.Visibility = Visibility.Visible;
                        ImageX.Visibility = Visibility.Collapsed;
                        AspectRatioSlider.Value = DefaultAspectRatio;
                        ImageCropper.AspectRatio = DefaultAspectRatio;
                        ImageCropper.CropShape = CropShape.Rectangular;
                        await ImageCropper.LoadImageFromFile(file);
                    }
                    //await Task.Delay(1500);
                    CanCrop = true;
                    LoadingUc.Stop();
                }
            }
            catch { }
        }

        void ShowImagePreview(StorageFile file)
        {
            try
            {
                if (IsVideo)
                {
                    ShowVideo();
                    MediaElementGrid.Visibility = Visibility.Visible;
                    ImageX.Visibility = Visibility.Collapsed;
                    CropGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ImageX.Visibility = Visibility.Visible;
                    MediaElementGrid.Visibility = Visibility.Collapsed;
                    CropGrid.Visibility = Visibility.Collapsed;
                    Show(file);
                }
            }
            catch { }
        }

        private async void MEMediaOpened(object sender, RoutedEventArgs e)
        {
            await Task.Delay(250);
            ShowTrim();
        }
        private void UploadPostView_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= CurrentSizeChanged;
        }
        void ShowVideo()
        {
            try
            {
                var MediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(FileToUpload));
                //FileToUpload
                ME.SetPlaybackSource(MediaPlaybackItem);
                ME.RemoveAllEffects();
                var transform = new VideoTransformEffectDefinition
                {
                    Rotation = MediaRotation.None,
                    OutputSize = new Size(BitmapDecoder.PixelWidth, BitmapDecoder.PixelHeight),
                    Mirror = MediaMirroringOptions.None,
                    CropRectangle = CurrentCroppedRectForVideo
                };

                ME.AddVideoEffect(transform.ActivatableClassId, true, transform.Properties);
            }
            catch { }
        }
        async void ShowTrim()
        {
            if (!IsVideo) return;

            try
            {
                var duration = ME.NaturalDuration.TimeSpan/*(await FileToUpload.GetDurationAsync())*/.TotalSeconds;

                if (duration > 59)
                    duration = 59;
                TrimControl.Minimum = 0;
                TrimControl.Maximum = duration;
                StartTimeText.Text = $"Start time: 00";
                EndTimeText.Text = $"End time: {duration:00}";
                await Task.Delay(250);
                TrimControl.RangeMax = duration;
            }
            catch { }
        }
        private void ToggleSnapShotChecked(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    if (ToggleSnapShot.IsChecked.HasValue)
            //        if (ToggleSnapShot.IsChecked.Value)
            //            ToggleSnapShot.IsChecked = false;
            //}
            //catch { }
        }

        //private void ToggleMuteAudioChecked(object sender, RoutedEventArgs e)
        //{
        //    ToolTipService.SetToolTip(ToggleMuteAudio, "Sound muted");
        //}

        //private void ToggleMuteAudioUnchecked(object sender, RoutedEventArgs e)
        //{
        //    ToolTipService.SetToolTip(ToggleMuteAudio, "Mute sound");

        //}

        //private void ToggleSnapShotClick(object sender, RoutedEventArgs e)
        //{
        //    Helper.ShowNotify("This option will be available in the next release...", 4000);
        //}
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            SetCanvas();
        }

        private void TrimControlValueChanged(object sender, Microsoft.Toolkit.Uwp.UI.Controls.RangeChangedEventArgs e)
        {
            if (sender == null) return;
            try
            {
                var start = TrimControl.RangeMin;
                var end = TrimControl.RangeMax;

                StartTimeText.Text = $"Start time: {start:00}";
                EndTimeText.Text = $"End time: {end:00}";
            }
            catch { }
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
                if (!CanCrop)
                {
                    Helper.ShowNotify("Please wait a few more seconds....");
                    return;
                }
                Helper.CreateCachedFolder();
                Helper.CreateCachedFolder();
                Helper.CreateCachedFolder();
                if (IsVideo)
                {
                    using (var fileStream = await ThumbnailFile.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None))
                        await ImageCropper.SaveAsync(fileStream, BitmapFileFormat.Jpeg);
                    ThumbnailFile = await new PhotoHelper().SaveToImage(ThumbnailFile, false);
                    var decoder = await BitmapDecoder.CreateAsync(await ThumbnailFile.OpenReadAsync());
                    BitmapDecoder = decoder;
                    CurrentCroppedRectForVideo = ImageCropper.CurrentCroppedRect;
                    ShowImagePreview(ThumbnailFile);
                }
                else
                {
                    var cacheFolder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
                    var file = await cacheFolder.CreateFileAsync(15.GenerateRandomStringStatic() + ".jpg");
                    //await ImageCropper.CroppedImage.SaveAsync(file);
                    using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None))
                        await ImageCropper.SaveAsync(fileStream, BitmapFileFormat.Jpeg);
                    FileToUpload = await new PhotoHelper().SaveToImage(file, false, false);

                    var decoder = await BitmapDecoder.CreateAsync(await FileToUpload.OpenReadAsync());
                    BitmapDecoder = decoder;
                    //CropGrid.Visibility = Visibility.Collapsed;
                    ShowImagePreview(FileToUpload);
                }
                Editing = false;

            }
            catch (Exception ex)
            {
                ex.PrintException("CropButtonClick");
            }
        }

        private void MainCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

        }

        private /*async*/ void CancelButtonClick(object sender, RoutedEventArgs e)
        {
        }
        private void ImageXTapped(object sender, TappedRoutedEventArgs e)
        {
            if (CanTagPeople)
            {
                var p = e.GetPosition(ImageX);
                var x = (p.X / ImageX.ActualWidth);
                var y = (p.Y / ImageX.ActualHeight);

                var trans = new CompositeTransform()
                {
                    TranslateX = p.X,
                    TranslateY = p.Y,
                };
                var mtc = new MediaTagUc()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Opacity = 1,
                    RenderTransform = trans,
                    Name = "UserTags" + 8.GenerateRandomStringStatic(),
                };
                
                mtc.SetUserTag(new InstaUserTagUpload
                {
                    X = x,
                    Y = y
                }, IsVideo);
                mtc.Tapped += MtcTapped;
                mtc.DoubleTapped += MtcDoubleTapped;
                mtc.Visibility = Visibility.Visible;
                UserTags.Children.Add(mtc);
                mtc.RenderTransform = new CompositeTransform()
                {
                    TranslateX = p.X,
                    TranslateY = p.Y,
                };
                MtcTapped(mtc, null);
            }
        }

        public List<InstaUserTagUpload> GetMediaTagUcs()
        {
            var list = new List<InstaUserTagUpload>();
            if (UserTags.Children.Count > 0)
            {
                foreach(var item in UserTags.Children)
                {
                    if (item is MediaTagUc mtc && mtc != null)
                        list.Add(mtc.UserTagUpload);
                }
            }
            return list;
        }
        private async void MtcTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is MediaTagUc mtc)
                {
                    await new AddUserTagDialog(mtc).ShowAsync();

                    if (string.IsNullOrEmpty(mtc.UserTagUpload.Username))
                        UserTags.Children.Remove(mtc);
                }
            }
            catch { }
        }

        private void MtcDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
                if (sender is MediaTagUc mtc)
                    UserTags.Children.Remove(mtc);
            }
            catch { }
        }

        #region Canvas

        private async Task<ImageSource> GetImageSourceAsync(IRandomAccessStream randomAccessStream)
        {
            try
            {
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(randomAccessStream);
                return bitmap;
            }
            catch { }
            return null;
        }
        private void CurrentSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            SetCanvas();
            //MainCanvas.Invalidate();
        }
        //private CanvasBitmap _image;
        public async void Show(StorageFile image)
        {
            try
            {
                Show();
                using(var stream = await image.OpenAsync(FileAccessMode.Read))
                ImageX.Source = await GetImageSourceAsync(stream);
                ////WaitLoading.IsActive = true;
                //CanvasDevice cd = CanvasDevice.GetSharedDevice();
                //var stream = await image.OpenAsync(FileAccessMode.Read);
                //_image = await CanvasBitmap.LoadAsync(cd, stream);
                ////WaitLoading.IsActive = false;
                //MainCanvas.Invalidate();
            }
            catch
            {
            }
        }
        private void Show()
        {
            SetCanvas();
        }


        ///// <summary>
        ///// Canvas drawing
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="args"></param>
        //private void MainCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        //{
        //    var target = GetDrawings(true);
        //    if (target != null)
        //    {
        //        args.DrawingSession.DrawImage(target);
        //    }
        //}
        private void SetCanvas()
        {
            try
            {

                var w = MainWorkSapce.ActualWidth - 40;
                var h = MainWorkSapce.ActualHeight - 40;

                ImageX.Width = w;
                ImageX.Height = h;
                //MainCanvas.Invalidate();
            }
            catch { }
        }
        //private async void GenerateResultImage()
        //{
        //    var img = GetDrawings(false);
        //    if (img != null)
        //    {
        //        IRandomAccessStream stream = new InMemoryRandomAccessStream();
        //        await img.SaveAsync(stream, CanvasBitmapFileFormat.Jpeg);
        //        BitmapImage result = new BitmapImage();
        //        stream.Seek(0);
        //        await result.SetSourceAsync(stream);
        //    }
        //}
        //private Rect GetImageDrawingRect()
        //{
        //    Rect des;

        //    var image_w = _image.Size.Width;
        //    var image_h = _image.Size.Height;


        //    var w = MainCanvas.Width - 20;
        //    var h = MainCanvas.Height - 20;
        //    if (image_w / image_h > w / h)
        //    {
        //        var left = 10;

        //        var width = w;
        //        var height = (image_h / image_w) * width;

        //        var top = (h - height) / 2 + 10;

        //        des = new Rect(left, top, width, height);
        //    }
        //    else
        //    {
        //        var top = 10;
        //        var height = h;
        //        var width = (image_w / image_h) * height;
        //        var left = (w - width) / 2 + 10;
        //        des = new Rect(left, top, width, height);
        //    }
        //    return des;
        //}
        //private CanvasRenderTarget GetDrawings(bool edit)
        //{
        //    double w, h;
        //    if (edit)
        //    {
        //        w = MainCanvas.ActualWidth;
        //        h = MainCanvas.ActualHeight;
        //    }
        //    else
        //    {
        //        Rect des = GetImageDrawingRect();

        //        w = (_image.Size.Width / des.Width) * MainCanvas.Width;
        //        h = (_image.Size.Height / des.Height) * MainCanvas.Height;
        //    }
        //    var scale = edit ? 1 : w / MainCanvas.Width;

        //    CanvasDevice device = CanvasDevice.GetSharedDevice();
        //    CanvasRenderTarget target = new CanvasRenderTarget(device, (float)w, (float)h, 96);
        //    using (CanvasDrawingSession graphics = target.CreateDrawingSession())
        //        DrawBackImage(graphics, scale);

        //    return target;
        //}
        //private void DrawBackImage(CanvasDrawingSession graphics, double scale)
        //{
        //    if (_image != null)
        //    {
        //        Rect des = GetImageDrawingRect();
        //        des.X *= scale;
        //        des.Y *= scale;
        //        des.Width *= scale;
        //        des.Height *= scale;

        //        //ICanvasImage image = GetBrightnessEffect(_image);
        //        var image = ApplyFilterTemplate(_image);

        //        graphics.DrawImage(image, des, _image.Bounds);
        //    }
        //}

        //private ICanvasImage ApplyFilterTemplate(ICanvasImage source)
        //{
        //    return source;
        //}

        #endregion

    }

    public class UploadUcItem : BaseModel
    {
        public UploadUc UploadUc { get; set; }

        private Visibility _videoVisibility = Visibility.Collapsed;
        public Visibility VideoVisibility { get => _videoVisibility;set { _videoVisibility = value; OnPropertyChanged("VideoVisibility"); } }

        private Visibility _plusVisibility = Visibility.Collapsed;
        public Visibility PlusVisibility { get => _plusVisibility; set { _plusVisibility = value; OnPropertyChanged("PlusVisibility"); } }

        private Visibility _closeVisibility = Visibility.Collapsed;
        public Visibility CloseVisibility { get => _closeVisibility; set { _closeVisibility = value; OnPropertyChanged("CloseVisibility"); } }

        bool _started;
        public bool Started
        {
            get { return _started; }
            set
            {
                _started = value;
                OnPropertyChanged("Started");
            }
        }
        BitmapImage thumb_;
        public BitmapImage Thumbnail
        {
            get { return thumb_; }
            set
            {
                thumb_ = value;
                OnPropertyChanged("Thumbnail");
            }
        }
        public async void SetThumbIfExists()
        {
            try
            {
                if (UploadUc != null)
                {
                    //Started = true;
                    VideoVisibility = UploadUc.IsVideo ? Visibility.Visible : Visibility.Collapsed;
                    var bitmap = new BitmapImage();
                    if (UploadUc.IsVideo && UploadUc.ThumbnailFile != null)
                        bitmap.SetSource(await UploadUc.ThumbnailFile.OpenAsync(FileAccessMode.Read));
                    else if (UploadUc.FileToUpload != null)
                        bitmap.SetSource(await UploadUc.FileToUpload.OpenAsync(FileAccessMode.Read));
                    Thumbnail = bitmap;
                }
            }
            catch { }
            try { if (Loadings != null) Loadings.Stop(); } catch { }
            }

        public LoadingUc Loadings { get; set; }
    }
}
