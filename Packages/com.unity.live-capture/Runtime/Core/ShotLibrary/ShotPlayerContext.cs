using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture
{
    class ShotPlayerContext : ITakeRecorderContext
    {
        ShotPlayer m_ShotPlayer;
        ShotLibrary m_Library;
        int m_LibraryVersion;
        Shot[] m_Shots = Array.Empty<Shot>();

        public ShotPlayer ShotPlayer => m_ShotPlayer;

        public int Version => m_LibraryVersion;

        public Shot[] Shots => m_Shots;

        public int Selection
        {
            get => m_ShotPlayer != null ? m_ShotPlayer.Selection : -1;
            set
            {
                if (m_ShotPlayer != null)
                {
                    m_ShotPlayer.Selection = value;
                }

                Rebuild();
            }
        }

        public ShotPlayerContext(ShotPlayer shotPlayer)
        {
            if (shotPlayer == null)
            {
                throw new ArgumentNullException(nameof(shotPlayer));
            }

            m_ShotPlayer = shotPlayer;

            UpdateCacheIfNeeded();
        }

        public void SetShot(int index, Shot shot)
        {
            if (m_Library != null)
            {
                m_Library.SetShot(index, shot);
            }
        }

        public Object GetStorage(int index)
        {
            return m_Library;
        }

        public bool IsValid()
        {
            return m_ShotPlayer != null;
        }

        public void Update()
        {
            UpdateCacheIfNeeded();
        }

        public double GetDuration()
        {
            if (m_ShotPlayer != null)
            {
                return m_ShotPlayer.Director.duration;
            }

            return 0d;
        }

        public double GetTime()
        {
            if (m_ShotPlayer != null)
            {
                return m_ShotPlayer.Director.time;
            }

            return 0d;
        }

        public bool IsPlaying()
        {
            if (m_ShotPlayer != null)
            {
                return PlayableDirectorControls.IsPlaying(m_ShotPlayer.Director);
            }

            return false;
        }

        public void Pause()
        {
            if (m_ShotPlayer != null)
            {
                PlayableDirectorControls.Pause(m_ShotPlayer.Director);
            }
        }

        public void Play()
        {
            if (m_ShotPlayer != null)
            {
                Timeline.SetAsMasterDirector(m_ShotPlayer.Director);
                PlayableDirectorControls.Play(m_ShotPlayer.Director);
            }
        }

        public void SetTime(double value)
        {
            if (m_ShotPlayer != null)
            {
                Timeline.SetAsMasterDirector(m_ShotPlayer.Director);
                PlayableDirectorControls.SetTime(m_ShotPlayer.Director, value);
            }
        }

        public IExposedPropertyTable GetResolver(int index)
        {
            if (m_ShotPlayer != null)
            {
                return m_ShotPlayer.Director;
            }

            return null;
        }

        public void ClearSceneBindings(int index)
        {
            if (m_ShotPlayer != null)
            {
                m_ShotPlayer.ClearSceneBindings();
            }
        }

        public void SetSceneBindings(int index)
        {
            if (m_ShotPlayer != null)
            {
                m_ShotPlayer.SetSceneBindings();
            }
        }

        public void Rebuild(int index)
        {
            Rebuild();
        }

        void Rebuild()
        {
            if (m_ShotPlayer != null)
            {
                m_ShotPlayer.SetupDirectorIfNeeded();

                Timeline.SetAsMasterDirector(m_ShotPlayer.Director);
                Timeline.Repaint();
            }
        }

        void UpdateCacheIfNeeded()
        {
            var library = default(ShotLibrary);

            if (m_ShotPlayer != null)
            {
                library = m_ShotPlayer.ShotLibrary;
            }

            if (m_Library != library)
            {
                m_LibraryVersion = -1;
                m_Library = library;
            }

            if (m_Library == null)
            {
                m_LibraryVersion = -1;
                m_Shots = Array.Empty<Shot>();
            }
            else if (m_LibraryVersion != m_Library.Version)
            {
                m_LibraryVersion = m_Library.Version;
                m_Shots = m_Library.Shots;
            }
        }
    }
}
