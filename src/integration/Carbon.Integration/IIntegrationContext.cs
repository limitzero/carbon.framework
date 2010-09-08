using System;
using System.Collections.Generic;
using Carbon.Core;
using Carbon.Core.RuntimeServices;
using Carbon.Integration.Dsl.Surface;

namespace Carbon.Integration
{
    /// <summary>
    /// Contract for establishing the context for all integration components to be either created or executed.
    /// </summary>
    public interface IIntegrationContext : IStartable, IDisposable
    {
        /// <summary>
        /// Event that is triggered when a message is delivered to a messaging endpoint.
        /// </summary>
        event EventHandler<ApplicationContextMessageDeliveredEventArgs> ApplicationContextMessageDelivered;

        /// <summary>
        /// Event that is triggered when a message is received from a messaging endpoint.
        /// </summary>
        event EventHandler<ApplicationContextMessageReceivedEventArgs> ApplicationContextMessageReceived;

        /// <summary>
        /// Event that is triggered when a message endpoint encounters an error.
        /// </summary>
        event EventHandler<ApplicationContextErrorEventArgs> ApplicationContextError;

        /// <summary>
        /// This will retreive a component from the underlying container for use.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        TComponent GetComponent<TComponent>();

        /// <summary>
        /// This will allow for construction of a dependant component for a
        /// messaging endpoint conversation.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component to create from the existing set of conversation messages.</typeparam>
        TComponent CreateComponent<TComponent>();

        /// <summary>
        /// This will load the surface by identifier from the configuration file for message interaction.
        /// </summary>
        /// <param name="id"></param>
        void LoadSurface(string id);

        /// <summary>
        /// This will load an <seealso cref="AbstractIntegrationSurface">integration surface</seealso>
        /// by type for coordinating message interaction.
        /// </summary>
        /// <typeparam name="TSurface">Type of the class representing the integration surface.</typeparam>
        void LoadSurface<TSurface>() where TSurface : AbstractIntegrationComponentSurface;

        /// <summary>
        /// This will load an <seealso cref="AbstractIntegrationSurface">integration surface</seealso>
        /// by type for coordinating message interaction.
        /// </summary>
        /// <param name="surface">Type of the class representing the integration surface.</typeparam>
        void LoadSurface(Type surface);

        /// <summary>
        /// This will load an <seealso cref="AbstractIntegrationSurface">integration surface</seealso>
        /// by type for coordinating message interaction and set the properties with the values specified.
        /// </summary>
        /// <param name="surface">Type of the class representing the integration surface.</typeparam>
        void LoadSurface(Type surface, IDictionary<string, object> parameters);

        /// <summary>
        /// This will load all of the <seealso cref="AbstractIntegrationSurface">integration surfaces</seealso>
        /// for coordinating message interaction.
        /// </summary>
        void LoadAllSurfaces();

        /// <summary>
        /// This will send a message to an endpoint who is listening on a particular channel.
        /// </summary>
        /// <param name="channel">Name of the channel to send the message on</param>
        /// <param name="messages">Message(s) to send</param>
        void Send(string channel, params IEnvelope[] messages);

        /// <summary>
        /// This will send a message to an endpoint who is listening on a particular channel.
        /// </summary>
        /// <param name="location">Uri scheme corresponding to a message adapter for transmitting the message</param>
        /// <param name="messages">Message(s) to send</param>
        void Send(Uri location, params IEnvelope[] messages);


    }
}