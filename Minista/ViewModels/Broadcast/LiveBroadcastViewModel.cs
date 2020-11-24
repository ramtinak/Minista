using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Helpers;
using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using static Helper;
namespace Minista.ViewModels.Broadcast
{
    public class LiveBroadcastViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Properties, Fields

        private double _viewerCount = 0;
        private bool _isPlaying = true;
        private bool _isMute = false;
        private bool _isCommentDisabled = false;
        private string _broadcastStatus = null;
        private int Interval = 0;
        private MediaPlayer _mediaPlayer;
        private InstaBroadcast _broadcast;
        public InstaBroadcast Broadcast { get => _broadcast; set => Set(nameof(Broadcast), ref _broadcast, value); }
        public LibVLC LibVLC { get; set; }
        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            private set => Set(nameof(MediaPlayer), ref _mediaPlayer, value);
        }
        public ICommand InitializedCommand { get; }
        private DispatcherTimer Timer = new DispatcherTimer();
        public bool IsPlaying { get => _isPlaying; set => Set(nameof(IsPlaying), ref _isPlaying, value); }
        public bool IsMute { get => _isMute; set { Set(nameof(IsMute), ref _isMute, value);  } }
        public bool IsCommentDisabled { get => _isCommentDisabled; set { Set(nameof(IsCommentDisabled), ref _isCommentDisabled, value); } }
        public double ViewerCount { get => _viewerCount; set { Set(nameof(ViewerCount), ref _viewerCount, value); } }
        public string BroadcastStatus { get => _broadcastStatus; set { Set(nameof(BroadcastStatus), ref _broadcastStatus, value); } }
        public ObservableCollection<InstaBroadcastComment> Items { get; set; } = new ObservableCollection<InstaBroadcastComment>();

        #endregion Properties, Fields

        #region ctor
        public LiveBroadcastViewModel()
        {
            InitializedCommand = new RelayCommand<InitializedEventArgs>(Initialize);
            Timer.Interval = TimeSpan.FromSeconds(3);
            Timer.Tick += TimerTick;
        }

        ~LiveBroadcastViewModel() => Dispose();
        #endregion ctor

        #region Player

        public async void Play()
        {
            try
            {
                Timer.Start();
                await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        Views.Broadcast.LiveBroadcastView.Current?.ShowLoading();
                    }
                    catch { }
                    DateTime.Now.PrintDebug();
                    Media media = new Media(LibVLC, Broadcast.DashPlaybackUrl, FromType.FromLocation);
                    media.AddOption(":sout = #transcode{vcodec=x264,vb=800,scale=0.25,acodec=none}:display :no-sout-rtp-sap :no-sout-standard-sap :ttl=1 :sout-keep :rtsp-mcast");
                    //media.AddOption(":video-filter=rotate{angle=180}");
                    MediaPlayer.Play(media);
                    MediaPlayer.Fullscreen = true;
                    "Media Added to MediaPlayer".PrintDebug();
                    DateTime.Now.PrintDebug();
                });
            }
            catch (Exception ex) { ex.PrintException("LiveBroadcastView.Play"); }
        }

        public void Stop()
        {
            try
            {
                Items?.Clear();
                Timer?.Stop();
                MediaPlayer?.Stop();
            }
            catch { }
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

        #region Ig Calls

        public async void GetHeartbeat()
        {
            try
            {
                var result = await InstaApi.LiveProcessor.GetHeartBeatAndViewerCountAsync(Broadcast.Id);
                if (result.Succeeded)
                {
                    ViewerCount = result.Value.ViewerCount;
                    string status = null;
                    switch(result.Value.BroadcastStatusType)
                    {
                        case InstagramApiSharp.Enums.InstaBroadcastStatusType.Interrupted:
                            status = "Paused";
                            break;
                        case InstagramApiSharp.Enums.InstaBroadcastStatusType.Stopped:
                            status = "LIVE is OVER"; 
                            Timer.Stop();
                            break;
                    }
                    BroadcastStatus = status;
                }
            }
            catch { }
        }
        public async void GetComments()
        {
            try
            {
                var last = Items.LastOrDefault();
                var t = last == null ? 0 : DateTimeHelper.ToUnixTime(last.CreatedAtUtc);
                var result = await InstaApi.LiveProcessor.GetCommentsAsync(Broadcast.Id, t.ToString());
                if (result.Succeeded)
                {
                    result.Value.Comments.ForEach(item =>
                    {
                        Items.Add(item);
                    });
                }
            }
            catch { }
        }

        public async void EndBroadcast()
        {
            try
            {
                await InstaApi.LiveProcessor.GetFinalViewerListAsync(Broadcast.Id);
            }
            catch { }
        }
        #endregion Ig Calls

        #region UI Update

        async void Show()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Interval = 0;
                Views.Broadcast.LiveBroadcastView.Current?.ShowLoading();
            });
        }
        async void Hide()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (Views.Broadcast.LiveBroadcastView.Current.IsInLoading)
                    Views.Broadcast.LiveBroadcastView.Current?.HideLoading();
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

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        private void Set<T>(string propertyName, ref T field, T value)
        {
            if (field == null && value != null || field != null && !field.Equals(value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
        async void OnPropertyChanged(string propertyName)
        {
            await Helper.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, delegate
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        #endregion PropertyChanged
    }
}
