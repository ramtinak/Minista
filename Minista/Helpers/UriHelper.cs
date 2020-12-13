using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minista.Helpers
{
    internal static class UriHelper
    {
        static readonly string IgUrl = "instagram.com";

        // Post/TV/Reel media is same so I will act as a single post
        static readonly string IgPost = $"{IgUrl}/p/";
        static readonly string IgTV = $"{IgUrl}/tv/";
        static readonly string IgReel = $"{IgUrl}/reel/";


        static readonly string IgStories = $"{IgUrl}/stories/";

        public static async void HandleUri(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            try
            {
                if (url.Contains(IgUrl))
                {
                    var n = url.Substring(url.IndexOf($"{IgUrl}/") + $"{IgUrl}/".Length);
                    if (n.Contains("&"))
                        n = n.Substring(0, n.IndexOf("&"));
                    if (n.Contains("?"))
                        n = n.Substring(0, n.IndexOf("?"));
                    // https://www.instagram.com/tv/Bzpc-iegMfH/?igshid=1fmtxf8nxvjq6
                    // https://www.instagram.com/p/Bz7dCO2hgYb/?igshid=1m1qa1waibwed
                    // https://instagram.com/stories/almenara.s/2088060456748019118?igshid=l6akfmgft718
                    if (url.ToLower().Contains(IgPost) || url.ToLower().Contains(IgTV)
                        || url.ToLower().Contains(IgReel))
                    {
                        if (url.Contains("/reel/"))
                            url = url.Replace("/reel/", "/p/");
                        NavigationService.Navigate(typeof(Views.Posts.SinglePostView), url);
                    }
                    else if (url.ToLower().Contains(IgStories))
                    {
                        // https://instagram.com/stories/almenara.s/2088060456748019118?igshid=l6akfmgft718
                        var u = url.Substring(url.IndexOf(IgStories) + IgStories.Length);
                        var user = u.Substring(0, u.IndexOf("/")).Replace("/", "");
                        string storyId = null;
                        if (u.Contains("/"))
                        {
                            storyId = u.Substring(u.IndexOf("/") + 1);
                            if (storyId.Contains("?"))
                                storyId = storyId.Substring(0, storyId.IndexOf("?"));
                            storyId = storyId.Replace("/", "");
                        }
                        user = user.Trim().ToLower();
                        var userResult = await Helper.InstaApi.UserProcessor.GetUserInfoByUsernameAsync(user);
                        if (userResult.Succeeded)
                        {
                            var friendshipResult = await Helper.InstaApi.UserProcessor.GetFriendshipStatusAsync(userResult.Value.Pk);
                            if (friendshipResult.Succeeded)
                            {
                                if (friendshipResult.Value.IsPrivate)
                                {
                                    if (friendshipResult.Value.Following || userResult.Value.Pk == Helper.CurrentUser.Pk)
                                        NavigationService.Navigate(typeof(Views.Stories.StoryViewX), new object[] { userResult.Value, storyId, url, url, url });
                                    else
                                    {
                                        Helper.ShowNotify($"You can't see @{user}'s stories becuase it's a private account.\r\nFollow this account to see their stories.", 4000);
                                        Helper.OpenProfile(userResult.Value.ToUserShort());
                                    }
                                }
                                else
                                    NavigationService.Navigate(typeof(Views.Stories.StoryViewX), new object[] { userResult.Value, storyId, url, url, url });
                            }
                            else
                                NavigationService.Navigate(typeof(Views.Stories.StoryViewX), new object[] { userResult.Value, storyId, url, url, url });
                        }
                        else
                            NavigationService.Navigate(typeof(Views.Stories.StoryViewX), new object[] { user, storyId, url });
                    }
                    else
                        Helper.OpenProfile(n);
                }
                else
                    url.OpenUrl();// Open in default Web Browser
                if (!NavigationService.IsThisType(typeof(Views.Main.StoryView)) && !NavigationService.IsThisType(typeof(Views.Stories.StoryViewX)))
                    NavigationService.ShowBackButton();
            }
            catch { }
        }

    }
}
