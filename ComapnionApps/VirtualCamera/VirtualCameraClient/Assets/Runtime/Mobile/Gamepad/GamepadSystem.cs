using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    class GamepadSystem : IInitializable, ITickable
    {
        static GamepadSystem s_Instance;

        static int s_DeferOnControlsChanged;
        static DeferOnControlsChangedWrapper s_DeferOnControlsChangedWrapper;

        internal static DeferOnControlsChangedWrapper DeferOnControlsChanged()
        {
            if (s_DeferOnControlsChangedWrapper == null)
            {
                s_DeferOnControlsChangedWrapper = new DeferOnControlsChangedWrapper();
            }
            s_DeferOnControlsChangedWrapper.Acquire();
            return s_DeferOnControlsChangedWrapper;
        }

        [Inject]
        IActionProcessor m_Processor;

        [Inject]
        IGamepadSerializer m_Serializer;

        bool m_Enabled = true;
        AxisSensitivities m_Sensitivities = new AxisSensitivities();
        InputActionMap m_ActionMap;
        IActionResolver m_ActionResolver;

        readonly List<ActionState> m_Actions = new List<ActionState>();
        readonly List<ActionConflictGroup> m_ConflictGroups = new List<ActionConflictGroup>();
        readonly Dictionary<ActionID, ActionState> m_ActionsLookup = new Dictionary<ActionID, ActionState>();

        public bool Enabled
        {
            get => m_Enabled;
            set => m_Enabled = value;
        }

        public AxisSensitivities Sensitivities => m_Sensitivities;

        public InputActionMap ActionMap
        {
            get => m_ActionMap;
            set
            {
                var changed = m_ActionMap != value;
                m_ActionMap = value;

                if (!changed)
                    return;

                using (DeferOnControlsChanged())
                {
                    Load();
                    OnControlsChanged();
                }
            }
        }

        public IActionResolver ActionResolver
        {
            get => m_ActionResolver;
            set => m_ActionResolver = value;
        }

        GamepadSystem()
        {
            s_Instance = this;
        }

        public void Initialize()
        {
            m_Processor.InitializeActions(m_Actions);

            foreach (var action in m_Actions)
            {
                m_ActionsLookup.Add(action.ID, action);
            }
        }

        public void Tick()
        {
            foreach (var group in m_ConflictGroups)
            {
                group.Update();
            }
        }

        internal class DeferOnControlsChangedWrapper : IDisposable
        {
            public void Acquire()
            {
                ++s_DeferOnControlsChanged;
            }

            public void Dispose()
            {
                if (s_DeferOnControlsChanged > 0)
                    --s_DeferOnControlsChanged;
                if (s_DeferOnControlsChanged == 0 && s_Instance != null)
                    s_Instance.OnControlsChanged();
            }
        }

        public void OnControlsChanged()
        {
            if (s_DeferOnControlsChanged != 0)
            {
                return;
            }

            s_DeferOnControlsChanged++;

            foreach (var action in m_Actions)
            {
                action.Reset();
            }

            m_ConflictGroups.Clear();
            ConflictGroupResolution.Resolve(m_ActionMap, out var groups);

            foreach (var group in groups)
            {
                var actionGroup = new ActionConflictGroup
                {
                    ModifierControl = group.ModifierControl
                };
                m_ConflictGroups.Add(actionGroup);

                if (group.CompositeAction != null)
                {
                    var composite = ResolveActionState(group.CompositeAction);
                    if (composite != null)
                    {
                        actionGroup.Register(composite, ActionConflictGroup.Role.Composite);
                    }
                }

                if (group.MainAction != null)
                {
                    var main = ResolveActionState(group.MainAction);
                    if (main != null)
                    {
                        actionGroup.Register(main, ActionConflictGroup.Role.Main);
                    }
                }

                if (group.ModifierAction != null)
                {
                    var modifier = ResolveActionState(group.ModifierAction);
                    if (modifier != null)
                    {
                        actionGroup.Register(modifier, ActionConflictGroup.Role.Modifier);
                    }
                }
            }

            foreach (var action in m_ActionMap)
            {
                var state = ResolveActionState(action);
                if (state == null)
                {
                    continue;
                }

                if (state.Role == ActionConflictGroup.Role.Modifier)
                {
                    string newInteractions = "tap(duration=0.5)";
                    BindingOverrides.OverrideInteractions(action, action.bindings[0], newInteractions);
                }
                else
                {
                    BindingOverrides.OverrideInteractions(action, action.bindings[0], null);
                }
            }

            s_DeferOnControlsChanged--;
        }

        public void OnActionTriggered(InputAction action, ActionUpdateData updateData)
        {
            if (!Enabled)
            {
                return;
            }

            var actionState = ResolveActionState(action);

            if (actionState is ButtonActionState buttonActionState)
            {
                buttonActionState.ProcessInput(updateData);
            }
            else if (actionState is AnalogActionState analogActionState)
            {
                var sensitivity = 1.0f;
                if (analogActionState.Axis != null)
                {
                    sensitivity = m_Sensitivities.GetSensitivity(analogActionState.Axis.ID);
                }
                analogActionState.ProcessInput(updateData, sensitivity);
            }
        }

        public ActionState ResolveActionState(InputAction input)
        {
            if (ActionResolver.TryResolveActionID(input, out var actionID))
            {
                return ResolveActionState(actionID);
            }
            return null;
        }

        public ActionState ResolveActionState(ActionID actionID)
        {
            return m_ActionsLookup[actionID];
        }

        public void Save()
        {
            m_Serializer.SaveSensitivities(m_Sensitivities);
            m_Serializer.SaveBindingOverrides(m_ActionMap);
        }

        public void Load()
        {
            using (DeferOnControlsChanged())
            {
                if (m_Serializer.TryLoadSensitivities(out var sensitivities))
                {
                    m_Sensitivities = sensitivities;
                }

                if (m_ActionMap != null)
                {
                    m_Serializer.TryLoadBindingOverrides(m_ActionMap);
                }
            }
        }
    }
}
