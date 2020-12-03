using InstagramApiSharp.API;
//using InstagramApiSharp.API.Push;
using MinistaHelper.Push;
using NotifySharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Minista.Helpers
{
    static partial class PushHelper
    {
        //comment
        //direct_v2_message
        //
        public static void HandleNotify(PushNotification push, IReadOnlyList<IInstaApi> apiList)
        {
            push.IgAction += $"&currentUser={push.IntendedRecipientUserId}";
            if (push.CollapseKey == "direct_v2_message")
                GoDirect(push, apiList.GetUserName(push.IntendedRecipientUserId));
            else
                GoLike(push, apiList.GetUserName(push.IntendedRecipientUserId));

            //switch (push.CollapseKey)
            //{
            //    case "direct_v2_message":
            //        GoDirect(push, apiList.GetUserName(push.IntendedRecipientUserId));
            //        return;
            //    case "like":
            //        //GoLike(push, apiList.GetUserName(push.IntendedRecipientUserId));
            //        //return; 
            //    //case "comment": //comment_like
            //        //GoLike(push, apiList.GetUserName(push.IntendedRecipientUserId));
            //        //return;
            //    default:
            //        GoLike(push, apiList.GetUserName(push.IntendedRecipientUserId));
            //        //NotificationHelper.ShowToast(push.Message, push.OptionalAvatarUrl, push.Title ?? "");
            //        return;
            //}
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
                    //if (msg.Contains(" "))
                    //{
                    //    var name = msg.Substring(0, msg.IndexOf(" "));
                    //    var text = msg.Substring(msg.IndexOf(" ") + 1);
                    //    Notify.SendMessageWithoutTextNotify(/*$"[{user}] " + */name, text, img, act);
                    //}
                    //else
                        Notify.SendMessageWithoutTextNotify(null, /*$"[{user}] " + */msg, img, act);
                }
                else
                { 
                    if (msg.Contains(":"))
                    {
                        var name = msg.Substring(0, msg.IndexOf(":"));

                        var text = msg.Substring(msg.IndexOf(":") + 1);
                         
                        Notify.SendMessageNotify(/*$"[{user}] " + */name, text, img, act);
                    }
                    else
                        Notify.SendMessageNotify(null, /*$"[{user}] " +*/ msg, img, act);
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
                //if (msg.Contains(" "))
                //{
                //    var name = msg.Substring(0, msg.IndexOf(" "));
                //    var text = msg.Substring(msg.IndexOf(" ") + 1);
                //    Notify.SendLikeNotify(/*$"[{user}] " + */name, text, img, act);
                //}
                //else
                    Notify.SendLikeNotify(null, /*$"[{user}] "+*/msg, img, act);

            }
            catch { }
        }
    }
    static partial class PushHelper
    {
        private const string BACKGROUND_ACTIVITY_ENTRY_POINT = "MinistaBH.NotifyQuickReplyTask";


        static void Unegister()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                Debug.WriteLine(task.Value.Name);
                switch (task.Value.Name)
                {
                    case BACKGROUND_ACTIVITY_ENTRY_POINT:
                        task.Value.Unregister(true);
                        return;
                }
            }
        }
        public static async void Register()
        {
            try
            {
                Unegister();
                await Task.Delay(250);
                var builder = new BackgroundTaskBuilder
                {
                    Name = BACKGROUND_ACTIVITY_ENTRY_POINT,
                    TaskEntryPoint = BACKGROUND_ACTIVITY_ENTRY_POINT
                };
                builder.SetTrigger(new ToastNotificationActionTrigger());
                builder.Register();
            }
            catch { }
        }
    }
}
