using System;
using System.Reflection;

namespace In.ServiceCommon.Interface
{
    public class ServiceCallInfo
    {
        public Type Type { get; set; }
        public MethodInfo Method { get; set; }
        public Type[] ArgumentTypes { get; set; }
        public string ShortTypeName { get; set; }
        public string ShortMethodName { get; set; }
        public Type ReturnType { get; set; }
        public bool Await { get; set; }
        public bool StreamingCall { get; set; }
    }
}