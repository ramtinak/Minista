using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Minista.Views.Settings.Security
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PasswordView : Page
    {
        public PasswordView()
        {
            this.InitializeComponent();
        }

        private void CurrentPasswordTextKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                try
                {
                    if (CurrentPasswordText.Password.Length > 5)
                        NewPasswordText.Focus(FocusState.Keyboard);
                    else
                        CurrentPasswordText.Focus(FocusState.Keyboard);
                }
                catch { }
            }
        }

        private void NewPasswordTextKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                try
                {
                    if (NewPasswordText.Password.Length > 5)
                        NewPassword2Text.Focus(FocusState.Keyboard);
                    else
                        NewPasswordText.Focus(FocusState.Keyboard);
                }
                catch { }
            }
        }

        private void NewPassword2TextKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                try
                {
                    if (NewPassword2Text.Password.Length > 5)
                    {
                        if (NewPasswordText.Password == NewPassword2Text.Password)
                        {
                            ChangeAsync();
                            return;
                        }
                        else
                            PasswordIsNotSame();
                    }
                    NewPassword2Text.Focus(FocusState.Keyboard);
                }
                catch { }
            }
        }

        private void ChangeButtonClick(object sender, RoutedEventArgs e) => ChangeAsync();
        async void ChangeAsync()
        {
            try
            {
                CurrentPasswordText.Password = CurrentPasswordText.Password.Trim();
                NewPasswordText.Password = NewPasswordText.Password.Trim();
                NewPassword2Text.Password = NewPassword2Text.Password.Trim();
                if (string.IsNullOrEmpty(CurrentPasswordText.Password))
                {
                    CurrentPasswordText.Focus(FocusState.Keyboard);
                    return;
                }
                if (string.IsNullOrEmpty(NewPasswordText.Password))
                {
                    NewPasswordText.Focus(FocusState.Keyboard);
                    return;
                }
                if (string.IsNullOrEmpty(NewPassword2Text.Password))
                {
                    NewPassword2Text.Focus(FocusState.Keyboard);
                    return;
                }
                if (NewPasswordText.Password != NewPassword2Text.Password)
                    PasswordIsNotSame();
                else if (NewPasswordText.Password.Length < 6 || NewPassword2Text.Password.Length < 6)
                    PasswordMustBe();
                else
                {
                    MainPage.Current?.ShowLoading();
                    var result = await Helper.InstaApi.AccountProcessor
                        .ChangePasswordAsync(CurrentPasswordText.Password, NewPasswordText.Password);
                    MainPage.Current?.HideLoading();
                    if (result.Succeeded)
                    {
                        Helper.ShowNotify("Your password changed successfully.", 4000);
                        Helper.InstaApi.GetLoggedUser().Password = NewPasswordText.Password;

                        await Task.Delay(300);
                        SessionHelper.SaveCurrentSession();
                        Helpers.NavigationService.GoBack();
                    }
                    else
                        Helper.ShowErr(result.Info.Message, result.Info.Exception);
                }
            }
            catch
            {
                MainPage.Current?.HideLoading();
            }
        }
        void PasswordMustBe() => Helper.ShowNotify("Passwords must be at least 6 characters.", 3500); 

        void PasswordIsNotSame() => Helper.ShowNotify("New password and repeated new password is not the same.\r\nPlease check it and try again.", 3500);
    }
}
