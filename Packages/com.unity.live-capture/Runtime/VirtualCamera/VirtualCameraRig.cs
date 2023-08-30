using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Rigs
{
    /// <summary>
    /// Struct that contains the configuration parameters of the <see cref="VirtualCameraRig"/>.
    /// </summary>
    [Serializable]
    struct VirtualCameraRigSettings
    {
        /// <summary>
        /// The default state of a rig settings.
        /// </summary>
        public static VirtualCameraRigSettings Identity => new VirtualCameraRigSettings
        {
            PositionLock = PositionAxis.None,
            RotationLock = RotationAxis.None,
            MotionScale = Vector3.one,
        };

        /// <summary>
        /// The <see cref="PositionAxis"/> that won't move.
        /// </summary>
        public PositionAxis PositionLock;

        /// <summary>
        /// Rotation Axis constraint settings.
        /// </summary>
        public RotationAxis RotationLock;

        /// <summary>
        /// Force dutch rotation to be zero.
        /// </summary>
        public bool ZeroDutch;

        /// <summary>
        /// Rebasing set to true indicates that the Virtual Camera is frozen in virtual space allowing the user
        /// to change position in real world space.
        /// </summary>
        public bool Rebasing;

        /// <summary>
        /// Scale of the movement for each axis. A scale of (1, 1, 1) means that the virtual camera position will
        /// Match the device position in real world.
        /// </summary>
        public Vector3 MotionScale;

        /// <summary>
        /// The angle around the x-axis to offset the local camera rotation.
        /// </summary>
        public float ErgonomicTilt;
    }

    /// <summary>
    /// Struct that contains the state of the <see cref="VirtualCameraRig"/>
    /// </summary>
    [Serializable]
    struct VirtualCameraRigState
    {
        /// <summary>
        /// The default VirtualCameraRigState.
        /// </summary>
        public static VirtualCameraRigState Identity => new VirtualCameraRigState
        {
            Pose = Pose.identity,
            Origin = Pose.identity,
            ARPose = Pose.identity,
            LocalPose = Pose.identity,
            LastInput = Pose.identity,
            RebaseOffset = Quaternion.identity,
            ErgonomicTilt = 0,
            JoystickPosition = Vector3.zero,
            JoystickAngles = Vector3.zero
        };

        /// <summary>
        /// The calculated global <see cref="UnityEngine.Pose"/> of the camera.
        /// </summary>
        [ReadOnly, Tooltip("The calculated global pose of the camera.")]
        public Pose Pose;

        /// <summary>
        /// The global offset in world space representing the origin of the virtual camera.
        /// </summary>
        [Tooltip("The point of origin of the camera.")]
        public Pose Origin;

        /// <summary>
        /// The <see cref="UnityEngine.Pose"/> of the camera in the AR room space.
        /// </summary>
        [Tooltip("The pose of the camera in the AR room space.")]
        public Pose ARPose;

        /// <summary>
        /// The calculated <see cref="UnityEngine.Pose"/> relative to the origin.
        /// </summary>
        [Tooltip("The calculated pose relative to the origin.")]
        public Pose LocalPose;

        /// <summary>
        /// The input sample of the previous calculation.
        /// </summary>
        [HideInInspector]
        public Pose LastInput;

        /// <summary>
        /// Offset relative to the pose input. Generated during a rebase.
        /// </summary>
        [HideInInspector]
        public Quaternion RebaseOffset;

        /// <summary>
        /// The current ergonomic tilt value of the state.
        /// </summary>
        [HideInInspector]
        public float ErgonomicTilt;

        /// <summary>
        /// The translation accumulated by joystick/gamepad input.
        /// </summary>
        [HideInInspector]
        public Vector3 JoystickPosition;

        /// <summary>
        /// The rotation accumulated by joystick/gamepad input, in euler angles (degrees).
        /// </summary>
        [HideInInspector]
        public Vector3 JoystickAngles;
    }

    /// <summary>
    /// Static class that contains the data needed for calculating the final <see cref="Pose"/> of a virtual camera.
    /// VirtualCameraRig implements rebasing, position and rotation axis lock and global offset.
    /// </summary>
    static class VirtualCameraRig
    {
        /// <summary>
        /// Updates the <see cref="VirtualCameraRigState"/> of the rig from a new pose sample.
        /// </summary>
        /// <param name="input">The pose sample to follow</param>
        /// <param name="settings">The settings that will be applied while updating the state</param>
        public static void Update(this ref VirtualCameraRigState state, Pose input, VirtualCameraRigSettings settings)
        {
            if (settings.Rebasing)
                state.Rebase(input);
            else
            {
                var deltaPosition = input.position - state.LastInput.position;
                var axis = (Axis)settings.PositionLock;

                // Make delta position local space
                deltaPosition = Quaternion.Inverse(input.rotation) * deltaPosition;
                // Remove locked axis contribution
                deltaPosition = Mask(deltaPosition, axis.HasFlag(Axis.X), axis.HasFlag(Axis.Y), axis.HasFlag(Axis.Z));
                // Motion scale
                deltaPosition = Vector3.Scale(settings.MotionScale, deltaPosition);

                var lastErgonomicTilt = Quaternion.Euler(state.ErgonomicTilt, 0f, 0f);
                var ergonomicTilt = Quaternion.Euler(settings.ErgonomicTilt, 0f, 0f);

                // Compensate for rebase offset and ergonomic tilt
                var targetEuler = (Quaternion.Inverse(state.RebaseOffset) * input.rotation).eulerAngles;
                var currentEuler = (state.ARPose.rotation * Quaternion.Inverse(lastErgonomicTilt)).eulerAngles;

                // Use the current euler value if the rotation axis is locked
                axis = (Axis)settings.RotationLock;

                if (axis.HasFlag(Axis.X))
                    targetEuler.x = currentEuler.x;

                if (axis.HasFlag(Axis.Y))
                {
                    state.Rebase(input);
                    targetEuler.y = currentEuler.y;
                }

                if (axis.HasFlag(Axis.Z))
                    targetEuler.z = settings.ZeroDutch ? 0f : currentEuler.z;

                var targetRotation = Quaternion.Euler(targetEuler);

                // Take the joystick Y rotation into account when applying the AR translation
                var joystickRotationY = Quaternion.Euler(0f, state.JoystickAngles.y, 0f);

                state.ARPose.rotation = targetRotation * ergonomicTilt;
                state.ARPose.position += joystickRotationY * targetRotation * deltaPosition;
                state.ErgonomicTilt = settings.ErgonomicTilt;
            }

            state.ComputePose(settings);
            state.LastInput = input;
        }

        /// <summary>
        /// Rebase the Y rotation of the camera rig
        /// </summary>
        /// <param name="state">The current state of the rig</param>
        /// <param name="input">The input to rebase</param>
        public static void Rebase(this ref VirtualCameraRigState state, Pose input)
        {
            var ergonomicTilt = Quaternion.Euler(state.ErgonomicTilt, 0f, 0f);
            var ergonomicInput = input.rotation * ergonomicTilt;

            var localRotationY = Quaternion.Euler(0f, state.ARPose.rotation.eulerAngles.y, 0f);
            var inputRotationY = Quaternion.Euler(0f, ergonomicInput.eulerAngles.y, 0f);
            state.RebaseOffset = Quaternion.Inverse(localRotationY) * inputRotationY;
        }

        /// <summary>
        /// Set the <see cref="VirtualCameraRigState"/> of the rig.
        /// </summary>
        /// <param name="state">The current state to be refreshed</param>
        /// <param name = "settings" > The settings that will be applied to refresh the state</param>
        public static void Refresh(this ref VirtualCameraRigState state, VirtualCameraRigSettings settings)
        {
            var lastErgonomicTilt = Quaternion.Euler(state.ErgonomicTilt, 0f, 0f);
            var ergonomicTilt = Quaternion.Euler(settings.ErgonomicTilt, 0f, 0f);
            var ergonomicTiltDelta = ergonomicTilt * Quaternion.Inverse(lastErgonomicTilt);

            state.ARPose.rotation *= ergonomicTiltDelta;
            state.JoystickAngles += ergonomicTiltDelta.eulerAngles;
            state.ComputePose(settings);

            state.ErgonomicTilt = settings.ErgonomicTilt;
        }

        /// <summary>
        /// Set the localPose from a Pose which represents the world position of the camera.
        /// </summary>
        /// <param name="worldCoordinates">World position used to evaluate the localPose</param>
        public static void WorldToLocal(this ref VirtualCameraRigState state, Pose worldCoordinates)
        {
            var originRotation = state.Origin.rotation;
            var originInv = Matrix4x4.TRS(state.Origin.position, originRotation, Vector3.one).inverse;
            state.JoystickPosition = Vector3.zero;
            state.JoystickAngles = Vector3.zero;
            state.ARPose.position = originInv.MultiplyPoint3x4(worldCoordinates.position);
            state.ARPose.rotation = Quaternion.Inverse(originRotation) * worldCoordinates.rotation;
            state.LocalPose.position = state.ARPose.position;
            state.LocalPose.rotation = state.ARPose.rotation;
            state.Pose.position = worldCoordinates.position;
            state.Pose.rotation = worldCoordinates.rotation;
        }

        /// <summary>
        /// Reset the local pose to the origin and update the rebase offset of the rotation to the new values.
        /// </summary>
        public static void Reset(this ref VirtualCameraRigState state)
        {
            state.JoystickPosition = Vector3.zero;
            state.JoystickAngles = Vector3.zero;
            state.ARPose.position = Vector3.zero;
            state.ARPose.rotation = Quaternion.identity;

            var ergonomicTilt = Quaternion.Euler(state.ErgonomicTilt, 0f, 0f);
            var ergonomicInput = state.LastInput.rotation * ergonomicTilt;
            state.RebaseOffset = Quaternion.Euler(0f, ergonomicInput.eulerAngles.y, 0f);
        }

        /// <summary>
        /// Translate the pose without taking into account the constraints.
        /// </summary>
        /// <param name="vector">The direction of the translation. The direction is also affected by the space parameter</param>
        /// <param name="deltaTime">The time step in seconds</param>
        /// <param name="speed">Scaled speed along x, y and z directions</param>
        /// <param name="pedestalSpace">Should the translation on the Y axis happen relative to world or local space</param>
        /// <param name="motionSpace">Should the translation on the X and Z axes happen relative to world or local space</param>
        /// <param name = "settings" > The settings that will be applied while translating the state</param>
        public static void Translate(this ref VirtualCameraRigState state, Vector3 vector, float deltaTime, Vector3 speed, Space pedestalSpace, Space motionSpace, VirtualCameraRigSettings settings)
        {
            deltaTime = Mathf.Max(0f, deltaTime);
            var deltaSpeed = speed * deltaTime;

            if (pedestalSpace == Space.Self)
            {
                state.JoystickPosition += state.LocalPose.up * vector.y * deltaSpeed.y;
            }
            else
            {
                state.JoystickPosition += Vector3.up * vector.y * deltaSpeed.y;
            }

            if (motionSpace == Space.Self)
            {
                state.JoystickPosition += state.LocalPose.forward * vector.z * deltaSpeed.z;
                state.JoystickPosition += state.LocalPose.right * vector.x * deltaSpeed.x;

            }
            else
            {
                Vector3 forward;

                if (settings.Rebasing)
                {
                    var heading = state.JoystickAngles.y + state.ARPose.rotation.eulerAngles.y;
                    forward = Quaternion.AngleAxis(heading, Vector3.up) * Vector3.forward;
                }
                else
                {
                    forward = Vector3.ProjectOnPlane(state.LocalPose.forward, Vector3.up);
                    var scaleTowardsZero = Mathf.InverseLerp(0.01f, 0.04f, forward.magnitude);
                    forward = forward.normalized * scaleTowardsZero;
                }

                var right = new Vector3(forward.z, forward.y, -forward.x);

                state.JoystickPosition += forward * vector.z * deltaSpeed.z;
                state.JoystickPosition += right * vector.x * deltaSpeed.x;
            }

            state.Refresh(settings);
        }

        /// <summary>
        /// Rotate the pose without taking into account the constraints.
        /// </summary>
        /// <param name="vector">The strength of the rotation around each axis.</param>
        /// <param name="deltaTime">The time step in seconds</param>
        /// <param name="strength">Scaled strength around x, y and z axis</param>
        /// <param name = "settings" > The settings that will be applied while rotating the state</param>
        public static void Rotate(this ref VirtualCameraRigState state, Vector3 vector, float deltaTime, Vector3 strength, VirtualCameraRigSettings settings)
        {
            deltaTime = Mathf.Max(0f, deltaTime);
            var deltaStrength = strength * deltaTime;
            var anglesToAdd = Vector3.Scale(vector, deltaStrength);

            state.JoystickAngles += anglesToAdd;

            state.Refresh(settings);
        }

        /// <summary>
        /// Sets up JoystickAngles based on the current AR orientation.
        /// Call this method before enabling joystick tilt & roll rotation mode (like when enabling rebasing).
        /// </summary>
        public static void InitializeJoystickValues(this ref VirtualCameraRigState state)
        {
            var AREuler = state.ARPose.rotation.eulerAngles;
            if (Mathf.Abs(state.ARPose.forward.y) > 0.99f)
            {
                // Hack to simulate worldX,worldY,localZ rotation order at singularities while still using the default worldZ,worldX,worldY rotation order.
                // ARPose at singularities has 0 pan, all the twist is contained in roll. We want 0 pan and all the twist contained in pan instead.
                state.JoystickAngles.y -= AREuler.z;
                state.JoystickAngles.z -= AREuler.z;
            }
        }

        static void ComputePose(this ref VirtualCameraRigState state, VirtualCameraRigSettings settings)
        {
            state.LocalPose.position = state.ARPose.position + state.JoystickPosition;

            if (settings.Rebasing)
            {
                var AREuler = state.ARPose.rotation.eulerAngles;
                state.LocalPose.rotation.eulerAngles = state.JoystickAngles + AREuler;
            }
            else
            {
                // Clear & disable X and Z joystick rotation while AR is enabled
                state.JoystickAngles.x = state.JoystickAngles.z = 0f;

                // Apply only the joystick Y rotation while AR is enabled
                var joystickRotationY = Quaternion.Euler(0f, state.JoystickAngles.y, 0f);

                state.LocalPose.rotation = joystickRotationY * state.ARPose.rotation;
            }

            var pose = new Pose(
                state.Origin.position + state.Origin.rotation * state.LocalPose.position,
                state.Origin.rotation * state.LocalPose.rotation);
            state.Pose = pose;
        }

        static Vector3 Mask(Vector3 vector, bool x, bool y, bool z)
        {
            return Vector3.Scale(vector, new Vector3(x ? 0f : 1f, y ? 0f : 1f, z ? 0f : 1f));
        }
    }
}
