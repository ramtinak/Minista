using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using InstagramApiSharp.API;
using Windows.Foundation.Collections;
using InstagramApiSharp.Helpers;
using InstagramApiSharp.Classes.Models;

namespace Minista.Helpers
{
    static public class NotificationActivationHelper
    {
        public static async void HandleActivation(IInstaApi defaultApi, List<IInstaApi> apiList, string args,
            ValueSet valuePairs, bool wait = false, 
            Action<long> profileAction = null, Action<string> liveAction = null,
            Action<string, InstaUserShortFriendship> threadAction = null)
            =>
            await HandleActivationAsync(defaultApi, apiList, args, valuePairs, wait, profileAction, liveAction, threadAction);

        public static async Task HandleActivationAsync(IInstaApi defaultApi, List<IInstaApi> apiList, string args,
            ValueSet valuePairs, bool wait = false, 
            Action<long> profileAction = null, Action<string> liveAction = null,
            Action<string, InstaUserShortFriendship> threadAction = null)
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
                    var pushCategory = queries["pushCategory"];

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
                        var threadId = queries["id"];
                        var itemId = queries.GetValueIfPossible("x");
                        if (string.IsNullOrEmpty(pushCategory) || pushCategory == "direct_v2_message") // messaging
                        {
                            //textBox : "00000005544666"
                            if (valuePairs?.Count > 0)
                            {
                                var text = valuePairs["textBox"].ToString();

                                await api.MessagingProcessor.SendDirectTextAsync(null, threadId, text.Trim());

                            }
                        }
                        else if(pushCategory == "direct_v2_pending" && queries["action"] is string pendingRequestAction)// pending requests
                        {
                            // Accept   Delete   Block  Dismiss
                            long userPk = await GetUserId(api, sourceUserId, null);
                            if (userPk == -1) return;

                            if (pendingRequestAction == "acceptDirectRequest")
                                await api.MessagingProcessor.ApproveDirectPendingRequestAsync(threadId);
                            else if (pendingRequestAction == "deleteDirectRequest")
                                await api.MessagingProcessor.DeclineDirectPendingRequestsAsync(threadId);
                            else if (pendingRequestAction == "blockDirectRequest")
                            {
                                await api.MessagingProcessor.DeclineDirectPendingRequestsAsync(threadId);
                                await api.UserProcessor.BlockUserAsync(userPk);
                            }
                            else //openPendingThread
                            {
                                var userInfo = await api.UserProcessor.GetUserInfoByIdAsync(userPk);
                                if (!userInfo.Succeeded) return;
                                var u = userInfo.Value;
                                var userShortFriendship = new InstaUserShortFriendship
                                {
                                    UserName = u.UserName,
                                    Pk = u.Pk,
                                    ProfilePicture = u.ProfilePicture,
                                    ProfilePicUrl = u.ProfilePicUrl,
                                    IsPrivate = u.IsPrivate,
                                    IsBestie = u.IsBestie,
                                    IsVerified = u.IsVerified,
                                    FullName = u.FullName,
                                };
                                if (u.FriendshipStatus != null)
                                    userShortFriendship.FriendshipStatus = new InstaFriendshipShortStatus
                                    {
                                        Following = u.FriendshipStatus.Following,
                                        IncomingRequest = u.FriendshipStatus.IncomingRequest,
                                        IsBestie = u.FriendshipStatus.IsBestie,
                                        IsPrivate = u.FriendshipStatus.IsPrivate,
                                        OutgoingRequest = u.FriendshipStatus.OutgoingRequest,
                                        Pk = u.Pk
                                    };
                                else
                                    userShortFriendship.FriendshipStatus = new InstaFriendshipShortStatus
                                    {
                                        Pk = u.Pk
                                    };
                                threadAction?.Invoke(threadId, userShortFriendship);
                            }
                        }
                    }
                    else if (collapsedKey == "private_user_follow_request" && queries["action"] is string followRequestAction)
                    {
                        //user?username=ministaapp
                        // Minista App (@ministaapp) has requested to follow you.
                        //"user?username=rmtjj73&currentUser=44579170833&sourceUserId=14564882672&
                        //collapseKey=private_user_follow_request&action=openProfile"
                        long userPk = await GetUserId(api, sourceUserId, queries["username"]);
                        if (userPk == -1) return;
                        if (followRequestAction == "acceptFriendshipRequest")
                            await api.UserProcessor.AcceptFriendshipRequestAsync(userPk);
                        else if (followRequestAction == "declineFriendshipRequest")
                            await api.UserProcessor.IgnoreFriendshipRequestAsync(userPk);
                        else
                            profileAction?.Invoke(userPk);

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
        static async Task<long> GetUserId(IInstaApi api, string sourceUserId, string username)
        {
            long.TryParse(sourceUserId, out long userPk);
            if (userPk <= 0)
            {
                if (string.IsNullOrEmpty(username)) return -1;
                var userResult = await api.UserProcessor.GetUserAsync(username);
                if (!userResult.Succeeded) return -1;
                userPk = userResult.Value.Pk;
            }
            return userPk;
        }
        static string GetValueIfPossible(this Dictionary<string, string> keyValuePairs, string key)
        {
            if (keyValuePairs.Any(x => x.Key == key))
                return keyValuePairs[key];
            else
                return null;
        }
    }
}
