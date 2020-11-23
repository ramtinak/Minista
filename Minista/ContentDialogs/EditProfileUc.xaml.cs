using Microsoft.Toolkit.Uwp.UI.Controls;
using Minista.Controls.PhotoCropper.Helpers;
using Minista.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Minista.UI.Controls;
using InstagramApiSharp.Classes.Models;
using System.Threading.Tasks;

namespace Minista.ContentDialogs
{
    public sealed partial class EditProfileUc : UserControl
    {
        public event EventHandler OnCompleted;
        private InstaCurrentUser CurrentUser;
        public EditProfileUc()
        {
            this.InitializeComponent();
            Uploader.OnCompleted += Uploader_OnCompleted;
            Uploader.OnError += Uploader_OnError;
        }

        public void Show()
        {
            SetUser();
            SetCurrentUser();
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
            HideEditor();
            OnCompleted?.Invoke(this, new EventArgs());
        }
        public void ShowEditor()
        {
            EditorGrid.Visibility = Visibility.Visible;
        }

        public void HideEditor()
        {
            EditorGrid.Visibility = Visibility.Collapsed;
            if (Uploader != null)
                Uploader.IsUploading = false;
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
                        CurrentUser = user;
                        PublicInformationGrid.Visibility = Helper.CurrentUser.IsBusiness ? Visibility.Visible : Visibility.Collapsed;
                        PersonalInformationButton.Visibility = !Helper.CurrentUser.IsBusiness ? Visibility.Visible : Visibility.Collapsed;
                        NameText.Text = user.FullName;
                        UsernameText.Text = user.UserName;
                        BioText.Text = user.Biography;
                        WebsiteText.Text = user.ExternalUrl;
                        UserImage.Fill = user.ProfilePicture.GetImageBrush();
                        PhoneNumberText.Text = user.PhoneNumber;
                        EmailText.Text = user.Email;
                        PersonalCustomGenderText.Text = CustomGenderText.Text = user.CustomGender;
                        if (!string.IsNullOrEmpty(user.Birthday))
                        {
                            try
                            {
                                var date = DateTime.Parse(user.Birthday);
                                BirthDatePicker.Date = DateTime.Parse(user.Birthday);
                            }
                            catch { }
                        }
                        switch (user.Gender)
                        {
                            case InstagramApiSharp.Enums.InstaGenderType.Female:
                               PersonalGenderCombo.SelectedIndex = GenderCombo.SelectedIndex = 0;
                                break;
                            case InstagramApiSharp.Enums.InstaGenderType.Male:
                                PersonalGenderCombo.SelectedIndex = GenderCombo.SelectedIndex = 1;
                                break;
                            case InstagramApiSharp.Enums.InstaGenderType.Custom:
                                PersonalGenderCombo.SelectedIndex = GenderCombo.SelectedIndex = 2;
                                break;
                            case InstagramApiSharp.Enums.InstaGenderType.Unknown:
                            default:
                                PersonalGenderCombo.SelectedIndex = GenderCombo.SelectedIndex = 3;
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
                    switch (GenderCombo.SelectedIndex)
                    {
                        case 0:
                            gender = InstagramApiSharp.Enums.InstaGenderType.Female;
                            break;
                        case 1:
                            gender = InstagramApiSharp.Enums.InstaGenderType.Male;
                            break;
                        case 2:
                            gender = InstagramApiSharp.Enums.InstaGenderType.Custom;
                            break;
                    }
                    ShowLoading();
                    try
                    {
                        if (CurrentUser != null)
                        {
                            if (CurrentUser.Gender != gender)
                            {
                                await Helper.InstaApi.AccountProcessor.SetGenderAsync(gender, CustomGenderText.Text);
                                await Task.Delay(1500);

                            }
                        }
                    }
                    catch { }
                    var userResult = await Helper.InstaApi.AccountProcessor
                    .EditProfileAsync(NameText.Text, BioText.Text, WebsiteText.Text, EmailText.Text, PhoneNumberText.Text, gender, UsernameText.Text);
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




        /////////////////////////////////////////////////////////
        ////////////////////// PROFILE UPLOADER /////////////////
        /////////////////////////////////////////////////////////

        public ProfileUploader Uploader = new ProfileUploader();
        private async void SelectFromGalleryButtonClick(object sender, RoutedEventArgs e)
        {
           
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".bmp");
            //openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile imgFile = await openPicker.PickSingleFileAsync();
            if (imgFile != null)
            {
                Helper.CreateCachedFolder();
                Helper.CreateCachedFolder();
                Helper.CreateCachedFolder();
                Helper.CreateCachedFolder();
                ShowEditor();
                ImageCropper.AspectRatio = 1d;
                ImageCropper.CropShape = UI.Controls.CropShape.Circular;
                await ImageCropper.LoadImageFromFile(imgFile);
            }
        }

        private void ExitImageButtonClick(object sender, RoutedEventArgs e) => HideEditor();

        private async void OkImageButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading();

                Helper.CreateCachedFolder();
                Helper.CreateCachedFolder();
                Helper.CreateCachedFolder();
                var cacheFolder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
                var file = await cacheFolder.CreateFileAsync(15.GenerateRandomStringStatic() + ".jpg");
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None))
                    await ImageCropper.SaveAsync(fileStream, UI.Controls.BitmapFileFormat.Jpeg);
                var fileToUpload = await new PhotoHelper().SaveToImage(file);
                Uploader.UploadSinglePhoto(fileToUpload);
            }
            catch { }
        }



        private async void Uploader_OnError(object sender, string e)
        {
            HideLoading();
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    e.ShowErr();
                });
            }
            catch { }
        }

        private async void Uploader_OnCompleted(object sender, EventArgs e)
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    var userResult = await Helper.InstaApi.GetCurrentUserAsync();
                    if (userResult.Succeeded)
                    {
                        var user = userResult.Value;
                        UserImage.Fill = user.ProfilePicture.GetImageBrush();
                        Helper.CurrentUser.ProfilePicture = user.ProfilePicture;
                        Helper.CurrentUser.ProfilePicUrl = user.ProfilePicUrl;
                    }

                    HideEditor();
                    HideLoading();
                });
            }
            catch { HideLoading(); }
           
        }

        private void PersonalExitButtonClick(object sender, RoutedEventArgs e)
        {
            PersonalInfoGrid.Visibility = Visibility.Collapsed;
        }

        private async void PersonalOkButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    var gender = InstagramApiSharp.Enums.InstaGenderType.Unknown;
                    switch (GenderCombo.SelectedIndex)
                    {
                        case 0:
                            gender = InstagramApiSharp.Enums.InstaGenderType.Female;
                            break;
                        case 1:
                            gender = InstagramApiSharp.Enums.InstaGenderType.Male;
                            break;
                        case 2:
                            gender = InstagramApiSharp.Enums.InstaGenderType.Custom;
                            break;
                    }
                    ShowLoading();
                    try
                    {
                        if (CurrentUser != null)
                        {
                            if (CurrentUser.Gender != gender)
                            {
                                await Helper.InstaApi.AccountProcessor.SetGenderAsync(gender, CustomGenderText.Text);
                                await Task.Delay(1500);
                            }
                            var date = BirthDatePicker.Date.Date;
                            if (date.Year == DateTime.UtcNow.Year)
                                date = RandomDay();

                            await Helper.InstaApi.AccountProcessor.SetBirthdayAsync(date);
                        }
                    }
                    catch { }
                    var userResult = await Helper.InstaApi.AccountProcessor.GetRequestForEditProfileAsync();
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
                        Helper.CurrentUser.ExternalUrl = user.ExternalUrl;
                    }
                    HideLoading();
                    PersonalInfoGrid.Visibility = Visibility.Collapsed;
                });
            }
            catch
            {
                PersonalInfoGrid.Visibility = Visibility.Collapsed;
            }
        }
        static readonly Random Rnd = new Random();
        DateTime RandomDay()
        {
            DateTime start = new DateTime(1985, 1, 1);
            int range = (new DateTime(2000, 1, 1) - start).Days;
            return start.AddDays(Rnd.Next(range));
        }
        private void PersonalGenderComboSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (PersonalGenderCombo.SelectedIndex != -1)
                {
                    if (PersonalGenderCombo.SelectedIndex == 2) PersonalCustomGenderText.Visibility = Visibility.Visible;
                    else PersonalCustomGenderText.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        private void GenderComboSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try 
            {
                if (GenderCombo.SelectedIndex != -1)
                {
                    if (GenderCombo.SelectedIndex == 2) CustomGenderText.Visibility = Visibility.Visible;
                    else CustomGenderText.Visibility = Visibility.Collapsed;
                }
            } 
            catch { }
        }

        private void PersonalInformationButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                PersonalInformationButton.IsChecked = true;
            }
            catch { }
            PersonalInfoGrid.Visibility = Visibility.Visible;
        }
    }
}
