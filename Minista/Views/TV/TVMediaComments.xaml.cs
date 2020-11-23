using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Minista.Helpers;
using Minista.Views.Infos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Minista.Views.TV
{
    public sealed partial class TVMediaComments : UserControl
    {
        readonly Compositor _compositor;
        readonly ImplicitAnimationCollection _elementImplicitAnimation;

        private readonly Visual _goUpButtonVisual;
        ScrollViewer ScrollView;
        InstaMedia Media;
        public static TVMediaComments Current;
        public TVMediaComments()
        {
            this.InitializeComponent();
            Current = this;
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Create ImplicitAnimations Collection. 
            _elementImplicitAnimation = _compositor.CreateImplicitAnimationCollection();

            //Define trigger and animation that should play when the trigger is triggered. 
            _elementImplicitAnimation["Offset"] = CreateOffsetAnimation();

            _goUpButtonVisual = GoUpButton.GetVisual();
            Loaded += CommentViewLoaded;
        }
        public Action BackAction;
        private void BackButton_Click(object sender, RoutedEventArgs e) => BackAction?.Invoke();
        private void CommentViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshControl.RefreshRequested -= RefreshControlRefreshRequested;
            }
            catch { }
            RefreshControl.RefreshRequested += RefreshControlRefreshRequested;
        }
        private void RefreshControlRefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            using (var RefreshCompletionDeferral = args.GetDeferral())
                CommentsVM.RunLoadMore(true);
        }
        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            RefreshControl.RequestRefresh();
        }
        private void ItemsLV_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                KeyDown -= OnKeyDownHandler;
            }
            catch { }
            KeyDown += OnKeyDownHandler;

        }
        private void OnKeyDownHandler(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.F5)
                    CommentsVM.RunLoadMore(true);
            }
            catch { }
        }
        public void SetMedia(InstaMedia media)
        {
            Media = media;

            if (ScrollView == null)
            {
                //if (NavigationMode == NavigationMode.Back) return;
                ScrollView = ItemsLV.FindScrollViewer();
                if (ScrollView != null) ScrollView.ViewChanging += ScrollViewViewChanging;
                CommentsVM.SetLV(ScrollView);
            }
            CommentsVM.ResetCache();
            ToggleGoUpButtonAnimation(false);
            if (Media != null)
                CommentsVM.SetMedia(Media);
            CommentsVM.RunLoadMore(true);
        }
        public async void SetMedia(string mediaId)
        {
            var media = await Helper.InstaApi.MediaProcessor.GetMediaByIdAsync(mediaId);
            if (media.Succeeded)
            {
                Media = media.Value;
                if (ScrollView == null)
                {
                    //if (NavigationMode == NavigationMode.Back) return;
                    ScrollView = ItemsLV.FindScrollViewer();
                    if (ScrollView != null) ScrollView.ViewChanging += ScrollViewViewChanging;
                    CommentsVM.SetLV(ScrollView);
                }
                CommentsVM.ResetCache();
                ToggleGoUpButtonAnimation(false);
                if (Media != null)
                    CommentsVM.SetMedia(Media);
                CommentsVM.RunLoadMore(true);
            }
        }
        private void MoreTailChildCommentsButtonClick(object sender, RoutedEventArgs e)
        {
            // max id mikhad, yani insert(0, item) bayad bezani
            try
            {
                if (sender is HyperlinkButton hyper && hyper.DataContext is InstaComment comment)
                {
                    CommentsVM.GetTailCommentReplies(comment);
                }
            }
            catch { }
        }
        private void MoreHeadChildCommentsButtonClick(object sender, RoutedEventArgs e)
        {
            // min id mikhad, yani Add() bayad bezani
            try
            {
                if (sender is HyperlinkButton hyper && hyper.DataContext is InstaComment comment)
                {
                    CommentsVM.GetHeadCommentReplies(comment);
                }
            }
            catch { }
        }

        private void UserClick(object sender, RoutedEventArgs e)
        {
            // open user details view | main comment
            try
            {
                if (sender is Button btn && btn.DataContext is InstaComment comment)
                    Helper.OpenProfile(comment.User);
            }
            catch { }
        }

        private void ReplyButtonClick(object sender, RoutedEventArgs e)
        {
            // reply | main comment

            if (sender is HyperlinkButton btn && btn != null)
            {
                if (btn.DataContext is InstaComment data && data != null)
                {
                    CommentsVM.ReplyComment = data;
                    var text = CommentText.Text;
                    CommentText.Text = $"@{data.User.UserName} {text}";
                }
                else if (btn.DataContext is InstaCommentShort data2 && data2 != null)
                {
                    CommentsVM.ReplyComment = data2.ToComment();
                    var text = CommentText.Text;
                    CommentText.Text = $"@{data2.User.UserName} {text}";
                }
            }
        }

        private async void LikeButtonClick(object sender, RoutedEventArgs e)
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


        private void ItemsLVRefreshRequested(object sender, EventArgs e)
        {
            CommentsVM?.Refresh();
        }




        private void CloseCommentButtonClick(object sender, RoutedEventArgs e)
        {
            if (CommentsVM == null) return;
            CommentsVM.ReplyComment = null;
            CommentText.Text = string.Empty;
        }



        private async void CommentButtonClick(object sender, RoutedEventArgs e)
        {
            if (Media == null)
                return;
            if (Helper.InstaApi == null)
                return;
            if (Helper.InstaApi != null && !Helper.InstaApi.IsUserAuthenticated)
                return;
            try
            {
                if (string.IsNullOrEmpty(CommentText.Text))
                {
                    CommentText.Focus(FocusState.Keyboard);
                    return;
                }
                IResult<InstaComment> result;
                if (CommentsVM.ReplyComment == null)
                    result = await Helper.InstaApi
                        .CommentProcessor.CommentMediaAsync(Media.InstaIdentifier, CommentText.Text);
                else
                    result = await Helper.InstaApi.CommentProcessor
                        .ReplyCommentMediaAsync(Media.InstaIdentifier, CommentsVM.ReplyComment.Pk.ToString(), CommentText.Text);

                if (result.Succeeded)
                {
                    Helper.ShowNotify("Comment sent successfully.");
                    CommentText.Text = "";
                    CommentsVM.ReplyComment = null;
                    if (CommentsVM.ReplyComment == null)
                        CommentsVM.Items.Add(result.Value);
                    //ItemsLV.Items.Add(result.Value);
                }
                else
                {
                    switch (result.Info.ResponseType)
                    {
                        case ResponseType.RequestsLimit:
                        case ResponseType.SentryBlock:
                            Helper.ShowNotify(result.Info.Message);
                            break;
                        case ResponseType.ActionBlocked:
                            Helper.ShowNotify("Action blocked.\r\nPlease try again 5 or 10 minutes later");
                            break;
                    }
                }
            }
            catch { }
        }






























        private void LVContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var elementVisual = ElementCompositionPreview.GetElementVisual(args.ItemContainer);
            if (args.InRecycleQueue)
            {
                elementVisual.ImplicitAnimations = null;
            }
            else
            {
                //Add implicit animation to each visual 
                elementVisual.ImplicitAnimations = _elementImplicitAnimation;
            }
        }
        #region Animation

        private CompositionAnimationGroup CreateOffsetAnimation()
        {

            //Define Offset Animation for the ANimation group
            Vector3KeyFrameAnimation offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromSeconds(.4);

            //Define Animation Target for this animation to animate using definition. 
            offsetAnimation.Target = "Offset";


            ScalarKeyFrameAnimation fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1f, 0.8f);
            fadeAnimation.Duration = TimeSpan.FromSeconds(.4);
            fadeAnimation.Target = "Opacity";




            //Define Rotation Animation for Animation Group. 
            ScalarKeyFrameAnimation rotationAnimation = _compositor.CreateScalarKeyFrameAnimation();
            rotationAnimation.InsertKeyFrame(.5f, 0.160f);
            rotationAnimation.InsertKeyFrame(1f, 0f);
            rotationAnimation.Duration = TimeSpan.FromSeconds(.4);

            //Define Animation Target for this animation to animate using definition. 
            rotationAnimation.Target = "RotationAngle";

            //Add Animations to Animation group. 
            CompositionAnimationGroup animationGroup = _compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);
            animationGroup.Add(rotationAnimation);
            animationGroup.Add(fadeAnimation);

            return animationGroup;
        }
        #endregion

        private double _lastVerticalOffset;
        private bool _isHideTitleGrid;
        private void ScrollViewViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            try
            {
                var scrollViewer = sender as ScrollViewer;
                if ((scrollViewer.VerticalOffset - _lastVerticalOffset) > 5 && !_isHideTitleGrid)
                {
                    _isHideTitleGrid = true;
                    ToggleGoUpButtonAnimation(false);
                }
                else if (scrollViewer.VerticalOffset < _lastVerticalOffset && _isHideTitleGrid)
                {
                    _isHideTitleGrid = false;
                    ToggleGoUpButtonAnimation(true);
                }
                if (scrollViewer.VerticalOffset <= 1)
                {
                    _isHideTitleGrid = true;
                    ToggleGoUpButtonAnimation(false);
                }
                _lastVerticalOffset = scrollViewer.VerticalOffset;
            }
            catch { }
        }
        private void ToggleGoUpButtonAnimation(bool show)
        {
            var scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(1f, show ? 1f : 0);
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);

            _goUpButtonVisual.CenterPoint = new Vector3((float)GoUpButton.ActualWidth / 2f, (float)GoUpButton.ActualHeight / 2f, 0f);
            _goUpButtonVisual.StartAnimation("Scale.X", scaleAnimation);
            _goUpButtonVisual.StartAnimation("Scale.Y", scaleAnimation);
        }
        private void GoUpButtonClick(object sender, RoutedEventArgs e)
        {
            ScrollView.ScrollToElement(0);
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

        #region LOADINGS
        public void ShowTopLoading() => TopLoading.Start();
        public void HideTopLoading() => TopLoading.Stop();


        public void ShowBottomLoading() => BottomLoading.Start();
        public void HideBottomLoading() => BottomLoading.Stop();
        #endregion LOADINGS

    }
}
