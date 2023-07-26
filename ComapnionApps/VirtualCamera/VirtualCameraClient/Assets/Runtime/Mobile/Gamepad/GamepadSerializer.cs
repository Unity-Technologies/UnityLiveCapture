using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    class GamepadSerializer : IGamepadSerializer
    {
        public const string k_BindingsSaveKey = "GamepadBindings";
        public const string k_SensitivitiesSaveKey = "GamepadSensitivities";

        public void SaveSensitivities(AxisSensitivities sensitivities)
        {
            var jsonSensitivities = new AxisSensitivities.JsonAxisSensitivities();
            jsonSensitivities.FromSensitivities(sensitivities);
            var serializedSensitivities = JsonUtility.ToJson(jsonSensitivities);
            SetData(k_SensitivitiesSaveKey, serializedSensitivities);
        }

        public void SaveBindingOverrides(InputActionMap actions)
        {
            var serializedBindings = BindingOverridesSerialization.SaveBindingOverridesAsJson(actions);
            SetData(k_BindingsSaveKey, serializedBindings);
        }

        public bool TryLoadSensitivities(out AxisSensitivities sensitivities)
        {
            if (TryGetData(k_SensitivitiesSaveKey, out var serialized))
            {
                if (!string.IsNullOrEmpty(serialized))
                {
                    try
                    {
                        var jsonSensitivities = JsonUtility.FromJson<AxisSensitivities.JsonAxisSensitivities>(serialized);
                        if (jsonSensitivities != null)
                        {
                            sensitivities = jsonSensitivities.ToSensitivities();
                            return true;
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Debug.LogWarning($"Couldn't parse gamepad sensitivities: {e} | {serialized}");
                    }
                }
            }

            sensitivities = default;
            return false;
        }

        public bool TryLoadBindingOverrides(InputActionMap actions)
        {
            if (TryGetData(k_BindingsSaveKey, out var serialized))
            {
                if (!string.IsNullOrEmpty(serialized))
                {
                    try
                    {
                        BindingOverridesSerialization.LoadBindingOverridesFromJson(actions, serialized);
                        return true;
                    }
                    catch (ArgumentException e)
                    {
                        Debug.LogWarning($"Couldn't parse gamepad input bindings: {e} | {serialized}");
                    }
                }
            }

            return false;
        }

        protected virtual void SetData(string key, string data)
        {
            PlayerPrefs.SetString(key, data);
        }

        protected virtual bool TryGetData(string key, out string data)
        {
            if (PlayerPrefs.HasKey(key))
            {
                data = PlayerPrefs.GetString(key);
                return true;
            }

            data = default;
            return false;
        }
    }
}
