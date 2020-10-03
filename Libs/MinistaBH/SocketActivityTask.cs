using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using Microsoft.Toolkit.Uwp.Notifications;
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
                        push.Start();

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
            //var delay = RequestDelay.FromSeconds(2, 4);
            var api = InstaApiBuilder.CreateBuilder()
                      .SetUser(sessionData)
                  //.SetApiVersion(InstagramApiSharp.Enums.InstaApiVersionType.Version64)
                  //.SetRequestDelay(delay)

#if DEBUG
                  .UseLogger(DebugLogger)
#endif

                      .Build();
            api.SetTimeout(TimeSpan.FromMinutes(2));

            //InstaApi = api;
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
    static class PushHelper
    {
        //comment
        //direct_v2_message
        //
        public static void HandleNotify(PushNotification push, IReadOnlyList<IInstaApi> apiList)
        {
            HandleNotify(push, apiList.GetUserName(push.IntendedRecipientUserId));
        }
        public static void HandleNotify(PushNotification push, string user)
        {
            switch (push.CollapseKey)
            {
                case "direct_v2_message":
                    GoDirect(push, user);
                    return;
                case "like":
                    GoLike(push, user);
                    return;
                case "comment": //comment_like
                    GoLike(push, user);
                    return;
                default:
                    NotificationHelper.ShowToast(push.Message, push.OptionalAvatarUrl, push.Title ?? "");
                    return;
            }
        }
        static void GoDirect(PushNotification push, string user)
        {
            try
            {
                var msg = push.Message;
                var act = push.IgAction;
                var img = push.OptionalAvatarUrl;
                if (msg.Contains("sent you a post") || msg.Contains("sent you a story"))
                {
                    if (msg.Contains(" "))
                    {
                        var name = msg.Substring(0, msg.IndexOf(" "));
                        var text = msg.Substring(msg.IndexOf(" ") + 1);
                        Notify.SendMessageWithoutTextNotify(/*$"[{user}] " + */name, text, img, act, push.OptionalImage);
                    }
                    else
                        Notify.SendMessageWithoutTextNotify(null, /*$"[{user}] " + */msg, img, act, push.OptionalImage);
                }
                else
                {
                    if (msg.Contains(":"))
                    {
                        var name = msg.Substring(0, msg.IndexOf(":"));

                        var text = msg.Substring(msg.IndexOf(":") + 1);

                        Notify.SendMessageNotify(/*$"[{user}] " +*/ name, text, img, act, push.OptionalImage);
                    }
                    else
                        Notify.SendMessageNotify(null,/* $"[{user}] " +*/ msg, img, act, push.OptionalImage);
                }

            }
            catch { }
        }

        static void GoLike(PushNotification push, string user)
        {
            try
            {
                var msg = push.Message;
                var act = push.IgAction;
                var img = push.OptionalAvatarUrl;
                if (msg.Contains(" "))
                {
                    var name = msg.Substring(0, msg.IndexOf(" "));
                    var text = msg.Substring(msg.IndexOf(" ") + 1);
                    Notify.SendLikeNotify(/*$"[{user}] " +*/ name, text, img, act, push.OptionalImage);
                }
                else
                    Notify.SendLikeNotify(null,/* $"[{user}] " + */msg, img, act, push.OptionalImage);

            }
            catch { }
        }
        public static string GetUserName(this IReadOnlyCollection<IInstaApi> apis, string u)
        {
            var user = apis.FirstOrDefault(x => x.GetLoggedUser().LoggedInUser.Pk == long.Parse(u));
            return user?.GetLoggedUser().UserName;
        }
    }
    class NotificationHelper
    {

        public static void ShowSuccessNotify(string title, string thumbnail)
        {
            try
            {
                // And send the notification
                ToastNotificationManager.CreateToastNotifier().Show(GetSuccessNotify(title, thumbnail));
            }
            catch { }
        }
        public static ToastNotification GetSuccessNotify(string title, string thumbnail)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = "Media downloaded"
                                },
                                new AdaptiveText()
                                {
                                    Text = title
                                }
                            },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = thumbnail,
                            HintCrop = ToastGenericAppLogoCrop.Default
                        }
                    }
                },
                //Actions = new ToastActionsCustom()
                //{
                //    Buttons =
                //        {
                //            new ToastButton("Open", "")
                //            {
                //                ActivationType = ToastActivationType.Background
                //            },
                //            new ToastButton("Close", "")
                //            {
                //                ActivationType = ToastActivationType.Foreground
                //            }
                //        }
                //},
                Launch = "action=d7"
            };

            // Create the toast notification
            return new ToastNotification(toastContent.GetXml());
        }

        public static ToastNotification GetFailedNotify(string title, string thumbnail, string text = null)
        {
            return GetSingleNotify(title, thumbnail, "Download failed");
        }

        public static void ShowNotify(string text, string thumbnail, string title = null)
        {
            try
            {
                // And send the notification
                ToastNotificationManager.CreateToastNotifier().Show(GetSingleNotify(text, thumbnail, title));
            }
            catch { }
        }
        public static ToastNotification GetSingleNotify(string text, string thumbnail, string title = null)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title
                            },
                            new AdaptiveText()
                            {
                                Text = text
                            }
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = thumbnail,
                            HintCrop = ToastGenericAppLogoCrop.Default
                        }
                    }
                }
            };

            // Create the toast notification
            return new ToastNotification(toastContent.GetXml());
        }

        public static ToastNotification GetSuccessUserNotify(string title, string thumbnail)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = "User profile downloaded"
                                },
                                new AdaptiveText()
                                {
                                    Text = title
                                }
                            },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = thumbnail,
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        }
                    }
                },
                //Actions = new ToastActionsCustom()
                //{
                //    Buttons =
                //        {
                //            new ToastButton("Open", "")
                //            {
                //                ActivationType = ToastActivationType.Background
                //            },
                //            new ToastButton("Close", "")
                //            {
                //                ActivationType = ToastActivationType.Foreground
                //            }
                //        }
                //},
                Launch = "action=d7"
            };

            // Create the toast notification
            return new ToastNotification(toastContent.GetXml());
        }
        public static void ShowToast(string text, string thumbnail, string title = null, string action = null)
        {
            try
            {
                // And send the notification
                ToastNotificationManager.CreateToastNotifier().Show(GetSingleUserNotify(text, thumbnail, title, action));
            }
            catch { }
        }
        public static ToastNotification GetSingleUserNotify(string text, string thumbnail, string title = null, string action = null)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title
                            },
                            new AdaptiveText()
                            {
                                Text = text
                            }
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = thumbnail,
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        }
                    }
                },Launch = action
            };

            // Create the toast notification
            return new ToastNotification(toastContent.GetXml());
        }

    }

}
