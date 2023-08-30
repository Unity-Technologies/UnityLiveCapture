using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.TouchFramework
{
    /// <summary>
    /// Place this component on RectTransform to generate an input axis based on the position relative to its parent
    /// RectTransform
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class JoystickControl : MonoBehaviour, IPointerUpHandler, IDragHandler, IPointerDownHandler
    {
        const float k_ResetJoystickTransitionTime = 0.1f;

        [SerializeField]
        [Tooltip("Higher values result in more movement of the joystick")]
        float m_Sensitivity = 0.4f;
#pragma warning disable 649
        [SerializeField]
        bool m_DisableXMovement;
#pragma warning restore 649
        RectTransform m_JoystickContainerRect;
        RectTransform m_JoystickRect;
        Vector2 m_InputAxis;
        Vector2 m_JoystickPositionOnPointerUp;

        Coroutine m_MoveJoystickCoroutine;

        /// <summary>
        /// Values of x and y between 0 and 1 that represent joystick movement
        /// </summary>
        public Vector2 inputAxis
        {
            get { return m_InputAxis; }
        }

        /// <summary>
        /// Higher values result in more movement of the joystick
        /// </summary>
        public float sensitivity
        {
            get => m_Sensitivity;
            set => m_Sensitivity = value;
        }

        void Awake()
        {
            m_JoystickRect = GetComponent<RectTransform>();

            if (transform.parent == null)
                throw new NullReferenceException("Parent is null. The JoystickControl component can't be on " +
                    "root game object.");

            m_JoystickContainerRect = transform.parent.GetComponent<RectTransform>();

            if (m_JoystickContainerRect == null)
                throw new NullReferenceException("JoystickControl component must be on a game object that is" +
                    " the child of a RectTransform");
        }

        IEnumerator ResetJoystickPosition()
        {
            var elapsedTime = 0f;
            var alpha = 0f;
            while (alpha <= 1f)
            {
                alpha = elapsedTime / k_ResetJoystickTransitionTime;
                alpha -= 1f;
                alpha = alpha * alpha * alpha + 1f;

                m_JoystickRect.anchoredPosition = Vector2.Lerp(m_JoystickPositionOnPointerUp, Vector2.zero, alpha);

                elapsedTime += Time.unscaledDeltaTime;

                yield return null;
            }

            m_MoveJoystickCoroutine = null;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData pointerEventData)
        {
            if (m_MoveJoystickCoroutine != null)
            {
                StopCoroutine(m_MoveJoystickCoroutine);
                m_MoveJoystickCoroutine = null;
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData pointerEventData)
        {
            m_InputAxis = Vector2.zero;
            m_JoystickPositionOnPointerUp = m_JoystickRect.anchoredPosition;

            if (m_MoveJoystickCoroutine != null)
            {
                StopCoroutine(m_MoveJoystickCoroutine);
                m_MoveJoystickCoroutine = null;
            }

            m_MoveJoystickCoroutine = StartCoroutine(ResetJoystickPosition());
        }

        void IDragHandler.OnDrag(PointerEventData pointerEventData)
        {
            var pointerPosition = pointerEventData.position;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_JoystickContainerRect,
                pointerPosition, pointerEventData.pressEventCamera, out var position);

            var pivot = m_JoystickContainerRect.pivot;
            var size = m_JoystickContainerRect.sizeDelta;
            var invSize = new Vector2(1f / size.x, 1f / size.y);

            position = Vector2.Scale(position, invSize) + pivot - Vector2.one * 0.5f;
            position *= 2f;

            if (m_DisableXMovement)
                position.x = 0f;

            if (position.sqrMagnitude > 1f)
            {
                position = position.normalized;
            }

            m_InputAxis = position;

            m_JoystickRect.anchoredPosition = Vector2.Scale(position, size) * sensitivity;
        }
    }
}
