using InstagramApiSharp.Classes.Models;
using Minista.Helpers;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Minista.Views.Stories
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StoryViewX : Page
    {
        public event EventHandler Navigation;
        public StoryViewX()
        {
            this.InitializeComponent();
        }
        private bool WasItBackButtonShown = false;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MainPage.Current?.HideHeaders();
            Helper.HideStatusBar();

            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();

            if (MainPage.Current != null)
            {
                if (MainPage.Current.BackButton.Visibility == Visibility.Visible)
                {
                    WasItBackButtonShown = true;
                    NavigationService.HideBackButton();
                }
                NavigationService.ShowSystemBackButton();
            }
            if (e.Parameter is object[] objArr)
            {
                if (objArr.Length == 2)
                {
                    if (objArr[0] is List<InstaReelFeed> reels)
                        Init(reels, (int)objArr[1]);
                }
                else if (objArr.Length == 3)
                {
                    var user = objArr[0] as string;
                    var storyId = objArr[1] as string;
                    ////var url = objArr[3] as string; // in dekorie ke faghat lengthemon beshe 3ta
                    user = user.Trim();
                    //SelectedStoryId = storyId.Trim();
                    var userResult = await Helper.InstaApi.UserProcessor.GetUserInfoByUsernameAsync(user);
                    if (userResult.Succeeded)
                        InitAsync(userResult.Value.Pk.ToString(), storyId.Trim());
                }
                else if (objArr.Length == 5)
                {
                    var user = objArr[0] as InstaUserInfo;
                    var storyId = objArr[1] as string;
                    ////var url = objArr[3] as string; // in dekorie ke faghat lengthemon beshe 3ta
                    //SelectedStoryId = storyId.Trim();

                    InitAsync(user.Pk.ToString(), storyId.Trim());
                }
                else if (objArr.Length == 4)
                {
                    var userId = (long)objArr[0];
                    var storyId = objArr[1] as string;
                    //SelectedStoryId = storyId.Trim();
                    InitAsync(userId.ToString(), storyId.Trim());
                }
            }
            else if (e.Parameter is InstaReelFeed reel && reel != null)
                Init(new List<InstaReelFeed> { reel }, 0);
            else
            {
                long pk = -1;
                if (e.Parameter is InstaUserShort userShort)
                    pk = userShort.Pk;
                else if (e.Parameter is long userId)
                    pk = userId;
                if (pk != -1)
                    InitAsync(pk.ToString());
            }
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            Navigation?.Invoke(this, null);
        }
        async void InitAsync(string pk, string selectedStoryId = null)
        {
            try 
            {
                var stories = await Helper.InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(pk);
                if (stories.Succeeded)
                {
                    Init(stories.Value.Items, 0, selectedStoryId);
                    //FeedList = stories.Value.Items;
                    //FeedListIndex = 0;
                    //PlayFeedUser();
                }
            }
            catch(Exception ex) { ex.PrintException("InitAsync"); }
        }
        List<UserStoryUc> UserStories = new List<UserStoryUc>();
        List<InstaReelFeed> Stories = new List<InstaReelFeed>();
        int CurrentSelectedIndex = 0;
        void Init(List<InstaReelFeed> reels, int index, string selectedStoryId = null)
        {
            try
            {
                if (reels == null || reels?.Count == 0)
                    return;
                CurrentSelectedIndex = index;
                //var reel = reels[index];
                //if (reel.Items.Count == 0)
                //{
                //    InitAsync(reel.User.Pk.ToString());
                //    return;
                //}
                UserStories.Clear();
                //reels.ForEach(x =>
                //{
                //    var uc = new UserStoryUc { StoryFeed = x };
                //    uc.PlayNextItem += OnUcPlayNextItem;
                //    uc.PlayPreviousItem += OnUcPlayPreviousItem;
                //    UserStories.Add(uc);
                //});
                Stories.Clear();
                Stories.AddRange(reels);

                var uc = new UserStoryUc { StoryFeed = Stories[index] };
                uc.PlayNextItem += OnUcPlayNextItem;
                uc.PlayPreviousItem += OnUcPlayPreviousItem;
                Contents.Content = uc;
                uc.FirstInit(selectedStoryId);


                //Contents.Content = UserStories[index];
                //UserStories[index].FirstInit(selectedStoryId);
            }
            catch { }
        }

        private void OnUcPlayNextItem(object sender, EventArgs e)
        {
            try
            {
                if (Stories.Count > 1)
                {
                    CurrentSelectedIndex = (CurrentSelectedIndex + 1) % Stories.Count;
                    if (CurrentSelectedIndex != 0)
                        Play(CurrentSelectedIndex);
                    else
                        NavigationService.GoBack();
                }
                else
                    NavigationService.GoBack();
            }
            catch { }
        }


        private void OnUcPlayPreviousItem(object sender, EventArgs e)
        {
            try
            {
                if (Stories.Count > 1)
                {
                    CurrentSelectedIndex = (CurrentSelectedIndex - 1) % Stories.Count;
                    if (CurrentSelectedIndex >= 0)
                        Play(CurrentSelectedIndex);
                    else
                        NavigationService.GoBack();
                }
                else
                    NavigationService.GoBack();
            }
            catch { }
        }
        void Play(int index)
        {
            try
            {
                var uc = new UserStoryUc { StoryFeed = Stories[index] };
                uc.PlayNextItem += OnUcPlayNextItem;
                uc.PlayPreviousItem += OnUcPlayPreviousItem;
                Contents.Content = uc;
                uc.FirstInit();
                //Contents.Content = UserStories[index];
                //UserStories[index].FirstInit();
            }
            catch { }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MainPage.Current?.ShowHeaders();
            Helper.ShowStatusBar();

            NavigationService.HideSystemBackButton();
            if (MainPage.Current != null && WasItBackButtonShown)
                NavigationService.ShowBackButton();
            WasItBackButtonShown = false;
            //try
            //{
            //    FeedListStatic = FeedList;
            //    StopEverything();
            //}
            //catch { }
        }
    }
}
