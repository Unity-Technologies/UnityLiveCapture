using System;
using System.Diagnostics.Contracts;

namespace Unity.LiveCapture.VideoStreaming.Server.Sdp
{
    class EncriptionKey
    {
        public EncriptionKey(string p)
        {
        }

        public static EncriptionKey ParseInvariant(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            Contract.EndContractBlock();

            throw new NotImplementedException();
        }
    }
}
