using Unity.LiveCapture.VirtualCamera;
using Unity.TouchFramework;
using Unity.CompanionAppCommon;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    class LinkedScroll : MonoBehaviour
    {
        [SerializeField]
        ListViewLinker m_XListViewLinker;
        [SerializeField]
        ListViewLinker m_ZListViewLinker;
        [SerializeField]
        SlideToggle m_Link;
        [SerializeField]
        protected Vector3Scroller m_Vector3Scroller;

        protected virtual void OnEnable()
        {
            m_Link.onValueChanged.AddListener(OnLinkValueChanged);
        }

        protected virtual void OnDisable()
        {
            m_Link.onValueChanged.RemoveListener(OnLinkValueChanged);
        }

        void OnLinkValueChanged(bool value)
        {
            if (value)
            {
                if (m_Vector3Scroller.LastAxisChanged() == Axis.X)
                {
                    m_ZListViewLinker.isLinked = true;
                }
                else
                {
                    m_XListViewLinker.isLinked = true;
                }
            }
            else
            {
                m_ZListViewLinker.isLinked = false;
                m_XListViewLinker.isLinked = false;
            }
        }
    }
}
