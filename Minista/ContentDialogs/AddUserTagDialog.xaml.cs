using InstagramApiSharp;
using InstagramApiSharp.Classes.Models;
using Minista.UserControls.Main;
using Minista.Views.Uploads;
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
    public sealed partial class AddUserTagDialog : ContentPopup
    {
        public ObservableCollection<InstaUserShort> ItemsSearch { get; set; } = new ObservableCollection<InstaUserShort>();
        MediaTagUc MediaTagUc;
        public AddUserTagDialog(MediaTagUc uc)
        {
            MediaTagUc = uc;
            this.InitializeComponent();
            Loaded += AddUserTagDialogLoaded;
        }

        private void AddUserTagDialogLoaded(object sender, RoutedEventArgs e)
        {
            LVUsers.ItemsSource = ItemsSearch;
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
                }
                else
                {
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
                    if (UserSearchText.Text.Contains('#'))
                        UserSearchText.Text = UserSearchText.Text.Remove('#');

                    var searches = await Helper.InstaApi.DiscoverProcessor.SearchPeopleAsync(UserSearchText.Text.ToLower(), PaginationParameters.MaxPagesToLoad(1), 50); ;
                    if (searches.Succeeded)
                    {
                        ItemsSearch.Clear();
                        var list = new List<InstaUserShort>();
                        if (searches.Value.Users?.Count > 0)
                        {
                            for (int i = 0; i < searches.Value.Users?.Count; i++)
                                list.Add(searches.Value.Users[i].ToUserShort());
                            ItemsSearch.AddRange(list);
                        }
                    }
                });
            }
            catch { }
        }

        private void LVUsersItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is InstaUserShort userShort)
                {
                    var userTag = MediaTagUc.UserTagUpload;
                    userTag.Pk = userShort.Pk;
                    userTag.Username = userShort.UserName;
                    MediaTagUc.SetUserTag(userTag, MediaTagUc.IsVideo);

                    Hide();
                }
            }
            catch { }
        }
    }
}
