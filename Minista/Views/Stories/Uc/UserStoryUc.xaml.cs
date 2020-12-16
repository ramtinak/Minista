using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Helpers;
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
using Windows.UI.Core;
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
        #region Properties and fields
        public event EventHandler PlayNextItem;
        public event EventHandler PlayPreviousItem;

        const int MaxIntervalForImage = 6;
        ProgressBar CurrentProgress;
        const double ProgressChangeValue = 0.1;
        double MaximumLength = MaxIntervalForImage;
        public ObservableCollection<StoryItemUc> Items { get; set; } = new ObservableCollection<StoryItemUc>();
        public StoryItemUc StoryItemUc;
        public DispatcherTimer Timer = new DispatcherTimer(), ProgressTimer = new DispatcherTimer();
        readonly List<ProgressBar> ProgressBarList = new List<ProgressBar>();
        public event EventHandler ItemChanged;
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
        #endregion

        #region ctor
        StoryViewX View;
        public UserStoryUc(StoryViewX view) : this()
        {
            View = view;
            view.Navigation += OnViewNavigation;
        }
        public UserStoryUc()
        {
            InitializeComponent();
            Timer.Interval = TimeSpan.FromSeconds(1);
            Timer.Tick += TimerTick;
            ProgressTimer.Interval = TimeSpan.FromMilliseconds(80);
            ProgressTimer.Tick += ProgressTimerTick;
            Unloaded += UserStoryUc_Unloaded;
        }

        private void UserStoryUc_Unloaded(object sender, RoutedEventArgs e)
        {
            "UserStoryUc_Unloaded".PrintDebug();
        }
        #endregion


        private void OnViewNavigation(object sender, EventArgs e)
        {
            "OnViewNavigation".PrintDebug();
        }

        async void SetStoryItems()
        {
            if (StoryFeed.Items.Count == 0)
            {
                var stories = await InstaApi.StoryProcessor.GetUsersStoriesAsHighlightsAsync(StoryFeed.User.Pk.ToString());
                if (stories.Succeeded)
                {
                    if (stories.Value?.Items?.Count > 0 && stories.Value?.Items[0] != null)
                    {
                        var item = stories.Value.Items[0];
                        //StoryFeed.Items.Clear();
                        //StoryFeed.Items.AddRange(item.Items);
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
                    var uc = new StoryItemUc(this) { StoryItem = x, Index = 0 };
                    uc.MediaEnded += OnMediaEnded;
                    uc.MediaFailed += OnMediaFailed;
                    uc.MediaOpened += OnMediaOpened;
                    uc.HoldingStarted += OnUcHoldingStarted;
                    uc.HoldingStopped += OnUcHoldingStopped;
                    uc.LeftTap += OnUcLeftTap;
                    uc.RightTap += OnUcRightTap;
                    uc.StartTimer += OnUcStartTimer;
                    uc.StopTimer += OnUcStopTimer;
                    Items.Add(uc);
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

        private void OnUcStopTimer(object sender, EventArgs e)
        {
            Timer.Stop();
        }

        private void OnUcStartTimer(object sender, EventArgs e)
        {
            Timer.Start();
        }

        public void ControlPanels(bool hide = false)
        {
            try
            {
                ProgressGrid.Visibility = UserGrid.Visibility = hide ? Visibility.Collapsed : Visibility.Visible;
                if(hide)
                    Timer.Stop();
                else
                    Timer.Start();
            }
            catch { }
        }
        private void OnUcHoldingStarted(object sender, EventArgs e)
        {
            "OnUcHoldingStarted".PrintDebug();
            ControlPanels(true);
        }

        private void OnUcHoldingStopped(object sender, EventArgs e)
        {
            "OnUcHoldingStopped".PrintDebug();
            ControlPanels();
        }

        private void OnMediaOpened(object sender, StoryItemUc e)
        {
            ("OnMediaOpened:  " + e.StoryItem.Id).PrintDebug();
        }

        private void OnMediaFailed(object sender, StoryItemUc e)
        {
            ("OnMediaFailed:  " + e.StoryItem.Id).PrintDebug();
            Timer.Stop();
        }

        private void OnMediaEnded(object sender, StoryItemUc e)
        {
            ("OnMediaEnded:  " + e.StoryItem.Id).PrintDebug();
            PlayNext();
        }
        private void OnUcRightTap(object sender, EventArgs e)
        {
            PlayNext();
        }

        private void OnUcLeftTap(object sender, EventArgs e)
        {
            PlayPrevious();
        }

        public async void FirstInit(string selectedStoryId = null, int index = -1)
        {
            try
            {
                if (!string.IsNullOrEmpty(StoryFeed.Title))
                {
                    TitleText.Text = StoryFeed.Title;
                    TitleCover.Source = StoryFeed.HighlightCoverMedia.CroppedImage.Uri.GetBitmap();
                    ShowTitle();
                    await Task.Delay(2000);
                }
                if (Items?.Count > 0)
                {
                    await Task.Delay(250);
                    if (index == -1)
                    {
                        for (int i = 0; i < Items.Count; i++)
                        {
                            if (string.IsNullOrEmpty(selectedStoryId))
                            {
                                if (Items[i].StoryItem.TakenAt.ToUnixTime() == StoryFeed.Seen)
                                {
                                    index = i /*+ 1*/;
                                    break;
                                }
                            }
                            else
                            {
                                if (Items[i].StoryItem.Id == selectedStoryId ||
                                Items[i].StoryItem.Pk.ToString() == selectedStoryId)
                                {
                                    index = i;
                                    break;
                                }
                            }
                        }

                        if (index != -1)
                            FlipView.SelectedIndex = index;
                    }
                    else
                        FlipView.SelectedIndex = index;
                }
            }
            catch (Exception ex) { ex.PrintException("UserStoryUc.FirstInit"); }
        }
        public void PlayNext()
        {
            try
            {
                TimerIndex = 0;
                if (Items.Count > 1)
                {
                    var index = (CurrentFlipViewIndex + 1) % Items.Count;
                    if (index != 0)
                        FlipView.SelectedIndex = index;
                    else
                    {
                        "Go Next Person 1".PrintDebug();
                        PlayNextItem?.Invoke(this, null);
                    }
                }
                else
                {
                    "Go Next Person 2".PrintDebug();
                    PlayNextItem?.Invoke(this, null);
                }
            }
            catch { }
        }
        public void PlayPrevious()
        {
            try
            {
                TimerIndex = 0;
                if (Items.Count > 1)
                {
                    var index = (CurrentFlipViewIndex - 1) % Items.Count;
                    if (index >= 0)
                        FlipView.SelectedIndex = index;
                    else
                    {
                        "Go Previous Person".PrintDebug();
                        PlayPreviousItem?.Invoke(this, null);
                    }
                }
                else
                {
                    "Go Previous Person".PrintDebug();
                    PlayPreviousItem?.Invoke(this, null);
                }
            }
            catch { }
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
                    {
                        try
                        {
                            if (CurrentProgress != null)
                                CurrentProgress.Value = 0;
                        }
                        catch { }
                        Items[CurrentFlipViewIndex].PauseVideo();
                    }
                    Items[index].PlayVideo(index);
                    StoryItemUc = Items[index];
                    ItemChanged?.Invoke(this, null);
                    if (StoryItemUc.IsMediaLoaded)
                    {
                        TimerIndex = 0;
                        Timer.Start();
                    }

                    CurrentProgress = ProgressBarList[index];
                    try
                    {
                        if (CurrentProgress != null)
                            CurrentProgress.Value = 100;
                    }
                    catch { }
                    try
                    {
                        DateText.Text = Convert.ToString(new Converters.DateTimeConverter().Convert(StoryItemUc.StoryItem.TakenAt, null, null, null));
                    }
                    catch { }
                    //StartProgressTimer();
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

        #region Title
        void ShowTitle()
        {
            try
            {
                TitleGrid.Visibility = Visibility.Visible;
                ShowTitleStoryboard.Begin();
            }
            catch { }
        }

        private async void ShowTitleCompleted(object sender, object e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await Task.Delay(2000);
                    HideTitle();
                });
            }
            catch { }
        }
        void HideTitle()
        {
            try
            {
                HideTitleStoryboard.Begin();
            }
            catch { }
        }

        private void HideTitleCompleted(object sender, object e)
        {
            TitleGrid.Visibility = Visibility.Collapsed;
        }
        #endregion
        #region Timers
        int TimerIndex = 0;
        private void TimerTick(object sender, object e)
        {
            try
            {
                if (ProgressGrid.Visibility == Visibility.Collapsed) return;
                if (StoryItemUc != null)
                {
                    if (StoryItemUc.StoryItem.MediaType == InstaMediaType.Image)
                    {
                        if(TimerIndex > MaxIntervalForImage)
                        {
                            PlayNext();
                            return;
                        }
                        TimerIndex++;
                    }
                }
            }
            catch { }
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
        #endregion Timers

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
