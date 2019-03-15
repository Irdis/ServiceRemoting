using System;

namespace In.ServiceCommon.Client
{
    public class ClientMessageHeader
    {
        public MessageType Type { get; set; }
        public Guid? Key { get; set; }
        public string StreamingTarget { get; set; }
    }
}