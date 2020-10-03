using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Minista.ContentDialogs
{
    public sealed partial class MediaUserTagDialog : ContentPopup
    {
        readonly ObservableCollection<InstaUserShort> Users = new ObservableCollection<InstaUserShort>();
        readonly InstaMediaType MediaType;
        MediaUserTagDialog()
        {
            this.InitializeComponent();
        }

        public MediaUserTagDialog(List<InstaUserTag> userTags, InstaMediaType mediaType) : base()
        {
            this.InitializeComponent();
            Loaded += MediaUserTagDialogLoaded;
            if (userTags?.Count > 0)
            {
                MediaType = mediaType;
                for (int i = 0; i < userTags.Count; i++)
                    Users.Add(userTags[i].User);
            }
            else
                Hide();
        }

        private void MediaUserTagDialogLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var str = "In this ";
                if (MediaType == InstaMediaType.Image)
                    str += "Photo";
                else
                    str += "Video";
                txtTitle.Text = str;
                LVUsers.ItemsSource = Users;
            }
            catch { }
        }

        private void LVUsersItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is InstaUserShort userShort && userShort != null)
                {
                    Hide();
                    Helper.OpenProfile(userShort);
                } 
            }
            catch { }
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e) => Hide();
    }
}
