﻿using InstagramApiSharp.API;
using InstagramApiSharp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
namespace MinistaBH
{
    public sealed class NotifyQuickReplyTask : IBackgroundTask
    {
        readonly CS CS = new CS();

        BackgroundTaskDeferral Deferral;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            Deferral = taskInstance.GetDeferral();
            if (!(taskInstance.TriggerDetails is ToastNotificationActionTriggerDetail details))
            {
                Deferral.Complete();
                //BackgroundTaskStorage.PutError("TriggerDetails was not ToastNotificationActionTriggerDetail.");
                return;
            }

            string arguments = details.Argument;
            if (arguments == "dismiss=True")
            {
                Deferral.Complete();
                return;
            }

            await CS.Load();
            A.InstaApiList = CS.InstaApiList;
            //System.Diagnostics.Debug.WriteLine(arguments);
            //var f = details.UserInput?.FirstOrDefault();
            //if (f == null) return;
            await HandleActivation(arguments, details.UserInput);
            Deferral.Complete();



        }
        async Task HandleActivation(string args, ValueSet valuePairs)
        {
            try
            {
                var queries = HttpUtility.ParseQueryString(args, out string type);
                if (queries?.Count > 0)
                {
                    var currentUser = queries["currentUser"];

                    IInstaApi api;
                    if (CS.InstaApiList.Count == 1)
                        api = CS.InstaApiList[0];
                    else
                    api = CS.InstaApiList.FirstOrDefault(x => x.GetLoggedUser().LoggedInUser.Pk.ToString() == currentUser);

                    if (api == null)
                        return;

                    //comments_v2?media_id=2437384931159496017_44428109093&target_comment_id=17887778494788574&permalink_enabled=True
                    //direct_v2?id=340282366841710300949128136069129367828&x=29641789960564789887017672389951488
                    //broadcast?id=17861965853258603&reel_id=1647718432&published_time=1606884766
                    //media?id=2455052815714850188_1647718432&media_id=2455052815714850188_1647718432
                    //direct_v2?id=340282366841710300949128136069129367828&x=29641841166106199869991401113518080
                    //
                    if (type == "direct_v2")
                    {
                        //direct_v2?id=340282366841710300949128136069129367828&x=29641841166106199869991401113518080
                        //textBox : "00000005544666"
                        var thread = queries["id"];
                        var itemId = queries["x"];

                        if (valuePairs?.Count > 0)
                        {
                            var text = valuePairs["textBox"].ToString();

                            await api.MessagingProcessor.SendDirectTextAsync(null, thread, text);
                        }
                    }
                }
            }
            catch { }
        }

    }

}
