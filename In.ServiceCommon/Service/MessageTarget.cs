using System;
using System.Reflection;

namespace In.ServiceCommon.Service
{
    public class MessageTarget
    {
        public Type Type { get; set; }
        public MethodInfo Method { get; set; }
        public bool Await { get; set; }
        public Guid? MessageKey { get; set; }
        public object[] Arguments { get; set; }
    }
}