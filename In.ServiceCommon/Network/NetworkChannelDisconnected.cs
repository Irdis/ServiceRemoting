using System;

namespace In.ServiceCommon.Network
{
    public class NetworkChannelDisconnected : Exception
    {
        public NetworkChannelDisconnected()
        {
        }

        public NetworkChannelDisconnected(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}