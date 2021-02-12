using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using System.Net;
using Newtonsoft.Json;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json.Linq;
using InstagramApiSharp.Classes.ResponseWrappers;
using System.Net.Http;
using System.Linq;
using InstagramApiSharp.Converters;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using static Helper;
using InstagramApiSharp.Converters.Json;
using Minista.Classes;
using InstagramApiSharp.Helpers;
using Minista.Views.Direct;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media.Playback;
using Windows.Media.Core;
//using FFmpegInterop;
using Windows.Graphics.Imaging;
using Minista.Views.MediaConverter;
using Windows.Foundation;

namespace Minista.Helpers
{
    class VideoUploader
    {
        public string Name { get; private set; }
        public VideoUploader()
        {
            Name = Guid.NewGuid().ToString();
        }
        internal string GenerateUploadId()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            var uploadId = (long)timeSpan.TotalMilliseconds;
            return uploadId.ToString();
        }
        public static string GenerateRandomUploadId()
        {
            return DateTime.UtcNow.ToUnixTimeMiliSeconds().ToString();
        }
        static Uri GetMediaConfigureUri()
        {
            return new Uri("https://i.instagram.com/api/v1/media/configure/?video=1", UriKind.RelativeOrAbsolute);
        }
        static Uri GetUploadPhotoUri(string uploadId, int fileHashCode)
        {
            return new Uri(string.Format("https://i.instagram.com/rupload_igvideo/{0}_0_{1}", uploadId, fileHashCode), UriKind.RelativeOrAbsolute);
        }

        private string GetRetryContext()
        {
            return new JObject
                {
                    {"num_step_auto_retry", 0},
                    {"num_reupload", 0},
                    {"num_step_manual_retry", 0}
                }.ToString(Formatting.None);
        }
        CancellationTokenSource cts;

        public string UploadId { get; private set; }
        public string Caption { get; set; }
        StorageFile NotifyFile;
        StorageFile Thumbnail;
        double Duration;
        public async void UploadVideo(StorageFile File, StorageFile thumbnail, string caption, BitmapDecoder decoder, Rect rectSize)
        {
            try
            {
                var videoInfo = await File.GetVideoInfoAsync();
                bool isMatch = false;
                if (videoInfo != null)
                    isMatch = videoInfo.Height == decoder.PixelHeight && videoInfo.Width == decoder.PixelWidth;
    
                var files =await new VideoConverter().ConvertFiles(new List<StorageFile> { File }, false, new Size(decoder.PixelWidth, decoder.PixelHeight), rectSize);
                if (files.Count > 0)
                    UploadSingleVideo(files[0], thumbnail, caption);

            }
            catch (Exception ex)
            {
                ex.PrintException("UploadVideo");
                UploadSingleVideo(File, thumbnail, caption);
            }
        }
        /*public*/ async void UploadSingleVideo(StorageFile File, StorageFile thumbnail, string caption)
        {
            Caption = caption;
            Thumbnail = thumbnail;
            if (!string.IsNullOrEmpty(Caption))
                Caption = Caption.Replace("\r", "\n");
            try
            {
                var frameHelper = await FrameHelper.GetVideoInfoAsync(File);
                Duration = frameHelper.Duration.TotalSeconds;

                await Task.Delay(250);
            }
            catch { }
            UploadId = GenerateUploadId();
            try
            {
                var cacheFolder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
                NotifyFile = await cacheFolder.CreateFileAsync(15.GenerateRandomStringStatic() + ".jpg");
                NotifyFile = await File.CopyAsync(cacheFolder);
            }
            catch { }
            var photoHashCode = Path.GetFileName(File.Path ?? $"C:\\{13.GenerateRandomStringStatic()}.jpg").GetHashCode();
            var photoEntityName = $"{UploadId}_0_{photoHashCode}";
            var instaUri = GetUploadPhotoUri(UploadId, photoHashCode);

            var device = InstaApi.GetCurrentDevice();
            var httpRequestProcessor = InstaApi.HttpRequestProcessor;

            BackgroundUploader BGU = new BackgroundUploader();

            var cookies = httpRequestProcessor.HttpHandler.CookieContainer.GetCookies(httpRequestProcessor.Client.BaseAddress);
            var strCookies = string.Empty;
            foreach (Cookie cook in cookies)
                strCookies += $"{cook.Name}={cook.Value}; ";
            // header haye asli in ha hastan faghat
            BGU.SetRequestHeader("Cookie", strCookies);
            var r = InstaApi.HttpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, device);
            foreach (var item in r.Headers)
            {
                BGU.SetRequestHeader(item.Key, string.Join(" ", item.Value));
            }


            var photoUploadParamsObj = new JObject
            {
                {"upload_id", UploadId},
                {"media_type", "1"},
                {"retry_context", GetRetryContext()},
                {"image_compression", "{\"lib_name\":\"moz\",\"lib_version\":\"3.1.m\",\"quality\":\"95\"}"},
            };
            var photoUploadParams = JsonConvert.SerializeObject(photoUploadParamsObj);
            var openedFile = await File.OpenAsync(FileAccessMode.Read);

            BGU.SetRequestHeader("X-Entity-Type", "image/jpeg");
            BGU.SetRequestHeader("Offset", "0");
            BGU.SetRequestHeader("X-Instagram-Rupload-Params", photoUploadParams);
            BGU.SetRequestHeader("X-Entity-Name", photoEntityName);
            BGU.SetRequestHeader("X-Entity-Length", openedFile.AsStream().Length.ToString());
            BGU.SetRequestHeader("X_FB_PHOTO_WATERFALL_ID", Guid.NewGuid().ToString());
            BGU.SetRequestHeader("Content-Transfer-Encoding", "binary");
            BGU.SetRequestHeader("Content-Type", "application/octet-stream");

            Debug.WriteLine("----------------------------------------Start upload----------------------------------");

            //var uploadX = await BGU.CreateUploadAsync(instaUri, parts, "", UploadId);
            var upload = BGU.CreateUpload(instaUri, File);
            upload.Priority = BackgroundTransferPriority.High;
            await HandleUploadAsync(upload, true);
        }
        async Task HandleUploadAsync(UploadOperation upload, bool start)
        {
            cts = new CancellationTokenSource();
            try
            {
                LogStatus("Running: " + upload.Guid);

                Progress<UploadOperation> progressCallback = new Progress<UploadOperation>(UploadProgress);
                if (start)
                {

                    // Start the upload and attach a progress handler.
                    await upload.StartAsync().AsTask(cts.Token, progressCallback);
                }
                else
                {
                    // The upload was already running when the application started, re-attach the progress handler.
                    await upload.AttachAsync().AsTask(cts.Token, progressCallback);
                }

                ResponseInformation response = upload.GetResponseInformation();

                LogStatus(string.Format("Completed: {0}, Status Code: {1}", upload.Guid, response.StatusCode));
            }
            catch (TaskCanceledException)
            {
                MainPage.Current?.HideMediaUploadingUc();
                LogStatus("Canceled: " + upload.Guid);
            }
            catch (Exception)
            {

                MainPage.Current?.HideMediaUploadingUc();
                //throw;

            }
        }

        async void UploadProgress(UploadOperation upload)
        {
            LogStatus(string.Format("Progress: {0}, Status: {1}", upload.Guid, upload.Progress.Status));

            BackgroundUploadProgress progress = upload.Progress;

            double percentSent = 100;
            if (progress.TotalBytesToSend > 0)
            {
                var bs = progress.BytesSent;
                percentSent = bs * 100 / progress.TotalBytesToSend;
            }

            SendNotify(percentSent);
            //MarshalLog(String.Format(" - Sent bytes: {0} of {1} ({2}%)",
            //  progress.BytesSent, progress.TotalBytesToSend, percentSent));

            LogStatus(string.Format(" - Sent bytes: {0} of {1} ({2}%), Received bytes: {3} of {4}",
                progress.BytesSent, progress.TotalBytesToSend, percentSent,
                progress.BytesReceived, progress.TotalBytesToReceive));

            if (progress.HasRestarted)
            {
                LogStatus(" - Upload restarted");
            }

            if (progress.HasResponseChanged)
            {
                var resp = upload.GetResponseInformation();
                // We've received new response headers from the server.
                LogStatus(" - Response updated; Header count: " + resp.Headers.Count);
                var response = upload.GetResultStreamAt(0);
                StreamReader stream = new StreamReader(response.AsStreamForRead());
                Debug.WriteLine("----------------------------------------Response from upload----------------------------------");
                var st = stream.ReadToEnd();
                Debug.WriteLine(st);
                //var res = JsonConvert.DeserializeObject<APIResponseOverride>(st);
                //await ConfigureMediaPhotoAsync(res.upload_id, Caption, null, null);
                var thumbStream = (await Thumbnail.OpenAsync(FileAccessMode.Read)).AsStream();
                var bytes= await thumbStream.ToByteArray();
                var img = new InstaImageUpload
                {
                    ImageBytes = bytes,
                    Uri = Thumbnail.Path
                };

                await UploadSinglePhoto(img, UploadId, false);
                await Task.Delay(15000);
                await FinishVideoAsync();
                await Task.Delay(1500);
                await ConfigureVideoAsync();
                //UploadCompleted?.Invoke(null, Convert.ToInt64(res.upload_id));
                // If you want to stream the response data this is a good time to start.
                // upload.GetResultStreamAt(0);
            }
        }

        private async Task<IResult<bool>> FinishVideoAsync()
        {
            try
            {
                Debug.WriteLine("----------------------------------------FinishVideoAsync----------------------------------");
                var instaUri = new Uri("https://i.instagram.com/api/v1/media/upload_finish/?video=1");
                var retryContext = GetRetryContext();
                var _user = InstaApi.GetLoggedUser();
                var _deviceInfo = InstaApi.GetCurrentDevice();
                var data = new JObject
                {
                    //{"date_time_digitalized", DateTime.UtcNow.ToString("yyyy:MM:dd+hh:mm:ss")},
                    //{"date_time_original", DateTime.UtcNow.ToString("yyyy:MM:dd+hh:mm:ss")},
                    //{"is_suggested_venue", "false"},
                    {"filter_type", "0"},
                    {"timezone_offset", InstaApi.GetTimezoneOffset().ToString()},
                    {"_csrftoken", _user.CsrfToken},
                    {"source_type", "4"},
                    {"_uid", _user.LoggedInUser.Pk.ToString()},
                    {"device_id", _deviceInfo.DeviceId},
                    {"_uuid", _deviceInfo.DeviceGuid.ToString()},
                    {"caption", Caption ?? string.Empty},
                    {"date_time_original", "19040101T000000.000Z"},
                    {"upload_id", UploadId},
                    {
                        "device", new JObject{
                            {"manufacturer", _deviceInfo.HardwareManufacturer},
                            {"model", _deviceInfo.DeviceModelIdentifier},
                            {"android_release", _deviceInfo.AndroidVer.VersionNumber},
                            {"android_version", int.Parse(_deviceInfo.AndroidVer.APILevel)}
                        }
                    },
                    {"length", Duration},
                    {
                        "extra", new JObject
                        {
                            {"source_width", 0},
                            {"source_height", 0}
                        }
                    },
                    {
                        "clips", new JArray{
                            new JObject
                            {
                                {"length", Duration},
                                {"creation_date", DateTime.Now.ToString("yyyy-dd-MMTh:mm:ss-0fff")},
                                {"source_type", "3"},
                                {"camera_position", "back"}
                            }
                        }
                    },
                    {"audio_muted", false},
                    {"poster_frame_index", 0},
                };


                var _httpRequestProcessor = InstaApi.HttpRequestProcessor;
                var request = InstaApi.HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                request.Headers.Add("retry_context", GetRetryContext());
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                var mediaResponse = JsonConvert.DeserializeObject<InstaMediaItemResponse>(json, new InstaMediaDataConverter());
                var converter = ConvertersFabric.Instance.GetSingleMediaConverter(mediaResponse);
                try
                {
                    var obj = converter.Convert();
                    if (obj != null)
                    {
                        Views.Main.MainView.Current?.MainVM.PostsGenerator.Items.Insert(0, new InstaPost
                        {
                            Media = obj,
                            Type = InstagramApiSharp.Enums.InstaFeedsType.Media
                        });
                        ShowNotify("Your photo uploaded successfully...", 3500);
                        NotificationHelper.ShowNotify(Caption?.Truncate(50), NotifyFile?.Path, "Media uploaded");
                    }
                }
                catch { }
                SendNotify(102);
                //RemoveThis();

                MainPage.Current?.HideMediaUploadingUc();
                return Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                SendNotify(102);
                //RemoveThis();

                MainPage.Current?.HideMediaUploadingUc();
                return Result.Fail(httpException, false, ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                SendNotify(102);
                //RemoveThis();
                MainPage.Current?.HideMediaUploadingUc();
                return Result.Fail<bool>(exception);
            }
        }


        private async Task<IResult<bool>> ConfigureVideoAsync()
        {
            try
            {
                Debug.WriteLine("----------------------------------------ConfigurePhotoAsync----------------------------------");
                var instaUri = GetMediaConfigureUri();
                var retryContext = GetRetryContext();
                var _user = InstaApi.GetLoggedUser();
                var _deviceInfo = InstaApi.GetCurrentDevice();
                var data = new JObject
                {
                    //{"date_time_digitalized", DateTime.UtcNow.ToString("yyyy:MM:dd+hh:mm:ss")},
                    //{"date_time_original", DateTime.UtcNow.ToString("yyyy:MM:dd+hh:mm:ss")},
                    //{"is_suggested_venue", "false"},
                    {"filter_type", "0"},
                    {"timezone_offset", InstaApi.GetTimezoneOffset().ToString()},
                    {"_csrftoken", _user.CsrfToken},
                    {"media_folder", "Camera"},
                    {"source_type", "4"},
                    {"_uid", _user.LoggedInUser.Pk.ToString()},
                    {"_uuid", _deviceInfo.DeviceGuid.ToString()},
                    {"device_id", _deviceInfo.DeviceId},
                    {"caption", Caption ?? string.Empty},
                    {"upload_id", UploadId},
                    {"date_time_original", "19040101T000000.000Z"},
                    {
                        "device", new JObject{
                            {"manufacturer", _deviceInfo.HardwareManufacturer},
                            {"model", _deviceInfo.DeviceModelIdentifier},
                            {"android_release", _deviceInfo.AndroidVer.VersionNumber},
                            {"android_version", int.Parse(_deviceInfo.AndroidVer.APILevel)}
                        }
                    },
                    {"length", Duration},
                    {
                        "extra", new JObject
                        {
                            {"source_width", 0},
                            {"source_height", 0}
                        }
                    },
                    {
                        "clips", new JArray{
                            new JObject
                            {
                                {"length", Duration},
                                //{"creation_date", DateTime.Now.ToString("yyyy-dd-MMTh:mm:ss-0fff")},
                                {"source_type", "4"},
                                //{"camera_position", "back"}
                            }
                        }
                    },
                    {"audio_muted", false},
                    {"poster_frame_index", 0},
                };


                var _httpRequestProcessor = InstaApi.HttpRequestProcessor;
                var request = InstaApi.HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                request.Headers.Add("retry_context", GetRetryContext());
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                var mediaResponse = JsonConvert.DeserializeObject<InstaMediaItemResponse>(json, new InstaMediaDataConverter());
                var converter = ConvertersFabric.Instance.GetSingleMediaConverter(mediaResponse);
                try
                {
                    var obj = converter.Convert();
                    if (obj != null)
                    {
                        Views.Main.MainView.Current?.MainVM.PostsGenerator.Items.Insert(0, new InstaPost
                        {
                            Media = obj,
                            Type = InstagramApiSharp.Enums.InstaFeedsType.Media
                        });
                        ShowNotify("Your photo uploaded successfully...", 3500);
                        NotificationHelper.ShowNotify(Caption?.Truncate(50), NotifyFile?.Path, "Media uploaded");
                    }
                }
                catch { }
                SendNotify(102);
                //RemoveThis();

                MainPage.Current?.HideMediaUploadingUc();
                return Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                SendNotify(102);
                //RemoveThis();

                MainPage.Current?.HideMediaUploadingUc();
                return Result.Fail(httpException, false, ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                SendNotify(102);
                //RemoveThis();
                MainPage.Current?.HideMediaUploadingUc();
                return Result.Fail<bool>(exception);
            }
        }



















        public async Task<IResult<string>> UploadSinglePhoto(InstaImageUpload image,  string uploadId = null, bool album = true, string recipient = null)
        {
            if (string.IsNullOrEmpty(uploadId))
                uploadId = GenerateUploadId();
            var photoHashCode = Path.GetFileName(image.Uri ?? $"C:\\{13.GenerateRandomStringStatic()}.jpg").GetHashCode();
            var photoEntityName = $"{uploadId}_0_{photoHashCode}";
            var photoUri = UriCreator.GetStoryUploadPhotoUri(uploadId, photoHashCode);
            var photoUploadParamsObj = new JObject
            {
                {"upload_id", uploadId},
                {"media_type", "1"},
                {"retry_context", GetRetryContext()},
                {"image_compression", "{\"lib_name\":\"moz\",\"lib_version\":\"3.1.m\",\"quality\":\"95\"}"},
                {"xsharing_user_ids", $"[{recipient ?? string.Empty}]"},
            };
            if (album)
                photoUploadParamsObj.Add("is_sidecar", "1");
            var photoUploadParams = JsonConvert.SerializeObject(photoUploadParamsObj);
            var imageBytes = image.ImageBytes ?? File.ReadAllBytes(image.Uri);
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.Add("Content-Transfer-Encoding", "binary");
            imageContent.Headers.Add("Content-Type", "application/octet-stream");


            var device = InstaApi.GetCurrentDevice();
            var httpRequestProcessor = InstaApi.HttpRequestProcessor;
            var request = InstaApi.HttpHelper.GetDefaultRequest(HttpMethod.Post, photoUri, device);
            request.Content = imageContent;
            request.Headers.Add("X-Entity-Type", "image/jpeg");
            request.Headers.Add("Offset", "0");
            request.Headers.Add("X-Instagram-Rupload-Params", photoUploadParams);
            request.Headers.Add("X-Entity-Name", photoEntityName);
            request.Headers.Add("X-Entity-Length", imageBytes.Length.ToString());
            request.Headers.Add("X_FB_PHOTO_WATERFALL_ID", Guid.NewGuid().ToString());
            var response = await httpRequestProcessor.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Result.Success(uploadId);
            }
            else
            {
                return Result.Fail<string>("NO UPLOAD ID");
            }
        }

        public void LogStatus(string Text) => Debug.WriteLine(Text);
        async void SendNotify(double value)
        {
            try
            {
                //if (MediaShare == null) return;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    //MediaShare.Percentage = value;
                });
            }
            catch { }
        }
    } 
}
