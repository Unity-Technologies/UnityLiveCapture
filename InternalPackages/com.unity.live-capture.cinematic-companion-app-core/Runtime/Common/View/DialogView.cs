using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.CompanionAppCommon
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    class DialogView : MonoBehaviour, IDialogView
    {
        RectTransform m_RectTransform;
        Canvas m_Canvas;
        BaseRaycaster m_Raycaster;

        public bool IsShown => GetCanvas().enabled;

        public bool IsInteractable => GetRaycaster().enabled;

        public RectTransform GetRectTransform()
        {
            if (m_RectTransform == null)
            {
                m_RectTransform = GetComponent<RectTransform>();
            }

            return m_RectTransform;
        }

        public Canvas GetCanvas()
        {
            if (m_Canvas == null)
            {
                m_Canvas = GetComponent<Canvas>();
            }

            return m_Canvas;
        }

        public BaseRaycaster GetRaycaster()
        {
            if (m_Raycaster == null)
            {
                m_Raycaster = GetComponent<BaseRaycaster>();
            }

            return m_Raycaster;
        }

        public Vector3 Position
        {
            set { GetRectTransform().position = value; }
        }

        public Vector2 Pivot
        {
            set { GetRectTransform().pivot = value; }
        }

        public Vector2 Size
        {
            set { GetRectTransform().sizeDelta = value; }
        }

        public void Show()
        {
            GetCanvas().enabled = true;
            OnShowChanged();
        }

        public void Hide()
        {
            GetCanvas().enabled = false;
            OnShowChanged();
        }

        public void SetInteractable(bool interactable)
        {
            GetRaycaster().enabled = interactable;
            OnInteractableChanged();
        }

        protected virtual void OnShowChanged() {}

        protected virtual void OnInteractableChanged() {}
    }
}
