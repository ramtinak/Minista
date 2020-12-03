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
        public static async void HandleActivation(string args, ValueSet valuePairs, bool wait = false)
        {
            try
            {
                if (wait)
                    await Task.Delay(7500); // Wait for loading information

                var queries = HttpUtility.ParseQueryString(args, out string type);
                if (queries?.Count > 0)
                {
                    var currentUser = queries["currentUser"];
                    IInstaApi api;
                    if (Helper.InstaApiList?.Count > 1)
                        api = Helper.InstaApiList.FirstOrDefault(x => x.GetLoggedUser().LoggedInUser.Pk.ToString() == currentUser);
                    else
                        api = Helper.InstaApi;

                    if (api == null) return;
                     
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
