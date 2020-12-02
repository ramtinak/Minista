using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;

namespace NotificationHandler
{
    public static class ApiHelper
    {
        public static readonly StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
        public static List<IInstaApi> InstaApiList { get; set; } = new List<IInstaApi>();
        internal static DebugLogger DebugLogger;
        public static IInstaApi BuildApi(string username = null, string password = null)
        {
            UserSessionData sessionData= UserSessionData.ForUsername("FAKEUSER").WithPassword("FAKEPASS");
            var api = InstaApiBuilder.CreateBuilder()
                      .SetUser(sessionData)

#if DEBUG
                  .UseLogger(new DebugLogger(LogLevel.All))
#endif

                      .Build();
            api.SetTimeout(TimeSpan.FromMinutes(1));
            return api;
        }

        public static async Task Load()
        {
            try
            {
                var files = await LocalFolder.GetFilesAsync();
                if (files?.Count > 0)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        var item = files[i];
                        if (item.Path.ToLower().EndsWith(".mises"))
                        {
                            try
                            {
                                var json = await FileIO.ReadTextAsync(item);
                                if (!string.IsNullOrEmpty(json))
                                {
                                    var content = CryptoHelper.Decrypt(json);
                                    var api = BuildApi();
                                    await api.LoadStateDataFromStringAsync(content);
                                    InstaApiList.Add(api);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
