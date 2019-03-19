using System;

namespace In.ServiceCommon.Interface
{
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