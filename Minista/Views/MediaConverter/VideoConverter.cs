using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FFmpegInterop;
using Windows.Foundation;
using Windows.Media.Effects;

namespace Minista.Views.MediaConverter
{
    public class VideoConverter
    {
        const string OutputExtension = ".mp4";
        readonly CancellationTokenSource Cts;
        readonly MediaTranscoder Transcoder = new MediaTranscoder();
        TimeSpan StartTime = new TimeSpan(0);
        TimeSpan StopTime = new TimeSpan(0, 0, 59);
        MediaStreamSource Mss;
        FFmpegInteropMSS FFmpegMSS;
        readonly List<StorageFile> QueueList = new List<StorageFile>();
        readonly List<StorageFile> ConvertedList = new List<StorageFile>();

        public bool IsConverting { get; private set; } = false;
        bool IsStoryVideo = false;

        public VideoConverter()
        {
            Transcoder = new MediaTranscoder();
            Cts = new CancellationTokenSource();
        }
        public async Task<List<StorageFile>> ConvertFiles(List<StorageFile> files, bool story, Size? size, Rect? rectSize)
        {
            try
            {
                IsStoryVideo = story;
                if (story)
                    StopTime = TimeSpan.FromSeconds(14.8);
                else
                    StopTime = new TimeSpan(0, 0, 59);
                QueueList.Clear();
                ConvertedList.Clear();
                foreach (var item in files)
                {
                    if (item.IsVideo())
                        try
                        {
                            QueueList.Add(item);
                        }
                        catch { }
                }
                if (QueueList.Any())
                {
                    int ix = 1;
                    const string text = "Some of your file(s) needs to be converted first. Please wait...\r\n";
                    foreach (var item in QueueList)
                    {
                        try
                        {
                            if (item.IsVideo())
                            {
                                Output(text + $"{ix} of {QueueList.Count}");
                                IsConverting = true;
                                var vid = await ConvertVideo(item, size, rectSize);
                                ("vid null: " + vid == null).PrintDebug();
                                if (vid != null)
                                    ConvertedList.Add(vid);
                            }
                        }
                        catch { }
                        ix++;
                    }
                }
                try
                {
                    Mss = null;
                    FFmpegMSS = null;
                }
                catch { }
            }
            catch (Exception ex)
            {
                ex.PrintException("ConvertFiles");
            }
            IsConverting = false;
            return ConvertedList;
        }
        async Task<StorageFile> ConvertVideo(StorageFile inputFile, Size? imageSize, Rect? rectSize)
        {
            try
            {
                var outputFile = await GenerateRandomOutputFile();

                if (inputFile != null && outputFile != null)
                {
                    var mediaProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
                    int height = 0, width = 0;
                    double duration = 0;
                    if (DeviceUtil.IsMobile)
                    {
                        var videoInfo = await inputFile.GetVideoInfoAsync();
                        height = (int)videoInfo.Height;
                        width = (int)videoInfo.Width;
                        duration = videoInfo.Duration.TotalSeconds;
                    }
                    else
                    {
                        FFmpegMSS = await FFmpegInteropMSS
                            .CreateFromStreamAsync(await inputFile.OpenReadAsync(), Helper.FFmpegConfig);
                        Mss = FFmpegMSS.GetMediaStreamSource();
                        height = FFmpegMSS.VideoStream.PixelHeight;
                        width = FFmpegMSS.VideoStream.PixelWidth;
                        duration = Mss.Duration.TotalSeconds;
                    }

                    var fileProfile = await Uploads.VideoConverterX.GetEncodingProfileFromFileAsync(inputFile);
                    if (fileProfile != null)
                    {
                        mediaProfile.Video.Bitrate = fileProfile.Video.Bitrate;
                        if (mediaProfile.Audio != null)
                        {
                            mediaProfile.Audio.Bitrate = fileProfile.Audio.Bitrate;
                            mediaProfile.Audio.BitsPerSample = fileProfile.Audio.BitsPerSample;
                            mediaProfile.Audio.ChannelCount = fileProfile.Audio.ChannelCount;
                            mediaProfile.Audio.SampleRate = fileProfile.Audio.SampleRate;
                        }
                        "Media profile copied from original video".PrintDebug();
                    }
                    if (!IsStoryVideo)
                    {
                        if (duration > 59)
                        {
                            Transcoder.TrimStartTime = StartTime;
                            Transcoder.TrimStopTime = StopTime;
                        }


                        var max = Math.Max(height, width);
                        if (max > 1920)
                            max = 1920;
                        if (imageSize == null)
                        {
                            mediaProfile.Video.Height = (uint)max;
                            mediaProfile.Video.Width = (uint)max;
                        }
                        else
                        {
                            mediaProfile.Video.Height = (uint)imageSize.Value.Height;
                            mediaProfile.Video.Width = (uint)imageSize.Value.Width;
                        }
                    }
                    else
                    {
                        if (duration > 14.9)
                        {
                            Transcoder.TrimStartTime = StartTime;
                            Transcoder.TrimStopTime = StopTime;
                        }
                        var size = Helpers.AspectRatioHelper.GetAspectRatioX(width, height);
                        mediaProfile.Video.Height = (uint)size.Height;
                        mediaProfile.Video.Width = (uint)size.Width;
                    }

                    var transform = new VideoTransformEffectDefinition
                    {
                        Rotation = MediaRotation.None,
                        OutputSize = imageSize.Value,
                        Mirror = MediaMirroringOptions.None,
                        CropRectangle = rectSize == null ? Rect.Empty : rectSize.Value
                    };
                    
                    Transcoder.AddVideoEffect(transform.ActivatableClassId, true, transform.Properties);

                    PrepareTranscodeResult preparedTranscodeResult;
                    if (DeviceUtil.IsMobile)
                        preparedTranscodeResult = await Transcoder.PrepareFileTranscodeAsync(inputFile, outputFile, mediaProfile);
                    else
                        preparedTranscodeResult = await Transcoder.PrepareMediaStreamSourceTranscodeAsync(Mss,
                            await outputFile.OpenAsync(FileAccessMode.ReadWrite), mediaProfile);

                    Transcoder.VideoProcessingAlgorithm = MediaVideoProcessingAlgorithm.Default;
                    if (preparedTranscodeResult.CanTranscode)
                    {
                        var progress = new Progress<double>(ConvertProgress);
                        await preparedTranscodeResult.TranscodeAsync().AsTask(Cts.Token, progress);
                        ConvertComplete(outputFile);
                        return outputFile;
                    }
                    else
                        preparedTranscodeResult.FailureReason.ToString().ShowMsg();

                }
            }
            catch (Exception ex) { ex.PrintException().ShowMsg("ConvertVideo"); }
            return null;

        }
        async Task<StorageFile> GenerateRandomOutputFile()
        {
            var folder = await KnownFolders.PicturesLibrary.GetFolderAsync(Helper.AppName);
            var outfile = await folder.CreateFileAsync(Helper.GenerateString("MINISTA_")
                + OutputExtension, CreationCollisionOption.GenerateUniqueName);
            return outfile;
        }

        void ConvertProgress(double percent) => Output("Converting... " + (int)percent + "%");
        void ConvertComplete(StorageFile file) => Output("Convert completed.");
        void Output(string content) => content.PrintDebug();
    }
}
