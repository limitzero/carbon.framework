using System;

namespace Carbon.Core.Builder
{
    /// <summary>
    /// The activation style of the component when registered in the container.
    /// </summary>
    public enum ActivationStyle
    {
        /// <summary>
        /// This will create a new instance of the component on every resolution from the container
        /// </summary>
        AsInstance,

        /// <summary>
        /// This will create a new instance of the component on the first resolution from the container and return the 
        /// same instance on each subsequent resolution.
        /// </summary>
        AsSingleton
    }

    /// <summary>
    /// Contract for the basic operations that an IoC container will have for registering and resolving components.
    /// </summary>
    public interface IObjectBuilder : IDisposable
    {
        /// <summary>
        /// This will set the contents of the container.
        /// </summary>
        /// <param name="container"></param>
        void SetContainer(object container);

        /// <summary>
        /// This will allow for construction of a dependant component for a messaging endpoint conversation.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component to create from the existing set of conversation messages.</typeparam>
        TComponent CreateComponent<TComponent>();

        /// <summary>
        /// This will resolve a component in the container by type.
        /// </summary>
        /// <typeparam name="TComponent">Type to look for in the container and return.</typeparam>
        /// <returns></returns>
        TComponent Resolve<TComponent>();

        /// <summary>
        /// This will resolve a component in the container by type.
        /// </summary>
        /// <<param name="component">Type of the component to look for in the container.</param>
        /// <returns></returns>
        object Resolve(Type component);

        /// <summary>
        /// This will resolve a component in the contanter by unique identifier.
        /// </summary>
        /// <param name="id">Identifier of the component</param>
        /// <returns></returns>
        object Resolve(string id);

        /// <summary>
        /// This will resolve all components that are assignable from the 
        /// component type.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        TComponent[] ResolveAll<TComponent>();

        /// <summary>
        /// This will register the component in the underlying container by type and identifier.
        /// </summary>
        /// <param name="id">Identifier of the component</param>
        /// <param name="component">Type of the component</param>
        void Register(string id, Type component);

        /// <summary>
        /// This will register the component in the underlying container by type and identifier.
        /// </summary>
        /// <param name="id">Identifier of the component</param>
        /// <param name="component">Type of the component</param>
        /// <param name="activationStyle">The activation style of the component in the container.</param>
        void Register(string id, Type component, ActivationStyle activationStyle);

        /// <summary>
        /// This will register the component in the underlying container by type and identifier.
        /// </summary>
        /// <param name="id">Identifier of the component</param>
        /// <param name="component">Instance of the component</param>
        /// <param name="activationStyle">The activation style of the component in the container.</param>
        void Register(string id, object component, ActivationStyle activationStyle);

        /// <summary>
        /// This will register the component in the underlying container by type and identifier.
        /// </summary>
        /// <typeparam name="TComponent">Type to register in the container.</typeparam>
        /// <param name="id">Identifier of the component</param>
        void Register<TComponent>(string id) where TComponent : class;

        /// <summary>
        /// This will register the component in the underlying container by type and identifier.
        /// </summary>
        /// <typeparam name="TComponent">Type to register in the container.</typeparam>
        /// <param name="id">Identifier of the component</param>
        /// <param name="activationStyle">The activation style of the component in the container.</param>
        void Register<TComponent>(string id, ActivationStyle activationStyle) where TComponent : class;

        /// <summary>
        /// This will register the component in the underlying container by type and identifier.
        /// </summary>
        /// <typeparam name="TContract">Interface type to register in the container for the component contract</typeparam>
        /// <typeparam name="TComponent">Class type to register in the container implementing the contract</typeparam>
        /// <param name="id">Identifier of the component</param>
        void Register<TContract, TComponent>(string id) where TComponent : class;

        /// <summary>
        /// This will register the component in the underlying container by type and identifier.
        /// </summary>
        /// <typeparam name="TContract">Interface type to register in the container for the component contract</typeparam>
        /// <typeparam name="TComponent">Class type to register in the container implementing the contract</typeparam>
        /// <param name="id">Identifier of the component</param>
        /// <param name="activationStyle">The activation style of the component in the container.</param>
        void Register<TContract, TComponent>(string id, ActivationStyle activationStyle) where TComponent : class;

        /// <summary>
        /// This will register the component in the underlying container by type and identifier.
        /// </summary>
        /// <param name="id">Identifier of the component</param>
        /// <param name="contract">Interface type to register in the container for the component contract</param>
        /// <param name="component">Class type to register in the container implementing the contract</param>
        void Register(string id, Type contract, Type component);

        /// <summary>
        /// This will register the component in the underlying container by type and identifier.
        /// </summary>
        /// <param name="id">Identifier of the component</param>
        /// <param name="contract">Interface type to register in the container for the component contract</param>
        /// <param name="component">Class type to register in the container implementing the contract</param>
        /// <param name="activationStyle">The activation style of the component in the container.</param>
        void Register(string id, Type contract, Type component, ActivationStyle activationStyle);

        /// <summary>
        /// This will register an instance of a component with a contract by type.
        /// </summary>
        /// <typeparam name="TContract">Interface type to register in the container for the component contract</typeparam>
        /// <param name="id">Identifier of the component</param>
        /// <param name="component">Instance of the component to be registered in the container</param>
        void Register<TContract>(string id, object component);

        /// <summary>
        /// This will register an instance of a component with a contract by type.
        /// </summary>
        /// <typeparam name="TContract">Interface type to register in the container for the component contract</typeparam>
        /// <param name="id">Identifier of the component</param>
        /// <param name="component">Instance of the component to be registered in the container</param>
        /// <param name="activationStyle">The activation style of the component in the container.</param>
        void Register<TContract>(string id, object component, ActivationStyle activationStyle);

        /// <summary>
        /// This will register an instance of a component with a contract by type.
        /// </summary>
        /// <param name="contract">Interface type to register in the container for the component contract</param>
        /// <param name="id">Identifier of the component</param>
        /// <param name="component">Instance of the component to be registered in the container</param>
        void Register(string id, Type contract, object component);

        /// <summary>
        /// This will register an instance of a component with a contract by type.
        /// </summary>
        /// <param name="contract">Interface type to register in the container for the component contract</param>
        /// <param name="id">Identifier of the component</param>
        /// <param name="component">Instance of the component to be registered in the container</param>
        /// <param name="activationStyle">The activation style of the component in the container.</param>
        void Register(string id, Type contract, object component, ActivationStyle activationStyle);
    }
}