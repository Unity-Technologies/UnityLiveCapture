using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class HelpSystem : BaseHelpSystem<HelpTooltipId, HelpTooltipArgs>
    {
        bool m_IsActive;

        public bool IsActive() => m_IsActive;

        public override void Toggle(bool value)
        {
            m_IsActive = value;
            base.Toggle(value);
        }
    }
}
