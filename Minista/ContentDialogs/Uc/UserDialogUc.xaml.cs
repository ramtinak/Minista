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


namespace Minista.ContentDialogs.Uc
{
    public sealed partial class UserDialogUc : UserControl, INotifyPropertyChanged
    {
        private string _username, _fullname;
        public string Title { get { return _username; } set { _username = value; OnPropertyChanged("Title"); } }
        public string FullName { get { return _fullname; } set { _fullname = value; OnPropertyChanged("FullName"); } }
        public InstaDirectInboxThread Thread
        {
            get { return (InstaDirectInboxThread)GetValue(ThreadProperty); }
            set
            {
                SetValue(ThreadProperty, value);
                SetThread(value);
            }
        }
        public static readonly DependencyProperty ThreadProperty =
            DependencyProperty.Register("Thread",
                typeof(InstaDirectInboxThread),
                typeof(UserDialogUc),
                new PropertyMetadata(null));
        public UserDialogUc()
        {
            this.InitializeComponent();
            DataContext = this;
        }
        void SetThread(InstaDirectInboxThread thread)
        {
            try
            {
                if (thread != null)
                {
                    UsersGrid.Children.Clear();
                    AddUsers(thread.Users);
                    if (thread.ThreadId.Contains("FAKETHREAD"))
                    {
                        Title = thread.Users[0].UserName;
                        FullName = thread.Users[0].FullName;
                    }
                    else
                    {
                        Title = thread.Title;

                    }
                    if (thread.Users == null || thread.Users?.Count == 0)
                    {
                        if (thread.Inviter != null)
                        {
                            
                            thread.Users.Add(thread.Inviter.ToUserShortFriendship());
                            if (string.IsNullOrEmpty(thread.Title))
                                thread.Title = thread.Inviter.UserName.ToLower();
                        }
                    }
                }
            }
            catch { }

            UsersGrid.UpdateLayout();

        }
        public void AddUsers(List<InstaUserShortFriendship> users)
        {
            try
            {
                if (users.Count == 1)
                {
                    var grid = new Grid();
                    grid.Height = grid.Width = 58;
                    var elp = GetEllipse();
                    elp.Fill = users.FirstOrDefault().ProfilePicture.GetImageBrush();
                    elp.Height = elp.Width = 58;
                    grid.Children.Add(elp);
                    UsersGrid.Children.Add(grid);
                }
                else
                {
                    Random rnd = new Random();
                    Thickness LastThickness = new Thickness(0, 0, 0, 0);
                    for (int i = 0; i < 2; i++)
                    {
                        var grid = new Grid();
                        grid.Height = grid.Width = 55;
                        var elp = GetEllipse();
                        elp.Fill = users[i].ProfilePicture.GetImageBrush();
                        elp.Height = elp.Width = 55;
                        grid.Margin = LastThickness;
                        grid.Children.Add(elp);
                        UsersGrid.Children.Add(grid);
                        var thick = CopyThickness(LastThickness);
                        thick.Right -= 2;
                        thick.Bottom -= 3;
                        thick.Left += rnd.Next(8, 10);
                        thick.Top += rnd.Next(7, 9);
                        LastThickness = thick;
                    }
                }
            }
            catch { }
        }
        public void UpdateStrokes()
        {
            try
            {
                var col = Thread.Selected.Value ? "#FF13d2ef".GetColorBrush() : new SolidColorBrush(Colors.Transparent);
                for (int i = 0; i < UsersGrid.Children.Count; i++)
                {
                    var type = UsersGrid.Children[i].GetType();
                    var grid = UsersGrid.Children[i] as Grid;
                    if (grid != null)
                    {
                        var elp = grid.Children[0] as Ellipse;
                        if (elp != null)
                            elp.Stroke = col;
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
        private void OnPropertyChanged(string memberName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }
    }
}
