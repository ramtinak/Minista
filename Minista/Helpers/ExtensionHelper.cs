using InstagramApiSharp.Classes.Models;
using Minista.Models;
using Minista.Models.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Minista
{
    static class ExtensionHelper
    {
        public static bool IsMine(this InstaUserShort userShort) => userShort.Pk == Helper.CurrentUser?.Pk;
        public static string GetUrl(string username, long pk) => $"https://instagram.com/stories/{username}/{pk}";
        public static void DisableScroll(this ScrollViewer sc)
        {
            if (sc != null)// won't be necessary
            {
                sc.HorizontalScrollMode = ScrollMode.Disabled;
                sc.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                sc.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                sc.VerticalScrollMode = ScrollMode.Auto;
            }
        }
        public static void EnableScroll(this ScrollViewer sc)
        {
            if (sc != null)// won't be necessary
            {
                sc.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                sc.VerticalScrollMode = ScrollMode.Enabled;
            }
        }
        public static List<T> ToListExt<T>(this T value)
        {
            return new List<T> { value };
        }
        public static string EncodeList(this long[] listOfValues, bool appendQuotation = true)
        {
            return EncodeList(listOfValues.ToList(), appendQuotation);
        }
        public static string EncodeList(this string[] listOfValues, bool appendQuotation = true)
        {
            return EncodeList(listOfValues.ToList(), appendQuotation);
        }
        public static string EncodeList(this List<long> listOfValues, bool appendQuotation = true)
        {
            if (!appendQuotation)
                return string.Join(",", listOfValues);
            var list = new List<string>();
            foreach (var item in listOfValues)
                list.Add(item.Encode());
            return string.Join(",", list);
        }
        public static string EncodeList(this List<string> listOfValues, bool appendQuotation = true)
        {
            if (!appendQuotation)
                return string.Join(",", listOfValues);
            var list = new List<string>();
            foreach (var item in listOfValues)
                list.Add(item.Encode());
            return string.Join(",", list);
        }
        public static string Encode(this long content)
        {
            return content.ToString().Encode();
        }
        public static string Encode(this string content)
        {
            return "\"" + content + "\"";
        }

        public static string EncodeRecipients(this long[] recipients)
        {
            return EncodeRecipients(recipients.ToList());
        }
        public static string EncodeRecipients(this List<long> recipients)
        {
            var list = new List<string>();
            foreach (var item in recipients)
                list.Add($"{item}");
            return string.Join(",", list);
        }
        static readonly Random Rnd = new Random();
        public static string GetThreadToken()
        {
            var str = "";
            // 6600286272511816379
            str += Rnd.Next(0, 9);
            str += Rnd.Next(0, 9);
            str += Rnd.Next(1000, 9999);
            str += Rnd.Next(11111, 99999);

            str += Rnd.Next(2222, 6789);

            return $"6600{str}";
        }

        public static void ResetPageCache(this Page page)
        {
            try
            {
                page.Frame.ResetPageCache();
            }
            catch { }
        }
        public static void ResetPageCache(this Frame frame)
        {
            try
            {
                var cacheSize = frame?.CacheSize;
                if (frame == null) return;
                if (cacheSize == null) return;
                frame.CacheSize = 0;
                frame.CacheSize = cacheSize ?? 0;
            }
            catch { }
        }
        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            //.{3}
            return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build/*, version.Revision*/);
        }
        public static async Task<long> GetFolderSize(this StorageFolder folder)
        {
            try
            {
                // Query all files in the folder. Make sure to add the CommonFileQuery
                // So that it goes through all sub-folders as well
                var folders = folder.CreateFileQuery(CommonFileQuery.OrderByName);

                // Await the query, then for each file create a new Task which gets the size
                var fileSizeTasks = (await folders.GetFilesAsync()).Select(async file => (await file.GetBasicPropertiesAsync()).Size);

                // Wait for all of these tasks to complete. WhenAll thankfully returns each result
                // as a whole list
                var sizes = await Task.WhenAll(fileSizeTasks);

                // Sum all of them up. You have to convert it to a long because Sum does not accept ulong.
                return sizes.Sum(l => (long)l);
            }
            catch { }
            return 0;
        }
        public static async Task<bool> VideoPermissionCheck()
        {
            MediaCapture _mediaCapture = new MediaCapture();
            try
            {
                await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Video
                });
                _mediaCapture.Dispose();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                _mediaCapture.Dispose();
                return false;
            }
        }
        public static async Task<bool> AudioPermissionCheck()
        {
            MediaCapture _mediaCapture = new MediaCapture();
            try
            {
                await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio
                });
                _mediaCapture.Dispose();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                _mediaCapture.Dispose();
                return false;
            }
        }
        public static InstaReelFeed ToReel(this InstaHashtagStory hashtagStory)
        {
            return new InstaReelFeed
            {
                Items = hashtagStory.Items,
                CanReply = hashtagStory.CanReply,
                CanReshare = hashtagStory.CanReshare,
                ExpiringAt = hashtagStory.ExpiringAt,
                Id = hashtagStory.Id,
                LatestReelMedia = hashtagStory.LatestReelMedia,
                Muted = hashtagStory.Muted,
                PrefetchCount = hashtagStory.PrefetchCount,
                Owner = hashtagStory.Owner,
                ReelType = hashtagStory.ReelType
            };
        }
        public static RecentActivityFeed ToRecentActivityFeed(this InstaRecentActivityFeed feed)
        {
            RecentActivityFeedType type = RecentActivityFeedType.Unknown;
            try
            {
                var v = DateTime.UtcNow.Subtract(feed.TimeStamp);
                if (v.TotalHours < 24)
                    type = RecentActivityFeedType.Today;
                else if (v.TotalDays <= 7)
                    type = RecentActivityFeedType.ThisWeek;
                else if (Convert.ToInt32(v.TotalDays / 7) < 4)
                    type = RecentActivityFeedType.ThisMonth;
                else
                    type = RecentActivityFeedType.Earlier;
            }
            catch { }
            return new RecentActivityFeed
            {
                CommentId = feed.CommentId,
                CommentIds = feed.CommentIds,
                Destination = feed.Destination,
                HashtagFollow = feed.HashtagFollow,
                InlineFollow = feed.InlineFollow,
                Links = feed.Links,
                Medias = feed.Medias,
                Pk = feed.Pk,
                ProfileId = feed.ProfileId,
                ProfileImage = feed.ProfileImage,
                ProfileImageDestination = feed.ProfileImageDestination,
                ProfileName = feed.ProfileName,
                RichText = feed.RichText,
                SecondProfileId = feed.SecondProfileId,
                SecondProfileImage = feed.SecondProfileImage,
                Text = feed.Text,
                TimeStamp = feed.TimeStamp,
                Type = feed.Type,
                ActivityCategoryType = type,
                RequestCount = feed.RequestCount,
                StoryType = feed.StoryType,
                SubText = feed.SubText
            };
        }
        public static InstaHashtag ToHashtag(this InstaHashtagShort hashtagShort)
        {
            return new InstaHashtag
            {
                Id = hashtagShort.Id,
                Name = hashtagShort.Name,
                ProfilePicture = hashtagShort.ProfilePicture,
                MediaCount = hashtagShort.MediaCount,
                FormattedMediaCount = hashtagShort.MediaCount.GetKiloMillion()
            };
        }
        public static InstaComment ToComment(this InstaCommentShort comment)
        {
            return new InstaComment
            {
                HasLikedComment = comment.HasLikedComment,
                User = comment.User,
                ContentType = comment.ContentType,
                Pk = comment.Pk,
                Text = comment.Text,
                Type = comment.Type,
                CreatedAt = comment.CreatedAt,
                CreatedAtUtc = comment.CreatedAt,
                LikesCount = comment.CommentLikeCount
            };
        }
        public static InstaComment ToComment(this InstaCaption caption)
        {
            long pk = -1;
            try
            {
                pk = int.Parse(caption.Pk);
            }
            catch { }
            return new InstaComment
            {
                CreatedAt = caption.CreatedAt,
                CreatedAtUtc = caption.CreatedAtUtc,
                UserId = caption.UserId,
                User = caption.User,
                Pk = pk,
                Text = caption.Text,
                BitFlags = caption.BitFlags,
                HasTranslation = caption.HasTranslation,
                IsCommentsDisabled = true
            };
        }
        public static InstaStoryFriendshipStatus ToStoryFriendshipStatus(this InstaFriendshipFullStatus friendship)
        {
            return new InstaStoryFriendshipStatus
            {
                IsPrivate = friendship.IsPrivate,
                Following = friendship.Following,
                IncomingRequest = friendship.IncomingRequest,
                IsBestie = friendship.IsBestie,
                OutgoingRequest = friendship.OutgoingRequest,
                Blocking = friendship.Blocking,
                FollowedBy = friendship.FollowedBy,
                Muting = friendship.Muting ,
                IsBlockingReel = friendship.IsBlockingReel,
                IsMutingReel = friendship.IsMutingReel
            };
        }
        public static InstaStoryFriendshipStatus ToStoryFriendshipStatus(this InstaFriendshipShortStatus friendship)
        {
            return new InstaStoryFriendshipStatus
            {
                IsPrivate = friendship.IsPrivate,
                Following = friendship.Following,
                IncomingRequest = friendship.IncomingRequest,
                IsBestie = friendship.IsBestie,
                OutgoingRequest = friendship.OutgoingRequest
            };
        }

        public static InstaFriendshipShortStatus ToFriendshipShortStatus(this InstaFriendshipFullStatus friendship)
        {
            return new InstaFriendshipShortStatus
            {
                IsPrivate = friendship.IsPrivate,
                Following = friendship.Following,
                IncomingRequest = friendship.IncomingRequest,
                IsBestie = friendship.IsBestie,
                OutgoingRequest = friendship.OutgoingRequest
            };
        }
        public static InstaReelFeed ToReelFeed(this InstaHighlightFeed model)
        {
            bool reshare = false, muted = false;
            if (model.CanReshare != null)
            {
                try
                {
                    reshare = bool.Parse(model.CanReshare.ToString());
                }
                catch { }
            }
            long seen = -1;
            if (model.Seen != null)
            {
                try
                {
                    seen = long.Parse(model.Seen.ToString());
                }
                catch { }
            }
            return new InstaReelFeed
            {
                CanReply = model.CanReply,
                CanReshare = reshare,
                ExpiringAt = model.CreatedAt,
                CreatedAt = model.CreatedAt,
                HasBestiesMedia = false,
                Id = model.HighlightId,
                Items = model.Items,
                LatestReelMedia = model.LatestReelMedia,
                MediaCount = model.MediaCount,
                PrefetchCount = model.PrefetchCount,
                Seen = seen,
                User = model.User?.ToUserShortFriendshipFull(),
                Muted = muted,
                ReelType = model.ReelType,
                Title = model.Title,
                HighlightCoverMedia = model.CoverMedia
            };
        }
        public static InstaReelFeed ToReelFeed(this StoryModel model)
        {
            return new InstaReelFeed
            {
                CanReply = model.CanReply,
                CanReshare = model.CanReshare,
                ExpiringAt = model.ExpiringAt,
                HasBestiesMedia = model.HasBestiesMedia,
                Id = model.Id,
                Items = model.Items,
                LatestReelMedia = model.LatestReelMedia,
                MediaCount = model.MediaCount,
                PrefetchCount = model.PrefetchCount,
                Seen = model.Seen,
                User = model.User,
                Muted = model.Muted,
                Owner = model.Owner,
                ReelType = model.ReelType,
                Title = model.Title
            };
        }
        public static StoryModel ToStoryModel(this InstaReelFeed model)
        {
            return new StoryModel
            {
                CanReply = model.CanReply,
                CanReshare = model.CanReshare,
                ExpiringAt = model.ExpiringAt,
                HasBestiesMedia = model.HasBestiesMedia,
                Id = model.Id,
                Items = model.Items,
                LatestReelMedia = model.LatestReelMedia,
                MediaCount = model.MediaCount,
                PrefetchCount = model.PrefetchCount,
                Seen = model.Seen,
                User = model.User,
                Muted = model.Muted,
                Owner = model.Owner,
                ReelType = model.ReelType
            };
        }
        public static InstaUserShort ToUserShort(this InstaUser user)
        {
            return new InstaUserShort
            {
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified,
                FullName = user.FullName,
                UserName = user.UserName,
                Pk = user.Pk,
                ProfilePictureId = user.ProfilePictureId,
                ProfilePicture = user.ProfilePicture,
                ProfilePicUrl = user.ProfilePicUrl,
                IsBestie = user.IsBestie
            };
        }
        public static InstaUserShortFriendship ToUserShortFriendship(this InstaUser user)
        {
            return new InstaUserShortFriendship
            {
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified,
                FullName = user.FullName,
                UserName = user.UserName,
                Pk = user.Pk,
                ProfilePictureId = user.ProfilePictureId,
                ProfilePicture = user.ProfilePicture,
                ProfilePicUrl = user.ProfilePicUrl,
                FriendshipStatus = user.FriendshipStatus,
                HasAnonymousProfilePicture = user.HasAnonymousProfilePicture,
                IsBestie = user.IsBestie
            };
        }
        public static InstaUser ToUser(this InstaUserShortFriendship user)
        {
            return new InstaUser(user.ToUserShort())
            {
                FriendshipStatus = user.FriendshipStatus
            };
        }
        public static InstaUserShort ToUserShort(this InstaUserShortFriendship user)
        {
            return new InstaUserShort
            {
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified,
                FullName = user.FullName,
                UserName = user.UserName,
                Pk = user.Pk,
                ProfilePictureId = user.ProfilePictureId,
                ProfilePicture = user.ProfilePicture,
                ProfilePicUrl = user.ProfilePicUrl,
                IsBestie = user.IsBestie
            };
        }
        public static InstaUserShort ToUserShort(this InstaBlockedUserInfo user)
        {
            return new InstaUserShort
            {
                IsPrivate = user.IsPrivate,
                FullName = user.FullName,
                UserName = user.UserName,
                Pk = user.Pk,
                ProfilePicture = user.ProfilePicture,
                ProfilePicUrl = user.ProfilePicture
            };
        }
        public static InstaUserShort ToUserShort(this InstaUserChaining user)
        {
            return new InstaUserShort
            {
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified,
                FullName = user.FullName,
                UserName = user.UserName,
                Pk = user.Pk,
                ProfilePictureId = user.ProfilePictureId,
                ProfilePicture = user.ProfilePicture,
                ProfilePicUrl = user.ProfilePicUrl,
                IsBestie = user.IsBestie
            };
        }
        public static InstaUserShortFriendship ToUserShortFriendship(this InstaUserChaining user)
        {
            return new InstaUserShortFriendship
            {
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified,
                FullName = user.FullName,
                UserName = user.UserName,
                Pk = user.Pk,
                ProfilePictureId = user.ProfilePictureId,
                ProfilePicture = user.ProfilePicture,
                ProfilePicUrl = user.ProfilePicUrl,
                FriendshipStatus = user.FriendshipStatus,
                IsBestie = user.IsBestie
            };
        }


        public static InstaUserShortFriendshipFull ToUserShortFriendshipFull(this InstaUserShort user)
        {
            return new InstaUserShortFriendshipFull
            {
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified,
                FullName = user.FullName,
                UserName = user.UserName,
                Pk = user.Pk,
                ProfilePictureId = user.ProfilePictureId,
                ProfilePicture = user.ProfilePicture,
                ProfilePicUrl = user.ProfilePicUrl,
                IsBestie = user.IsBestie
            };
        }
        public static InstaUserShortFriendship ToUserShortFriendship(this InstaUserShort user)
        {
            return new InstaUserShortFriendship
            {
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified,
                FullName = user.FullName,
                UserName = user.UserName,
                Pk = user.Pk,
                ProfilePictureId = user.ProfilePictureId,
                ProfilePicture = user.ProfilePicture,
                ProfilePicUrl = user.ProfilePicUrl,
                IsBestie = user.IsBestie
            };
        }
        public static InstaUserShort CopyUserShort(this InstaUserShort user)
        {
            return new InstaUserShort
            {
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified,
                FullName = user.FullName,
                UserName = user.UserName,
                Pk = user.Pk,
                ProfilePictureId = user.ProfilePictureId,
                ProfilePicture = user.ProfilePicture,
                ProfilePicUrl = user.ProfilePicUrl,
                IsBestie = user.IsBestie
            };
        }
        public static InstaUserInfo ToUserInfo(this InstaUserShort userShort)
        {
            return new InstaUserInfo
            {
                IsPrivate = userShort.IsPrivate,
                IsVerified = userShort.IsVerified,
                FullName = userShort.FullName,
                UserName = userShort.UserName,
                Pk = userShort.Pk,
                ProfilePictureId = userShort.ProfilePictureId,
                ProfilePicture = userShort.ProfilePicture,
                ProfilePicUrl = userShort.ProfilePicUrl,
                HasAnonymousProfilePicture = userShort.HasAnonymousProfilePicture
            };
        }

        public static InstaUserShort ToUserShort(this InstaUserInfo userInfo)
        {
            return new InstaUserShort
            {
                IsPrivate = userInfo.IsPrivate,
                IsVerified = userInfo.IsVerified,
                FullName = userInfo.FullName,
                UserName = userInfo.UserName,
                Pk = userInfo.Pk,
                ProfilePictureId = userInfo.ProfilePictureId,
                ProfilePicUrl = userInfo.ProfilePicUrl,
                ProfilePicture = userInfo.ProfilePicture,
                HasAnonymousProfilePicture = userInfo.HasAnonymousProfilePicture
            };
        }

        public static void CopyText(this string text)
        {
            try
            {
                var package = new DataPackage();
                package.SetText(text);
                Clipboard.SetContent(package);
            }
            catch { }
        }
        public static void OpenEmail(this string url) => ($"mailto:{url}").OpenUrl();
        public static async void OpenUrl(this string url)
        {
            try
            {
                var options = new Windows.System.LauncherOptions
                {
                    TreatAsUntrusted = false
                };
                if (SettingsHelper.Settings.HandleTelegramLinks)
                {
                    if (url.Contains("t.me/") || url.Contains("telegram.org/") || url.Contains("telegram.me/"))
                    {
                        var u = url;
                        var start = "";
                        if (url.Contains("t.me/"))
                            start = "t.me/";
                        else if (url.Contains("telegram.org/"))
                            start = "telegram.org/";
                        else if (url.Contains("telegram.me/"))
                            start = "telegram.me/";
                        if (!string.IsNullOrEmpty(start))
                        {
                            u = url.Substring(url.IndexOf(start) + start.Length);
                            const string join = "joinchat/";
                            string Replace(string str) => str.Replace("/", "").Replace(" ", "");

                            if (u.ToLower().Contains(join))
                            {
                                u = u.Substring(u.IndexOf(join) + join.Length);
                                url = $"tg://join?invite={Replace(u)}";
                            }
                            else if (u.ToLower().Contains("socks?"))
                                url = $"tg://{Replace(u)}";
                            else if (u.ToLower().Contains("proxy?"))
                                url = $"tg://{Replace(u)}";
                            else
                                url = $"tg://resolve?domain={Replace(u)}";
                        }
                    }
                }
                await Windows.System.Launcher.LaunchUriAsync(url.ToUri(), options);
            }
            catch { }
        }

        public static string TrimAnyBullshits(this string input) => input.Trim().Replace(" ", "").Replace("\t", "").Replace("@", "").Replace("#", "");
        public static bool HasPersianChar(this string input) => Regex.IsMatch(input, @"([\u0600-\u06FF]+\s?)|[\u06F0-\u06F9]", RegexOptions.IgnoreCase);

        public static MatchCollection GetHashtags(this string CaptionText)
           => Regex.Matches(Uri.UnescapeDataString(CaptionText), @"(?:#)([A-Za-z\u0600-\u06FF0-9_](?:(?:[A-Za-z\u0600-\u06FF0-9_]|(?:\.(?!\.))){0,28}(?:[A-Za-z\u0600-\u06FF0-9_]))?)");

        public static MatchCollection GetUsernames(this string CaptionText)
            => Regex.Matches(Uri.UnescapeDataString(CaptionText), @"(?:@)([A-Za-z\u0600-\u06FF0-9_](?:(?:[A-Za-z\u0600-\u06FF0-9_]|(?:\.(?!\.))){0,28}(?:[A-Za-z\u0600-\u06FF0-9_]))?)");
        public static Uri ToUri(this string url)
        {
            return new Uri(url);
        }
        public static string ToDigits(this double value)
        {
            return ToDigits((int)value);
        }
        //public static string ToDigits(this int value)
        //{
        //    return ToDigits((long)value);
        //}
        public static string ToDigits(this int value)
        {
            //string.Format("{0:n}", 1234567.123).PrintDebug();
            //string.Format("{0:#,#.##}", 1234567.123).PrintDebug();
            //string.Format("{0:#,###,###.##}", 1234567.123).PrintDebug();
            //1234567.123.ToString("N2", CultureInfo.InvariantCulture).PrintDebug();
            //return value.ToString("N2", CultureInfo.InvariantCulture);
            return string.Format("{0:#,#}", value);
        }
        public static string ToDigits(this long value)
        {
            return string.Format("{0:#,#}", value);
        }
        public static string Truncate(this string value, int maxChars = 25)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        public static void ScrollToElement(this ScrollViewer scrollViewer, UIElement element,
            bool isVerticalScrolling = true, bool smoothScrolling = true, float? zoomFactor = null)
        {
            var transform = element.TransformToVisual((UIElement)scrollViewer.Content);
            var position = transform.TransformPoint(new Point(0, 0));

            if (isVerticalScrolling)
            {
                scrollViewer.ChangeView(null, position.Y, zoomFactor, !smoothScrolling);
            }
            else
            {
                scrollViewer.ChangeView(position.X, null, zoomFactor, !smoothScrolling);
            }
        }
        public static void ScrollToElement(this ScrollViewer scrollViewer, double scrollOffset,
            bool isVerticalScrolling = true, bool smoothScrolling = true, float? zoomFactor = null)
        {

            if (isVerticalScrolling)
            {
                scrollViewer.ChangeView(null, scrollOffset, zoomFactor, !smoothScrolling);
            }
            else
            {
                scrollViewer.ChangeView(scrollOffset, null, zoomFactor, !smoothScrolling);
            }
        }
        public static ScrollViewer FindScrollViewer(this DependencyObject d)
        {
            if (d is ScrollViewer)
                return d as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var sw = FindScrollViewer(VisualTreeHelper.GetChild(d, i));
                if (sw != null) return sw;
            }
            return null;
        }
        public static void AddRangeForExplore(this ObservableCollection<InstaMedia> collection, List<InstaMedia> list)
        {
            //foreach (var item in list)
            //{
            //    collection.Add(item);
            //}
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if(!collection.Any(x => x.InstaIdentifier == l.InstaIdentifier))
                collection.Add(list[i]);
            }
        }
        public static void AddRange<T>(this ObservableCollection<T> collection, List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
                collection.Add(list[i]);
            //foreach (var item in list)
            //    collection.Add(item);
        }
        public static void AddRange<T>(this ObservableCollection<T> collection, ObservableCollection<T> list)
        {
            for (int i = 0; i < list.Count; i++)
                collection.Add(list[i]);
            //foreach (var item in list)
            //    collection.Add(item);
        }
        public static string Divide(this int i) => ((long)i).Divide();
        public static string Divide(this long d)
        {
            if (d >= 1000 && d < 1000000)
                return $"{((d / 1000f)).ToString("0.00",CultureInfo.CurrentCulture )}k";
            else if (d > 1000000)
                return $"{((d / 1000000f)).ToString("0.00", CultureInfo.CurrentCulture)}m";
            else
                return d.ToString(CultureInfo.CurrentCulture);
        }
        #region DEBUG
        public static string PrintDebug(this object obj)
        {
            var content = Convert.ToString(obj);
            Debug.WriteLine(content);
            return content;
        }
        public static string PrintException(this Exception ex, string name = null)
        {
            var sb = new StringBuilder();
            if (name == null)
                sb.AppendLine("Exeption thrown:");
            else sb.AppendLine($"Exeption thrown in '{name}': ");
            sb.AppendLine(ex.Message);
            sb.AppendLine("Source: " + ex.Source);
            sb.AppendLine("StackTrace: " + ex.StackTrace);
            sb.AppendLine();
            var content = sb.PrintDebug();
            return content;
        }
        #endregion DEBUG



        public static readonly string[] SupportedImageTypes = new string[] { ".jpg", ".jpeg", ".png" };
        public static readonly string[] SupportedVideoTypes = new string[] { ".mp4" };
        public static readonly string[] SupportedExtraVideoTypes = new string[]
        {
        ".mpeg",
        ".mpeg4",
        ".mov",
        ".mkv",
        ".m4v",
        ".avi",
        ".3gp",
        ".wmv"
        };

        public static string CalculateBytes(this long fileSize)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double size = fileSize;
            var unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return $"{size:0.#} {units[unit]}";
        }

        #region Check supported extensions
        public static bool NeedsConvert(this StorageFile file)
        {
            return file.Path.NeedsConvert();
        }
        public static bool NeedsConvert(this string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return SupportedExtraVideoTypes.Contains(ext);
        }
        public static bool IsVideo(this StorageFile file)
        {
            return file.Path.NeedsConvert() || file.Path.IsVideo();
        }
        public static bool IsVideo(this string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return SupportedVideoTypes.Contains(ext) || path.NeedsConvert();
        }
        public static bool IsImage(this StorageFile file)
        {
            var ext = Path.GetExtension(file.Path).ToLower();
            return SupportedImageTypes.Contains(ext);
        }
        public static bool IsSupported(this StorageFile file)
        {
            return file.IsVideo() || file.IsImage();
        }
        #endregion Check supported extensions

        public static uint GetHeight(this ImageProperties props)
        {
            return props.Height;
            //return props.Orientation == PhotoOrientation.Rotate180 ? props.Height : props.Width;
        }

        public static uint GetWidth(this ImageProperties props)
        {
            return props.Width;
            //return props.Orientation == PhotoOrientation.Rotate180 ? props.Width : props.Height;
        }



        public static uint GetHeight(this VideoProperties props)
        {
            return props.Orientation == VideoOrientation.Rotate180 || props.Orientation == VideoOrientation.Normal ? props.Height : props.Width;
        }

        public static uint GetWidth(this VideoProperties props)
        {
            return props.Orientation == VideoOrientation.Rotate180 || props.Orientation == VideoOrientation.Normal ? props.Width : props.Height;
        }
        public static async void BeginOnUIThread(this DependencyObject element, Action action)
        {
            try
            {
                await element.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(action));
            }
            catch
            {
                // Most likey Excep_InvalidComObject_NoRCW_Wrapper, so we can just ignore it
            }
        }
    }
}
