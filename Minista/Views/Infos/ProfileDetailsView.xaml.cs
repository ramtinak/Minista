using InstagramApiSharp.Classes.Models;
using Minista.Models;
using Minista.ViewModels.Infos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class ProfileDetailsView : Page
    {
        readonly Compositor _compositor;
        readonly ImplicitAnimationCollection _elementImplicitAnimation;
        private readonly Visual _goUpButtonVisual, _refreshButtonVisual;
        //ScrollViewer ScrollView;
        //public ProfileDetailsViewModel ProfileDetailsVM { get; set; } = new ProfileDetailsViewModel();
        bool ImageAnimationExists = false;
        public static ProfileDetailsView Current;
        private bool CanLoadFirstPopUp = false;
        NavigationMode NavigationMode;
        public Thickness StoryStrokeThickness { get; set; } = new Thickness();
        public ProfileDetailsView()
        {
            this.InitializeComponent();
            Current = this;
            //DataContext = ProfileDetailsVM;
            //NavigationCacheMode = NavigationCacheMode.Enabled;
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Create ImplicitAnimations Collection. 
            _elementImplicitAnimation = _compositor.CreateImplicitAnimationCollection();

            //Define trigger and animation that should play when the trigger is triggered. 
            //_elementImplicitAnimation["Offset"] = CreateOffsetAnimation();
            _elementImplicitAnimation["Opacity"] = CreateOpacityAnimation();

            _goUpButtonVisual = GoUpButton.GetVisual();
            _refreshButtonVisual = RefreshButton.GetVisual();
            Loaded += ProfileDetailsViewLoaded;
            //LV.ScrollChanged += LVScrollChanged;
            EditProfileUc.OnCompleted += EditProfileUc_OnCompleted;
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

        //        ProfileDetailsVM.SetScrollViewer(ScrollView);
        //    }
        //}

        public void UpdateUserImage(string img)
        {
            try
            {
                UserImage.Fill = img.GetImageBrush();
            }
            catch { }
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
            ProfileDetailsVM?.SetBroadcast(broadcast);
        }
        //NavigationMode NavigationMode = NavigationMode.New;
        bool SetInfo = false;
        private void ProfileDetailsViewLoaded(object sender, RoutedEventArgs e)
        {
            //Loaded -= ProfileDetailsViewLoaded;
            try
            {
                RefreshControl.RefreshRequested -= RefreshControlRefreshRequested;
                RefreshControl.Visualizer.RefreshStateChanged -= RefreshControlRefreshStateChanged;
            }
            catch { }
            RefreshControl.RefreshRequested += RefreshControlRefreshRequested;
            if (RefreshControl.Visualizer != null)
                RefreshControl.Visualizer.RefreshStateChanged += RefreshControlRefreshStateChanged;
            Helper.CreateCachedFolder();
            ToggleGoUpButtonAnimation(false);
            ToggleRefreshButtonAnimation(false);
            if(NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();
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
                ProfileDetailsVM.View = this;

                ProfileDetailsVM.SetBiographyTextBlock(BiographyText);
                if (!SetInfo)
                {
                    ProfileDetailsVM.SetInfo();
                    SetInfo = true;
                }
                ScrollableUserPostUc.SetData(Helper.CurrentUser.ToUserShort(), ProfileDetailsVM.MediaGeneratror);
                ScrollableUserTaggedPostUc.SetData(Helper.CurrentUser.ToUserShort(), ProfileDetailsVM.TaggedMediaGeneratror);
                ScrollableUserShopPostUc.SetData(Helper.CurrentUser.ToUserShort(), ProfileDetailsVM.ShopMediaGeneratror);
                CanLoadFirstPopUp = true;
            }
        }
        private void RefreshControlRefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            using (var RefreshCompletionDeferral = args.GetDeferral())
            {
            }
            try
            {
                if (MPivot.SelectedIndex == 0)
                    ProfileDetailsVM.Refresh();
                else
                {
#pragma warning disable IDE0019 // Use pattern matching
                    var pi = MPivot.Items[MPivot.SelectedIndex] as PivotItem;
#pragma warning restore IDE0019 // Use pattern matching
                    if (pi != null)
                    {
                        if (pi.Tag is string str && !string.IsNullOrEmpty(str))
                        {
                            if (str == "Tag")
                            {
                                ProfileDetailsVM.Refresh(false, false, true, false);
                            }
                            else if (str == "TV")
                            {
                                ProfileDetailsVM.Refresh(false, true, false, false);

                            }
                            else if (str == "Shop")
                            {
                                ProfileDetailsVM.Refresh(false, false, false, true);
                            }
                        }
                    }
                }
            }
            catch { }
        }
        private void RefreshControlRefreshStateChanged(Microsoft.UI.Xaml.Controls.RefreshVisualizer sender, Microsoft.UI.Xaml.Controls.RefreshStateChangedEventArgs args)
        {
            if (args.NewState == Microsoft.UI.Xaml.Controls.RefreshVisualizerState.Refreshing)
            {
                RefreshButton.IsEnabled = false;
            }
            else
            {
                RefreshButton.IsEnabled = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationMode = e.NavigationMode;
            KeyDown += OnKeyDownHandler;
            //if (NavigationMode != NavigationMode.Back)
            //{
            //    NavigationCacheMode = NavigationCacheMode.Disabled;
            //    NavigationCacheMode = NavigationCacheMode.Enabled;
            //}

            if (e != null && e.Parameter != null)
            {
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

        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
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
        //    ProfileDetailsVM.Refresh();
        //}
        private void MPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (MPivot.SelectedIndex == 0)
                {
                    ProfileDetailsVM.MediaGeneratror.IsMine = true;
                    ProfileDetailsVM.ShopMediaGeneratror.IsMine = false;
                    ProfileDetailsVM.TVMediaGeneratror.IsMine = false;
                    ProfileDetailsVM.TaggedMediaGeneratror.IsMine = false;
                }
                else
                {
#pragma warning disable IDE0019 // Use pattern matching
                    var pi = MPivot.Items[MPivot.SelectedIndex] as PivotItem;
#pragma warning restore IDE0019 // Use pattern matching

                    if (pi != null)
                    {
                        if (pi.Tag is string str && !string.IsNullOrEmpty(str))
                        {
                            if (str == "Tag")
                            {
                                ProfileDetailsVM.TaggedMediaGeneratror.IsMine = true;
                                ProfileDetailsVM.TVMediaGeneratror.IsMine = false;
                                ProfileDetailsVM.MediaGeneratror.IsMine = false;
                                ProfileDetailsVM.ShopMediaGeneratror.IsMine = false;
                                if (ProfileDetailsVM.TaggedMediaGeneratror.Items.Count == 0)
                                    ProfileDetailsVM.GetTaggedPosts(true);
                            }
                            else if (str == "TV")
                            {
                                ProfileDetailsVM.TVMediaGeneratror.IsMine = true;
                                ProfileDetailsVM.TaggedMediaGeneratror.IsMine = false;
                                ProfileDetailsVM.MediaGeneratror.IsMine = false;
                                ProfileDetailsVM.ShopMediaGeneratror.IsMine = false;
                                if (ProfileDetailsVM.TVMediaGeneratror.Items.Count == 0)
                                    ProfileDetailsVM.GetTVPosts(true);
                            }
                            else if (str == "Shop")
                            {
                                ProfileDetailsVM.ShopMediaGeneratror.IsMine = true;
                                ProfileDetailsVM.TVMediaGeneratror.IsMine = false;
                                ProfileDetailsVM.TaggedMediaGeneratror.IsMine = false;
                                ProfileDetailsVM.MediaGeneratror.IsMine = false;
                                if (ProfileDetailsVM.ShopMediaGeneratror.Items.Count == 0)
                                    ProfileDetailsVM.GetShoppablePosts(true);
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
            if (MPivot.Items.Count > 1)
            {
                var count = MPivot.Items.Count;
                try
                {
                    for (int i = count - 1; i > 0; i--)
                        MPivot.Items.RemoveAt(i);
                }
                catch { }
            }
        }
        public bool IsTabCreated = false;
        public void CreateTabs(bool igtv = false, bool shop = false, bool tagged = false)
        {
            if (IsTabCreated) return;
            IsTabCreated = true;
            try
            {
                if (igtv)
                {
                    var gv = GenerateGV();
                    gv.DesiredWidth = 210;
                    gv.ItemTemplateSelector = null;
                    gv.ItemTemplate = Resources["TVTemplate"] as DataTemplate;
                    gv.Loaded += GvTVMediaGeneratrorLoaded;
                    gv.ItemsSource = ProfileDetailsVM.TVMediaGeneratror.Items;
                    gv.ItemClick += TVLVItemClick;
                    var pi = GeneratePivotItem("", "TV");
                    pi.Content = gv;
                    MPivot.Items.Add(pi);
                }
                if (shop)
                {
                    var gv = GenerateGV();
                    gv.ItemHeight = 130;
                    gv.Loaded += GvShopMediaGeneratrorLoaded;
                    gv.ItemsSource = ProfileDetailsVM.ShopMediaGeneratror.Items;
                    gv.ItemClick += ShopLVItemClick;
                    var pi = GeneratePivotItem("", "Shop");
                    pi.Content = gv;
                    MPivot.Items.Add(pi);
                }
                if (tagged)
                {
                    var gv = GenerateGV();
                    gv.ItemHeight = 130;
                    gv.ItemsSource = ProfileDetailsVM.TaggedMediaGeneratror.Items;
                    gv.Loaded += GvTaggedMediaGeneratrorLoaded;
                    gv.ItemClick += TaggedLVItemClick;
                    var pi = GeneratePivotItem("", "Tag");
                    pi.Content = gv;
                    MPivot.Items.Add(pi);
                }
            }
            catch (Exception ex)
            {
                ex.PrintException("CreateTabs");
            }
        }
        private async void TVLVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is InstaMedia media)
                {
                    var index = ProfileDetailsVM.TVMediaGeneratror.Items.IndexOf(media);
                    await Task.Delay(350);
                    Helpers.NavigationService.Navigate(typeof(TV.TVPlayer),
                        new object[] { ProfileDetailsVM.TVMediaGeneratror.Items.ToList(), index, ProfileDetailsVM.User.ToUserShort() });
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
                    var index = ProfileDetailsVM.TaggedMediaGeneratror.Items.IndexOf(media);

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
                    var index = ProfileDetailsVM.ShopMediaGeneratror.Items.IndexOf(media);

                    ScrollableUserShopPostUc.Visibility = Visibility.Visible;
                    await Task.Delay(350);

                    ScrollableUserShopPostUc.ScrollTo(index);
                }
            }
            catch { }
        }
        private void GvMediaGeneratrorLoaded(object sender, RoutedEventArgs e)
        {
            S1 = (sender as AdaptiveGridViewOriginal).FindScrollViewer();
            if (S1 != null)
                S1.ViewChanging += OnSCViewChanging;
            ProfileDetailsVM.MediaGeneratror.SetLV(S1);
        }
        private void GvTVMediaGeneratrorLoaded(object sender, RoutedEventArgs e)
        {
            var S2 = (sender as AdaptiveGridViewOriginal).FindScrollViewer();
            if (S2 != null)
            {
                //if (SCMain.VerticalScrollMode == ScrollMode.Disabled)
                //    S2.EnableScroll();
                S2.ViewChanging += OnSCViewChanging;
            }
            ProfileDetailsVM.TVMediaGeneratror.SetLV(S2);
        }

        private void GvShopMediaGeneratrorLoaded(object sender, RoutedEventArgs e)
        {
            S3 = (sender as AdaptiveGridViewOriginal).FindScrollViewer();
            if (S3 != null)
            {
                //if (SCMain.VerticalScrollMode == ScrollMode.Disabled)
                //    S3.EnableScroll();
                S3.ViewChanging += OnSCViewChanging;
            }
            ProfileDetailsVM.TVMediaGeneratror.SetLV(S3);
        }
        private void GvTaggedMediaGeneratrorLoaded(object sender, RoutedEventArgs e)
        {
            S4 = (sender as AdaptiveGridViewOriginal).FindScrollViewer();
            if (S4 != null)
            {
                //if (SCMain.VerticalScrollMode == ScrollMode.Disabled)
                //    S4.EnableScroll();
                S4.ViewChanging += OnSCViewChanging;
            }
            ProfileDetailsVM.TaggedMediaGeneratror.SetLV(S4);
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
            var fs = (double)App.Current.Resources["ExtraBigFontSize"];
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
        AdaptiveGridViewOriginal GenerateGV()
        {
            var type = Resources["MediaUserInfoTemplateSelector"].GetType();
            var tms = Resources["MediaUserInfoTemplateSelector"] as DataTemplateSelector;
            var cs = Resources["GridViewItemContainerStyle"] as Style;
            var gv = new AdaptiveGridViewOriginal
            {
                Margin = new Thickness(5),
                //MinItemHeight = 140,
                DesiredWidth = 130,
                SelectionMode = ListViewSelectionMode.None,
                IsItemClickEnabled = true,
                ItemTemplateSelector = tms,
                ItemContainerStyle = cs,
            };
            gv.ContainerContentChanging += LVContainerContentChanging;
            ScrollViewer.SetIsVerticalScrollChainingEnabled(gv, false);
            gv.ItemContainerTransitions = new TransitionCollection
            {
                new EntranceThemeTransition()
            };
            //<local:AdaptiveGridViewX x:Name="LV"
            //                                            Margin="5"
            //                                            MinItemHeight="140"
            //                                            MinItemWidth="140"
            //                                            SelectionMode="None"
            //                                            ItemClick="LVItemClick"
            //                                            IsItemClickEnabled="True"
            //                                            ItemTemplateSelector="{StaticResource MediaUserInfoTemplateSelector}"
            //                                            ItemsSource="{Binding MediaGeneratror.Items, Mode=OneWay}"
            //                                            ContainerContentChanging="LVContainerContentChanging"
            //                                            ItemContainerStyle="{StaticResource GridViewItemContainerStyle}">

            //                        <local:AdaptiveGridViewX.ItemContainerTransitions>
            //                            <TransitionCollection>
            //                                <EntranceThemeTransition />
            //                            </TransitionCollection>
            //                        </local:AdaptiveGridViewX.ItemContainerTransitions>
            //                    </local:AdaptiveGridViewX>
            return gv;
        }

        private async void LVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is InstaMedia media)
                {
                    var index = ProfileDetailsVM.MediaGeneratror.Items.IndexOf(media);
                    //Helpers.NavigationService.Navigate(typeof(Posts.ScrollableUserPostView),
                    //    new object[] { ProfileDetailsVM.User, ProfileDetailsVM.MediaGeneratror, index });

                    ScrollableUserPostUc.Visibility = Visibility.Visible;
                    await Task.Delay(350);
                    ScrollableUserPostUc.MediaGeneratror.IsLoading = false;
                    ScrollableUserPostUc.ScrollTo(index);
                }

            }
            catch { }
        }

        private void LVContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            //Microsoft.Toolkit.Uwp.UI.Controls.ScrollHeaderMode = Microsoft.Toolkit.Uwp.UI.Controls.ScrollHeaderMode.QuickReturn
            try
            {
                //Microsoft.Toolkit.Uwp.UI.Extensions.VisualExtensions.cen
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
            offsetAnimation.Duration = TimeSpan.FromSeconds(.25);

            //Define Animation Target for this animation to animate using definition. 
            offsetAnimation.Target = "Offset";


            ScalarKeyFrameAnimation fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1f, 0.9f);
            fadeAnimation.Duration = TimeSpan.FromSeconds(.25);
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
            //animationGroup.Add(fadeAnimation);

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
        private void GoUpButtonClick(object sender, RoutedEventArgs e)
        {
            SCMain.ScrollToElement(0);
        }

        private void HyperlinkButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ProfileDetailsVM.User.ExternalUrl.OpenUrl();
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
                        foreach (var item in ProfileDetailsVM.Highlights)
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
                if ((ProfileDetailsVM.Stories == null && ProfileDetailsVM.Broadcast == null) ||
                    (ProfileDetailsVM.Stories != null && ProfileDetailsVM.Stories.Count == 0 && ProfileDetailsVM.Broadcast == null))
                    Helpers.NavigationService.Navigate(typeof(ImageVideoView), ProfileDetailsVM.User);
                else
                {
                    UserImageFlyout.Items.Clear();
                    var openPicture = new MenuFlyoutItem
                    {
                        Text = "Open profile picture",
                        Height = 48
                    };
                    openPicture.Click += MenuOpenPictureClick;
                    UserImageFlyout.Items.Add(openPicture);

                    if (ProfileDetailsVM.Stories.Count > 0)
                    {
                        var openStory = new MenuFlyoutItem
                        {
                            Text = "Open stories",
                            Height = 48
                        };
                        openStory.Click += MenuOpenStoryClick;
                        UserImageFlyout.Items.Add(openStory);
                    }
                    if (ProfileDetailsVM.Broadcast != null)
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

                //Helpers.NavigationService.Navigate(typeof(Main.StoryView), new object[] { ProfileDetailsVM.Stories.ToList(), 0 });
            }
            catch { }
        }

        private void MenuOpenLiveClick(object sender, RoutedEventArgs e) => Helper.OpenLive(ProfileDetailsVM.Broadcast);

        private void MenuOpenPictureClick(object sender, RoutedEventArgs e) =>
            Helpers.NavigationService.Navigate(typeof(ImageVideoView), ProfileDetailsVM.User);

        private void MenuOpenStoryClick(object sender, RoutedEventArgs e) =>
            Helpers.NavigationService.Navigate(typeof(Main.StoryView), new object[] { ProfileDetailsVM.Stories.ToList(), 0 });

        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            //ProfileDetailsVM.Refresh();
            RefreshControl.RequestRefresh();
        }

        private /*async*/ void EditButtonClick(object sender, RoutedEventArgs e)
        {
            //"EditButtonClick ghable raftan".PrintDebug();
            //await new ContentDialogs.EditProfileDialog().ShowAsync();
            EditProfileUc.Show();
          
        }

        private void EditProfileUc_OnCompleted(object sender, EventArgs e)
        {
            "EditButtonClick raft khate badi".PrintDebug();
            ProfileDetailsVM.User = Helper.CurrentUser;
            try
            {
                UpdateUserImage(Helper.CurrentUser.ProfilePicture);
                ProfileDetailsVM.SetBio();
            }
            catch { }
        }

        public void SetBusinessProfile()
        {
            try
            {
                var user = Helper.CurrentUser;

                LVBusinessInfo.ItemsSource = null;
                if (user.IsBusiness)
                {
                    GridBusinessInfo.Visibility = Visibility.Visible;
                    var list = new ObservableCollection<BusinessProfile>();
                    if (!string.IsNullOrEmpty(user.PublicPhoneNumber))
                    {
                        list.Add(new BusinessProfile
                        {
                            BusinessContactMethod = user.BusinessContactMethod.ToString(),
                            Tag = "Phone"
                        });
                        //var btn = GetButton();
                        //btn.Content = user.BusinessContactMethod.ToString();
                        ////btn.Tag = $"+{user.PublicPhoneCountryCode}{user.PublicPhoneNumber}";
                        //btn.Tag = "Phone";
                        //btn.Click += BusinessButtonClick;
                        //LVBusinessInfo.Items.Add(btn);
                    }
                    if (!string.IsNullOrEmpty(user.PublicEmail))
                    {
                        list.Add(new BusinessProfile
                        {
                            BusinessContactMethod = "Email",
                            Tag = "Email"
                        });
                        var btn = GetButton();
                        //btn.Content = "Email";
                        ////btn.Tag = $"+{user.PublicPhoneCountryCode}{user.PublicPhoneNumber}";
                        //btn.Tag = "Email";
                        //btn.Click += BusinessButtonClick;
                        //LVBusinessInfo.Items.Add(btn);
                    }
                    LVBusinessInfo.ItemsSource = list;
                    if (list.Count == 0)
                        GridBusinessInfo.Visibility = Visibility.Collapsed;
                }
                else
                    GridBusinessInfo.Visibility = Visibility.Collapsed;
            }
            catch { }
        }
        private void LVBusinessInfoItemClick(object sender, ItemClickEventArgs e)
        {
            try
            { 
                if (e.ClickedItem is BusinessProfile profile)
                {
                    var user = Helper.CurrentUser;
                    if (profile.Tag == "Phone")
                    {
                        $"+{user.PublicPhoneCountryCode}{user.PublicPhoneNumber}".CopyText();
                        MainPage.Current.ShowInAppNotify($"Email: \"+{user.PublicPhoneCountryCode}{user.PublicPhoneNumber}\" copied ;)", 1500);
                        if (profile.BusinessContactMethod == "Call")
                            $"ms-call://?PhoneNumber=+{user.PublicPhoneCountryCode}{user.PublicPhoneNumber}".OpenUrl();
                        else
                            $"ms-chat://?PhoneNumber+{user.PublicPhoneCountryCode}{user.PublicPhoneNumber}".OpenUrl();
                    }
                    else if (profile.Tag == "Email")
                    {
                        user.PublicEmail.CopyText();
                        MainPage.Current.ShowInAppNotify($"Email: \"{user.PublicEmail}\" copied ;)", 1500);
                        $"mailto://{user.PublicEmail}".OpenUrl();
                    }
                }
            }
            catch { }
        }


        private void BusinessButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn)
                {
                    var user = Helper.CurrentUser;
                    if (btn.Tag.ToString() == "Phone")
                    {
                        $"+{user.PublicPhoneCountryCode}{user.PublicPhoneNumber}".CopyText();
                        MainPage.Current.ShowInAppNotify($"Email: \"+{user.PublicPhoneCountryCode}{user.PublicPhoneNumber}\" copied ;)", 1500);
                        if (btn.Content.ToString() == "Call")
                        {
                            $"ms-call://?PhoneNumber=+{user.PublicPhoneCountryCode}{user.PublicPhoneNumber}".OpenUrl();
                        }
                        else
                        {
                            $"ms-chat://?PhoneNumber+{user.PublicPhoneCountryCode}{user.PublicPhoneNumber}".OpenUrl();
                        }
                    }
                    else if(btn.Tag.ToString() == "Email")
                    {
                        user.PublicEmail.CopyText();
                        MainPage.Current.ShowInAppNotify($"Email: \"{user.PublicEmail}\" copied ;)", 1500);
                        $"mailto://{user.PublicEmail}".OpenUrl();
                    }
                }
            }
            catch { }
        }

        private void CopyBiographyMenuFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ProfileDetailsVM.User.Biography.CopyText();
                Helper.ShowNotify($"{ProfileDetailsVM.User.UserName}'s biography copied ;)");
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
                ProfileDetailsVM.User.UserName.CopyText();
                Helper.ShowNotify($"{ProfileDetailsVM.User.UserName} copied ;)");
            }
            catch { }
        }
        private void CopyUsernameAddressMenuFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var urlAddress = Helper.InstagramUrl + ProfileDetailsVM.User.UserName;
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
                ProfileDetailsVM.User.ExternalUrl.CopyText();
                Helper.ShowNotify("Url copied ;)");
            }
            catch { }
        }

        private void FollowersButtonClick(object sender, RoutedEventArgs e)
        {
            Helpers.NavigationService.Navigate(typeof(FollowView), new object[] { ProfileDetailsVM.User.ToUserShort(), 0 });
        }

        private void FollowingButtonClick(object sender, RoutedEventArgs e)
        {
            Helpers.NavigationService.Navigate(typeof(FollowView), new object[] { ProfileDetailsVM.User.ToUserShort(), 1 });
        }
        private void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            Helpers.NavigationService.Navigate(typeof(Settings.SettingsView));
        }
        private async void AccountToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AccountToggleButton.IsChecked = false;
            }
            catch { }
            if (Helper.Passcode.IsEnabled)
            {
                var psd = new ContentDialogs.PasscodeDialog(true)
                {
                    CallMeAnAction = async () => { await new ContentDialogs.AddOrChooseUserDialog().ShowAsync(); }
                };
                await psd.ShowAsync();
                return;
            }
            try
            {
                await new ContentDialogs.AddOrChooseUserDialog().ShowAsync();
            }
            catch { }
        }
        Button GetButton()
        {
            //< Button x: Name = "EmailButton"
            //        Background = "Transparent"
            //        Content = "Email"
            //        Width = "140"
            //        Margin = "5"
            //        HorizontalAlignment = "Stretch"
            //        Click = "EmailButton_Click" />
            return new Button
            {
                Background = new SolidColorBrush(Colors.Transparent),
                Width = 120,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
        }
        private void IGTVVListViewItemTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (MPivot.Items.Count > 1)
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
        private void MPivotLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MPivot != null)
                    MPivot.Height = ActualHeight ;

            }
            catch { }
        }

        #region Scroll handling
        private ScrollViewer S1, /*S2, */S3, S4;

        //private bool tryingEnableSCs = false;
        //bool isMainScrollEnabled = true;

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
                //return;
                //if (scrollViewer.VerticalOffset >= GridMainScrollViewer.ActualHeight && !tryingEnableSCs)
                //{
                //    tryingEnableSCs = true;
                //    scrollViewer.DisableScroll();
                //    S1?.EnableScroll();
                //    S2?.EnableScroll();
                //    S3?.EnableScroll();
                //    S4?.EnableScroll();
                //    isMainScrollEnabled = false;
                //    ("DISABELING SC MAIN").PrintDebug();
                //    GridMainScrollViewer.Height = 0;
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

            HandleGoUpRefreshButtons(scrollViewer);
            //return;
            //if ((scrollViewer.VerticalOffset - _lastMainVerticalOffset) > 5 && !_isMainHideTitleGrid)
            //{
            //    _isMainHideTitleGrid = true;
            //}
            //else if (scrollViewer.VerticalOffset < _lastMainVerticalOffset && _isMainHideTitleGrid)
            //{
            //    _isMainHideTitleGrid = false;
            //    "2".PrintDebug();
            //    if (scrollViewer.VerticalOffset < 5.0) return;

            //    //SCMain.EnableScroll();
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
            //        ("DISABELING SC1 SC2").PrintDebug();
            //        lastSC1Offset = S1.VerticalOffset;
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
            //        await Task.Delay(10);
            //        SCMain.EnableScroll();
            //        SCMain.ChangeView(null, GridMainScrollViewer.ActualHeight - 10, null);
            //        await Task.Delay(750);
            //        _triedFirst = false;

            //    }
            //}
            //_lastMainVerticalOffset = scrollViewer.VerticalOffset;
        }

        #endregion Scroll handling

        #region LOADINGS
        public void ShowTopLoading() => TopLoading.Start();

        public void HideTopLoading() => TopLoading.Stop();


        public void ShowBottomLoading() => BottomLoading.Start();


        public void HideBottomLoading() => BottomLoading.Stop();
        #endregion LOADINGS














    }
}
