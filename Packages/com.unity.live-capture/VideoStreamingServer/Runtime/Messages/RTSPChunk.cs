using System;
using UnityEngine;

namespace Unity.LiveCapture.VideoStreaming.Server.Messages
{
    /// <summary>
    /// Class wich represent each message echanged on Rtsp socket.
    /// </summary>
    abstract class RtspChunk : ICloneable
    {
        /// <summary>
        /// Logs the message to debug.
        /// </summary>
        public void LogMessage()
        {
            LogMessage(LogType.Log);
        }

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="alevel">The log level.</param>
        public abstract void LogMessage(LogType aLevel);

        /// <summary>
        /// Gets or sets the data associate with the message.
        /// </summary>
        /// <value>Array of byte transmit with the message.</value>
        public byte[] Data
        { get; set; }

        /// <summary>
        /// Gets or sets the source port wich receive the message.
        /// </summary>
        /// <value>The source port.</value>
        public RtspListener SourcePort { get; set; }

        #region ICloneable Membres

        /// <summary>
        /// Crée un nouvel objet qui est une copie de l'instance en cours.
        /// </summary>
        /// <returns>
        /// Nouvel objet qui est une copie de cette instance.
        /// </returns>
        public abstract object Clone();

        #endregion
    }
}
