using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.LiveCapture
{
    interface IKeyframeReducerImpl<T> where T : struct
    {
        bool CanReduce(T value, T first, T second, float maxError);
    }

    class KeyframeReducer<T> : IEnumerator<T> where T : struct
    {
        const int kDefaultBufferCapacity = 50;
        const int kMaxSearchWindow = 50;

        IKeyframeReducerImpl<T> m_Impl;
        CircularBuffer<T> m_Buffer = new CircularBuffer<T>(kDefaultBufferCapacity);
        bool m_First;
        bool m_Flushing;
        int m_KeyCount;
        int m_OutputKeyCount;
        readonly int m_MaxSearchWindow = kMaxSearchWindow;

        public float MaxError { get; set; }
        public T Current { get; private set; }
        public float Ratio => m_OutputKeyCount / (float)m_KeyCount;

        object IEnumerator.Current => Current;

        void IDisposable.Dispose() { }

        public KeyframeReducer(IKeyframeReducerImpl<T> impl, int maxSearchWindow = kMaxSearchWindow)
        {
            if (impl == null)
                throw new ArgumentNullException(nameof(impl));

            if (maxSearchWindow <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxSearchWindow));

            m_Impl = impl;
            m_MaxSearchWindow = maxSearchWindow;

            Reset();
        }

        public void Reset()
        {
            m_KeyCount = 0;
            m_OutputKeyCount = 0;
            m_First = true;
            m_Flushing = false;
            m_Buffer.Clear();
        }

        public void Add(T keyframe)
        {
            if (m_Buffer.Count == m_Buffer.Capacity)
            {
                m_Buffer.Capacity = m_Buffer.Capacity * 2;
            }

            m_Buffer.Add(keyframe);

            ++m_KeyCount;
        }

        public void Flush()
        {
            m_Flushing = true;
        }

        public bool IsEmpty()
        {
            return m_Buffer.Count == 0;
        }

        public bool MoveNext()
        {
            if (m_First && m_Buffer.Count > 0)
            {
                m_First = false;

                Current = m_Buffer[0];

                PopUntil(0);

                ++m_OutputKeyCount;

                return true;
            }

            if (m_Buffer.Count < 2 && !m_Flushing)
            {
                return false;
            }

            do
            {
                var index2 = Mathf.Min(m_MaxSearchWindow, m_Buffer.Count - 1);
                var index1 = index2 - 1;
                var keyFound = false;

                while (index1 >= 0)
                {
                    var second = m_Buffer[index2];
                    var canReduce = true;

                    while (canReduce && index1 >= 0)
                    {
                        var test = m_Buffer[index1];

                        canReduce = m_Impl.CanReduce(test, Current, second, MaxError);

                        --index1;
                    }

                    if (!canReduce)
                    {
                        keyFound = true;
                        index2 = index1 + 1;
                    }
                }

                if (!keyFound && m_Flushing && m_Buffer.Count > 0)
                {
                    if (index2 == 0 || index2 == m_Buffer.Count - 1)
                    {
                        keyFound = true;
                    }
                    else if (index2 == m_MaxSearchWindow)
                    {
                        PopUntil(index2);
                    }
                }

                if (keyFound)
                {
                    Current = m_Buffer[index2];

                    PopUntil(index2);

                    ++m_OutputKeyCount;

                    return true;
                }
            } while (m_Flushing && m_Buffer.Count > 0);

            return false;
        }

        void PopUntil(int amount)
        {
            while (amount >= 0)
            {
                m_Buffer.PopFront();

                --amount;
            }
        }
    }
}
