using System.Collections;
using NSubstitute;
using NUnit.Framework;
using Unity.CompanionAppCommon;
using UnityEngine.TestTools;
using Unity.CompanionApps.VirtualCamera.Gamepad;
using Unity.LiveCapture.CompanionApp;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera.Tests
{
    // Normally, our class with the test cases would need to inherit ZenjectIntegrationTestFixture.
    // We can't do that because it takes away our control over Setup and TearDown, where we need to
    // also manage InputSystem.
    //
    // This class is a proxy, for use as in instance instead of a parent class.
    //
    // A minor modification to ZenjectIntegrationTestFixture.cs was necessary.
    class ZenjectIntegrationTestFixtureWrapper : ZenjectIntegrationTestFixture
    {
        public DiContainer ContainerProxy
        {
            get => base.Container;
        }

        public void SkipInstallProxy()
        {
            base.SkipInstall();
        }

        public void PreInstallProxy()
        {
            base.PreInstall();
        }

        public void PostInstallProxy()
        {
            base.PostInstall();
        }
    }

    class GamepadTests
    {
        // Used to create a substitute object to count method calls
        public interface ISignalListener
        {
            void OnMoveRight(float value);
            void OnFastForward(float value);
            void OnRewind(float value);
            void OnToggleRecording();
            void OnSkipOneFrame();
            void OnResetPose();
            void OnResetLens();
        }

        // Prevents us from saving/loading any gamepad user settings during tests
        class NullGamepadSerializer : IGamepadSerializer
        {
            public void SaveSensitivities(AxisSensitivities sensitivities)
            {

            }

            public void SaveBindingOverrides(InputActionMap actions)
            {

            }

            public bool TryLoadSensitivities(out AxisSensitivities sensitivities)
            {
                sensitivities = new AxisSensitivities();
                return true;
            }

            public bool TryLoadBindingOverrides(InputActionMap actions)
            {
                return true;
            }
        }

        // Saves/loads gamepad user settings to class variables, instead of storage.
        // Use SensitivitiesData and BindingOverridesData as if you were manipulating storage data directly.
        class TestGamepadSerializer : GamepadSerializer
        {
            public string SensitivitiesData;
            public string BindingOverridesData;

            protected override bool TryGetData(string key, out string data)
            {
                if (key == k_SensitivitiesSaveKey)
                {
                    data = SensitivitiesData;
                    return data != null;
                }
                else if (key == k_BindingsSaveKey)
                {
                    data = BindingOverridesData;
                    return data != null;
                }

                data = default;
                return false;
            }

            protected override void SetData(string key, string data)
            {
                if (key == k_SensitivitiesSaveKey)
                {
                    SensitivitiesData = data;
                }
                else if (key == k_BindingsSaveKey)
                {
                    BindingOverridesData = data;
                }
            }
        }

        // The paths are relative to Assets/Tests/Resources
        const string kDefaultDriverPath = "GamepadDefault/Driver";
        const string kAnalogModifierDriverPath = "GamepadAnalogModifier/Driver";
        const string kBindingOverridesDriverPath = "GamepadBindingOverrides/Driver";

        [Inject]
        GamepadSystem m_GamepadSystem;

        [Inject]
        IGamepadDriver m_GamepadDriver;

        ZenjectIntegrationTestFixtureWrapper m_Zenject;
        InputTestFixture m_Input;

        // The mock input device
        UnityEngine.InputSystem.Gamepad m_Gamepad;

        [SetUp]
        public void Setup()
        {
            m_Input = new InputTestFixture();

            // Notably, pushes a brand new input context so that we don't get affected by or affect the current context
            m_Input.Setup();

            // Not sure why it doesn't register automatically
            InputSystem.RegisterBindingComposite<OneModifierCustomComposite>();

            // Create the mock input device
            m_Gamepad = InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();


            m_Zenject = new ZenjectIntegrationTestFixtureWrapper();
            m_Zenject.Setup();
        }

        [TearDown]
        public void TearDown()
        {
            m_Zenject.TearDown();

            // Pops the test input context, returns to main context
            InputSystem.RemoveDevice(m_Gamepad);
            m_Input.TearDown();
        }

        void CommonInstall(ISignalListener listener, string driverPath)
        {
            m_Zenject.PreInstallProxy();

            var Container = m_Zenject.ContainerProxy;
            SignalBusInstaller.Install(Container);


            // Gamepad
            Container.BindInterfacesAndSelfTo<GamepadSystem>().AsSingle();
            Container.BindInterfacesAndSelfTo<NullGamepadSerializer>().AsSingle();
            Container.BindInterfacesAndSelfTo<ActionProcessor>().AsSingle();

            var gamepadDriver = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(driverPath));
            Container.Bind<IGamepadDriver>().FromInstance(gamepadDriver.GetComponent<GamepadDriver>());
            Container.BindInterfacesTo<GamepadDriverMediator>().AsSingle();

            var notifier = Substitute.For<IActionNotifier>();
            Container.BindInterfacesAndSelfTo<IActionNotifier>().FromInstance(notifier);


            // App
            var appHost = Substitute.For<ICompanionAppHost>();
            Container.BindInterfacesAndSelfTo<ICompanionAppHost>().FromInstance(appHost);
            appHost.DeviceMode.Returns(DeviceMode.LiveStream);


            // Models
            var connection = new ConnectionModel();
            Container.BindInterfacesAndSelfTo<ConnectionModel>().FromInstance(connection);

            var camera = new CameraModel();
            Container.BindInterfacesAndSelfTo<CameraModel>().FromInstance(camera);

            var settings = new SettingsModel();
            Container.BindInterfacesAndSelfTo<SettingsModel>().FromInstance(settings);


            m_Zenject.PostInstallProxy();
            Container.Inject(this);


            // Assign the callbacks of the actions that we know we will use during testing to the listener.
            // (Overriding the default callbacks assigned in ActionProcessor.)
            (m_GamepadSystem.ResolveActionState(ActionID.MoveRight) as AnalogActionState).Callback = listener.OnMoveRight;
            (m_GamepadSystem.ResolveActionState(ActionID.MoveLeft) as AnalogActionState).Callback = (x) => {};
            (m_GamepadSystem.ResolveActionState(ActionID.FastForward) as AnalogActionState).Callback = listener.OnFastForward;
            (m_GamepadSystem.ResolveActionState(ActionID.Rewind) as AnalogActionState).Callback = listener.OnRewind;
            (m_GamepadSystem.ResolveActionState(ActionID.ToggleRecording) as ButtonActionState).Callback = listener.OnToggleRecording;
            (m_GamepadSystem.ResolveActionState(ActionID.SkipOneFrame) as ButtonActionState).Callback = listener.OnSkipOneFrame;
            (m_GamepadSystem.ResolveActionState(ActionID.ResetPose) as ButtonActionState).Callback = listener.OnResetPose;
            (m_GamepadSystem.ResolveActionState(ActionID.ResetLens) as ButtonActionState).Callback = listener.OnResetLens;
        }

        // Utility functions that skip a frame to let InputSystem process the change and trigger callbacks
        IEnumerator SetControl<TValue>(InputControl<TValue> control, TValue value) where TValue : struct
        {
            m_Input.Set(control, value);
            yield return null;
        }
        IEnumerator Press(ButtonControl control)
        {
            m_Input.Press(control);
            yield return null;
        }
        IEnumerator Release(ButtonControl control)
        {
            m_Input.Release(control);
            yield return null;
        }
        IEnumerator PressAndRelease(ButtonControl control)
        {
            m_Input.PressAndRelease(control);
            yield return null;
        }




        [UnityTest]
        public IEnumerator Disconnect_Reconnect_Device_Handled()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kAnalogModifierDriverPath);
            yield return null;

            Assert.IsTrue(m_GamepadDriver.HasDevice);

            InputSystem.RemoveDevice(m_Gamepad);
            Assert.IsFalse(m_GamepadDriver.HasDevice);

            m_Gamepad = InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();
            Assert.IsTrue(m_GamepadDriver.HasDevice);

            yield return Press(m_Gamepad.leftShoulder);
            listener.DidNotReceive().OnToggleRecording();

            yield return Release(m_Gamepad.leftShoulder);
            listener.Received().OnToggleRecording();
        }




        [UnityTest]
        public IEnumerator Default_MainAnalog_Actuated()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kDefaultDriverPath);
            yield return null;

            yield return PressAndRelease(m_Gamepad.buttonSouth);

            yield return SetControl(m_Gamepad.leftStick, new Vector2(1f, 0f));
            listener.Received().OnMoveRight(1f);

            yield return SetControl(m_Gamepad.leftStick, new Vector2(0.5f, 0f));
            listener.Received().OnMoveRight(0.4296875f);

            yield return SetControl(m_Gamepad.leftStick, new Vector2(0f, 0f));
            listener.Received().OnMoveRight(0f);
        }

        [UnityTest]
        public IEnumerator Default_CompositeAnalog_ActuatedExclusive()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kDefaultDriverPath);
            yield return null;

            yield return Press(m_Gamepad.buttonSouth);
            listener.DidNotReceive().OnToggleRecording();

            yield return SetControl(m_Gamepad.rightTrigger, 1f);
            listener.Received().OnFastForward(2f);

            yield return SetControl(m_Gamepad.rightTrigger, 0.5f);
            listener.Received().OnFastForward(0.9375f);

            yield return Release(m_Gamepad.buttonSouth);
            listener.DidNotReceive().OnToggleRecording();

            listener.Received().OnFastForward(0f);
        }

        [UnityTest]
        public IEnumerator Default_CompositeAnalogInverseOrder_ActuatedExclusive()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kDefaultDriverPath);
            yield return null;

            yield return SetControl(m_Gamepad.rightTrigger, 1f);

            yield return Press(m_Gamepad.buttonSouth);
            listener.DidNotReceive().OnToggleRecording();
            listener.Received().OnFastForward(2f);

            yield return Release(m_Gamepad.buttonSouth);
            listener.DidNotReceive().OnToggleRecording();

            listener.Received().OnFastForward(0f);
        }

        [UnityTest]
        public IEnumerator Default_CompositeButton_ActuatedExclusive()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kDefaultDriverPath);
            yield return null;

            yield return Press(m_Gamepad.buttonSouth);
            listener.DidNotReceive().OnToggleRecording();

            yield return Press(m_Gamepad.dpad.right);
            listener.Received().OnSkipOneFrame();

            yield return Release(m_Gamepad.buttonSouth);
            listener.DidNotReceive().OnToggleRecording();
        }

        [UnityTest]
        public IEnumerator Default_Modifier_Tap()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kDefaultDriverPath);
            yield return null;

            yield return Press(m_Gamepad.buttonSouth);
            listener.DidNotReceive().OnToggleRecording();

            yield return null;

            yield return Release(m_Gamepad.buttonSouth);
            listener.Received().OnToggleRecording();
        }

        [UnityTest]
        public IEnumerator Default_Modifier_Timeout()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kDefaultDriverPath);
            yield return null;

            yield return Press(m_Gamepad.buttonSouth);
            listener.DidNotReceive().OnToggleRecording();

            yield return new WaitForSeconds(0.75f);

            yield return Release(m_Gamepad.buttonSouth);
            listener.DidNotReceive().OnToggleRecording();
        }

        [UnityTest]
        public IEnumerator Default_Axis_Sum()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kDefaultDriverPath);
            yield return null;

            yield return Press(m_Gamepad.buttonSouth);

            yield return SetControl(m_Gamepad.rightTrigger, 1f);
            listener.Received().OnFastForward(2f);

            yield return SetControl(m_Gamepad.leftTrigger, 0.5f);
            listener.Received().OnRewind(1.0625f);
        }




        [UnityTest]
        public IEnumerator AnalogModifier_MainAnalog_ActuatedExclusive()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kAnalogModifierDriverPath);
            yield return null;

            yield return PressAndRelease(m_Gamepad.leftShoulder);

            yield return SetControl(m_Gamepad.leftStick, new Vector2(1f, 0f));
            listener.Received().OnMoveRight(1f);

            yield return SetControl(m_Gamepad.leftStick, new Vector2(0.5f, 0f));
            listener.Received().OnMoveRight(0.46875f);

            yield return SetControl(m_Gamepad.leftStick, new Vector2(0f, 0f));
            listener.Received().OnMoveRight(0f);

            listener.DidNotReceive().OnFastForward(Arg.Any<float>());
        }

        [UnityTest]
        public IEnumerator AnalogModifier_CompositeAnalog_ActuatedExclusive()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kAnalogModifierDriverPath);
            yield return null;

            yield return Press(m_Gamepad.leftShoulder);
            listener.DidNotReceive().OnToggleRecording();

            yield return SetControl(m_Gamepad.leftStick, new Vector2(1f, 0f));
            listener.Received().OnFastForward(1f);

            yield return SetControl(m_Gamepad.leftStick, new Vector2(0.5f, 0f));
            listener.Received().OnFastForward(0.46875f);

            yield return Release(m_Gamepad.leftShoulder);
            listener.DidNotReceive().OnToggleRecording();

            listener.Received().OnFastForward(0f);

            listener.DidNotReceive().OnMoveRight(Arg.Any<float>());
        }

        [UnityTest]
        public IEnumerator AnalogModifier_CompositeAnalogInverseOrder_ActuatedExclusive()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kAnalogModifierDriverPath);
            yield return null;

            yield return SetControl(m_Gamepad.leftStick, new Vector2(1f, 0f));

            yield return Press(m_Gamepad.leftShoulder);
            listener.DidNotReceive().OnToggleRecording();
            listener.Received().OnFastForward(1f);

            yield return Release(m_Gamepad.leftShoulder);
            listener.DidNotReceive().OnToggleRecording();

            listener.Received().OnFastForward(0f);
        }

        [UnityTest]
        public IEnumerator AnalogModifier_CompositeButton_ActuatedExclusive()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kAnalogModifierDriverPath);
            yield return null;

            yield return Press(m_Gamepad.leftShoulder);
            listener.DidNotReceive().OnToggleRecording();

            yield return Press(m_Gamepad.buttonNorth);
            listener.Received().OnResetLens();

            yield return Release(m_Gamepad.buttonSouth);
            listener.DidNotReceive().OnToggleRecording();

            listener.DidNotReceive().OnResetPose();
        }

        [UnityTest]
        public IEnumerator AnalogModifier_Modifier_Tap()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kAnalogModifierDriverPath);
            yield return null;

            yield return Press(m_Gamepad.leftShoulder);
            listener.DidNotReceive().OnToggleRecording();

            yield return null;

            yield return Release(m_Gamepad.leftShoulder);
            listener.Received().OnToggleRecording();
        }




        [UnityTest]
        public IEnumerator Sensitivities_Load_HandlesInvalidData()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kDefaultDriverPath);
            yield return null;

            var serializer = new TestGamepadSerializer();

            serializer.SensitivitiesData = null;
            Assert.IsFalse(serializer.TryLoadSensitivities(out var _));

            serializer.SensitivitiesData = "";
            Assert.IsFalse(serializer.TryLoadSensitivities(out var _));

            serializer.SensitivitiesData = "Invalid JSON";
            Assert.IsFalse(serializer.TryLoadSensitivities(out var _));

            // Empty arrays
            serializer.SensitivitiesData = "{\"m_Names\":[],\"m_Values\":[]}";
            Assert.IsTrue(serializer.TryLoadSensitivities(out var _));

            // Array count mismatch
            serializer.SensitivitiesData = "{\"m_Names\":[\"InvalidAxis\"],\"m_Values\":[]}";
            Assert.IsTrue(serializer.TryLoadSensitivities(out var _));

            // Invalid axis name
            serializer.SensitivitiesData = "{\"m_Names\":[\"InvalidAxis\"],\"m_Values\":[10.0]}";
            Assert.IsTrue(serializer.TryLoadSensitivities(out var _));
        }

        [UnityTest]
        public IEnumerator Sensitivities_Save_ValidReadback()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kDefaultDriverPath);
            yield return null;

            for (var i = 0; i < (int) AxisID.Count; i++)
            {
                var axis = (AxisID) i;
                m_GamepadSystem.Sensitivities.SetSensitivity(axis, i);
            }

            var serializer = new TestGamepadSerializer();
            serializer.SaveSensitivities(m_GamepadSystem.Sensitivities);
            serializer.TryLoadSensitivities(out var sensitivities);

            for (var i = 0; i < (int) AxisID.Count; i++)
            {
                var axis = (AxisID) i;
                var sensitivity = sensitivities.GetSensitivity(axis);
                Assert.AreEqual(i, sensitivity);
            }
        }

        [UnityTest]
        public IEnumerator BindingOverrides_Load_HandlesInvalidData()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kBindingOverridesDriverPath);
            yield return null;

            var serializer = new TestGamepadSerializer();

            serializer.BindingOverridesData = null;
            Assert.IsFalse(serializer.TryLoadBindingOverrides(m_GamepadSystem.ActionMap));

            serializer.BindingOverridesData = "";
            Assert.IsFalse(serializer.TryLoadBindingOverrides(m_GamepadSystem.ActionMap));

            serializer.BindingOverridesData = "Invalid JSON";
            Assert.IsFalse(serializer.TryLoadBindingOverrides(m_GamepadSystem.ActionMap));

            // Empty array
            serializer.BindingOverridesData = "{\"bindings\": []}";
            Assert.IsTrue(serializer.TryLoadBindingOverrides(m_GamepadSystem.ActionMap));

            // Non-matching Guid
            serializer.BindingOverridesData = "{\"bindings\": [{\"action\": \"Main/buttonEast\", \"id\": \"99999999-9999-9999-9999-999999999999\", \"path\": \"<Gamepad>/buttonSouth\", \"interactions\": \"tap\", \"processors\": \"\", \"overridePath\": true, \"overrideInteractions\": true, \"overrideProcessors\": false}]}";
            Assert.IsTrue(serializer.TryLoadBindingOverrides(m_GamepadSystem.ActionMap));

            // Completely valid JSON
            serializer.BindingOverridesData = "{\"bindings\": [{\"action\": \"Main/buttonEast\", \"id\": \"6c5f7738-2e5b-4266-ade8-4d6616b170de\", \"path\": \"<Gamepad>/start\", \"interactions\": \"tap\", \"processors\": \"\", \"overridePath\": true, \"overrideInteractions\": true, \"overrideProcessors\": false}]}";
            Assert.IsTrue(serializer.TryLoadBindingOverrides(m_GamepadSystem.ActionMap));

            m_GamepadSystem.ActionMap.RemoveAllBindingOverrides();
        }

        struct TestBindingOverride
        {
            public string overridePath;
            public string overrideInteractions;

            public TestBindingOverride(string overridePath, string overrideInteractions)
            {
                this.overridePath = overridePath;
                this.overrideInteractions = overrideInteractions;
            }
        }

        [UnityTest]
        public IEnumerator BindingOverrides_Save_ValidReadback()
        {
            var listener = Substitute.For<ISignalListener>();
            CommonInstall(listener, kBindingOverridesDriverPath);
            yield return null;

            var newPath = "<Gamepad>/buttonSouth";
            var newInteractions = "tap";

            var bindingOverrides = new[]
            {
                new TestBindingOverride(newPath, newInteractions),
                new TestBindingOverride(null, newInteractions),
                new TestBindingOverride(newPath, null),
                new TestBindingOverride(null, null),
                new TestBindingOverride(newPath, ""),
                new TestBindingOverride("", newInteractions),
                new TestBindingOverride(newPath, ""),
                new TestBindingOverride("", ""),
                new TestBindingOverride(null, ""),
                new TestBindingOverride("", null),
            };

            var bindingOverrideIndex = 0;
            foreach (var action in m_GamepadSystem.ActionMap)
            {
                foreach (var binding in action.bindings)
                {
                    if (bindingOverrideIndex >= bindingOverrides.Length)
                        break;
                    if (binding.isComposite)
                        continue;

                    var bindingOverride = new InputBinding
                    {
                        id = binding.id,
                        overridePath = bindingOverrides[bindingOverrideIndex].overridePath,
                        overrideInteractions = bindingOverrides[bindingOverrideIndex].overrideInteractions
                    };

                    m_GamepadSystem.ActionMap.ApplyBindingOverride(bindingOverride);

                    bindingOverrideIndex++;
                }
            }

            var serializer = new TestGamepadSerializer();
            serializer.SaveBindingOverrides(m_GamepadSystem.ActionMap);

            m_GamepadSystem.ActionMap.RemoveAllBindingOverrides();

            foreach (var action in m_GamepadSystem.ActionMap)
            {
                foreach (var binding in action.bindings)
                {
                    Assert.IsNull(binding.overridePath);
                    Assert.IsNull(binding.overrideInteractions);
                }
            }

            serializer.TryLoadBindingOverrides(m_GamepadSystem.ActionMap);

            bindingOverrideIndex = 0;
            foreach (var action in m_GamepadSystem.ActionMap)
            {
                foreach (var binding in action.bindings)
                {
                    if (bindingOverrideIndex >= bindingOverrides.Length)
                        break;
                    if (binding.isComposite)
                        continue;

                    var bindingOverride = bindingOverrides[bindingOverrideIndex];
                    Assert.AreEqual(bindingOverride.overridePath, binding.overridePath);
                    Assert.AreEqual(bindingOverride.overrideInteractions, binding.overrideInteractions);

                    bindingOverrideIndex++;
                }
            }

            m_GamepadSystem.ActionMap.RemoveAllBindingOverrides();
        }

        class TestScaler : IScaler<float>
        {
            Vector2 m_Range;

            public Vector2 Range
            {
                get => m_Range;
                set => m_Range = value;
            }

            public float ClampToInputScale(float value)
            {
                return Mathf.Clamp01(value);
            }

            public float ToInputScale(float value)
            {
                return Mathf.InverseLerp(m_Range.x, m_Range.y, value);
            }

            public float ToValueScale(float value)
            {
                return Mathf.Lerp(m_Range.x, m_Range.y, value);
            }
        }

        [Test]
        public void ValueTracker()
        {
            m_Zenject.SkipInstallProxy();

            var tracker = new FloatValueTracker();
            var scaler = new TestScaler();

            scaler.Range = new Vector2(10f, 20f);
            tracker.Scaler = scaler;

            tracker.Update(15f, 0.05f, 0f);
            Assert.IsTrue(tracker.Changing);
            Assert.AreEqual(15f, tracker.Current);

            tracker.Update(15f, 0.05f, 2f);
            Assert.IsTrue(tracker.Changing);
            Assert.AreEqual(16f, tracker.Current);

            tracker.Update(18f, 0f, 2f);
            Assert.IsFalse(tracker.Changing);
        }
    }
}
