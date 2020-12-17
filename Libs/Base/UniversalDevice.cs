/*
 * Developer: Ramtin Jokar [ Ramtinak@live.com ] [ My Telegram Account: https://t.me/ramtinak ]
 * 
 * Copyright (c) December, 2020
 * 
 * IRANIAN DEVELOPERS
 * 
 */

using InstagramApiSharp.Classes.Android.DeviceInfo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.Graphics.Display;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace Base
{
    public class UniversalDevice : AndroidDevice
    {
        public UniversalDevice()
        {
            var rnd = new Random();
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            var deviceGuid = deviceInfo.Id;
            AndroidVer = AndroidVersion.GetAndroid9();
            DeviceGuid = deviceGuid;
            DeviceId = ApiRequestMessage.GenerateDeviceIdFromGuid(deviceGuid);
            PhoneGuid = GetPhoneGuid(deviceGuid);
            FamilyDeviceGuid = GetFamilyDeviceGuid(deviceGuid);
            PushDeviceGuid = GetPushDeviceGuid(deviceGuid);
            PigeonSessionId = GetPigeonSessionId(deviceGuid);
            IGBandwidthSpeedKbps = $"{rnd.Next(1233, 1567).ToString(CultureInfo.InvariantCulture)}.{rnd.Next(100, 999).ToString(CultureInfo.InvariantCulture)}";
            IGBandwidthTotalTimeMS = rnd.Next(781, 999).ToString(CultureInfo.InvariantCulture);
            try
            {
                IGBandwidthTotalBytesB = ((int)((double.Parse(IGBandwidthSpeedKbps, CultureInfo.InvariantCulture) * double.Parse(IGBandwidthTotalTimeMS, CultureInfo.InvariantCulture)) + rnd.Next(100, 999))).ToString();
            }
            catch { }
            var deviceModel = GetDeviceModelIfPossible(deviceInfo.SystemProductName);
            var displayInfo = DisplayInformation.GetForCurrentView();
            var dpi = displayInfo.LogicalDpi + "dpi";
            var height = displayInfo.ScreenHeightInRawPixels;
            var width = displayInfo.ScreenWidthInRawPixels;
            var resolution = height < width ? $"{height}x{width}" : $"{width}x{height}";
            var id = deviceGuid.ToString().Split('-')[1];
            AndroidBoardName = GetBoardNameIfPossible(deviceInfo.SystemProductName);
            DeviceBrand = deviceInfo.SystemManufacturer;
            HardwareManufacturer = deviceInfo.SystemManufacturer;
            DeviceModel = deviceModel;
            DeviceModelIdentifier = deviceModel;
            FirmwareBrand = GetFirmwareBrandIfPossible(deviceInfo.SystemProductName);
            Resolution = resolution;
            Dpi = dpi;
            HardwareModel = id.Substring(1 , 2) + id.Substring(2) + id.Substring(0, 2);
        }
        string GetBoardNameIfPossible(string device)
        {
            if (device.Contains(' '))
                return device.Split(' ')[0];
            return device;
        }
        string GetDeviceModelIfPossible(string device)
        {
            if (device.Contains(' '))
                return device.Split(' ')[1].ToUpper();
            return new string(device.ToCharArray().Reverse().ToArray()).ToUpper();
        }
        string GetFirmwareBrandIfPossible(string device)
        {
            if (device.Contains(' '))
                return new string(device.Split(' ')[1].ToCharArray().Reverse().ToArray()).ToUpper();
            return new string(device.ToCharArray().Reverse().ToArray()).ToUpper();
        }
        protected Guid GetPhoneGuid(Guid guid)
        {
            var builder = new List<string>();
            var spl = guid.ToString().Split('-');
            foreach (var item in spl)
                builder.Add(new string(item.ToCharArray().Reverse().ToArray()));
            return new Guid(string.Join("-", builder));
        }
        protected Guid GetFamilyDeviceGuid(Guid guid)
        {
            var builder = new List<string>();
            var spl = guid.ToString().Split('-');
            foreach (var item in spl)
                builder.Add(new string(item.ToCharArray().Reverse().ToArray()));

            var part1 = builder[1];
            var part2 = builder[2];
            var part3 = builder[3];

            builder[0] = builder[0].Substring(4) + builder[0].Substring(0, 4);
            builder[1] = part3.Substring(2, 2) + part1.Substring(1, 2);
            builder[2] = part1.Substring(2, 2) + part2.Substring(1, 2);
            builder[3] = part2.Substring(2, 2) + part3.Substring(1, 2);
            builder[4] = builder[4].Substring(6) + builder[4].Substring(0, 6);
            return new Guid(string.Join("-", builder));
        }
        protected Guid GetPushDeviceGuid(Guid guid)
        {
            var builder = new List<string>();
            var spl = guid.ToString().Split('-');
            foreach (var item in spl)
                builder.Add(new string(item.ToCharArray().Reverse().ToArray()));

            var part1 = builder[1];
            var part2 = builder[2];
            var part3 = builder[3];
            builder[0] = builder[0].Substring(6, 2) + builder[0].Substring(2, 2) + builder[0].Substring(4, 2) + builder[0].Substring(0, 2);
            builder[1] = part2.Substring(1, 2) + part2.Substring(0, 2);
            builder[2] = part3.Substring(2) + part2.Substring(0, 2);
            builder[3] = part1.Substring(1, 2) + part1.Substring(0, 2);
            builder[4] = builder[4].Substring(9, 2) + builder[4].Substring(0, 2) + builder[4].Substring(8, 2) +
                builder[4].Substring(6, 2) + builder[4].Substring(1, 2) + builder[4].Substring(4, 2);
            return new Guid(string.Join("-", builder));
        }
        protected Guid GetPigeonSessionId(Guid guid)
        {
            var builder = new List<string>();
            var spl = guid.ToString().Split('-');
            foreach (var item in spl)
                builder.Add(new string(item.ToCharArray().Reverse().ToArray()));

            var part1 = builder[1];
            var part2 = new string(builder[2].ToCharArray().Reverse().ToArray());
            var part3 = builder[3];
            builder[0] = builder[0].Substring(4, 2) + builder[0].Substring(2, 2) + builder[0].Substring(6, 2) + builder[0].Substring(0, 2);
            builder[1] = part2.Substring(1, 2) + part2.Substring(1, 2);
            builder[2] = part2.Substring(1, 2) + part3.Substring(1, 2);
            builder[3] = part1.Substring(2) + part1.Substring(2);
            builder[4] = builder[4].Substring(0, 2) + builder[4].Substring(4, 2) + builder[4].Substring(6, 2) +
                builder[4].Substring(8, 2) + builder[4].Substring(1, 2) + builder[4].Substring(9, 2);
            return new Guid(string.Join("-", builder));
        }
    }
}
