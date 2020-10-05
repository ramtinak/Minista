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
        public ObservableCollection<MetricInsightsItem> Hashtags { get; set; } = new ObservableCollection<MetricInsightsItem>();
        public InstaMedia Media { get; private set; }
        public string MediaId { get; private set; }
        private string _impressionsBottomText = null;
        int _ownerProfileViewsCount = 0, _reachCount = 0, _profileActionsCount = 0, _impressionCount = 0, _nonFollowsCount = 0, _ownerAccountFollowsCount = 0;
        public int OwnerProfileViewsCount { get => _ownerProfileViewsCount; set { _ownerProfileViewsCount = value; RaisePropertyChanged("OwnerProfileViewsCount"); } }
        public int ReachCount { get => _reachCount; set { _reachCount = value; RaisePropertyChanged("ReachCount"); } }
        public int ProfileActionsCount { get => _profileActionsCount; set { _profileActionsCount = value; RaisePropertyChanged("ProfileActionsCount"); } }
        public int ImpressionCount { get => _impressionCount; set { _impressionCount = value; RaisePropertyChanged("ImpressionCount"); } }
        public int OwnerAccountFollowsCount { get => _ownerAccountFollowsCount; set { _ownerAccountFollowsCount = value; RaisePropertyChanged("OwnerAccountFollowsCount"); } }
        public string ImpressionsBottomText { get => _impressionsBottomText; set { _impressionsBottomText = value; RaisePropertyChanged("ImpressionsBottomText"); } }
        public int NonFollowsCount { get; set; }


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
                        ImpressionsBottomText = null;
                        Interactions.Clear();
                        Discoveries.Clear(); 
                        Impressions.Clear();
                        Hashtags.Clear();
                        if (result.Value.Metrics != null)
                        {
                            Interactions.Add(new MetricInsightsItem("Profile Visits", result.Value.Metrics.OwnerProfileViewsCount));
                            if (result.Value.Metrics.ProfileActions?.Data?.Nodes?.Count > 0)
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
                            if (result.Value.Metrics.ReachFollowStatus?.Data?.Nodes?.Count > 0)
                            {
                                foreach (var item in result.Value.Metrics.ReachFollowStatus.Data.Nodes)
                                    if (item.Name == "NON_FOLLOWER")
                                    {
                                        txtNonFollowsCount.Text = (NonFollowsCount = (item.Value * 100) / ReachCount).ToString();
                                    }
                            }

                            Discoveries.Add(new MetricInsightsItem("Follows", result.Value.Metrics.OwnerAccountFollowsCount));
                            Discoveries.Add(new MetricInsightsItem("Reach", result.Value.Metrics.ReachCount));
                            Discoveries.Add(new MetricInsightsItem("Impressions", result.Value.Metrics.ImpressionCount));
                            if (result.Value.Metrics.ImpressionsSurfaces?.Data?.Nodes?.Count > 0)
                            {
                                var impression = result.Value.Metrics.ImpressionCount;
                                var list = new List<string>();
                                foreach (var item in result.Value.Metrics.ImpressionsSurfaces.Data.Nodes)
                                {
                                    if (item.Name == "FEED")
                                    {
                                        impression -= item.Value;
                                        list.Add("Home");
                                        Impressions.Add(new MetricInsightsItem("From Home", item.Value));
                                    }
                                    else if (item.Name == "PROFILE")
                                    {
                                        impression -= item.Value;
                                        list.Add("Profile");
                                        Impressions.Add(new MetricInsightsItem("From Profile", item.Value));
                                    }
                                    else if (item.Name == "HASHTAG")
                                    {
                                        impression -= item.Value;
                                        list.Add("Hashtags");
                                        Impressions.Add(new MetricInsightsItem("From Hashtags", item.Value));
                                    }
                                }
                                if (impression < result.Value.Metrics.ImpressionCount)
                                {
                                    list.Add("Other");
                                    Impressions.Add(new MetricInsightsItem("From Other", impression));
                                }
                                ImpressionsBottomText = string.Join(",", list);
                            }
                            if (result.Value.Metrics.HashtagsImpressions?.Hashtags?.Nodes?.Count > 0)
                            {
                                // HASHTAGS
                                try
                                {
                                    Hashtags.Add(new MetricInsightsItem("All Hashtags", result.Value.Metrics.HashtagsImpressions.Hashtags.Value));
                                    foreach (var item in result.Value.Metrics.HashtagsImpressions.Hashtags.Nodes)
                                    {
                                        var val = 0;
                                        if (item.Organic != null)
                                            val = item.Organic.Value;
                                        Hashtags.Add(new MetricInsightsItem($"#{item.Name}", val));
                                    }
                                }
                                catch { }
                            }

                        }
                        if(LVHashtags != null)
                        LVHashtags.Visibility = Hashtags.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
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

        private void HashtagsListViewItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is MetricInsightsItem item && item != null)
                {
                    if (item.Name != "All Hashtags")
                    {
                        
                        Helpers.NavigationService.Navigate(typeof(Views.Infos.HashtagView), item.Name);
                        Hide();
                    }
                }
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
