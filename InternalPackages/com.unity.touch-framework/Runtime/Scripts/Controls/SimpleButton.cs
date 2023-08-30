using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.TouchFramework
{
    public class SimpleButton : MonoBehaviour, IPointerClickHandler
    {
        public Action onClick = delegate {};

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick.Invoke();
        }
    }
}
