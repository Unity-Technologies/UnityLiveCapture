using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class HelpSystem : BaseHelpSystem<HelpTooltipId, HelpTooltipArgs>
    {
        [Inject]
        Platform m_Platform;

        [Inject]
        StateModel m_State;

        public override void Toggle(bool value)
        {
            m_State.IsHelpMode = value;
            base.Toggle(value);
        }

        protected override bool IsArgsValid(HelpTooltipArgs args)
        {
            return args.gameObject.scene.isLoaded && args.platform == m_Platform;
        }
    }
}
