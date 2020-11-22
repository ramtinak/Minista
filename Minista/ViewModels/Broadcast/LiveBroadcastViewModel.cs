using InstagramApiSharp.Classes.Models;
using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Minista.ViewModels.Broadcast
{
    public class LiveBroadcastViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool _isPlaying = true;
        private bool _isMute = false;
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
        public bool IsPlaying { get => _isPlaying; set => Set(nameof(IsPlaying), ref _isPlaying, value); }
        public bool IsMute { get => _isMute; set { Set(nameof(IsMute), ref _isMute, value);  } }
        public LiveBroadcastViewModel()
        {
            InitializedCommand = new RelayCommand<InitializedEventArgs>(Initialize);

        }
        ~LiveBroadcastViewModel() => Dispose();


        public async void Play()
        {
            try
            {
                //await Helper.InstaApi.LiveProcessor.joi
                var media = new Media(LibVLC, Broadcast.DashPlaybackUrl, FromType.FromLocation);
                media.AddOption(":sout = #transcode{vcodec=x264,vb=800,scale=0.25,acodec=none}:display :no-sout-rtp-sap :no-sout-standard-sap :ttl=1 :sout-keep :rtsp-mcast");
                MediaPlayer.Play(media);
            }
            catch (Exception ex) { ex.PrintException("LiveBroadcastView.Play"); }
        }

        private void Initialize(InitializedEventArgs eventArgs)
        {
            LibVLC = new LibVLC(eventArgs.SwapChainOptions);
            MediaPlayer = new MediaPlayer(LibVLC)
            {
                //":sout = #transcode{vcodec=x264,vb=800,scale=0.25,acodec=none,fps=23}:display :no-sout-rtp-sap :no-sout-standard-sap :ttl=1 :sout-keep"};
                FileCaching = 0,
                NetworkCaching = 6000,
                EnableHardwareDecoding = true
            };
            MediaPlayer.Playing += MediaPlayer_Playing;
            MediaPlayer.Paused += MediaPlayer_Paused;
        }

        private void MediaPlayer_Paused(object sender, EventArgs e) => IsPlaying = false;
        private void MediaPlayer_Playing(object sender, EventArgs e) => IsPlaying = true;

        public void Dispose()
        {
            var mediaPlayer = MediaPlayer;
            MediaPlayer = null;
            mediaPlayer?.Dispose();
            LibVLC?.Dispose();
            LibVLC = null;
        }


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

    }
}
