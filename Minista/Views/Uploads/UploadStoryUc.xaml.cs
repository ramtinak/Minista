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

namespace Minista.Views.Uploads
{
    public sealed partial class UploadStoryUc : UserControl
    {
        public bool Editing { get; set; } = true;
        public StorageFile FileToUpload, ThumbnailFile;
        private const double DefaultAspectRatio = 0.62d;
        public StorageUploadItem UploadItem => GetUploadItem();
        //BitmapDecoder BitmapDecoder;
        public bool IsVideo = false;
        bool CanCrop = false;

        public UploadStoryUc() 
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
            if (FileToUpload == null && ThumbnailFile == null) return null;
            VideoEncodingQuality quality;
            if (QualityCombo.SelectedItem != null)
                quality = (VideoEncodingQuality)Enum.Parse(typeof(VideoEncodingQuality), ((ComboBoxItem)QualityCombo.SelectedItem).Content.ToString());
            else
                quality = VideoEncodingQuality.Auto;
            var upItem = new StorageUploadItem
            {
                VideoEncodingQuality = quality,
                //VideoToUpload = FileToUpload,
                //ImageToUpload = ThumbnailFile ?? FileToUpload,
                PixelHeight = (uint)Editor.ScaledSize.Height/*BitmapDecoder.PixelHeight*/,
                PixelWidth = (uint)Editor.ScaledSize.Width/* BitmapDecoder.PixelWidth*/,
                StartTime = TimeSpan.FromSeconds(TrimControl.RangeMin),
                EndTime = TimeSpan.FromSeconds(TrimControl.RangeMax),
                //AudioMuted = ToggleMuteAudio.IsChecked ?? false,
                Rect = Editor.ScaledCropRect,
                IsStory = true
            };
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
            catch (Exception ex)
            {
                ex.PrintException("GetUploadItemAsync");
            }
            return upItem;
        }
        public async void SetFile(StorageFile file)
        {
            try
            {
                Editing = true;
                CanCrop = false;
                LoadingUc.Start();
                ResetEditor();
                if (file.IsVideo())
                {
                    VideoButton.Visibility = Visibility.Visible;
                    IsVideo = true;
                    FileToUpload = file;

                    ThumbnailFile = await file.GetSnapshotFromD3D(TimeSpan.Zero, true);
                    ThumbnailFile = await new PhotoHelper().SaveToImage(ThumbnailFile, false);
                    CropGrid.Opacity = 1;
                    CropGrid.Visibility = Visibility.Visible;
                    await Editor.SetFileAsync(ThumbnailFile);
                    MainCanvas.Visibility = Visibility.Collapsed;

                    await Task.Delay(1500);
                    CanCrop = true;
                    //LoadingUc.Stop();
                }
                else
                {
                    VideoButton.Visibility = Visibility.Collapsed;
                    IsVideo = false;
                    ThumbnailFile = null;
    
                    FileToUpload = await new PhotoHelper().SaveToImage(file, false);

                    CropGrid.Opacity = 1;
                    CropGrid.Visibility = Visibility.Visible;
                    await Editor.SetFileAsync(FileToUpload);
                    MainCanvas.Visibility = Visibility.Collapsed;
                    await Task.Delay(1500);
                    CanCrop = true;
                    //LoadingUc.Stop();
                }
            }
            finally
            {
                LoadingUc.Stop();
            }
        }
        void ResetEditor()
        {
            Editor.LargeChangeRatio = 0.05;
            Editor.MaximumRatio = 0.9;
            Editor.MinimumRatio = 0.50;
            Editor.SmallChangeRatio = 0.05;
            Editor.StepFrequencyRatio = 0.05;
            Editor.AspectRatio = DefaultAspectRatio;
        }
        public async Task SetFileAsync(StorageFile file)
        {
            try
            {
                Editing = true;
                CanCrop = false;
                LoadingUc.Start();
                ResetEditor();
                if (file.IsVideo())
                {
                    VideoButton.Visibility = Visibility.Visible;
                    IsVideo = true;
                    FileToUpload = file;
                    ThumbnailFile = await file.GetSnapshotFromD3D(TimeSpan.Zero, true);
                    ThumbnailFile = await new PhotoHelper().SaveToImage(ThumbnailFile, false);


                    ThumbnailFile = await file.GetSnapshotFromD3D(TimeSpan.Zero, true);
                    ThumbnailFile = await new PhotoHelper().SaveToImage(ThumbnailFile, false);
                    CropGrid.Opacity = 1;
                    CropGrid.Visibility = Visibility.Visible;
                    await Editor.SetFileAsync(ThumbnailFile);
                    MainCanvas.Visibility = Visibility.Collapsed;

                    CanCrop = true;
                    LoadingUc.Stop();
                }
                else
                {
                    VideoButton.Visibility = Visibility.Collapsed;
                    IsVideo = false;
                    ThumbnailFile = null;

                    FileToUpload = await new PhotoHelper().SaveToImage(file, false);

                    CropGrid.Opacity = 1;
                    CropGrid.Visibility = Visibility.Visible;
                    await Editor.SetFileAsync(file);
                    MainCanvas.Visibility = Visibility.Collapsed;

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
                    MainCanvas.Visibility = Visibility.Collapsed;
                    CropGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MainCanvas.Visibility = Visibility.Visible;
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
                    OutputSize = Editor.ScaledSize,
                    Mirror = MediaMirroringOptions.None,
                    CropRectangle = Editor.ScaledCropRect
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

                if (duration > 15)
                    duration = 15;
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
        private async void Editor_Completed(object sender, EventArgs e)
        {
            try
            {
                if (!CanCrop)
                {
                    Helper.ShowNotify("Please wait a few more seconds....");
                    return;
                }
                if (IsVideo)
                {
                    var cacheFolder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
                    var file = await cacheFolder.CreateFileAsync(15.GenerateRandomStringStatic() + ".jpg");

                    await Editor.SaveToFileAsync(file);
                    ThumbnailFile = await new PhotoHelper().SaveToImage(file, false);
                   
                    ShowImagePreview(ThumbnailFile);
                }
                else
                {
                    var cacheFolder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
                    var file = await cacheFolder.CreateFileAsync(15.GenerateRandomStringStatic() + ".jpg");

                    await Editor.SaveToFileAsync(file);
                    FileToUpload = await new PhotoHelper().SaveToImage(file, false, false);
                    ShowImagePreview(FileToUpload);
                }
                Editing = false;
            }
            catch (Exception ex)
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
            {
                args.DrawingSession.DrawImage(target);
            }
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

                var image = ApplyFilterTemplate(_image);

                graphics.DrawImage(image, des, _image.Bounds);
            }
        }

        private ICanvasImage ApplyFilterTemplate(ICanvasImage source)
        {
            return source;
        }

        #endregion

    }
}
