using InstagramApiSharp.Classes.Models;
using Minista.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Minista.UserControls
{
    public sealed partial class VoicePlayerUc : UserControl
    {
        private readonly DispatcherTimer Timer = new DispatcherTimer();
        private InstaDirectInboxItem CurrentDirectInboxItem;
        private ToggleButton VoicePlayPauseButton;
        private ProgressVoice ProgressVoice;
        private bool IsUcLoaded = false;
        public VoicePlayerUc()
        {
            InitializeComponent();
            Timer.Interval = TimeSpan.FromMilliseconds(300);
            Timer.Tick += TimerTick;
            Loaded += VoicePlayerUcLoaded;
        }
        private void VoicePlayerUcLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if(!IsUcLoaded)
                {
                    if (MainPage.Current != null)
                    {
                        ("VoicePlayerUc + MainPage.Current  NULL NABOD").PrintDebug();
                        if (MainPage.Current.ME != null)
                        {
                            MainPage.Current.ME.MediaOpened += MEMediaOpened;
                            MainPage.Current.ME.MediaEnded += MEMediaEnded;
                            MainPage.Current.ME.MediaFailed += MEMediaFailed;
                            MainPage.Current.ME.CurrentStateChanged += MECurrentStateChanged;
                            ("VoicePlayerUc + MainPage.Current.ME  NULL NABOD").PrintDebug();
                        }
                    }
                    else
                        ("VoicePlayerUc + MainPage.Current  NULL BOOOOOOOOOOOOD").PrintDebug();

                    IsUcLoaded = true;
                }
            }
            catch { }
        }

        public async void SetDirectItem(InstaDirectInboxThread thread, InstaDirectInboxItem item, ProgressVoice progressVoice, ToggleButton /*AppBarButton*/ button)
        {
            if (item == null) return;

            try
            {
                FasterModeButton.IsChecked = false;
            }
            catch { }
            try
            {
                if (CurrentDirectInboxItem != null)
                {
                    if (CurrentDirectInboxItem.ItemId == item.ItemId)
                    {
                        try
                        {
                            Visibility = Visibility.Visible;
                        }
                        catch { }
                        if (MainPage.Current.ME.CurrentState == MediaElementState.Paused)
                            Play();
                        else
                            Pause();
                        return;
                    }
                }
                if (VoicePlayPauseButton != null)
                    VoicePlayPauseButton.Tag/*Content*/ = Helper.PlayMaterialIcon;
                ProgressVoice = progressVoice;
                await Task.Delay(200);
                CurrentDirectInboxItem = item;

                VoicePlayPauseButton = button;
                var date = item.TimeStamp;

                var textDate = $"{date.ToString("MMM", CultureInfo.InvariantCulture)} {date.Day} at {date.ToString("hh:mm tt")}";
                txtTime.Text = "00:00";

                if (item.UserId == Helper.CurrentUser.Pk)
                {
                    txtInfo.Text = $"You {textDate}";
                }
                else
                {
                    var user = thread.Users.FirstOrDefault(x => x.Pk == item.UserId);
                    if (user == null)
                        txtInfo.Text = $"{textDate}";
                    else
                    {
                        txtInfo.Text = $"{user.UserName} {textDate}";
                    }
                }

            }
            catch { }

            try
            {
                MainPage.Current.ME.PlaybackRate = 1.00;
            }
            catch { }
            try
            {
                MainPage.Current.ME.Source = new Uri(CurrentDirectInboxItem.VoiceMedia.Media.Audio.AudioSource);
            }
            catch { }
            try
            {
                Visibility = Visibility.Visible;
            }
            catch { }
        }
        public void Play()
        {
            try
            {
                MainPage.Current?.ME.Play();
            }
            catch { }
            try
            {
                Timer.Start();
            }
            catch { }
        }
        public void Pause()
        {
            try
            {
                if (VoicePlayPauseButton != null)
                    VoicePlayPauseButton.Tag/*Content*/= Helper.PlayMaterialIcon;
                MainPage.Current?.ME.Pause();
            }
            catch { }
            try
            {
                Timer.Stop();
            }
            catch { }
        }
        private void Reset()
        {
            try
            {
                MainPage.Current.ME.Stop();

                txtInfo.Text = "";
                txtTime.Text = "00:00";
            }
            catch { }
        }
        private void PlayPauseButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    PlayPauseButton.IsChecked = false;
                }
                catch { }
                if (PlayPauseButton.Tag.ToString() == Helper.PlayMaterialIcon)
                {
                    Play();
                }
                else
                    Pause();
            }
            catch { }
        }

        private void FasterModeButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((int)(MainPage.Current.ME.PlaybackRate) == 1)
                {
                    MainPage.Current.ME.PlaybackRate = 2.00;

                    FasterModeButton.IsChecked = true;
                }
                else
                {
                    MainPage.Current.ME.PlaybackRate = 1.00;

                    FasterModeButton.IsChecked = false;
                }
            }
            catch { }
        }

        private void HideButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Timer.Stop();
                Reset();
            }
            catch { }
            try
            {
                HideButton.IsChecked = false;
            }
            catch { }
            try
            {
                Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        private void TimerTick(object sender, object e)
        {
            try
            {
                var position = MainPage.Current.ME.Position;
                txtTime.Text = $"{position.Minutes.ToString("00")}:{position.Seconds.ToString("00")}";

                try
                {
                    if(ProgressVoice != null)
                    {
                        ProgressVoice.Value = position.TotalMilliseconds;

                    }
                }
                catch { }
            }
            catch { }
        }
        #region MEdia Events
        private void MEMediaOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                Play();

                if (ProgressVoice != null)
                    ProgressVoice.Maximum = MainPage.Current.ME.NaturalDuration.TimeSpan.TotalMilliseconds;
            }
            catch { }
        }

        private void MEMediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (VoicePlayPauseButton != null)
                    VoicePlayPauseButton.Tag/*Content*/ = Helper.PlayMaterialIcon;
            }
            catch { }
            try
            {
                Timer.Stop();
            }
            catch { }
        }

        private void MEMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                Timer.Stop();
            }
            catch { }
            try
            {
                if (VoicePlayPauseButton != null)
                    VoicePlayPauseButton.Tag/*Content*/ = Helper.PlayMaterialIcon;
            }
            catch { }
        }

        private async void MECurrentStateChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    if (MainPage.Current != null)
                    {
                        if (MainPage.Current.ME.CurrentState == MediaElementState.Playing || MainPage.Current.ME.CurrentState == MediaElementState.Buffering ||
                        MainPage.Current.ME.CurrentState == MediaElementState.Opening)
                        {
                            PlayPauseButton.Tag = Helper.PauseMaterialIcon;
                            if (VoicePlayPauseButton != null)
                                VoicePlayPauseButton.Tag/*Content*/ = Helper.PauseMaterialIcon;
                        }
                        else
                        {
                            PlayPauseButton.Tag = Helper.PlayMaterialIcon;

                            if (VoicePlayPauseButton != null)
                                VoicePlayPauseButton.Tag/*Content*/= Helper.PlayMaterialIcon;
                        }
                    }
                });
            }
            catch { }
        }
        #endregion
    }
}
