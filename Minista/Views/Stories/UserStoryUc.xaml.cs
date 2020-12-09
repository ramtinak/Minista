using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Minista.Views.Stories
{
    public sealed partial class UserStoryUc : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<StoryItemUc> Items { get; set; } = new ObservableCollection<StoryItemUc>();
        const int MaxIntervalForImage = 6;
        public DispatcherTimer Timer = new DispatcherTimer();
        public int StoryIndex { get; set; } = 0;
        readonly List<ProgressBar> ProgressBarList = new List<ProgressBar>();
        readonly GestureRecognizer GestureRecognizer = new GestureRecognizer();
        bool IsHighlight { get; set; } = false;
        public string Title { get; set; }
        public InstaReelFeed StoryFeed
        {
            get
            {
                return (InstaReelFeed)GetValue(StoryFeedProperty);
            }
            set
            {
                SetValue(StoryFeedProperty, value);
                this.DataContext = value;
                OnPropertyChanged("StoryFeed");

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
        }

        private void MainGridKTapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
