using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;
using InstagramApiSharp.API.Builder;
using Windows.UI.Popups;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Net;
using Minista;
using Minista.Helpers;
using InstagramApiSharp.Enums;
using Windows.Services.Store;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Newtonsoft.Json.Linq;
using FFmpegInterop;
using Windows.Media.Editing;
using Windows.Graphics.Imaging;
using Minista.Classes;
using Windows.Storage.AccessCache;
using Minista.Views.Security;
using Windows.Foundation.Metadata;
using InstagramApiSharp.Classes.Android.DeviceInfo;
//using Google.Protobuf.WellKnownTypes;
// xml tags:
// \n = &#xA;
// \r = &#xD;
// \t = &#x9;
// space = &#x20;
static class Helper
{
    internal static IPasscode Passcode { get; set; }
    //internal static bool IsApiPresents => ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5);
    private readonly static string[] SupportedImagesTypes = new string[] { ".jpg", ".jpeg", ".png", ".bmp" };
    private readonly static string[] SupportedVideosTypes = new string[] { ".mp4", ".mpeg4" };
    //public static RuntimeSupportedSDKs SDKs = new RuntimeSupportedSDKs();

    public static bool IsSupportedImage(this string extension)
    {
        if (string.IsNullOrEmpty(extension)) return false;
        try
        {
            if (!extension.StartsWith("."))
                extension = Path.GetExtension(extension);
        }
        catch { }
        return SupportedImagesTypes.Contains(extension.ToLower());
    }
    public static bool IsSupportedVideo(this string extension)
    {
        if (string.IsNullOrEmpty(extension)) return false;
        try
        {
            if (!extension.StartsWith("."))
                extension = Path.GetExtension(extension);
        }
        catch { }
        return SupportedVideosTypes.Contains(extension.ToLower());
    }
    public static FFmpegInteropConfig FFmpegConfig;
    public static void CreateConfig()
    {
        FFmpegConfig = new FFmpegInteropConfig
        {
        };
    }
    public static readonly JArray SupportedCapabalities = new JArray
        {
            new JObject
            {
                {"name","SUPPORTED_SDK_VERSIONS"},
                {"value","66.0,67.0,68.0,69.0,70.0,71.0,72.0,73.0,74.0,75.0,76.0,77.0,78.0,79.0,80.0,81.0,82.0,83.0"+
                    "84.0,85.0,86.0,87.0,88.0,89.0,90.0,91.0,92.0,93.0,94.0,95.0,96.0,97.0,98.0,99.0,100.0,101.0"}
            },
            new JObject
            {
                {"name","FACE_TRACKER_VERSION"},
                {"value","14"}
            },
            new JObject
            {
                {"name","COMPRESSION"},
                {"value","ETC2_COMPRESSION"}
            },
            new JObject
            {
                {"name","world_tracker"},
                {"value","world_tracker_enabled"}
            },
            new JObject
            {
                {"name","gyroscope"},
                {"value","gyroscope_enabled"}
            }
        };

    public const string AppName = "Minista";
    public const string SessionFileType = ".mises";
    public const string FolderToken = "MinistaFTokenRmt";
    private static CoreDispatcher _dispatcher;
    public static CoreDispatcher Dispatcher
    {
        get
        {
            if (_dispatcher != null)
            {
                return _dispatcher;
            }
            else
            {
                _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
                return _dispatcher;
            }
        }
    }

    static readonly Random Rnd = new Random();

    public static string InstaApiSelectedUsername { get; set; }
    public static bool UserChanged { get; set; } = false;
    public static IInstaApi InstaApi { get; set; }

    public static void AddInstaApi(IInstaApi api)
    {
        if (api != null)
        {
            if (InstaApiList.Count == 0)
                InstaApiList.Add(api);
            else
            {
                try
                {
                    if (InstaApiList.Any(x => x.GetLoggedUser().UserName.ToLower() != api.GetLoggedUser().UserName.ToLower()))
                        InstaApiList.Add(api);
                }
                catch { InstaApiList.Add(api); }
            }
        }
    }
    public static void AddInstaApiX(this List<IInstaApi>apis, IInstaApi api)
    {
        if (api != null)
        {
            if (apis.Count == 0)
                apis.Add(api);
            else
            {
                try
                {
                    if (!apis.Any(x => x.GetLoggedUser().UserName.ToLower() != api.GetLoggedUser().UserName.ToLower()))
                        apis.Add(api);
                }
                catch { apis.Add(api); }
            }
        }
    }
    public static async void RemoveInstaApi(this List<IInstaApi> apis, IInstaApi api)
    {
        try
        {
            if (api != null && api.IsUserAuthenticated)
            {
                var files = await SessionHelper.LocalFolder.GetFilesAsync();
                if (files?.Count > 0)
                {
                    var username = api.GetLoggedUser().LoggedInUser.UserName.ToLower();
                    var selectedFile = files.FirstOrDefault(x => x.Path.ToLower().EndsWith(username + SessionFileType));
                    if (selectedFile != null)
                        await selectedFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                apis.Remove(api);
            }
        }
        catch { }
    }
    public static IInstaApi InstaApiTrash { get; set; }

    public static List<IInstaApi> InstaApiList { get; set; } = new List<IInstaApi>();
    public const string MiddleDot = "᛫";
    public const string ShoppingMaterialIcon = "";
    public const string PlayMaterialIcon = "";
    public const string PauseMaterialIcon = "";
    public const string TelegramMaterialIcon = "";
    public const string TikMaterialIcon = "";
    public const string VideoMaterialIcon = "ﳵ";
    public const string CheckMaterialIcon = "";
    public const string XMaterialIcon = "";
    public const string VerifiedBadgeMaterialIcon = "";

    public const string Dots3HorizontalMaterialIcon = ""; 
    public const string Dots3VerticalMaterialIcon = "";

    //public const string RecordMaterialIcon = "";
    public const string StopMaterialIcon = "";
    public const string VerifiedAccountMaterialIcon = "";
    public const string UpMaterialIcon = "";
    public const string DownMaterialIcon = "";
    public const string LeftMaterialIcon = "";
    public const string RightMaterialIcon = "";

    public const string Up2MaterialIcon = "";
    public const string Down2MaterialIcon = "";
    public const string Left2MaterialIcon = "";
    public const string Right2MaterialIcon = "";

    public const string PlusMaterialIcon = "";
    public const string FingerprintMaterialIcon = "";
    public const string LockMaterialIcon = "";
    public const string UnLockMaterialIcon = "";


    public const string OneTikMaterialIcon = "";
    public const string DoubleTikMaterialIcon = "";
    public const string PendingTikMaterialIcon = "";


    public const string InstagramUrl = "https://www.instagram.com/";
    public static BitmapImage NoProfilePictureBitmap => "ms-appx:///Assets/Images/no-profile.jpg".GetBitmap();
    public static Uri NoProfilePictureUri => "ms-appx:///Assets/Images/no-profile.jpg".ToUri();
    public static InstaUserInfo CurrentUser { get; set; }

    public static string GetUserName(this IReadOnlyCollection<IInstaApi> apis, string u)
    { 
        var user = apis.FirstOrDefault(x => x.GetLoggedUser().LoggedInUser.Pk == long.Parse(u));
        return user?.GetLoggedUser().UserName;
    }
    public static void RemovePageFromBackStack(this Type type)
    {
        try
        {
            var l = NavigationService.Frame.BackStack.ToList();
            for (int i=0;i< l.Count;i++)
            {
                if (l[i].SourcePageType == type)
                {
                    NavigationService.Frame.BackStack.RemoveAt(i);
                    break;
                }
            }
            l = null;
            "------------------------- REMOVING BACK STACK -------------------".PrintDebug();
        }
        catch { }
    }
    public static void OpenProfile(string username)
    {
        if (username == null)
            return;
        username = username.Trim();
        if (username.ToLower() == InstaApi.GetLoggedUser().LoggedInUser.UserName.ToLower())
            NavigationService.Navigate(typeof(Minista.Views.Infos.ProfileDetailsView));
        else
        {
            if (NavigationService.Frame.Content is Minista.Views.Infos.UserDetailsView view && view != null)
                view.LoadExternalProfile(username);
            else
                NavigationService.Navigate(typeof(Minista.Views.Infos.UserDetailsView), username);
        }
    }
    public static void OpenProfile(long pk)
    {
        if (pk == InstaApi.GetLoggedUser().LoggedInUser.Pk)
            NavigationService.Navigate(typeof(Minista.Views.Infos.ProfileDetailsView));
        else
        {
            if (NavigationService.Frame.Content is Minista.Views.Infos.UserDetailsView view && view != null)
                view.LoadExternalProfile(new InstaUserShort { Pk = pk });
            else
                NavigationService.Navigate(typeof(Minista.Views.Infos.UserDetailsView), new InstaUserShort { Pk = pk });
        }
    }
    public static void OpenProfile(InstaUserShort userShort)
    {
        if (userShort.Pk == InstaApi.GetLoggedUser().LoggedInUser.Pk)
            NavigationService.Navigate(typeof(Minista.Views.Infos.ProfileDetailsView));
        else
        {
            if (NavigationService.Frame.Content is Minista.Views.Infos.UserDetailsView view && view != null)
                view.LoadExternalProfile(userShort);
            else
                NavigationService.Navigate(typeof(Minista.Views.Infos.UserDetailsView), userShort);
        }
    }
    public static void OpenProfile(InstaUserShortFriendship userShortFriendship)
    {
        if (userShortFriendship.Pk == InstaApi.GetLoggedUser().LoggedInUser.Pk)
            NavigationService.Navigate(typeof(Minista.Views.Infos.ProfileDetailsView));
        else
        {
            if (NavigationService.Frame.Content is Minista.Views.Infos.UserDetailsView view && view != null)
                view.LoadExternalProfile(userShortFriendship);
            else
                NavigationService.Navigate(typeof(Minista.Views.Infos.UserDetailsView), userShortFriendship);
        }
    }
    public static void OpenProfile(InstaUserShortFriendshipFull userShortFriendshipFull)
    {
        if (userShortFriendshipFull.Pk == InstaApi.GetLoggedUser().LoggedInUser.Pk)
            NavigationService.Navigate(typeof(Minista.Views.Infos.ProfileDetailsView));
        else
        {
            if (NavigationService.Frame.Content is Minista.Views.Infos.UserDetailsView view && view != null)
                view.LoadExternalProfile(userShortFriendshipFull);
            else
                NavigationService.Navigate(typeof(Minista.Views.Infos.UserDetailsView), userShortFriendshipFull);
        }
    }
    public static void OpenProfile(List<InstaUserShortFriendship> userShortFriendship)
    {
        if (userShortFriendship[0].Pk == InstaApi.GetLoggedUser().LoggedInUser.Pk)
            NavigationService.Navigate(typeof(Minista.Views.Infos.ProfileDetailsView));
        else
        {
            if (NavigationService.Frame.Content is Minista.Views.Infos.UserDetailsView view && view != null)
                view.LoadExternalProfile(userShortFriendship);
            else
                NavigationService.Navigate(typeof(Minista.Views.Infos.UserDetailsView), userShortFriendship);
        }
    }
    public static void OpenProfile(object[] objs)
    {
        if ((objs[0] as InstaUserShort).Pk == InstaApi.GetLoggedUser().LoggedInUser.Pk)
            NavigationService.Navigate(typeof(Minista.Views.Infos.ProfileDetailsView));
        else
        {
            if (NavigationService.Frame.Content is Minista.Views.Infos.UserDetailsView view && view != null)
                view.LoadExternalProfile(objs);
            else
                NavigationService.Navigate(typeof(Minista.Views.Infos.UserDetailsView), objs);
        }
    }
    public static void OpenProfile(InstaTVSearchResult search)
    {
        if (search.User.Pk == InstaApi.GetLoggedUser().LoggedInUser.Pk)
            NavigationService.Navigate(typeof(Minista.Views.Infos.ProfileDetailsView), true);
        else
        {
            if (NavigationService.Frame.Content is Minista.Views.Infos.UserDetailsView view && view != null)
                view.LoadExternalProfile(search);
            else
                NavigationService.Navigate(typeof(Minista.Views.Infos.UserDetailsView), search);
        }
    }
    public async static void OpenLive(InstaBroadcast broadcast)
    {
        if (SettingsHelper.Settings.LivePlaybackType == LivePlaybackType.Minista)
            NavigationService.Navigate(typeof(Minista.Views.Broadcast.LiveBroadcastView), broadcast);
        else
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                  NavigationService.Navigate(typeof(Minista.Views.Broadcast.VlcLiveBroadcastView), broadcast));
        }
    }
    public async static void OpenLive(string broadcastId)
    {
        if (SettingsHelper.Settings.LivePlaybackType == LivePlaybackType.Minista)
            NavigationService.Navigate(typeof(Minista.Views.Broadcast.LiveBroadcastView), broadcastId);
        else
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                  NavigationService.Navigate(typeof(Minista.Views.Broadcast.VlcLiveBroadcastView), broadcastId));
        }
    }
    public static void ShowNotify(string text, int duration = 1500)
    {
        try
        {
            if (MainPage.Current != null)
                MainPage.Current.ShowInAppNotify(text, duration);
        }
        catch { }
    }
    public static bool AlmostEqual(this float x, float y, float tolerance = 0.01f) =>
       Math.Abs(x - y) < tolerance;

    public static void ChangeAppMinSize()
    {
        ChangeAppMinSize(100, 100);
    }

    public static void ChangeAppMinSize(double width, double height)
    {
        ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(width, height));
    }
    public static void SetAnimation(StorageFile file)
    {
        try
        {
            if (NavigationService.Frame.Content is Minista.Views.Main.MainView view && view != null)
            {
                view.SetAnimation(file);
            }
        }
        catch { }
    }

    internal static void PurchaseMessage()
    {
        ("This feature is not for you since you are using a free version of Minista, upgrade to a premium version").ShowMsg("Warning");
    }

    public static void MuteRequested(long userPk)
    {
        try
        {
            if (NavigationService.Frame.Content is Minista.Views.Main.MainView view && view != null)
            {
                view.MainVM.PostsGenerator.MuteRequested(userPk);
            }
        }
        catch { }
    }
    public static void MuteHashtagRequested(string tagName)
    {
        try
        {
            if (NavigationService.Frame.Content is Minista.Views.Main.MainView view && view != null)
            {
                view.MainVM.PostsGenerator.MuteHashtagRequested(tagName);
            }
        }
        catch { }
    }
    public static SolidColorBrush GetColorFromResource(this string resourceKey) => Application.Current.Resources[resourceKey] as SolidColorBrush;
    public static void SetColorToResource(this string resourceKey, Color color) => (Application.Current.Resources[resourceKey] as SolidColorBrush).Color = color;

    public static void RemoveItemRequested(string mediaId)
    {
        try
        {
            if (NavigationService.Frame.Content is Minista.Views.Main.MainView view && view != null)
            {
                view.MainVM.PostsGenerator.MuteHashtagRequested(mediaId);
            }
        }
        catch { }
    }
    public static void DontShowThisItemRequested(string mediaId)
    {
        try
        {
            if (NavigationService.Frame.Content is Minista.Views.Main.MainView view && view != null)
            {
                view.MainVM.PostsGenerator.DontShowThisItemRequested(mediaId);
            }
        }
        catch { }
    }

    public static string GetKiloMillion(this long l)
    {
        string str = "";
        if (l > 0 && l < 1000)
            str = $"{(int)(l)}";
        else if (l >= 1000 && l < 1000000)
            str = $"{(int)(l / 1000)}k";
        else if (l >= 1000000)
            str = $"{(int)(l / 1000000)}m";
        return str;
    }
    #region Purchase
    //public static bool IsAppPurchased = false;
    public static bool IsAppPurchasedForKharejia = false;

    static StoreContext StoreContext = null;
    private static async Task ConfigureStorePurchase()
    {
        try
        {
            StoreContext = StoreContext.GetDefault();
            StoreContext.OfflineLicensesChanged += OfflineLicensesChanged;

            StoreProductResult result = await StoreContext.GetStoreProductForCurrentAppAsync();
            if (result.ExtendedError == null)
            {
                Debug.WriteLine("Price: " + result.Product.Price.FormattedPrice);
            }
        }
        catch { }
    }
    private static void OfflineLicensesChanged(StoreContext sender, object args)
    {
        try
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await GetLicenseState();
            });
        }
        catch { }
    }
    public static async Task<bool> GetLicenseState()
    {
        try
        {
            if (StoreContext == null)
                await ConfigureStorePurchase();

            await Task.Delay(450);

            StoreAppLicense license = await StoreContext.GetAppLicenseAsync();

            if (license.IsActive)
            {
                if (license.IsTrial)
                {
                    Debug.WriteLine("Price: " + "Trial license");
                    IsAppPurchasedForKharejia = false;
                    return false;
                }
                else
                {
                    Debug.WriteLine("Price: " + "Full license");
                    IsAppPurchasedForKharejia = true;
                    return true;
                }
            }
            else
            {
                Debug.WriteLine("Price: " + "Inactive license");
            }
        }
        catch { }
        IsAppPurchasedForKharejia = false;
        return false;
    }
    #endregion Purchase
    internal static DebugLogger DebugLogger;
    public static IInstaApi BuildApi(string username = null, string password = null)
    {
        UserSessionData sessionData;
        if (string.IsNullOrEmpty(username))
            sessionData = UserSessionData.ForUsername("FAKEUSER").WithPassword("FAKEPASS");
        else
            sessionData = new UserSessionData { UserName = username, Password = password };

        DebugLogger = new DebugLogger(InstagramApiSharp.Logger.LogLevel.All);
        AndroidDevice device = null;
        var dontGenerateToken = false;
        if (InstaApi != null)
            if (InstaApi.IsUserAuthenticated)
            {
                device = InstaApi.GetCurrentDevice();
                dontGenerateToken = true;
            }

        //var delay = RequestDelay.FromSeconds(2, 4);
        var api = InstaApiBuilder.CreateBuilder()
                  .SetUser(sessionData)
                  .SetDevice(device)
                  .UseNewDevices()
                  //.SetApiVersion(InstagramApiSharp.Enums.InstaApiVersionType.Version64)
                  //.SetRequestDelay(delay)

#if DEBUG
                  .UseLogger(DebugLogger)
#endif

                  .Build();
        api.LoadApiVersionFromSessionFile = false;
        api.DontGenerateToken = dontGenerateToken;
        api.SetTimeout(TimeSpan.FromMinutes(2));

        //InstaApi = api;
        return api;
    }
    public static BitmapImage GetBitmap(this string url) =>  new BitmapImage(new Uri(url));
    public static ImageBrush GetImageBrush(this string url) => new ImageBrush { ImageSource = url.GetBitmap(), Stretch = Stretch.UniformToFill };
    public static bool CheckLogin() => InstaApi?.IsUserAuthenticated ?? false;

    public static void FullscreenModeInXbox(/*bool full = true*/)
    {
        try
        {
            ////ApplicationViewScaling.TrySetDisableLayoutScaling(true);
            var view = ApplicationView.GetForCurrentView();
            view.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            //if(full)
            //    view.TryEnterFullScreenMode();
            //else
            //    view.ExitFullScreenMode();
        }
        catch { }
    }
    public async static void ShowMsg(this string message, string title = null)
    {
        try
        {
            await new MessageDialog(message, title ?? string.Empty).ShowAsync();
        }
        catch { }
    }
    public static void ShowErr(this string message, Exception exception = null)
    {
        try
        {
            var content = message;
            if (exception != null)
                content = exception.PrintException(message);

            ShowMsg(content, "ERR");
        }
        catch { }
    }

    public async static void DeleteCachedFolder()
    {
        try
        {
            var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Cache");
            await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }
        catch { }
    }

    public async static void DeleteCachedFilesFolder()
    {
        try
        {
            var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("CachedFiles");
            await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }
        catch { }
    }
    public async static void CreateCachedFolder()
    {
        try
        {
            await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.FailIfExists);
        }
        catch { }
    }
    public async static void CreateCachedFilesFolder()
    {
        try
        {
            await ApplicationData.Current.LocalFolder.CreateFolderAsync("CachedFiles", CreationCollisionOption.FailIfExists);
        }
        catch { }
    }
    public async static Task CreatePictureFolder()
    {
        StorageFolder folder = null;
        try
        {
            if (SettingsHelper.Settings.DownloadLocationChanged)
            {
                var contains = StorageApplicationPermissions.FutureAccessList.ContainsItem(FolderToken);
                if (contains)
                    folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(FolderToken);
            }
        }
        catch { }
        try
        {
            if (folder == null)
                folder = await KnownFolders.PicturesLibrary.CreateFolderAsync(AppName, CreationCollisionOption.FailIfExists);
        }
        catch { }
        try
        {
            await folder?.CreateFolderAsync("Stories", CreationCollisionOption.FailIfExists);
        }
        catch { }
    }
    public async static Task<StorageFolder> GetPictureFolder()
    {
        await CreatePictureFolder();
        StorageFolder folder = null;
        if (SettingsHelper.Settings.DownloadLocationChanged)
        {
            var contains = StorageApplicationPermissions.FutureAccessList.ContainsItem(FolderToken);
            if (contains)
                folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(FolderToken);
            
        }
        if (folder == null)
            folder = await KnownFolders.PicturesLibrary.GetFolderAsync(AppName);
        return folder;
    }
    public async static Task<StorageFolder> GetPictureFolderForStories()
    {
        var folder = await GetPictureFolder();
        return await folder.GetFolderAsync("Stories");
    }
    public async static Task<StorageFile> GenerateRandomOutputFile()
    {
        var folder = await KnownFolders.PicturesLibrary.GetFolderAsync(Helper.AppName);
        var outfile = await folder.CreateFileAsync(Helper.GenerateString("MINISTA_") + ".jpg", CreationCollisionOption.GenerateUniqueName);
        return outfile;
    }
    static public async Task RunInBackground(Action action)
    {
        try
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    action?.Invoke();
                }
                catch (WebException webEx)
                {
                    webEx.PrintException("RunInBackgroundWebEX");
                }
                catch (Exception ex)
                {
                    ex.PrintException("RunInBackgroundEX");
                }
            }, new CancellationTokenSource().Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        catch (Exception ex)
        {
            ex.PrintException("RunInBackground");
        }
    }

    #region Theme and App title

    public static async void HideStatusBar()
    {
        try
        {
            if (DeviceUtil.IsMobile)
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }
        }
        catch { }
    }
    public static async void ShowStatusBar(Color? backgroundColor = null, Color? foregroundColor = null)
    {
        try
        {
            if (DeviceUtil.IsMobile)
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.ShowAsync();

                statusBar.BackgroundOpacity = 1;
                if (!backgroundColor.HasValue)
                {
                    statusBar.BackgroundColor = /*color*/ ((SolidColorBrush)Application.Current.Resources["DefaultBackgroundColor}"]).Color/*GetColorFromHex("#FF151515")*/;
                    statusBar.ForegroundColor = Colors.White;
                }
                else
                {
                    statusBar.BackgroundColor = backgroundColor;
                    statusBar.ForegroundColor = foregroundColor;
                }
            }
        }
        catch { }
    }
    public static string ChangeAppTitle(this string newTitle)
    {
        //try
        //{
        //    var appView = ApplicationView.GetForCurrentView();
        //    appView.Title = newTitle;
        //}
        //catch { }
        return newTitle;
    }

    public static void ChangeTileBarTheme(Color? foregroundColor = null, Color? innerForegroundColor = null)
    {
        try
        {
            //var color = new Color() { A = 100, R = 31, G = 31, B = 31 };
            var color = Colors.Transparent;

            var inactive = GetColorFromHex("#FF646464");
            if (!DeviceUtil.IsMobile)
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;

                titleBar.BackgroundColor = color;
                titleBar.ForegroundColor = foregroundColor ?? Colors.White;
                titleBar.InactiveBackgroundColor = color;
                titleBar.InactiveForegroundColor = inactive;

                byte buttonAlpha = 255;
                titleBar.ButtonBackgroundColor = color;
                titleBar.ButtonHoverBackgroundColor = new Color() { A = buttonAlpha, R = 17, G = 17, B = 17 };
                titleBar.ButtonPressedBackgroundColor = Colors.Black;
                titleBar.ButtonInactiveBackgroundColor = color;

                titleBar.ButtonForegroundColor = innerForegroundColor ?? Colors.White;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonPressedForegroundColor = Colors.DarkGray;
                titleBar.ButtonInactiveForegroundColor = inactive;
            }
        }
        catch { }
    }
    #endregion Theme and App title

    #region Get Color from Hex
    public static SolidColorBrush GetColorBrush(this string hexColorString/*, string v*/)
    {
        return new SolidColorBrush(GetColorFromHex(hexColorString));
    }
    private static readonly Regex _hexColorMatchRegex = new Regex("^#?(?<a>[a-z0-9][a-z0-9])?(?<r>[a-z0-9][a-z0-9])(?<g>[a-z0-9][a-z0-9])(?<b>[a-z0-9][a-z0-9])$", RegexOptions.IgnoreCase);
    public static Color GetColorFromHex(this string hexColorString)
    {
        if (hexColorString == null)
            throw new NullReferenceException("Hex string can't be null.");


        var match = _hexColorMatchRegex.Match(hexColorString);


        if (!match.Success)
            throw new InvalidCastException(string.Format("Can't convert string \"{0}\" to argb or rgb color. Needs to be 6 (rgb) or 8 (argb) hex characters long. It can optionally start with a #.", hexColorString));
        byte a = 255;
        if (match.Groups["a"].Success)
            a = Convert.ToByte(match.Groups["a"].Value, 16);
        byte r = Convert.ToByte(match.Groups["r"].Value, 16);
        byte b = Convert.ToByte(match.Groups["b"].Value, 16);
        byte g = Convert.ToByte(match.Groups["g"].Value, 16);
        return Color.FromArgb(a, r, g, b);
    }
    public static Color? GetNullableColorFromHex(this string hexColorString)
    {
        if (string.IsNullOrEmpty(hexColorString))
            return null;

        return hexColorString.GetColorFromHex();
    }
    #endregion

    #region Random string generator
    public static string GenerateRandomStringStatic(this int length)
    {
        return GenerateRandomString(length);
    }
    public static string GenerateRandomString(int length = 10)
    {
        const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
        var chars = Enumerable.Range(0, length)
            .Select(x => pool[Rnd.Next(0, pool.Length)]);
        return AppName + new string(chars.ToArray());
    }
    public static string GenerateString()
    {
        return $"{AppName}_{DateTime.Now:yyyyMMdd_hhmmss}";
    }
    public static string GenerateString(string name)
    {
        return $"[{AppName}] {name}_{DateTime.Now:yyyyMMdd_hhmmss}";
    }
    #endregion  Random string generator
    public static async Task<byte[]> ToByteArray(this Stream stream)
    {
        using (var ms = new MemoryStream())
        {
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
    public static async Task<StorageFile> GetSnapshot(this StorageFile file, TimeSpan time)
    {
        try
        {
            if (time == TimeSpan.Zero) time = TimeSpan.FromSeconds(3.5);

             var stream = await file.OpenAsync(FileAccessMode.Read);
            bool exactSeek = false;
            CreateCachedFolder();
            var grabber = await FrameGrabber.CreateFromStreamAsync(stream);
            var frame = await grabber
                .ExtractVideoFrameAsync(time, exactSeek);
            var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Cache");


            var savedFile = await folder.CreateFileAsync(13.GenerateRandomStringStatic() + ".jpg", CreationCollisionOption.GenerateUniqueName);


            var oStream = await savedFile.OpenAsync(FileAccessMode.ReadWrite);
            await frame.EncodeAsJpegAsync(oStream);
            oStream.Dispose();

            return savedFile;
        }
        catch (Exception ex)
        {
            ex.PrintException("GetSnapshot");
        }
        return null;
    }
    public static async Task<StorageFile> GetSnapshotFromD3D(this StorageFile file, TimeSpan time, bool detectThumbSize = false)
    {
        try
        {
            if (time == TimeSpan.Zero) time = TimeSpan.FromSeconds(3.5);

            CreateCachedFolder();
            var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Cache");
            Debug.WriteLine(folder.Path);
            var savedFile = await folder.CreateFileAsync(13.GenerateRandomStringStatic() + ".jpg", CreationCollisionOption.GenerateUniqueName);
            var clip = await MediaClip.CreateFromFileAsync(file);
            var composition = new MediaComposition();
            composition.Clips.Add(clip);
            ImageStream thumb;
            if (detectThumbSize)
            {
                var info = await file.GetVideoInfoAsync();
                if (info == null)
                    thumb = await composition.GetThumbnailAsync(time, 1280, 720, VideoFramePrecision.NearestFrame);
                else
                    thumb = await composition.GetThumbnailAsync(time, (int)info.Width, (int)info.Height, VideoFramePrecision.NearestFrame);
            }
            else
                thumb = await composition.GetThumbnailAsync(time, 1280, 720, VideoFramePrecision.NearestFrame);
            var stream = thumb.AsStream();
            var bytes = await stream.ToByteArray();
            await FileIO.WriteBytesAsync(savedFile, bytes);

            return savedFile;
        }
        catch (Exception ex)
        {
            ex.PrintException("GetSnapshot");
        }
        return null;
    }
}

