using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace In.ServiceCommon.Interface
{
    public class InterfaceInfoProvider
    {
        private readonly List<Type> _services;

        private readonly Dictionary<Tuple<Type, MethodInfo>, ServiceCallInfo> _interfaceInfos = new Dictionary<Tuple<Type, MethodInfo>, ServiceCallInfo>();

        public InterfaceInfoProvider(List<Type> services)
        {
            _services = services;
            Init();
        }

        private void Init()
        {
            foreach (var service in _services)
            {
                Analyze(service);
            }
        }

        private void Analyze(Type service)
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
                    _interfaceInfos.Add(Tuple.Create(service, methodInfo), new ServiceCallInfo
                    {
                        ShortTypeName = shortName,
                        ShortMethodName = methodShortName,
                        ReturnType = methodInfo.ReturnType,
                        Await = awaitExec
                    });
                }
                else
                {
                    _interfaceInfos.Add(Tuple.Create(service, methodInfo), new ServiceCallInfo
                    {
                        ShortTypeName = shortName,
                        ShortMethodName = methodInfo.Name,
                        ReturnType = methodInfo.ReturnType,
                        Await = true
                    });
                }
            }
        }

        public ServiceCallInfo GetServiceCallInfo(Type type, MethodInfo method)
        {
            return _interfaceInfos[Tuple.Create(type, method)];
        }
    }
}