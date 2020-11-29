using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Minista.Views;
using Minista.Views.Sign;
using Minista.Views.Searches;
using Minista.Views.Direct;
using Minista.Views.Infos;
using Minista.Views.Main;
using Minista.Views.Posts;
using Windows.UI.Xaml.Media.Animation;
using Minista.Views.TV;
using Minista.Views.Uploads;

namespace Minista.Helpers
{
    static public class NavigationService
    {
        static public Frame Frame { get; private set; } = MainPage.Current.MyFrame;

        internal static void SetFrame(Frame myFrame)
        {
            Frame = myFrame;
        }
        static public void RemoveAllBackStack()
        {
            try
            {
                if (Frame.CanGoBack)
                    foreach (var f in Frame.BackStack.ToList())
                        Frame.BackStack.Remove(f);

                HideBackButton();
            }
            catch { }
        }
        static public void StartService(Frame pageFrame = null)
        {
            if (pageFrame != null)
                Frame = pageFrame;
            var n = SystemNavigationManager.GetForCurrentView();
            n.BackRequested += BackRequested;
            Frame.Navigated += Navigated;
        }


        static public void StopService()
        {
            var n = SystemNavigationManager.GetForCurrentView();

            Frame.Navigated -= Navigated;
            n.BackRequested -= BackRequested;

            Helper.DeleteCachedFolder();
            Helper.DeleteCachedFilesFolder();
        }


        static private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            GoBack(e);
        }
        private static void Navigated(object sender, NavigationEventArgs e)
        {
            if (!Frame.CanGoBack)
                HideBackButton();
            else
            {
                if(Frame.Content is MainView)
                    HideBackButton();
                else
                    ShowBackButton();
            }
            //if (Frame.Content is Views.PostView)
            //{
            //    if (Frame.BackStack.Any())
            //    {
            //        Frame.BackStack.Clear();
            //        HideBackButton();
            //    }
            //}
        }
        public static void HideSystemBackButton() => SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        public static void HideBackButton()
        {
            if (DeviceUtil.IsMobile)
            {
                if (SettingsHelper.Settings.HeaderPosition != Classes.HeaderPosition.Bottom)
                    MainPage.Current?.HideBackButton();
            }
            else
                MainPage.Current?.HideBackButton();
            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
        public static void ShowSystemBackButton() => SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        public static void ShowBackButton()
        {
            if (DeviceUtil.IsMobile)
            {
                if (SettingsHelper.Settings.HeaderPosition != Classes.HeaderPosition.Bottom)
                    MainPage.Current?.ShowBackButton();
            }
            else
                MainPage.Current?.ShowBackButton();
            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }


        public static void GoBack(BackRequestedEventArgs e = null)
        {
            //if (Frame.CanGoBack)
            {

                if (MainPage.Current?.PassCodeView != null)
                    if (MainPage.Current.PassCodeView.Visibility == Visibility.Visible)
                        return;

                //if (e != null)
                //    e.Handled = true;
                ClearForwardStacks();
                if (Frame.Content is UserDetailsView userDetails)
                {
                    if (userDetails.ScrollableUserPostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        userDetails.ScrollableUserPostUc.Visibility = Visibility.Collapsed;
                        return;
                    }
                    if (userDetails.ScrollableUserTaggedPostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        userDetails.ScrollableUserTaggedPostUc.Visibility = Visibility.Collapsed;
                        return;
                    }
                    if (userDetails.ScrollableUserShopPostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        userDetails.ScrollableUserShopPostUc.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                if (Frame.Content is ProfileDetailsView profile)
                {
                    if (profile.EditProfileUc.Uploader!= null)
                    {
                        if (profile.EditProfileUc.Uploader.IsUploading)
                        {
                            if (e != null)
                                e.Handled = true;
                            "Profile is uploading...\r\nPlease wait...".ShowMsg();
                            return;
                        }
                    }
                    if (profile.EditProfileUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        profile.EditProfileUc.Hide();
                        return;
                    }
                    if (profile.ScrollableUserPostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        profile.ScrollableUserPostUc.Visibility = Visibility.Collapsed;
                        return;
                    }
                    if (profile.ScrollableUserTaggedPostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        profile.ScrollableUserTaggedPostUc.Visibility = Visibility.Collapsed;
                        return;
                    }
                    if (profile.ScrollableUserShopPostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        profile.ScrollableUserShopPostUc.Visibility = Visibility.Collapsed;
                        return;
                    }
                }

                if (Frame.Content is TVPlayer tvPlayer)
                {
                    if (tvPlayer.MediaComments.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        tvPlayer.HideComments();
                        return;
                    }
                    if (tvPlayer.FooterView.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        tvPlayer.HideFooter();
                        return;
                    }
                }
                if (Frame.Content is HashtagView hashtagView)
                {
                    if (hashtagView.ScrollableHashtagPostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        hashtagView.ScrollableHashtagPostUc.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (hashtagView.ScrollableRecentHashtagPostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        hashtagView.ScrollableRecentHashtagPostUc.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                if (Frame.Content is SavedPostsView savedPostsView)
                {
                    if (savedPostsView.ScrollableSavedPostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        savedPostsView.ScrollableSavedPostUc.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                if (Frame.Content is LikedPostView likedPostView)
                {
                    if (likedPostView.ScrollableLikedPostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        likedPostView.ScrollableLikedPostUc.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                if (Frame.Content is CloseFriendsView closeFriendsView)
                {
                    if (closeFriendsView.SearchGrid.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        closeFriendsView.SearchGrid.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                if (Frame.Content is ArchiveView archiveView)
                {
                    if (archiveView.ScrollableArchivePostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        archiveView.ScrollableArchivePostUc.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                if (Frame.Content is RecoverView recover)
                {
                    if(recover.Second.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        recover.Reset();
                        return;
                    }
                    //if(Frame.CanGoBack)

                    //    Frame.GoBack(new DrillInNavigationTransitionInfo());
                    //return;
                }
                if (Frame.Content is ExploreView exploreView)
                {
                    if(exploreView.ScrollableExplorePostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        exploreView.ScrollableExplorePostUc.Visibility = Visibility.Collapsed;
                        return;
                    }
                    if (exploreView.SearchView.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        exploreView.SearchView.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                if (Frame.Content is ExploreClusterView exploreClusterView)
                {
                    if (exploreClusterView.ScrollableExplorePostUc.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        exploreClusterView.ScrollableExplorePostUc.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                if (Frame.Content is DirectRequestsView directRequestsView)
                {
                    try
                    {
                        if (directRequestsView.ItemsLV.SelectionMode == ListViewSelectionMode.Extended)
                        {
                            directRequestsView.ResetPanels();
                            return;
                        }
                    }
                    catch { }
                }
                if (Frame.Content is StoryView storyView)
                {
                    try
                    {
                        if (!storyView.MainStoryViewerUc.CanPressBack())
                        {
                            
                            return;
                        }
                    }
                    catch { }
                }

                if (Frame.Content is TVView tVView)
                {
                    if (tVView.TVSearchView.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        tVView.TVSearchView.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                if (Frame.Content is UploadView uploadView)
                {
                    if (uploadView.AddLocationView.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        uploadView.AddLocationView.Visibility = Visibility.Collapsed;
                        return;
                    }
                    if (uploadView.OptionsGrid.Visibility == Visibility.Visible)
                    {
                        if (e != null)
                            e.Handled = true;
                        uploadView.OptionsGrid.Visibility = Visibility.Collapsed;
                        uploadView.ShowNextButton();
                        return;
                    }
                }
                //if (Frame.Content is Views.Infos.UserDetailsView)
                {
                    if (Frame.CanGoBack)
                    {
                        if (e != null)
                            e.Handled = true;
                        Frame.GoBack(new DrillInNavigationTransitionInfo());
                    }
                    else
                    {
                        if (Helper.CurrentUser != null && !(Frame.Content is MainView))
                        //Helper.CurrentUser != null && (Frame.Content is StoryView) ||
                        //Helper.CurrentUser != null && (Frame.Content is UserDetailsView) ||
                        //Helper.CurrentUser != null && (Frame.Content is ProfileDetailsView) ||
                        //Helper.CurrentUser != null && (Frame.Content is SinglePostView))
                        {
                            if (e != null)
                                e.Handled = true;
                            MainPage.Current?.NavigateToMainView();
                        }
                    }
                }
                //  if (MainPage.Current.About.Visibility == Visibility.Visible)
                //  {
                //      MainPage.Current.About.Visibility = Visibility.Collapsed;
                //      return;
                //  }

                //  if (Views.VideoConverterView.Current != null && Views.VideoConverterView.Current.Visibility == Visibility.Visible &&
                //      Views.VideoConverterView.Current.IsConverting)
                //  {
                //      "Back button not work while app is converting video".ShowMsg();
                //      return;
                //  }
                //  if(MainPage.Current.SignIn.IsSomethingGoingOn())
                //  {
                //      MainPage.Current.SignIn.HideAndReset();
                //      return;
                //  }
                //  if (Views.ConfigView.Current != null && Views.ConfigView.Current.UploadingGrid.Visibility == Visibility.Visible &&
                //Views.ConfigView.Current.IsUploading)
                //      return;
                //try
                //{
                //    Frame.GoBack();
                //}
                //catch { }
            }
        }

        static void ClearForwardStacks()
        {
            try
            {
                Frame.ForwardStack.Clear();
            }
            catch { }
        }



        static public bool IsDirect()
        {
            try
            {
                return Frame.Content is ThreadView || Frame.Content is InboxView;
            }
            catch { }
            return false;
        }






        static public bool CheckType(Type t)
        {
            return Frame.Content != null && Frame.Content.GetType() == t;
        }



        static public bool Navigate<T>(object parameter = null)
        {
            var type = typeof(T);

            return Navigate(type, parameter);
        }
        static public bool Navigate(Type t, object parameter = null)
        {
            ("Navigating to " + t.Name).PrintDebug();
            return Frame.Navigate(t, parameter, new EntranceNavigationTransitionInfo() { });
            //return Frame.Navigate(t, parameter, new DrillInNavigationTransitionInfo() { });
        }

        static public void GoBack()
        {
            GoBack(null);
        }
    }
}
