using InstagramApiSharp.Classes.Models;
using Minista.Helpers.Uploaders;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.System;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Minista.Controls
{
    public class ChatRecordButton : GlyphToggleButton
    {
        private DispatcherTimer _timer;
        public InstaDirectInboxThread CurrentThread;
        private DateTime _start;

        public TimeSpan Elapsed => DateTime.Now - _start;

        public bool IsRecording => recordingAudioVideo;
        public bool IsLocked => recordingLocked;
        internal static StorageFolder localFolder;

        public ChatRecordButton()
        {
            DefaultStyleKey = typeof(ChatRecordButton);
            ClickMode = ClickMode.Press;
            Click += OnClick;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _timer.Tick += (s, args) =>
            {
                _timer.Stop();
                RecordAudioVideoRunnable();
            };
            Loaded += async(e, a) =>
            {
                 try
                 {
                     localFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Cache");
                 }
                 catch { }
            };
            Recorder.Current.RecordingStarted += Current_RecordingStarted;
            Recorder.Current.RecordingStopped += Current_RecordingStopped;
            Recorder.Current.RecordingFailed += Current_RecordingStopped;
        }

        private void RecordAudioVideoRunnable()
        {
            calledRecordRunnable = true;
            recordAudioVideoRunnableStarted = false;
            Recorder.Current.Start(CurrentThread);
            UpdateRecordingInterface();
        }

        private void Current_RecordingStarted(object sender, EventArgs e)
        {
            if (!recordingAudioVideo)
            {
                recordingAudioVideo = true;
                UpdateRecordingInterface();

                if (enqueuedLocking)
                {
                    LockRecording();
                }
            }
        }

        private void Current_RecordingStopped(object sender, EventArgs e)
        {
            if (recordingAudioVideo)
            {
                // cancel typing
                recordingAudioVideo = false;
                UpdateRecordingInterface();
            }
        }

        private int recordInterfaceState;

        private DisplayRequest _request;

        private void UpdateRecordingInterface()
        {

            if (recordingLocked && recordingAudioVideo)
            {
                if (recordInterfaceState == 2)
                {
                    return;
                }
                recordInterfaceState = 2;

                this.BeginOnUIThread(() =>
                {
                    VisualStateManager.GoToState(this, "Locked", false);

                    ClickMode = ClickMode.Press;
                    RecordingLocked?.Invoke(this, EventArgs.Empty);
                });
            }
            else if (recordingAudioVideo)
            {
                if (recordInterfaceState == 1)
                {
                    return;
                }
                recordInterfaceState = 1;
                try
                {
                    if (_request == null)
                    {
                        _request = new DisplayRequest();
                        _request.GetType();
                    }
                }
                catch { }

                recordingLocked = false;

                _start = DateTime.Now;

                this.BeginOnUIThread(() =>
                {
                    VisualStateManager.GoToState(this, "Started", false);

                    ClickMode = ClickMode.Release;
                    RecordingStarted?.Invoke(this, EventArgs.Empty);
                });
            }
            else
            {
                if (_request != null)
                {
                    try
                    {
                        _request.RequestRelease();
                        _request = null;
                    }
                    catch { }
                }
                if (recordInterfaceState == 0)
                {
                    return;
                }
                recordInterfaceState = 0;

                recordingLocked = false;

                this.BeginOnUIThread(() =>
                {
                    VisualStateManager.GoToState(this, "Stopped", false);

                    ClickMode = ClickMode.Press;
                    RecordingStopped?.Invoke(this, EventArgs.Empty);
                });
            }

        }

        private  void OnClick(object sender, RoutedEventArgs e)
        {
            if (ClickMode == ClickMode.Press)
            {
                if (recordingLocked)
                {
                    if (!hasRecordVideo || calledRecordRunnable)
                    {
                        Recorder.Current.Stop( false);
                        recordingAudioVideo = false;
                        UpdateRecordingInterface();
                    }

                    return;
                }

                ClickMode = ClickMode.Release;



                _timer.Stop();

                if (hasRecordVideo)
                {

                    calledRecordRunnable = false;
                    recordAudioVideoRunnableStarted = true;
                    _timer.Start();
                }
                else
                {
                    RecordAudioVideoRunnable();
                }
            }
            else
            {
                ClickMode = ClickMode.Press;

                OnRelease();
            }
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            CapturePointer(e.Pointer);
            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            ReleasePointerCapture(e.Pointer);


            OnRelease();
        }

        protected override void OnPointerCanceled(PointerRoutedEventArgs e)
        {
            base.OnPointerCanceled(e);
            ReleasePointerCapture(e.Pointer);

            //Logger.Debug(Target.Recording, "OnPointerCanceled");

            OnRelease();
        }

        protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
        {
            base.OnPointerCaptureLost(e);

            //Logger.Debug(Target.Recording, "OnPointerCaptureLost");

            OnRelease();
        }

        private void OnRelease()
        {
            if (recordingLocked)
            {
                //Logger.Debug(Target.Recording, "Recording is locked, abort");
                return;
            }
            if (recordAudioVideoRunnableStarted)
            {
                _timer.Stop();
            }
            else if (!hasRecordVideo || calledRecordRunnable)
            {

                Recorder.Current.Stop(false);
                recordingAudioVideo = false;
                UpdateRecordingInterface();
            }
        }

        private async Task<bool> CheckDeviceAccessAsync()
        {
            var access = DeviceAccessInformation.CreateFromDeviceClass(DeviceClass.AudioCapture );
            if (access.CurrentStatus == DeviceAccessStatus.Unspecified)
            {
                MediaCapture capture = null;
                try
                {
                    capture = new MediaCapture();
                    var settings = new MediaCaptureInitializationSettings
                    {
                        StreamingCaptureMode = StreamingCaptureMode.Audio
                    };
                    await capture.InitializeAsync(settings);
                }
                catch { }
                finally
                {
                    if (capture != null)
                    {
                        capture.Dispose();
                        capture = null;
                    }
                }

                return false;
            }
            else if (access.CurrentStatus != DeviceAccessStatus.Allowed)
            {
                //var message = audio
                //    ? mode == ChatRecordMode.Voice
                //    ? Strings.Resources.PermissionNoAudio
                //    : Strings.Resources.PermissionNoAudioVideo
                //    : Strings.Resources.PermissionNoCamera;

                //this.BeginOnUIThread(async () =>
                //{
                //    var confirm = await TLMessageDialog.ShowAsync(message, Strings.Resources.AppName, Strings.Resources.PermissionOpenSettings, Strings.Resources.OK);
                //    if (confirm == ContentDialogResult.Primary)
                //    {
                //        await Launcher.LaunchUriAsync(new Uri("ms-settings:appsfeatures-app"));
                //    }
                //});

                return false;
            }

            return true;
        }

        private bool hasRecordVideo = false;

        private bool calledRecordRunnable;
        private bool recordAudioVideoRunnableStarted;

        private bool recordingAudioVideo;

        private bool recordingLocked;
        private bool enqueuedLocking;

        public void CancelRecording()
        {
            Recorder.Current.Stop(true);
            recordingAudioVideo = false;
            UpdateRecordingInterface();
        }

        public void LockRecording()
        {
            enqueuedLocking = false;
            recordingLocked = true;
            UpdateRecordingInterface();
        }

        public void ToggleRecording()
        {
            if (recordingLocked)
            {
                if (!hasRecordVideo || calledRecordRunnable)
                {
                    Recorder.Current.Stop( false);
                    recordingAudioVideo = false;
                    UpdateRecordingInterface();
                }
            }
            else
            {
                enqueuedLocking = true;
                RecordAudioVideoRunnable();
            }
        }

        public event EventHandler RecordingStarted;
        public event EventHandler RecordingStopped;
        public event EventHandler RecordingLocked;

        class Recorder
        {
            public event EventHandler RecordingFailed;
            public event EventHandler RecordingStarted;
            public event EventHandler RecordingStopped;
            public event EventHandler RecordingTooShort;

            [ThreadStatic]
            private static Recorder _current;
            public static Recorder Current => _current = _current ?? new Recorder();

            private readonly ConcurrentQueueWorker _recordQueue;

            private Mp3Recorder _recorder;
            private StorageFile _file;
            private DateTime _start;
            private InstaDirectInboxThread CurrentThread;

            public Recorder()
            {
                _recordQueue = new ConcurrentQueueWorker(1);
            }

            public async void Start(InstaDirectInboxThread thread)
            {
                CurrentThread = thread;
                await _recordQueue.Enqueue(async () =>
                {

                    if (_recorder != null)
                    {

                        RecordingFailed?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    var fileName = string.Format("voice_{0:yyyy}-{0:MM}-{0:dd}_{0:HH}-{0:mm}-{0:ss}.mp3", DateTime.Now);
                    var cache = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

                    try
                    {
                        _file = cache;
                        _recorder = new Mp3Recorder(cache)
                        {
                            m_mediaCapture = new MediaCapture()
                        };

                        await _recorder.m_mediaCapture.InitializeAsync(/*_recorder.settings*/);


                        await _recorder.StartAsync();


                        _start = DateTime.Now;
                    }
                    catch (Exception ex)
                    {

                        _recorder.Dispose();
                        _recorder = null;

                        _file = null;

                        RecordingFailed?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    RecordingStarted?.Invoke(this, EventArgs.Empty);
                });
            }


            public async void Stop(bool cancel)
            {
                await _recordQueue.Enqueue(async () =>
                {

                    var recorder = _recorder;
                    var file = _file;

                    if (recorder == null || file == null)
                    {
                        return;
                    }

                    RecordingStopped?.Invoke(this, EventArgs.Empty);

                    var now = DateTime.Now;
                    var elapsed = now - _start;
                    await recorder.StopAsync();

                    if (cancel || elapsed.TotalMilliseconds < 700)
                    {
                        try
                        {
                            await file.DeleteAsync();
                        }
                        catch { }


                        if (elapsed.TotalMilliseconds < 700)
                        {
                            RecordingTooShort?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        Send(file);
                    }

                    _recorder = null;
                    _file = null;
                });
            }

            private async void Send(StorageFile file)
            {
                try
                {
                    //viewModel.Dispatcher.Dispatch(async () =>
                    //{
                    //    await viewModel.SendVoiceNoteAsync(file, duration, null);
                    //});
                    //Transcode(file);
                    try
                    {
                        await Helper.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            Uploader.UploadSingleVoice(file, CurrentThread.ThreadId);
                        });
                    }
                    catch { }
                }
                catch { }

            }




            /*readonly*/ MediaTranscoder _Transcoder = new MediaTranscoder();
            MediaEncodingProfile _Profile;
            CancellationTokenSource _cts;
            StorageFile _OutputFile;
            async void Transcode(StorageFile input)
            {
                //try
                {
                    _Transcoder = new MediaTranscoder();
                    _cts = new CancellationTokenSource();
                    _Profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
                    _OutputFile = await localFolder.CreateFileAsync(13.GenerateRandomStringStatic() + ".mp4", CreationCollisionOption.GenerateUniqueName);

                    var preparedTranscodeResult = await _Transcoder.PrepareFileTranscodeAsync(input, _OutputFile, _Profile);
                    //_Transcoder.VideoProcessingAlgorithm = MediaVideoProcessingAlgorithm.Default;
                    if (preparedTranscodeResult.CanTranscode)
                    {
                        var progress = new Progress<double>(TranscodeProgress);
                        await preparedTranscodeResult.TranscodeAsync().AsTask(_cts.Token, progress);
                    }
                    else
                    {
                        ("Failed to start.\r\nError: " + preparedTranscodeResult.FailureReason).PrintDebug();
                    }
                }
                //catch { }
            }
            VoiceUploader Uploader = new VoiceUploader();

          async  void TranscodeProgress(double percent)
            {
                try
                {
                    Debug.WriteLine($"{(int)percent}%");
                    if ((int)percent == 100)
                    {
                        try
                        {
                           await Helper.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                Uploader.UploadSingleVoice(_OutputFile, CurrentThread.ThreadId);
                            });
                        }
                        catch { }
                    }
                }
                catch { }
            }
            internal sealed class Mp3Recorder
            {
                private StorageFile m_file;
                //private LowLagMediaRecording m_lowLag;
                public MediaCapture m_mediaCapture;
                //public MediaCaptureInitializationSettings settings;

                public StorageFile File
                {
                    get { return m_file; }
                }

                public Mp3Recorder(StorageFile file)
                {
                    m_file = file;
                    InitializeSettings();
                }
                private void InitializeSettings()
                {
                    //settings = new MediaCaptureInitializationSettings
                    //{
                    //    //MediaCategory = MediaCategory.Speech,
                    //    //AudioProcessing = AudioProcessing.Raw,
                    //    //MemoryPreference = MediaCaptureMemoryPreference.Auto,
                    //    //SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                    //    StreamingCaptureMode = StreamingCaptureMode.Audio
                    //};
                }

                public async Task StartAsync()
                {
                    {
                        var mp3Profile = MediaEncodingProfile.CreateM4a(AudioEncodingQuality.Medium);
                        await m_mediaCapture.StartRecordToStorageFileAsync(mp3Profile, m_file);
                    }
                }

                public async Task StopAsync()
                {
                    try
                    {
                        //if (m_lowLag != null)
                        //{
                        //    await m_lowLag.StopAsync();
                        //    await m_lowLag.FinishAsync();
                        //}
                        //else
                        {
                            await m_mediaCapture.StopRecordAsync();
                        }

                        m_mediaCapture.Dispose();
                        m_mediaCapture = null;
                    }
                    catch { }
                }

                public void Dispose()
                {
                    try
                    {
                        //m_lowLag = null;

                        m_mediaCapture.Dispose();
                        m_mediaCapture = null;
                    }
                    catch { }
                }
            }
        }
    }

    public class ConcurrentQueueWorker
    {
        private ConcurrentQueue<Func<Task>> taskQueue = new ConcurrentQueue<Func<Task>>();
#pragma warning disable IDE0044 // Add readonly modifier
        private ManualResetEvent mre = new ManualResetEvent(true);
#pragma warning restore IDE0044 // Add readonly modifier
        private int _concurrentCount = 1;
        private readonly object o = new object();

        /// <summary>
        /// Max Task Count we can run concurrently
        /// </summary>
        public int MaxConcurrentCount { get; private set; }

        public ConcurrentQueueWorker(int maxConcurrentCount)
        {
            MaxConcurrentCount = maxConcurrentCount;
        }

        /// <summary>
        /// Add task into the queue and run it.
        /// </summary>
        /// <param name="tasks"></param>
        public Task Enqueue(Func<Task> task)
        {
            taskQueue.Enqueue(task);

            mre.WaitOne();

            return Task.Run(async () =>
            {
                while (true)
                {
                    if (taskQueue.Count > 0 && MaxConcurrentCount >= _concurrentCount)
                    {
                        var nextTaskAction = default(Func<Task>);
                        if (taskQueue.TryDequeue(out nextTaskAction))
                        {
                            Interlocked.Increment(ref _concurrentCount);

                            await nextTaskAction();

                            lock (o)
                            {
                                mre.Reset();
                                Interlocked.Decrement(ref _concurrentCount);
                                mre.Set();
                            }

                            break;
                        }
                    }
                }
            });
        }
    }

}
