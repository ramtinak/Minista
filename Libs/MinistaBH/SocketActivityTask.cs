using Base;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using Microsoft.Toolkit.Uwp.Notifications;
using Minista.Helpers;
using MinistaHelper;
using MinistaHelper.Push;
using NotifySharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;

namespace MinistaBH
{
    public sealed class SocketActivityTask : IBackgroundTask
    {
        readonly CS CS = new CS();
        BackgroundTaskDeferral deferral;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();

            try
            {
                var details = (SocketActivityTriggerDetails)taskInstance.TriggerDetails;
                Debug.WriteLine($"{details.Reason}");
                var internetProfile = NetworkInformation.GetInternetConnectionProfile();
                if (internetProfile == null)
                {
                    Debug.WriteLine("No internet..");
                    return;
                }
                string selectedUser = null;
                try
                {
                    var obj = ApplicationSettingsHelper.LoadSettingsValue("InstaApiSelectedUsername");
                    if (obj is string str)
                        selectedUser = str;
                }
                catch { }
                await CS.Load();
                A.InstaApiList = CS.InstaApiList;
                var api = !string.IsNullOrEmpty(selectedUser)? 
                    (CS.InstaApiList.FirstOrDefault(x=>x.GetLoggedUser().LoggedInUser.UserName.ToLower() == selectedUser.ToLower()) ?? CS.InstaApiList[0]) : CS.InstaApiList[0];

               
                //foreach (var api in CS.InstaApiList)
                {
                    try
                    {
                        var push = new PushClient(CS.InstaApiList, api)
                        {
                            IsRunningFromBackground = true
                        };
                        push.MessageReceived += A.OnMessageReceived;
                        push.OpenNow();
                        //push.Start();

                        switch (details.Reason)
                        {
                            case SocketActivityTriggerReason.SocketClosed:
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(5));
                                    await push.StartFresh();
                                    break;
                                }
                            default:
                                {
                                    var socket = details.SocketInformation.StreamSocket;
                                    await push.StartWithExistingSocket(socket);
                                    break;
                                }
                        }
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        await push.TransferPushSocket();
                    }
                    catch { }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Debug.WriteLine($"{typeof(SocketActivityTask).FullName}: Can't finish push cycle. Abort.");
            }
            finally
            {
                deferral.Complete();
            }
        }

    }
    internal sealed class A
    {
        public static List<IInstaApi> InstaApiList { get; set; } = new List<IInstaApi>();

        public static void OnMessageReceived(object sender, PushReceivedEventArgs e)
        {
            PushHelper.HandleNotify(e.NotificationContent, InstaApiList);
        }
    }
    internal class CS
    {
        internal static DebugLogger DebugLogger;
        public static IInstaApi BuildApi(string username = null, string password = null)
        {
            UserSessionData sessionData;
            if (string.IsNullOrEmpty(username))
                sessionData = UserSessionData.ForUsername("FAKEUSER").WithPassword("FAKEPASS");
            else
                sessionData = new UserSessionData { UserName = username, Password = password };

            DebugLogger = new DebugLogger(LogLevel.All);
            var api = InstaApiBuilder.CreateBuilder()
                      .SetUser(sessionData)
                      //.SetDevice(new UniversalDevice())
#if DEBUG
                  .UseLogger(DebugLogger)
#endif

                      .Build();
            return api;
        }
        public static readonly StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
        public List<IInstaApi> InstaApiList { get; set; } = new List<IInstaApi>();

       public async Task Load()
        {
            try
            {
                var files = await LocalFolder.GetFilesAsync();
                if (files?.Count > 0)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        var item = files[i];
                        if (item.Path.ToLower().EndsWith(".mises2"))
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
