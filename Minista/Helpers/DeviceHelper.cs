using System.Text;
using Telerik.UI.Automation.Peers;
using Windows.ApplicationModel;
using Windows.Devices.Input;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Minista.Helpers
{
    static class DeviceHelper
    {
        internal const string PackageName = "64775ParseDevInc.Minista";
        internal const string Publisher = "CN=ACB1F19A-470F-487C-A7C7-D50D6F38E651";
        internal const string FamilyName = "64775ParseDevInc.Minista_f8qv91k3qc5fy";
        internal const string PublisherId = "f8qv91k3qc5fy";

        //String output = String.Format("Name: \"{0}\"\n" +
        //                            //"Version: {1}\n" +
        //                            //"Architecture: {2}\n" +
        //                            "ResourceId: \"{1}\"\n" +
        //                            "Publisher: \"{2}\"\n" +
        //                            "PublisherId: \"{3}\"\n" +
        //                            "FullName: \"{4}\"\n" +
        //                            "FamilyName: \"{5}\"\n" +
        //                            "IsFramework: {6}\n",
        //                            packageId.Name,
        //                            //versionString(packageId.Version),
        //                            //architectureString(packageId.Architecture),
        //                            packageId.ResourceId,
        //                            packageId.Publisher,
        //                            packageId.PublisherId,
        //                            packageId.FullName,
        //                            packageId.FamilyName,
        //                            package.IsFramework);
        internal static bool IsThisMinista()
        {
            var flag = false;
            try
            {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                return PackageName == packageId.Name && Publisher == packageId.Publisher &&
                    FamilyName == packageId.FamilyName && PublisherId == packageId.PublisherId;
            }
            catch { }

            return flag;
        }
        public static string GetDeviceInfo()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<--------- Device Information --------->");
                try
                {
                    var deviceInfo = new EasClientDeviceInformation();

                    sb.AppendLine("DeviceName: " + deviceInfo.FriendlyName);
                    sb.AppendLine("OperatingSystem: " + deviceInfo.OperatingSystem);
                }
                catch { }
                // get the system family name
                AnalyticsVersionInfo ai = AnalyticsInfo.VersionInfo;
                sb.AppendLine("SystemFamily: " + ai.DeviceFamily);
                // get the system version number
                string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                ulong v = ulong.Parse(sv);
                ulong v1 = (v & 0xFFFF000000000000L) >> 48;
                ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
                ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
                ulong v4 = (v & 0x000000000000FFFFL);
                sb.AppendLine("SystemVersion: " + $"{v1}.{v2}.{v3}.{v4}");

                // get the package architecure
                Package package = Package.Current;
                sb.AppendLine("SystemArchitecture: " + package.Id.Architecture.ToString());

                // get the user friendly app name
                sb.AppendLine("ApplicationName: " + package.DisplayName);

                // get the app version
                PackageVersion pv = package.Id.Version;
                sb.AppendLine("ApplicationVersion: " + $"{pv.Major}.{pv.Minor}.{pv.Build}.{pv.Revision}");

                // get the device manufacturer and model name
                EasClientDeviceInformation eas = new EasClientDeviceInformation();
                sb.AppendLine("DeviceManufacturer: " + eas.SystemManufacturer);
                sb.AppendLine("DeviceModel: " + eas.SystemProductName);
                sb.AppendLine("<--------- Device Information --------->");
                sb.AppendLine();
                sb.AppendLine();

                return sb.ToString().PrintDebug();
            }
            catch { }
            return string.Empty;
        }
    }
}
