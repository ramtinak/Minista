using InstagramApiSharp.Classes.Models;
using System;
using System.Threading.Tasks;
using static Helper;
using MinistaLivePlayback;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Minista.ViewModels.Broadcast
{
    public class MinistaBroadcasdViewModel : LiveBroadcastViewModel, IDisposable
    {
        #region Properties, Fields

        public MinistaPlayer MinistaPlayer { get; set; }

        #endregion Properties, Fields

        #region ctor

        public MinistaBroadcasdViewModel()
        {
            Timer.Interval = TimeSpan.FromSeconds(3);
            Timer.Tick += TimerTick;
        }

        ~MinistaBroadcasdViewModel() => Dispose();

        #endregion ctor

        #region Player

        public async void Play(MediaElement mediaElement)
        {
            try
            {
                Show();
                MinistaPlayer = new MinistaPlayer();
                await MinistaPlayer.Initialize(new Uri(Broadcast.DashPlaybackUrl), mediaElement);
                mediaElement.CurrentStateChanged += MediaElementCurrentStateChanged;
                Hide();
            }
            catch (Exception ex) { ex.PrintException("LiveBroadcastView.Play"); }
        }

        private void MediaElementCurrentStateChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (sender is MediaElement me && me != null)
                {
                    switch (me.CurrentState)
                    {
                        case MediaElementState.Closed:
                        case MediaElementState.Playing:
                        case MediaElementState.Stopped:
                            Hide();
                            return;
                        case MediaElementState.Paused:
                            Show();
                            return;
                    }
                    switch (me.CurrentState)
                    {
                        case MediaElementState.Closed:
                        case MediaElementState.Stopped:
                            LiveStopped();
                            return;
                    }
                }
            }
            catch { }
        }

        public override void Stop()
        {
            try
            {
                base.Stop();
                MinistaPlayer.MediaPlayer?.Stop();
            }
            catch { }
        }

        public override void LiveStopped()
        {
            try
            {
                base.LiveStopped();
                MinistaPlayer.MediaPlayer?.Stop();
            }
            catch { }
        }

        public override void HardStopLive()
        {
            try
            {
                base.HardStopLive();
                MinistaPlayer.MediaPlayer?.Stop();
            }
            catch { }
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

        void Show()
        {
            Interval = 0;
            Views.Broadcast.LiveBroadcastView.Current?.ShowLoading();
        }
        void Hide()
        {
            if (Views.Broadcast.LiveBroadcastView.Current.IsInLoading)
                Views.Broadcast.LiveBroadcastView.Current?.HideLoading();
        }

        #endregion UI Update

        #region IDisposable
        public async void Dispose()
        {
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Stop();
                    Timer.Stop();
                    Timer = null;
                    MinistaPlayer = null;
                });
            }
            catch { }
        }
        #endregion IDisposable
    }
}
