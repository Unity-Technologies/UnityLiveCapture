using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;

namespace Unity.LiveCapture
{
    class TakeRecorderImpl
    {
        public static TakeRecorderImpl Instance { get; } = new TakeRecorderImpl(LiveCaptureDeviceManager.Instance);

        LiveCaptureDeviceManager m_DeviceManager;
        ITakeRecorderContext m_Context;
        List<LiveCaptureDevice> m_RecordingDevices = new List<LiveCaptureDevice>();
        bool m_IsLive = true;
        double m_RecordingStartTime;
        Playable m_RecordTimer;
        bool m_IsPlayingCached;
        TakeRecorderPlaybackState m_PlaybackState = new TakeRecorderPlaybackState();
        ITakeRecorderContext m_RecordContext;
        Texture2D m_Screenshot;
        internal ITakeRecorderContext RecordContext => m_RecordContext;
        internal ITakeRecorderContext PlaybackContext => m_PlaybackState.Context;
        // For testing purposes only.
        internal bool SkipDeviceReadyCheck { get; set; }
        internal bool SkipProducingAssets { get; set; }
        public bool PlayTakeContents { get; set; }

        public FrameRate FrameRate
        {
            get => LiveCaptureSettings.Instance.FrameRate;
            set => LiveCaptureSettings.Instance.FrameRate = value;
        }

        public ITakeRecorderContext Context
        {
            get
            {
                TryGetContext(out var context);

                return context;
            }
        }

        public void LiveUpdate()
        {
            UpdateRecordingElapsedTime();
            HandleIsLiveChanged();
            HandleRecordingEnded();
            PlaybackChangeCheck();
        }

        public void StartRecording()
        {
            if (!IsRecording
                && CanStartRecording()
                && TryGetContext(out m_RecordContext))
            {
                IsRecording = true;

                TakeScreenshot(ref m_Screenshot);
                RebuildContext();

                m_RecordTimer = StartTimer();
                m_RecordingStartTime = DateTime.Now.TimeOfDay.TotalSeconds;

                UpdateRecordingElapsedTime();
                Play();

                foreach (var device in m_DeviceManager.Devices)
                {
                    Debug.Assert(device != null);

                    device.InvokeStartRecording();
                }

                TakeRecorder.InvokeRecordingStateChange();
            }
        }

        public void StopRecording()
        {
            if (IsRecording)
            {
                IsRecording = false;

                Stop();

                m_RecordingDevices.Clear();

                foreach (var device in m_DeviceManager.Devices)
                {
                    Debug.Assert(device != null);

                    if (device.IsRecording)
                    {
                        m_RecordingDevices.Add(device);
                    }

                    device.InvokeStopRecording();
                }

                Debug.Assert(m_RecordContext != null);

                if (m_RecordContext.IsValid())
                {
                    m_RecordContext.Pause();

                    ProduceTake(m_RecordContext);
                    RebuildContext();
                }

                m_RecordingDevices.Clear();
                m_RecordContext = null;

                StopTimer(ref m_RecordTimer);
                DisposeScreenshot(ref m_Screenshot);
                UpdateRecordingElapsedTime();

                TakeRecorder.InvokeRecordingStateChange();
            }
        }

        public bool IsPlaying()
        {
            if (TryGetContext(out var context))
            {
                return context.IsPlaying();
            }

            return false;
        }

        public void Play()
        {
            if (!m_PlaybackState.IsPlaying() && TryGetContext(out var context))
            {
                var mode = TakeRecorderPlaybackMode.Context;

                if (PlayTakeContents)
                {
                    mode = TakeRecorderPlaybackMode.Contents;
                }

                if (IsRecording)
                {
                    mode = TakeRecorderPlaybackMode.Recording;
                }

                m_PlaybackState.Play(context, mode);

                if (m_PlaybackState.IsPlaying())
                {
                    PlaybackChangeCheck();
                }
            }
        }

        public void GoToBeginning()
        {
            if (TryGetContext(out var context))
            {
                var time = 0d;

                if (PlayTakeContents && context.GetSelectedShot() is Shot shot)
                {
                    var take = shot.Take;

                    if (take != null && take.TryGetContentRange(out var start, out var end))
                    {
                        var timeOffset = shot.TimeOffset;

                        time = start - timeOffset;
                    }
                }

                context.SetTime(time);
            }
        }

        /// <inheritdoc/>
        public void Pause()
        {
            if (TryGetContext(out var context))
            {
                context.Pause();

                PlaybackChangeCheck();
            }
        }

        /// <inheritdoc/>
        public double GetDuration()
        {
            if (TryGetContext(out var context))
            {
                return context.GetDuration();
            }

            return 0d;
        }

        /// <inheritdoc/>
        public double GetTime()
        {
            if (TryGetContext(out var context))
            {
                return context.GetTime();
            }

            return 0d;
        }

        public void SetTime(double time)
        {
            if (TryGetContext(out var context))
            {
                context.SetTime(time);

                PlaybackChangeCheck();
            }
        }

        public void Stop()
        {
            m_PlaybackState.Stop();
        }

        void RebuildContext()
        {
            if (TryGetContext(out var context))
            {
                context.Pause();
                context.Rebuild();
            }
        }

        void ProduceTake(ITakeRecorderContext context)
        {
            if (SkipProducingAssets)
            {
                return;
            }

            Debug.Assert(context != null);

            var index = context.Selection;

            if (context.GetSelectedShot() is not Shot shot)
            {
                return;
            }

            var contextStartTime = shot.TimeOffset;
            var contextDuration = context.GetDuration();
            var recordingStartTime = m_PlaybackState.InitialOffset;
            var recordingDuration = m_RecordTimer.GetTime();
            var resolver = context.GetResolver();

            Debug.Assert(resolver != null);

            using var takeBuilder = new TakeBuilder(
                contextStartTime,
                contextDuration,
                recordingStartTime,
                recordingDuration,
                shot.SceneNumber,
                shot.Name,
                shot.TakeNumber,
                shot.Description,
                shot.Directory,
                DateTime.Now,
                shot.IterationBase,
                FrameRate,
                m_Screenshot,
                resolver);

            using (TimelineDisableUndoScope.Create())
            {
                ProduceTake(takeBuilder);
            }

            var timeline = takeBuilder.Take.Timeline;
            if (timeline != null)
            {
                takeBuilder.Take.Duration = timeline.duration;
            }

            shot.TakeNumber++;

            var take = takeBuilder.Take;

            shot.Take = take;

            context.SetShot(index, shot);

            var director = resolver as PlayableDirector;

            SetDirty(director);
            SetDirty(context.GetShotStorage());
        }

        void SetDirty(Object obj)
        {
#if UNITY_EDITOR
            if (obj != null)
            {
                EditorUtility.SetDirty(obj);
            }
#endif
        }

        void ProduceTake(TakeBuilder takeBuilder)
        {
            foreach (var device in m_RecordingDevices)
            {
                if (device != null && device.isActiveAndEnabled)
                {
                    try
                    {
                        device.Write(takeBuilder);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }
            }
            takeBuilder.AlignTracks(m_RecordingStartTime);
        }

        public void Update()
        {
            if (m_Context != null)
            {
                m_Context.Update();
            }

            m_PlaybackState.Update();
        }

        bool IsDeviceReady(LiveCaptureDevice device)
        {
            return device != null && device.isActiveAndEnabled && device.IsLive && device.IsReady();
        }

        bool AreDevicesReadyForRecording(IEnumerable<LiveCaptureDevice> devices)
        {
            if (SkipDeviceReadyCheck)
            {
                return true;
            }

            foreach (var device in devices)
            {
                if (IsDeviceReady(device))
                {
                    return true;
                }
            }

            return false;
        }

        internal bool CanStartRecording()
        {
            if (IsLive && TryGetContext(out var context) && context.GetSelectedShot() is not null)
            {
                return AreDevicesReadyForRecording(m_DeviceManager.Devices);
            }

            return false;
        }

        static void TakeScreenshot(ref Texture2D texture)
        {
            DisposeScreenshot(ref texture);

            var camera = LiveCaptureUtility.GetTopCamera();

            if (camera != null)
            {
                texture = Screenshot.Take(camera);
            }
        }

        static void DisposeScreenshot(ref Texture2D texture)
        {
            if (texture != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(texture);
                }
                else
                {
                    Object.DestroyImmediate(texture);
                }

                texture = null;
            }
        }

        void PlaybackChangeCheck()
        {
            var isPlaying = IsPlaying();

            if (m_IsPlayingCached != isPlaying)
            {
                m_IsPlayingCached = isPlaying;

                TakeRecorder.InvokePlaybackStateChange();
            }
        }

        void HandleRecordingEnded()
        {
            if (IsRecording)
            {
                if (!CanStartRecording())
                {
                    StopRecording();
                }
                else if (m_RecordContext.IsValid())
                {
                    if (m_RecordContext.GetDuration() > 0d && !m_RecordContext.IsPlaying())
                    {
                        StopRecording();
                    }
                }
                else
                {
                    StopRecording();
                }
            }
        }

        static Playable StartTimer()
        {
            var graph = PlayableGraph.Create("TakeRecorderTimer");
            var output = ScriptPlayableOutput.Create(graph, "Output");
            var playable = Playable.Create(graph);
            var updateMode = Application.isPlaying
                ? DirectorUpdateMode.GameTime
                : DirectorUpdateMode.UnscaledGameTime;

            output.SetSourcePlayable(playable);
            graph.SetTimeUpdateMode(updateMode);
            graph.Play();

            return playable;
        }

        static void StopTimer(ref Playable playable)
        {
            if (!playable.IsValid())
            {
                return;
            }

            var graph = playable.GetGraph();

            if (graph.IsValid())
            {
                graph.Destroy();
            }

            playable = Playable.Null;
        }

        void HandleIsLiveChanged()
        {
            if (m_IsLive != IsLive)
            {
                m_IsLive = IsLive;

                TakeRecorder.InvokeLiveStateChange();

                if (m_IsLive)
                {
                    Stop();
                }
                else
                {
                    StopRecording();
                }
            }
        }

        bool TryGetContext(out ITakeRecorderContext context)
        {
            context = m_Context;

            return context != null;
        }

        /// <summary>
        /// Indicates whether the take recorder is ready for recording.
        /// </summary>
        public bool IsLive
        {
            get => m_DeviceManager.IsLive;
            set => m_DeviceManager.IsLive = value;
        }

        /// <summary>
        /// Checks whether the take recorder is recording or not.
        /// </summary>
        /// <returns>
        /// true if playing; otherwise, false.
        /// </returns>
        public bool IsRecording { get; private set; }

        /// <summary>
        /// The time elapsed since the start of the recording.
        /// </summary>
        public double RecordingElapsedTime { get; internal set; }

        public TakeRecorderImpl(LiveCaptureDeviceManager deviceManager)
        {
            if (deviceManager == null)
            {
                throw new ArgumentNullException(nameof(deviceManager));
            }

            m_DeviceManager = deviceManager;
        }

        public void SetContext(ITakeRecorderContext context)
        {
            if (context != m_Context)
            {
                Pause();
                StopRecording();

                m_Context = context;
            }
        }

        void UpdateRecordingElapsedTime()
        {
            if (m_RecordTimer.IsValid())
            {
                RecordingElapsedTime = m_RecordTimer.GetTime();
            }
        }
    }

    /// <summary>
    /// A take recorder that manages a set of capture devices.
    /// </summary>
    //[HelpURL(Documentation.baseURL + "ref-component-take-recorder" + Documentation.endURL)]
    public static class TakeRecorder
    {
        /// <summary>
        /// TakeRecorder executes this event when live mode has been enabled or disabled.
        /// </summary>
        public static event Action LiveStateChanged;

        /// <summary>
        /// TakeRecorder executes this event when recording has started or stopped.
        /// </summary>
        public static event Action RecordingStateChanged;

        /// <summary>
        /// TakeRecorder executes this event when playback has started or stopped.
        /// </summary>
        public static event Action PlaybackStateChanged;

        internal static TakeRecorderImpl Impl { get; set; } = TakeRecorderImpl.Instance;

        internal static void ClearSubscribers()
        {
            LiveStateChanged = null;
            RecordingStateChanged = null;
            PlaybackStateChanged = null;
        }

        /// <summary>
        /// Indicates whether the take recorder is ready for recording.
        /// </summary>
        public static bool IsLive
        {
            get => Impl.IsLive;
            set => Impl.IsLive = value;
        }

        /// <summary>
        /// Indicates whether or not to play the entire shot or just the range where the
        /// Take contains recorded data.
        /// </summary>
        public static bool PlayTakeContents
        {
            get => Impl.PlayTakeContents;
            set => Impl.PlayTakeContents = value;
        }

        /// <summary>
        /// The frame rate to use for recording.
        /// </summary>
        public static FrameRate FrameRate
        {
            get => Impl.FrameRate;
            set => Impl.FrameRate = value;
        }

        /// <summary>
        /// The current <see cref="Shot"/> to record to.
        /// </summary>
        public static Shot? Shot => Context?.GetSelectedShot();

        internal static ITakeRecorderContext Context => Impl.Context;

        internal static void SetContext(ITakeRecorderContext context) => Impl.SetContext(context);

        /// <summary>
        /// Checks whether the take recorder is recording or not.
        /// </summary>
        /// <returns>
        /// true if playing; otherwise, false.
        /// </returns>
        public static bool IsRecording() => Impl.IsRecording;

        /// <summary>
        /// Starts the recording of a new take for the selected slate.
        /// </summary>
        public static void StartRecording() => Impl.StartRecording();

        /// <summary>
        /// Stops the recording.
        /// </summary>
        public static void StopRecording() => Impl.StopRecording();

        /// <summary>
        /// Returns the time elapsed since the start of the recording.
        /// </summary>
        /// <returns>The time elapsed since the start of the recording, in seconds.</returns>
        public static double GetRecordingElapsedTime() => Impl.RecordingElapsedTime;

        /// <summary>
        /// Checks whether the take recorder is playing the selected take or not.
        /// </summary>
        /// <returns>
        /// true if playing; otherwise, false.
        /// </returns>
        public static bool IsPreviewPlaying() => Impl.IsPlaying();

        /// <summary>
        /// Starts playing the selected take.
        /// </summary>
        public static void PlayPreview() => Impl.Play();

        internal static void GoToBeginning() => Impl.GoToBeginning();

        /// <summary>
        /// Pauses the playback of the selected take.
        /// </summary>
        public static void PausePreview() => Impl.Pause();

        /// <summary>
        /// Returns the current playback duration of the selected take.
        /// </summary>
        /// <returns>The current duration in seconds.</returns>
        public static double GetPreviewDuration() => Impl.GetDuration();

        /// <summary>
        /// Returns the current playback time of the selected take.
        /// </summary>
        /// <returns>The current time in seconds.</returns>
        public static double GetPreviewTime() => Impl.GetTime();

        /// <summary>
        /// Changes the current playback time of the selected take.
        /// </summary>
        /// <param name="time">The current time in seconds.</param>
        public static void SetPreviewTime(double time) => Impl.SetTime(time);

        internal static void InvokeLiveStateChange() => Invoke(LiveStateChanged);

        internal static void InvokeRecordingStateChange() => Invoke(RecordingStateChanged);

        internal static void InvokePlaybackStateChange() => Invoke(PlaybackStateChanged);

        static void Invoke(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
