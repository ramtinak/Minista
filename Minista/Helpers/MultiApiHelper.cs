using InstagramApiSharp.API;
using Minista.Views.Main;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
namespace Minista.Helpers
{
    static class MultiApiHelper
    {
        public static List<MinistaHelper.Push.PushClient> Pushs = new List<MinistaHelper.Push.PushClient>();
        public static /*async*/ void SetupPushNotification(IReadOnlyList<IInstaApi> apiList)
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
                                p.LogReceived += MainView.OnLogReceived;
                                p.OpenNow();
                                api.PushClient = p;

                                api.PushClient.Start();

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
            if (Helper.DontUseTimersAndOtherStuff) return;
            SessionHelper.SaveCurrentSession();
        }

        private static void P_LogReceived(object sender, object e)
        {
        }

        static void PushClientMessageReceived(object sender, PushReceivedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e?.Json);
            if (e != null)
            System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(ToPn2(e.NotificationContent)));
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
        }
        static PushNotification2 ToPn2(PushNotification p)
        {
            PushNotification2 notification = new PushNotification2
            {
                Sound = p.Sound,
                SourceUserId = p.SourceUserId,
                BadgeCount = new BadgeCount2
                {
                    Activities = p.BadgeCount.Activities,
                    Direct = p.BadgeCount.Direct,
                    Ds = p.BadgeCount.Ds
                },
                CollapseKey =p.CollapseKey,
                PushCategory = p.PushCategory,
                IgAction = p.IgAction,
                IgActionOverride = p.IgActionOverride,
                InAppActors = p.InAppActors,
                IntendedRecipientUserId = p.IntendedRecipientUserId,
                Message = p.Message,
                OptionalAvatarUrl = p.OptionalAvatarUrl,
                OptionalImage = p.OptionalImage,
                PushId = p.PushId,
                TickerText = p.TickerText,
                Title = p.Title,
               
            };


            return notification;
        }
    }
    public struct PushNotification2
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string TickerText { get; set; }
        public string IgAction { get; set; }
        public string CollapseKey { get; set; }
        public string OptionalImage { get; set; }
        public string OptionalAvatarUrl { get; set; }
        public string Sound { get; set; }
        public string PushId { get; set; }
        public string PushCategory { get; set; }
        public string IntendedRecipientUserId { get; set; }
        public string SourceUserId { get; set; }
        public string IgActionOverride { get; set; }
        public BadgeCount2 BadgeCount { get; set; }
        public string InAppActors { get; set; }
    }
    public struct BadgeCount2
    {
        public int Direct { get; set; }
        public int Ds { get; set; }
        public int Activities { get; set; }
    }
}
