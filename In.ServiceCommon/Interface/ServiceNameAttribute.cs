using System;

namespace In.ServiceCommon.Interface
{
    public class ServiceNameAttribute : Attribute
    {
        public string Name { get; set; }

        public ServiceNameAttribute(string name)
        {
            Name = name;
        }
    }
}