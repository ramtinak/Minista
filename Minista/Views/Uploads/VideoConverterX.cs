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
using Minista.Classes;
using Windows.Media.Editing;
using Windows.UI.Core;

namespace Minista.Views.Uploads
{
    public class VideoConverterX
    {
        public event ConvertionTextChanged OnText;
        public event ConvertionTextChanged OnOutput; 
        const string OutputExtension = ".mp4";
        readonly MediaTranscoder Transcoder = new MediaTranscoder();
        readonly List<StorageUploadItem> ConvertedList = new List<StorageUploadItem>();

        public bool IsConverting { get; private set; } = false;
        public VideoConverterX()
        {
            Transcoder = new MediaTranscoder();
        }
        public async Task<List<StorageUploadItem>> ConvertFilesAsync(List<StorageUploadItem> uploadItems, bool story = false)
        {
            try
            {
                ConvertedList.Clear();
                if (uploadItems.Any())
                {
                    int ix = 1;
                    const string text = "Some of your file(s) needs to be converted first. Don't close or leave Minista while converting video....";
                    foreach (var item in uploadItems)
                    {
                        try
                        {
                            Text(text + $"{ix} of {uploadItems.Count}");

                            if (item.IsVideo)
                            {
                                IsConverting = true;
                                var vid = await ConvertVideoAsync(item);
                                ("vid null: " + vid == null).PrintDebug();
                                if (vid != null)
                                    ConvertedList.Add(vid);
                            }else
                                ConvertedList.Add(item);
                        }
                        catch { }
                        ix++;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.PrintException("ConvertFiles");
            }
            IsConverting = false;
            return ConvertedList;
        }
        async Task<StorageUploadItem> ConvertVideoAsync(StorageUploadItem uploadItem)
        {
            try
            {
                var mediaProfile = MediaEncodingProfile.CreateMp4(uploadItem.VideoEncodingQuality);
                var outputFile = await GenerateRandomOutputFile();

                if (uploadItem != null && outputFile != null)
                {
                    var inputFile = uploadItem.VideoToUpload;
                    
                    var fileProfile = await GetEncodingProfileFromFileAsync(inputFile);
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
                    //MediaProfile = await MediaEncodingProfile.CreateFromFileAsync(inputFile);
                    Transcoder.TrimStartTime = uploadItem.StartTime;
                    Transcoder.TrimStopTime = uploadItem.EndTime;

                    mediaProfile.Video.Height = (uint)uploadItem.Size.Height;
                    mediaProfile.Video.Width = (uint)uploadItem.Size.Width;


                    var transform = new VideoTransformEffectDefinition
                    {
                        Rotation = MediaRotation.None,
                        OutputSize = uploadItem.Size,
                        Mirror = MediaMirroringOptions.None,
                        CropRectangle = uploadItem.Rect
                    };

                    Transcoder.AddVideoEffect(transform.ActivatableClassId, true, transform.Properties);

                    var preparedTranscodeResult = await Transcoder
                        .PrepareFileTranscodeAsync(inputFile,
                        outputFile,
                        mediaProfile);

                    if (preparedTranscodeResult.CanTranscode)
                    {
                        var progress = new Progress<double>(ConvertProgress);
                        await preparedTranscodeResult.TranscodeAsync().AsTask(new CancellationTokenSource().Token, progress);
                        ConvertComplete(outputFile);
                        uploadItem.VideoToUpload = outputFile;
                        return uploadItem;
                    }
                    else
                        preparedTranscodeResult.FailureReason.ToString().ShowMsg();

                }
            }
            catch (Exception ex) { ex.PrintException().ShowMsg("ConvertVideo"); }
            return uploadItem;

        }

        internal static async Task<MediaEncodingProfile> GetEncodingProfileFromFileAsync(StorageFile file)
        { 
            try
            {
                return await MediaEncodingProfile.CreateFromFileAsync(file);
            }
            catch { }
            return null;
        }
        async Task<StorageFile> GenerateRandomOutputFile()
        {
            try
            {
                var folder = await KnownFolders.PicturesLibrary.GetFolderAsync(Helper.AppName);
                var outfile = await folder.CreateFileAsync(Helper.GenerateString("MINISTA_")
                    + OutputExtension, CreationCollisionOption.GenerateUniqueName);
                return outfile;
            }
            catch { return await GenerateRandomOutputFileInCache(); }
        }
        async Task<StorageFile> GenerateRandomOutputFileInCache()
        {
            var folder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
            var outfile = await folder.CreateFileAsync(Helper.GenerateString("MINISTA_")
                + OutputExtension, CreationCollisionOption.GenerateUniqueName);
            return outfile;
        }

        void ConvertProgress(double percent) => Output("Converting... " + (int)percent + "%");
        void ConvertComplete(StorageFile file) => Output("Convert completed.");
        void Output(string content)
        {
            content.PrintDebug();
            OnOutput?.Invoke(content);
        }
        void Text(string content) => OnText?.Invoke(content);
    }
    public delegate void ConvertionTextChanged(string text);
}
