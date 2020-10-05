using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using Minista.ContentDialogs;
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


namespace Minista.UserControls.Story
{
    public sealed partial class StoryInsightUc : UserControl, INotifyPropertyChanged
    {
        public InstaInsightSurfaceType SurfaceType { get; set; } = InstaInsightSurfaceType.Story;
        public InstaMediaInsightsX VM { get; set; } = new InstaMediaInsightsX();
        public ObservableCollection<MetricInsightsItem> Interactions { get; set; } = new ObservableCollection<MetricInsightsItem>();
        public ObservableCollection<MetricInsightsItem> Discoveries { get; set; } = new ObservableCollection<MetricInsightsItem>();
        public ObservableCollection<MetricInsightsItem> Impressions { get; set; } = new ObservableCollection<MetricInsightsItem>();
        public InstaStoryItem Media { get; private set; }
        int _reachCount = 0, _profileActionsCount = 0, _storyRepliesCount = 0;
        public int StoryRepliesCount { get => _storyRepliesCount; set { _storyRepliesCount = value; RaisePropertyChanged("StoryRepliesCount"); } }
        public int ReachCount { get => _reachCount; set { _reachCount = value; RaisePropertyChanged("ReachCount"); } }
        public int ProfileActionsCount { get => _profileActionsCount; set { _profileActionsCount = value; RaisePropertyChanged("ProfileActionsCount"); } }
        public string UserName => Helper.CurrentUser.UserName.ToLower();
        public StoryInsightUc()
        {
            this.InitializeComponent();
        }


        public async void SetItem(InstaStoryItem media)
        {
            try
            {
                Media = media;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    ShowLoading();
                    var result = await Helper.InstaApi.BusinessProcessor.GetMediaInsightsAsync(Media.Pk.ToString() , SurfaceType);
                    if (result.Succeeded)
                    {
                        DataContext = VM = result.Value;
                        StoryRepliesCount = result.Value.StoryRepliesCount;
                        ProfileActionsCount = result.Value.Metrics.ProfileActionsCount;
                        ReachCount = result.Value.Metrics.ReachCount;
                        Interactions.Clear();
                        Discoveries.Clear();
                        Impressions.Clear();
                        if (result.Value.Metrics != null)
                        {
                            if (result.Value.Metrics.ShareCount?.Shares?.Nodes?.Count > 0)
                            {
                                foreach (var item in result.Value.Metrics.ShareCount.Shares.Nodes)
                                {
                                    if (item.Name == "TO_OTHERS")
                                        Interactions.Add(new MetricInsightsItem("Shares", item.Value));
                                    // These types also exists 
                                    // TO_CREATOR
                                }
                            }
                            Interactions.Add(new MetricInsightsItem("Replies", StoryRepliesCount));
                            Interactions.Add(new MetricInsightsItem("Profile Visits", result.Value.Metrics.OwnerProfileViewsCount));


                            Discoveries.Add(new MetricInsightsItem("Impressions", result.Value.Metrics.ImpressionCount));
                            Discoveries.Add(new MetricInsightsItem("Follows", result.Value.Metrics.OwnerAccountFollowsCount));
                            //  StoryLinkNavigationCount  ?!
                            var navigations = result.Value.TapsBackCount + result.Value.TapsForwardCount + 
                            result.Value.StoryExitsCount + result.Value.StorySwipeAwayCount;

                            if (navigations > 0)
                            {
                                Discoveries.Add(new MetricInsightsItem("Navigation", navigations));
                                var impression = result.Value.Metrics.ImpressionCount;
                                var list = new List<string>();
                                if(result.Value.TapsBackCount > 0)
                                    Impressions.Add(new MetricInsightsItem("Back", result.Value.TapsBackCount));
                                if (result.Value.TapsForwardCount > 0)
                                    Impressions.Add(new MetricInsightsItem("Forward", result.Value.TapsForwardCount));
                                if (result.Value.TapsForwardCount > 0)
                                    Impressions.Add(new MetricInsightsItem("Next Story", result.Value.StorySwipeAwayCount));
                                if (result.Value.TapsForwardCount > 0)
                                    Impressions.Add(new MetricInsightsItem("Exited", result.Value.StoryExitsCount));
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

        private void HashtagsListViewItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is MetricInsightsItem item && item != null)
                {
                    if (item.Name != "All Hashtags")
                    {

                        Helpers.NavigationService.Navigate(typeof(Views.Infos.HashtagView), item.Name);
                    }
                }
            }
            catch { }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(string memberName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
    }
}
