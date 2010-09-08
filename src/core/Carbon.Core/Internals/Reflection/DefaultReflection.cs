using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

namespace Carbon.Core.Internals.Reflection
{
    public class DefaultReflection : IReflection
    {
        public DefaultReflection()
        {
         
        }

        /// <summary>
        /// This will build a concrete proxy object based on the interface contract.
        /// </summary>
        /// <param name="contract">Type of the interface to create the concrete proxy.</typeparam>
        /// <returns></returns>
        public object BuildProxyFor(Type contract)
        {
            object proxy = null;
            if(!contract.IsInterface) return proxy;

            var methods = new List<MethodInfo>(contract.GetMethods());

            var assemblyName = new AssemblyName("proxyAssembly");
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("proxyModule", "_proxy.dll");

            TypeBuilder typeBuilder = null;

            if (contract.Name.StartsWith("I"))
            {
                var proxyName = contract.Name.Substring(1, contract.Name.Length - 1);
                typeBuilder = moduleBuilder.DefineType(proxyName, TypeAttributes.Public);
            }
            else
            {
                typeBuilder = moduleBuilder.DefineType(contract.Name + "Proxy", TypeAttributes.Public);
            }

            typeBuilder.AddInterfaceImplementation(contract);

            // for inheritance chains, get all of the methods for full implementation and subsequent interfaces:
            foreach (var @interface in contract.GetInterfaces())
            {
                typeBuilder.AddInterfaceImplementation(@interface);

                foreach (var method in @interface.GetMethods())
                    if (!methods.Contains(method))
                        methods.Add(method);
            }

            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] { });

            var ilGenerator = ctorBuilder.GetILGenerator();
            //ilGenerator.EmitWriteLine("Creating Proxy instance");
            ilGenerator.Emit(OpCodes.Ret);

            var fieldNames = new List<string>();

            // cycle through all of the methods and create the "set" and "get" methods:
            foreach (var methodInfo in methods)
            {
                // build the backing field for the property based on the property name:
                var fieldName = string.Concat("m_", methodInfo.Name
                                                        .Replace("set_", string.Empty)
                                                        .Replace("get_", string.Empty))
                    .Trim();

                // create the property name based on the set_XXX and get_XXX matching methods:
                var propertyName = fieldName.Replace("m_", string.Empty);

                if (!fieldNames.Contains(fieldName))
                {
                    fieldNames.Add(fieldName);
                    FieldBuilder fieldBuilder = typeBuilder.DefineField(fieldName.ToLower(),
                                                                        methodInfo.ReturnType,
                                                                        FieldAttributes.Private);

                    // The last argument of DefineProperty is null, because the
                    // property has no parameters. (If you don't specify null, you must
                    // specify an array of Type objects. For a parameterless property,
                    // use an array with no elements: new Type[] {})
                    PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName,
                                                                                 PropertyAttributes.HasDefault,
                                                                                 methodInfo.ReturnType,
                                                                                 null);

                    // The property set and property get methods require a special
                    // set of attributes.
                    MethodAttributes getSetAttr =
                        MethodAttributes.Public |
                        MethodAttributes.HideBySig |
                        MethodAttributes.SpecialName |
                        MethodAttributes.NewSlot |
                        MethodAttributes.Virtual |
                        MethodAttributes.Final;

                    // Define the "get" accessor method for the property.
                    MethodBuilder getPropMethodBuilder =
                        typeBuilder.DefineMethod(string.Concat("get_", propertyName),
                                                 getSetAttr,
                                                 methodInfo.ReturnType,
                                                 Type.EmptyTypes);

                    ILGenerator getPropMethodIL = getPropMethodBuilder.GetILGenerator();

                    getPropMethodIL.Emit(OpCodes.Ldarg_0);
                    getPropMethodIL.Emit(OpCodes.Ldfld, fieldBuilder);
                    getPropMethodIL.Emit(OpCodes.Ret);

                    // Define the "set" accessor method for the property.
                    MethodBuilder setPropMethodBuilder =
                        typeBuilder.DefineMethod(string.Concat("set_", propertyName),
                                                 getSetAttr,
                                                 null,
                                                 new Type[] { methodInfo.ReturnType });

                    ILGenerator setPropMethodIL = setPropMethodBuilder.GetILGenerator();

                    setPropMethodIL.Emit(OpCodes.Ldarg_0);
                    setPropMethodIL.Emit(OpCodes.Ldarg_1);
                    setPropMethodIL.Emit(OpCodes.Stfld, fieldBuilder);
                    setPropMethodIL.Emit(OpCodes.Ret);

                    // Last, we must map the two methods created above to our PropertyBuilder to 
                    // their corresponding behaviors, "get" and "set" respectively. 
                    propertyBuilder.SetGetMethod(getPropMethodBuilder);
                    propertyBuilder.SetSetMethod(setPropMethodBuilder);

                }

            }

            var constructedType = typeBuilder.CreateType();
            var instance = Activator.CreateInstance(constructedType);
            proxy = instance;

            var file = typeBuilder.Name + "_proxy.dll";
            var filename = Path.Combine(Environment.CurrentDirectory, file);

            try
            {
                if (File.Exists(filename))
                    File.Delete(filename);

                assemblyBuilder.Save(file);
            }
            catch
            {
              
            }

            return proxy;
        }

        /// <summary>
        /// This will build a concrete proxy object based on the interface contract.
        /// </summary>
        /// <typeparam name="TContract">Interface for building a concrete instance</typeparam>
        /// <returns></returns>
        public TContract BuildProxyFor<TContract>()
        {
            // this is adapted from http://msdn.microsoft.com/en-us/library/system.reflection.emit.propertybuilder.aspx
             
            var proxy = default(TContract);

            var typeOfT = typeof(TContract);
            var methods = new List<MethodInfo>(typeOfT.GetMethods());

            var assemblyName = new AssemblyName("proxyAssembly");
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("proxyModule", "_proxy.dll");

            TypeBuilder typeBuilder = null;

            if (typeof(TContract).Name.StartsWith("I"))
            {
                var proxyName = typeof(TContract).Name.Substring(1, typeof(TContract).Name.Length - 1);
                typeBuilder = moduleBuilder.DefineType(proxyName, TypeAttributes.Public);
            }
            else
            {
                typeBuilder = moduleBuilder.DefineType(typeof(TContract).Name + "Proxy", TypeAttributes.Public);
            }

            typeBuilder.AddInterfaceImplementation(typeOfT);

            // for inheritance chains, get all of the methods for full implementation and subsequent interfaces:
            foreach (var @interface in typeof(TContract).GetInterfaces())
            {
                typeBuilder.AddInterfaceImplementation(@interface);

                foreach (var method in @interface.GetMethods())
                    if (!methods.Contains(method))
                        methods.Add(method);
            }

            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] { });

            var ilGenerator = ctorBuilder.GetILGenerator();
            //ilGenerator.EmitWriteLine("Creating Proxy instance");
            ilGenerator.Emit(OpCodes.Ret);

            var fieldNames = new List<string>();

            // cycle through all of the methods and create the "set" and "get" methods:
            foreach (var methodInfo in methods)
            {
                // build the backing field for the property based on the property name:
                var fieldName = string.Concat("m_", methodInfo.Name
                                                        .Replace("set_", string.Empty)
                                                        .Replace("get_", string.Empty))
                    .Trim();

                // create the property name based on the set_XXX and get_XXX matching methods:
                var propertyName = fieldName.Replace("m_", string.Empty);

                if (!fieldNames.Contains(fieldName))
                {
                    fieldNames.Add(fieldName);
                    FieldBuilder fieldBuilder = typeBuilder.DefineField(fieldName.ToLower(),
                                                                        methodInfo.ReturnType,
                                                                        FieldAttributes.Private);

                    // The last argument of DefineProperty is null, because the
                    // property has no parameters. (If you don't specify null, you must
                    // specify an array of Type objects. For a parameterless property,
                    // use an array with no elements: new Type[] {})
                    PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName,
                                                                                 PropertyAttributes.HasDefault,
                                                                                 methodInfo.ReturnType,
                                                                                 null);

                    // The property set and property get methods require a special
                    // set of attributes.
                    MethodAttributes getSetAttr =
                        MethodAttributes.Public |
                        MethodAttributes.HideBySig |
                        MethodAttributes.SpecialName |
                        MethodAttributes.NewSlot |
                        MethodAttributes.Virtual |
                        MethodAttributes.Final;

                    // Define the "get" accessor method for the property.
                    MethodBuilder getPropMethodBuilder =
                        typeBuilder.DefineMethod(string.Concat("get_", propertyName),
                                                 getSetAttr,
                                                 methodInfo.ReturnType,
                                                 Type.EmptyTypes);

                    ILGenerator getPropMethodIL = getPropMethodBuilder.GetILGenerator();

                    getPropMethodIL.Emit(OpCodes.Ldarg_0);
                    getPropMethodIL.Emit(OpCodes.Ldfld, fieldBuilder);
                    getPropMethodIL.Emit(OpCodes.Ret);

                    // Define the "set" accessor method for the property.
                    MethodBuilder setPropMethodBuilder =
                        typeBuilder.DefineMethod(string.Concat("set_", propertyName),
                                                 getSetAttr,
                                                 null,
                                                 new Type[] { methodInfo.ReturnType });

                    ILGenerator setPropMethodIL = setPropMethodBuilder.GetILGenerator();

                    setPropMethodIL.Emit(OpCodes.Ldarg_0);
                    setPropMethodIL.Emit(OpCodes.Ldarg_1);
                    setPropMethodIL.Emit(OpCodes.Stfld, fieldBuilder);
                    setPropMethodIL.Emit(OpCodes.Ret);

                    // Last, we must map the two methods created above to our PropertyBuilder to 
                    // their corresponding behaviors, "get" and "set" respectively. 
                    propertyBuilder.SetGetMethod(getPropMethodBuilder);
                    propertyBuilder.SetSetMethod(setPropMethodBuilder);

                }

            }

            var constructedType = typeBuilder.CreateType();
            var instance = Activator.CreateInstance(constructedType);
            proxy = (TContract)instance;

            var file = typeBuilder.Name + "_proxy.dll";
            var filename = Path.Combine(Environment.CurrentDirectory, file);

            try
            {
                if (File.Exists(filename))
                    File.Delete(filename);

                assemblyBuilder.Save(file);
            }
            catch
            {

            }

            return proxy;

        }

        public Type[] FindMessagesAssignableFrom(Type contractType, Type consumer)
        {
            var types = new List<Type>();

            if (ContainsInterface(consumer, contractType))
            {
                foreach (var method in consumer.GetMethods())
                {
                    if (method.Name.ToLower().Trim() == "consume")
                    {
                        foreach (var parameter in method.GetParameters())
                        {
                            if (!types.Contains(parameter.ParameterType))
                                types.Add(parameter.ParameterType);

                            // pull in the enum types if required
                            foreach (var property in parameter.ParameterType.GetProperties())
                            {
                                if (property.PropertyType.BaseType == typeof(Enum))
                                    if (!types.Contains(property.PropertyType))
                                        types.Add(property.PropertyType);
                            }
                        }
                    }
                }
            }

            return types.ToArray();
        }

        public Type[] FindAllMessagesForConsumer(Type consumer)
        {
            var retval = new List<Type>();

            var msgs = new List<Type>();

            //msgs.AddRange(FindMessagesAssignableFrom(typeof(Consumes<>), consumer));
            //msgs.AddRange(FindMessagesAssignableFrom(typeof(Conversation<>), consumer));
            //msgs.AddRange(FindMessagesAssignableFrom(typeof(InitiatedBy<>), consumer));
            //msgs.AddRange(FindMessagesAssignableFrom(typeof(OrchestratedBy<>), consumer));

            foreach (var msg in msgs)
            {
                if (!retval.Contains(msg))
                    retval.Add(msg);
            }

            return retval.ToArray();
        }

        public Type FindTypeForName(string typeName)
        {
            Type instance = null;
            Assembly asm = null;

            var typeParts = typeName.Split(new char[] { ',' });

            try
            {
                asm = Assembly.Load(typeParts[1]);
            }
            catch
            {
                string msg = string.Format("Could not load the assembly {0} to create type {1}.", typeParts[1],
                                           typeParts[0]);
                return instance;
            }

            try
            {
                foreach (var type in asm.GetTypes())
                {
                    if(type.AssemblyQualifiedName.Trim().ToLower() != typeName.Trim().ToLower()) continue;
                    instance = type; 
                    break;
                }
            }
            catch
            {
                string msg = string.Format("Could not find the type {0}.", typeParts[0]);
                return instance;
            }

            return instance;
        }

        public TMessage BuildInstance<TMessage>()
        {
            var instance = default(TMessage);

            instance = (TMessage)this.BuildInstance(typeof(TMessage));

            return instance;
        }

        public object BuildInstance(Type currentType)
        {
            object instance = null;

            try
            {
                instance = currentType.Assembly.CreateInstance(currentType.FullName);
            }
            catch (Exception)
            {
                string msg = string.Format("Could create the instance from the assembly '{0}' to create type '{1}'.",
                                           currentType.Assembly.FullName,
                                           currentType.FullName);
                throw;
            }

            return instance;
        }

        public object BuildInstance(string typeName)
        {
            object instance = null;
            Assembly asm = null;

            var typeParts = typeName.Split(new char[] { ',' });

            try
            {
                asm = Assembly.Load(typeParts[1]);
            }
            catch
            {
                string msg = string.Format("Could not load the assembly {0} to create type {1}.", typeParts[1],
                                           typeParts[0]);

                //m_logger.Error(msg, exception);
                return instance;
            }

            try
            {
                instance = asm.CreateInstance(typeParts[0]);
            }
            catch
            {
                string msg = string.Format("Could not create the type {0}.", typeParts[0]);
                //m_logger.Error(msg, exception);
                return instance;
            }

            return instance;
        }

        public T BuildInstance<T>(string typeName) where T : class
        {
            T retval = default(T);

            try
            {
                var inst = BuildInstance(typeName);
                retval = (T)inst;
            }
            catch
            {
                string msg = string.Format("Could cast object instance to type of {0}", typeof(T).FullName);
                //m_logger.Error(msg, exception);
            }

            return retval;
        }

        public MethodInfo[] FindConsumerMethodsThatConsumesMessage(object consumer, object message)
        {
            var retval = new List<MethodInfo>();

            foreach (var method in consumer.GetType().GetMethods())
            {
                foreach (var parameter in method.GetParameters())
                {
                    if (parameter.ParameterType.IsGenericType && message.GetType().IsGenericType)
                    {
                        var parameterGenericInstance = parameter.ParameterType.GetGenericArguments()[0];
                        var messageGenericInstance = message.GetType().GetGenericArguments()[0];

                        if (parameterGenericInstance == messageGenericInstance)
                            if (!retval.Contains(method))
                                retval.Add(method);
                    }
                    else
                    {
                        if (parameter.ParameterType == message.GetType())
                            if (!retval.Contains(method))
                                retval.Add(method);
                    }

                }

            }

            return retval.ToArray();
        }

        public void InvokeConsumerMethod(object consumer, object message)
        {
            var methods = FindConsumerMethodsThatConsumesMessage(consumer, message);
            foreach (var method in methods)
            {
                try
                {
                    //this.SetPropertyValue(consumer, "Bus", bus);
                    method.Invoke(consumer, new object[] { message });
                }
                catch
                {
                    continue;
                }

            }
        }

        public void InvokeConsumerMethod(Type consumer, object message)
        {
            var instance = BuildInstance(consumer.AssemblyQualifiedName);
            InvokeConsumerMethod(instance, message);
        }

        public Type[] ExtractAllConsumersFrom(Assembly assembly)
        {
            var types = new List<Type>();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass & !type.IsAbstract)
                {
                    //if (ContainsInterface(type, typeof(Consumes<>)) ||
                    //    ContainsInterface(type, typeof(InitiatedBy<>)) ||
                    //    ContainsInterface(type, typeof(OrchestratedBy<>)))
                    //{
                    //    if (!types.Contains(type))
                    //        types.Add(type);
                    //}
                }
            }

            return types.ToArray();
        }

        public Type[] ExtractAllConversationsFrom(Assembly assembly)
        {
            var types = new List<Type>();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass & !type.IsAbstract)
                {
                    //if (ContainsInterface(type, typeof(Conversation<>)))
                    //{
                    //    if (!types.Contains(type))
                    //        types.Add(type);
                    //}
                }
            }

            return types.ToArray();
        }

        public Type FindConcreteTypeImplementingInterface(Type interfaceType, Assembly assemblyToScan)
        {
            Type retval = null;

            foreach (var type in assemblyToScan.GetTypes())
            {
                if (type.IsClass & !type.IsAbstract)
                    if (interfaceType.IsAssignableFrom(type))
                    {
                        retval = type;
                        break;
                    }
            }

            return retval;
        }

        public Type[] FindConcreteTypesImplementingInterface(Type interfaceType, Assembly assemblyToScan)
        {
            var retval = new List<Type>();

            foreach (var type in assemblyToScan.GetTypes())
            {
                if (type.IsClass & !type.IsAbstract)
                    if (interfaceType.IsAssignableFrom(type))
                    {
                        if (!retval.Contains(type))
                            retval.Add(type);

                    }
            }

            return retval.ToArray();
        }

        public object[] FindConcreteTypesImplementingInterfaceAndBuild(Type interfaceType, Assembly assemblyToScan)
        {
            var objects = new List<object>();
            var types = this.FindConcreteTypesImplementingInterface(interfaceType, assemblyToScan);

            foreach (var type in types)
            {
                objects.Add(this.BuildInstance(type.AssemblyQualifiedName));
            }

            return objects.ToArray();
        }

        public object InvokeMethod(MethodInfo method, object instance, object message)
        {
            object retval = null;

            try
            {
                retval = method.Invoke(instance, new object[] { message });
            }
            catch
            {
                throw;
            }

            return retval;
        }


        public object InvokeMethod(string methodName, object instance, object message)
        {
            object retval = null;

            try
            {
                var method = instance.GetType().GetMethod(methodName);
                if (message == null)
                    retval = method.Invoke(instance, null);
                else
                {
                    retval = method.Invoke(instance, new object[] { message });    
                }
                
            }
            catch(Exception exception)
            {
                throw;
            }

            return retval;
        }

        public Type[] FindAllMessagesUnderNamespace(string assemblyName, string namespaceToSearch)
        {
            var types = new List<Type>();

            try
            {
                var asm = Assembly.Load(assemblyName);
                foreach (var type in asm.GetTypes())
                {
                    if (type.IsClass & !type.IsAbstract)
                    {
                        if (string.IsNullOrEmpty(namespaceToSearch))
                        {
                            types.Add(type);
                        }
                        else if (type.FullName.StartsWith(namespaceToSearch))
                        {
                            types.Add(type);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }

            return types.ToArray();
        }

        public Type[] FindAllMessagesUnderNamespace(Assembly assembly, string namespaceToSearch)
        {
            var types = new List<Type>();

            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass & !type.IsAbstract)
                    {
                        if (string.IsNullOrEmpty(namespaceToSearch))
                        {
                            types.Add(type);
                        }
                        else if (type.FullName.StartsWith(namespaceToSearch))
                        {
                            types.Add(type);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }

            return types.ToArray();
        }

        public void SetPropertyValue(object instance, string name, string value)
        {
            try
            {
                Int32 intValue = 1;
                bool boolValue = false;
                DateTime dateTimeValue = DateTime.MinValue;
                bool isPropertySet = false;

                PropertyInfo propInfo = null;

                foreach (var prop in instance.GetType().GetProperties())
                {
                    if (prop.Name.Trim().ToLower() == name.Trim().ToLower())
                    {
                        propInfo = prop;
                        break;
                    }
                }

                if (propInfo == null)
                    return;


                if (Boolean.TryParse(value, out boolValue))
                {
                    propInfo.SetValue(instance, boolValue, null);
                    isPropertySet = true;
                }

                if (DateTime.TryParse(value, out dateTimeValue))
                {
                    propInfo.SetValue(instance, dateTimeValue, null);
                    isPropertySet = true;
                }

                if (Int32.TryParse(value, out intValue))
                {
                    propInfo.SetValue(instance, intValue, null);
                    isPropertySet = true;
                }

                if (!isPropertySet)
                    propInfo.SetValue(instance, value, null);
            }
            catch
            {

            }
        }

        public object GetPropertyValue(object instance, string name)
        {
            PropertyInfo propInfo = null;

            foreach (var prop in instance.GetType().GetProperties())
            {
                if (prop.Name.Trim().ToLower() == name.Trim().ToLower())
                {
                    propInfo = prop;
                    break;
                }
            }

            if (propInfo == null)
                return null;

            return propInfo.GetValue(instance, null);
        }

        public T CreateClone<T>(object instance) where T : class
        {
            var newInstance = this.BuildInstance(instance.GetType().AssemblyQualifiedName);

            foreach (var property in instance.GetType().GetProperties())
            {
                SetPropertyValue(newInstance, property.Name, property.GetValue(instance, null));
            }

            return (T)newInstance;
        }

        public void SetPropertyValue(object instance, string name, object value)
        {
            try
            {

                PropertyInfo propInfo = null;

                foreach (var prop in instance.GetType().GetProperties())
                {
                    if (prop.Name.Trim().ToLower() == name.Trim().ToLower())
                    {
                        propInfo = prop;
                        break;
                    }
                }

                if (propInfo == null)
                    return;

                propInfo.SetValue(instance, value, null);

            }
            catch
            {

            }
        }

        private static bool ContainsInterface(Type consumer, Type interfaceType)
        {
            var isFound = false;

            var interfaces = new List<Type>(consumer.GetInterfaces());
            foreach (var item in interfaces)
            {
                if (item.Name == interfaceType.Name)
                {
                    isFound = true;
                    break;
                }
            }

            return isFound;
        }

        public Type GetTypeForNamedInstance(string typeName)
        {
            Type instance = null;
            Assembly asm = null;

            var typeParts = typeName.Split(new char[] { ',' });

            try
            {
                asm = Assembly.Load(typeParts[1]);
            }
            catch
            {
                string msg = string.Format("Could not load the assembly {0} to create type {1}.", typeParts[1],
                                           typeParts[0]);

                //m_logger.Error(msg, exception);
                return instance;
            }

            try
            {
                foreach (var type in asm.GetTypes())
                {
                    if (type.FullName.ToLower().Trim() == typeParts[0].Trim().ToLower())
                    {
                        instance = type;
                        break;
                    }
                }
            }
            catch
            {
                string msg = string.Format("Could not retrieve the type {0}.", typeParts[0]);
                //m_logger.Error(msg, exception);
                return instance;
            }

            return instance;
        }

        public object FindAndInvokeMethod(object instance, string methodName, object value)
        {
            object retval = null;

            try
            {
                var method = instance.GetType().GetMethod(methodName);
                retval = method.Invoke(instance, new object[] { value });

            }
            catch (Exception exception)
            {
                string msg = string.Format("Could not invoke method '{0}' on the type {1}. Reason: {2}",
                                           methodName, instance.GetType().FullName, exception.Message);
                throw new Exception(msg, exception);
            }

            return retval;
        }

        public void InvokeSagaPersisterSave(object persister, object saga)
        {
            InvokeMethod("Save", persister, saga);
        }

        public void InvokeSagaPersisterComplete(object persister, Guid id)
        {
            InvokeMethod("Complete", persister, id);
        }

        public object InvokeSagaPersisterFind(object persister,  Guid id)
        {
            return this.InvokeMethod("Find", persister, id);
        }

        /// <summary>
        /// This will create a generic version of the generic type and the generic type 
        /// value for resolving concrete instances implemented in the container.
        /// </summary>
        /// <param name="genericType">Generic type to create</param>
        /// <param name="genericTypeValues">Values to supply to the generic type for construction </param>
        /// <returns></returns>
        public Type GetGenericVersionOf(Type genericType,  params Type[] genericTypeValues)
        {
            var newType = genericType.MakeGenericType(genericTypeValues);
            return newType;
        }
    }
}