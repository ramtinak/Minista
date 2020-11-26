using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using InstagramApiSharp.Helpers;
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
    public class LiveBroadcastViewModel : INotifyPropertyChanged
    {
        #region Properties, Fields

        private double _viewerCount = 0;
        private bool _isPlaying = true;
        private bool _isMute = false;
        private string _broadcastStatus = null;
        internal int Interval = 0;
        private InstaBroadcast _broadcast;
        private Visibility _commentsVisibility = Visibility.Visible;
        private Visibility _commentsDisabledVisibility = Visibility.Collapsed;
        public InstaBroadcast Broadcast { get => _broadcast; set => Set(nameof(Broadcast), ref _broadcast, value); }
        public ICommand InitializedCommand { get; set; }
        internal DispatcherTimer Timer = new DispatcherTimer();
        public bool IsPlaying { get => _isPlaying; set => Set(nameof(IsPlaying), ref _isPlaying, value); }
        public bool IsMute { get => _isMute; set { Set(nameof(IsMute), ref _isMute, value);  } }
        public double ViewerCount { get => _viewerCount; set { Set(nameof(ViewerCount), ref _viewerCount, value); } }
        public string BroadcastStatus { get => _broadcastStatus; set { Set(nameof(BroadcastStatus), ref _broadcastStatus, value); } }
        public Visibility CommentsVisibility { get => _commentsVisibility; set { Set(nameof(CommentsVisibility), ref _commentsVisibility, value); } }
        public Visibility CommentsDisabledVisibility { get => _commentsDisabledVisibility; set { Set(nameof(CommentsDisabledVisibility), ref _commentsDisabledVisibility, value); } }
        public ObservableCollection<InstaBroadcastComment> Items { get; set; } = new ObservableCollection<InstaBroadcastComment>();

        #endregion Properties, Fields

        #region Player

        public virtual void Stop()
        {
            try
            {
                Items?.Clear();
                Timer?.Stop();
            }
            catch { }
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
                    DetermineLiveStatus(Broadcast.Id, result.Value.BroadcastStatusType);
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
                    CommentsVisibility = result.Value.CommentMuted ? Visibility.Collapsed : Visibility.Visible;
                    CommentsDisabledVisibility = !result.Value.CommentMuted ? Visibility.Collapsed : Visibility.Visible;
                    result.Value.Comments.ForEach(item =>
                    {
                        Items.Add(item);
                    });
                }
            }
            catch { }
        }

        public void EndBroadcast()
        {
            //try
            //{
            //    // This should be call from a broadcast owner, not anyone else
            //    await InstaApi.LiveProcessor.GetFinalViewerListAsync(Broadcast.Id);
            //}
            //catch { }
        }

        #endregion Ig Calls

        #region UI Update

        public async Task SetBroadcast(InstaBroadcast broadcast)
        {
            Broadcast = broadcast;
            if (broadcast.BroadcastOwner == null)
                await SetBroadcast(broadcast.Id);
        }

        public async Task SetBroadcast(string broadcastId)
        {
            if (!string.IsNullOrEmpty(broadcastId))
            {
                var result = await InstaApi.LiveProcessor.GetInfoAsync(broadcastId);
                if (result.Succeeded)
                {
                    var broadcast = new InstaBroadcast
                    {
                        Id = result.Value.Id.ToString(),
                        DashAbrPlaybackUrl = result.Value.DashAbrPlaybackUrl,
                        DashManifest = result.Value.DashManifest,
                        DashPlaybackUrl = result.Value.DashPlaybackUrl,
                        BroadcastOwner = result.Value.BroadcastOwner
                    };
                    Broadcast = broadcast;
                }
            }
        }
        public void Reset()
        {
            ViewerCount = 0;
            CommentsVisibility = Visibility.Visible;
            CommentsDisabledVisibility = Visibility.Collapsed;
            Interval = 0;
            BroadcastStatus = null;
        }

        public void DetermineLiveStatus(string id, InstaBroadcastStatusType statusType)
        {
            try
            {
                if (Broadcast == null) return;
                if (Broadcast.Id == id)
                {
                    switch (statusType)
                    {
                        case InstaBroadcastStatusType.Interrupted:
                            LivePaused();
                            break;
                        case InstaBroadcastStatusType.Stopped:
                            LiveStopped();
                            break;
                        case InstaBroadcastStatusType.Active:
                            ActiveLive();
                            break;
                        case InstaBroadcastStatusType.HardStop:
                            HardStopLive();
                            break;
                    }
                }
            }
            catch(Exception ex) { ex.PrintException("LiveBroadcastViewModel.DetermineLiveStatus"); }
        }

        public void ActiveLive()
        {
            BroadcastStatus = null;
        }
        public void LivePaused()
        {
            BroadcastStatus = "Paused";
        }
        public virtual void LiveStopped()
        {
            try
            {
                Timer.Stop();

                BroadcastStatus = "LIVE is OVER";
            }
            catch { }
        }
        public virtual void HardStopLive()
        {
            try
            {
               Timer.Stop();
               ShowNotify(BroadcastStatus = $"They Kicked you OUT, You won't be able to come back to @{Broadcast.BroadcastOwner.UserName.ToLower()}'s lives.", 4500);
            }
            catch { }
        }

        #endregion UI Update

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        internal void Set<T>(string propertyName, ref T field, T value)
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
