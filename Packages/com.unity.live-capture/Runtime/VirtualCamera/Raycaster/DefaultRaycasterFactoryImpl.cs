using System;
using System.Collections.Generic;
using Unity.LiveCapture.VirtualCamera.Raycasting;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Raycaster factory implementation that produces <see cref="DefaultRaycaster"/> instances.
    /// </summary>
    class DefaultRaycasterFactoryImpl : IRaycasterFactoryImpl
    {
        static GraphicsRaycaster s_GraphicsRaycaster;

        HashSet<IRaycaster> m_Instances = new HashSet<IRaycaster>();

        /// <inheritdoc/>
        public IRaycaster Create()
        {
            if (s_GraphicsRaycaster == null)
            {
                s_GraphicsRaycaster = new GraphicsRaycaster();
            }

            var raycaster = new DefaultRaycaster(s_GraphicsRaycaster);

            m_Instances.Add(raycaster);

            return raycaster;
        }

        /// <inheritdoc/>
        public void Dispose(IRaycaster raycaster)
        {
            if (!m_Instances.Contains(raycaster))
                throw new Exception("The provided raycaster was not created by this factory");

            m_Instances.Remove(raycaster);

            if (m_Instances.Count == 0)
            {
                s_GraphicsRaycaster.Dispose();
                s_GraphicsRaycaster = null;
            }
        }
    }
}
