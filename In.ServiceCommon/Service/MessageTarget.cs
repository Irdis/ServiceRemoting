using System;
using System.Reflection;

namespace In.ServiceCommon.Service
{
    public class MessageTarget
    {
        public Type Type { get; set; }
        public string ShortTypeName { get; set; }
        public MethodInfo Method { get; set; }
        public string ShortMethodName { get; set; }
        public bool Await { get; set; }
        public Guid? MessageKey { get; set; }
        public object[] Arguments { get; set; }
        public Type[] ArgumentTypes { get; set; }
        public bool StreamingCall { get; set; }
    }
}