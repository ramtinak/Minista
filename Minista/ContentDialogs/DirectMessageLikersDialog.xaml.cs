using System;
using System.Collections.Generic;
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

using InstagramApiSharp.Classes.Models;
namespace Minista.ContentDialogs
{
    public sealed partial class DirectMessageLikersDialog : ContentPopup
    {
        private readonly List<InstaUserShort> Users;
        public DirectMessageLikersDialog(List<InstaUserShort> usrrs)
        {
            InitializeComponent();
            Users = usrrs;
            Loaded += DirectMessageLikersDialogLoaded;
        }

        private void DirectMessageLikersDialogLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LVMenu.ItemsSource = Users;
            }
            catch { }
        }

        private void DoneToggleButtonClick(object sender, RoutedEventArgs e) => Hide();

        private void LVMenuItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is InstaUserShort user && user != null)
                    Helper.OpenProfile(user);
            }
            catch { }
        }
    }
}
