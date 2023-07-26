using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.LiveCapture
{
    struct Keyframe<T> where T : struct
    {
        public float Time { get; set; }
        public T Value { get; set; }
        public T InTangent { get; set; }
        public T OutTangent { get; set; }
    }

    interface ITangentUpdaterImpl<T> where T : struct
    {
        T UpdateFirstTangent(in T keyframe, in T nextKeyframe);
        T UpdateLastTangent(in T keyframe, in T prevKeyframe);
        T UpdateTangents(in T keyframe, in T prevKeyframe, in T nextKeyframe);
    }

    class TangentUpdater<T> : IEnumerator<T> where T : struct
    {
        ITangentUpdaterImpl<T> m_Impl;
        Queue<T> m_Keyframes = new Queue<T>();
        bool m_Flushing;
        bool m_First = true;
        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        void IDisposable.Dispose() { }

        public TangentUpdater(ITangentUpdaterImpl<T> impl)
        {
            if (impl == null)
                throw new ArgumentNullException(nameof(impl));

            m_Impl = impl;
        }

        public void Reset()
        {
            m_Keyframes.Clear();
            m_Flushing = false;
            m_First = true;
        }

        public void Add(T keyframe)
        {
            m_Keyframes.Enqueue(keyframe);
        }

        public void Flush()
        {
            m_Flushing = true;
        }

        public bool IsEmpty()
        {
            return m_Keyframes.Count == 0;
        }

        public bool MoveNext()
        {
            if (m_Keyframes.Count == 0)
            {
                return false;
            }

            if (m_Keyframes.Count < 2 && !m_Flushing)
            {
                return false;
            }

            var keyframe = m_Keyframes.Dequeue();

            if (m_First)
            {
                m_First = false;

                if (m_Keyframes.TryPeek(out T nextKeyframe))
                {
                    Current = m_Impl.UpdateFirstTangent(keyframe, nextKeyframe);
                }
                else
                {
                    Current = m_Impl.UpdateFirstTangent(keyframe, keyframe);
                }

                return true;
            }
            else if (m_Keyframes.Count == 0)
            {
                Current = m_Impl.UpdateLastTangent(keyframe, Current);

                return true;
            }
            else if (m_Keyframes.TryPeek(out var nextKeyframe))
            {
                Current = m_Impl.UpdateTangents(keyframe, Current, nextKeyframe);

                return true;
            }

            return false;
        }
    }
}
