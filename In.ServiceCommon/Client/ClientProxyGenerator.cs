using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using In.ServiceCommon.Interface;

namespace In.ServiceCommon.Client
{
    public class ClientProxyGenerator
    {
        private readonly string _componentName;
        private readonly IDictionary<Tuple<Type,MethodInfo>, ServiceCallInfo> _infoProvider;
        private static readonly OpCode[] _intCodes = new OpCode[]
        {
            OpCodes.Ldc_I4_0,
            OpCodes.Ldc_I4_1,
            OpCodes.Ldc_I4_2,
            OpCodes.Ldc_I4_3,
            OpCodes.Ldc_I4_4,
            OpCodes.Ldc_I4_5,
            OpCodes.Ldc_I4_6,
            OpCodes.Ldc_I4_7,
            OpCodes.Ldc_I4_8
        };
        private static readonly OpCode[] _argCodes = new OpCode[]
        {
            OpCodes.Ldarg_0,
            OpCodes.Ldarg_1,
            OpCodes.Ldarg_2,
            OpCodes.Ldarg_3
        };

        public ClientProxyGenerator(InterfaceInfoProvider infoProvider)
            : this(infoProvider, "__" + Guid.NewGuid().ToString("N"))
        {
        }

        public ClientProxyGenerator(InterfaceInfoProvider infoProvider, string componentName)
        {
            _componentName = componentName;
            _infoProvider = infoProvider.GetServiceCallInfos().ToDictionary(info => Tuple.Create(info.Type, info.Method));
        }
        public List<object> Build(List<Type> interfaces)
        {
            var result = Generate(interfaces);
            return result;
        }

        private List<object> Generate(ICollection<Type> interfaces)
        {
            var currentDomain = AppDomain.CurrentDomain;
            var assemName = new AssemblyName();
            assemName.Name = _componentName + "Assembly";
            var assemBuilder = currentDomain.DefineDynamicAssembly(assemName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemBuilder.DefineDynamicModule(_componentName + "Module");
            var result = new List<object>();
            foreach (var @interface in interfaces)
            {
                result.Add(CreateImpl(moduleBuilder, @interface));
            }
            return result;
        }

        private object CreateImpl(ModuleBuilder moduleBuilder, Type interfaceType)
        {
            var classBuilder = moduleBuilder.DefineType(
                $"{interfaceType.Name}__{Guid.NewGuid():N}__Class",
                TypeAttributes.Class | TypeAttributes.Public,
                typeof(ClientProxyBase), new[] { interfaceType });
            var ctorBuilder = classBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[0]);
            ctorBuilder.GetILGenerator().Emit(OpCodes.Ret);
            foreach (var methodInfo in interfaceType.GetMethods())
            {
                var parameterTypes = methodInfo.GetParameters().Select(info => info.ParameterType).ToArray();
                var methodBuilder = classBuilder.DefineMethod(methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    CallingConventions.Standard,
                    methodInfo.ReturnType, parameterTypes);
                CreateDelegateMethod(interfaceType, methodBuilder, methodInfo);
            }

            var type = classBuilder.CreateType();
            var instance = Activator.CreateInstance(type);
            return instance;
        }

        private void CreateDelegateMethod(Type interfaceType, MethodBuilder methodBuilder, MethodInfo methodInfo)
        {
            var serviceCallInfo = _infoProvider[Tuple.Create(interfaceType, methodInfo)];
            var parameters = methodInfo.GetParameters();
            var gen = methodBuilder.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, typeof(ClientProxyBase).GetMethod("get_ServiceProxy"));
            gen.Emit(OpCodes.Ldstr, serviceCallInfo.ShortTypeName);
            gen.Emit(OpCodes.Ldstr, serviceCallInfo.ShortMethodName);
            LoadInt(gen, parameters.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                gen.Emit(OpCodes.Dup);
                LoadInt(gen, i);
                LoadArg(gen, i + 1);
                if (parameter.ParameterType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, parameter.ParameterType);
                }
                gen.Emit(OpCodes.Stelem_Ref);
            }

            if (methodInfo.ReturnType != typeof(void))
            {
                gen.Emit(OpCodes.Callvirt, typeof(ClientServiceProxy).GetMethod("Call"));
                if (methodInfo.ReturnType.IsValueType)
                {
                    gen.Emit(OpCodes.Unbox_Any, methodInfo.ReturnType);
                }
                else
                {
                    gen.Emit(OpCodes.Castclass, methodInfo.ReturnType);
                }
            }
            else if (serviceCallInfo.Await)
            {
                gen.Emit(OpCodes.Callvirt, typeof(ClientServiceProxy).GetMethod("CallVoidSync"));
            }
            else
            {
                gen.Emit(OpCodes.Callvirt, typeof(ClientServiceProxy).GetMethod("CallVoidAsync"));
            }

            gen.Emit(OpCodes.Ret);
        }

        private void LoadInt(ILGenerator generator, int value)
        {
            if (value < _intCodes.Length)
            {
                generator.Emit(_intCodes[value]);
            }
            else
            {
                generator.Emit(OpCodes.Ldc_I4_S, value);
            }
        }

        private void LoadArg(ILGenerator generator, int value)
        {
            if (value < _argCodes.Length)
            {
                generator.Emit(_argCodes[value]);
            }
            else
            {
                generator.Emit(OpCodes.Ldarg_S, value);
            }
        }
    }
}
