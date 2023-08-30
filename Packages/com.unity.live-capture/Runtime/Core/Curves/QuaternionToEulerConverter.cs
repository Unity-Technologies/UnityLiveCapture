using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.LiveCapture
{
    class QuaternionToEulerConverter : IEnumerator<Keyframe<Vector3>>
    {
        Queue<Keyframe<Quaternion>> m_Keyframes = new Queue<Keyframe<Quaternion>>();
        bool m_Flushing;
        bool m_First = true;
        public Keyframe<Vector3> Current { get; private set; }

        object IEnumerator.Current => Current;

        void IDisposable.Dispose() { }

        public void Reset()
        {
            m_Keyframes.Clear();
            m_Flushing = false;
            m_First = true;
        }

        public void Add(Keyframe<Quaternion> keyframe)
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

                if (m_Keyframes.TryPeek(out Keyframe<Quaternion> nextKeyframe))
                {
                    var idt = nextKeyframe.Time - keyframe.Time;
                    var odt = nextKeyframe.Time - keyframe.Time;

                    Current = Calculate(keyframe, idt, odt, Current.Value);
                }
                else
                {
                    Current = Calculate(keyframe, 0f, 0f, Current.Value);
                }

                return true;
            }
            else if (m_Keyframes.Count == 0)
            {
                var idt = keyframe.Time - Current.Time;
                var odt = keyframe.Time - Current.Time;

                Current = Calculate(keyframe, idt, odt, Current.Value);

                return true;
            }
            else if (m_Keyframes.TryPeek(out var nextKeyframe))
            {
                var idt = keyframe.Time - Current.Time;
                var odt = nextKeyframe.Time - keyframe.Time;

                Current = Calculate(keyframe, idt, odt, Current.Value);

                return true;
            }

            return false;
        }

        static Keyframe<Vector3> Calculate(in Keyframe<Quaternion> keyframe, float idt, float odt, in Vector3 hint)
        {
            var quat = keyframe.Value;
            var iquat = quat.Add(keyframe.InTangent.Mul(idt / 3f));
            var oquat = quat.Add(keyframe.OutTangent.Mul(odt / 3f));

            quat.Normalize();
            iquat.Normalize();
            oquat.Normalize();

            var euler = quat.eulerAngles;
            var ieuler = iquat.eulerAngles;
            var oeuler = oquat.eulerAngles;
            var inTangent = Vector3.zero;
            var outTangent = Vector3.zero;

            for (var c = 0; c < 3; ++c)
            {
                ieuler[c] = Mathf.Repeat(ieuler[c] - euler[c] + 180f, 360f) + euler[c] - 180f;
                inTangent[c] = 3f * (ieuler[c] - euler[c]) / idt;

                oeuler[c] = Mathf.Repeat(oeuler[c] - euler[c] + 180f, 360f) + euler[c] - 180f;
                outTangent[c] = 3f * (oeuler[c] - euler[c]) / odt;
            }

            return new Keyframe<Vector3>()
            {
                Time = keyframe.Time,
                Value = MathUtility.ClosestEuler(quat, hint),
                InTangent = inTangent,
                OutTangent = outTangent
            };
        }
    }
}
