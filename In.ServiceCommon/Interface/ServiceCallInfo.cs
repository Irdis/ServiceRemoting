using System;

namespace In.ServiceCommon.Interface
{
    public class ServiceCallInfo
    {
        public string ShortTypeName { get; set; }
        public string ShortMethodName { get; set; }
        public Type ReturnType { get; set; }
        public bool Await { get; set; }
    }
}