using InstagramApiSharp.Classes.Models;
using Minista.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Minista.ContentDialogs
{
    public sealed partial class StoryMenuDialog : ContentPopup
    {
        public InstaStoryItem StoryItem { get; set; }
        public InstaReelFeed ReelFeed { get; set; }
        public Views.Main.StoryView View { get; set; }

        public StoryMenuDialog(InstaReelFeed reelFeed, InstaStoryItem storyItem , Views.Main.StoryView view)
        {
            StoryItem = storyItem;
            ReelFeed = reelFeed;
            View = view;
            InitializeComponent();
            LVMenu.Items.Add(new StoryMenu { Text = "Download", Command = StoryMenuCommand.Download });
            LVMenu.Items.Add(new StoryMenu { Text = "Share Link", Command = StoryMenuCommand.ShareLink });

            if (reelFeed.User.IsMine())
            {
                LVMenu.Items.Add(new StoryMenu { Text = "Delete", Command = StoryMenuCommand.Delete });
            }
            else
            {
                LVMenu.Items.Add(new StoryMenu { Text = "Report", Command = StoryMenuCommand.Report });
            }
            LVMenu.Items.Add(new StoryMenu { Text = "Cancel", Command = StoryMenuCommand.Cancel });
        }
        private async void LVMenuItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem != null && e.ClickedItem is StoryMenu menu && menu != null)
                {
                    switch (menu.Command)
                    {
                        case StoryMenuCommand.Delete:
                            {
                                var type = InstagramApiSharp.Enums.InstaSharingType.Photo;
                                if (StoryItem.MediaType == InstaMediaType.Video) type = InstagramApiSharp.Enums.InstaSharingType.Video;
                                var delete = await Helper.InstaApi.StoryProcessor
                                    .DeleteStoryAsync(StoryItem.Id, type);
                                if (delete.Succeeded)
                                {
                                    try
                                    {
                                        //View.FeedList[View.FeedListIndex].Items.Clear();
                                        //FeedList[FeedListIndex].Items.AddRange(storiesAfter.FirstOrDefault().Items);
                                        var yek = View.FeedList[View.FeedListIndex].Items.SingleOrDefault(ss => ss.Id.ToLower() == StoryItem.Id.ToLower());
                                        if (yek != null)
                                        {
                                            View.SkipNext();
                                            View.FeedList[View.FeedListIndex].Items.Remove(yek);
                                        }
                                        if (Views.Main.MainView.Current?.MainVM?.Stories?.Count > 0)
                                        {
                                            try
                                            {
                                                foreach (var item in Views.Main.MainView.Current.MainVM.Stories.ToList())
                                                {
                                                    //var single = storiesAfter.SingleOrDefault(ss => ss.User.Pk.ToString() == item.User.Pk.ToString());
                                                    
                                                    if (item.User.Pk == Helper.CurrentUser.Pk)
                                                    {
                                                        var single = item.Items.SingleOrDefault(ss => ss.Id.ToLower() == StoryItem.Id.ToLower());
                                                        item.Items.Remove(single);
                                                        if (item.Items.Count == 0)
                                                            Views.Main.MainView.Current.MainVM.Stories.Remove(item);
                                                        break;
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                    catch { }
                                }
                                Hide();
                            }
                            break;
                        case StoryMenuCommand.Download:
                            try
                            {
                                var item = StoryItem;
                                if (item.MediaType == InstaMediaType.Image)
                                {
                                    var url = item.Images.FirstOrDefault().Uri;
                                    DownloadHelper.Download(url, item.Images.LastOrDefault().Uri, false, ReelFeed.User.UserName, null, true, true);
                                }
                                else
                                {
                                    var url = item.Videos.FirstOrDefault().Uri;
                                    DownloadHelper.Download(url, item.Images.LastOrDefault().Uri, true, ReelFeed.User.UserName, null, true, true);
                                }
                            }
                            catch { }
                            Hide();
                            break;

                        case StoryMenuCommand.Report:
                            var report = await Helper.InstaApi.MediaProcessor.ReportMediaAsync(StoryItem.Id);
                            Hide();
                            if (report.Succeeded)
                            {
                                Helper.ShowNotify("Reported");
                            }
                            break;
                        case StoryMenuCommand.Settings:
                            var abc = await new Windows.UI.Popups.MessageDialog("NOT available at the moment...").ShowAsync();
                            break;
                        case StoryMenuCommand.ShareLink:
                            try
                            {
                                var url = ExtensionHelper.GetUrl(ReelFeed.User.UserName.ToLower(), StoryItem.Pk);
                                url.CopyText();
                                Helper.ShowNotify("Url copied ;-)");
                            }
                            catch { }
                            Hide();
                            break;
                        case StoryMenuCommand.Cancel:
                            Hide();
                            break;
                    }

                }
            }
            catch
            {
                Hide();
            }
        }
        enum StoryMenuCommand
        {
            Download,
            Report,
            ShareLink,
            Delete,
            Settings,
            Cancel

        }
        class StoryMenu
        {
            public string Text { get; set; }
            public StoryMenuCommand Command { get; set; }
        }
    }
}
