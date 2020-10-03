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
    public sealed partial class EditProfileDialog : ContentPopup
    {
        public EditProfileDialog()
        {
            this.InitializeComponent();
            
            Loaded += EditProfileDialogLoaded;
        }

        private void EditProfileDialogLoaded(object sender, RoutedEventArgs e)
        {
            SetUser();
            SetCurrentUser();
        }

        void SetUser()
        {
            try
            {
                var user = Helper.CurrentUser;
                NameText.Text = user.FullName;
                UsernameText.Text = user.UserName;
                BioText.Text = user.Biography; 
                WebsiteText.Text = user.ExternalUrl;
                UserImage.Fill = user.ProfilePicture.GetImageBrush();
                GenderCombo.SelectedIndex = 0;
            }
            catch { }
        }
        async void SetCurrentUser()
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    ShowLoading();
                    var userResult = await Helper.InstaApi.GetCurrentUserAsync();
                    if (userResult.Succeeded)
                    {
                        var user = userResult.Value;
                        NameText.Text = user.FullName;
                        UsernameText.Text = user.UserName;
                        BioText.Text = user.Biography;
                        WebsiteText.Text = user.ExternalUrl;
                        UserImage.Fill = user.ProfilePicture.GetImageBrush();
                        PhoneNumberText.Text = user.PhoneNumber;
                        EmailText.Text = user.Email;
                        switch (user.Gender)
                        {
                            case InstagramApiSharp.Enums.InstaGenderType.Male:
                                GenderCombo.SelectedIndex = 1;
                                break;
                            case InstagramApiSharp.Enums.InstaGenderType.Female:
                                GenderCombo.SelectedIndex = 2;
                                break;
                            default:
                                GenderCombo.SelectedIndex = 0;
                                break;
                        }
                    }
                    HideLoading();
                });
            }
            catch { HideLoading(); }
        }
        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private async void OkButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    var gender = InstagramApiSharp.Enums.InstaGenderType.Unknown;
                    switch(GenderCombo.SelectedIndex)
                    {
                        case 1:
                            gender = InstagramApiSharp.Enums.InstaGenderType.Male;
                            break;
                        case 2:
                            gender = InstagramApiSharp.Enums.InstaGenderType.Female;
                            break;
                    }
                    ShowLoading();
                    var userResult = await Helper.InstaApi.AccountProcessor
                    .EditProfileAsync(NameText.Text,BioText.Text,WebsiteText.Text,EmailText.Text,PhoneNumberText.Text, gender, UsernameText.Text);
                    if (userResult.Succeeded)
                    {
                        var user = userResult.Value;
                        Helper.CurrentUser.UserName = user.Username;
                        Helper.CurrentUser.FullName = user.FullName;
                        Helper.CurrentUser.ProfilePicture = user.ProfilePicUrl;
                        Helper.CurrentUser.ProfilePicUrl = user.ProfilePicUrl;
                        Helper.CurrentUser.ProfilePictureId = user.ProfilePicId;
                        Helper.CurrentUser.Biography = user.Biography;
                        Helper.CurrentUser.BiographyWithEntities = user.BiographyWithEntities;
                        Helper.CurrentUser.ExternalUrl = user.ExternalUrl;
                    }
                    HideLoading();
                });
            }
            catch { }
        }

        public void ShowLoading()
        {
            LoadingPb.IsActive = true;
            LoadingGrid.Visibility = Visibility.Visible;
        }

        public void HideLoading()
        {
            LoadingPb.IsActive = false;
            LoadingGrid.Visibility = Visibility.Collapsed;
        }
        private void SelectFromGalleryButtonClick(object sender, RoutedEventArgs e)
        {

        }

    }
}
