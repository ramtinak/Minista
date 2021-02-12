using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//using FFmpegInterop;
using Windows.Storage.Pickers;
using Windows.Storage;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Helpers;
using InstagramApiSharp.Enums;
using System.Threading.Tasks;
using static Helper;

namespace Minista.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage1 : Page
    {
        public BlankPage1()
        {
            this.InitializeComponent();
            SizeChanged += BlankPage1_SizeChanged;

        }
        //bool isSecond = false;
        private void BlankPage1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //IEnumerable<PivotHeaderPanel> headerpanel = FindVisualChildren<PivotHeaderPanel>(MPivot);
            //double totalwidth = headerpanel.ElementAt(0).ActualWidth;
          /*  if (isSecond)*/ return;
            //isSecond = true;
            //IEnumerable<PivotHeaderItem> items = FindVisualChildren<PivotHeaderItem>(MPivot);
            //int headitemcount = items.Count();
            //var width = MPivot.ActualWidth / headitemcount;
            //System.Diagnostics.Debug.WriteLine(width);
            //for (int i = 0; i < headitemcount; i++)
            //{
            //    items.ElementAt(i).Width = width;
            //}
            ////System.Diagnostics.Debug.WriteLine(MPivot.Items.Count);
            ////var width = MPivot.ActualWidth / MPivot.Items.Count;
            ////for (int i = 0; i < MPivot.Items.Count; i++)
            ////{
            ////    if (MPivot.Items[i] is PivotItem item)
            ////    {
            ////        if(item.Header is Grid grid)
            ////            grid.Width = width;
            ////    }
            ////}
        }
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        //async void SendDMToMyAunt()
        //{
        //    string username = "rmt4006".ToLower();
        //    //long userPK = 123;
        //    var message = "Hello my auntie";

        //    // remove \r characters, since windows is adding it to new lines
        //    message = message.Replace("\r", "");

        //    var inbox = await InstaApi.MessagingProcessor.GetDirectInboxAsync(PaginationParameters.MaxPagesToLoad(1));

        //    if (inbox.Succeeded)
        //    {

        //        // Act as Instagram>
        //        // search throw ranked recipients:
        //        // manipulate instagram by searching like real instagram:
        //        await InstaApi.MessagingProcessor.GetRankedRecipientsByUsernameAsync(username.Substring(0, 2));
        //        await InstaApi.MessagingProcessor.GetRankedRecipientsByUsernameAsync(username.Substring(0, 4));
        //        var rankedRecipients = await InstaApi.MessagingProcessor.GetRankedRecipientsByUsernameAsync(username);
        //        if (rankedRecipients.Succeeded)
        //        {
        //            var threadId = string.Empty;
        //            long userPk = -1;
        //            if (rankedRecipients.Value?.Threads?.Count > 0)
        //            {
        //                var byThread = rankedRecipients.Value.Threads.FirstOrDefault(x => x.Users.Count == 1 && x.Users.FirstOrDefault()?.UserName.ToLower() == username);
        //                if (byThread != null)
        //                    threadId = byThread.ThreadId;
        //            }
        //            else
        //            {
        //                var byUser = rankedRecipients.Value.Users.FirstOrDefault(x =>  x.UserName.ToLower() == username);
        //                if (byUser != null)
        //                    userPk = byUser.Pk;

        //            }

        //            // now send message:

        //            if (userPk != -1) // via user public key (user id PK) if exists
        //            {
        //                var dm = await InstaApi.MessagingProcessor.SendDirectTextAsync(userPk.ToString(), null, message);

        //            }
        //            else if (!string.IsNullOrEmpty(threadId)) // with thread id if exists
        //            {
        //                var dm = await InstaApi.MessagingProcessor.SendDirectTextAsync(null, threadId, message);

        //            }
        //            else
        //                Console.WriteLine("WHAT THE FUCK?! NO THREAD OR PK FOUND");

        //        }
        //    }
        //}

        //async Task<InstaDirectInboxThread> GetThread(InstaDirectInboxContainer inbox, long userPK)
        //{
        //    if (inbox?.Inbox?.Threads?.Count > 0)
        //    {
        //        var exists = inbox.Inbox.Threads.FirstOrDefault(x => x.Users.Count == 1 && x.Users.FirstOrDefault()?.Pk == userPK);
        //        if (exists != null)
        //            return exists;
        //        var getThreadByParticipants = await Helper.InstaApi.MessagingProcessor.GetThreadByParticipantsAsync(inbox.SeqId, new long[] { userPK });
        //        if (getThreadByParticipants.Succeeded)
        //            return getThreadByParticipants.Value;
        //    }
        //    return null;
        //}
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".bmp");
            //openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".mp4");
            var file = await openPicker.PickSingleFileAsync();
            if (file == null) return;

            var stream = await file.OpenAsync(FileAccessMode.Read);
            //var grabber = await FrameGrabber.CreateFromStreamAsync(stream);
            //var frame = await grabber
            //    .ExtractVideoFrameAsync(TimeSpan.FromSeconds(4.5), false);
            int ix = 1;
            var savedFile = await KnownFolders.MusicLibrary.CreateFileAsync(ix + ".jpg", CreationCollisionOption.GenerateUniqueName);
            var oStream = await savedFile.OpenAsync(FileAccessMode.ReadWrite);
            //await frame.EncodeAsJpegAsync(oStream);
            oStream.Dispose();

            for (int i = 0; i < 100; i++)
            {
                //var frame2 = await grabber .ExtractNextVideoFrameAsync();
                //var savedFile2 = await KnownFolders.MusicLibrary.CreateFileAsync(ix + ".jpg", CreationCollisionOption.GenerateUniqueName);
                //var oStream2 = await savedFile2.OpenAsync(FileAccessMode.ReadWrite);
                //await frame2.EncodeAsJpegAsync(oStream2);
                //oStream2.Dispose();

                //var img = new Image
                //{
                //    Height = 150,
                //    Width = 150,
                //    Name = ix.ToString()
                //};
                //img.Source
                //LV.Children.Add(img);
            }

        }

        private void GridLoaded(object sender, RoutedEventArgs e)
        {

        }
        //int LatestItemIndex = -1;
        //object LatestSelected = null;
        //private void LVPostsContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        //{
        //    try
        //    {
        //        if (LatestItemIndex != -1 && args.ItemIndex != LatestItemIndex && LatestSelected != null
        //            && LatestSelected is MyModel uc && uc != null)
        //        {
        //            ((LVPosts.ItemsPanelRoot.Children[LatestItemIndex] as ListViewItem).ContentTemplateRoot as MediaElement).Stop();
        //        }
        //    }
        //    catch { }

        //    LatestSelected = args.Item;
        //    LatestItemIndex = args.ItemIndex;
        //}
    }
    /*static*/ class PESSSSSSS
    {
        public async void AAAAA()
        {
            var userResult = await InstaApi.UserProcessor.GetUserAsync("instagram");

            if (userResult.Succeeded)
            {
                var stories = await InstaApi.StoryProcessor.GetUserStoryAsync(userResult.Value.Pk); // getting user's stories
                if (stories.Succeeded && stories.Value?.Items?.Count > 0)
                {
                    var reelStory = stories.Value; // reel story contains all stories of someone
                    var storyItem1 = reelStory.Items[0]; // choose a story

                    // Mark a story as seen>
                    // Since there are 3 different API for seen a story, you have to checks them first

                    if (reelStory.IsElection)// checks for election story
                    {
                        var dic = new List<InstaStoryElectionKeyValue>
                        {
                           new InstaStoryElectionKeyValue
                           {
                               StoryId = reelStory.Id,
                               StoryItemId = storyItem1.Id,
                               TakenAtUnix = storyItem1.TakenAt.ToUnixTime().ToString()
                           }
                        };
                        var seen = await InstaApi.StoryProcessor.MarkMultipleElectionStoriesAsSeenAsync(dic);
                    }
                    else // otherwise is normal story or hashtag story
                    {
                        var dic = new Dictionary<string, long>
                        {
                            {storyItem1.Id, storyItem1.TakenAt.ToUnixTime()},
                            // you can mark multiple stories in here!
                        };
                        var seen = await InstaApi.StoryProcessor.MarkMultipleStoriesAsSeenAsync(dic);
                    }

                }

            }

        }
    }
}
