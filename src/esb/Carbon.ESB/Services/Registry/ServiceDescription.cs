using System;

namespace Kharbon.Registries.For.BackgroundServices
{
    public class ServiceDescription : IServiceDescription
    {
        public string Name { get; private set; }
        public Type Contract { get; private set; }
        public Type Service { get; private set; }

        /// <summary>
        ///  .ctor
        /// </summary>
        /// <param name="name">The name of the service</param>
        /// <param name="contract">The interface or contract that the concrete service instance implements</param>
        /// <param name="service">The concrete instance of the service</param>
        public ServiceDescription(string name, Type contract, Type service)
        {
            Name = name;
            Contract = contract;
            Service = service;
        }
    }
}