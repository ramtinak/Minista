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
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

using Minista.Enums;
using InstagramApiSharp.Enums;
using InstagramApiSharp.Helpers;
using System.Threading.Tasks;
using Minista.ContentDialogs.Uc;

namespace Minista.ContentDialogs
{
    public sealed partial class UsersDialog : ContentPopup, INotifyPropertyChanged
    {
        #region Properties       
        readonly Compositor _compositor;
        readonly ImplicitAnimationCollection _elementImplicitAnimation;
        //Visibility _buttonVisibility = Visibility.Collapsed;
        //public Visibility ButtonVisibility { get { return _buttonVisibility; } set { _buttonVisibility = value; OnPropertyChangedX("ButtonVisibility"); } }
        readonly UsersDialogCommand UsersDialogCommand;
        public InstaStoryItem StoryItem;
        public InstaReelFeed ReelFeed;
        public InstaMedia Media;
        public InstaUserShort UserProfile;
        public ObservableCollection<UserDialogUc> Items { get; set; } = new ObservableCollection<UserDialogUc>();
        public ObservableCollection<UserDialogUc> ItemsSearch { get; set; } = new ObservableCollection<UserDialogUc>();
        public ObservableCollection<InstaDirectInboxThread> ItemsSenders { get; set; } = new ObservableCollection<InstaDirectInboxThread>();
        private bool _isPrivate = false;
        public bool IsPrivate { get { return _isPrivate; }set { _isPrivate = value; OnPropertyChangedX("IsPrivate"); } }

        private readonly string CarouselChildMediaId;

        #endregion Properties

        public UsersDialog(InstaMedia media, string carouselChildMediaId) : this()
        {
            CarouselChildMediaId = carouselChildMediaId;
            Media = media;
            UsersDialogCommand = UsersDialogCommand.Media;
            try
            {
                IsPrivate = media.User.IsPrivate;
            }
            catch { }
            try
            {
                if (IsPrivate)
                {
                    txtPrivateAccount.Text = $"Only {media.User.UserName.ToLower()}'s followers can see this post.";
                    PrivateAccountGrid.Visibility = Visibility.Visible;
                }
                else
                    PrivateAccountGrid.Visibility = Visibility.Collapsed;
            }
            catch { }
        }
        public UsersDialog(InstaUserShort userShort) : this()
        {
            UserProfile = userShort;
            UsersDialogCommand = UsersDialogCommand.Profile;
        }
        public UsersDialog(InstaStoryItem storyItem, InstaReelFeed reelFeed) : this()
        {
            StoryItem = storyItem;
            ReelFeed = reelFeed;
            UsersDialogCommand = UsersDialogCommand.Story;
            try
            {
                IsPrivate = reelFeed.User.IsPrivate;
            }
            catch { }
            try
            {
                if (IsPrivate)
                {
                    txtPrivateAccount.Text = $"Only {reelFeed.User.UserName.ToLower()}'s followers can see this story.";
                    PrivateAccountGrid.Visibility = Visibility.Visible;
                }
                else
                    PrivateAccountGrid.Visibility = Visibility.Collapsed;
            }
            catch { }
        }
        private UsersDialog()
        {
            this.InitializeComponent();
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Create ImplicitAnimations Collection. 
            _elementImplicitAnimation = _compositor.CreateImplicitAnimationCollection();

            //Define trigger and animation that should play when the trigger is triggered. 
            _elementImplicitAnimation["Offset"] = CreateOffsetAnimation();
            DataContext = this;

            Loaded += UsersDialogLoaded;
        }
        private async Task LoadInfo()
        {
            try
            {
                if (UserHelper.BanyanSuggestions == null || UserHelper.BanyanSuggestions?.Items == null || UserHelper.BanyanSuggestions?.Items?.Count == 0)
                {
                    ShowIndicator();
                    await UserHelper.GetBanyanAsync();
                    await Task.Delay(2000);
                }
                foreach (var item in UserHelper.BanyanSuggestions.Items)
                    Items.Add(GetUserDialogUc(item)/*.CopyUserShort()*/);
            }
            catch { }
            HideIndicator();
        }
        private async void UsersDialogLoaded(object sender, RoutedEventArgs e)
        {
            await LoadInfo();
            LV.ItemsSource = Items;
            LVSearch.ItemsSource = ItemsSearch;
            //LVUsers.ItemsSource = Items;
            try
            {
                UserImage.Fill = Helper.InstaApi.GetLoggedUser().LoggedInUser.ProfilePicture.GetImageBrush();
            }
            catch { }
        }
        void ShowIndicator()
        {
            Busy1.Visibility = Busy2.Visibility = Visibility.Visible;
            Busy1.IsActive = Busy2.IsActive = true;
        }
        void HideIndicator()
        {
            Busy1.Visibility = Busy2.Visibility = Visibility.Collapsed;
            Busy1.IsActive = Busy2.IsActive = false;
        }

        private async void SendButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //var userIds = new List<long>();
                //foreach (var user in ItemsSenders)
                //    userIds.Add(user.Pk);

                Random rnd = new Random();

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    if (UsersDialogCommand == UsersDialogCommand.Media)
                    {
                        var type = Media.MediaType;
                        if (type == InstaMediaType.Carousel)
                            type = Media.Carousel.FirstOrDefault().MediaType;
                        if (Media.ProductType?.ToLower() == "igtv")
                        {
                            foreach (var item in ItemsSenders)
                            {
                                await SendDirectFelixShare(item);
                                await Task.Delay(rnd.Next(750, 1500));
                            }
                            //var userIdsStr = new List<string>();
                            //foreach (var user in ItemsSenders)
                            //    userIdsStr.Add(user.Pk.ToString());
                            //var shareToUsers = await Helper.InstaApi.MessagingProcessor.SendDirectFelixShareAsync(Media.InstaIdentifier,  MessageText.Text, null, userIdsStr.ToArray());

                            //if (shareToUsers.Succeeded)
                            Helper.ShowNotify($"Forwarded to {ItemsSenders.Count} people{(ItemsSenders.Count == 1 ? "" : "s")}");
                        }
                        else
                        {
                            foreach (var item in ItemsSenders)
                            {
                                await ShareMediaToUser(item, type);
                                await Task.Delay(rnd.Next(750, 1500));
                            }
                            //var shareToUsers = await Helper.InstaApi.MessagingProcessor.ShareMediaToUserAsync(Media.InstaIdentifier, type,
                            //    MessageText.Text, userIds.ToArray());

                            //if (shareToUsers.Succeeded)
                                Helper.ShowNotify($"Forwarded to {ItemsSenders.Count} people{(ItemsSenders.Count == 1 ? "" : "s")}");
                        }
                    }
                    else if (UsersDialogCommand == UsersDialogCommand.Story)
                    {
                        var type = InstaSharingType.Photo;
                        if (StoryItem.MediaType == InstaMediaType.Video)
                            type = InstaSharingType.Video;

                        //var shareToUsers = await Helper.InstaApi.StoryProcessor.ShareStoryAsync(ReelFeed.Id, StoryItem.Id, null, userIds.ToArray(),
                        //    MessageText.Text, type);

                        //if (shareToUsers.Succeeded)
                        foreach (var item in ItemsSenders)
                        {
                            await ShareStory(item, type);
                            await Task.Delay(rnd.Next(750, 1500));
                        }
                        Helper.ShowNotify($"Forwarded to {ItemsSenders.Count} people{(ItemsSenders.Count == 1 ? "" : "s")}");
                    }
                    else if (UsersDialogCommand == UsersDialogCommand.Profile)
                    {
                        foreach (var item in ItemsSenders)
                        {
                            await SendDirectProfileToRecipients(item);
                            await Task.Delay(rnd.Next(750, 1500));
                        }
                        //var shareProfile = await Helper.InstaApi.MessagingProcessor
                        //.SendDirectProfileToRecipientsAsync(UserProfile.Pk, string.Join(",", userIds));
                        //if (!string.IsNullOrEmpty(MessageText.Text))
                        //    await Helper.InstaApi.MessagingProcessor.SendDirectTextAsync(string.Join(",", userIds), null, MessageText.Text);

                        //if (shareProfile.Succeeded)
                        Helper.ShowNotify($"Profile shared to {ItemsSenders.Count} people{(ItemsSenders.Count == 1 ? "" : "s")}");
                    }
                });
            }
            catch { }
            Hide();
        }

        async Task SendDirectFelixShare(InstaDirectInboxThread thread)
        { 
            try
            {
                if (thread.Title.Contains("FAKETHREAD"))
                {
                    var userIds = new List<string>();

                    foreach (var user in thread.Users)
                        userIds.Add(user.Pk.ToString());
                    var felixShare = await Helper.InstaApi.MessagingProcessor
                        .SendDirectFelixShareAsync(Media.InstaIdentifier, MessageText.Text, null, userIds.ToArray());
                }
                else
                {
                    var felixShare = await Helper.InstaApi.MessagingProcessor
                        .SendDirectFelixShareAsync(Media.InstaIdentifier, MessageText.Text, new string[] { thread.ThreadId }, null);
                }
            }
            catch{ }
        }
        async Task ShareMediaToUser(InstaDirectInboxThread thread, InstaMediaType type)
        {
            try
            {
                if (thread.Title.Contains("FAKETHREAD"))
                {
                    var userIds = new List<long>();

                    foreach (var user in thread.Users)
                        userIds.Add(user.Pk);

                    var shareToUsers = await Helper.InstaApi.MessagingProcessor.ShareMediaToUserAsync(Media.InstaIdentifier, type,
                       MessageText.Text, CarouselChildMediaId, userIds.ToArray());
                }
                else
                {
                    var shareToUsers = await Helper.InstaApi.MessagingProcessor.ShareMediaToThreadAsync(Media.InstaIdentifier, type,
                        MessageText.Text, CarouselChildMediaId, new string[] { thread.ThreadId });
                }
            }
            catch { }
        }
        async Task ShareStory(InstaDirectInboxThread thread, InstaSharingType type)
        {
            try
            {
                if (thread.Title.Contains("FAKETHREAD"))
                {
                    var userIds = new List<long>();

                    foreach (var user in thread.Users)
                        userIds.Add(user.Pk);

                    var shareStory = await Helper.InstaApi.StoryProcessor.ShareStoryAsync(ReelFeed.Id, StoryItem.Id, null, userIds.ToArray(),
                   MessageText.Text, type);
                }
                else
                {
                    var shareStory = await Helper.InstaApi.StoryProcessor.ShareStoryAsync(ReelFeed.Id, StoryItem.Id, new string[] { thread.ThreadId }, null,
                   MessageText.Text, type);
                }
            }
            catch { }
        }
        async Task SendDirectProfileToRecipients(InstaDirectInboxThread thread)
        {
            try
            {
                if (thread.Title.Contains("FAKETHREAD"))
                {
                    var userIds = new List<long>();

                    foreach (var user in thread.Users)
                        userIds.Add(user.Pk);
                    var shareProfile = await Helper.InstaApi.MessagingProcessor
                     .SendDirectProfileToRecipientsAsync(UserProfile.Pk, string.Join(",", userIds));
                    if (!string.IsNullOrEmpty(MessageText.Text))
                        await Helper.InstaApi.MessagingProcessor.SendDirectTextAsync(string.Join(",", userIds), null, MessageText.Text);
                }
                else
                {
                    var shareProfile = await Helper.InstaApi.MessagingProcessor
                     .SendDirectProfileAsync(UserProfile.Pk, new string[] { thread.ThreadId });
                    if (!string.IsNullOrEmpty(MessageText.Text))
                        await Helper.InstaApi.MessagingProcessor.SendDirectTextAsync(null, thread.ThreadId, MessageText.Text);
                }
            }
            catch { }
        }


        #region Events
        private void LVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is UserDialogUc uc)
                {
                    if (uc.Thread.Selected != null)
                    {
                        if (uc.Thread.Selected.Value)
                        {
                            uc.Thread.Selected = false;

                            uc.Thread.CloseButton = false;
                            try
                            {
                                var exists = ItemsSenders.FirstOrDefault(x => x.ThreadId == uc.Thread.ThreadId);
                                if (exists!=null)
                                    //if (ItemsSenders.Count> 0)
                                ItemsSenders.Remove(exists);
                            }
                            catch { }
                            uc.UpdateStrokes();
                        }
                        else
                        {
                            uc.Thread.Selected = true;
                            try
                            {
                                ItemsSenders.Add(uc.Thread);
                            }
                            catch { }
                            uc.UpdateStrokes();
                        }
                    }
                    else
                    {
                        uc.Thread.Selected = false;
                        uc.UpdateStrokes();
                    }

                    DoVisible();
                }

            }
            catch { }
        }

        void DoVisible()
        {
            try
            {
                if (ItemsSenders.Count > 0)
                    SendButton.IsEnabled = true;  //ButtonVisibility = Visibility.Visible;
                else
                    SendButton.IsEnabled = false; // ButtonVisibility = Visibility.Collapsed;
            }
            catch { }
        }

        private void DeleteButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Ellipse ellipse && ellipse.DataContext is InstaDirectInboxThread thread)
                {
                    // delete
                    //ItemsSenders.Remove(uc);
                    var exists = ItemsSenders.FirstOrDefault(x => x.ThreadId == thread.ThreadId);
                    if (exists != null)
                        //if (ItemsSenders.Count> 0)
                        ItemsSenders.Remove(exists);
                    var item = Items.FirstOrDefault(u => u.Thread.ThreadId == thread.ThreadId);
                    if (item != null && item.Thread.Selected.HasValue && item.Thread.Selected.Value)
                    {
                        item.Thread.Selected = false;
                        item.Thread.CloseButton = false;
                    }
                    else
                    {
                        item = ItemsSearch.FirstOrDefault(u => u.Thread.ThreadId == thread.ThreadId);
                        item.Thread.Selected = false;
                        item.Thread.CloseButton = false;
                    }
                    item.UpdateStrokes();
                    DoVisible();
                }
            }
            catch { }
        }

        private void ItemTapGridPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid grid && grid.DataContext is InstaDirectInboxThread thread)
                    thread.CloseButton = true;
                //if (sender is Grid grid && grid.DataContext is InstaUserShort userShort)
                //    userShort.CloseButton = true;
            }
            catch { }
        }

        private void ItemTapGridPointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid grid && grid.DataContext is InstaDirectInboxThread thread)
                    thread.CloseButton = false;
                //if (sender is Grid grid && grid.DataContext is InstaUserShort userShort)
                //    userShort.CloseButton = false;
            }
            catch { }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Hide();
            }
            catch { }
        }

        private void UserSearchTextKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                    DoSearch();
            }
            catch { }
        }

        private void UserSearchTextTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (UserSearchText.Text.Length <= 2)
                {
                    ItemsSearch.Clear();
                    LV.Visibility = Visibility.Visible;
                    LVSearch.Visibility = Visibility.Collapsed;
                }
                else
                {
                    LV.Visibility = Visibility.Collapsed;
                    LVSearch.Visibility = Visibility.Visible;
                    DoSearch();
                }
            }
            catch { }
        }
        async void DoSearch()
        {
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    var searches = await Helper.InstaApi.MessagingProcessor.GetRankedRecipientsByUsernameAsync(UserSearchText.Text.ToLower());
                    if (searches.Succeeded)
                    {
                        ItemsSearch.Clear();
                        var list = new List<UserDialogUc>();
                        if (searches.Value.Items?.Count > 0)
                        {
                            for (int i = 0; i < searches.Value.Items?.Count; i++)
                                list.Add(GetUserDialogUc(searches.Value.Items[i]));
                            ItemsSearch.AddRange(list);
                        }

                        //if (searches.Value.Threads?.Count > 0)
                        //{
                        //    foreach (var thread in searches.Value.Threads)
                        //    {
                        //        try
                        //        {
                        //            //list.Add(thread.Users.FirstOrDefault());
                        //        }
                        //        catch { }
                        //    }
                        //}
                        //if (searches.Value.Users?.Count > 0)
                        //    list.AddRange(searches.Value.Users);
                    }
                });
            }
            catch { }
        }

        #endregion Events

        readonly Random Rnd = new Random();

        UserDialogUc GetUserDialogUc(InstaDirectInboxThread thread)
        {
            var uc = new UserDialogUc
            {
                Name = (Rnd.Next(11111, 999999) + Rnd.Next(10000, 999999)).ToString(),
                Thread = thread,
                //IsRightTapEnabled = true,
                //IsHoldingEnabled = true
            };
            //var deleteMenuFlyoutItem = new MenuFlyoutItem
            //{
            //    DataContext = thread,
            //    Text = "Delete"
            //};
            //deleteMenuFlyoutItem.Click += DeleteMenuFlyoutItemClick;
            //var mediaFlyout = new MenuFlyout
            //{
            //    Items =
            //    {
            //        deleteMenuFlyoutItem,
            //    }
            //};
            //FlyoutBase.SetAttachedFlyout(uc, mediaFlyout);
            return uc;
        }
        #region Animation
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

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChangedX(string memberName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }

        #endregion PropertyChanged
    }
}
namespace Minista.Converters
{
    class UsersDialogBoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "Send";
            if (value is bool data)
                return data ? "Undo" : "Send";

            return "Send";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    class UsersDialogBoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return new SolidColorBrush(Colors.Transparent);
            if (value is bool data)
                return data ? "#FF13d2ef".GetColorBrush() : new SolidColorBrush(Colors.Transparent);

            return new SolidColorBrush(Colors.Transparent);
            //if (value == null) return "#FF212121".GetColorBrush();
            //if (value is bool data)
            //    return data ? new SolidColorBrush(Colors.Transparent): "#FF212121".GetColorBrush();

            //return "#FF212121".GetColorBrush();
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    class UsersDialogBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is bool data)
                return data ? Visibility.Visible : Visibility.Collapsed;

            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
