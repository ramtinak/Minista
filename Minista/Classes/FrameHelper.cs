using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Minista
{
    static class FrameHelper
    {
        public static async Task<TimeSpan> GetDurationAsync(this StorageFile file)
        {
            try
            {
                var basic = await file.GetBasicPropertiesAsync();
                var video = await file.Properties.GetVideoPropertiesAsync();
                return video.Duration;
            }
            catch { }
            return TimeSpan.Zero;
        }
        public static async Task<VideoProperties> GetVideoInfoAsync(this StorageFile file)
        {
            try
            {
                var basic = await file.GetBasicPropertiesAsync();
                return await file.Properties.GetVideoPropertiesAsync();
            }
            catch { }
            return null;
        }
    }
}
