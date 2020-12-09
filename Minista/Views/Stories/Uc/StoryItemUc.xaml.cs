using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Minista.ContentDialogs;
using Minista.Controls;
using Minista.Helpers;
using Minista.UserControls.Main;
using Minista.UserControls.Story;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UICompositionAnimations.Enums;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace Minista.Views.Stories
{
    public sealed partial class StoryItemUc : UserControl, INotifyPropertyChanged
    {
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
                OnPropertyChanged("StoryItem");
                SetImageOrVideo();
            }
        }
        public static readonly DependencyProperty StoryItemProperty =
            DependencyProperty.Register("StoryItem",
                typeof(InstaStoryItem),
                typeof(StoryItemUc),
                new PropertyMetadata(null));
        public int Index { get; set; } = -1;
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string memberName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        public bool IsMediaLoaded { get; private set; } = false;
        readonly InstaReelFeed StoryFeed;
        readonly UserStoryUc UserStoryUc;

        public StoryItemUc(UserStoryUc userStoryUc) : this()
        {
            UserStoryUc = userStoryUc;
            StoryFeed = userStoryUc.StoryFeed;
        }
        public StoryItemUc()
        {
            InitializeComponent();
            DataContextChanged += StoryItemUcDataContextChanged;
            Unloaded += StoryItemUcUnloaded;
            Loaded += StoryItemUcLoaded;
        }

        private void StoryItemUcLoaded(object sender, RoutedEventArgs e)
        {
            try 
            {
                if(!IsMediaLoaded)
                    SetImageOrVideo();
            }
            catch { }
        }

        private void StoryItemUcUnloaded(object sender, RoutedEventArgs e)
        {
            StopRefreshAnimation();
        }

        //public void SetStory()
        //{
        //    try
        //    {
        //        SetImageOrVideo();
        //        var anim = BackgroundImage.Blur(17);
        //        anim.SetDurationForAll(0);
        //        anim.SetDelay(0);
        //        anim.Start();
        //    }
        //    catch { }
        //}
        void SetImageOrVideo()
        {
            try
            {
                if (StoryItem.Images?.Count > 0)
                    BackgroundImage.Source = StoryItem.Images.LastOrDefault().Uri.GetBitmap();
                var anim = BackgroundImage.Blur(17);
                anim.SetDurationForAll(0);
                anim.SetDelay(0);
                anim.Start();
            }
            catch (Exception ex)
            {
                ex.PrintException("SetImageOrVideo1");
            }
            try
            {
                if (StoryItem.MediaType == InstaMediaType.Image)
                    Image.Source = StoryItem.Images[0].Uri.GetBitmap();
                else
                    MediaElement.Source = new Uri(StoryItem.Videos[0].Uri);

            }
            catch (Exception ex)
            {
                ex.PrintException("SetImageOrVideo2");
            }
        }
        bool OpenVideo = false;
        public void PlayVideo(int index)
        {
            try
            {
                MediaElement.CurrentState.PrintDebug();
                if (StoryItem.MediaType == InstaMediaType.Video)
                {
                    if (OpenVideo || index > 0)
                        MediaElement.Play();
                    else
                        OpenVideo = true;
                }
            }
            catch (Exception ex)
            {
                ex.PrintException("PlayVideo");
            }
        }
        public void PauseVideo()
        {
            try
            {
                if (StoryItem.MediaType == InstaMediaType.Video)
                    MediaElement.Pause();
            }
            catch (Exception ex)
            {
                ex.PrintException("PauseVideo");
            }
        }
        private void StoryItemUcDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            //try
            //{
            //    if (args.NewValue is InstaStoryItem item && item != null)
            //    {
            //        if (item.MediaType == InstaMediaType.Video)
            //            MediaElement.Source = new Uri(item.Videos[0].Uri);
            //    }
            //}
            //catch { }
        }

        private void OnMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            "OnMediaOpened".PrintDebug();
            IsMediaLoaded = false;
            ShowRefreshButton();
            //if (!IsMediaLoaded)
            //{
            //    try
            //    {
            //        MediaElement.Play();
            //    }
            //    catch { }
            //}
        }

        private void OnMediaOpened(object sender, RoutedEventArgs e)
        {
            "OnMediaOpened".PrintDebug();
            if (OpenVideo)
                MediaElement.Play();
            IsMediaLoaded = true;
            SetStuff();
            StopRefreshAnimation();
        }
        private void OnImageExFailed(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageExFailedEventArgs e)
        {
            IsMediaLoaded = false;
            ShowRefreshButton();
        }

        private void OnImageExOpened(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageExOpenedEventArgs e)
        {
            IsMediaLoaded = true;
            StopRefreshAnimation();
            SetStuff();
        }


        void SetStuff()
        {
            try
            {
                //IsStoryInnerShowing = false;
                StorySuffItems.Children.Clear();
                StorySuffItems.Visibility = Visibility.Visible;
                var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
                var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                var actwidth = MediaElement.ActualWidth == 0 ? Image.ActualWidth : MediaElement.ActualWidth;
                var actheight = MediaElement.ActualHeight == 0 ? Image.ActualHeight : MediaElement.ActualHeight;
                var size = AspectRatioHelper.CalculateSizeInBox(StoryItem.OriginalWidth, StoryItem.OriginalHeight, actheight, actwidth);
                StorySuffItems.Width = size.Width;
                StorySuffItems.Height = size.Height;

                if (StoryItem.StoryFeedMedia?.Count > 0)
                {
                    foreach (var item in StoryItem.StoryFeedMedia)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var rect = new Rectangle()
                        {
                            Fill = new SolidColorBrush(Colors.Black),
                            Opacity = 0,
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            DataContext = item.MediaId.ToString(),
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "StoryFeedMedia" + rndName
                        };

                        rect.Tapped += ShowPanel;
                        StorySuffItems.Children.Add(rect);
                    }
                }


                if (StoryItem.ReelMentions?.Count > 0)
                {
                    foreach (var item in StoryItem.ReelMentions)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var rect = new Rectangle()
                        {
                            Fill = new SolidColorBrush(Colors.Black),
                            Opacity = 0,
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            DataContext = item.User.UserName.ToLower(),
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "UserMention" + rndName,
                            Tag = item.User.FullName
                        };
                        rect.Tapped += ShowPanel;
                        StorySuffItems.Children.Add(rect);
                    }
                }
                if (StoryItem.StoryHashtags?.Count > 0)
                {
                    foreach (var item in StoryItem.StoryHashtags)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var rect = new Rectangle()
                        {
                            Fill = new SolidColorBrush(Colors.Black),
                            Opacity = 0,
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            DataContext = item.Hashtag.Name.ToLower(),
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "Hashtag" + rndName
                        };
                        rect.Tapped += ShowPanel;
                        StorySuffItems.Children.Add(rect);
                    }
                }

                // NOT COMPLETE
                if (StoryItem.StoryLocations?.Count > 0)
                {
                    foreach (var item in StoryItem.StoryLocations)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var rect = new Rectangle()
                        {
                            Fill = new SolidColorBrush(Colors.Black),
                            Opacity = 0,
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            DataContext = item.Location.Name.ToLower(),
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "Location" + rndName
                        };
                        rect.Tapped += ShowPanel;
                        StorySuffItems.Children.Add(rect);
                    }
                }

                // COMPLETED
                if (StoryItem.StoryPolls?.Count > 0)
                {
                    foreach (var item in StoryItem.StoryPolls)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var storyPoll = new StoryPollUc()
                        {
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "Poll" + rndName
                        };
                        storyPoll.SetItem(item, StoryItem);
                        StorySuffItems.Children.Add(storyPoll);
                    }
                }

                // COMPLETED
                if (StoryItem.StoryQuestions?.Count > 0)
                {
                    foreach (var item in StoryItem.StoryQuestions)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var storyQuestionControl = new StoryQuestionControl()
                        {
                            Background = "#A5EEF900".GetColorBrush(),
                            Opacity = 0,
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            //DataContext = item.Location.Name.ToLower(),
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "Questions" + rndName,
                            StoryId = StoryItem.Id,
                            StoryQuestionItem = item
                        };
                        storyQuestionControl.Tapped += StoryQuestionControlTapped;
                        StorySuffItems.Children.Add(storyQuestionControl);
                    }
                }

                // COMPLETED
                if (StoryItem.StoryQuizs?.Count > 0)
                {
                    foreach (var item in StoryItem.StoryQuizs)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var storyQuiz = new StoryQuizUc()
                        {
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            //DataContext = item.Location.Name.ToLower(),
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "Quizs" + rndName
                        };
                        storyQuiz.SetQuiz(item, StoryItem);
                        StorySuffItems.Children.Add(storyQuiz);
                    }
                }

                // COMPLETED
                if (StoryItem.StorySliders?.Count > 0)
                {
                    foreach (var item in StoryItem.StorySliders)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var storySliderUc = new StorySliderUc()
                        {
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "StorySliders" + rndName,
                        };
                        storySliderUc.SetItem(item, StoryItem);
                        StorySuffItems.Children.Add(storySliderUc);
                    }
                }

                // COMPLETED
                if (StoryItem.Countdowns?.Count > 0)
                {
                    foreach (var item in StoryItem.Countdowns)
                    {
                        var trans = new CompositeTransform() { CenterX = (size.Width * item.Width / 2), CenterY = (size.Height * item.Height / 2), Rotation = item.Rotation * 360 };
                        var marg = new Thickness(((item.X * size.Width) - ((item.Width / 2) * size.Width)),
                            ((item.Y * size.Height) - ((item.Height / 2) * size.Height)), 0, 0);
                        var rndName = 8.GenerateRandomStringStatic();
                        var storyCountdown = new StoryCountdownUc()
                        {
                            Margin = marg,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            RenderTransform = trans,
                            Width = item.Width * size.Width,
                            Height = item.Height * size.Height,
                            Name = "Countdowns" + rndName
                        };
                        storyCountdown.SetItem(item, StoryItem);
                        StorySuffItems.Children.Add(storyCountdown);
                    }
                }

                if (StorySuffItems.Children.Any())
                {
                    foreach (var xItem in StorySuffItems.Children)
                    {
                        var item = xItem as Rectangle;
                        if (item != null)
                        {
                            try
                            {
                                var randomName = 8.GenerateRandomStringStatic();
                                var name = "";
                                var innerType = StoryInnerUc.StoryInnerItem.Hashtag;
                                var title = "";
                                if (item.Name.Contains("StoryFeedMedia"))
                                {
                                    name = "MediaFeed" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.MediaFeed;
                                    title = "See Post";
                                }
                                else if (item.Name.Contains("UserMention"))
                                {
                                    name = "UserMention" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.UserMention;
                                    title = item.Tag.ToString();
                                }
                                else if (item.Name.Contains("Hashtag"))
                                {
                                    name = "Hashtag" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.Hashtag;
                                    title = "See Hashtag";
                                }
                                else if (item.Name.Contains("Location"))
                                {
                                    name = "Location" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.Location;
                                    title = "See Location";
                                }

                                else if (item.Name.Contains("Poll"))
                                {
                                    name = "Poll" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.Hashtag;
                                    title = "See Poll";
                                }
                                else if (item.Name.Contains("Questions"))
                                {
                                    name = "Questions" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.Hashtag;
                                    title = "See Questions";
                                }
                                else if (item.Name.Contains("Quizs"))
                                {
                                    name = "Quizs" + randomName;
                                    innerType = StoryInnerUc.StoryInnerItem.Hashtag;
                                    title = "See Quizs";
                                }
                                item.Tag = name;
                                var margin = new Thickness
                                {
                                    Bottom = item.Margin.Bottom,
                                    Left = item.Margin.Left + 20,
                                    Right = item.Margin.Right,
                                    Top = item.Margin.Top - 25
                                };
                                var innerUc = new StoryInnerUc(innerType, title, item.DataContext.ToString())
                                {
                                    Visibility = Visibility.Visible,
                                    Opacity = 0,
                                    VerticalAlignment = VerticalAlignment.Top,
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    Margin = margin,
                                    RenderTransform = item.RenderTransform,
                                    Name = name
                                };
                                StorySuffItems.Children.Add(innerUc);


                                innerUc.Visibility = Visibility.Collapsed;
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex) { ex.PrintException("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"); }
        }

        bool IsStoryInnerShowing = false;
        private async void ShowPanel(object sender, TappedRoutedEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                try
                {
                    if (StorySuffItems.Children.Count > 0)
                    {
                        for (int i = 0; i < StorySuffItems.Children.Count; i++)
                        {
                            if (StorySuffItems.Children[i] is StoryInnerUc item && item != null)
                            {
                                if (item.Name == rect.Tag.ToString())
                                {
                                    //IsHolding = true;
                                    IsStoryInnerShowing = true;
                                        await item.Animation(FrameworkLayer.Xaml)
                                              .Scale(1, 0, Easing.QuadraticEaseInOut)
                                              .Duration(0)
                                              .StartAsync();
                                        item.Visibility = Visibility.Visible;

                                        await item.Animation(FrameworkLayer.Xaml)
                                              .Opacity(0, 1, Easing.CircleEaseOut)
                                              .Scale(0, 1.2, Easing.QuadraticEaseInOut)
                                              .Duration(250)
                                              .StartAsync();


                                        await item.Animation(FrameworkLayer.Xaml)
                                                .Scale(1.2, 1, Easing.QuadraticEaseInOut)
                                                .Duration(80)
                                                .StartAsync();
                                    break;
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private async void StoryQuestionControlTapped(object sender, TappedRoutedEventArgs e)
        {
            //IsHolding = true;
            try
            {
                if (sender is StoryQuestionControl storyQuestion && storyQuestion != null)
                    if (storyQuestion.StoryQuestionItem.QuestionSticker.ViewerCanInteract)
                        await new StoryQuestionDialog(storyQuestion).ShowAsync();
            }
            catch { }
            //IsHolding = false;
        }

        void HideStoryInnerPanels()
        {
            try
            {
                if (IsStoryInnerShowing)
                {
                    if (StorySuffItems.Children.Count > 0)
                    {
                        for (int i = 0; i < StorySuffItems.Children.Count; i++)
                        {
                            if (StorySuffItems.Children[i] is StoryInnerUc item)
                            {
                                item.Visibility = Visibility.Collapsed;
                                item.Opacity = 0;
                            }
                        }
                        //IsHolding = false;
                        IsStoryInnerShowing = false;
                    }
                }
            }
            catch { }

        }
        readonly Storyboard StoryboardX = new Storyboard();
        FontIcon RefreshFontIcon;
        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            SetImageOrVideo();
            StartRefreshAnimation(); 
        }

        private void LeftGridTapped(object sender, TappedRoutedEventArgs e)
        {
            //if (IsHolding) return;
            //SkipPrevious();
        }

        private void RightGridTapped(object sender, TappedRoutedEventArgs e)
        {
            //if (IsHolding) return;
            //SkipNext();
        }

        private void MainGridKTapped(object sender, TappedRoutedEventArgs e)
        {

        }
        private void FontIconConLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FontIcon fontIcon)
                    RefreshFontIcon = fontIcon;
            }
            catch (Exception ex)
            {
                ex.PrintException("FontIconConLoaded");
            }
        }

        private void SeeMoreButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StoryItem.StoryCTA?.Count > 0)
                {
                    var url = StoryItem.StoryCTA.FirstOrDefault().WebUri;

                    if (url.Contains("l.instagram.com/"))
                    {
                        var u = url.Substring(url.IndexOf("?u=") + "?u=".Length);
                        u = u.Substring(0, u.IndexOf("&"));
                        if (u.Contains("instagram.com/"))
                            UriHelper.HandleUri(System.Net.WebUtility.HtmlDecode(System.Net.WebUtility.UrlDecode(u)));
                        else
                            url.OpenUrl();
                    }
                    else
                        url.OpenUrl();
                }
            }
            catch { }
        }
        private void ReactionGVItemClick(object sender, ItemClickEventArgs e)
        { }
        private async void ReplyButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        //Items[StoryIndex].StoryItem, StoryFeed
                        var type = InstaSharingType.Photo;
                        if (StoryItem.MediaType == InstaMediaType.Video)
                            type = InstaSharingType.Video;
                        //IsHolding = false;
                        var reply = await Helper.InstaApi.StoryProcessor.ReplyToStoryAsync(StoryItem.Id, StoryItem.User.Pk,
                            ReplyText.Text, type);

                        if (reply.Succeeded)
                        {
                            ReplyText.Text = string.Empty;
                            Helper.ShowNotify($"Reply sent.");
                        }

                        //if (Items[StoryIndex]?.StoryItem.MediaType == InstaMediaType.Video)
                        //{
                        //    IsHolding = false;
                        //    Items[StoryIndex].MediaElement.Play();
                        //}
                    }
                    catch { }
                });

            }
            catch { }
        }
        private void ReplyTextGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                //IsHolding = true;
                ReactionGV.Visibility = Visibility.Visible;
            }
            catch { }
        }

        private void ReplyTextLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                //IsHolding = false;
                ReactionGV.Visibility = Visibility.Collapsed;
            }
            catch { }
        }
        private void ReplyTextTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ReplyText.Text))
                {
                    ReplyButton.Visibility = Visibility.Collapsed;
                    ShareButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ReplyButton.Visibility = Visibility.Visible;
                    ShareButton.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }
        private void SeenByButtonClick(object sender, RoutedEventArgs e)
        {
            //if (Items.Count > StoryIndex)
            //{
            //    IsHolding = true;
            //    StorySuffItems.Visibility = Visibility.Collapsed;
            //    MainStoryViewerUc.SetStoryItem(Items[StoryIndex].StoryItem);
            //}
        }
        private async void ShareButtonClick(object sender, RoutedEventArgs e)
        {
            //if (MainStoryViewerUc.Visibility == Visibility.Visible) return;
            try
            {
                //IsHolding = true;
                await new UsersDialog(StoryItem, StoryFeed).ShowAsync();
            }
            catch { }
            //IsHolding = false;
            try
            {
                if (StoryItem.MediaType == InstaMediaType.Video)
                MediaElement.Play();
             
            }
            catch { }
        }
        private async void MoreOptionsButtonClick(object sender, RoutedEventArgs e)
        {
            //IsHolding = true;
            try
            {
                await new StoryMenuDialog(StoryFeed, StoryItem, UserStoryUc).ShowAsync();
            }
            catch { }
            //IsHolding = false;

        }
        void StartRefreshAnimation()
        {
            try
            {
                if (RefreshFontIcon != null)
                {
                    DoubleAnimation doubleAnimation = new DoubleAnimation
                    {
                        From = 0,
                        To = 360,
                        Duration = new Duration(TimeSpan.FromSeconds(1.1)),
                        RepeatBehavior = RepeatBehavior.Forever
                    };
                    Storyboard.SetTarget(doubleAnimation, RefreshFontIcon);
                    Storyboard.SetTargetProperty(doubleAnimation, "(UIElement.RenderTransform).(RotateTransform.Angle)");
                    StoryboardX.Children.Clear();
                    StoryboardX.Children.Add(doubleAnimation);
                    StoryboardX.Begin();
                }
            }
            catch (Exception ex)
            {
                ex.PrintException("StartRefreshAnimation");
            }
        }
        void StopRefreshAnimation()
        {
            try
            {
                HideRefreshButton();
                StoryboardX.Stop();
            }
            catch (Exception ex)
            {
                ex.PrintException("StopRefreshAnimation");
            }
        }
        void ShowRefreshButton()
        {
            RefreshButton.IsEnabled = true;
            RefreshButton.Visibility = Visibility.Visible;
        }
        void HideRefreshButton() => RefreshButton.Visibility = Visibility.Collapsed;
    }
}
