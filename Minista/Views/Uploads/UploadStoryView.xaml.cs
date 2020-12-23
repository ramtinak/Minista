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
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Minista.Views.Uploads
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadStoryView : Page
    {
        public static UploadStoryView Current;

        private readonly VideoConverterX Convertions = new VideoConverterX();
        public UploadStoryView()
        {
            this.InitializeComponent();
            Current = this;
            Loaded += UploadView_Loaded;
            Convertions.OnText += ConvertionsOnText;
            Convertions.OnOutput += ConvertionsOnOutput;
        }

        private void UploadView_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ImportButtonClick(object sender, RoutedEventArgs e) => ImportFile();

        private async void ImportFile(bool appendFiles = false)
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
                openPicker.FileTypeFilter.Add(".png");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".bmp");
                openPicker.FileTypeFilter.Add(".mp4");
                var file = await openPicker.PickSingleFileAsync();
                if (file == null) return;
                await UploadStoryUc.SetFileAsync(file);
                Helper.DontUseTimersAndOtherStuff = false;
            }
            catch (Exception ex)
            {
                ex.PrintException("ImportButtonClick");
                Helper.ShowErr("ImportButtonClick", ex);
                Helper.DontUseTimersAndOtherStuff = false;
            }

        }

        private async void UploadButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var uploadItem = UploadStoryUc.UploadItem;
                if (uploadItem == null)
                {
                    Helper.ShowNotify("Please select at least one image or video.", 5500);
                    return;
                }
                if (UploadStoryUc.Editing)
                {
                    Helper.ShowNotify("Your photo/video needs edit, edit it first and then try again.", 5500);
                    return;
                }
                ConvertionGrid.Visibility = Visibility.Visible;
                LoadingUc.Start();
                var converted = await Convertions.ConvertFilesAsync(new List<StorageUploadItem> { uploadItem });

                ConvertionGrid.Visibility = Visibility.Collapsed;
                LoadingUc.Stop();
                var su = new StorageUpload();
                su.SetUploadItem(converted, "");
                Helper.ShowNotify("Convertion is done now you can use Minista\r\n" +
                    "We will upload your media(s) to story in background.", 6000);
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
    }
}
