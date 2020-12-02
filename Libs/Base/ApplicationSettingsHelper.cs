using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

public static class ApplicationSettingsHelper
{
    const string ChangeKey = "changesKey111";
    public static bool GetChanges()
    {
        return LoadSettingsValue(ChangeKey) != null;
    }
    public static void SetChanges()
    {
        SaveSettingsValue(ChangeKey, "Nist");
    }

    public static void RemoveSettingsValue(string key)
    {
        if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            return;
        ApplicationData.Current.LocalSettings.Values.Remove(key);
    }
    public static object LoadSettingsValue(string key)
    {
        if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            return null;
        else
        {
            var value = ApplicationData.Current.LocalSettings.Values[key];

            return value;
        }
    }
    public static void SaveSettingsValue(string key, object value)
    {
        if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            ApplicationData.Current.LocalSettings.Values.Add(key, value);
        else
            ApplicationData.Current.LocalSettings.Values[key] = value;
    }

}
