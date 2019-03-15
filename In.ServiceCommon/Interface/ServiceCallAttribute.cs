using System;

namespace In.ServiceCommon.Interface
{
    public class StreamingContractAttribute : Attribute
    {
        public Type ClientAdapter { get; set; }
        public Type NetworkInterface { get; set; }

        public StreamingContractAttribute(Type clientAdapter, Type networkInterface)
        {
            ClientAdapter = clientAdapter;
            NetworkInterface = networkInterface;
        }
    }

    public class ServiceSubscriptionAttribute : Attribute
    {   
    }

    public class ServiceCallAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Await { get; set; }

        public ServiceCallAttribute()
        {
            Await = true;
        }
    }
}