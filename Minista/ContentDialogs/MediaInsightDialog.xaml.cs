using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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


namespace Minista.ContentDialogs
{
    public sealed partial class MediaInsightDialog : ContentPopup, INotifyPropertyChanged
    {
        public InstaInsightSurfaceType SurfaceType { get; set; } = InstaInsightSurfaceType.Post;
        public InstaMediaInsightsX VM { get; set; } = new InstaMediaInsightsX();
        public ObservableCollection<MetricInsightsItem> Interactions { get; set; } = new ObservableCollection<MetricInsightsItem>();
        public ObservableCollection<MetricInsightsItem> Discoveries { get; set; } = new ObservableCollection<MetricInsightsItem>();
        public ObservableCollection<MetricInsightsItem> Impressions { get; set; } = new ObservableCollection<MetricInsightsItem>();
        public InstaMedia Media { get; private set; }
        public string MediaId { get; private set; }
        int _ownerProfileViewsCount = 0, _reachCount = 0, _profileActionsCount = 0, _impressionCount = 0, _followsCount = 0;
        public int OwnerProfileViewsCount { get => _ownerProfileViewsCount; set { _ownerProfileViewsCount = value; RaisePropertyChanged("OwnerProfileViewsCount"); } }
        public int ReachCount { get => _reachCount; set { _reachCount = value; RaisePropertyChanged("ReachCount"); } }
        public int ProfileActionsCount { get => _profileActionsCount; set { _profileActionsCount = value; RaisePropertyChanged("ProfileActionsCount"); } }
        public int ImpressionCount { get => _impressionCount; set { _impressionCount = value; RaisePropertyChanged("ImpressionCount"); } }
        public int FollowsCount { get => _followsCount; set { _followsCount = value; RaisePropertyChanged("FollowsCount"); } }
        public string UserName => Helper.CurrentUser.UserName.ToLower();
        public MediaInsightDialog(InstaMedia media) : this() => Media = media;

        public MediaInsightDialog(string mediaId, InstaInsightSurfaceType surfaceType) : this()
        {
            MediaId = mediaId;
            SurfaceType = surfaceType;
        }
        public MediaInsightDialog()
        {
            this.InitializeComponent();
            DataContext = VM;
            Loaded += MediaInsightDialogLoaded;
        }
        private async void MediaInsightDialogLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    ShowLoading();
                    var result = await Helper.InstaApi.BusinessProcessor.GetMediaInsightsAsync(Media != null ? Media.Pk.ToString() : MediaId, SurfaceType);
                    if (result.Succeeded)
                    {
                        DataContext = VM = result.Value;
                        OwnerProfileViewsCount = result.Value.Metrics.OwnerProfileViewsCount;
                        ProfileActionsCount = result.Value.Metrics.ProfileActionsCount;
                        ReachCount = result.Value.Metrics.ReachCount;
                        ImpressionCount = result.Value.Metrics.ImpressionCount;
                        if (result.Value.Metrics?.ProfileActions != null)
                        {
                            Interactions.Clear();
                            Interactions.Add(new MetricInsightsItem("Profile Visits", result.Value.Metrics.OwnerProfileViewsCount));
                            if (result.Value.Metrics?.ProfileActions.Data?.Nodes?.Count > 0)
                            {
                                foreach (var item in result.Value.Metrics.ProfileActions.Data.Nodes)
                                {
                                    if (item.Name == "BIO_LINK_CLICKED")
                                        Interactions.Add(new MetricInsightsItem("Website Clicks", item.Value));
                                    // These types also exists 
                                    // CALL
                                    // DIRECTION
                                    // EMAIL
                                    // TEXT
                                }
                            }
                            Discoveries.Clear();
                            if (result.Value.Metrics?.ReachFollowStatus.Data?.Nodes?.Count > 0)
                            {
                                foreach (var item in result.Value.Metrics.ReachFollowStatus.Data.Nodes)
                                    if (item.Name == "NON_FOLLOWER")
                                    {
                                        FollowsCount = item.Value;
                                        Discoveries.Add(new MetricInsightsItem("Follows", item.Value));
                                    }
                            }
                            Discoveries.Add(new MetricInsightsItem("Reach", result.Value.Metrics.ReachCount));
                            Discoveries.Add(new MetricInsightsItem("Impressions", result.Value.Metrics.ImpressionCount));
                            if (result.Value.Metrics?.ImpressionsSurfaces.Data?.Nodes?.Count > 0)
                            {
                                var impression = result.Value.Metrics.ImpressionCount;
                                foreach (var item in result.Value.Metrics.ImpressionsSurfaces.Data.Nodes)
                                    if (item.Name == "FEED")
                                    {
                                        impression -= item.Value;
                                        Impressions.Add(new MetricInsightsItem("From Home", item.Value));
                                    }
                                if(impression < result.Value.Metrics.ImpressionCount)
                                    Impressions.Add(new MetricInsightsItem("From Other", impression));
                            }
                        }
                    }
                    else
                        Helper.ShowErr(result.Info.Message, result.Info.Exception);
                    HideLoading();
                });
            }
            catch { HideLoading(); }
        }

        void ShowLoading()
        {
            try
            {
                LoadingGrid.Visibility = Visibility.Visible;
                LoadingUc.Start();
            }
            catch { }
        }
        void HideLoading()
        {
            try
            {
                LoadingGrid.Visibility = Visibility.Collapsed;
                LoadingUc.Stop();
            }
            catch { }
        }
        private void ExitButtonClick(object sender, RoutedEventArgs e) => Hide();


        public class MetricInsightsItem : BaseModel
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public MetricInsightsItem(string name, int value)
            {
                Name = name;
                Value = value;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(string memberName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
    }
}
