using InstagramApiSharp.Classes.Models;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Minista.ContentDialogs;
using Minista.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UICompositionAnimations.Enums;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

#pragma warning disable CS0628
#pragma warning disable IDE0019
#pragma warning disable IDE0008

namespace Minista.UserControls.Main
{
    public sealed partial class MediaMainX2Uc : UserControl
    {
        public InstaMedia Media { get; set; }


        public bool IsArchivePost
        {
            get
            {
                return (bool)GetValue(IsArchivePostProperty);
            }
            set
            {
                SetValue(IsArchivePostProperty, value);
            }
        }
        public static readonly DependencyProperty IsArchivePostProperty =
            DependencyProperty.Register("IsArchivePost",
                typeof(bool),
                typeof(MediaMainX2Uc),
                new PropertyMetadata(false));

        //public InstaMedia Media
        //{
        //    get
        //    {
        //        return (InstaMedia)GetValue(MediaProperty);
        //    }
        //    set
        //    {
        //        SetValue(MediaProperty, value);
        //        //this.DataContext = value;
        //        //OnPropertyChanged("Media");
        //        //SetDataContext(value);
        //    }
        //}
        //public static readonly DependencyProperty MediaProperty =
        //    DependencyProperty.Register("Media",
        //        typeof(InstaMedia),
        //        typeof(MediaMainUc),
        //        new PropertyMetadata(null));

        public bool MiniMode
        {
            get
            {
                return (bool)GetValue(MiniModeProperty);
            }
            set
            {
                SetValue(MiniModeProperty, value);
                SetMiniMode(value);
            }
        }
        public static readonly DependencyProperty MiniModeProperty =
            DependencyProperty.Register("MiniMode",
                typeof(bool),
                typeof(MediaMainUc),
                new PropertyMetadata(false));
        private readonly Compositor _compositor;
        private readonly Visual _countGridVisual;
        public MediaMainX2Uc()
        {
            this.InitializeComponent();
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _countGridVisual = CountGrid.GetVisual();
            DataContextChanged += MediaMainUc_DataContextChanged;
            SizeChanged += MediaMainUc_SizeChanged;
        }

        private void MediaMainUc_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //try
            //{
            //    //FlipView.Width = ActualWidth;
            //    if (FlipView.Items.Count > 0)
            //    {
            //        for (int i = 0; i < FlipView.Items.Count; i++)
            //        {
            //            try
            //            {
            //                if (FlipView.Items[i] is ImageEx imageEx)
            //                    imageEx.Width = ActualWidth;
            //                else if (FlipView.Items[i] is MediaElement mediaElement)
            //                    mediaElement.Width = ActualWidth;
            //            }
            //            catch { }
            //        }
            //    }
            //}
            //catch { }
        }

        public void SetMiniMode(bool miniMode)
        {
            if (miniMode)
            {
                MEPlayer.MinHeight = 250;
                //txtCaption.MaxHeight = 120;
                txtCaption.Height = 85;
                ShowMoreButton.Visibility = Visibility.Visible;
                CommentGrid.Visibility = Visibility.Collapsed;
                PreviewCommentsGrid.Visibility = Visibility.Collapsed;
                FollowUnfollowButton.Visibility = Visibility.Visible;
            }
        }
        private void MediaMainUc_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue is InstaMedia media && media != null)
                SetDataContext(media);
        }
        void SetDataContext(InstaMedia media)
        {
            //if (args.NewValue is InstaPost post && post != null && post.Media != null)
            //{
            //    if (post.Media.Location != null)
            //        LocationText.Visibility = Visibility.Visible;
            //    else LocationText.Visibility = Visibility.Collapsed;
            //    SetCaption(post.Media);
            //    SetLike(post.Media.HasLiked);
            //    SetCarousel(post.Media);
            //}
            //else
            if (media != null)
            {
                Media = media;
                if (Media.Location != null)
                    LocationText.Visibility = Visibility.Visible;
                else LocationText.Visibility = Visibility.Collapsed;
                try
                {
                    void ShowHideInsights(bool show) => ViewInsightsGrid.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                    if (Media.User.Pk == Helper.CurrentUser?.Pk)
                    {
                        if (Helper.CurrentUser.IsBusiness)
                            ShowHideInsights(Helper.CurrentUser.IsBusiness);
                        else
                            ShowHideInsights(false);
                    }
                }
                catch { }
                SetCaption(Media);
                SetLike(Media.HasLiked);
                SetCarousel(Media);
                SetUserTags(Media);
                if (media.MediaType == InstaMediaType.Image)
                {
                    VideoUserTagButton.Visibility = Visibility.Collapsed;
                    if (media.UserTags.Count > 0)
                    {
                        ImageUserTagButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ImageUserTagButton.Visibility = Visibility.Collapsed;
                    }
                }
                else if (media.MediaType == InstaMediaType.Video)
                {
                    ImageUserTagButton.Visibility = Visibility.Collapsed;
                    if (media.UserTags.Count > 0)
                    {
                        VideoUserTagButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        VideoUserTagButton.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        void SetUserTags(InstaMedia media)
        {
            try
            {
                if (media.MediaType == InstaMediaType.Image)
                {
                    ImageUserTags.Children.Clear();
                    if (media.UserTags?.Count > 0)
                    {

                        for (int i = 0; i < media.UserTags.Count; i++)
                        {
                            var item = media.UserTags[i];


                            var trashItem = new MediaTagUc()
                            {
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,

                            };
                            trashItem.SetUserTag(item);


                            var cX = CalculateXPosition(item.Position.X, ImageUserTags.ActualWidth, trashItem.ActualWidth);
                            var cY = CalculateYPosition(item.Position.Y, ImageUserTags.ActualHeight, trashItem.ActualHeight);

                            var trans = new CompositeTransform()
                            {
                                TranslateX = cX,
                                TranslateY = cY,
                            };
                            var rndName = 8.GenerateRandomStringStatic();

                            var mtc = new MediaTagUc()
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                Opacity = 0,
                                RenderTransform = trans,
                                Name = "UserTags" + rndName,
                            };
                            mtc.SetUserTag(item);
                            mtc.Tapped += ShowPanel;
                            //mtc.Visibility = Visibility.Collapsed;
                            ImageUserTags.Children.Add(mtc);

                            cX = CalculateXPosition(item.Position.X, ImageUserTags.ActualWidth, mtc.ActualWidth);
                            cY = CalculateYPosition(item.Position.Y, ImageUserTags.ActualHeight, mtc.ActualHeight);

                            mtc.RenderTransform = new CompositeTransform()
                            {
                                TranslateX = cX,
                                TranslateY = cY,
                            };
                        }
                    }
                }
                else if (media.MediaType == InstaMediaType.Carousel)
                {
                    try
                    {
                        if (Media.Carousel.Count == FlipView.Items.Count)
                        {
                            for (int a = 0; a < Media.Carousel.Count; a++)
                            {
                                var carouselItem = Media.Carousel[a];
                                if (carouselItem != null)
                                {
                                    if (carouselItem.MediaType == InstaMediaType.Image)
                                    {
                                        var countX = FlipView.Items.Count;
                                        if (FlipView.ItemsPanelRoot == null)
                                            break;
                                        var ix = FlipView.ItemsPanelRoot.Children[a] as FlipViewItem;
                                        if (ix != null && ix?.ContentTemplateRoot != null)
                                        {
                                            var mainGrid = ix.ContentTemplateRoot as Grid;
                                            if (mainGrid?.Children.Count > 0)
                                            {
                                                for (int b = 0; b < mainGrid.Children.Count; b++)
                                                {
                                                    var secondGrid = mainGrid.Children[b] as Grid;
                                                    if (secondGrid != null)
                                                    {
                                                        if (secondGrid.Name == "ImageVideoUserTagsTemplate")
                                                        {
                                                            secondGrid.Children.Clear();
                                                            if (carouselItem.UserTags.Count > 0)
                                                            {
                                                                for (int i = 0; i < carouselItem.UserTags.Count; i++)
                                                                {
                                                                    var item = carouselItem.UserTags[i];


                                                                    var cX = CalculateXPosition(item.Position.X, secondGrid.ActualWidth, 0);
                                                                    var cY = CalculateYPosition(item.Position.Y, secondGrid.ActualHeight,0);

                                                                    var trans = new CompositeTransform()
                                                                    {
                                                                        TranslateX = cX,
                                                                        TranslateY = cY,
                                                                    };
                                                                    var rndName = 8.GenerateRandomStringStatic();

                                                                    var mtc = new MediaTagUc()
                                                                    {
                                                                        HorizontalAlignment = HorizontalAlignment.Left,
                                                                        VerticalAlignment = VerticalAlignment.Top,
                                                                        Opacity = 0,
                                                                        RenderTransform = trans,
                                                                        Name = "UserTags" + rndName,
                                                                    };
                                                                    mtc.SetUserTag(item);
                                                                    mtc.Tapped += ShowPanel;
                                                                    secondGrid.Children.Add(mtc);
                                                                    cX = CalculateXPosition(item.Position.X, secondGrid.ActualWidth, mtc.ActualWidth);
                                                                    cY = CalculateYPosition(item.Position.Y, secondGrid.ActualHeight, mtc.ActualHeight);

                                                                    mtc.RenderTransform = new CompositeTransform()
                                                                    {
                                                                        TranslateX = cX,
                                                                        TranslateY = cY,
                                                                    };
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        double CalculateXPosition(double x, double parentControlWidth, double controlWidth)
        {
            var n = x * parentControlWidth;

            if (n + controlWidth > parentControlWidth)
            {
                var more = parentControlWidth - (n + controlWidth);
                return n + more;
            }
            else if (n - controlWidth < 0)
            {

                return n + controlWidth;
            }


            return n;
        }
        double CalculateYPosition(double y, double parentControlHeight, double controlHeight)
        {
            var n = y * parentControlHeight;
            if (n + controlHeight > parentControlHeight)
            {
                var more = parentControlHeight - (n + controlHeight);
                return n + more;
            }
            else if (n - controlHeight < 0)
            {

                return n + controlHeight;
            }
            return n;
        }
        private void ImageUserTagsSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (Media == null) return;
                if (Media.MediaType == InstaMediaType.Image)
                {
                    if (ImageUserTags.Children.Count > 0)
                    {

                        for (int i = 0; i < ImageUserTags.Children.Count; i++)
                        {
                            try
                            {
                                var mtc = ImageUserTags.Children[i] as MediaTagUc;
                                if (mtc != null)
                                {
                                    if (mtc.UserTag != null)
                                    {
                                        var item = mtc.UserTag;
                                        var gridItem = mtc;
                                        var cX = CalculateXPosition(item.Position.X, ImageUserTags.ActualWidth, gridItem.ActualWidth);
                                        var cY = CalculateYPosition(item.Position.Y, ImageUserTags.ActualHeight, gridItem.ActualHeight);
                                        var trans = new CompositeTransform()
                                        {
                                            TranslateX = cX,
                                            TranslateY = cY
                                        };
                                        mtc.RenderTransform = trans;
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                else if (Media.MediaType == InstaMediaType.Carousel)
                {
                    try
                    {
                        if (Media.Carousel.Count == FlipView.Items.Count)
                            for (int a = 0; a < Media.Carousel.Count; a++)
                            {
                                var carouselItem = Media.Carousel[a];
                                if (carouselItem != null)
                                {
                                    if (carouselItem.MediaType == InstaMediaType.Image)
                                    {
                                        var countX = FlipView.Items.Count;
                                        if (FlipView.ItemsPanelRoot == null)
                                            break;

                                        var ix = FlipView.ItemsPanelRoot.Children[a] as FlipViewItem;
                                        if (ix != null && ix?.ContentTemplateRoot != null)
                                        {
                                            var mainGrid = ix.ContentTemplateRoot as Grid;
                                            if (mainGrid?.Children.Count > 0)
                                            {
                                                for (int b = 0; b < mainGrid.Children.Count; b++)
                                                {
                                                    var secondGrid = mainGrid.Children[b] as Grid;
                                                    if (secondGrid != null)
                                                    {

                                                        if (secondGrid.Name == "ImageVideoUserTagsTemplate")
                                                        {
                                                            secondGrid.Children.Clear();

                                                            for (int i = 0; i < secondGrid.Children.Count; i++)
                                                            {
                                                                try
                                                                {

                                                                    var mtc = secondGrid.Children[i] as MediaTagUc;
                                                                    if (mtc != null)
                                                                    {
                                                                        if (mtc.UserTag != null)
                                                                        {
                                                                            var item = mtc.UserTag;
                                                                            var gridItem = mtc;
                                                                            var cX = CalculateXPosition(item.Position.X, secondGrid.ActualWidth, gridItem.ActualWidth);
                                                                            var cY = CalculateYPosition(item.Position.Y, secondGrid.ActualHeight, gridItem.ActualHeight);
                                                                            var trans = new CompositeTransform()
                                                                            {
                                                                                TranslateX = cX,
                                                                                TranslateY = cY
                                                                            };
                                                                            mtc.RenderTransform = trans;
                                                                        }
                                                                    }
                                                                }
                                                                catch { }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                    }
                    catch { }

                }
            }
            catch { }

        }

        private void ShowPanel(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                //var grid = sender as Grid;
                //if (grid == null) return;
                var mtu = sender as MediaTagUc;
                if (mtu != null && mtu?.UserTag != null)
                {
                    if (mtu.Opacity != 1) return;
                    "ShowPanel".PrintDebug();
                    Helper.OpenProfile(mtu.UserTag.User);
                }
            }
            catch { }
        }

        private /*async*/ void ImageUserTagsTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (ImageUserTags.Children.Count == 0) return;
                var grid = ImageUserTags.Children[0] as MediaTagUc;
                if (grid == null) return;

                if (grid.Opacity == 1)
                {
                    try
                    {
                        for (int i = 0; i < ImageUserTags.Children.Count; i++)
                        {
                            var item = ImageUserTags.Children[i] as MediaTagUc;
                            if (item != null)
                            {
                                /*await*/
                                item.Animation(FrameworkLayer.Xaml)
                                       .Scale(1, 1.2, Easing.QuadraticEaseInOut)
                                           .Duration(100)
                                           .Start();


                                /*await*/
                                item.Animation(FrameworkLayer.Xaml)
                            .Opacity(1, 0, Easing.CircleEaseOut)
                            .Scale(1.2, 0, Easing.QuadraticEaseInOut)
                            .Duration(400)
                            .Start();

                                //item.Opacity = 0;

                                item.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        for (int i = 0; i < ImageUserTags.Children.Count; i++)
                        {
                            try
                            {
                                var item = ImageUserTags.Children[i] as MediaTagUc;
                                if (item != null)
                                {
                                    item.Opacity = 1;
                                    /*await*/
                                    item.Animation(FrameworkLayer.Xaml)
                                      .Scale(1, 0, Easing.QuadraticEaseInOut)
                                      .Duration(0)
                                      //.Delay(250)
                                      .Start();
                                    item.Visibility = Visibility.Visible;

                                    /*await*/
                                    item.Animation(FrameworkLayer.Xaml)
                                .Opacity(0, 1, Easing.CircleEaseOut)
                                .Scale(0, 1.2, Easing.QuadraticEaseInOut)
                                .Duration(400)
                                .Start();


                                    /*await*/
                                    item.Animation(FrameworkLayer.Xaml)
                                  .Scale(1.2, 1, Easing.QuadraticEaseInOut)
                                  .Duration(100)
                                  .Start();
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }


        private void VideoUserTagsTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (Media == null) return;
                if (Media.MediaType != InstaMediaType.Carousel) return;
                if (Media.Carousel[FlipView.SelectedIndex].UserTags.Count == 0) return;

                if (FlipView.ItemsPanelRoot == null) return;

                var ix = FlipView.ItemsPanelRoot.Children[FlipView.SelectedIndex] as FlipViewItem;
                Grid ImageVideoUserTagsTemplate = null;
                if (ix != null && ix?.ContentTemplateRoot != null)
                {
                    var mainGrid = ix.ContentTemplateRoot as Grid;
                    if (mainGrid?.Children.Count > 0)
                    {
                        for (int b = 0; b < mainGrid.Children.Count; b++)
                        {
                            var secondGrid = mainGrid.Children[b] as Grid;
                            if (secondGrid != null)
                            {

                                if (secondGrid.Name == "ImageVideoUserTagsTemplate")
                                {
                                    ImageVideoUserTagsTemplate = secondGrid;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (ImageVideoUserTagsTemplate == null)
                    return;
                var grid = ImageVideoUserTagsTemplate.Children[0] as MediaTagUc;
                if (grid == null) return;

                if (grid.Opacity == 1)
                {
                    try
                    {
                        for (int i = 0; i < ImageVideoUserTagsTemplate.Children.Count; i++)
                        {
                            var item = ImageVideoUserTagsTemplate.Children[i] as MediaTagUc;
                            if (item != null)
                            {
                                /*await*/
                                item.Animation(FrameworkLayer.Xaml)
                                       .Scale(1, 1.2, Easing.QuadraticEaseInOut)
                                           .Duration(100)
                                           .Start();


                                /*await*/
                                item.Animation(FrameworkLayer.Xaml)
                            .Opacity(1, 0, Easing.CircleEaseOut)
                            .Scale(1.2, 0, Easing.QuadraticEaseInOut)
                            .Duration(400)
                            .Start();

                                //item.Opacity = 0;

                                item.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        for (int i = 0; i < ImageVideoUserTagsTemplate.Children.Count; i++)
                        {
                            try
                            {
                                var item = ImageVideoUserTagsTemplate.Children[i] as MediaTagUc;
                                if (item != null)
                                {
                                    item.Opacity = 1;
                                    /*await*/
                                    item.Animation(FrameworkLayer.Xaml)
                                      .Scale(1, 0, Easing.QuadraticEaseInOut)
                                      .Duration(0)
                                      //.Delay(250)
                                      .Start();
                                    item.Visibility = Visibility.Visible;

                                    /*await*/
                                    item.Animation(FrameworkLayer.Xaml)
                                .Opacity(0, 1, Easing.CircleEaseOut)
                                .Scale(0, 1.2, Easing.QuadraticEaseInOut)
                                .Duration(400)
                                .Start();


                                    /*await*/
                                    item.Animation(FrameworkLayer.Xaml)
                                  .Scale(1.2, 1, Easing.QuadraticEaseInOut)
                                  .Duration(100)
                                  .Start();
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
        private async void UserTagButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Media == null) return;
                if (Media.MediaType == InstaMediaType.Image || Media.MediaType == InstaMediaType.Video)
                {
                    await new MediaUserTagDialog(Media.UserTags,Media.MediaType).ShowAsync();
                }
                else
                {
                    var carouselItem = Media.Carousel[FlipView.SelectedIndex];
                    await new MediaUserTagDialog(carouselItem.UserTags, carouselItem.MediaType).ShowAsync();

                }
            }
            catch { }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string memberName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }
        void SetCarousel(InstaMedia media)
        {
            try
            {
                CarouselDotsGrid.Children.Clear();
                if (media.MediaType == InstaMediaType.Carousel && media.Carousel?.Count > 0)
                {
                    for (int i = 0; i < media.Carousel.Count; i++)
                    {
                        var ellipse = new Ellipse
                        {
                            Margin = new Thickness(2),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Grid.SetColumn(ellipse, i);
                        if (i == 0)
                        {
                            ellipse.Fill = "#FFCBCBCB".GetColorBrush();// bozorg
                            ellipse.Height = ellipse.Width = 8;
                        }
                        else
                        {
                            ellipse.Fill = "#FF4C4A48".GetColorBrush();// kochik
                            ellipse.Height = ellipse.Width = 6;
                        }
                        CarouselDotsGrid.Children.Add(ellipse);
                    }

                    if (!string.IsNullOrEmpty(media.CarouselShareChildMediaId))
                    {
                        var defaultMedia = media.Carousel.FirstOrDefault(m => m.InstaIdentifier == media.CarouselShareChildMediaId);
                        if (defaultMedia != null)
                        {
                            var index = media.Carousel.IndexOf(defaultMedia);
                            if (index != -1)
                                FlipView.SelectedIndex = index;
                        }
                    }
                }
            }
            catch { }
        }
        void SetLike(bool hasLiked)
        {
            LikeButton.Content = hasLiked ? "" : "";

            LikeButton.Foreground = hasLiked ? Helper.GetColorBrush("#FFE03939") : (SolidColorBrush)Application.Current.Resources["DefaultForegroundColor"];
        }
        //bool IsCaptionSet = false;
        async void SetCaption(InstaMedia media)
        {
            try
            {
                //if (IsCaptionSet)
                //    return;
                //IsCaptionSet = true;
                if (media == null)
                {
                    txtCaption.Blocks.Clear();

                    return;
                }



                if (media.Caption == null)
                {
                    txtCaption.Blocks.Clear();

                    return;
                }

                await MainPage.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //txtCaptionX.Inlines
                    //using (var pg = new PassageHelper())
                    //{
                    //    var passages = pg.GetParagraph(media.Caption.Text, CaptionHyperLinkClick);
                    //    txtCaption.Blocks.Clear();
                    //    txtCaption.Blocks.Add(passages);
                    //}
                    //using (var pg = new PassageHelperX())
                    //{
                    //    var passages = pg.GetInlines(media.Caption.Text, CaptionHyperLinkClick);
                    //    txtCaptionX.Inlines.Clear();
                    //    txtCaptionX.FlowDirection = passages.Item2 ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                    //    passages.Item1.ForEach(item =>
                    //    txtCaptionX.Inlines.Add(item));
                    //}
                    var text = media.Caption.Text;
                    var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                    ShowMoreButton.Visibility = Visibility.Collapsed;
                    if (lines?.Length > 3)
                    {
                        txtCaption.Height = 85;
                        ShowMoreButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        var spaces = text.Split(new string[] { " ", "," }, StringSplitOptions.None);
                        if (spaces?.Length > 25)
                        {
                            txtCaption.Height = 85;
                            ShowMoreButton.Visibility = Visibility.Visible;
                        }
                    }
                    using (var pg = new PassageHelperX())
                    {
                        var passages = pg.GetInlines(text, HyperLinkHelper.HyperLinkClick);
                        txtCaption.Blocks.Clear();
                        txtCaption.FlowDirection = passages.Item2 ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                        var p = new Paragraph();
                        passages.Item1.ForEach(item =>
                        p.Inlines.Add(item));
                        txtCaption.Blocks.Add(p);
                    }
                });
                //using (var pg = new WordsHelper())
                //{
                //    var passages = pg.GetParagraph(Media.Caption.Text, CaptionHyperLinkClick);
                //    txtCaption.Blocks.Clear();
                //    passages.ForEach(p=>
                //        txtCaption.Blocks.Add(p));
                //}
            }
            catch { }
        }

        private void ShowMoreButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowMoreButton.Visibility = Visibility.Collapsed;
                txtCaption.Height = double.NaN;
            }
            catch { }
        }
        private async void MenuButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await new MediaDialog(Media, IsArchivePost, FlipView.SelectedIndex).ShowAsync();
            }
            catch { }
        }

        private async void LikeButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (Media.HasLiked)
                    {
                        Helper.SetAnimation(await AssetHelper.GetAsync(LottieTypes.Dislike));
                        Media.HasLiked = false;
                        Media.LikesCount--;
                        SetLike(false);
                        var unlike = await Helper.InstaApi.MediaProcessor.UnLikeMediaAsync(Media.InstaIdentifier);
                        if (!unlike.Succeeded)
                        {
                            if (unlike.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                            Media.LikesCount++;
                            Media.HasLiked = true;
                            SetLike(true);
                        }
                    }
                    else
                    {
                        Helper.SetAnimation(await AssetHelper.GetAsync(LottieTypes.Heart));
                        Media.HasLiked = true;
                        Media.LikesCount++;
                        SetLike(true);
                        var like = await Helper.InstaApi.MediaProcessor.LikeMediaAsync(Media.InstaIdentifier);
                        if (!like.Succeeded)
                        {
                            if (like.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                            Media.HasLiked = false;
                            Media.LikesCount--;
                            SetLike(false);
                        }
                    }
                });
            }
            catch { }
        }
        private async void LikeButtonPrevCommentClick(object sender, RoutedEventArgs e)
        {
            // like | main comment

            if (sender is AppBarButton btn && btn != null)
            {
                btn.DataContext.GetType().PrintDebug();
                if (btn.DataContext is InstaComment data && data != null)
                {
                    try
                    {
                        if (data.HasLikedComment)
                        {
                            var result = await Helper.InstaApi.CommentProcessor.UnlikeCommentAsync(data.Pk.ToString());
                            if (result.Succeeded)
                            {
                                data.HasLikedComment = false;
                                //btn.DataContext = data;
                            }
                        }
                        else
                        {
                            var result = await Helper.InstaApi.CommentProcessor.LikeCommentAsync(data.Pk.ToString());
                            if (result.Succeeded)
                            {
                                data.HasLikedComment = true;
                                //btn.DataContext = data;
                            }
                        }
                    }
                    catch { }
                }
                else if (btn.DataContext is InstaCommentShort data2 && data2 != null)
                {
                    try
                    {
                        if (data2.HasLikedComment)
                        {
                            var result = await Helper.InstaApi.CommentProcessor.UnlikeCommentAsync(data2.Pk.ToString());
                            if (result.Succeeded)
                            {
                                data2.HasLikedComment = false;
                                //btn.DataContext = data2;
                            }
                        }
                        else
                        {
                            var result = await Helper.InstaApi.CommentProcessor.LikeCommentAsync(data2.Pk.ToString());
                            if (result.Succeeded)
                            {
                                data2.HasLikedComment = true;
                                //btn.DataContext = data2;
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        private void UserButtonClick(object sender, RoutedEventArgs e)
        {
            if (Media == null)
                return;
            if (Media.FollowHashtagInfo == null)
            {
                try
                {
                    //THIS THIS THIS THIS THIS
                    //if (NavigationService.Frame.Content is Views.Main.MainView view && view != null)
                    //    view.MediaMainUc = this;
                }
                catch { }
                try
                {
                    var connectedAnimationService = ConnectedAnimationService.GetForCurrentView();
                    connectedAnimationService.DefaultDuration = TimeSpan.FromMilliseconds(850);

                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("UserImage", UserImage);
                }
                catch { }
                Helper.OpenProfile(new object[] { Media.User.ToUserShort(), UserImage.Fill });
            }
            else
            {
                NavigationService.Navigate(typeof(Views.Infos.HashtagView), Media.FollowHashtagInfo.Name);
            }
        }

        private void TextBlockDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                if (sender is TextBlock textBlock && args.NewValue is InstaComment comment)
                {
                    using (var pg = new PassageHelperX())
                    {
                        var passages = pg.GetInlines(comment.Text, HyperLinkHelper.HyperLinkClick);
                        textBlock.Inlines.Clear();
                        textBlock.FlowDirection = passages.Item2 ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                        passages.Item1.ForEach(item =>
                        textBlock.Inlines.Add(item));
                    }
                }
            }
            catch { }

        }
        private void MediaElementCurrentStateChanged(object sender, RoutedEventArgs e)
        {

        }

        public void PauseVideo()
        {
            try
            {
                if (Media.MediaType == InstaMediaType.Image) return;
                if (Media.MediaType == InstaMediaType.Video)
                {
                    //if (MEPlayer.CurrentState == MediaElementState.Buffering || MEPlayer.CurrentState == MediaElementState.Playing)
                    MEPlayer.Pause();
                }
                else /*if (Media.MediaType == InstaMediaType.Carousel)*/
                {
                    foreach (var item in FlipView.ItemsPanelRoot.Children)
                    {
                        try
                        {
                            if (item.GetType() == typeof(Grid))
                            {
                                var grid = item as Grid;
                                if (grid != null)
                                {
                                    var me = grid.Children[0] as MediaElement;
                                    if(me != null)
                                        me.Stop();
                                }
                            }
                        }
                        catch { }
                    }
                    //if (MEPlayer.CurrentState == MediaElementState.Buffering || MEPlayer.CurrentState == MediaElementState.Playing)
                    var fp = FlipView.ItemsPanelRoot.Children[FlipView.SelectedIndex] as FlipViewItem;
                    if (fp != null)
                    {
                        var rootGrid = fp.ContentTemplateRoot as Grid;
                        if (rootGrid != null)
                        {
                            MediaElement me = rootGrid.Children[0] as MediaElement;
                            if (me != null)
                            {
                                if (PlayNextFromMediaEnd)
                                    me.Stop();
                            }
                        }

                    }
                }
            }
            catch { }
        }
        bool PlayNextFromMediaEnd = false;
        private void MediaElementMediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FlipView.SelectedIndex < FlipView.Items.Count - 1)
                {
                    PlayNextFromMediaEnd = true;
                    FlipView.SelectedIndex++;
                }
            }
            catch { }
        }
        MediaElement LatestMediaElement;
        private async void FlipViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (FlipView.SelectedIndex != -1)
                {
                    CountText.Text = $"{FlipView.SelectedIndex + 1}/{FlipView.Items.Count}";
                    CountGridAnimation(true);
                    SetUserTags(Media);
                    try
                    {
                        for (int i = 0; i < CarouselDotsGrid.Children.Count; i++)
                        {
                            var ellipse = CarouselDotsGrid.Children[i] as Ellipse;
                            if (i == FlipView.SelectedIndex)
                            {
                                ellipse.Fill = "#FFCBCBCB".GetColorBrush();// bozorg
                                ellipse.Height = ellipse.Width = 8;
                            }
                            else
                            {
                                ellipse.Fill = "#FF4C4A48".GetColorBrush();// kochik
                                ellipse.Height = ellipse.Width = 6;
                            }
                        }
                    }
                    catch { }
                    if (LatestMediaElement != null)
                        LatestMediaElement.Pause();

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await Task.Delay(3500);
                        CountGridAnimation(false);
                    });
                    var fp = FlipView.ItemsPanelRoot.Children[FlipView.SelectedIndex] as FlipViewItem;
                    if (fp != null)
                    {
                        var rootGrid = fp.ContentTemplateRoot as Grid;
                        if (rootGrid != null)
                        {
                            MediaElement me = rootGrid.Children[0] as MediaElement;
                            if (me != null)
                            {
                                LatestMediaElement = me;
                                if (PlayNextFromMediaEnd)
                                    me.Play();
                            }
                        }

                    }

                    PlayNextFromMediaEnd = false;
                }
            }
            catch { }
        }

        private void CountGridAnimation(bool show)
        {
            var scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(1f, show ? 1f : 0);
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(750);

            _countGridVisual.CenterPoint = new Vector3((float)CountGrid.ActualWidth / 2f, (float)CountGrid.ActualHeight / 2f, 0f);
            _countGridVisual.StartAnimation("Scale.X", scaleAnimation);
            _countGridVisual.StartAnimation("Scale.Y", scaleAnimation);
        }

        private async void Comment4ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var comment = await Helper.InstaApi.CommentProcessor.CommentMediaAsync(Media.InstaIdentifier, CommentText.Text.Trim());
                    if (comment.Succeeded)
                    {
                        CommentText.Text = string.Empty;
                        Helper.ShowNotify("Your comment sent successfully.");
                    }
                    else
                    {
                        if (comment.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                            Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                        else
                            Helper.ShowNotify("Something wen't wrong:\r\nError message: " + comment.Info.Message);
                    }
                });
            }
            catch { }
        }

        private async void ShareButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await new UsersDialog(Media, Media.MediaType == InstaMediaType.Carousel ? Media.Carousel[FlipView.SelectedIndex].InstaIdentifier : null).ShowAsync();
            }
            catch { }
        }

        private async void CollectionButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (Media.HasViewerSaved)
                    {
                        var comment = await Helper.InstaApi.MediaProcessor.UnSaveMediaAsync(Media.InstaIdentifier);
                        if (comment.Succeeded)
                        {
                            CommentText.Text = string.Empty;
                            Media.HasViewerSaved = false;
                        }
                        else
                        {
                            if (comment.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                            else
                                Helper.ShowNotify("Something wen't wrong:\r\nError message: " + comment.Info.Message);
                        }

                    }
                    else
                    {
                        var comment = await Helper.InstaApi.MediaProcessor.SaveMediaAsync(Media.InstaIdentifier);
                        if (comment.Succeeded)
                        {
                            CommentText.Text = string.Empty;
                            Helper.ShowNotify("Saved to collection.");
                            Media.HasViewerSaved = true;
                        }
                        else
                        {
                            if (comment.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                            else
                                Helper.ShowNotify("Something wen't wrong:\r\nError message: " + comment.Info.Message);
                        }
                    }

                });

            }
            catch { }
        }

        private void LikersClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(typeof(Views.Main.LikersView), Media);
            }
            catch { }
        }
        private void CommentsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(typeof(Views.Posts.CommentView), Media);
            }
            catch { }
        }
        private void MediaElementTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is MediaElement me && me != null)
                {
                    //if (me.CurrentState == MediaElementState.Playing)
                    //{
                    //    if (me.IsMuted)
                    //        me.IsMuted = false;
                    //    else
                    //        me.IsMuted = true;
                    //}
                }
            }
            catch { }
        }

        private async void OnItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (Media.HasLiked)
                    {
                        //Helper.SetAnimation(await AssetHelper.GetAsync(LottieTypes.Dislike));
                        //Media.HasLiked = false;
                        //Media.LikesCount--;
                        //SetLike(false);
                        //var unlike = await Helper.InstaApi.MediaProcessor.UnLikeMediaAsync(Media.InstaIdentifier);
                        //if (!unlike.Succeeded)
                        //{
                        //    if (like.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                        //        Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                        //    Media.LikesCount++;
                        //    Media.HasLiked = true;
                        //    SetLike(true);
                        //}
                    }
                    else
                    {
                        Helper.SetAnimation(await AssetHelper.GetAsync(LottieTypes.Heart));
                        Media.HasLiked = true;
                        Media.LikesCount++;
                        SetLike(true);
                        var like = await Helper.InstaApi.MediaProcessor.LikeMediaAsync(Media.InstaIdentifier);
                        if (!like.Succeeded)
                        {
                            if (like.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                            Media.HasLiked = false;
                            Media.LikesCount--;
                            SetLike(false);
                        }
                    }
                });
            }
            catch { }
        }

        private async void FollowUnfollowButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (FollowUnfollowButton.Content.ToString() == "Follow")
                    {
                        var result = await Helper.InstaApi.UserProcessor.FollowUserAsync(Media.User.Pk);
                        if (result.Succeeded)
                            Media.User.FriendshipStatus.Following = result.Value.Following;
                        else
                        {
                            if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                            else
                                Helper.ShowNotify("Something wen't wrong:\r\nError message: " + result.Info.Message);
                        }
                    }
                    else if (FollowUnfollowButton.Content.ToString() == "Unfollow")
                    {
                        var result = await Helper.InstaApi.UserProcessor.UnFollowUserAsync(Media.User.Pk);
                        if (result.Succeeded)
                            Media.User.FriendshipStatus.Following = result.Value.Following;
                        else
                        {
                            if (result.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.Spam)
                                Helper.ShowNotify(ErrorMessages.FeedbackRequiredMessage, 2000);
                            else
                                Helper.ShowNotify("Something wen't wrong:\r\nError message: " + result.Info.Message);
                        }
                    }
                    //btn.DataContext = user;
                });
            }
            catch { }
        }


        private void UserButtonPreviewComment1Click(object sender, RoutedEventArgs e)
        {
            if (Media == null)
                return;
            if (Media.PreviewComments?[0] != null)
            {
                //try
                //{
                //    if (NavigationService.Frame.Content is Views.Main.MainView view && view != null)
                //        view.MediaMainUc = this;
                //}
                //catch { }
                //try
                //{
                //    var connectedAnimationService = ConnectedAnimationService.GetForCurrentView();
                //    connectedAnimationService.DefaultDuration = TimeSpan.FromMilliseconds(850);

                //    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("UserImage", UserImage);
                //}
                //catch { }
                Helper.OpenProfile(Media.PreviewComments[0].User);
            }
        }
        private void UserButtonPreviewComment2Click(object sender, RoutedEventArgs e)
        {
            if (Media == null)
                return;
            if (Media.PreviewComments?[1] != null)
            {
                Helper.OpenProfile(Media.PreviewComments[1].User);
            }
        }

        private void UserImageEllipseTapped(object sender, TappedRoutedEventArgs e)
        {
            if (Media == null)
                return;
            if (Media.FollowHashtagInfo == null)
                NavigationService.Navigate(typeof(Views.Main.StoryView), Media.User.ToUserShort());
            else
                NavigationService.Navigate(typeof(Views.Infos.HashtagView), Media.FollowHashtagInfo.Name);
        }
        private async void ViewInsightsButtonClick(object sender, RoutedEventArgs e)
        {
            await new MediaInsightDialog(Media).ShowAsync();
        }
    }
}

#pragma warning restore CS0628