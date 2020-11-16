using InstagramApiSharp.Classes.Models;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Minista.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace Minista.UserControls.Main
{
    public sealed partial class StoryFeedItemUc : UserControl
    {
        public ObservableCollection<InstaStoryItem> Stories { get; set; } = new ObservableCollection<InstaStoryItem>();
        public InstaStoryItem StoryItem
        {
            get
            {
                return (InstaStoryItem)GetValue(StoryItemProperty);
            }
            set
            {
                SetValue(StoryItemProperty, value);
                DataContext = value;
                OnPropertyChanged2("StoryItem");
                SetStory();
            }
        }
        public static readonly DependencyProperty StoryItemProperty =
            DependencyProperty.Register("StoryItem",
                typeof(InstaStoryItem),
                typeof(StoryFeedItemUc),
                new PropertyMetadata(null));
        public StoryFeedItemUc()
        {
            this.InitializeComponent();
            DataContextChanged += StoryFeedItemUcDataContextChanged;
            MediaElement.MediaFailed += MediaElement_MediaFailed;
        }
        bool Tried = false;
        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (!Tried)
            {
                try
                {
                    MediaElement.Play();
                }
                catch { }
                Tried = true;
            }
        }
        

        private void StoryFeedItemUcDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                if (args.NewValue is InstaStoryItem item && item != null)
                {
                    if (item.MediaType == InstaMediaType.Video)
                    {
                        Tried = false;
                        MediaElement.Source = new Uri(item.Videos[0].Uri);
                    }
                }
            }
            catch { }
        }

        public void SetStory()
        {
            try
            {
                SetImage();
                var anim = BackgroundImage.Blur(17)/*.Rotate(30)*/;
                anim.SetDurationForAll(0);
                anim.SetDelay(0);
                anim.Start();
            }
            catch { }
        }
        void SetImage()
        {
            try
            {
                if (StoryItem.MediaType == InstaMediaType.Image)
                {

                    Image.Source = StoryItem.Images[0].Uri.GetBitmap();
                }
            }
            catch { }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged2(string memberName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }

        public bool IsImageOpened = false;
        bool retry = false, retry2 = false;
        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            IsImageOpened = false;
            if (!retry)
            {
                SetImage();
                retry = true;
                "Retry Load Image".PrintDebug();
            }
            else
            {
                if (!retry2)
                {
                    SetImage();
                    retry2 = true;
                    "Retry2 Load Image".PrintDebug();
                }
                else
                    IsImageOpened = true;
            }
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            IsImageOpened = true;
            SetSuff();
        }
        void SetSuff()
        {
            try
            {
                if (NavigationService.Frame.Content is Views.Main.StoryView storyView)
                    storyView.SetStuff();
            }
            catch { }
        }
        private void Image_ImageExFailed(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageExFailedEventArgs e)
        {
            IsImageOpened = false;
            if (!retry)
            {
                SetImage();
                retry = true;
                "Retry Load Image".PrintDebug();
            }
            else
            {
                if (!retry2)
                {
                    SetImage();
                    retry2 = true;
                    "Retry2 Load Image".PrintDebug();
                }
                else
                    IsImageOpened = true;
            }
        }

        private void Image_ImageExOpened(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageExOpenedEventArgs e)
        {
            IsImageOpened = true;
            SetSuff();
        }
    }
}
