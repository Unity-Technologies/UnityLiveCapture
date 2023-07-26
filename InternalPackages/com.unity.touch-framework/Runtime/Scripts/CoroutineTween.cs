/*
 * This code is provided as is without warranty, guarantee of function,
 * or provided support
 */

/* Example usage: move an object from (0, 0, 0) to (1, 1, 1) over 1s
    void Start()
    {
        PositionTween positionTween = new PositionTween
        {
            startPosition = Vector3.zero,
            targetPosition = Vector3.one,
            duration = 1f
        };

        positionTween.AddOnChangedCallback(MoveObject);

        var tweenRunner = new TweenRunner<PositionTween>();
        tweenRunner.Init(this);
        tweenRunner.StartTween(positionTween);
    }

    public void MoveObject( Vector3 position )
    {
        transform.position = position;
    }

    OR use extension method

    TweenPosition(transform, Vector3.zero, Vector3.one, 1f);
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    // Base interface for tweeners, using an interface instead of an abstract class as we want the tweens to be structs.
    public interface ITweenValue
    {
        void TweenValue(float floatPercentage);
        bool ignoreTimeScale { get; }
        float duration { get; }
        bool ValidOnChangedTarget();
    }

    // Float tween class, receives the
    // TweenValue callback and then sets
    // the value on the target.
    internal struct FloatTween : ITweenValue
    {
        public class FloatTweenCallback : UnityEvent<float> {}

        private FloatTweenCallback m_Target;
        private float m_StartValue;
        private float m_TargetValue;
        UnityEvent m_OnCompleteTarget;

        private float m_Duration;
        private bool m_IgnoreTimeScale;

        public float startValue
        {
            get { return m_StartValue; }
            set { m_StartValue = value; }
        }

        public float targetValue
        {
            get { return m_TargetValue; }
            set { m_TargetValue = value; }
        }

        public float duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }

        public bool ignoreTimeScale
        {
            get { return m_IgnoreTimeScale; }
            set { m_IgnoreTimeScale = value; }
        }

        public void TweenValue(float floatPercentage)
        {
            if (!ValidOnChangedTarget())
                return;

            var newValue = Mathf.Lerp(m_StartValue, m_TargetValue, floatPercentage);
            m_Target.Invoke(newValue);

            if (m_OnCompleteTarget != null && Math.Abs(floatPercentage - 1f) < Mathf.Epsilon)
            {
                m_OnCompleteTarget.Invoke();
            }
        }

        public void AddOnChangedCallback(UnityAction<float> callback)
        {
            if (m_Target == null)
                m_Target = new FloatTweenCallback();

            m_Target.AddListener(callback);
        }

        public void AddOnCompleteCallback(UnityAction callback)
        {
            if (m_OnCompleteTarget == null)
                m_OnCompleteTarget = new UnityEvent();

            m_OnCompleteTarget.AddListener(callback);
        }

        public bool ValidOnChangedTarget()
        {
            return m_Target != null;
        }
    }

    internal struct PositionTween : ITweenValue
    {
        public class PositionTweenCallback : UnityEvent<Vector3> {}

        PositionTweenCallback m_OnChangedTarget;
        UnityEvent m_OnCompleteTarget;
        Vector3 m_StartPosition;
        Vector3 m_TargetPosition;

        float m_Duration;
        bool m_IgnoreTimeScale;

        AnimationCurve m_CustomAnimationCurve;

        public Vector3 startPosition
        {
            get { return m_StartPosition; }
            set { m_StartPosition = value; }
        }

        public Vector3 targetPosition
        {
            get { return m_TargetPosition; }
            set { m_TargetPosition = value; }
        }

        public float duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }

        public bool ignoreTimeScale
        {
            get { return m_IgnoreTimeScale; }
            set { m_IgnoreTimeScale = value; }
        }

        public void TweenValue(float floatPercentage)
        {
            if (!ValidOnChangedTarget())
                return;

            var newPosition = Vector3.Lerp(m_StartPosition, m_TargetPosition, floatPercentage);

            m_OnChangedTarget.Invoke(newPosition);

            if (m_OnCompleteTarget != null && Math.Abs(floatPercentage - 1f) < Mathf.Epsilon)
            {
                m_OnCompleteTarget.Invoke();
            }
        }

        public void AddOnChangedCallback(UnityAction<Vector3> callback)
        {
            if (m_OnChangedTarget == null)
                m_OnChangedTarget = new PositionTweenCallback();

            m_OnChangedTarget.AddListener(callback);
        }

        public void AddOnCompleteCallback(UnityAction callback)
        {
            if (m_OnCompleteTarget == null)
                m_OnCompleteTarget = new UnityEvent();

            m_OnCompleteTarget.AddListener(callback);
        }

        public bool ValidOnChangedTarget()
        {
            return m_OnChangedTarget != null;
        }
    }

    internal struct RotationTween : ITweenValue
    {
        public class RotationTweenCallback : UnityEvent<Quaternion> {}

        RotationTweenCallback m_OnChangedTarget;
        UnityEvent m_OnCompleteTarget;
        Quaternion m_StartRotation;
        Quaternion m_TargetRotation;

        float m_Duration;
        bool m_IgnoreTimeScale;

        AnimationCurve m_CustomAnimationCurve;


        public Quaternion startRotation
        {
            get { return m_StartRotation; }
            set { m_StartRotation = value; }
        }

        public Quaternion targetRotation
        {
            get { return m_TargetRotation; }
            set { m_TargetRotation = value; }
        }

        public float duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }

        public bool ignoreTimeScale
        {
            get { return m_IgnoreTimeScale; }
            set { m_IgnoreTimeScale = value; }
        }

        public void TweenValue(float floatPercentage)
        {
            if (!ValidOnChangedTarget())
                return;

            var newRotation = Quaternion.Slerp(m_StartRotation, m_TargetRotation, floatPercentage);

            m_OnChangedTarget.Invoke(newRotation);

            if (m_OnCompleteTarget != null && Math.Abs(floatPercentage - 1f) < Mathf.Epsilon)
            {
                m_OnCompleteTarget.Invoke();
            }
        }

        public void AddOnChangedCallback(UnityAction<Quaternion> callback)
        {
            if (m_OnChangedTarget == null)
                m_OnChangedTarget = new RotationTweenCallback();

            m_OnChangedTarget.AddListener(callback);
        }

        public void AddOnCompleteCallback(UnityAction callback)
        {
            if (m_OnCompleteTarget == null)
                m_OnCompleteTarget = new UnityEvent();

            m_OnCompleteTarget.AddListener(callback);
        }

        public bool ValidOnChangedTarget()
        {
            return m_OnChangedTarget != null;
        }

        public bool ValidOnCompleteTarget()
        {
            return m_OnCompleteTarget != null;
        }
    }

    internal struct ScaleTween : ITweenValue
    {
        public class ScaleTweenCallback : UnityEvent<Vector3> {}

        private ScaleTweenCallback m_OnChangedTarget;
        private UnityEvent m_OnCompleteTarget;
        private Vector3 m_StartScale;
        private Vector3 m_TargetScale;

        private float m_Duration;
        private bool m_IgnoreTimeScale;

        private AnimationCurve m_CustomAnimationCurve;

        public Vector3 startScale
        {
            get { return m_StartScale; }
            set { m_StartScale = value; }
        }

        public Vector3 targetScale
        {
            get { return m_TargetScale; }
            set { m_TargetScale = value; }
        }

        public float duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }

        public bool ignoreTimeScale
        {
            get { return m_IgnoreTimeScale; }
            set { m_IgnoreTimeScale = value; }
        }

        public void TweenValue(float floatPercentage)
        {
            if (!ValidOnChangedTarget())
                return;

            var newScale = Vector3.Lerp(m_StartScale, m_TargetScale, floatPercentage);

            m_OnChangedTarget.Invoke(newScale);

            if (m_OnCompleteTarget != null && Math.Abs(floatPercentage - 1f) < Mathf.Epsilon)
            {
                m_OnCompleteTarget.Invoke();
            }
        }

        public void AddOnChangedCallback(UnityAction<Vector3> callback)
        {
            if (m_OnChangedTarget == null)
                m_OnChangedTarget = new ScaleTweenCallback();

            m_OnChangedTarget.AddListener(callback);
        }

        public void AddOnCompleteCallback(UnityAction callback)
        {
            if (m_OnCompleteTarget == null)
                m_OnCompleteTarget = new UnityEvent();

            m_OnCompleteTarget.AddListener(callback);
        }

        public bool ValidOnChangedTarget()
        {
            return m_OnChangedTarget != null;
        }

        public bool ValidOnCompleteTarget()
        {
            return m_OnCompleteTarget != null;
        }
    }

    // Color tween class, receives the TweenValue callback and then sets the value on the target.
    internal struct ColorTween : ITweenValue
    {
        public enum ColorTweenMode
        {
            All,
            RGB,
            Alpha
        }

        public class ColorTweenCallback : UnityEvent<Color> {}

        ColorTweenCallback m_OnChangedTarget;
        UnityEvent m_OnCompleteTarget;
        Color m_StartColor;
        Color m_TargetColor;
        ColorTweenMode m_TweenMode;

        float m_Duration;
        bool m_IgnoreTimeScale;

        public Color startColor
        {
            get { return m_StartColor; }
            set { m_StartColor = value; }
        }

        public Color targetColor
        {
            get { return m_TargetColor; }
            set { m_TargetColor = value; }
        }

        public ColorTweenMode tweenMode
        {
            get { return m_TweenMode; }
            set { m_TweenMode = value; }
        }

        public float duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }

        public bool ignoreTimeScale
        {
            get { return m_IgnoreTimeScale; }
            set { m_IgnoreTimeScale = value; }
        }

        public void TweenValue(float floatPercentage)
        {
            if (!ValidOnChangedTarget())
                return;

            var newColor = Color.Lerp(m_StartColor, m_TargetColor, floatPercentage);

            if (m_TweenMode == ColorTweenMode.Alpha)
            {
                newColor.r = m_StartColor.r;
                newColor.g = m_StartColor.g;
                newColor.b = m_StartColor.b;
            }
            else if (m_TweenMode == ColorTweenMode.RGB)
            {
                newColor.a = m_StartColor.a;
            }
            m_OnChangedTarget.Invoke(newColor);

            if ((m_OnCompleteTarget != null) && Math.Abs(floatPercentage - 1f) < Mathf.Epsilon)
            {
                m_OnCompleteTarget.Invoke();
            }
        }

        public void AddOnChangedCallback(UnityAction<Color> callback)
        {
            if (m_OnChangedTarget == null)
                m_OnChangedTarget = new ColorTweenCallback();

            m_OnChangedTarget.AddListener(callback);
        }

        public void AddOnCompleteCallback(UnityAction callback)
        {
            if (m_OnCompleteTarget == null)
                m_OnCompleteTarget = new UnityEvent();

            m_OnCompleteTarget.AddListener(callback);
        }

        public bool ValidOnChangedTarget()
        {
            return m_OnChangedTarget != null;
        }
    }

    public enum EaseType
    {
        Linear,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
    }

    public enum TweenLoopType
    {
        Clamp,
        Loop,
        PingPong,
    }

    /// <summary>
    /// Tween runner, executes the given tween. The coroutine will live within the given behaviour container.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TweenRunner<T> where T : struct, ITweenValue // changed from internal to public
    {
        protected MonoBehaviour m_CoroutineContainer;
        protected IEnumerator m_Tween;
        protected EaseType m_EaseType;
        protected TweenLoopType m_TweenLoopType = TweenLoopType.Clamp;

        protected bool m_Paused;
//        protected bool m_PlayBackwards;

        /// <summary>
        /// Utility function for starting the tween private static IEnumerator Start( T tweenInfo )
        /// </summary>
        /// <param name="tweenInfo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private IEnumerator Start(T tweenInfo)
        {
            if (!tweenInfo.ValidOnChangedTarget())
                yield break;

            var elapsedTime = 0.0f;
            while (elapsedTime < tweenInfo.duration)
            {
                if (m_Paused)
                {
                    yield return null;
                    continue;
                }

                elapsedTime += tweenInfo.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                var t = Mathf.Clamp01(elapsedTime / tweenInfo.duration);

//                if (m_PlayBackwards)
//                    t = 1 - t;

                switch (m_EaseType)
                {
                    case EaseType.Linear:
                        break;
                    case EaseType.EaseInSine:
                        t = -Mathf.Cos(t * 0.5f * Mathf.PI) + 1f;
                        break;
                    case EaseType.EaseOutSine:
                        t = Mathf.Sin(t * 0.5f * Mathf.PI);
                        break;
                    case EaseType.EaseInOutSine:
                        t = -0.5f * (Mathf.Cos(t * Mathf.PI) - 1f);
                        break;
                    case EaseType.EaseInQuad:
                        t = t * t;
                        break;
                    case EaseType.EaseOutQuad:
                        t = -t * (t - 2f);
                        break;
                    case EaseType.EaseInOutQuad:
                        t *= 2f;
                        if (t < 1f)
                            t = t * t * 0.5f;
                        else
                        {
                            t -= 1f;
                            t = -0.5f * (t * (t - 2f) - 1f);
                        }
                        break;
                    case EaseType.EaseInCubic:
                        t = t * t * t;
                        break;
                    case EaseType.EaseOutCubic:
                        t -= 1f;
                        t = t * t * t + 1f;
                        break;
                    case EaseType.EaseInOutCubic:
                        t *= 2f;
                        if (t < 1)
                            t = 0.5f * t * t * t;
                        t -= 2f;
                        t = 0.5f * (t * t * t + 2f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                tweenInfo.TweenValue(t);

                yield return null;
            }
            tweenInfo.TweenValue(1.0f);
        }

        public void Init(MonoBehaviour coroutineContainer)
        {
            m_CoroutineContainer = coroutineContainer;
        }

        public void StartTween(T info, EaseType easeType = EaseType.Linear, bool startImmedietely = true,
            TweenLoopType tweenLoopType = TweenLoopType.Clamp)
        {
            if (m_CoroutineContainer == null)
            {
                Debug.LogWarning("Coroutine container not configured... did you forget to call Init?");
                return;
            }

            m_Paused = false;

            m_EaseType = easeType;

            if (m_Tween != null)
            {
                m_CoroutineContainer.StopCoroutine(m_Tween);
                m_Tween = null;
            }

            if (!m_CoroutineContainer.gameObject.activeInHierarchy)
            {
                info.TweenValue(1.0f);
                return;
            }

            m_TweenLoopType = tweenLoopType;

            m_Tween = Start(info);

            if (startImmedietely)
                m_CoroutineContainer.StartCoroutine(m_Tween);
        }

        public void PlayTween()
        {
            if (m_Tween != null && m_CoroutineContainer != null && m_CoroutineContainer.gameObject.activeInHierarchy)
            {
                m_CoroutineContainer.StartCoroutine(m_Tween);
            }
        }

        public void RestartTween()
        {
            if (m_Tween != null && m_CoroutineContainer != null)
            {
                m_CoroutineContainer.StartCoroutine(m_Tween);
            }
        }

        public void StopTween()
        {
            if (m_Tween != null)
            {
                m_CoroutineContainer.StopCoroutine(m_Tween);
            }
        }

        public void PauseTween()
        {
            m_Paused = true;
        }

        public void ResumeTween()
        {
            m_Paused = false;
        }
    }

    internal static class TweenExtensionMethods
    {
        public static TweenRunner<PositionTween> TweenPosition(this MonoBehaviour mb, Transform transform,
            Vector3 fromPosition, Vector3 toPosition,
            float duration, EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false,
            UnityAction onCompleteCallback = null)
        {
            var positionTween = new PositionTween
            {
                startPosition = fromPosition,
                targetPosition = toPosition,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            positionTween.AddOnChangedCallback(newPosition => transform.position = newPosition);

            if (onCompleteCallback != null)
                positionTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<PositionTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(positionTween, easeType);

            return tweenRunner;
        }

        public static TweenRunner<PositionTween> TweenPosition(this MonoBehaviour mb, Transform transform,
            Vector3 toPosition,
            float duration, EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false,
            UnityAction onCompleteCallback = null)
        {
            var positionTween = new PositionTween
            {
                startPosition = mb.transform.position,
                targetPosition = toPosition,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            positionTween.AddOnChangedCallback(newPosition => transform.position = newPosition);

            if (onCompleteCallback != null)
                positionTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<PositionTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(positionTween, easeType);

            return tweenRunner;
        }

        public static TweenRunner<PositionTween> TweenLocalPosition(this MonoBehaviour mb, Transform transform,
            Vector3 fromPosition, Vector3 toPosition,
            float duration, EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false,
            UnityAction onCompleteCallback = null)
        {
            var positionTween = new PositionTween
            {
                startPosition = fromPosition,
                targetPosition = toPosition,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            positionTween.AddOnChangedCallback(newPosition => transform.localPosition = newPosition);

            if (onCompleteCallback != null)
                positionTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<PositionTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(positionTween, easeType);

            return tweenRunner;
        }

        public static TweenRunner<PositionTween> TweenLocalPosition(this MonoBehaviour mb, Transform transform,
            Vector3 toPosition,
            float duration, EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false,
            UnityAction onCompleteCallback = null)
        {
            var positionTween = new PositionTween
            {
                startPosition = mb.transform.position,
                targetPosition = toPosition,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            positionTween.AddOnChangedCallback(newPosition => transform.localPosition = newPosition);

            if (onCompleteCallback != null)
                positionTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<PositionTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(positionTween, easeType);

            return tweenRunner;
        }

        public static TweenRunner<ScaleTween> TweenScale(this MonoBehaviour mb, Transform transform, Vector3 fromScale, Vector3 toScale,
            float duration, EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false,
            UnityAction onCompleteCallback = null)
        {
            var scaleTween = new ScaleTween
            {
                startScale = fromScale,
                targetScale = toScale,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            scaleTween.AddOnChangedCallback(newScale => transform.localScale = newScale);

            if (onCompleteCallback != null)
                scaleTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<ScaleTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(scaleTween, easeType);

            return tweenRunner;
        }

        public static TweenRunner<ScaleTween> TweenScale(this MonoBehaviour mb, Transform transform, Vector3 toScale,
            float duration, EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false,
            UnityAction onCompleteCallback = null)
        {
            var scaleTween = new ScaleTween
            {
                startScale = transform.localScale,
                targetScale = toScale,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            scaleTween.AddOnChangedCallback(newScale => transform.localScale = newScale);

            if (onCompleteCallback != null)
                scaleTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<ScaleTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(scaleTween, easeType);

            return tweenRunner;
        }

        public static TweenRunner<RotationTween> TweenRotation(this MonoBehaviour mb, Transform transform, Quaternion fromRotation, Quaternion toRotation,
            float duration, EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false,
            UnityAction onCompleteCallback = null)
        {
            var rotationTween = new RotationTween
            {
                startRotation = fromRotation,
                targetRotation = toRotation,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            rotationTween.AddOnChangedCallback(newRotation => transform.rotation = newRotation);

            if (onCompleteCallback != null)
                rotationTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<RotationTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(rotationTween, easeType);

            return tweenRunner;
        }

        public static TweenRunner<RotationTween> TweenRotation(this MonoBehaviour mb, Transform transform, Quaternion toRotation,
            float duration, EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false,
            UnityAction onCompleteCallback = null)
        {
            var rotationTween = new RotationTween
            {
                startRotation = transform.rotation,
                targetRotation = toRotation,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            rotationTween.AddOnChangedCallback(newRotation => transform.rotation = newRotation);

            if (onCompleteCallback != null)
                rotationTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<RotationTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(rotationTween, easeType);

            return tweenRunner;
        }

        public static TweenRunner<RotationTween> TweenLocalRotation(this MonoBehaviour mb, Transform transform, Quaternion fromRotation, Quaternion toRotation,
            float duration, EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false,
            UnityAction onCompleteCallback = null)
        {
            var rotationTween = new RotationTween
            {
                startRotation = fromRotation,
                targetRotation = toRotation,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            rotationTween.AddOnChangedCallback(newRotation => transform.localRotation = newRotation);

            if (onCompleteCallback != null)
                rotationTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<RotationTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(rotationTween, easeType);

            return tweenRunner;
        }

        public static TweenRunner<RotationTween> TweenLocalRotation(this MonoBehaviour mb, Transform transform, Quaternion toRotation,
            float duration, EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false,
            UnityAction onCompleteCallback = null)
        {
            var rotationTween = new RotationTween
            {
                startRotation = transform.localRotation,
                targetRotation = toRotation,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            rotationTween.AddOnChangedCallback(newRotation => transform.localRotation = newRotation);

            if (onCompleteCallback != null)
                rotationTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<RotationTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(rotationTween, easeType);

            return tweenRunner;
        }

        public static TweenRunner<ColorTween> TweenColor(this MonoBehaviour mb, Material material, Color fromColor,
            Color toColor, float duration, ColorTween.ColorTweenMode colorTweenMode = ColorTween.ColorTweenMode.All,
            EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false, UnityAction onCompleteCallback = null)
        {
            var colorTween = new ColorTween
            {
                startColor = fromColor,
                targetColor = toColor,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            colorTween.AddOnChangedCallback(newColor => material.color = newColor);

            if (onCompleteCallback != null)
                colorTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<ColorTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(colorTween, easeType);

            return tweenRunner;
        }

        public static TweenRunner<ColorTween> TweenColor(this MonoBehaviour mb, Material material, Color toColor,
            float duration, EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false,
            UnityAction onCompleteCallback = null)
        {
            var colorTween = new ColorTween
            {
                startColor = material.color,
                targetColor = toColor,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            colorTween.AddOnChangedCallback(newColor => material.color = newColor);

            if (onCompleteCallback != null)
                colorTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<ColorTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(colorTween, easeType);

            return tweenRunner;
        }

        public static TweenRunner<ColorTween> TweenColor(this MonoBehaviour mb, Graphic graphic, Color toColor,
            float duration, EaseType easeType = EaseType.Linear, bool ignoreTimeScale = false,
            UnityAction onCompleteCallback = null)
        {
            var colorTween = new ColorTween
            {
                startColor = graphic.color,
                targetColor = toColor,
                duration = duration,
                ignoreTimeScale = ignoreTimeScale
            };

            colorTween.AddOnChangedCallback(newColor => graphic.color = newColor);

            if (onCompleteCallback != null)
                colorTween.AddOnCompleteCallback(onCompleteCallback);

            var tweenRunner = new TweenRunner<ColorTween>();
            tweenRunner.Init(mb);
            tweenRunner.StartTween(colorTween, easeType);

            return tweenRunner;
        }
    }
}
