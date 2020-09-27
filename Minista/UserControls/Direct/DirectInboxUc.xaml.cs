using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace Minista.UserControls.Direct
{
    public sealed partial class DirectInboxUc : UserControl, INotifyPropertyChanged
    {
        public bool HadNewMessages { get; set; }
        public InstaUserPresence UserPresence { get; private set; }
        public InstaDirectInboxThread Thread
        {
            get { return (InstaDirectInboxThread)GetValue(ThreadProperty); }
            set
            {
                SetValue(ThreadProperty, value);
                DataContext = value;
                //Sets(value);
                OnPropertyChanged("Thread");
            }
        }
        public static readonly DependencyProperty ThreadProperty =
            DependencyProperty.Register("Thread",
                typeof(InstaDirectInboxThread),
                typeof(DirectInboxUc),
                new PropertyMetadata(null));

        public DirectInboxUc()
        {
            InitializeComponent();
            DataContextChanged += DirectInboxUcDataContextChanged;
        }

        private void DirectInboxUcDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue is InstaDirectInboxThread thread)
                Sets(thread);
        }

        void Sets(InstaDirectInboxThread thread)
        {
            try
            {
                if (thread != null)
                {
                    UsersGrid.Children.Clear();
                    AddUsers(thread.Users);
                    if (thread.Users == null || thread.Users?.Count == 0)
                    {
                        if (thread.Inviter != null)
                        {
                            thread.Users.Add(thread.Inviter.ToUserShortFriendship());
                            if (string.IsNullOrEmpty(thread.Title))
                                thread.Title = thread.Inviter.UserName.ToLower();
                        }
                    }
                    if (thread.Items?.LastOrDefault() != null)
                    {
                        UpdateItem(thread.Items.LastOrDefault());
                    }

                }
            }
            catch { }

            UsersGrid.UpdateLayout();

        }

        public void UpdateItem(InstaDirectInboxItem last)
        {
            try
            {
                if (last == null) return;
                var pk = Helper.InstaApi.GetLoggedUser().LoggedInUser.Pk;
                var type = last.ItemType;
                var userId = last.UserId;
                var username = Thread.Users.FirstOrDefault(x => x.Pk == userId)?.UserName;
                var moreThanOnUser = Thread.Users?.Count > 1;

                var unreadMessageMoreThan1 = false;

                var converter = new Converters.DateTimeConverter();
                var date = "";
                if (last != null)
                    date = $" {Helper.MiddleDot} {converter.Convert(last?.TimeStamp)}";
                int ix = 0;
                if (Thread.HasUnreadMessage)
                {
                    ix = 1;
                    try
                    {
                        var user = Thread.Users.FirstOrDefault().UserName;
                        var reversed = Thread.Items;
                        reversed.Reverse();

                        if (Thread.LastSeenAt.LastOrDefault() != null)
                        {
                            var itemId = Thread.LastSeenAt.LastOrDefault().ItemId;
                            var single = reversed.FirstOrDefault(a => a.ItemId == itemId);
                            var index = reversed.IndexOf(single);
                            if (index != -1)
                                ix = Thread.Items.Count - (index + 1);
                            txtFooter.Text = $"{ix} new messages{date}";
                        }
                        //if (thread.LastActivity != thread.LastSeenAt.LastOrDefault().SeenTime)

                    }
                    catch { }

                    unreadMessageMoreThan1 = ix > 1;
                    HadNewMessages = true;
                }
                if (ix > 0)
                    NewMessageEllipse.Visibility = Visibility.Visible;
                else
                    NewMessageEllipse.Visibility = Visibility.Collapsed;

                if (!unreadMessageMoreThan1)
                {
                    if (type == InstaDirectThreadItemType.ActionLog && last.ActionLog != null)
                        txtFooter.Text = last.ActionLog.Description.Truncate(45) + date;
                    else if (type == InstaDirectThreadItemType.AnimatedMedia && last.AnimatedMedia != null)
                    {
                        if (pk == userId)
                            txtFooter.Text = "You sent a gif".Truncate(45) + date;
                        else
                        {
                            if (moreThanOnUser)
                                txtFooter.Text = $"{username} sent you a gif".Truncate(45) + date;
                            else
                                txtFooter.Text = $"Sent you a gif".Truncate(45) + date;
                        }
                    }
                    else if (type == InstaDirectThreadItemType.FelixShare && last.FelixShareMedia != null)
                    {
                        if (pk == userId)
                            txtFooter.Text = "You shared a IGTV post".Truncate(45) + date;
                        else
                        {
                            if (moreThanOnUser)
                                txtFooter.Text = $"{username} shared you a IGTV post".Truncate(45) + date;
                            else
                                txtFooter.Text = "Shared you a IGTV post".Truncate(45) + date;
                        }
                    }
                    else if (type == InstaDirectThreadItemType.Hashtag && last.HashtagMedia != null)
                    {
                        if (pk == userId)
                            txtFooter.Text = "You shared a hashtag".Truncate(45) + date;
                        else
                        {
                            if (moreThanOnUser)
                                txtFooter.Text = $"{username} shared you a hashtag".Truncate(45) + date;
                            else
                                txtFooter.Text = "Shared you a hashtag".Truncate(45) + date;
                        }
                    }
                    else if (type == InstaDirectThreadItemType.Like && last.Text != null)
                    {
                        txtFooter.Text = last.Text.Truncate(45) + date;
                    }
                    else if (type == InstaDirectThreadItemType.Link && last.LinkMedia != null)
                    {
                        if (pk == userId)
                            txtFooter.Text = "You shared a link".Truncate(45) + date;
                        else
                        {
                            if (moreThanOnUser)
                                txtFooter.Text = $"{username} shared you a link".Truncate(45) + date;
                            else
                                txtFooter.Text = "Shared you a link".Truncate(45) + date;
                        }
                    }
                    else if (type == InstaDirectThreadItemType.Location && last.LocationMedia != null)
                    {
                        if (pk == userId)
                            txtFooter.Text = "You shared a location".Truncate(45) + date;
                        else
                        {
                            if (moreThanOnUser)
                                txtFooter.Text = $"{username} shared you a location".Truncate(45) + date;
                            else
                                txtFooter.Text = "Shared you a location".Truncate(45) + date;
                        }
                    }
                    else if (type == InstaDirectThreadItemType.Media && last.Media != null)
                    {
                        if (pk == userId)
                            txtFooter.Text = "You shared a post".Truncate(45) + date;
                        else
                        {
                            if (moreThanOnUser)
                                txtFooter.Text = $"{username} shared you a post".Truncate(45) + date;
                            else
                                txtFooter.Text = "Shared you a post".Truncate(45) + date;
                        }
                    }
                    else if (type == InstaDirectThreadItemType.MediaShare && last.MediaShare != null)
                    {
                        if (pk == userId)
                            txtFooter.Text = "You shared a post".Truncate(45) + date;
                        else
                        {
                            if (moreThanOnUser)
                                txtFooter.Text = $"{username} shared you a post".Truncate(45) + date;
                            else
                                txtFooter.Text = "Shared you a post".Truncate(45) + date;
                        }
                    }
                    else if (type == InstaDirectThreadItemType.Placeholder && last.Placeholder != null)
                    {
                        txtFooter.Text = last.Placeholder.Message.Truncate(45) + date;
                    }
                    else if (type == InstaDirectThreadItemType.Profile && last.ProfileMedia != null)
                    {
                        if (pk == userId)
                            txtFooter.Text = "You shared a profile".Truncate(45) + date;
                        else
                        {
                            if (moreThanOnUser)
                                txtFooter.Text = $"{username} shared you a profile".Truncate(45) + date;
                            else
                                txtFooter.Text = "Shared you a profile".Truncate(45) + date;
                        }
                    }
                    else if (type == InstaDirectThreadItemType.RavenMedia/* && last.RavenMedia != null*/)
                    {
                        if (pk == userId)
                            txtFooter.Text = "You shared a disappearing media".Truncate(45) + date;
                        else
                        {
                            if (moreThanOnUser)
                                txtFooter.Text = $"{username} shared you a disappearing media".Truncate(45) + date;
                            else
                                txtFooter.Text = "Shared you a disappearing media".Truncate(45) + date;
                        }
                    }
                    else if (type == InstaDirectThreadItemType.ReelShare && last.ReelShareMedia != null)
                    {
                        if (pk == userId)
                            txtFooter.Text = "You shared a story".Truncate(45) + date;
                        else
                        {
                            if (moreThanOnUser)
                                txtFooter.Text = $"{username} shared you a story".Truncate(45) + date;
                            else
                                txtFooter.Text = "Shared you a story".Truncate(45) + date;
                        }
                    }
                    else if (type == InstaDirectThreadItemType.StoryShare && last.StoryShare != null)
                    {
                        if (pk == userId)
                            txtFooter.Text = "You shared a story".Truncate(45) + date;
                        else
                        {
                            if (moreThanOnUser)
                                txtFooter.Text = $"{username} shared you a story".Truncate(45) + date;
                            else
                                txtFooter.Text = "Shared you a story".Truncate(45) + date;
                        }
                    }
                    else if (type == InstaDirectThreadItemType.Text && last.Text != null)
                    {
                        txtFooter.Text = last.Text.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Truncate(45) + date;
                    }
                    else if (type == InstaDirectThreadItemType.VideoCallEvent && last.VideoCallEvent != null)
                    {
                        txtFooter.Text = last.VideoCallEvent.Description.Truncate(45) + date;
                    }
                    else if (type == InstaDirectThreadItemType.VoiceMedia && last.VoiceMedia != null)
                    {
                        if (pk == userId)
                            txtFooter.Text = "You shared a voice".Truncate(45) + date;
                        else
                        {
                            if (moreThanOnUser)
                                txtFooter.Text = $"{username} shared you a voice".Truncate(45) + date;
                            else
                                txtFooter.Text = "Shared you a voice".Truncate(45) + date;
                        }
                    }
                    else if (type == InstaDirectThreadItemType.LiveViewerInvite && last.LiveViewerInvite != null)
                    {
                        txtFooter.Text = last.LiveViewerInvite.Text;
                    }
                    else
                        txtFooter.Text = date;
                }
            }
            catch { }
        }

        public void SetUserPresence(InstaUserPresence userPresence)
        {
            UserPresence = userPresence;
            if (userPresence == null)
                UserPresenceStatusGrid.Visibility = OnlineStatusEllipse.Visibility = Visibility.Collapsed;
            else
            {
                //if (userPresence.IsActive)
                //{
                //    OnlineStatusEllipse.Visibility = Visibility.Visible;
                //    UserPresenceStatusGrid.Visibility = Visibility.Collapsed;
                //}
                //else
                {
                    var span = DateTime.UtcNow - userPresence.LastActivity;
                    if (span.Hours == 0 && span.Minutes < 4)
                    {
                        OnlineStatusEllipse.Visibility = Visibility.Visible;
                        UserPresenceStatusGrid.Visibility = Visibility.Collapsed;
                    }
                    else if (span.Hours == 0 && span.Minutes > 3)
                    {
                        UserPresenceStatusText.Text = $"{span.Minutes}m";

                        OnlineStatusEllipse.Visibility = Visibility.Collapsed;
                        UserPresenceStatusGrid.Visibility = Visibility.Visible;
                    }
                    else
                        UserPresenceStatusGrid.Visibility = OnlineStatusEllipse.Visibility = Visibility.Collapsed;
                }
            }
        }



        
        public void AddUsers(List<InstaUserShortFriendship> users)
        {
            try
            {
                if (users.Count == 1)
                {
                    var elp = GetEllipse();
                    elp.Fill = users.FirstOrDefault().ProfilePicture.GetImageBrush();
                    elp.Height = elp.Width = 62;
                    UsersGrid.Children.Add(elp);
                }
                else
                {
                    //UsersGrid.Margin = new Thickness(-35, -15, 0, 0);
                    Random rnd = new Random();
                    if (users.Count > 3)
                    {
                        Thickness LastThickness = new Thickness(0, 0, 18, 18);
                        for (int i = 0; i < 3; i++)
                        {
                            var elp = GetEllipse();
                            elp.Fill = users[i].ProfilePicture.GetImageBrush();
                            elp.Height = elp.Width = 58;
                            elp.Margin = LastThickness;
                            UsersGrid.Children.Add(elp);
                            var thick = CopyThickness(LastThickness);
                            //thick.Right -= rnd.Next(5, 9);
                            //thick.Bottom -= rnd.Next(5, 9);
                            //thick.Left += rnd.Next(6, 9);
                            //thick.Top += rnd.Next(4, 7);

                            thick.Right -= 6;
                            thick.Bottom -= 7;
                            thick.Left += rnd.Next(9, 12);
                            thick.Top += rnd.Next(8, 10);
                            LastThickness = thick;
                        }
                    }
                    else
                    {
                        Thickness LastThickness = new Thickness(0, 0, 18, 18);
                        for (int i = 0; i < users.Count; i++)
                        {
                            var elp = GetEllipse();
                            elp.Fill = users[i].ProfilePicture.GetImageBrush();
                            elp.Height = elp.Width = 56;
                            elp.Margin = LastThickness;
                            UsersGrid.Children.Add(elp);
                            var thick = CopyThickness(LastThickness);
                            //thick.Right -= rnd.Next(5, 9);
                            //thick.Bottom -= rnd.Next(5, 9);
                            //thick.Left += rnd.Next(6, 9);
                            //thick.Top += rnd.Next(4, 7);
                            thick.Right -= 6;
                            thick.Bottom -= 7;
                            thick.Left += rnd.Next(9, 12);
                            thick.Top += rnd.Next(8, 10);
                            LastThickness = thick;
                        }
                    }
                }
            }
            catch { }
        }
        Thickness CopyThickness(Thickness thickness)
        {
            return new Thickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
        }
        Ellipse GetEllipse()
        {
            return new Ellipse
            {
                StrokeThickness = 2,
                Stroke = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string memberName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            "Grid_Holding".PrintDebug();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            "Grid_RightTapped".PrintDebug();
        }

        private void DeleteFlyoutClick(object sender, RoutedEventArgs e)
        {
             
        }
        private void CopyUsernameFlyoutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Copy("@"+Thread.Users.FirstOrDefault().UserName);
            }
            catch { }
        }
        void Copy(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text.CopyText();
                Helper.ShowNotify("Username copied ;-)");
            }
        }
    }
}
