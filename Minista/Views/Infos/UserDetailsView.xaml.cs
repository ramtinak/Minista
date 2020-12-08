using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;
using Minista.ViewModels.Infos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


namespace Minista.Views.Infos
{
    public sealed partial class UserDetailsView : Page
    {
        readonly Compositor _compositor;
        readonly ImplicitAnimationCollection _elementImplicitAnimation;
        InstaUserShort UserShort;
        InstaUserShortFriendship UserShortFriendship;
        private readonly Visual _goUpButtonVisual, _refreshButtonVisual;
        //ScrollViewer ScrollView;
        //public UserDetailsViewModel UserDetailsVM { get; set; } = new UserDetailsViewModel();
        bool ImageAnimationExists = false;
        private string Username = null;
        private bool ShowTVTab = false;
        private bool CanLoadFirstPopUp = false;
        NavigationMode NavigationMode;
        public static UserDetailsView Current;
        public UserDetailsView()
        {
            this.InitializeComponent();
            Current = this;
            //DataContext = UserDetailsVM;
            //NavigationCacheMode = NavigationCacheMode.Enabled;
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
           
            // Create ImplicitAnimations Collection. 
            _elementImplicitAnimation = _compositor.CreateImplicitAnimationCollection();

            //Define trigger and animation that should play when the trigger is triggered. 

            //_elementImplicitAnimation["Offset"] = CreateOffsetAnimation();
            _elementImplicitAnimation["Opacity"] = CreateOpacityAnimation();

            _goUpButtonVisual = GoUpButton.GetVisual();
            _refreshButtonVisual = RefreshButton.GetVisual();
            Loaded += UserDetailsViewLoaded;
            //LV.ScrollChanged += LVScrollChanged;
            SizeChanged += ProfileDetailsViewSizeChanged;
        }

        private void ProfileDetailsViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (MPivot != null)
                    MPivot.Height = ActualHeight;

            }
            catch { }
        }

        //private void LVScrollChanged(object sender, ScrollViewer e)
        //{
        //    if (ScrollView == null)
        //    {

        //        ScrollView = e;
        //        ScrollView.ViewChanging += ScrollViewViewChanging;

        //        //ToggleGoUpButtonAnimation(false);
        //        //ToggleRefreshButtonAnimation(false);
        //        //UserDetailsVM.View = this;
        //        //if (UserShort != null)
        //        //    UserDetailsVM.SetUser(UserShort);
        //        //else if (UserShortFriendship != null)
        //        //{
        //        //    UserDetailsVM.SetUser(UserShortFriendship);
        //        //    UpdateUserImage(UserShortFriendship.ProfilePicture);
        //        //}

        //        UserDetailsVM.SetScrollViewer(ScrollView);
        //        //UserDetailsVM.SetBiographyTextBlock(BiographyText);
        //    }
        //}

        public void UpdateUserImage(string img)
        {
            try
            {
                UserImage.Fill = img.GetImageBrush();
            }
            catch { }
            //var media = new InstaMedia();
            //media.Carousel[0].Images[0].Uri;
        }

        public void SetBroadcastVisibility(bool hide)
        {
            try
            {
                if (AnimationStory != null)
                {
                    if (hide)
                        AnimationStory.Stop();
                    else
                        AnimationStory.Begin();
                }
            }
            catch { }
        }
        public void SetBroadcast(InstaBroadcast broadcast)
        {
            UserDetailsVM?.SetBroadcast(broadcast);
        }
        public void ResetUserImage()
        {
            try
            {
                UpdateUserImage("ms-appx:///Assets/Images/transparent-img.png");
            }
            catch { }
        }
        private void UserDetailsViewLoaded(object sender, RoutedEventArgs e)
        {
            //Loaded -= UserDetailsViewLoaded;
            try
            {
                RefreshControl.RefreshRequested -= RefreshControlRefreshRequested;
                RefreshControl.Visualizer.RefreshStateChanged -= RefreshControlRefreshStateChanged;
            }
            catch { }
            RefreshControl.RefreshRequested += RefreshControlRefreshRequested;
            if (RefreshControl.Visualizer != null)
                RefreshControl.Visualizer.RefreshStateChanged += RefreshControlRefreshStateChanged;
            ToggleGoUpButtonAnimation(false);
            ToggleRefreshButtonAnimation(false);
            if (NavigationMode == NavigationMode.Back && UserDetailsVM.User != null)
            {
                if (UserDetailsVM.User?.Pk == UserShort?.Pk)
                    return;
            }
            else if (NavigationMode == NavigationMode.Back && UserDetailsVM.User != null)
            {
                if (UserDetailsVM.User?.Pk == UserShortFriendship?.Pk)
                    return;
            }
            else if (NavigationMode == NavigationMode.Back && UserDetailsVM.User != null)
            {
                if (UserDetailsVM.User?.UserName?.ToLower() == Username?.ToLower())
                    return;
            }
            else if (NavigationMode == NavigationMode.New)
            {
                GetType().RemovePageFromBackStack();
                //this.ResetPageCache();
                //NavigationCacheMode = NavigationCacheMode.Disabled;
                NavigationCacheMode = NavigationCacheMode.Enabled;
                CanLoadFirstPopUp = false;
                UserDetailsVM.MediaGeneratror.SetLV(SCMain);
                UserDetailsVM.TVMediaGeneratror.SetLV(SCMain);
                UserDetailsVM.TaggedMediaGeneratror.SetLV(SCMain);
                UserDetailsVM.ShopMediaGeneratror.SetLV(SCMain);
            }
            try
            {
                if (NavigationMode == NavigationMode.New /*|| NavigationMode == NavigationMode.Refresh*/)
                {
                    if (ScrollableUserPostUc.Visibility == Visibility.Visible)
                        ScrollableUserPostUc.Visibility = Visibility.Collapsed;
                    if (ScrollableUserTaggedPostUc.Visibility == Visibility.Visible)
                        ScrollableUserTaggedPostUc.Visibility = Visibility.Collapsed;
                    if (ScrollableUserShopPostUc.Visibility == Visibility.Visible)
                        ScrollableUserShopPostUc.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (GridMainScrollViewer != null)
                            GridMainScrollViewer.Height = double.NaN;
                    }
                    catch { }
                }
            }
            catch { }
            if (!CanLoadFirstPopUp)
            {
                //if (!CanLoadFirstPopUp) return;
                UserDetailsVM.ResetToDefault();
                ResetUserImage();
                UserDetailsVM.View = this;


                IsTabCreated = false;
                try
                {
                    ResetTabs();
                }
                catch { }
                if (ScrollableUserPostUc != null)
                    ScrollableUserPostUc.Visibility = Visibility.Collapsed;
                if (ScrollableUserTaggedPostUc != null)
                    ScrollableUserTaggedPostUc.Visibility = Visibility.Collapsed;
                if (ScrollableUserShopPostUc != null)
                    ScrollableUserShopPostUc.Visibility = Visibility.Collapsed;
                if (UserShort != null)
                {
                    UserDetailsVM.SetUser(UserShort);
                    ScrollableUserPostUc.SetData(UserShort, UserDetailsVM.MediaGeneratror);
                    ScrollableUserTaggedPostUc.SetData(UserShort, UserDetailsVM.TaggedMediaGeneratror);
                    ScrollableUserShopPostUc.SetData(UserShort, UserDetailsVM.ShopMediaGeneratror);
                }
                else if (UserShortFriendship != null)
                {
                    UserDetailsVM.SetUser(UserShortFriendship);
                    UpdateUserImage(UserShortFriendship.ProfilePicture);
                    ScrollableUserPostUc.SetData(UserShortFriendship.ToUserShort(), UserDetailsVM.MediaGeneratror);
                    ScrollableUserTaggedPostUc.SetData(UserShortFriendship.ToUserShort(), UserDetailsVM.TaggedMediaGeneratror);
                    ScrollableUserShopPostUc.SetData(UserShortFriendship.ToUserShort(), UserDetailsVM.ShopMediaGeneratror);
                }
                else if (!string.IsNullOrEmpty(Username))
                {
                    UserDetailsVM.SetUsername(Username);
                    ScrollableUserPostUc.SetData(new InstaUserShort { UserName = Username }, UserDetailsVM.MediaGeneratror);
                    ScrollableUserTaggedPostUc.SetData(new InstaUserShort { UserName = Username }, UserDetailsVM.TaggedMediaGeneratror);
                    ScrollableUserShopPostUc.SetData(new InstaUserShort { UserName = Username }, UserDetailsVM.ShopMediaGeneratror);
                }
                UserDetailsVM.SetBiographyTextBlock(BiographyText);
                CanLoadFirstPopUp = true;
            }
        }
        public void LoadExternalProfile(object e)
        {
            if (e is InstaUserShort userShort && userShort != null)
            {
                ShowTVTab = false;
                //UserDetailsVM = new UserDetailsViewModel();
                //DataContext = UserDetailsVM;
                UserShort = userShort;
                UserShortFriendship = null;
                Username = null;
                //NavigationCacheMode = NavigationCacheMode.Disabled;
                //NavigationCacheMode = NavigationCacheMode.Required;
                try
                {
                    ConnectedAnimation imageAnimation =
                        ConnectedAnimationService.GetForCurrentView().GetAnimation("UserImage");
                    if (imageAnimation != null)
                    {
                        ImageAnimationExists = true;
                        //imageAnimation.Completed += ImageAnimationClose_Completed;
                        //if (LatestGrid?.Name != InfoGrid.Name)
                        //    LatestGrid.Background = new SolidColorBrush(Helper.GetColorFromHex("#FF2E2E2E"));

                        imageAnimation.TryStart(UserImage);
                        //GridShadow.Visibility = GVSHOW.Visibility = Visibility.Collapsed;
                    }
                    else ImageAnimationExists = false;
                }
                catch { }
            }
            else if (e is List<InstaUserShortFriendship> userShortFriendshipList)
            {
                ShowTVTab = false;
                UserShortFriendship = userShortFriendshipList.FirstOrDefault();
                UserShort = null;
                Username = null;
            }
            //else if (e.Parameter is InstaUserShortFriendship userShortFriendship)
            //{
            //    UserShortFriendship = userShortFriendship;
            //    UserShort = null;
            //    Username = null;
            //}
            else if (e is string username && !string.IsNullOrEmpty(username))
            {
                ShowTVTab = false;
                Username = username;
                UserShort = null;
                UserShortFriendship = null;
            }
            else if (e is InstaTVSearchResult tvSearch && tvSearch != null)
            {
                ShowTVTab = true;
                UserShort = tvSearch.User;
                UserShortFriendship = null;
                Username = null;
                try
                {
                    UserImage.Fill = tvSearch.User.ProfilePicture.GetImageBrush();

                    ConnectedAnimation imageAnimation =
                        ConnectedAnimationService.GetForCurrentView().GetAnimation("UserImage");
                    if (imageAnimation != null)
                    {
                        ImageAnimationExists = true;
                        imageAnimation.TryStart(UserImage);
                    }
                    else ImageAnimationExists = false;
                }
                catch { }
            }

            else if (e is object[] obj)
            {
                ShowTVTab = false;
                //UserDetailsVM = new UserDetailsViewModel();
                //DataContext = UserDetailsVM;
                //NavigationCacheMode = NavigationCacheMode.Disabled;
                //NavigationCacheMode = NavigationCacheMode.Required;
                UserShort = obj[0] as InstaUserShort;
                Username = null;
                UserShortFriendship = null;
                try
                {
                    UserImage.Fill = obj[1] as Brush;
                    ConnectedAnimation imageAnimation =
                       ConnectedAnimationService.GetForCurrentView().GetAnimation("UserImage");
                    if (imageAnimation != null)
                    {
                        ImageAnimationExists = true;
                        //imageAnimation.Completed += ImageAnimationClose_Completed;
                        //if (LatestGrid?.Name != InfoGrid.Name)
                        //    LatestGrid.Background = new SolidColorBrush(Helper.GetColorFromHex("#FF2E2E2E"));

                        imageAnimation.TryStart(UserImage);
                        //GridShadow.Visibility = GVSHOW.Visibility = Visibility.Collapsed;
                    }
                    else
                        ImageAnimationExists = false;
                }
                catch { }
            }
            LoadData();
        }
        void LoadData()
        {
            try
            {
                UserDetailsVM.ResetToDefault();

                if (ScrollableUserPostUc != null)
                    ScrollableUserPostUc.Visibility = Visibility.Collapsed;
                if (ScrollableUserTaggedPostUc != null)
                    ScrollableUserTaggedPostUc.Visibility = Visibility.Collapsed;
                if (ScrollableUserShopPostUc != null)
                    ScrollableUserShopPostUc.Visibility = Visibility.Collapsed;
                ResetUserImage();
                UserDetailsVM.View = this;
                if (UserShort != null)
                {
                    UserDetailsVM.SetUser(UserShort);
                    ScrollableUserPostUc.SetData(UserShort, UserDetailsVM.MediaGeneratror);
                    ScrollableUserTaggedPostUc.SetData(UserShort, UserDetailsVM.TaggedMediaGeneratror);
                    ScrollableUserShopPostUc.SetData(UserShort, UserDetailsVM.ShopMediaGeneratror);
                }
                else if (UserShortFriendship != null)
                {
                    UserDetailsVM.SetUser(UserShortFriendship);
                    UpdateUserImage(UserShortFriendship.ProfilePicture);
                    ScrollableUserPostUc.SetData(UserShortFriendship.ToUserShort(), UserDetailsVM.MediaGeneratror);
                    ScrollableUserTaggedPostUc.SetData(UserShortFriendship.ToUserShort(), UserDetailsVM.TaggedMediaGeneratror);
                    ScrollableUserShopPostUc.SetData(UserShortFriendship.ToUserShort(), UserDetailsVM.ShopMediaGeneratror);
                }
                else if (!string.IsNullOrEmpty(Username))
                {
                    UserDetailsVM.SetUsername(Username);
                    ScrollableUserPostUc.SetData(new InstaUserShort { UserName = Username }, UserDetailsVM.MediaGeneratror);
                    ScrollableUserTaggedPostUc.SetData(new InstaUserShort { UserName = Username }, UserDetailsVM.TaggedMediaGeneratror);
                    ScrollableUserShopPostUc.SetData(new InstaUserShort { UserName = Username }, UserDetailsVM.ShopMediaGeneratror);
                }
                UserDetailsVM.SetBiographyTextBlock(BiographyText);
                CanLoadFirstPopUp = true;
            }
            catch { }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //NavigationMode = e.NavigationMode;
            //if (NavigationMode != NavigationMode.Back)
            //{
            //    NavigationCacheMode = NavigationCacheMode.Disabled;
            //    NavigationCacheMode = NavigationCacheMode.Enabled;
            //}

            KeyDown += OnKeyDownHandler;
            NavigationMode = e.NavigationMode;
            if (e != null && e.Parameter != null)
            {
                if (e.Parameter is InstaUserShort userShort && userShort != null)
                {
                    ShowTVTab = false;
                    UserShort = userShort;
                    UserShortFriendship = null;
                    Username = null;
                    try
                    {
                        ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("UserImage");
                        if (imageAnimation != null)
                        {
                            ImageAnimationExists = true;
                            imageAnimation.TryStart(UserImage);
                        }
                        else
                            ImageAnimationExists = false;
                    }
                    catch { }
                }
                else if (e.Parameter is List<InstaUserShortFriendship> userShortFriendshipList)
                {
                    ShowTVTab = false;
                    UserShortFriendship = userShortFriendshipList.FirstOrDefault();
                    UserShort = null;
                    Username = null;
                }
                else if (e.Parameter is string username && !string.IsNullOrEmpty(username))
                {
                    ShowTVTab = false;
                    Username = username;
                    UserShort = null;
                    UserShortFriendship = null;
                }
                else if (e.Parameter is InstaTVSearchResult tvSearch && tvSearch != null)
                {
                    ShowTVTab = true;
                    UserShort = tvSearch.User;
                    UserShortFriendship = null;
                    Username = null;
                    try
                    {
                        UserImage.Fill = tvSearch.User.ProfilePicture.GetImageBrush();

                        ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("UserImage");
                        if (imageAnimation != null)
                        {
                            ImageAnimationExists = true;
                            imageAnimation.TryStart(UserImage);
                        }
                        else 
                            ImageAnimationExists = false;
                    }
                    catch { }
                }

                else if (e.Parameter is object[] obj)
                {
                    ShowTVTab = false;
                    UserShort = obj[0] as InstaUserShort;
                    Username = null;
                    UserShortFriendship = null;
                    try
                    {
                        UserImage.Fill = obj[1] as Brush;
                        ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("UserImage");
                        if (imageAnimation != null)
                        {
                            ImageAnimationExists = true;
                            imageAnimation.TryStart(UserImage);
                        }
                        else
                            ImageAnimationExists = false;
                    }
                    catch { }
                }
            }
  
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            try
            {
                if (_scrollingHostDisabled)
                {
                    _scrollingHostDisabled = false;
                    SetInnerScrollMode(false);
                    SCMain.VerticalScrollMode = ScrollMode.Auto;
                    SCMain.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    SCMain.ChangeView(null, 0, null, false);
                }
            }
            catch { }
            KeyDown -= OnKeyDownHandler;
            try
            {
                if (!ImageAnimationExists) return;
                var connectedAnimationService = ConnectedAnimationService.GetForCurrentView();
                connectedAnimationService.DefaultDuration = TimeSpan.FromMilliseconds(850);

                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("UserPictureUserDetailsView",
                    UserImage);
            }
            catch { }
        }
        private void OnKeyDownHandler(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.F5)
                    RefreshControl.RequestRefresh();
            }
            catch { }
        }
        //private void LVRefreshRequested(object sender, EventArgs e)
        //{
        //    UserDetailsVM.Refresh();
        //}
        private void MPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (MPivot.SelectedIndex == 0)
                {
                    UserDetailsVM.MediaGeneratror.IsMine = true;
                    UserDetailsVM.ShopMediaGeneratror.IsMine = false;
                    UserDetailsVM.TVMediaGeneratror.IsMine = false;
                    UserDetailsVM.TaggedMediaGeneratror.IsMine = false;
                }
                else
                {
                    var pi = MPivot.Items[MPivot.SelectedIndex] as PivotItem;
                    if (pi != null)
                    {
                        if (pi.Tag is string str && !string.IsNullOrEmpty(str))
                        {
                            if (str == "Tag")
                            {
                                UserDetailsVM.TaggedMediaGeneratror.IsMine = true;
                                UserDetailsVM.TVMediaGeneratror.IsMine = false;
                                UserDetailsVM.MediaGeneratror.IsMine = false;
                                UserDetailsVM.ShopMediaGeneratror.IsMine = false;
                                if (UserDetailsVM.TaggedMediaGeneratror.Items.Count == 0)
                                    UserDetailsVM.GetTaggedPosts(true);
                            }
                            else if (str == "TV")
                            {
                                UserDetailsVM.TVMediaGeneratror.IsMine = true;
                                UserDetailsVM.TaggedMediaGeneratror.IsMine = false;
                                UserDetailsVM.MediaGeneratror.IsMine = false;
                                UserDetailsVM.ShopMediaGeneratror.IsMine = false;
                                if (UserDetailsVM.TVMediaGeneratror.Items.Count == 0)
                                    UserDetailsVM.GetTVPosts(true);
                            }
                            else if (str == "Shop")
                            {
                                UserDetailsVM.ShopMediaGeneratror.IsMine = true;
                                UserDetailsVM.TVMediaGeneratror.IsMine = false;
                                UserDetailsVM.TaggedMediaGeneratror.IsMine = false;
                                UserDetailsVM.MediaGeneratror.IsMine = false;
                                if (UserDetailsVM.ShopMediaGeneratror.Items.Count == 0)
                                    UserDetailsVM.GetShoppablePosts(true);
                            }
                        }
                    }
                }
            }
            catch { }
        }
        public void ResetTabs()
        {
            if (IsTabCreated) return;
            try
            {
                if (MPivot.Items.Count > 0)
                {
                    //MPivot.Items.Clear();
                    var count = MPivot.Items.Count;
                    for (int i = count - 1; i > 0; i--)
                        MPivot.Items.RemoveAt(i);
                }
            }
            catch { }
        }
        public bool IsTabCreated = false;
        public void CreateTabs(bool igtv =false,bool shop = false, bool tagged = false)
        {
            if (IsTabCreated) return;
            IsTabCreated = true;
            try
            {
                //try
                //{
                //    var gv = GenerateGV();
                //    gv.Loaded += GvMediaGeneratrorLoaded;
                //    gv.ItemsSource = UserDetailsVM.MediaGeneratror.Items;
                //    gv.ItemClick += LVItemClick;
                //    var pi = GeneratePivotItem("", "Media");
                //    pi.Content = gv;
                //    MPivot.Items.Add(pi);
                //}
                //catch { }
                if (igtv)
                {
                    var gv = GenerateGV(true);
                    gv.DesiredWidth = 210;
                    //gv.width = 160;
                    gv.ItemTemplateSelector = null;
                    gv.ItemTemplate = Resources["TVTemplate"] as DataTemplate;
                    gv.Loaded += GvTVMediaGeneratrorLoaded;
                    gv.ItemClick += TVLVItemClick;
                    gv.ItemsSource = UserDetailsVM.TVMediaGeneratror.Items;
                    var pi = GeneratePivotItem("", "TV");
                    pi.Content = gv;
                    MPivot.Items.Add(pi);
                }
                if (shop)
                {
                    var gv = GenerateGV();
                    gv.ItemHeight = 200;
                    gv.Loaded += GvShopMediaGeneratrorLoaded;
                    gv.ItemsSource = UserDetailsVM.ShopMediaGeneratror.Items;
                    gv.ItemClick += ShopLVItemClick;
                    var pi = GeneratePivotItem("", "Shop");
                    pi.Content = gv;
                    MPivot.Items.Add(pi);
                }
                if (tagged)
                {
                    var gv = GenerateGV();
                    gv.ItemHeight = 200;
                    gv.ItemsSource = UserDetailsVM.TaggedMediaGeneratror.Items;
                    gv.Loaded += GvTaggedMediaGeneratrorLoaded;
                    gv.ItemClick += TaggedLVItemClick;
                    var pi = GeneratePivotItem("", "Tag");
                    pi.Content = gv;
                    MPivot.Items.Add(pi);
                }
                if (MPivot.Items.Count > 1&& ShowTVTab)
                {
                    for (int i = 0; i < MPivot.Items.Count; i++)
                    {
                        try
                        {
                            if (MPivot.Items[i] is PivotItem pi && pi.Tag is string str && !string.IsNullOrEmpty(str))
                            {
                                if (str.ToLower() == "tv")
                                {
                                    MPivot.SelectedIndex = i;
                                    return;
                                }
                            }
                        }
                        catch { }
                    }
                    ShowTVTab = false;
                }
            }
            catch(Exception ex)
            {
                ex.PrintException("CreateTabs");
            }
        }

        private async void TVLVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is InstaMedia media/* && sender is AdaptiveGridViewX gv*/)
                {
                    var index = UserDetailsVM.TVMediaGeneratror.Items.IndexOf(media);
                    await Task.Delay(350);
                    Helpers.NavigationService.Navigate(typeof(TV.TVPlayer), 
                        new object[] { UserDetailsVM.TVMediaGeneratror.Items.ToList(), index, UserDetailsVM.User.ToUserShort() });
                }
            }
            catch { }
        }

        private async void TaggedLVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is InstaMedia media)
                {
                    var index = UserDetailsVM.TaggedMediaGeneratror.Items.IndexOf(media);

                    ScrollableUserTaggedPostUc.Visibility = Visibility.Visible;
                    await Task.Delay(350);
                    ScrollableUserTaggedPostUc.ScrollTo(index);
                }
            }
            catch { }
        }
        private async void ShopLVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is InstaMedia media)
                {
                    var index = UserDetailsVM.ShopMediaGeneratror.Items.IndexOf(media);

                    ScrollableUserShopPostUc.Visibility = Visibility.Visible;
                    await Task.Delay(350);
                    ScrollableUserShopPostUc.ScrollTo(index);
                }
            }
            catch { }
        }
        private void GvMediaGeneratrorLoaded(object sender, RoutedEventArgs e)
        {
            "GvMediaGeneratrorLoaded".PrintDebug();
            S1 = (sender as AdaptiveGridViewOriginal).FindScrollViewer();
            //S1 = (sender as AdaptiveGridViewX).FindScrollViewer();
            if (S1 != null)
            {
                S1.ViewChanging += OnSCViewChanging;
                S1.ViewChanged += InnerLVsScrollViewViewChanged;
            }
            UserDetailsVM.MediaGeneratror.SetLV(S1);
        }
        private void GvTVMediaGeneratrorLoaded(object sender, RoutedEventArgs e)
        {
            S2 = (sender as AdaptiveGridViewOriginal).FindScrollViewer();
            if (S2 != null)
            {
                //if (SCMain.VerticalScrollMode == ScrollMode.Disabled)
                //    S2.EnableScroll();
                S2.ViewChanging += OnSCViewChanging;
                S2.ViewChanged += InnerLVsScrollViewViewChanged;
            }
            UserDetailsVM.TVMediaGeneratror.SetLV(S2);
        }

        private void GvShopMediaGeneratrorLoaded(object sender, RoutedEventArgs e)
        {
            S3 = (sender as AdaptiveGridViewOriginal).FindScrollViewer();
            if (S3 != null)
            {
                //if (SCMain.VerticalScrollMode == ScrollMode.Disabled)
                //    S3.EnableScroll();
                S3.ViewChanging += OnSCViewChanging;
                S3.ViewChanged += InnerLVsScrollViewViewChanged;
            }
            UserDetailsVM.TVMediaGeneratror.SetLV(S3);
        }
        private void GvTaggedMediaGeneratrorLoaded(object sender, RoutedEventArgs e)
        {
            S4 = (sender as AdaptiveGridViewOriginal).FindScrollViewer();
            if (S4 != null)
            {
                //if (SCMain.VerticalScrollMode == ScrollMode.Disabled)
                //    S4.EnableScroll();
                S4.ViewChanging += OnSCViewChanging;
                S4.ViewChanged += InnerLVsScrollViewViewChanged;
            }
            UserDetailsVM.TaggedMediaGeneratror.SetLV(S4);
        }

        PivotItem GeneratePivotItem(string glyph, string tag)
        {
            //<PivotItem.Header>
            //                       <Grid  MinWidth="90">
            //                           <FontIcon FontFamily="{StaticResource MaterialSymbolFont}"
            //                                     FontSize="{StaticResource ExtraBigFontSize}"
            //                                     Glyph="" />
            //                       </Grid>
            //                   </PivotItem.Header>
            var ff = App.Current.Resources["MaterialSymbolFont"] as FontFamily;
            var fs = (double)App.Current.Resources["ExtraBigFontSize"];// inja faghat
            var fg = (SolidColorBrush)App.Current.Resources["DefaultForegroundColor"];

            var item = new PivotItem
            {
                Header = new Grid
                {
                    MinWidth = 85,
                    Children =
                    {
                        new FontIcon
                        {
                            FontFamily = ff,
                            FontSize =fs,
                            Glyph = glyph,
                            Foreground = fg
                        }
                    }
                },
                Tag = tag
            };
            return item;
        }
        AdaptiveGridViewOriginal GenerateGV(bool tv = false)
        {
            var type = Resources["MediaUserInfoTemplateSelector"].GetType();
            var tms = Resources["MediaUserInfoTemplateSelector"] as DataTemplateSelector;
            var cs = Resources["GridViewItemContainerStyle"] as Style;
            var gv = new AdaptiveGridViewOriginal
            {
                //Margin = new Thickness(5),
                DesiredWidth = 180,
                //MinItemHeight = 140,
                //MinItemWidth = 140,
                SelectionMode = ListViewSelectionMode.None,
                IsItemClickEnabled = true,
                ItemTemplateSelector = tms,
                ItemContainerStyle = cs,
            };
            if (tv)
                gv.HeightEqualsWidth = false;
            gv.ContainerContentChanging += LVContainerContentChanging;

            gv.ItemContainerTransitions = new TransitionCollection
            {
                new EntranceThemeTransition()
            };
            return gv;
        }

        private /*async*/ void RefreshControlRefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            // Respond to a request by performing a refresh and using the deferral object.
            using (var RefreshCompletionDeferral = args.GetDeferral())
            {
                try
                {
                    if (MPivot.SelectedIndex == 0)
                        UserDetailsVM.Refresh();
                    else
                    {
                        var pi = MPivot.Items[MPivot.SelectedIndex] as PivotItem;
                        if (pi != null)
                        {
                            if (pi.Tag is string str && !string.IsNullOrEmpty(str))
                            {
                                if (str == "Tag")
                                {
                                    UserDetailsVM.Refresh(false, false, true, false);
                                }
                                else if (str == "TV")
                                {
                                    UserDetailsVM.Refresh(false, true, false, false);

                                }
                                else if (str == "Shop")
                                {
                                    UserDetailsVM.Refresh(false, false, false, true);
                                }
                            }
                        }
                    }
                }
                catch { }

            }
        }

        private void RefreshControlRefreshStateChanged(Microsoft.UI.Xaml.Controls.RefreshVisualizer sender, Microsoft.UI.Xaml.Controls.RefreshStateChangedEventArgs args)
        {
            // Respond to visualizer state changes.
            // Disable the refresh button if the visualizer is refreshing.
            //("NewState::: " + args.NewState).PrintDebug();
            if (args.NewState == Microsoft.UI.Xaml.Controls.RefreshVisualizerState.Refreshing)
            {
                RefreshButton.IsEnabled = false;
            }
            else
            {
                RefreshButton.IsEnabled = true;
            }
        }







        private async void LVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is InstaMedia media)
                {
                    var index = UserDetailsVM.MediaGeneratror.Items.IndexOf(media);

                    //Helpers.NavigationService.Navigate(typeof(Posts.ScrollableUserPostView), 
                    //    new object[] { UserDetailsVM.User, UserDetailsVM.MediaGeneratror , index});
                    ScrollableUserPostUc.Visibility = Visibility.Visible;
                    await Task.Delay(350);
                    ScrollableUserPostUc.ScrollTo(index);
                }

            }
            catch { }
        }

        private void LVContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            try
            {
                var elementVisual = ElementCompositionPreview.GetElementVisual(args.ItemContainer);
                if (args.InRecycleQueue)
                {
                    elementVisual.ImplicitAnimations = null;
                }
                else
                {
                    //Add implicit animation to each visual 
                    elementVisual.ImplicitAnimations = _elementImplicitAnimation;
                }
            }
            catch { }
        }


        #region Animation

        private CompositionAnimationGroup CreateOpacityAnimation()
        {
            ScalarKeyFrameAnimation fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1.0f, 0.0f);
            fadeAnimation.Duration = TimeSpan.FromSeconds(.35);
            fadeAnimation.Target = "Opacity";
            //Add Animations to Animation group. 
            CompositionAnimationGroup animationGroup = _compositor.CreateAnimationGroup();
            animationGroup.Add(fadeAnimation);

            return animationGroup;
        }
        private CompositionAnimationGroup CreateOffsetAnimation()
        {

            //Define Offset Animation for the ANimation group
            Vector3KeyFrameAnimation offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromSeconds(.4);

            //Define Animation Target for this animation to animate using definition. 
            offsetAnimation.Target = "Offset";


            ScalarKeyFrameAnimation fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1f, 0.8f);
            fadeAnimation.Duration = TimeSpan.FromSeconds(.4);
            fadeAnimation.Target = "Opacity";




            ////Define Rotation Animation for Animation Group. 
            //ScalarKeyFrameAnimation rotationAnimation = _compositor.CreateScalarKeyFrameAnimation();
            //rotationAnimation.InsertKeyFrame(.5f, 0.160f);
            //rotationAnimation.InsertKeyFrame(1f, 0f);
            //rotationAnimation.Duration = TimeSpan.FromSeconds(.4);

            ////Define Animation Target for this animation to animate using definition. 
            //rotationAnimation.Target = "RotationAngle";

            //Add Animations to Animation group. 
            CompositionAnimationGroup animationGroup = _compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);
            //animationGroup.Add(rotationAnimation);
            animationGroup.Add(fadeAnimation);

            return animationGroup;
        }
        #endregion




















        private double _lastVerticalOffset;
        private bool _isHideTitleGrid;






        void HandleGoUpRefreshButtons(ScrollViewer scrollViewer)
        {
            try
            {
                if ((scrollViewer.VerticalOffset - _lastVerticalOffset) > 5 && !_isHideTitleGrid)
                {
                    _isHideTitleGrid = true;
                    ToggleGoUpButtonAnimation(false);
                    ToggleRefreshButtonAnimation(false);
                }
                else if (scrollViewer.VerticalOffset < _lastVerticalOffset && _isHideTitleGrid)
                {
                    _isHideTitleGrid = false;
                    ToggleGoUpButtonAnimation(true);
                    ToggleRefreshButtonAnimation(true);
                }
                if (scrollViewer.VerticalOffset == 0)
                {
                    _isHideTitleGrid = true;
                    ToggleGoUpButtonAnimation(false);
                    ToggleRefreshButtonAnimation(false);
                }
                _lastVerticalOffset = scrollViewer.VerticalOffset;
            }
            catch { }
        }
        private void ToggleGoUpButtonAnimation(bool show)
        {
            var scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(1f, show ? 1f : 0);
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);

            _goUpButtonVisual.CenterPoint = new Vector3((float)GoUpButton.ActualWidth / 2f, (float)GoUpButton.ActualHeight / 2f, 0f);
            _goUpButtonVisual.StartAnimation("Scale.X", scaleAnimation);
            _goUpButtonVisual.StartAnimation("Scale.Y", scaleAnimation);
        }
        private void ToggleRefreshButtonAnimation(bool show)
        {
            var scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(1f, show ? 1f : 0);
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);

            _refreshButtonVisual.CenterPoint = new Vector3((float)GoUpButton.ActualWidth / 2f, (float)GoUpButton.ActualHeight / 2f, 0f);
            _refreshButtonVisual.StartAnimation("Scale.X", scaleAnimation);
            _refreshButtonVisual.StartAnimation("Scale.Y", scaleAnimation);
        }

        #region Click and tap events 
        private void GoUpButtonClick(object sender, RoutedEventArgs e)
        {
            SCMain.ScrollToElement(0);
            //LVStories.ScrollIntoView()
        }

        private void HyperlinkButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UserDetailsVM.User.ExternalUrl.OpenUrl();
            }
            catch { }
        }

        private void LVHighlightsItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (sender is ListView listView && listView != null)
                {
                    if (e.ClickedItem is InstaHighlightFeed reelFeed && reelFeed != null)
                    {
                        var index = LVHighlights.Items.IndexOf(reelFeed);
                        var list = new List<InstaReelFeed>();
                        foreach (var item in UserDetailsVM.Highlights)
                            list.Add(item.ToReelFeed());

                        Helpers.NavigationService.Navigate(typeof(Main.StoryView), new object[] { list, index });
                    }
                }
            }
            catch { }
        }

        private void UserImageTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {

                if ((UserDetailsVM.Stories == null && UserDetailsVM.Broadcast == null) ||
                    (UserDetailsVM.Stories != null && UserDetailsVM.Stories.Count == 0 && UserDetailsVM.Broadcast == null))
                    Helpers.NavigationService.Navigate(typeof(ImageVideoView), UserDetailsVM.User);
                else
                {
                    UserImageFlyout.Items.Clear();
                    var openPicture = new MenuFlyoutItem
                    {
                        Text = "Open profile picture",
                        //Height = 48
                    };
                    openPicture.Click += MenuOpenPictureClick;

                    UserImageFlyout.Items.Add(openPicture);
                    if (UserDetailsVM.Stories.Count > 0)
                    {
                        var openStory = new MenuFlyoutItem
                        {
                            Text = "Open stories",
                        };
                        openStory.Click += MenuOpenStoryClick;
                        UserImageFlyout.Items.Add(openStory);
                    }
                    if (UserDetailsVM.Broadcast != null)
                    {
                        var openLive = new MenuFlyoutItem
                        {
                            Text = "Open live broadcast",
                        };
                        openLive.Click += MenuOpenLiveClick;
                        UserImageFlyout.Items.Add(openLive);
                    }
                    try
                    {
                        FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                    }
                    catch { }
                }

                //Helpers.NavigationService.Navigate(typeof(Main.StoryView), new object[] { UserDetailsVM.Stories.ToList(), 0 });
            }
            catch { }
        }

        private void MenuOpenLiveClick(object sender, RoutedEventArgs e) => Helper.OpenLive(UserDetailsVM.Broadcast);

        private void MenuOpenPictureClick(object sender, RoutedEventArgs e) =>
            Helpers.NavigationService.Navigate(typeof(ImageVideoView), UserDetailsVM.User);

        private void MenuOpenStoryClick(object sender, RoutedEventArgs e) =>
            Helpers.NavigationService.Navigate(typeof(Main.StoryView), new object[] { UserDetailsVM.Stories.ToList(), 0 });

        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            //UserDetailsVM.Refresh();

            RefreshControl.RequestRefresh();
        }


        private void SeeAllButtonClick(object sender, RoutedEventArgs e)
        {

        }
        private async void FollowUnfollowButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn != null && btn.DataContext is InstaUserChaining user)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        if (user.FollowText == "Follow")
                        {
                            var result = await Helper.InstaApi.UserProcessor.FollowUserAsync(user.Pk);
                            if (result.Succeeded)
                            {
                                if (result.Value.OutgoingRequest)
                                    user.FollowText = "Requested";
                                else if (result.Value.Following)
                                    user.FollowText = "Unfollow";
                            }
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                                else
                                    Helper.ShowNotify($"Error while sending follow request to '{user.UserName}.\r\n" +
                                 $"Error message: {result.Info?.Message}", 2000);
                            }
                        }
                        else if (user.FollowText == "Unfollow")
                        {
                            var result = await Helper.InstaApi.UserProcessor.UnFollowUserAsync(user.Pk);
                            if (result.Succeeded)
                                user.FollowText = "Follow";
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                                else
                                    Helper.ShowNotify($"Error while sending follow request to '{user.UserName}.\r\n" +
                                 $"Error message: {result.Info?.Message}", 2000);
                            }
                        }
                        //btn.DataContext = user;
                    });

                }
            }
            catch { }
        }

        private async void FollowUnFollowMainButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn != null)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        if (btn.Content.ToString() == "Follow")
                        {
                            var result = await Helper.InstaApi.UserProcessor.FollowUserAsync(UserDetailsVM.User.Pk);
                            if (result.Succeeded)
                                UserDetailsVM.FriendshipStatus = result.Value?.ToStoryFriendshipStatus();
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                                else
                                    Helper.ShowNotify($"Error while sending follow request to '{UserDetailsVM.User.UserName}.\r\n" +
                                         $"Error message: {result.Info?.Message}", 2000);
                            }
                        }
                        else if (btn.Content.ToString() == "Unfollow" || btn.Content.ToString() == "Requested")
                        {
                            var result = await Helper.InstaApi.UserProcessor.UnFollowUserAsync(UserDetailsVM.User.Pk);
                            if (result.Succeeded)
                                UserDetailsVM.FriendshipStatus = result.Value?.ToStoryFriendshipStatus();
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                                else
                                    Helper.ShowNotify($"Error while sending follow request to '{UserDetailsVM.User.UserName}.\r\n" +
                                    $"Error message: {result.Info?.Message}", 2000);
                            }
                        }
                        else if (btn.Content.ToString() == "Unblock")
                        {
                            var result = await Helper.InstaApi.UserProcessor.UnBlockUserAsync(UserDetailsVM.User.Pk);
                            if (result.Succeeded)
                            {
                                UserDetailsVM.FriendshipStatus = result.Value?.ToStoryFriendshipStatus();
                                UserDetailsVM.Refresh();
                            }
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                                else
                                    Helper.ShowNotify($"Error while sending follow request to '{UserDetailsVM.User.UserName}.\r\n" +
                                         $"Error message: {result.Info?.Message}", 2000);
                            }
                        }
                        else
                        if (btn.Content.ToString() == "Follow Back")
                        {
                            var result = await Helper.InstaApi.UserProcessor.FollowUserAsync(UserDetailsVM.User.Pk);
                            if (result.Succeeded)
                                UserDetailsVM.FriendshipStatus = result.Value?.ToStoryFriendshipStatus();
                            else
                            {
                                if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                    Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                                else
                                    Helper.ShowNotify($"Error while sending follow request to '{UserDetailsVM.User.UserName}.\r\n" +
                                         $"Error message: {result.Info?.Message}", 2000);
                            }
                        }
                    });
                }
            }
            catch { }
        }

        private void FollowersButtonClick(object sender, RoutedEventArgs e)
        {
            if (UserDetailsVM.FriendshipStatus != null)
                if (UserDetailsVM.FriendshipStatus.IsPrivate && !UserDetailsVM.FriendshipStatus.Following)
                {
                    Helper.ShowNotify($"Since ${UserDetailsVM.User.UserName} is a private account, you are not allowed to see their followers/followings.\r\nFollow this user to see their followers/followings.", 2000);
                    return;
                }
            Helpers.NavigationService.Navigate(typeof(FollowView), new object[] { UserDetailsVM.User.ToUserShort(), 0 });
        }

        private void FollowingButtonClick(object sender, RoutedEventArgs e)
        {
            if (UserDetailsVM.FriendshipStatus != null)
                if (UserDetailsVM.FriendshipStatus.IsPrivate && !UserDetailsVM.FriendshipStatus.Following)
                {
                    Helper.ShowNotify($"Since ${UserDetailsVM.User.UserName} is a private account, you are not allowed to see their followers/followings.\r\nFollow this user to see their followers/followings.", 2000);
                    return;
                }
            Helpers.NavigationService.Navigate(typeof(FollowView), new object[] { UserDetailsVM.User.ToUserShort(), 1 });
        }

        private void CopyBiographyMenuFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UserDetailsVM.User.Biography.CopyText();
                Helper.ShowNotify($"{UserDetailsVM.User.UserName}'s biography copied ;)");
            }
            catch { }
        }

        private void BiographyGridTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            catch { }
        }

        private void CopyUsernameMenuFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UserDetailsVM.User.UserName.CopyText();
                Helper.ShowNotify($"{UserDetailsVM.User.UserName} copied ;)");
            }
            catch { }
        }
        private void CopyUsernameAddressMenuFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var urlAddress = Helper.InstagramUrl + UserDetailsVM.User.UserName;
                urlAddress.CopyText();
                Helper.ShowNotify($"{urlAddress} copied ;)");
            }
            catch { }
        }
        private void UsernameGridTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            catch { }
        }

        private void CopyExternalUrlMenuFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UserDetailsVM.User.ExternalUrl.CopyText();
                Helper.ShowNotify("Url copied ;)");
            }
            catch { }
        }

        private void ChainingItemTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid grid && grid != null && grid.DataContext is InstaUserChaining user)
                    Helper.OpenProfile(user.ToUserShortFriendship()/*.ToListExt()*/);
            } 
            catch { }
        }

        private async void MoreButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await new ContentDialogs.UserProfileMenusDialog(UserDetailsVM.User, UserDetailsVM.FriendshipStatus).ShowAsync();
            }
            catch { }
        }
#endregion Click and tap events 

        #region Scroll handling
        private ScrollViewer S1, S2, S3, S4;

        //private bool tryingEnableSCs = false;
        ////bool isMainScrollEnabled = true;

        //private double _lastMainVerticalOffset;
        //private bool _isMainHideTitleGrid;
        //private bool _triedFirst = false;
        //private double lastSC1Offset = 0;
        //private double lastSC2Offset = 0;
        //private double lastSC3Offset = 0;
        //private double lastSC4Offset = 0;
        private /*async*/ void ScrollViewViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                var scrollViewer = sender as ScrollViewer;
                HandleGoUpRefreshButtons(scrollViewer);
                //Helper.DebugLogger.LogLevel = LogLevel.None;
                //return;
                //if (scrollViewer.VerticalOffset >= GridMainScrollViewer.ActualHeight && !tryingEnableSCs)
                //{
                //    tryingEnableSCs = true;
                //    scrollViewer.DisableScroll();
                //    S1?.EnableScroll();
                //    S2?.EnableScroll();
                //    S3?.EnableScroll();
                //    S4?.EnableScroll();

                //    //isMainScrollEnabled = false;
                //    ("DISABELING SC MAIN").PrintDebug();
                //    GridMainScrollViewer.Height = 0;
                //    //GridMainScrollViewer.Visibility = Visibility.Collapsed;
                //    //SCMain.ChangeView(null, GridMainScrollViewer.ActualHeight, null);
                //    try
                //    {
                //        await Task.Delay(40);

                //        S1?.ChangeView(null, lastSC1Offset, null);
                //        S2?.ChangeView(null, lastSC2Offset, null);
                //        S3?.ChangeView(null, lastSC3Offset, null);
                //        S4?.ChangeView(null, lastSC4Offset, null);

                //    }
                //    catch { }
                //    await Task.Delay(500);
                //    tryingEnableSCs = false;
                //}
            }
            catch { }
        }

        private /*async*/ void OnSCViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            //if (S1 != null)
            //    "if (S1 != null) NABOd".PrintDebug();
            //if (S2 != null)
            //    "if (S2 != null) NABOd".PrintDebug();
            //if (S3 != null)
            //    "if (S3 != null) NABOd".PrintDebug();
            //if (S4 != null)
            //    "if (S4 != null) NABOd".PrintDebug();
            HandleGoUpRefreshButtons(scrollViewer);
            return;

            //if ((scrollViewer.VerticalOffset - _lastMainVerticalOffset) > 5 && !_isMainHideTitleGrid)
            //{
            //    _isMainHideTitleGrid = true;
            //}
            //else if (scrollViewer.VerticalOffset < _lastMainVerticalOffset && _isMainHideTitleGrid)
            //{
            //    _isMainHideTitleGrid = false;
            //    "2".PrintDebug();
            //    if (scrollViewer.VerticalOffset < 5.0)
            //    {
            //        //SCMain.EnableScroll();
            //        return;
            //    }
            //}
            //if (scrollViewer.VerticalOffset < 5.0)
            //{
            //    "3".PrintDebug();

            //    _isMainHideTitleGrid = true;

            //}
            //if (_lastMainVerticalOffset > scrollViewer.VerticalOffset)
            //{
            //    if (scrollViewer.VerticalOffset < 5.0 && _isMainHideTitleGrid && !_triedFirst)
            //    {
            //        _triedFirst = true;
            //        ("DISABELING SC1 SC2 S3 S4").PrintDebug();
            //        if (S1 != null)
            //            lastSC1Offset = S1.VerticalOffset;
            //        if (S2 != null)
            //            lastSC2Offset = S2.VerticalOffset;
            //        if (S3 != null)
            //            lastSC3Offset = S3.VerticalOffset;
            //        if (S4 != null)
            //            lastSC4Offset = S4.VerticalOffset;

            //        S1?.DisableScroll();
            //        S2?.DisableScroll();
            //        S3?.DisableScroll();
            //        S4?.DisableScroll();
            //        //scrollViewer.DisableScroll();

            //        await Task.Delay(150);
            //        SCMain.EnableScroll();
            //        GridMainScrollViewer.Height = double.NaN;
            //        //GridMainScrollViewer.Visibility = Visibility.Visible;
            //        await Task.Delay(10);
            //        SCMain.EnableScroll();
            //        SCMain.ChangeView(null, GridMainScrollViewer.ActualHeight - 10, null);
            //        await Task.Delay(750);
            //        _triedFirst = false;

            //    }
            //}
            //_lastMainVerticalOffset = scrollViewer.VerticalOffset;
        }

        private void IGTVVListViewItemTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if(MPivot.Items.Count>1)
                    for (int i = 0; i < MPivot.Items.Count; i++)
                    {
                        try
                        {
                            if (MPivot.Items[i] is PivotItem pi && pi.Tag is string str && !string.IsNullOrEmpty(str))
                            {
                                if (str.ToLower() == "tv")
                                {
                                    MPivot.SelectedIndex = i;
                                    return;
                                }
                            }
                        }
                        catch { }
                    }
            }
            catch { }
        }
        #endregion Scroll handling

        #region Scroll v2
        private double _scrollingHost;
        private bool _scrollingHostDisabled = false;

        private double _innerScrolls;
        private bool _innerScrollsDisabled = true;
        private void ScrollViewViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (_scrollingHostDisabled)
            {
                if (!e.IsIntermediate)
                {
                    scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null, true);
                }

                _scrollingHost = scrollViewer.VerticalOffset;
                return;
            }

            if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 12 && _scrollingHost < scrollViewer.VerticalOffset)
            {
                _scrollingHostDisabled = true;
                SetScrollMode(false);

                SetInnerScrollMode(true);
                _innerScrollsDisabled = false;
            }

            _scrollingHost = scrollViewer.VerticalOffset;
        }

        private void InnerLVsScrollViewViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer2 = sender as ScrollViewer;
            if (_innerScrollsDisabled)
            {
                if (!e.IsIntermediate)
                {
                    scrollViewer2.ChangeView(null, 12, null, false);
                }

                _innerScrolls = scrollViewer2.VerticalOffset;
                return;
            }

            if (scrollViewer2.VerticalOffset <= 12 && _innerScrolls > scrollViewer2.VerticalOffset)
            {
                SetScrollMode(true);
                _scrollingHostDisabled = false;

                _innerScrollsDisabled = true;
                SetInnerScrollMode(false);
            }

            _innerScrolls = scrollViewer2.VerticalOffset;
        }
        private IEnumerable<ScrollViewer> GetScrollViewers()
        {
            if (S1 != null)
                yield return S1;

            if (S2 != null)
                yield return S2;
            if (S3 != null)
                yield return S3;

            if (S4 != null)
                yield return S4;
        }

        public void SetInnerScrollMode(bool enable)
        {
            foreach (var scrollViewer in GetScrollViewers())
            {
                if (enable)
                {
                    scrollViewer.VerticalScrollMode = ScrollMode.Auto;
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scrollViewer.ChangeView(null, 12, null, true);
                }
                else
                {
                    scrollViewer.ChangeView(null, 12, null, true);
                    scrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
            }
        }
        private void SetScrollMode(bool enable)
        {
            if (enable)
            {
                SCMain.VerticalScrollMode = ScrollMode.Auto;
                SCMain.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                SCMain.ChangeView(null, SCMain.ScrollableHeight - 48, null, false);
            }
            else
            {
                SCMain.ChangeView(null, SCMain.ScrollableHeight, null, true);
                SCMain.VerticalScrollMode = ScrollMode.Disabled;
                SCMain.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }
        }
        #endregion

        #region LOADINGS
        public void ShowTopLoading() => TopLoading.Start();
        public void HideTopLoading() => TopLoading.Stop();


        public void ShowBottomLoading() => BottomLoading.Start();


        public void HideBottomLoading() => BottomLoading.Stop();
        #endregion LOADINGS

    }
}
