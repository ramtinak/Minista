using Minista.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minista.Classes
{
    public class MinistaSettings
    {
        public LivePlaybackType LivePlaybackType { get; set; } = LivePlaybackType.LibVLC;
        public HeaderPosition HeaderPosition { get; set; } = HeaderPosition.Top;
        public bool GhostMode { get; set; } = false;
        public bool AskedAboutPosition { get; set; } = false;
        public bool ElementSound { get; set; } = false;
        public bool RemoveAds { get; set; } = true;
        public bool DownloadLocationChanged { get; set; } = false;
        public double LockControlX { get; set; } = 0;
        public double LockControlY { get; set; } = 120;
        public bool HandleTelegramLinks { get; set; } = false;

        public MinistaThemeCore CurrentTheme { get; set; } = new MinistaThemeCore
        {
            Theme = new MinistaDarkTheme()
        };

        public AppTheme AppTheme { get; set; } = AppTheme.Dark;

        public DownloadQuality DownloadQuality { get; set; } = DownloadQuality.HighestQuality;

        public StoryViewType StoryViewType { get; set; } = StoryViewType.NewOne;
    } 
    public enum HeaderPosition
    {
        Top,
        Bottom
    }
    public enum LivePlaybackType
    {
        Minista,
        LibVLC
    }
    public enum AppTheme
    {
        Dark,
        Light,
        Custom
    }
    public enum DownloadQuality
    {
        HighestQuality,
        LowestQuality
    }
    public enum StoryViewType
    {
        NewOne,
        OldOne
    }
}
