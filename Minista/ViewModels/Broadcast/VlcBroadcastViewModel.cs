using InstagramApiSharp.Classes.Models;
using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using static Helper;

namespace Minista.ViewModels.Broadcast
{
    public class VlcBroadcastViewModel : LiveBroadcastViewModel, IDisposable
    {
        #region Properties, Fields

        private MediaPlayer _mediaPlayer;
        public LibVLC LibVLC { get; set; }
        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            private set => Set(nameof(MediaPlayer), ref _mediaPlayer, value);
        }

        #endregion Properties, Fields

        #region ctor

        public VlcBroadcastViewModel() 
        {
            Timer.Interval = TimeSpan.FromSeconds(3);
            Timer.Tick += TimerTick;
            InitializedCommand = new RelayCommand<InitializedEventArgs>(Initialize);
        }

        ~VlcBroadcastViewModel() => Dispose();

        #endregion ctor

        #region Player

        public async void Play()
        {
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, /*async*/() =>
                {
                    Show();
                    Media media = new Media(LibVLC, Broadcast.DashPlaybackUrl, FromType.FromLocation);
                    //var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("record.mp4", CreationCollisionOption.GenerateUniqueName);
                    //string faToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                    //("faToken: " + faToken).PrintDebug();
                    //media.AddOption(":sout=#file{dst=" + file.Path + "}");
                    media.AddOption(":sout = #transcode{vcodec=x264,vb=800,scale=0.25,acodec=none}:display :no-sout-rtp-sap :no-sout-standard-sap :ttl=1 :sout-keep :rtsp-mcast");
                    //media.AddOption(":sout-keep");

                    MediaPlayer.Play(media);
                    MediaPlayer.Fullscreen = true;
                });
            }
            catch (Exception ex) { ex.PrintException("VlcLiveBroadcastView.Play"); }
        }

        public override void Stop()
        {
            try
            {
                base.Stop();
                MediaPlayer?.Stop();
            }
            catch { }
        }

        public override void LiveStopped()
        {
            try
            {
                base.LiveStopped();
                MediaPlayer?.Stop();
            }
            catch { }
        }

        public override void HardStopLive()
        {
            try
            {
                base.HardStopLive();
                MediaPlayer?.Stop();
            }
            catch { }
        }

        private void MediaPlayerBuffering(object sender, MediaPlayerBufferingEventArgs e) => Show();

        private void MediaPlayerPaused(object sender, EventArgs e)
        {
            Show();
            IsPlaying = false;
        }

        private void MediaPlayerPlaying(object sender, EventArgs e)
        {
            IsPlaying = true;
            Hide();
        }

        private void Initialize(InitializedEventArgs eventArgs)
        {
            LibVLC = new LibVLC(eventArgs.SwapChainOptions);
            MediaPlayer = new MediaPlayer(LibVLC)
            {
                FileCaching = 0,
                NetworkCaching = 6000,
                EnableHardwareDecoding = true
            };
            MediaPlayer.Playing += MediaPlayerPlaying;
            MediaPlayer.Paused += MediaPlayerPaused;
            MediaPlayer.Buffering += MediaPlayerBuffering;
        }

        private void TimerTick(object sender, object e)
        {
            if (Interval > 5)
            {
                Hide();
                Interval = 0;
            }
            Interval++;
            GetHeartbeat();
            GetComments();
        }
        #endregion Player

        #region UI Update

        async void Show()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Interval = 0;
                Views.Broadcast.VlcLiveBroadcastView.Current?.ShowLoading();
            });
        }
        async void Hide()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (Views.Broadcast.VlcLiveBroadcastView.Current.IsInLoading)
                    Views.Broadcast.VlcLiveBroadcastView.Current?.HideLoading();
            });
        }

        #endregion UI Update

        #region IDisposable 
        public async void Dispose()
        {
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    MediaPlayer mediaPlayer = MediaPlayer;
                    Stop();
                    Timer.Stop();
                    Timer = null;
                    MediaPlayer = null;
                    mediaPlayer?.Dispose();
                    LibVLC?.Dispose();
                    LibVLC = null;
                });
            }
            catch { }
        }
        #endregion IDisposable
    }
}
