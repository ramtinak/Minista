using InstagramApiSharp.Classes.Models;
using Minista.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing.PrintTicket;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace Minista.Views.Uploads
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadView : Page
    {
        public InstaLocationShort CurrentLocation { get; set; } 
        //public ObservableCollection<UploadUc> UploadUcList = new ObservableCollection<UploadUc>();
        public ObservableCollection<UploadUcItem> UploadUcListX = new ObservableCollection<UploadUcItem>();
        public static UploadView Current;

        private readonly VideoConverterX Convertions = new VideoConverterX();
        public UploadView()
        {
            this.InitializeComponent();
            Current = this;
            LV.ItemsSource = UploadUcListX;
            Loaded += UploadView_Loaded;
            //FlipView.ItemsSource = UploadUcList;
            Convertions.OnText += ConvertionsOnText;
            Convertions.OnOutput += ConvertionsOnOutput;
        }

        private void UploadView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UserImage.Fill = Helper.CurrentUser.ProfilePicture.GetImageBrush();
            }
            catch { }
        }

        private void ImportButtonClick(object sender, RoutedEventArgs e) => ImportFiles();

        private async void UploadButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UploadUcListX.Count == 0)
                {
                    Helper.ShowNotify("Please select at least one image or video.", 5500);
                    return;
                }
                int index = 0;
                bool editing = false;
                int ix = 0;
                //foreach (var item in UploadUcList)
                //{
                //    if (item.Editing)
                //    {
                //        editing = true;
                //        index = ix;
                //        break;
                //    }
                //    ix++;
                //}

                foreach (var item in UploadUcListX)
                {
                    if (item.PlusVisibility == Visibility.Collapsed)
                    {
                        if (item.UploadUc.Editing)
                        {
                            editing = true;
                            index = ix;
                            break;
                        }
                        ix++;
                    }
                }
                if (editing)
                {
                    Helper.ShowNotify("Some of your photo(s)/video(s) needs edit, edit them first and then try again.", 5500);
                    //LV.SelectedIndex = index;
                    CPresenter.Content = UploadUcListX[index].UploadUc;
                    //FlipView.SelectedIndex = index;
                    return;
                }
                var list = new List<StorageUploadItem>();

                foreach (var item in UploadUcListX)
                    if (item.PlusVisibility == Visibility.Collapsed)
                    {
                        var itemX = item.UploadUc.UploadItem;
                        itemX.Location = CurrentLocation;
                        itemX.DisableComments = ToggleTurnOffCommenting.IsOn;
                        list.Add(itemX);
                    }
                ConvertionGrid.Visibility = Visibility.Visible;
                LoadingUc.Start();
                var converted = await Convertions.ConvertFilesAsync(list);

                ConvertionGrid.Visibility = Visibility.Collapsed;
                LoadingUc.Stop();
                var su = new StorageUpload();
                su.SetUploadItem(converted, CaptionText.Text);
                Helper.ShowNotify("Convertion is done now you can use Minista\r\n" +
                    "We will upload your media(s) in background.", 6000);

                ShowNextButton();
                Helpers.NavigationService.GoBack();
            }
            catch { }
        }

        private async void ConvertionsOnOutput(string text)
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResultText.Text = text;
                });
            }
            catch { }
        }

        private async void ConvertionsOnText(string text)
        {
            try
            {
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ContentText.Text = text;
                });
            }
            catch { }
        }
        private async void ImportFiles(bool appendFiles = false)
        {
            try
            {
                Helper.DontUseTimersAndOtherStuff = true;
                FileOpenPicker openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".bmp");
                //openPicker.FileTypeFilter.Add(".gif");
                openPicker.FileTypeFilter.Add(".png");
                openPicker.FileTypeFilter.Add(".mp4");
                //openPicker.FileTypeFilter.Add(".mkv");
                var files = await openPicker.PickMultipleFilesAsync();
                Helper.DontUseTimersAndOtherStuff = false;
                ImportFiles(files, appendFiles);
            }
            catch (Exception ex)
            {
                ex.PrintException("ImportButtonClick");
                Helper.ShowErr("ImportButtonClick", ex);
                Helper.DontUseTimersAndOtherStuff = false;
            }

        }
        private async void ImportFiles(IReadOnlyList<StorageFile> files, bool appendFiles = false)
        {
            try
            {
                if (files == null) return;
                if (files.Count > 0)
                {
                    if (!appendFiles)
                    {
                        //UploadUcList.Clear();
                        UploadUcListX.Clear();
                        CurrentLocation = null;
                        NextButton.IsEnabled = true;
                        NextButton.Visibility = Visibility.Visible;
                        UploadButton.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        var plusItem = UploadUcListX.FirstOrDefault(x => x.PlusVisibility == Visibility.Visible);
                        if (plusItem != null)
                            UploadUcListX.Remove(plusItem);
                    }
                    LoadingUc.Start();
                    foreach (var file in files)
                    {
                        var item = new UploadUcItem
                        {
                            CloseVisibility = Visibility.Visible
                        };
                        var uc = new UploadUc
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch
                        };
                        uc.SetNewFile(file);
                        item.UploadUc = uc;
                        UploadUcListX.Add(item);
                        //item.SetThumbIfExists();
                        //UploadUcList.Add(uc);
                    }
                    await Task.Delay(500);
                    var tasks = new List<Task>();
                    foreach (var item in UploadUcListX)
                    {
                        try
                        {
                            if (item.Loadings != null && !item.Started)
                            {
                                item.Started = true;
                                item.Loadings.Start();
                                tasks.Add(item.UploadUc.InitFileAsync());
                            }
                        }
                        catch { }
                    }
                    await Task.Delay(3000);
                    if (UploadUcListX.Count < 9)
                    {
                        var item = new UploadUcItem
                        {
                            VideoVisibility = Visibility.Collapsed,
                            PlusVisibility = Visibility.Visible
                        };
                        UploadUcListX.Add(item);
                    }
                    if (!appendFiles && UploadUcListX.Count > 0) CPresenter.Content = UploadUcListX[0].UploadUc;

                    await Task.WhenAll(tasks);
                    foreach (var item in UploadUcListX)
                        item.SetThumbIfExists();
                    LoadingUc.Stop();

                }
            }
            catch(Exception ex) { ex.PrintException("ImportFiles(IReadOnlyList<StorageFile>"); }

        }
        public void ShowNextButton()
        {
            NextButton.IsEnabled = true;
            OptionsGrid.Visibility = Visibility.Collapsed;
            NextButton.Visibility = Visibility.Visible;
            UploadButton.Visibility = Visibility.Collapsed;
        }
        private void LVItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem != null)
                {
                    var item = e.ClickedItem as UploadUcItem;
                    if (item != null)
                    {
                        if (item.PlusVisibility == Visibility.Visible)
                            ImportFiles(true);
                        else
                        {
                            CPresenter.Content = item.UploadUc;
                        }
                    }
                }
            }
            catch { }
        }

        private void LoadingUc_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var uc = sender as UserControls.LoadingUc;
            if (uc != null)
            {
                try
                {
                    if (uc.DataContext is UploadUcItem item && item != null)
                    {
                        item.Loadings = uc;
                        //if (item.Started)
                        //    uc.Start();
                        //else
                        //    uc.Stop();
                    }
                }
                catch { }
            }
        }

        private void DeleteItemTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is Ellipse elp && elp.DataContext is UploadUcItem item)
                    UploadUcListX.Remove(item);
            }
            catch { }
        }

        private void TagPeopleToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TagPeopleToggleButton.IsChecked = false;
            }
            catch { }
            TagPeople();
            //Helper.ShowNotify("This is only available in the developer version FOR NOW", 3500);
        }

        private void AddLocationToggleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddLocationToggleButton.IsChecked = false;
            }
            catch { }
            AddLocationView.Visibility = Visibility.Visible;
        }

        private void ToggleTurnOffCommentingToggled(object sender, RoutedEventArgs e)
        {
        }

        private void RemoveLocationButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentLocation = null;
            txtLocationAddress.Text = txtLocationName.Text = string.Empty;

            LocationGrid.Visibility = Visibility.Collapsed;
        }
        public void SetLocation(InstaLocationShort locationShort)
        {
            CurrentLocation = locationShort;
            txtLocationAddress.Text = locationShort.Address;
            txtLocationName.Text = locationShort.Name;
            LocationGrid.Visibility = Visibility.Visible;
        }

        private void NextButtonClick(object sender, RoutedEventArgs e)
        {
            if (UploadUcListX.Count == 0)
            {
                Helper.ShowNotify("Please select at least one photo or video.", 2000);
                return;
            }
            OptionsGrid.Visibility = Visibility.Visible;
            UploadButton.Visibility = Visibility.Visible;
            NextButton.Visibility = Visibility.Collapsed;
        }
        void TagPeople()
        {
            for (int i = 0; i < UploadUcListX.Count; i++)
            {
                try
                {
                    if (UploadUcListX[i].PlusVisibility == Visibility.Collapsed)
                        UploadUcListX[i].UploadUc.CanTagPeople = true;
                }
                catch { }
            }
            NextButton.IsEnabled = true;

            OptionsGrid.Visibility = Visibility.Collapsed;
            UploadButton.Visibility = Visibility.Collapsed;
            TagText.Visibility = NextButton.Visibility = Visibility.Visible;

        }
    }
}
