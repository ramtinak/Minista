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
using Minista.Themes;

namespace Minista
{
    internal static class SettingsHelper
    {
        public static Type GetStoryView() => Settings.StoryViewType == StoryViewType.NewOne ? typeof(Views.Stories.StoryViewX) : typeof(Views.Main.StoryView);
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

                try
                {
                    MinistaThemeCore themeCore = null;
                    MinistaTheme theme = null;
                    switch(Settings.AppTheme)
                    {
                        case AppTheme.Custom:
                            if (Settings.CurrentTheme != null)
                                themeCore = Settings.CurrentTheme;
                            theme = Settings.CurrentTheme?.Theme ?? new MinistaDarkTheme();
                            break;
                        case AppTheme.Light:
                            themeCore = GetMeTheme("Light");
                            theme = new MinistaWhiteTheme();
                            break;
                        case AppTheme.Dark:
                        default:
                            themeCore = GetMeTheme("Dark");
                            theme = new MinistaDarkTheme();
                            break;
                    }
                    if (themeCore == null)
                        themeCore = GetUnkownTheme();
                    themeCore.Theme = theme;
                    Settings.CurrentTheme = themeCore;
                    ThemeHelper.InitTheme(themeCore);
                }
                catch { }
            }
            catch { }
        }
        internal static MinistaThemeCore GetMeTheme(string name)
        {
            return new MinistaThemeCore
            {
                Publisher = new MinistaPublisher
                {
                    Name = $"Minista {name} theme",
                    Publisher = "Ramtin",
                    Version = "1.0.0"
                }
            };
        }
        internal static MinistaThemeCore GetUnkownTheme()
        {
            return new MinistaThemeCore
            {
                Publisher = new MinistaPublisher
                {
                    Name = "Minista Unknown theme",
                    Publisher = "Unknown",
                    Version = "1.0.0"
                }
            };
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
