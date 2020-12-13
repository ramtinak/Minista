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
        public StoryViewX()
        {
            this.InitializeComponent();
        }
        private bool WasItBackButtonShown = false;
        protected override void OnNavigatedTo(NavigationEventArgs e)
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
                    //var list = new List<InstaReelFeed>();
                    if (objArr[0] is List<InstaReelFeed> reels)
                    {
                        //FeedList = reels;
                        var index = (int)objArr[1];

                        Contents.Content = new UserStoryUc { StoryFeed = reels[index] };
                    }
                }
            }
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
