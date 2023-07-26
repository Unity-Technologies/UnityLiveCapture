namespace Unity.LiveCapture
{
    enum TakeRecorderPlaybackMode
    {
        None,
        Context,
        Contents,
        Recording
    }

    class TakeRecorderPlaybackState
    {
        ITakeRecorderContext m_Context;
        TakeRecorderPlaybackMode m_Mode;
        bool m_IsPlaying;
        double m_PlaybackStart;
        double m_PlaybackEnd;
        double m_PlaybackInitialOffset;

        public ITakeRecorderContext Context => m_Context;
        public double InitialOffset => m_PlaybackInitialOffset;

        public bool IsPlaying()
        {
            return IsValid() && m_Context.IsPlaying();
        }

        public void Play(ITakeRecorderContext context, TakeRecorderPlaybackMode mode)
        {
            m_Context = context;
            m_Mode = mode;

            Validate();
            Play();
        }

        public void Stop()
        {
            if (!IsValid() || !m_Context.IsPlaying())
            {
                return;
            }

            var time = m_PlaybackStart;

            if (m_Mode == TakeRecorderPlaybackMode.Recording)
            {
                time = m_PlaybackInitialOffset;
            }

            m_Context.SetTime(time);
            m_Context = null;

            Validate();
        }

        public void Update()
        {
            if (!IsValid() || !m_Context.IsPlaying())
            {
                m_Context = null;

                return;
            }

            if (m_Context.GetTime() >= m_PlaybackEnd)
            {
                Stop();
            }
        }

        bool IsValid()
        {
            return m_Context != null && m_Context.IsValid();
        }

        void Validate()
        {
            if (!IsValid())
            {
                m_Mode = TakeRecorderPlaybackMode.None;
            }
            else if (m_Mode == TakeRecorderPlaybackMode.Contents
                && m_Context.GetSelectedShot() is Shot shot && shot.Take == null)
            {
                m_Mode = TakeRecorderPlaybackMode.Context;
            }
        }

        void Play()
        {
            if (!IsValid() || m_Mode == TakeRecorderPlaybackMode.None)
            {
                return;
            }

            var start = 0d;
            var end = m_Context.GetDuration();

            if (m_Mode == TakeRecorderPlaybackMode.Contents
                && m_Context.GetSelectedShot() is Shot shot)
            {
                var take = shot.Take;

                if (take != null && take.TryGetContentRange(out var rangeStart, out var rangeEnd))
                {
                    var timeOffset = shot.TimeOffset;

                    start = rangeStart - timeOffset;
                    end = rangeEnd - timeOffset;
                }
            }

            var time = m_Context.GetTime();

            if (time < start || time > end)
            {
                time = start;
            }

            m_PlaybackInitialOffset = time;
            m_PlaybackStart = start;
            m_PlaybackEnd = end;

            m_Context.SetTime(time);
            m_Context.Play();
        }
    }
}
