using System;
using System.Collections;
using System.Collections.Generic;
using Carbon.Core.Builder;
using Carbon.Core.Internals.Reflection;
using Castle.Core;
using Castle.MicroKernel;

namespace Carbon.Windsor.Container.Integration
{
    /// <summary>
    /// Adapter for the Castle Windsor Container translating the commands of <seealso cref="IObjectBuilder"/>
    /// to the native implementation.
    /// </summary>
    public class WindsorContainerObjectBuilder : IObjectBuilder
    {
        private IKernel m_kernel = null;

        /// <summary>
        /// .ctor
        /// </summary>
        public WindsorContainerObjectBuilder()
            : this(null)
        {

        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="kernel">Underlying container for holding components</param>
        public WindsorContainerObjectBuilder(IKernel kernel)
        {
            m_kernel = kernel;
            if (m_kernel == null)
            {
                m_kernel = new DefaultKernel(); ;
            }
        }

        public void SetContainer(object container)
        {
            this.m_kernel = container as IKernel;
        }

        public TComponent CreateComponent<TComponent>()
        {
            if (typeof(TComponent).IsInterface)
                return m_kernel.Resolve<IReflection>().BuildProxyFor<TComponent>();

            return m_kernel.Resolve<IReflection>().BuildInstance<TComponent>();
        }


        public TComponent Resolve<TComponent>()
        {
            var component = default(TComponent);

            try
            {
                component = m_kernel.Resolve<TComponent>();
            }
            catch 
            {
              
            }

            return component;
        }

        public TComponent[] ResolveAll<TComponent>()
        {
            var listing = new List<TComponent>();

            try
            {
                var components = m_kernel.ResolveAll<TComponent>();
                listing = new List<TComponent>(components);
            }
            catch
            {
              
            }

            return listing.ToArray();
        }

        public object Resolve(Type component)
        {
            object retval = null;

            try
            {
                retval = m_kernel.Resolve(component);
            }
            catch
            {

            }

            return retval;
        }

        public object Resolve(string id)
        {
            object retval = null;

            try
            {
              retval = m_kernel.Resolve(id, new Hashtable());
            }
            catch 
            {
               
            }

            return retval;
        }

        public void Register(string id, Type component)
        {
            this.Register(id, component, ActivationStyle.AsInstance);
        }

        public void Register(string id, object component, ActivationStyle activationStyle)
        {
            if (string.IsNullOrEmpty(id))
                id = component.GetType().Name;

            try
            {

                // activation styles are not supported for this:
                m_kernel.AddComponentInstance(id, component);

                if (activationStyle == ActivationStyle.AsInstance)
                    m_kernel.AddComponentInstance(id, component);
            }
            catch (ComponentRegistrationException exception)
            {
                // component already registered:
            }
        }


        public void Register(string id, Type component, ActivationStyle activationStyle)
        {
            if (string.IsNullOrEmpty(id))
                id = component.Name;
            try
            {

                if (activationStyle == ActivationStyle.AsInstance)
                    m_kernel.AddComponent(id, component, LifestyleType.Transient);

                if (activationStyle == ActivationStyle.AsSingleton)
                    m_kernel.AddComponent(id, component, LifestyleType.Singleton);
            }
            catch (ComponentRegistrationException exception)
            {
                // component already registered:
            }
        }

        public void Register<TComponent>(string id) where TComponent : class
        {
            this.Register<TComponent>(id, ActivationStyle.AsInstance);
        }

        public void Register<TComponent>(string id, ActivationStyle activationStyle) where TComponent : class
        {
            if (string.IsNullOrEmpty(id))
                id = typeof(TComponent).Name;

            try
            {
                if (activationStyle == ActivationStyle.AsInstance)
                    m_kernel.AddComponent(id, typeof(TComponent), LifestyleType.Transient);

                if (activationStyle == ActivationStyle.AsSingleton)
                    m_kernel.AddComponent(id, typeof(TComponent), LifestyleType.Singleton);

            }
            catch (ComponentRegistrationException exception)
            {
                // component already registered
            }
        }

        public void Register<TContract, TComponent>(string id) where TComponent : class
        {
            this.Register<TContract, TComponent>(id);
        }

        public void Register<TContract, TComponent>(string id, ActivationStyle activationStyle) where TComponent : class
        {
            if (string.IsNullOrEmpty(id))
                id = typeof(TContract).Name;
            try
            {

                if (activationStyle == ActivationStyle.AsInstance)
                    m_kernel.AddComponent(id, typeof(TContract), typeof(TComponent), LifestyleType.Transient);

                if (activationStyle == ActivationStyle.AsSingleton)
                    m_kernel.AddComponent(id, typeof(TContract), typeof(TComponent), LifestyleType.Singleton);
            }
            catch (ComponentRegistrationException exception)
            {
                //component already registered:
            }
        }

        public void Register(string id, Type contract, Type component)
        {
            this.Register(id, contract, component, ActivationStyle.AsInstance);
        }

        public void Register(string id, Type contract, Type component, ActivationStyle activationStyle)
        {
            if (string.IsNullOrEmpty(id))
                id = contract.Name;

            try
            {
                if (activationStyle == ActivationStyle.AsInstance)
                    m_kernel.AddComponent(id, contract, component, LifestyleType.Transient);

                if (activationStyle == ActivationStyle.AsSingleton)
                    m_kernel.AddComponent(id, contract, component, LifestyleType.Singleton);
            }
            catch (ComponentRegistrationException exception)
            {
                // component already present:
            }

        }

        public void Register<TContract>(string id, object component)
        {
            this.Register<TContract>(id, component, ActivationStyle.AsInstance);
        }

        public void Register<TContract>(string id, object component, ActivationStyle activationStyle)
        {
            if (string.IsNullOrEmpty(id))
                id = typeof(TContract).Name;

            try
            {
                // activation styles are not supported by Winsor for this:
                m_kernel.AddComponentInstance(id, typeof(TContract), component);
            }
            catch (ComponentRegistrationException exception)
            {
                // component already present:
            }
        }

        public void Register(string id, Type contract, object component)
        {
            this.Register(id, contract, component, ActivationStyle.AsInstance);
        }

        public void Register(string id, Type contract, object component, ActivationStyle activationStyle)
        {
            if (string.IsNullOrEmpty(id))
                id = contract.Name;
            try
            {
                // activation styles are not supported by Winsor for this:
                m_kernel.AddComponentInstance(id, contract, component);
            }
            catch (ComponentRegistrationException exception)
            {
                // component already present:
            }
        }

        public void Dispose()
        {
            m_kernel.Dispose();
        }


    }
}