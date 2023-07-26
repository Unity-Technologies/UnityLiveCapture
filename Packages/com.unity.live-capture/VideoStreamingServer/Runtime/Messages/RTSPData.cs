using UnityEngine;

namespace Unity.LiveCapture.VideoStreaming.Server.Messages
{
    /// <summary>
    /// Message wich represent data. ($ limited message)
    /// </summary>
    class RtspData : RtspChunk
    {
        private static ILogger _logger = new Logger(Debug.unityLogger.logHandler);

        static RtspData()
        {
            _logger.logEnabled = false;
        }

        /// <summary>
        /// Logs the message to debug.
        /// </summary>
        public override void LogMessage(LogType aLevel)
        {
            // if the level is not logged directly return
            if (!_logger.IsLogTypeAllowed(aLevel))
                return;
            _logger.Log(aLevel, "Data message");
            if (Data == null)
                _logger.Log(aLevel, "Data : null");
            else
                _logger.Log(aLevel, "Data length :-{0}-", Data.Length);
        }

        public int Channel { get; set; }

        /// <summary>
        /// Clones this instance.
        /// <remarks>Listner is not cloned</remarks>
        /// </summary>
        /// <returns>a clone of this instance</returns>
        public override object Clone()
        {
            RtspData result = new RtspData();
            result.Channel = this.Channel;
            if (this.Data != null)
                result.Data = this.Data.Clone() as byte[];
            result.SourcePort = this.SourcePort;
            return result;
        }
    }
}
