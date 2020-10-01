using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Helper;
using static CryptoHelper;
using Windows.Storage;
using InstagramApiSharp.Enums;
using Minista.Helpers;

namespace Minista
{
    static class SessionHelper
    {
        const string AccountsPath = "Accounts";
        //const string OldStateFile = "Session.bin";
        //static string StateFile = "Session" + SessionFileType;
        public static readonly StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
        public static bool DontSaveSettings = false;
        static async void CreateFolder()
        {
            try
            {
                await LocalFolder.CreateFolderAsync(AccountsPath, CreationCollisionOption.FailIfExists);
            }
            catch { }
        }
        public static async void DeleteCurrentSession()
        {
            //try
            //{
            //    var file = await LocalFolder.GetFileAsync(OldStateFile);
            //    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            //}
            //catch (Exception ex) { ex.PrintException("DeleteCurrentSession ex"); }
            try
            {
                var file = await LocalFolder.GetFileAsync(InstaApiSelectedUsername + SessionFileType);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception ex) { ex.PrintException("DeleteCurrentSession ex"); }
        }
        //public static async void SaveCurrentSession()
        //{
        //    CreateFolder();
        //    if (InstaApi == null)
        //        return;
        //    if (!InstaApi.IsUserAuthenticated)
        //        return;
        //    try
        //    {
        //        var state = await InstaApi.GetStateDataAsStringAsync();
        //        var file = await LocalFolder.CreateFileAsync(StateFile, CreationCollisionOption.ReplaceExisting);
        //        var content = Encrypt(state);
        //        await FileIO.WriteTextAsync(file, content, Windows.Storage.Streams.UnicodeEncoding.Utf8);
        //    }
        //    catch (Exception ex) { ex.PrintException("SaveCurrentSession ex"); }
        //}

        public static async void SaveCurrentSession()
        {
            CreateFolder();
            if (InstaApi == null)
                return;
            if (!InstaApi.IsUserAuthenticated)
                return;
            try
            {
                var state = await InstaApi.GetStateDataAsStringAsync();
                var file = await LocalFolder.CreateFileAsync(InstaApiSelectedUsername + SessionFileType, CreationCollisionOption.ReplaceExisting);
                var content = Encrypt(state);
                await FileIO.WriteTextAsync(file, content, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }
            catch (Exception ex) { ex.PrintException("SaveCurrentSession ex"); }
        }
        //public static async Task LoadSession()
        //{
        //    try
        //    {
        //        CreateFolder();
        //        LocalFolder.Path.PrintDebug();
        //        var file = await LocalFolder.GetFileAsync(OldStateFile);
        //        var json = await FileIO.ReadTextAsync(file);
        //        if (string.IsNullOrEmpty(json))
        //            return;
        //        var content = Decrypt(json);
        //        content.PrintDebug();
        //        BuildApi();

        //        //await Task.Delay(350);

        //        await InstaApi.LoadStateDataFromStringAsync(content);

        //        return;
        //    }
        //    catch (Exception ex) { ex.PrintException("LoadSession ex"); }

        //    try
        //    {
        //        var file = await LocalFolder.GetFileAsync(StateFile);
        //        var json = await FileIO.ReadTextAsync(file);
        //        if (string.IsNullOrEmpty(json))
        //            return;
        //        var content = Decrypt(json);
        //        content.PrintDebug();
        //        BuildApi();
        //        await InstaApi.LoadStateDataFromStringAsync(content);

        //        return;
        //    }
        //    catch (Exception ex) { ex.PrintException("LoadSession ex"); }
        //}


        public static async Task LoadSessionsAsync()
        {
            try
            {
                LocalFolder.Path.PrintDebug();
                var files = await LocalFolder.GetFilesAsync();
                if (files?.Count > 0)
                {
                    for(int i = 0; i < files.Count;i++)
                    {
                        var item = files[i];
                        if (item.Path.ToLower().EndsWith(SessionFileType))
                        {
                            try
                            {
                                var json = await FileIO.ReadTextAsync(item);
                                if (!string.IsNullOrEmpty(json))
                                {
                                    var content = Decrypt(json);
                                    content.PrintDebug();
                                    var api = BuildApi();
                                    await api.LoadStateDataFromStringAsync(content);
                                    InstaApiList.Add(api);
                                }
                            }
                            catch { }
                        }
                    }
                    if (InstaApiList.Count > 0)
                    {
                        if (string.IsNullOrEmpty(InstaApiSelectedUsername))
                        {
                            InstaApi = InstaApiList[0];


                            InstaApiSelectedUsername = InstaApi.GetLoggedUser().LoggedInUser.UserName.ToLower();
                        }
                        else
                        {
                            if (InstaApiList.Count == 1)
                            {
                                InstaApi = InstaApiList[0];
                            }
                            else
                            {
                                var first = InstaApiList
                                    .FirstOrDefault(x => x.GetLoggedUser()?.LoggedInUser.UserName.ToLower() ==
                                    InstaApiSelectedUsername.ToLower());


                                if (first == null)
                                {
                                    InstaApi = InstaApiList[0];
                                }
                                else
                                {
                                    InstaApi = first;
                                }
                            }
                        }

                        "Loaded users:".PrintDebug();
                        foreach (var item in InstaApiList)
                            item.GetLoggedUser().UserName.ToLower().PrintDebug();

                        CurrentUser = InstaApi.GetLoggedUser().LoggedInUser.ToUserInfo();
                    }

                    //var apis = InstaApiList.ToList();
                    //if (InstaApi != null)
                    //    apis.AddInstaApiX(Helper.InstaApi);
                    //MultiApiHelper.SetupPushNotification(apis);
                }
            }
            catch (Exception ex) { ex.PrintException("LoadSession ex"); }
        }

    }
}
