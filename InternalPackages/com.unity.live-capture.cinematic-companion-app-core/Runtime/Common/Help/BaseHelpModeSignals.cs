using System;

namespace Unity.CompanionAppCommon
{
    class BaseHelpModeSignals<T> where T : struct, Enum
    {
        public class Toggle : Signal<bool> {}
        public class OpenTooltip : Signal<T> {}
        public class CloseTooltip {}
    }
}
