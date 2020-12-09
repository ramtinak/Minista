using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static Helper;
namespace Minista.Views.Stories
{
    public sealed partial class UserStoryUc : UserControl, INotifyPropertyChanged
    {
        const int MaxIntervalForImage = 6;
        ProgressBar CurrentProgress;
        const double ProgressChangeValue = 0.1;
        double MaximumLength = MaxIntervalForImage;
        public ObservableCollection<StoryItemUc> Items { get; set; } = new ObservableCollection<StoryItemUc>();
        public DispatcherTimer Timer = new DispatcherTimer(), ProgressTimer = new DispatcherTimer();
        readonly List<ProgressBar> ProgressBarList = new List<ProgressBar>();
        readonly GestureRecognizer GestureRecognizer = new GestureRecognizer();
        int CurrentFlipViewIndex = -1;
        public int StoryIndex { get; set; } = 0;
        bool IsHighlight { get; set; } = false;
        public string Title { get; set; }
        public InstaReelFeed StoryFeed
        {
            get => (InstaReelFeed)GetValue(StoryFeedProperty);
            set
            {
                SetValue(StoryFeedProperty, value);
                DataContext = value;
                OnPropertyChanged("StoryFeed");
                if (value != null) SetStoryItems();
            }
        }
        public static readonly DependencyProperty StoryFeedProperty =
            DependencyProperty.Register("StoryFeed",
                typeof(InstaReelFeed),
                typeof(UserStoryUc),
                new PropertyMetadata(null));
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string memberName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        public UserStoryUc()
        {
            InitializeComponent();
            Timer.Interval = TimeSpan.FromSeconds(1);
            Timer.Tick += TimerTick;
            ProgressTimer.Interval = TimeSpan.FromMilliseconds(80);
            ProgressTimer.Tick += ProgressTimerTick;
        }

        async void SetStoryItems()
        {
            if (StoryFeed.Items.Count == 0)
            {
                var stories = await InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(StoryFeed.User.Pk.ToString());
                if (stories.Succeeded)
                {
                    if (stories.Value.Items?.Count > 0 && stories.Value.Items[0] != null)
                    {
                        var item = stories.Value.Items[0];
                        StoryFeed.Items.Clear();
                        StoryFeed.Items.AddRange(item.Items);
                        SetStoryItems(item.Items);
                    }
                }
            }
            else
                SetStoryItems(StoryFeed.Items);
        }
        void SetStoryItems(List<InstaStoryItem> items)
        {
            Items.Clear();
            ProgressGrid.Children.Clear();
            ProgressBarList.Clear();
            ProgressGrid.ColumnDefinitions.Clear();
            if (items?.Count > 0)
            {
                int ix = 0;
                var margin = new Thickness(2.5, 5, 2.5, 5);
                if (items.Count > 14 && items.Count < -20)
                    margin = new Thickness(2.0, 5, 2.0, 5);
                else if (StoryFeed.Items.Count > 20)
                    margin = new Thickness(.4, 5, .4, 5);
                items.ForEach(x =>
                {
                    ProgressGrid.ColumnDefinitions.Add(GenerateColumn());
                    Items.Add(new StoryItemUc(this) { StoryItem = x , Index = 0});
                    ProgressBar p = GenerateProgress(margin);
                    if (x.MediaType == InstaMediaType.Video)
                        p.Maximum = x.VideoDuration;
                    else
                        p.Maximum = MaxIntervalForImage;

                    Grid.SetColumn(p, ix);
                    ProgressBarList.Add(p);
                    ProgressGrid.Children.Add(p);
                    ix++;
                });
            }
            FlipView.ItemsSource = Items;
        }

        private void FlipViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var index = FlipView.SelectedIndex;
                if (index != -1)
                {
                    index.PrintDebug();
                    if (CurrentFlipViewIndex != -1 && CurrentFlipViewIndex != index)
                        Items[CurrentFlipViewIndex].PauseVideo();
                    Items[index].PlayVideo(index);
                }
                CurrentFlipViewIndex = index;
            }
            catch (Exception ex)
            {
                ex.PrintException("FlipViewSelectionChanged");
            }
        }
        private void UserButtonClick(object sender, RoutedEventArgs e)
        {
            //if (MainStoryViewerUc.Visibility == Visibility.Visible) return;
            try
            {
                OpenProfile(StoryFeed.User);
            }
            catch { }
        }
        public void RemoveItem(InstaStoryItem storyItem)
        {
            try
            {
                var yek = Items.SingleOrDefault(ss => ss.StoryItem.Id.ToLower() == storyItem.Id.ToLower());
                if (yek != null)
                {
                    var index = Items.IndexOf(yek);
                    //View.SkipNext();
                    Items.Remove(yek);
                    ProgressBarList.RemoveAt(index);
                    ProgressGrid.Children.RemoveAt(index);
                    ProgressGrid.ColumnDefinitions.RemoveAt(index);
                }
            }
            catch { }
        }
        
        private void TimerTick(object sender, object e)
        {

        }
        void StartProgressTimer()
        {
            try
            {
                CurrentProgress = ProgressBarList[CurrentFlipViewIndex];
                CurrentProgress.Maximum = MaximumLength = Items[CurrentFlipViewIndex].StoryItem.MediaType == InstaMediaType.Image ? MaxIntervalForImage : Items[CurrentFlipViewIndex].StoryItem.VideoDuration;
                ProgressTimer.Start();
            }
            catch (Exception ex)
            {
                ex.PrintException("StartProgressTimer");
            }
        }
        async void StopProgressTimer()
        {
            try
            {
                ProgressTimer.Stop();
                await Task.Delay(150);
                CurrentProgress = null;
            }
            catch (Exception ex)
            {
                ex.PrintException("StartProgressTimer");
            }
        }
        private void ProgressTimerTick(object sender, object e)
        {
            try
            {
                if (CurrentProgress == null)
                {
                    ProgressTimer.Stop();
                    return;
                }
                CurrentProgress.Value += ProgressChangeValue;
                if (CurrentProgress.Value == MaximumLength)
                {
                    ProgressTimer.Stop();
                    return;
                }
            }
            catch (Exception ex)
            {
                ex.PrintException("ProgressTimerTick");
            }
        }

        #region UI generators
        ColumnDefinition GenerateColumn()
        {
            return new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
        }
        ProgressBar GenerateProgress(Thickness? margin = null)
        {
            if (margin == null)
                margin = new Thickness(2.5, 5, 2.5, 5);
            //<ProgressBar Value="10" Grid.Column="1" Height="2" Margin="5" Background="#41FFFFFF" Foreground="#B5C3C3C3" />
            var p = new ProgressBar
            {
                Height = 1.6,
                Margin = margin.Value,
                Background = /*"#41FFFFFF"*/"#74FFFFFF".GetColorBrush(),
                Foreground = new SolidColorBrush(Colors.White),
                LargeChange = .01,
                SmallChange = .01
                //Foreground = "#B5C3C3C3".GetColorBrush()
            };
            try
            {
                if (DeviceUtil.OverRS2OS)
                    p.CornerRadius = new CornerRadius(0.8);
            }
            catch { }
            return p;
        }
        #endregion UI generators
    }
}
