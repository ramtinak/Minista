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


namespace Minista.ContentDialogs
{
    public sealed partial class LogoutDialog : ContentPopup
    {
        public LogoutDialog()
        {
            this.InitializeComponent();

            txtUser.Text = txtUsername.Text = Helper.CurrentUser.UserName.ToLower();
            UserImage.Fill = Helper.CurrentUser.ProfilePicture.GetImageBrush();
        }

        private async void OkButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    await Helper.InstaApi.LogoutAsync();
                }
                catch { }
                foreach (var item in Helper.InstaApiList.ToList())
                {
                    if (item.GetLoggedUser().UserName.ToLower() == Helper.InstaApiSelectedUsername.ToLower())
                    {
                        Helper.InstaApiList.Remove(item);
                        break;
                    }
                }
                SessionHelper.DeleteCurrentSession();
                Helper.InstaApi = null;
                Helper.InstaApiSelectedUsername = null;
                SettingsHelper.SaveSettings();
                if (Helper.InstaApiList.Count == 0)
                {
                    Helpers.NavigationService.Navigate(typeof(Views.Sign.SignInView));
                    if (Helpers.NavigationService.Frame.BackStack.Any())
                        Helpers.NavigationService.Frame.BackStack.Clear();
                    MainPage.Current?.HideHeaders();
                    Helpers.NavigationService.HideBackButton();
                }
                else
                {
                    Helper.InstaApi = Helper.InstaApiList[0];
                    Helper.InstaApiSelectedUsername = Helper.InstaApiList[0].GetLoggedUser().UserName.ToLower();
                    SettingsHelper.SaveSettings();
                    Helper.UserChanged = true;
                    try
                    {
                        MainPage.Current.NavigateToMainView(true);
                    }
                    catch { }
                }
          
            }
            catch { }
            Hide();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e) => Hide();
    }
}
