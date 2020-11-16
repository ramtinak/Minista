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
using Windows.Media.Core;
using FFmpegInterop;
using Minista.Helpers;
using InstagramApiSharp;
using Windows.Storage.Streams;
using InstagramApiSharp.API;

namespace Minista.Views.Uploads
{
    public delegate void UploadHandler(Uploader sender);
    public class StorageUpload
    {
        public List<Uploader> Uploads { get; set; } = new List<Uploader>();
        public List<Uploader> CompletedUploads { get; set; } = new List<Uploader>();

        public void SetUploadItem(List<StorageUploadItem> uploadItems, string caption)
        {
            try
            {
                if (!string.IsNullOrEmpty(caption))
                    caption = caption.Replace("\r", "\n");
                if (uploadItems.Count == 0) return;
                for (int i = 0; i < uploadItems.Count; i++)
                    uploadItems[i].Caption = caption;

                if (uploadItems.Count == 1)
                {
                    var uploader = new Uploader();
                    uploader.UploadFile(uploadItems[0]);
                }
                else
                {
                    for (int i = 0; i < uploadItems.Count; i++)
                    {
                        uploadItems[i].IsAlbum = true;

                       var up = uploadItems[i];
                        var uploader = new Uploader();
                        uploader.OnUploadCompleted += StorageUploadOnUploadCompleted;
                        uploader.UploadFile(up);
                        Uploads.Add(uploader);
                    }

                }

                AppUploadHelper.Uploaders.Add(this);

            }
            catch { }
        }
        private void StorageUploadOnUploadCompleted(Uploader sender)
        {
            CompletedUploads.Add(sender);

            if (Uploads.Count > 0)
                if (Uploads.Count == CompletedUploads.Count)
                    //configure album
                    ConfigureAlbum();
        }

        private async void ConfigureAlbum()
        {
            try
            {
                MainPage.Current?.ShowMediaUploadingUc();
                var instaUri = InstagramApiSharp.Helpers.UriCreator.GetMediaAlbumConfigureUri();
                var clientSidecarId = Uploader.GenerateUploadId();
                var childrenArray = new JArray();

                foreach (var album in Uploads)
                {
                    childrenArray.Add(album.GetMediaConfigure());
                }
                var retryContext = Uploader.GetRetryContext();
                var _user = InstaApi.GetLoggedUser();
                var _deviceInfo = InstaApi.GetCurrentDevice();
                var _httpRequestProcessor = InstaApi.HttpRequestProcessor;
                var data = new JObject
                {
                    {"_uuid", _deviceInfo.DeviceGuid.ToString()},
                    {"_uid", _user.LoggedInUser.Pk.ToString()},
                    {"_csrftoken", _user.CsrfToken},
                    {"caption", Uploads[0].UploadItem.Caption?? ""},
                    {"client_sidecar_id", clientSidecarId},
                    {"upload_id", clientSidecarId},
                    {"timezone_offset", InstaApi.GetTimezoneOffset().ToString()},
                    {"source_type", "4"},
                    {"device_id", _deviceInfo.DeviceId},
                    {"creation_logger_session_id", Guid.NewGuid().ToString()},
                    {
                        "device", new JObject
                        {
                            {"manufacturer", _deviceInfo.HardwareManufacturer},
                            {"model", _deviceInfo.DeviceModelIdentifier},
                            {"android_release", _deviceInfo.AndroidVer.VersionNumber},
                            {"android_version", int.Parse(_deviceInfo.AndroidVer.APILevel)}
                        }
                    },
                    {"children_metadata", childrenArray},
                };
                if (Uploads[0].UploadItem.DisableComments)
                    data.Add("disable_comments", "1");
                if (Uploads[0].UploadItem.Location != null)
                {
                    data.Add("location", Uploads[0].UploadItem.Location.GetJson());
                    data.Add("date_time_digitalized", DateTime.Now.ToString("yyyy:dd:MM+h:mm:ss"));
                }
                var request = InstaApi.HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                MainPage.Current?.HideMediaUploadingUc();

                var mediaResponse = JsonConvert.DeserializeObject<InstaMediaAlbumResponse>(json);
                var converter = ConvertersFabric.Instance.GetSingleMediaFromAlbumConverter(mediaResponse);
                try
                {
                    var obj = converter.Convert();
                    if (obj != null)
                    {
                        Main.MainView.Current?.MainVM.PostsGenerator.Items.Insert(0, new InstaPost
                        {
                            Media = obj,
                            Type = InstagramApiSharp.Enums.InstaFeedsType.Media
                        });
                        ShowNotify($"Your Album uploaded successfully...", 3500);
                        var up = Uploads[0];
                        NotificationHelper.ShowNotify(up.UploadItem.Caption?.Truncate(50), up.NotifyFile?.Path, "Album uploaded");
                    }
                }
                catch { }
                //RemoveThis();

                MainPage.Current?.HideMediaUploadingUc();
            }
            catch (Exception ex){ ex.PrintException("ConfigureAlbum"); }
        }




    }
    public class Uploader
    {
        public event UploadHandler OnUploadCompleted;
        public event UploadHandler OnFailed;
        public event UploadHandler OnSuccess;
        public string Name { get; private set; }
        internal static readonly Random Rnd = new Random();
        CancellationTokenSource cts;

        public string UploadId { get; private set; }
        public StorageFile NotifyFile;
        public StorageUploadItem UploadItem { get; private set; }
        public Uploader()
        {
            Name = Guid.NewGuid().ToString();
        }

        public async void UploadFile(StorageUploadItem uploadItem)
        {
            MainPage.Current?.ShowMediaUploadingUc();
            UploadId = GenerateUploadId(true);
            uploadItem.UploadId = UploadId;
            UploadItem = uploadItem;
            try
            {
                var cacheFolder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
                NotifyFile = await cacheFolder.CreateFileAsync(15.GenerateRandomStringStatic() + ".jpg");
                NotifyFile = await UploadItem.ImageToUpload.CopyAsync(cacheFolder);
            }
            catch { }
            var hashCode = Path.GetFileName($"C:\\{13.GenerateRandomStringStatic()}.jpg").GetHashCode();
            var storyHashId = GenerateStoryHashId();
            var storyHash = InstagramApiSharp.Helpers.CryptoHelper.CalculateHash(InstaApi.GetApiVersionInfo().SignatureKey, storyHashId).Substring(0 , "763975004617a2a82570956fc58340b8".Length);

            var entityName = UploadItem.IsStory ? $"{storyHash}-0-{storyHashId}" : $"{UploadId}_0_{hashCode}";
            var instaUri = UploadItem.IsVideo ? (UploadItem.IsStory ? GetUploadVideoStoryUri(entityName) : GetUploadVideoUri(UploadId, hashCode)) : GetUploadPhotoUri(UploadId, hashCode);
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


            JObject uploadParamsObj = new JObject
            {
                {"upload_id", UploadId},
                {"media_type", "1"},
                {"retry_context", GetRetryContext()},
                {"image_compression", "{\"lib_name\":\"moz\",\"lib_version\":\"3.1.m\",\"quality\":\"95\"}"},
            };
            if (UploadItem.IsVideo)
            {
                uploadParamsObj = new JObject
                {
                    {"upload_media_height", UploadItem.PixelHeight},
                    {"upload_media_width",UploadItem.PixelWidth},
                    {"upload_media_duration_ms", UploadItem.Duration.TotalMilliseconds},
                    {"upload_id", UploadId},
                    {"retry_context", GetRetryContext()},
                    {"media_type", "2"},
                    {"xsharing_user_ids", "[]"},
                    {"extract_cover_frame", "1"}
                };

                if (UploadItem.IsStory)
                    uploadParamsObj.Add("for_album", "1");
            }
            if(UploadItem.IsAlbum)
                uploadParamsObj.Add("is_sidecar", "1");
            var uploadParams = JsonConvert.SerializeObject(uploadParamsObj);
            var fileBytes = "";
            try
            {
                StorageFile file = UploadItem.IsVideo ? UploadItem.VideoToUpload : UploadItem.ImageToUpload;
                using (var openedFile = await file.OpenAsync(FileAccessMode.Read))
                {
                    fileBytes = openedFile.AsStream().Length.ToString();
                    await Task.Delay(250);
                }
            }
            catch { }
            BGU.SetRequestHeader("X-Entity-Type", UploadItem.IsVideo ? "video/mp4" : "image/jpeg");
            if (UploadItem.IsStory)
            {
                BGU.SetRequestHeader("Segment-Start-Offset", "0");
                BGU.SetRequestHeader("Segment-Type", "3");
            }
            BGU.SetRequestHeader("Offset", "0");
            BGU.SetRequestHeader("X-Instagram-Rupload-Params", uploadParams);
            BGU.SetRequestHeader("X-Entity-Name", entityName);
            BGU.SetRequestHeader("X-Entity-Length", fileBytes);
            BGU.SetRequestHeader($"X_FB_{(UploadItem.IsVideo ? "VIDEO" : "PHOTO")}_WATERFALL_ID",UploadItem.IsStory ? UploadId : Guid.NewGuid().ToString());
            BGU.SetRequestHeader("Content-Transfer-Encoding", "binary");
            BGU.SetRequestHeader("Content-Type", "application/octet-stream");
            Debug.WriteLine("----------------------------------------Start upload----------------------------------");

            var upload = BGU.CreateUpload(instaUri, UploadItem.IsVideo ? UploadItem.VideoToUpload : UploadItem.ImageToUpload);
            upload.Priority = BackgroundTransferPriority.Default;
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
                    await upload.StartAsync().AsTask(cts.Token, progressCallback);
                else
                    await upload.AttachAsync().AsTask(cts.Token, progressCallback);

                ResponseInformation response = upload.GetResponseInformation();

                LogStatus(string.Format("Completed: {0}, Status Code: {1}", upload.Guid, response.StatusCode));
            }
            catch (TaskCanceledException)
            {
                OnFailed?.Invoke(this);
                MainPage.Current?.HideMediaUploadingUc();
                LogStatus("Canceled: " + upload.Guid);
            }
            catch (Exception ex)
            {
                OnFailed?.Invoke(this);
                ex.PrintException("HandleUploadAsync");
                MainPage.Current?.HideMediaUploadingUc();
            }
        }
        async void UploadProgress(UploadOperation upload)
        {
            LogStatus(string.Format("Progress: {0}, Status: {1}", upload.Guid, upload.Progress.Status));
            MainPage.Current?.ShowMediaUploadingUc();

            BackgroundUploadProgress progress = upload.Progress;

            double percentSent = 100;
            if (progress.TotalBytesToSend > 0)
                percentSent = progress.BytesSent * 100 / progress.TotalBytesToSend;
            SendNotify(percentSent);
            LogStatus(string.Format(" - Sent bytes: {0} of {1} ({2}%), Received bytes: {3} of {4}",
                progress.BytesSent, progress.TotalBytesToSend, percentSent,
                progress.BytesReceived, progress.TotalBytesToReceive));

            if (progress.HasRestarted)
                LogStatus(" - Upload restarted");

            if (progress.HasResponseChanged)
            {
                var resp = upload.GetResponseInformation();
                LogStatus(" - Response updated; Header count: " + resp.Headers.Count);
                var response = upload.GetResultStreamAt(0);
                StreamReader stream = new StreamReader(response.AsStreamForRead());
                Debug.WriteLine("----------------------------------------Response from upload----------------------------------");

                var st = stream.ReadToEnd();
                Debug.WriteLine(st);
                if (UploadItem.IsVideo)
                {
                    if (!UploadItem.IsStory)
                    {
                        var thumbStream = (await UploadItem.ImageToUpload.OpenAsync(FileAccessMode.Read)).AsStream();
                        var bytes = await thumbStream.ToByteArray();
                        var img = new InstaImageUpload
                        {
                            ImageBytes = bytes,
                            Uri = UploadItem.ImageToUpload.Path
                        };
                        await UploadSinglePhoto(img, UploadId);
                    }
                    await Task.Delay(15000);
                    if (!UploadItem.IsStory)
                        await FinishVideoAsync();
                    await Task.Delay(1500);
                }
                if (!UploadItem.IsAlbum)
                    await ConfigurMediaAsync();
                else
                    OnUploadCompleted?.Invoke(this);
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
                    {"filter_type", "0"},
                    {"timezone_offset", InstaApi.GetTimezoneOffset().ToString()},
                    {"_csrftoken", _user.CsrfToken},
                    {"source_type", "4"},
                    {"_uid", _user.LoggedInUser.Pk.ToString()},
                    {"device_id", _deviceInfo.DeviceId},
                    {"_uuid", _deviceInfo.DeviceGuid.ToString()},
                    {"caption", UploadItem.Caption ?? string.Empty},
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
                    {"length", UploadItem.Duration},
                    {
                        "extra", new JObject
                        {
                            {"source_width", UploadItem.PixelWidth},
                            {"source_height", UploadItem.PixelHeight}
                        }
                    },
                    {
                        "clips", new JArray{
                            new JObject
                            {
                                {"length", UploadItem.Duration},
                                //{"creation_date", DateTime.Now.ToString("yyyy-dd-MMTh:mm:ss-0fff")},
                                {"source_type", "3"},
                                //{"camera_position", "back"}
                            }
                        }
                    },
                    {"audio_muted", UploadItem.AudioMuted},
                    {"poster_frame_index", 0},
                };
                if (UploadItem.DisableComments)
                    data.Add("disable_comments", "1");
                var _httpRequestProcessor = InstaApi.HttpRequestProcessor;
                var request = InstaApi.HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                request.Headers.Add("retry_context", GetRetryContext());
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                var mediaResponse = JsonConvert.DeserializeObject<InstaMediaItemResponse>(json, new InstaMediaDataConverter());
                var converter = ConvertersFabric.Instance.GetSingleMediaConverter(mediaResponse);
                try
                {
                    //var obj = converter.Convert();
                    //if (obj != null)
                    //{
                    //    Main.MainView.Current?.MainVM.PostsGenerator.Items.Insert(0, new InstaPost
                    //    {
                    //        Media = obj,
                    //        Type = InstagramApiSharp.Enums.InstaFeedsType.Media
                    //    });
                    //    //ShowNotify("Your photo uploaded successfully...", 3500);
                    //    NotificationHelper.ShowNotify(UploadItem.Caption?.Truncate(50), NotifyFile?.Path, "Media uploaded");
                    //}
                }
                catch { }
                SendNotify(102);

                return Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                SendNotify(102);

                return Result.Fail(httpException, false, ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                SendNotify(102);
                return Result.Fail<bool>(exception);
            }
        }

        private async Task<IResult<bool>> ConfigurMediaAsync()
        {
            try
            {
                Debug.WriteLine($"----------------------------------------Configure{(UploadItem.IsVideo ? "Video" : "Photo")}{(UploadItem.IsStory ? " to Story": "")} Async----------------------------------");
                var instaUri = GetMediaConfigureUri(UploadItem.IsVideo, UploadItem.IsStory);
                var retryContext = GetRetryContext();
                var _user = InstaApi.GetLoggedUser();
                var _deviceInfo = InstaApi.GetCurrentDevice();
                var data = GetMediaConfigure();

                var _httpRequestProcessor = InstaApi.HttpRequestProcessor;
                var request = InstaApi.HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                request.Headers.Add("retry_context", GetRetryContext());
                var response = await _httpRequestProcessor.SendAsync(request);
                MainPage.Current?.HideMediaUploadingUc();

                var json = await response.Content.ReadAsStringAsync();
                var mediaResponse = JsonConvert.DeserializeObject<InstaMediaItemResponse>(json, new InstaMediaDataConverter());
                var converter = ConvertersFabric.Instance.GetSingleMediaConverter(mediaResponse);

                try
                {
                    var obj = converter.Convert();
                    if (obj != null)
                    {
                        if (UploadItem.IsStory)
                            Main.MainView.Current?.MainVM.RefreshStories(true);
                        else
                        {
                            Main.MainView.Current?.MainVM.PostsGenerator.Items.Insert(0, new InstaPost
                            {
                                Media = obj,
                                Type = InstagramApiSharp.Enums.InstaFeedsType.Media
                            });
                        }
                        ShowNotify($"Your {(UploadItem.IsStory ? "Story" : (UploadItem.IsVideo ? "Video" : "Photo"))} uploaded successfully...", 3500);
                        NotificationHelper.ShowNotify(UploadItem.Caption?.Truncate(50), NotifyFile?.Path,( UploadItem.IsStory ? "Story" :"Media")+" uploaded");
                    }
                }
                catch { }
                SendNotify(102);
                //RemoveThis();
                OnSuccess?.Invoke(this);
                MainPage.Current?.HideMediaUploadingUc();
                return Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                SendNotify(102);
                //RemoveThis();

                OnFailed?.Invoke(this);
                MainPage.Current?.HideMediaUploadingUc();
                return Result.Fail(httpException, false, ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                OnFailed?.Invoke(this);
                SendNotify(102);
                //RemoveThis();
                MainPage.Current?.HideMediaUploadingUc();
                return Result.Fail<bool>(exception);
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
        void RemoveThis()
        {
            try
            {
                //var single = AppUploadHelper.Uploaders.FirstOrDefault(s => s.Name == Name);
                //if (single != null)
                //    AppUploadHelper.Uploaders.Remove(single);
            }
            catch { }
        }
        #region Get configure token
        public JObject GetMediaConfigure()
        {
            var _user = InstaApi.GetLoggedUser();
            var _deviceInfo = InstaApi.GetCurrentDevice();
            var caption = UploadItem.Caption ?? string.Empty;
            if (!string.IsNullOrEmpty(caption)) caption = caption.Replace('\r', '\n');
            var data = new JObject
            {
                {"timezone_offset", InstaApi.GetTimezoneOffset().ToString()},
                {"_csrftoken", _user.CsrfToken},
                {"media_folder", "Camera"},
                {"source_type", /*UploadItem.IsVideo ? "4" :*/ "3"},
                {"_uid", _user.LoggedInUser.Pk.ToString()},
                {"_uuid", _deviceInfo.DeviceGuid.ToString()},
                {"device_id", _deviceInfo.DeviceId},
                {"caption", caption},
                {"upload_id", UploadId},
                {
                    "device", new JObject{
                        {"manufacturer", _deviceInfo.HardwareManufacturer},
                        {"model", _deviceInfo.DeviceModelIdentifier},
                        {"android_release", _deviceInfo.AndroidVer.VersionNumber},
                        {"android_version", int.Parse(_deviceInfo.AndroidVer.APILevel)}
                    }
                },
                {
                    "extra", new JObject
                    {
                        {"source_width", UploadItem.PixelWidth},
                        {"source_height", UploadItem.PixelHeight}
                    }
                },
            };
            if (UploadItem.DisableComments)
                data.Add("disable_comments", "1");

            if (UploadItem.IsStory)
            {
                data.Add(InstaApiConstants.SUPPORTED_CAPABALITIES_HEADER, InstaApiConstants.SupportedCapabalities.ToString(Formatting.None));
                data.Add("allow_multi_configures", "1");
                data.Add("configure_mode", "1");
                data.Add("audience", "default");
            }
            if (UploadItem.IsVideo)
            {
                data.Add("filter_type", "0");
                data.Add("length", UploadItem.Duration.TotalSeconds);
                data.Add("clips", new JArray{
                            new JObject
                            {
                                {"length",  UploadItem.Duration.TotalSeconds},
                                {"source_type", "3"},
                            }
                        });
                if (UploadItem.IsStory)
                {
                    data.Add("has_original_sound", "1");
                    data.Add("video_result", "");
                }
                data.Add("audio_muted", UploadItem.AudioMuted);
                data.Add("poster_frame_index", 0);
            }
            if (UploadItem.Location != null && !UploadItem.IsAlbum)
            {
                data.Add("location", UploadItem.Location.GetJson());
                data.Add("date_time_digitalized", DateTime.UtcNow.ToString("yyyy:dd:MM+h:mm:ss"));
            }
            if (UploadItem.UserTags?.Count > 0)
            {
                var tagArr = new JArray();
                var isVid = UploadItem.IsVideo;
                foreach (var tag in UploadItem.UserTags)
                {
                    if (tag.Pk != -1)
                    {
                        var position = new JArray(isVid ? 0.0 : tag.X, isVid ? 0.0 : tag.Y);
                        var singleTag = new JObject
                            {
                                {"user_id", tag.Pk},
                                {"position", position}
                            };
                        tagArr.Add(singleTag);
                    }
                }

                var root = new JObject
                    {
                        {"in", tagArr}
                    };
                data.Add("usertags", root.ToString(Formatting.None));
            }
            return data;
        }
        #endregion
        #region Upload Image
        async Task<IResult<string>> UploadSinglePhoto(InstaImageUpload image, string uploadId = null, string recipient = null)
        {
            if (string.IsNullOrEmpty(uploadId))
                uploadId = GenerateUploadId();
            var photoHashCode = Path.GetFileName(image.Uri ?? $"C:\\{13.GenerateRandomStringStatic()}.jpg").GetHashCode();
            var photoEntityName = $"{uploadId}_0_{photoHashCode}";
            var photoUri = GetUploadPhotoUri(uploadId, photoHashCode);
            var photoUploadParamsObj = new JObject
            {
                {"upload_id", uploadId},
                {"media_type", "1"},
                {"retry_context", GetRetryContext()},
                {"image_compression", "{\"lib_name\":\"moz\",\"lib_version\":\"3.1.m\",\"quality\":\"95\"}"},
                {"xsharing_user_ids", $"[{recipient ?? string.Empty}]"},
            };
            if (UploadItem.IsAlbum)
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
            request.Headers.Add("X_FB_PHOTO_WATERFALL_ID", UploadItem.IsStory ? UploadId : Guid.NewGuid().ToString());
            var response = await httpRequestProcessor.SendAsync(request);
           /* var json =*/ await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Result.Success(uploadId);
            }
            else
            {
                return Result.Fail<string>("NO UPLOAD ID");
            }
        }

        #endregion
        #region Uri Helpers


        static public string GenerateUploadId(bool story = false)
        {
            //173258081688145
            string r = story ? "18" : "37";
            for (int i = 0; i < 15; i++)
                r += Rnd.Next(0, 9).ToString();
            return r;
        }

        static public string GenerateStoryHashId()
        { 
            //752599
            string r ="7";
            for (int i = 0; i < 5; i++)
                r += Rnd.Next(0, 9).ToString();
            return r;
        }
        static Uri GetMediaConfigureUri(bool isVideo = false, bool isStory = false)
        {
            return new Uri("https://i.instagram.com/api/v1/media/" + (isStory ? "configure_to_story/" : "configure/") + (isVideo ? "?video=1" : ""), UriKind.RelativeOrAbsolute);
        }
        static Uri  GetUploadVideoUri(string uploadId, int fileHashCode)
        {
            return new Uri(string.Format("https://i.instagram.com/rupload_igvideo/{0}_0_{1}", uploadId, fileHashCode), UriKind.RelativeOrAbsolute);
        }
        static Uri GetUploadVideoStoryUri(string uploadId)
        {
            return new Uri($"https://i.instagram.com/rupload_igvideo/{uploadId}", UriKind.RelativeOrAbsolute);
        }
        static Uri GetUploadPhotoUri(string uploadId, int fileHashCode)
        {
            return new Uri(string.Format("https://i.instagram.com/rupload_igphoto/{0}_0_{1}", uploadId, fileHashCode), UriKind.RelativeOrAbsolute);
        }

        static internal string GetRetryContext()
        {
            return new JObject
                {
                    {"num_step_auto_retry", 0},
                    {"num_reupload", 0},
                    {"num_step_manual_retry", 0}
                }.ToString(Formatting.None);
        }


        #endregion
    }
}