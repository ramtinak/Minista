using InstagramApiSharp.API;
//using InstagramApiSharp.API.Push;
using MinistaHelper.Push;
using NotifySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minista.Helpers
{
    static class PushHelper
    {
        //comment
        //direct_v2_message
        //
        public static void HandleNotify(PushNotification push, IReadOnlyList<IInstaApi> apiList)
        {
            switch(push.CollapseKey)
            {
                case "direct_v2_message":
                    GoDirect(push, apiList.GetUserName(push.IntendedRecipientUserId));
                    return;
                case "like":
                    GoLike(push, apiList.GetUserName(push.IntendedRecipientUserId));
                    return; 
                case "comment": //comment_like
                    GoLike(push, apiList.GetUserName(push.IntendedRecipientUserId));
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
                        Notify.SendMessageWithoutTextNotify(/*$"[{user}] " + */name, text, img, act);
                    }
                    else
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
                if (msg.Contains(" "))
                {
                    var name = msg.Substring(0, msg.IndexOf(" "));
                    var text = msg.Substring(msg.IndexOf(" ") + 1);
                    Notify.SendLikeNotify(/*$"[{user}] " + */name, text, img, act);
                }
                else
                    Notify.SendLikeNotify(null, /*$"[{user}] "+*/msg, img, act);

            }
            catch { }
        }

    }
}
