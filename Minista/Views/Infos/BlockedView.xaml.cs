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
using Minista.ViewModels.Infos;
using Windows.UI.Core;
using InstagramApiSharp.Classes.Models;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using System.Numerics;

namespace Minista.Views.Infos
{
    public sealed partial class BlockedView : Page
    {
        public static BlockedView Current;
        private bool IsLoadedBefore = false;
        private readonly Visual _refreshButtonVisual;

        readonly Compositor _compositor;
        public BlockedView()
        {
            this.InitializeComponent();
            Current = this;
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _refreshButtonVisual = RefreshButton.GetVisual();
            Loaded += BlockedViewLoaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            KeyDown += OnKeyDownHandler;
            if (e.NavigationMode == NavigationMode.New)
            {
                GetType().RemovePageFromBackStack();
                IsLoadedBefore = false;
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            KeyDown -= OnKeyDownHandler;
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
        private void BlockedViewLoaded(object sender, RoutedEventArgs e)
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
            if (!IsLoadedBefore)
            {
                IsLoadedBefore = true;
                var sv = ItemsLV.FindScrollViewer();
                BlockedVM.SetLV(sv);
                BlockedVM.RunLoadMore(true);
                try
                {
                    sv.ViewChanging -= ScrollViewViewChanging;
                }
                catch { }
                sv.ViewChanging += ScrollViewViewChanging;
            }
        }

        private void RefreshControlRefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            using (var RefreshCompletionDeferral = args.GetDeferral())
                BlockedVM.RunLoadMore(true);
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
        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            RefreshControl.RequestRefresh();
        }

        private void UserTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid grid && grid.DataContext is InstaBlockedUserInfo user && user != null)
                    Helper.OpenProfile(user.ToUserShort());
            }
            catch { }
        }



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
                    ToggleRefreshButtonAnimation(false);
                }
                else if (scrollViewer.VerticalOffset < _lastVerticalOffset && _isHideTitleGrid)
                {
                    _isHideTitleGrid = false;
                    ToggleRefreshButtonAnimation(true);
                }
                if (scrollViewer.VerticalOffset == 0)
                {
                    _isHideTitleGrid = true;
                    ToggleRefreshButtonAnimation(false);
                }
                _lastVerticalOffset = scrollViewer.VerticalOffset;
            }
            catch { }
        }

        private void ToggleRefreshButtonAnimation(bool show)
        {
            try
            {
                var scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
                scaleAnimation.InsertKeyFrame(1f, show ? 1f : 0);
                scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);

                _refreshButtonVisual.CenterPoint = new Vector3((float)RefreshButton.ActualWidth / 2f, (float)RefreshButton.ActualHeight / 2f, 0f);
                _refreshButtonVisual.StartAnimation("Scale.X", scaleAnimation);
                _refreshButtonVisual.StartAnimation("Scale.Y", scaleAnimation);
            }
            catch { }
        }



        #region LOADINGS
        public void ShowTopLoading() => TopLoading.Start();
        public void HideTopLoading() => TopLoading.Stop();



        public void ShowBottomLoading() => BottomLoading.Start();
        public void HideBottomLoading() => BottomLoading.Stop();
        #endregion LOADINGS

    }
}
