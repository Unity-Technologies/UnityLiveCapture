using System;
using System.Diagnostics.Contracts;

namespace Unity.LiveCapture.VideoStreaming.Server.Sdp
{
    class SdpTimeZone
    {
        public SdpTimeZone()
        {
        }

        public static SdpTimeZone ParseInvariant(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            Contract.EndContractBlock();

            SdpTimeZone returnValue = new SdpTimeZone();

            throw new NotImplementedException();
            // return returnValue;
        }
    }
}
