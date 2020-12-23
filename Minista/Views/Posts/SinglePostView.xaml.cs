using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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


namespace Minista.Views.Posts
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SinglePostView : Page, INotifyPropertyChanged
    {
        private InstaMedia _media;
        public InstaMedia/*InstaPost*/ Media
        {
            get
            {
                return _media;
            }
            set
            {
                _media = value;
                OnPropertyChanged("Media");
                MediaUc.Visibility = Visibility.Visible;
                MediaUc.DataContext = value/*.Media*/;
                //MediaUc.DataContext = value;
            }
        }
        //public static readonly DependencyProperty MediaProperty =
        //    DependencyProperty.Register("Media",
        //        typeof(InstaMedia),
        //        typeof(SinglePostView),
        //        new PropertyMetadata(null));
        public static SinglePostView Current;
        private bool CanLoadFirstPopUp = false;
        private InstaMedia MediaPrivate;
        private string MediaIdOrLINK;
        private string MediaIdOrLINKPrivate;
        NavigationMode NavigationMode;
        public SinglePostView()
        {
            this.InitializeComponent();
            Current = this;
            Loaded += SinglePostView_Loaded;
        }

        private void SinglePostView_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationMode == NavigationMode.Back && Media != null)
            {
                if (MediaPrivate == null) return;
                if (Media.InstaIdentifier == MediaPrivate.InstaIdentifier)
                    return;
            }
            if (NavigationMode == NavigationMode.Back && !string.IsNullOrEmpty(MediaIdOrLINKPrivate))
            {
                if (MediaIdOrLINKPrivate.ToLower() == MediaIdOrLINK.ToLower())
                    return;
            }
            else if (NavigationMode == NavigationMode.New)
            {
                NavigationCacheMode = NavigationCacheMode.Enabled;
                CanLoadFirstPopUp = false;
            }
            if (!CanLoadFirstPopUp)
            {
                Handle();
                CanLoadFirstPopUp = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationMode = e.NavigationMode;
            if (e.NavigationMode == NavigationMode.New)
                GetType().RemovePageFromBackStack();

            try
            {
                if (e.Parameter is InstaMedia media)
                {
                    MediaPrivate = /*ToPost*/(media);
                    MediaIdOrLINKPrivate = null;
                }
                else if (e.Parameter is string mediaIdOrLink)
                {
                    MediaIdOrLINKPrivate = mediaIdOrLink;
                    MediaPrivate = null;
                    //HandleLinkOrMediaId(mediaIdOrLink);
                }
            }
            catch { }
        }
        public void HandleUri(object obj)
        {
            try
            {
                if (obj is InstaMedia media)
                {
                    MediaPrivate = media;
                    MediaIdOrLINKPrivate = null;
                    Handle();
                }
                else if (obj is string mediaIdOrLink)
                {
                    MediaIdOrLINKPrivate = mediaIdOrLink;
                    MediaPrivate = null;
                    Handle();
                }
            }
            catch { }
        }
        void Handle()
        {
            try
            {
                Media = null;
                if (!string.IsNullOrEmpty(MediaIdOrLINKPrivate))
                {
                    MediaIdOrLINK = MediaIdOrLINKPrivate;
                    HandleLinkOrMediaId(MediaIdOrLINK);
                }
                else if (MediaPrivate != null)
                {
                    Media = MediaPrivate;
                }
            }
            catch { }
        }
        async void HandleLinkOrMediaId(string mediaIdOrLink)
        {
            try
            {
                if (string.IsNullOrEmpty(mediaIdOrLink)) return;
                mediaIdOrLink = mediaIdOrLink.Trim();
                await MainPage.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    ShowTopLoading();
                    try
                    {
                        string mediaId = mediaIdOrLink;
                        if (mediaIdOrLink.StartsWith("http://") || mediaIdOrLink.StartsWith("https://") || mediaIdOrLink.StartsWith("www."))
                        {
                            var mediaInfoFromUrl = await Helper.InstaApi.MediaProcessor.GetMediaIdFromUrlAsync(new Uri(mediaIdOrLink));
                            if (mediaInfoFromUrl.Succeeded)
                                mediaId = mediaInfoFromUrl.Value;
                            else
                            {
                                if (mediaInfoFromUrl.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.PrivateMedia)
                                {
                                    Helper.ShowNotify("This media is private and can't get view it with URL.", 5000);
                                    HideTopLoading();
                                    Helpers.NavigationService.GoBack();
                                    return;
                                }
                                else if (mediaInfoFromUrl.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.NoMediaMatch)
                                {
                                    Helper.ShowNotify("No media match.\r\nThis means media is deleted or something is wrong with URL you provided.", 5000);
                                    HideTopLoading();
                                    Helpers.NavigationService.GoBack();
                                    return;
                                }
                            }
                        }

                        var media = await Helper.InstaApi.MediaProcessor.GetMediaByIdAsync(mediaId);
                        if (media.Succeeded)
                            Media = /*ToPost*/(media.Value);
                        else
                        {
                            Helper.ShowNotify("Something went wrong!", 4000);
                        }
                    }
                    catch { }
                    HideTopLoading();
                });
            }
            catch { HideTopLoading(); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string memberName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }
        InstaPost ToPost(InstaMedia media)
        {
            return new InstaPost { Media = media, Type = InstagramApiSharp.Enums.InstaFeedsType.Media };
        }


        #region LOADINGS
        public void ShowTopLoading() => Loading.Start();
        public void HideTopLoading() => Loading.Stop();

        #endregion LOADINGS
    }
}
