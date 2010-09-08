using System;
using System.Reflection;

namespace Carbon.Core.Internals.Reflection
{
    /// <summary>
    /// Contract for utilities needed for entity reflection-based capabilties.
    /// </summary>
    public interface IReflection
    {
        /// <summary>
        /// This will build a concrete proxy object based on the interface contract.
        /// </summary>
        /// <typeparam name="TContract">Interface on which to build the concreate instance.</typeparam>
        /// <returns></returns>
        TContract BuildProxyFor<TContract>();

        /// <summary>
        /// This will search the given message consumer and return the list of messages that it will process.
        /// </summary>
        /// <param name="contractType">The type of the contract to inspect on the consumer</param>
        /// <param name="consumer">The type associated with the given message consumer</param>
        Type[] FindMessagesAssignableFrom(Type contractType, Type consumer);

        /// <summary>
        /// This will search the given message consumer and return the list of messages that it will process.
        /// </summary>
        /// <param name="consumer">The type associated with the given message consumer</param>
        Type[] FindAllMessagesForConsumer(Type consumer);

        /// <summary>
        /// This will build an instance of the given type.
        /// </summary>
        /// <param name="typeName">This will create an object based on the .NET fully qualified type name of the object.</param>
        object BuildInstance(string typeName);

        /// <summary>
        /// This will build an instance of the given type and cast it to the well-known type T.
        /// </summary>
        /// <param name="typeName">This will create an object based on the .NET fully qualified type name of the object.</param>
        T BuildInstance<T>(string typeName) where T : class;

        /// <summary>
        /// This will build an instance of the given type and cast it to the well-known type T.
        /// </summary>
        /// <typeparam name="TMessage">Message to build</typeparam>
        /// <returns></returns>
        TMessage BuildInstance<TMessage>();

        /// <summary>
        /// This will build an instance of the given type.
        /// </summary>
        /// <param name="currentType">This will create an object based on the current type.</param>
        object BuildInstance(Type currentType);

        /// <summary>
        /// This will search a given message consumer that will implement the method containing a specific message
        /// </summary>
        MethodInfo[] FindConsumerMethodsThatConsumesMessage(object consumer, object message);

        /// <summary>
        /// This will invoke the a method on the consumer for the given message.
        /// </summary>
        void InvokeConsumerMethod(object consumer, object message);

        /// <summary>
        /// This will invoke the a method on the consumer for the given message.
        /// </summary>
        void InvokeConsumerMethod(Type consumer, object message);

        /// <summary>
        /// This will examine an entire assembly and extract all of the consumers.
        /// </summary>
        /// <param name="assembly">Assembly to scan</param>
        /// <returns></returns>
        Type[] ExtractAllConsumersFrom(Assembly assembly);

        Type FindConcreteTypeImplementingInterface(Type interfaceType, Assembly assemblyToScan);

        Type[] FindAllMessagesUnderNamespace(string assemblyName, string namespaceToSearch);

        Type[] FindAllMessagesUnderNamespace(Assembly assembly, string namespaceToSearch);

        void SetPropertyValue(object instance, string name, string value);

        void SetPropertyValue(object instance, string name, object value);

        object GetPropertyValue(object instance, string name);

        /// <summary>
        /// This will create a clone of an object by executing a deep-copy of all of the properties.
        /// The class instance used must have a no argument constructor for cloning.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        T CreateClone<T>(object instance) where T : class;

        /// <summary>
        /// This will examine an assembly and extract out all conversations.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        Type[] ExtractAllConversationsFrom(Assembly assembly);

        Type[] FindConcreteTypesImplementingInterface(Type interfaceType, Assembly assemblyToScan);

        object[] FindConcreteTypesImplementingInterfaceAndBuild(Type interfaceType, Assembly assemblyToScan);

        object InvokeMethod(MethodInfo method, object instance, object message);
        Type GetTypeForNamedInstance(string typeName);

        object FindAndInvokeMethod(object instance, string methodName, object value);

        /// <summary>
        /// This will build a concrete proxy object based on the interface contract.
        /// </summary>
        /// <param name="contract">Type of the interface to create the concrete proxy.</typeparam>
        /// <returns></returns>
        object BuildProxyFor(Type contract);

        Type FindTypeForName(string typeName);

        /// <summary>
        /// This will create a generic version of the generic type and the generic type 
        /// value for resolving concrete instances implemented in the container.
        /// </summary>
        /// <param name="genericType">Generic type to create</param>
        /// <param name="genericTypeValues">Values to supply to the generic type for construction </param>
        /// <returns></returns>
        Type GetGenericVersionOf(Type genericType,  params Type[] genericTypeValues);

        object InvokeMethod(string methodName, object instance, object message);

        /// <summary>
        /// This will invoke the "Save" method on the generic instance of the saga persister.
        /// </summary>
        /// <param name="persister">Instance of the persister</param>
        /// <param name="saga">Saga to save</param>
        void InvokeSagaPersisterSave(object persister, object saga);

        /// <summary>
        /// This will invoke the "Complete" method on the generic instance of the saga persister.
        /// </summary>
        /// <param name="persister">Instance of the persister</param>
        /// <param name="id">Saga identifier to mark for completion</param>
        void InvokeSagaPersisterComplete(object persister, Guid id);

        /// <summary>
        /// This will invoke the "Find" method on the generic instance of the saga persister.
        /// </summary>
        /// <param name="persister">Instance of the persister</param>
        /// <param name="id">Saga identifier to find</param>
        object InvokeSagaPersisterFind(object persister, Guid id);
    }
}