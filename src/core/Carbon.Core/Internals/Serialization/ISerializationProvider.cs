using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;


namespace Carbon.Core.Internals.Serialization
{
    /// <summary>
    /// Basic contract for classes that can provide serialization/de-serialization services 
    /// to the internal framework for class that can be serialized or deserialized.
    /// </summary>
    public interface ISerializationProvider 
    {
        /// <summary>
        /// This will return all of the types to be serialized by the serialization instance.
        /// </summary>
        /// <returns></returns>
        Type[] GetTypes();


        /// <summary>
        /// (Read-Only). Flag to indicate whether the serializer is loaded with the types and ready for serialization/de-serialization 
        /// operations.
        /// </summary>
        bool IsInitialized();

        /// <summary>
        /// This will initialize the serialization provider so that serialization/de-serialization can 
        /// begin.
        /// </summary>
        /// <exception cref="Exception">Thrown if the custom types are not added before calling this method.</exception>
        void Initialize();

        /// <summary>
        /// This will scan an assembly and load all of the concrete user-defined classes into the serialization engine.
        /// </summary>
        /// <param name="assemblyToScan"></param>
        void Scan(params Assembly[] assemblyToScan);

        /// <summary>
        /// This will scan an assembly by name and load all of the concrete user-defined classes into the serialization engine.
        /// </summary>
        /// <param name="assemblyToScan"></param>
        void Scan(params string[] assemblyToScan);

        /// <summary>
        /// This will add a user-defined type to the serializer and not check for infrastructure specific rules.
        /// </summary>
        /// <param name="customType"></param>
        void AddType(Type customType);

        /// <summary>
        /// This will add a user-defined type to the serializer.
        /// </summary>
        /// <param name="customType">Custom type to load in seralizer.</param>
        void AddCustomType(Type customType);

        /// <summary>
        /// This will add a collection user-defined types to the serializer.
        /// </summary>
        /// <param name="customTypes">Custom types to load in seralizer.</param>
        void AddCustomTypes(IList<Type> customTypes);

        /// <summary>
        /// This will add a collection of serializable objects to the serializer.
        /// </summary>
        /// <param name="messages">Collection of messages to add to the serializer.</param>
        void AddCustomMessages(IList<object> messages);

        /// <summary>
        /// Deserializes a xml-string based representation of the object into the corresponding concrete type.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        object Deserialize(string instance);

        /// <summary>
        /// Deserializes a <seealso cref="Stream"/> representation of the object into the corresponding concrete type.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> containing the contents to create into a concrete instance.</param>
        /// <returns></returns>
        object Deserialize(Stream stream);

        /// <summary>
        /// Deserializes a <seealso cref="Stream"/> representation of the object into the corresponding concrete type.
        /// </summary>
        /// <param name="bytes"><see cref="Byte"/> containing the contents to create into a concrete instance.</param>
        /// <returns></returns>
        object Deserialize(byte[] bytes);

        /// <summary>
        /// Serializes an object representation of the object into the corresponding string representation.
        /// </summary>
        /// <param name="message">Serializable object.</param>
        /// <returns>
        /// <seealso cref="string"/>
        /// </returns>
        string Serialize(object message);

        /// <summary>
        /// Serializes an object representation of the object into the corresponding array of bytes representation.
        /// </summary>
        /// <param name="message">Serializable object.</param>
        /// <returns>
        ///  Array of <seealso cref="byte"/>
        /// </returns>
        byte[] SerializeToBytes(object message);

        /// <summary>
        /// This will refresh the instance of the serialization provider in the event that new types are added.
        /// </summary>
        void Refresh();

        /// <summary>
        /// This will take in an arbitrary object and try to return the xml serialized version of the object, if it can not 
        /// be serialized, it will return back the full type name of the object.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string Resolve(object message);

    }
}