using System;

namespace In.ServiceCommon.Client
{
    public class RemoteMethodInfo
    {
        public bool AwaitResult { get; set; }
        public Type ReturnType { get; set; }
    }
}