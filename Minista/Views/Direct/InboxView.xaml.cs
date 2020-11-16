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
using Minista.ViewModels.Direct;
using Minista.UserControls.Direct;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using InstagramApiSharp.API.RealTime;

namespace Minista.Views.Direct
{
    public sealed partial class InboxView : Page
    {
        private readonly DispatcherTimer Timer = new DispatcherTimer();
        public InboxViewModel InboxVM { get; set; } = InboxViewModel.Instance;
        public static InboxView Current;
        private readonly Visual _refreshButtonVisual;

        readonly Compositor _compositor;
        public InboxView()
        {
            this.InitializeComponent();
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _refreshButtonVisual = RefreshButton.GetVisual();
            Current = this;
            DataContext = InboxVM;
            Loaded += InboxViewLoaded;
            Timer.Interval = TimeSpan.FromSeconds(30);
            Timer.Tick += TimerTick;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            KeyDown -= OnKeyDownHandler;
            try
            {
                Timer.Stop();
            }
            catch { }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            KeyDown += OnKeyDownHandler;
            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();

            try
            {
                Timer.Start();
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
        private void InboxViewLoaded(object sender, RoutedEventArgs e)
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
        }
        private void RefreshControlRefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            using (var RefreshCompletionDeferral = args.GetDeferral())
                InboxVM?.Refresh();
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
        private void ItemsLVLoaded(object sender, RoutedEventArgs e)
        {
            var scroll = ItemsLV.FindScrollViewer();
            if (scroll != null)
                scroll.ViewChanging += ScrollViewViewChanging;
            InboxVM?.SetLV(scroll);
        }
        private void ItemsLVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem != null && e.ClickedItem is DirectInboxUc directInboxUc && directInboxUc != null)
                {
                    Helpers.NavigationService.Navigate(typeof(ThreadView), directInboxUc.Thread);
                }
            }
            catch { }
        }


        private void TimerTick(object sender, object e)
        {
            try
            {
                InboxVM.GetUserPresense();
            }
            catch { }
        }

        private void RequestsButtonClick(object sender, RoutedEventArgs e)
        {
            Helpers.NavigationService.Navigate(typeof(DirectRequestsView));
        }
        private void SearchTextTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (SearchText.Text.Length <= 0)
                {
                    InboxVM.SearchVisibility = Visibility.Collapsed;
                    InboxVM.ItemsSearch.Clear();
                }
                else
                    DoSearch();
            }
            catch { }
        }
        private void SearchTextKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                    DoSearch();
            }
            catch { }
        }
        void DoSearch() => InboxVM.Search(SearchText.Text);

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
