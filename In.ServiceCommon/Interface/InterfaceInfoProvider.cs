using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace In.ServiceCommon.Interface
{
    public class InterfaceInfoProvider
    {
        private readonly List<Type> _services;

        public List<Type> Services => _services;

        public InterfaceInfoProvider(List<Type> services)
        {
            _services = services;
        }

        public List<ServiceCallInfo> GetServiceCallInfos()
        {
            var result = new List<ServiceCallInfo>();
            foreach (var service in _services)
            {
                GetServiceCallInfos(service, result);
            }
            return result;
        }

        private void GetServiceCallInfos(Type service, List<ServiceCallInfo> aggregate)
        {
            var shortNameAttribute = service.GetCustomAttributes(typeof(ServiceNameAttribute), false).FirstOrDefault();
            var shortName = shortNameAttribute == null
                ? service.FullName
                : ((ServiceNameAttribute) shortNameAttribute).Name;
            var methods = service.GetMethods();
            foreach (var methodInfo in methods)
            {
                var methodInfoAttribute = methodInfo.GetCustomAttributes(typeof(ServiceCallAttribute), false).FirstOrDefault();
                if (methodInfoAttribute != null)
                {
                    var attribute = (ServiceCallAttribute) methodInfoAttribute;
                    var methodShortName = attribute.Name ?? methodInfo.Name;
                    var awaitExec = attribute.Await;
                    aggregate.Add(new ServiceCallInfo
                    {
                        Type = service,
                        Method = methodInfo,
                        ShortTypeName = shortName,
                        ShortMethodName = methodShortName,
                        ReturnType = methodInfo.ReturnType,
                        Await = awaitExec
                    });
                }
                else
                {
                    aggregate.Add(new ServiceCallInfo
                    {
                        Type = service,
                        Method = methodInfo,
                        ShortTypeName = shortName,
                        ShortMethodName = methodInfo.Name,
                        ReturnType = methodInfo.ReturnType,
                        Await = true
                    });
                }
            }
        }

    }
}