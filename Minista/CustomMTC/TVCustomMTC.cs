using InstagramApiSharp.Classes.Models;
using Minista.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Minista;
using Minista.Helpers;
using Minista.Views.TV;

namespace Minista
{
    public sealed class TVCustomMTC : MediaTransportControls
    {
        InstaMedia Media;
        public DispatcherTimer Timer { get; private set; } = new DispatcherTimer();
        bool CanHideControls = true;
        public bool CanShowControls = true;
        public TVPlayer TVPlayer;
        #region UI
        TextBlock TitleText, UsernameText, DateText;
        Ellipse ProfileEllipse;
        Run ViewCountRun, CommentsCountRun;
        private AppBarButton LikeButton, CommentButton, ShareButton;
        Button UpNextButton;
        bool Liked;


        Storyboard PanelFadeinStory, PanelFadeoutStory;
        Grid RootGrid, UpGrid, BottomGrid;

        Border ControlPanel_ControlPanelVisibilityStates_Border, UserBorder;
        Grid TapGrid;
        #endregion UI

        public TVCustomMTC()
        {
            this.DefaultStyleKey = typeof(TVCustomMTC);
        }

        public void SetMedia(InstaMedia instaMedia)
        {
            Media = instaMedia;
            if (instaMedia == null)
            {
                ResetEverything();
                return;
            }

            var content = string.Empty;
            if (instaMedia.ViewCount > 0)
                content = $"{instaMedia.ViewCount.Divide()}";
            //if (instaMedia.LikesCount > 0)
            //{
            //    var likes = $"{instaMedia.LikesCount.Divide()}";
            //    if (!string.IsNullOrEmpty(content))

            //        content = likes;
            //    else
            //        content += $" | {likes}";
            //}

            if (!string.IsNullOrEmpty(instaMedia.CommentsCount))
            {
                var c = int.Parse(instaMedia.CommentsCount);
                if (c > 0)
                {
                    var comments = $"{c.Divide()}";
                    CommentsCountRun.Text = $"{comments}";
                }
            }

            Liked = instaMedia.HasLiked;
            SetLike(instaMedia.HasLiked);
            ViewCountRun.Text = content;
            DateText.Text = instaMedia.TakenAt.ToString("MMMM dd", CultureInfo.InvariantCulture);
            UsernameText.Text = instaMedia.User.UserName;
            TitleText.Text = instaMedia.Title;
            ProfileEllipse.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri(instaMedia.User.ProfilePicture)) };
            try
            {
                CanShowControls = true;
                PanelFadeinStory.Begin();
            }
            catch { }
        }
        protected override void OnApplyTemplate()
        {
            ControlPanel_ControlPanelVisibilityStates_Border = GetTemplateChild("ControlPanel_ControlPanelVisibilityStates_Border") as Border;
            TapGrid = GetTemplateChild("TapGrid") as Grid;
            RootGrid = GetTemplateChild("RootGrid") as Grid;
            UpGrid = GetTemplateChild("UpGrid") as Grid;
            BottomGrid = GetTemplateChild("BottomGrid") as Grid;
            PanelFadeoutStory = GetTemplateChild("PanelFadeoutStory") as Storyboard;
            PanelFadeinStory = GetTemplateChild("PanelFadeinStory") as Storyboard;


            DateText = GetTemplateChild("DateText") as TextBlock;
            UsernameText = GetTemplateChild("UsernameText") as TextBlock;
            TitleText = GetTemplateChild("TitleText") as TextBlock;
            ProfileEllipse = GetTemplateChild("ProfileEllipse") as Ellipse;

            ViewCountRun = GetTemplateChild("ViewCountRun") as Run;
            CommentsCountRun = GetTemplateChild("CommentsCountRun") as Run;
            LikeButton = GetTemplateChild("LikeButton") as AppBarButton;
            CommentButton = GetTemplateChild("CommentButton") as AppBarButton;
            ShareButton = GetTemplateChild("ShareButton") as AppBarButton;
            UpNextButton = GetTemplateChild("UpNextButton") as Button;
            UserBorder = GetTemplateChild("UserBorder") as Border;


            RootGrid.Tapped += TapBorder_Tapped;
            UpGrid.Tapped += UpGrid_Tapped;
            BottomGrid.Tapped += BottomGrid_Tapped;

            TapGrid.Tapped += TapGrid_Tapped;
            CommentButton.Click += CommentButtonClick;
            ShareButton.Click += ShareButton_Click;
            UpNextButton.Click += UpNextButton_Click;
            UserBorder.Tapped += UserBorder_Tapped;
            base.OnApplyTemplate();
        }

        private void UserBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (Media != null)
                {
                    Helper.OpenProfile(new InstaTVSearchResult { User = Media.User.ToUserShortFriendship() });
                }
            }
            catch { }
        }

        private async void UpNextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TVPlayer.MediaComments.Visibility = Visibility.Collapsed;
                HidePanel();
                CanShowControls = false;
                TVPlayer.FooterView.Visibility = Visibility.Visible;
                await Task.Delay(100);
                TVPlayer.FooterView.LVItems.SelectedIndex = TVPlayer.FooterView.FooterVM.SelectedIndex;

                TVPlayer.FooterView.LVItems.ScrollIntoView(TVPlayer.FooterView.FooterVM.Items[TVPlayer.FooterView.FooterVM.SelectedIndex]);
            }
            catch { }
        }

        private async void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await new ContentDialogs.UsersDialog(Media, null).ShowAsync();
            }
            catch { }
        }

        private void CommentButtonClick(object sender, RoutedEventArgs e)
        {
            if(TVPlayer != null)
            {
                TVPlayer.MediaComments.Visibility = Visibility.Visible;
                HidePanel();
                CanShowControls = false;
                TVPlayer.MediaComments.SetMedia(Media);
            }
        }

        private void TapGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (!CanHideControls) return;
                if ((int)ControlPanel_ControlPanelVisibilityStates_Border.Opacity == 1)
                    PanelFadeoutStory.Begin();
            }
            catch { }
        }

        private async void UpGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
               await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
               {
                   "UpGrid_Tapped".PrintDebug();
                   CanHideControls = false;
                   await Task.Delay(300);
                   CanHideControls = true;

               });
            }
            catch { }
        }

        private async void BottomGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    "BottomGrid_Tapped".PrintDebug();
                    CanHideControls = false;
                    await Task.Delay(300);
                    CanHideControls = true;

                });
            }
            catch { }
        }

        private void TapBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                "TapBorder_Tapped".PrintDebug();

                if (!CanHideControls) return;
                if (!CanShowControls) return;
                if ((int)ControlPanel_ControlPanelVisibilityStates_Border.Opacity != 1)
                    PanelFadeinStory.Begin();
            }
            catch { }
        }

        public void ShowPanel()
        {
            PanelFadeinStory.Begin();
        }
        public void HidePanel()
        {
            PanelFadeoutStory.Begin();
        }

        public void ResetEverything()
        {
            Media = null;
            UsernameText.Text = "";
            DateText.Text = "";
            TitleText.Text = "";
            ViewCountRun.Text = "";
            ViewCountRun.Text = "";
            ProfileEllipse.Fill = null;
            SetLike(false);

        }
        void SetLike(bool hasLiked)
        {
            LikeButton.Content = hasLiked ? "" : "";

            LikeButton.Foreground = hasLiked ? "#FFE03939".GetColorBrush() : new SolidColorBrush(Colors.White);
            Liked = hasLiked;
        }
    }
}
