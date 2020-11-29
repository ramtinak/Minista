using InstagramApiSharp.API;
using Minista.Views.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace Minista.Helpers
{
    static class MultiApiHelper
    {
        public static List<MinistaHelper.Push.PushClient> Pushs = new List<MinistaHelper.Push.PushClient>();
        public static async void SetupPushNotification(IReadOnlyList<IInstaApi> apiList)
        { 
            try
            {
                var helpers = Helper.InstaApiList;//???
                if (apiList.Count > 0)
                {
                    //var api = apiList[0];
                    var currentPK = Helper.InstaApi.GetLoggedUser().LoggedInUser.Pk;
                    //foreach (var api in apiList)
                    //{
                    //    // shutdown mqtt
                    //    if (api.PushClient != null)
                    //    {
                    //        try
                    //        {
                    //            api.PushClient.Shutdown();
                    //            await Task.Delay(50);
                    //            api.PushClient = null;
                    //        }
                    //        catch { }
                    //    }
                    //}
                    //var canbeMultiple = apiList.Any(x => x.GetCurrentDevice().DeviceGuid.ToString() != Helper.InstaApi.GetCurrentDevice().DeviceGuid.ToString());
                    //if (!canbeMultiple)
                    //{
                    //    try
                    //    {
                    //        var api = Helper.InstaApi ?? apiList[0];
                    //        var p = new MinistaHelper.Push.PushClient(apiList.ToList(), api);
                    //        p.ValidateData();
                    //        p.FbnsTokenChanged += P_FbnsTokenChanged;
                    //        p.MessageReceived += PushClientMessageReceived;
                    //        p.LogReceived += P_LogReceived;
                    //        if (api.GetLoggedUser().LoggedInUser.Pk != currentPK)
                    //            p.DontTransferSocket = true;
                    //        p.OpenNow();
                    //        api.PushClient = p;

                    //        api.PushClient.Start();

                    //    }
                    //    catch { }
                    //}
                    //else
                    {
                        //foreach (var api in apiList)
                        // open only for one account!
                        var api = Helper.InstaApi ?? apiList[0];
                        {
                            try
                            {
                                var p = new MinistaHelper.Push.PushClient(apiList.ToList(), api);
                                p.ValidateData();
                                p.FbnsTokenChanged += P_FbnsTokenChanged;
                                p.MessageReceived += PushClientMessageReceived;
                                p.LogReceived += P_LogReceived;
                                //if (api.GetLoggedUser().LoggedInUser.Pk != currentPK)
                                //    p.DontTransferSocket = true;
                                p.OpenNow();
                                api.PushClient = p;

                                api.PushClient.Start();
                                //try
                                //{
                                //    await api.PushClient.Shutdown();
                                //}
                                //catch { }
                                //try
                                //{
                                //    api.PushClient.MessageReceived -= PushClientMessageReceived;
                                //}
                                //catch { }
                                //api.PushClient.MessageReceived += PushClientMessageReceived;
                                //await api.PushClient.Start();

                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }

        private static void P_FbnsTokenChanged(object sender, object e)
        {
            SessionHelper.SaveCurrentSession();
        }

        private static /*async*/ void P_LogReceived(object sender, object e)
        {
            try
            {
                //await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                //{
                //    if (MainView.Current != null)
                //    {
                //        if (MainView.Current.DebugText != null)
                //        {
                //            if (e is string str)
                //                MainView.Current.DebugText.Text += str + Environment.NewLine;
                //            else if (e is Exception exception)
                //                MainView.Current.DebugText.Text += exception.PrintException() + Environment.NewLine;

                //        }
                //    }

                //});
            }
            catch { }
        }

        static void PushClientMessageReceived(object sender, PushReceivedEventArgs e)
        {
            //if (MainView.Current != null)
            //    if (MainView.Current.DebugText != null)
            //        MainView.Current.DebugText.Text += "NOTIFY RECEIVED :>   " + JsonConvert.SerializeObject(e) + Environment.NewLine;
            System.Diagnostics.Debug.WriteLine(e?.Json);
            if (NavigationService.IsDirect())
            {
                try
                {
                    if (sender is IInstaApi api && Helper.CurrentUser.Pk == api.GetLoggedUser().LoggedInUser.Pk)
                    {
                        if (!e.CollapseKey.Contains("direct"))
                            PushHelper.HandleNotify(e.NotificationContent, Helper.InstaApiList);
                    }
                    else
                        PushHelper.HandleNotify(e.NotificationContent, Helper.InstaApiList);
                }
                catch { PushHelper.HandleNotify(e.NotificationContent, Helper.InstaApiList); }
            }
            else
                PushHelper.HandleNotify(e.NotificationContent, Helper.InstaApiList);
            //Helpers.NotificationHelper.ShowToast(e.NotificationContent.Message, e.NotificationContent.OptionalAvatarUrl, e.NotificationContent.Title ?? "");
        }
    }
}
