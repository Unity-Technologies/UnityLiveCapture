using UnityEngine;
using UnityEngine.Timeline;

namespace Unity.LiveCapture
{
    class TimelinePropertyPreviewer : IPropertyPreviewer
    {
        IPropertyCollector m_Driver;

        public TimelinePropertyPreviewer(IPropertyCollector driver)
        {
            m_Driver = driver;
        }

        public void Register(Component component, string propertyName)
        {
            m_Driver.AddFromName(component, propertyName);
        }

        public void Register(GameObject go, string propertyName)
        {
            m_Driver.AddFromName(go, propertyName);
        }
    }
}
