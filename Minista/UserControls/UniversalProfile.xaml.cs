using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Minista.UserControls
{
    public sealed partial class UniversalProfile : UserControl
    {
        // InstaUser
        // InstaUserShort
        // InstaUserShortFriendshipFull
        // InstaUserShortFriendship
        // InstaBlockedUserInfo
        // InstaUserInfo
        // 
        // 

        public InstaUserShort User
        {
            get => (InstaUserShort)GetValue(UserProperty);
            set
            {
                SetValue(UserProperty, value);
            }
        }

        public UserType UserType
        {
            get => (UserType)GetValue(UserTypeProperty);
            set => SetValue(UserTypeProperty, value);
        }

        public InstaMedia Media
        {
            get => (InstaMedia)GetValue(MediaProperty);
            set => SetValue(MediaProperty, value);
        }

        #region Dependency Properties

        public static readonly DependencyProperty UserProperty =
            DependencyProperty.Register("User",
                typeof(InstaUserShort),
                typeof(UniversalProfile),
                new PropertyMetadata(null));
        public static readonly DependencyProperty UserTypeProperty =
            DependencyProperty.Register("UserType",
                typeof(UserType),
                typeof(UniversalProfile),
                new PropertyMetadata(UserType.User));

        public static readonly DependencyProperty MediaProperty =
            DependencyProperty.Register("Media",
                typeof(InstaMedia),
                typeof(UniversalProfile),
                new PropertyMetadata(null));

        #endregion Dependency Properties

        public UniversalProfile()
        {
            this.InitializeComponent();
        }
    }
    public enum UserType
    {
        User,
        UserShort,
        UserShortFriendshipFull,
        UserShortFriendship,
        BlockedUserInfo,
        UserInfo,
    }
}
