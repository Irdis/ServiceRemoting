using System;

namespace In.ServiceCommon.Service
{
    public class MessageTargetHeader
    {
        public string Type { get; set; }
        public string Method { get; set; }
        public bool Await { get; set; }
        public Guid? MessageKey { get; set; }
    }
}