using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Rigs
{
    static class VirtualCameraDamping
    {
        const float k_LogNegligibleResidual = -4.605170186f; // == math.Log(0.01f);
        const float k_Epsilon = 0.0001f;

        /// <summary>Positions the virtual camera according to the transposer rules.</summary>
        /// <param name="lastPose">The last pose of the damped value.</param>
        /// <param name="input">The current pose of the Camera.</param>
        /// <param name="damping">The damping settings.</param>
        /// <param name="deltaTime">The current delta time the damping is applied on.</param>
        /// <returns>The damped position.</returns>
        public static Pose Calculate(Pose lastPose, Pose input, Damping damping, float deltaTime)
        {
            if (!damping.Enabled)
                return input;

            var current = lastPose;
            var targetOrientation = input.rotation;
            var dampedOrientation = targetOrientation;
            if (deltaTime >= 0 && damping.Aim > 0)
            {
                var relative = (Quaternion.Inverse(current.rotation) * targetOrientation).eulerAngles;
                for (var i = 0; i < 3; ++i)
                    if (relative[i] > 180)
                        relative[i] -= 360;
                relative = Damp(relative, Vector3.one * damping.Aim, deltaTime);
                dampedOrientation = current.rotation * Quaternion.Euler(relative);
            }

            var targetPosition = input.position;
            var currentPosition = current.position;
            var worldOffset = targetPosition - currentPosition;

            // Adjust for damping, which is done in camera-offset-local coords
            if (deltaTime >= 0 && (damping.Body.x > 0 || damping.Body.y > 0 || damping.Body.z > 0))
            {
                var localOffset = Quaternion.Inverse(dampedOrientation) * worldOffset;
                localOffset = Damp(localOffset, damping.Body, deltaTime);
                worldOffset = dampedOrientation * localOffset;
            }
            current.position = currentPosition + worldOffset;
            current.rotation = dampedOrientation;
            return current;
        }

        /// Get a damped version of a Vector3.  This is the portion of the Vector3 that will take effect over the given time.
        /// returns The damped amount.  This will be the original amount scaled by a value between 0 and 1.
        static Vector3 Damp(Vector3 initial, Vector3 dampTime, float deltaTime)
        {
            for (var i = 0; i < 3; ++i)
                initial[i] = Damp(initial[i], dampTime[i], deltaTime);
            return initial;
        }

        /// Get a damped version of a float.  This is the portion of the float that will take effect over the given time.
        /// returns The damped amount.  This will be the original amount scaled by a value between 0 and 1.
        static float Damp(float initial, float dampTime, float deltaTime)
        {
            if (dampTime < k_Epsilon || Mathf.Abs(initial) < k_Epsilon)
                return initial;
            if (deltaTime < k_Epsilon)
                return 0;
            var k = -k_LogNegligibleResidual / dampTime;
            return initial * (1 - Mathf.Exp(-k * deltaTime));
        }
    }
}
