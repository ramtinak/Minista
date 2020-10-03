using InstagramApiSharp.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Minista.ContentDialogs
{
    public sealed partial class AddOrChooseUserDialog : ContentPopup
    {
        private readonly ObservableCollection<UserChoose> Users = new ObservableCollection<UserChoose>();
        public AddOrChooseUserDialog()
        {
            InitializeComponent();

            Users.Clear();
            foreach (var item in Helper.InstaApiList)
            {
                if (item.GetLoggedUser().UserName.ToLower() != Helper.InstaApi.GetLoggedUser().UserName.ToLower())
                {
                    var user = new UserChoose
                    {
                        InstaApi = item,
                        Username = item.GetLoggedUser().UserName,
                        ProfilePicture = item.GetLoggedUser().LoggedInUser.ProfilePicture
                    };
                    Users.Add(user);
                }
            }

            UserImage.Fill = Helper.InstaApi.GetLoggedUser().LoggedInUser.ProfilePicture.GetImageBrush();
            txtUsername.Text = Helper.InstaApi.GetLoggedUser().UserName.ToLower();
            LVUsers.ItemsSource = Users;
        }

       

        private void AddUserButtonClick(object sender, RoutedEventArgs e)
        {
            Helpers.NavigationService.Navigate(typeof(Views.Sign.SignInView));
            MainPage.Current?.HideHeaders();
            Hide();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void UserGridTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Grid grid && grid.DataContext is UserChoose user && user != null)
                {
                    Hide();
                    Helper.InstaApi = user.InstaApi;
                    Helper.InstaApiSelectedUsername = user.InstaApi.GetLoggedUser().UserName.ToLower();
                    SettingsHelper.SaveSettings();
                    try
                    {
                        Helpers.NavigationService.HideBackButton();
                    }
                    catch { }
                    Helper.UserChanged = true;

                    var apis = Helper.InstaApiList.ToList();
                    apis.AddInstaApiX(Helper.InstaApi);
                    Helpers.MultiApiHelper.SetupPushNotification(apis);
                    try
                    {
                        MainPage.Current.NavigateToMainView(true);
                    }
                    catch { }
                }
            }
            catch { }
        }
        private async void ExportUserButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is AppBarButton btn && btn.DataContext is UserChoose user && user != null)
                {
                    var savePicker = new FileSavePicker
                    {
                        CommitButtonText = "Export"
                    };
                    savePicker.SuggestedFileName = user.InstaApi.GetLoggedUser().LoggedInUser.UserName.ToLower();
                    savePicker.FileTypeChoices.Add("Minista Session File", new List<string> { Helper.SessionFileType });

                    var file = await savePicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        var state = await user.InstaApi.GetStateDataAsStringAsync();
                        await Task.Delay(350);
                        var content = CryptoHelper.Encrypt(state);

                        await Task.Delay(500);

                        await FileIO.WriteTextAsync(file, content, Windows.Storage.Streams.UnicodeEncoding.Utf8);

                        Helper.ShowNotify("User session details exported in >\r\n" + file.Path);

                    }
                }
                else
                {
                    var savePicker = new FileSavePicker
                    {
                        CommitButtonText = "Export"
                    };
                    savePicker.SuggestedFileName = Helper.InstaApiSelectedUsername.ToLower();
                    savePicker.FileTypeChoices.Add("Minista Session File", new List<string> { Helper.SessionFileType });

                    var file = await savePicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        var state = await Helper.InstaApi.GetStateDataAsStringAsync();
                        await Task.Delay(350);
                        var content = CryptoHelper.Encrypt(state);

                        await Task.Delay(500);

                        await FileIO.WriteTextAsync(file, content, Windows.Storage.Streams.UnicodeEncoding.Utf8);

                        Helper.ShowNotify("User session details exported in >\r\n" + file.Path);

                    }
                }
            }
            catch { }
        }

        class UserChoose
        {
            public string Username { get; set; }
            public string ProfilePicture { get; set; }
            public IInstaApi InstaApi { get; set; }
        }

        private async void LogoutUserButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Hide();
                await new LogoutDialog().ShowAsync();
                //var dialog = new MessageDialog($"Are you sure you want to logout from '{Helper.InstaApi.GetLoggedUser().LoggedInUser.UserName}' account ?");
                //dialog.Commands.Add(new UICommand("No"));
                //dialog.Commands.Add(new UICommand("Yes"));
                //dialog.CancelCommandIndex = 0;
                //dialog.DefaultCommandIndex = 0;
                //var label = await dialog.ShowAsync();
                //if (label.Label == "Yes")
                //    await new LogoutDialog().ShowAsync();
            }
            catch { }
        }
    }
}
