using InstagramApiSharp.Classes.Models;
using Minista.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Navigation;

namespace Minista.Views.Main
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LikersView : Page
    {
        readonly Compositor _compositor;
        readonly ImplicitAnimationCollection _elementImplicitAnimation;

        private readonly Visual _goUpButtonVisual;
        ScrollViewer ScrollView;
        private InstaMedia Media;
        private bool CanLoadFirstPopUp = false;
        NavigationMode NavigationMode;
        public static LikersView Current;
        public LikersView()
        {
            this.InitializeComponent();
            Current = this;
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Create ImplicitAnimations Collection. 
            _elementImplicitAnimation = _compositor.CreateImplicitAnimationCollection();

            //Define trigger and animation that should play when the trigger is triggered. 
            _elementImplicitAnimation["Offset"] = CreateOffsetAnimation();

            _goUpButtonVisual = GoUpButton.GetVisual();
            Loaded += LikersViewLoaded;
        }

        private void LikersViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshControl.RefreshRequested -= RefreshControlRefreshRequested;
                RefreshControl.Visualizer.RefreshStateChanged -= RefreshControlRefreshStateChanged;
            }
            catch { }
            RefreshControl.RefreshRequested += RefreshControlRefreshRequested;
            if (RefreshControl.Visualizer != null)
                RefreshControl.Visualizer.RefreshStateChanged += RefreshControlRefreshStateChanged;
            if (NavigationMode == NavigationMode.Back && LikersVM.Media != null)
            {
                if (LikersVM.Media.InstaIdentifier == Media.InstaIdentifier)
                    return;
            }
            else if (NavigationMode == NavigationMode.New)
            {
                NavigationCacheMode = NavigationCacheMode.Enabled;
                CanLoadFirstPopUp = false;
            }
            if (!CanLoadFirstPopUp)
            {
                if (ScrollView == null)
                {
                    //if (NavigationMode == NavigationMode.Back) return;
                    ScrollView = ItemsLV.FindScrollViewer();
                    ScrollView.ViewChanging += ScrollViewViewChanging;
                }
                LikersVM.ResetCache();
                LikersVM.SetMedia(Media);
                LikersVM.RunLoadMore();
                ToggleGoUpButtonAnimation(false);
                CanLoadFirstPopUp = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationMode = e.NavigationMode;

            KeyDown += OnKeyDownHandler;
            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();
            try
            {
                if (e.Parameter is InstaMedia media && media != null)
                {
                    Media = media;
                }
            }
            catch { }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            KeyDown -= OnKeyDownHandler;
        }

        private void RefreshControlRefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            using (var RefreshCompletionDeferral = args.GetDeferral())
                LikersVM.RunLoadMore();
        }
        private void RefreshControlRefreshStateChanged(Microsoft.UI.Xaml.Controls.RefreshVisualizer sender, Microsoft.UI.Xaml.Controls.RefreshStateChangedEventArgs args)
        {
            if (args.NewState == Microsoft.UI.Xaml.Controls.RefreshVisualizerState.Refreshing)
                RefreshButton.IsEnabled = false;
            else
                RefreshButton.IsEnabled = true;
        }
        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            RefreshControl.RequestRefresh();
        }
        private void OnKeyDownHandler(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.F5)
                    LikersVM.RunLoadMore();
            }
            catch { }
        }
        private async void FollowClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn != null)
                {
                    if (btn.DataContext is InstaUserShortFriendship data && data != null)
                    {
                        $"{data.UserName}  {data.Pk}".PrintDebug();
                        var result = await Helper.InstaApi.UserProcessor.FollowUserAsync(data.Pk);
                        if (result.Succeeded)
                        {
                            var x = result.Value;
                            data.FriendshipStatus = new InstaFriendshipShortStatus
                            {
                                Following = x.Following,
                                IncomingRequest = x.IncomingRequest,
                                IsPrivate = x.IsPrivate,
                                OutgoingRequest = x.OutgoingRequest
                            };
                            btn.DataContext = data;
                        }
                        else
                        {
                            switch (result.Info.ResponseType)
                            {
                                case InstagramApiSharp.Classes.ResponseType.RequestsLimit:
                                case InstagramApiSharp.Classes.ResponseType.SentryBlock:
                                    result.Info.Message.ShowMsg("ERR");
                                    break;
                                case InstagramApiSharp.Classes.ResponseType.ActionBlocked:
                                    "Action blocked.\r\nPlease try again 5 or 10 minutes later".ShowMsg("ERR");
                                    break;
                            }
                        }
                    }
                }
            }
            catch { }
        }



        private void ItemsLVRefreshRequested(object sender, EventArgs e)
        {
            LikersVM?.RunLoadMore(true);
        }

        private void UserButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn != null)
                {
                    if (btn.DataContext is InstaUserShortFriendship data && data != null)
                        Helper.OpenProfile(new List<InstaUserShortFriendship> { data });
                }
            }
            catch { }

        }


        private void LVContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
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
        #region Animation

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




            //Define Rotation Animation for Animation Group. 
            ScalarKeyFrameAnimation rotationAnimation = _compositor.CreateScalarKeyFrameAnimation();
            rotationAnimation.InsertKeyFrame(.5f, 0.160f);
            rotationAnimation.InsertKeyFrame(1f, 0f);
            rotationAnimation.Duration = TimeSpan.FromSeconds(.4);

            //Define Animation Target for this animation to animate using definition. 
            rotationAnimation.Target = "RotationAngle";

            //Add Animations to Animation group. 
            CompositionAnimationGroup animationGroup = _compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);
            animationGroup.Add(rotationAnimation);
            animationGroup.Add(fadeAnimation);

            return animationGroup;
        }
        #endregion



















        private double _lastVerticalOffset;
        private bool _isHideTitleGrid;





        private void ScrollViewViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                var scrollViewer = sender as ScrollViewer;
                if ((scrollViewer.VerticalOffset - _lastVerticalOffset) > 5 && !_isHideTitleGrid)
                {
                    _isHideTitleGrid = true;
                    ToggleGoUpButtonAnimation(false);
                }
                else if (scrollViewer.VerticalOffset < _lastVerticalOffset && _isHideTitleGrid)
                {
                    _isHideTitleGrid = false;
                    ToggleGoUpButtonAnimation(true);
                }
                if (scrollViewer.VerticalOffset <= 1)
                {
                    _isHideTitleGrid = true;
                    ToggleGoUpButtonAnimation(false);
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
        private void GoUpButtonClick(object sender, RoutedEventArgs e)
        {
            ScrollView.ScrollToElement(0);
        }

        #region LOADINGS
        public void ShowTopLoading() => TopLoading.Start();
        public void HideTopLoading() => TopLoading.Stop();
        #endregion LOADINGS
    }
}
