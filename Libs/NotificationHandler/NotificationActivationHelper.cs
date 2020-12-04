using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using InstagramApiSharp.API;
using Windows.Foundation.Collections;
using InstagramApiSharp.Helpers;

namespace Minista.Helpers
{
    static public class NotificationActivationHelper
    {
        public static async void HandleActivation(IInstaApi defaultApi, List<IInstaApi> apiList, string args,
            ValueSet valuePairs, bool wait = false, Action<long> profileAction = null, Action<string> liveAction = null)
            =>
            await HandleActivationAsync(defaultApi, apiList, args, valuePairs, wait, profileAction, liveAction);

        public static async Task HandleActivationAsync(IInstaApi defaultApi, List<IInstaApi> apiList, string args,
            ValueSet valuePairs, bool wait = false, Action<long> profileAction = null, Action<string> liveAction = null)
        {
            try
            {
                if (wait)
                    await Task.Delay(7500); // Wait for loading information

                if (args == "dismiss=True") return;

                var queries = HttpUtility.ParseQueryString(args, out string type);
                if (queries?.Count > 0)
                {
                    var currentUser = queries["currentUser"];
                    var collapsedKey = queries["collapseKey"];
                    var sourceUserId = queries["sourceUserId"];

                    IInstaApi api;
                    if (apiList?.Count > 1)
                        api = apiList.FirstOrDefault(x => x.GetLoggedUser().LoggedInUser.Pk.ToString() == currentUser);
                    else
                        api = defaultApi;

                    if (api == null) return;

                    //comments_v2?media_id=2437384931159496017_44428109093&target_comment_id=17887778494788574&permalink_enabled=True
                    //direct_v2?id=340282366841710300949128136069129367828&x=29641789960564789887017672389951488
                    //broadcast?id=17861965853258603&reel_id=1647718432&published_time=1606884766
                    //media?id=2455052815714850188_1647718432&media_id=2455052815714850188_1647718432
                    //direct_v2?id=340282366841710300949128136069129367828&x=29641841166106199869991401113518080
                    if (type == "direct_v2")
                    {
                        //direct_v2?id=340282366841710300949128136069129367828&x=29641841166106199869991401113518080
                        //textBox : "00000005544666"
                        var thread = queries["id"];
                        var itemId = queries["x"];

                        if (valuePairs?.Count > 0)
                        {
                            var text = valuePairs["textBox"].ToString();

                            await api.MessagingProcessor.SendDirectTextAsync(null, thread, text.Trim());

                        }
                    }
                    else if (collapsedKey == "private_user_follow_request" && queries["action"] is string followRequestAction)
                    {
                        //user?username=ministaapp
                        // Minista App (@ministaapp) has requested to follow you.
                        //"user?username=rmtjj73&currentUser=44579170833&sourceUserId=14564882672&
                        //collapseKey=private_user_follow_request&action=openProfile"
                        long userPk = -1;
                        long.TryParse(sourceUserId, out userPk);
                        if (userPk <= 0)
                        {
                            var userResult = await api.UserProcessor.GetUserAsync(queries["username"]);
                            if (!userResult.Succeeded) return;
                            userPk = userResult.Value.Pk;
                        }
                        if (followRequestAction == "acceptFriendshipRequest")
                            await api.UserProcessor.AcceptFriendshipRequestAsync(userPk);
                        else if (followRequestAction == "declineFriendshipRequest")
                            await api.UserProcessor.IgnoreFriendshipRequestAsync(userPk);
                        else
                        {
                            profileAction?.Invoke(userPk);
                            //Helper.OpenProfile(userPk); 
                        }

                    }
                    else if (type == "broadcast" && collapsedKey == "live_broadcast")
                    {
                        //broadcast?id=18035667694304049&reel_id=1647718432&published_time=1607056892
                        if (queries["id"] is string broadcastId)
                            liveAction?.Invoke(broadcastId);
                    }
                }
            }
            catch { }
        }
    }
}
