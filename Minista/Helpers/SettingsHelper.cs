using Minista.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Minista.Helpers;
using Windows.UI.Xaml;
using MinistaHelper;
namespace Minista
{
    internal static class SettingsHelper
    {
        //public static bool IsSomethingChanged { get; set; } = false;
        public static MinistaSettings Settings { get; set; } = new MinistaSettings();

        public static void LoadSettings()
        {
            try
            {
                try
                {
                    Helper.Passcode = Views.Security.Passcode.Build();
                   
                }
                catch { }
                try
                {
                    var obj = ApplicationSettingsHelper.LoadSettingsValue(nameof(Settings));
                    if (obj is string str && !string.IsNullOrEmpty(str))
                    {
                        var settings = JsonConvert.DeserializeObject<MinistaSettings>(str);
                        if (settings != null)
                            Settings = settings;
                    }
                }
                catch { Settings = new MinistaSettings(); }
                try
                {
                    var obj = ApplicationSettingsHelper.LoadSettingsValue(nameof(Helper.InstaApiSelectedUsername));
                    if (obj is string str)
                        Helper.InstaApiSelectedUsername = str;
                }
                catch { }

                try
                {
                    if(Settings.ElementSound)
                        ElementSoundPlayer.State = ElementSoundPlayerState.On;
                }
                catch { }
            }
            catch { }
        }

        public static void SaveSettings()
        {
            try
            {
                //return;
                var json = JsonConvert.SerializeObject(Settings);
                ApplicationSettingsHelper.SaveSettingsValue(nameof(Settings), json);
            }
            catch { }
            try
            {
                ApplicationSettingsHelper.SaveSettingsValue(nameof(Helper.InstaApiSelectedUsername), Helper.InstaApiSelectedUsername);
            }
            catch { }
        }
    }
}
