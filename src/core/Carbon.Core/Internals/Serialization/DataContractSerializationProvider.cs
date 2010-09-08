using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Stereotypes.For.Components.Message;

namespace Carbon.Core.Internals.Serialization
{
    public class DataContractSerializationProvider : ISerializationProvider
    {
        private readonly IReflection m_reflection;
        private static List<Type> m_types = null;
        private static DataContractSerializer m_serializer = null;
        private object m_custom_type_lock = new object();

        ///<summary>
        /// Default constructor.
        ///</summary>
        public DataContractSerializationProvider(IReflection reflection)
        {
            m_reflection = reflection;
            if (m_types == null)
                m_types = new List<Type>();
        }

        public Type[] GetTypes()
        {
            return m_types.ToArray();
        }

        public void Initialize()
        {
            if (!IsInitialized())
                throw new Exception("No types added for serialization/de-serialization...");

            this.Refresh();
        }

        public void Refresh()
        {
            m_serializer = new DataContractSerializer(typeof(object), m_types.ToArray());
        }

        public void Scan(params string[] assemblyToScan)
        {
            foreach (var asm in assemblyToScan)
                this.Scan(Assembly.Load(asm));
        }

        public void Scan(params Assembly[] assemblyToScan)
        {
            foreach (var assembly in assemblyToScan)
            {
                foreach (var type in assembly.GetTypes())
                {
                    //TODO: find out why this does not pickup message endpoint objects
                    //if (type.GetCustomAttributes(typeof(MessageAttribute), true).Length == 0 ||
                    //    type.GetCustomAttributes(typeof(MessageEndpointAttribute), true).Length == 0) continue;

                    if (type.GetCustomAttributes(typeof(MessageAttribute), true).Length == 0) continue;

                    if (type.IsClass && !type.IsAbstract)
                        this.AddCustomType(type);

                    if (type.IsEnum)
                        this.AddCustomType(type);

                    // interfaces used as messages (need a proxy for them):
                    if (type.IsInterface)
                        this.AddCustomType(type);
                }

            }
        }

        public object Deserialize(string instance)
        {
            if (!IsInitialized())
                throw new Exception("No types added for deserialization...");

            return Deserialize(ASCIIEncoding.ASCII.GetBytes(instance));
        }

        public object Deserialize(Stream stream)
        {
            if (!IsInitialized())
                throw new Exception("No types added for deserialization...");

            return m_serializer.ReadObject(stream);
        }

        public object Deserialize(byte[] bytes)
        {
            if (!IsInitialized())
                throw new Exception("No types added for deserialization...");

            if (m_serializer == null)
                this.Initialize();

            object retval = null;

            try
            {
                using (var stream = new MemoryStream(bytes))
                {
                    retval = m_serializer.ReadObject(stream);
                }
            }
            catch(Exception exception)
            {
                throw;
            }

            return retval;
        }

        public string Serialize(object message)
        {
            if (!IsInitialized())
                throw new Exception("No types added for serialization...");

            var retval = string.Empty;

            if (m_serializer == null)
                this.Initialize();

            if (!m_types.Contains(message.GetType()))
            {
                m_types.Add(message.GetType());
                Initialize();
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    m_serializer.WriteObject(stream, message);
                    stream.Seek(0, SeekOrigin.Begin);

                    var textconverter = new UTF8Encoding();
                    retval = textconverter.GetString(stream.ToArray());
                }
            }
            catch(Exception exception)
            {
                throw;
            }

            return retval;
        }

        public byte[] SerializeToBytes(object message)
        {
            if (!IsInitialized())
                throw new Exception("No types added for de-serialization...");

            var retval = Serialize(message);

            return ASCIIEncoding.ASCII.GetBytes(retval);
        }

        public void AddType(Type customType)
        {
            if(!m_types.Contains(customType))
                lock (m_custom_type_lock)
                {
                    m_types.Add(customType);
                    Refresh();
                }
        }

        public void AddCustomType(Type customType)
        {
            lock (m_custom_type_lock)
            {
                if (m_types != null)
                    if (!m_types.Contains(customType))
                        if (CanBeUsedForMessaging(customType))
                            if (customType.IsInterface)
                            {
                                var proxy = m_reflection.BuildProxyFor(customType);
                                if (m_types.Find(x => x.Name.Trim().ToLower() == proxy.GetType().Name.Trim().ToLower()) == null)
                                    //if (!m_types.Contains(proxy.GetType()))
                                    m_types.Add(proxy.GetType());
                            }
                            else
                            {
                                m_types.Add(customType);
                            }
            }
        }

        public void AddCustomTypes(IList<Type> customTypes)
        {
            foreach (var type in customTypes)
            {
                AddCustomType(type);
            }
        }

        public void AddCustomMessages(IList<object> messages)
        {
            foreach (var message in messages)
            {
                if (message.GetType().IsClass & !message.GetType().IsAbstract)
                    AddCustomType(message.GetType());
            }
        }

        public string Resolve(object message)
        {
            var retval = message.GetType().FullName;

            try
            {
                retval = this.Serialize(message);
            }
            catch
            {
                throw;
            }

            return retval;
        }

        public bool IsInitialized()
        {
            return m_types.Count > 0;
        }

        private bool CanBeUsedForMessaging(Type currentType)
        {
            var isMessage = currentType.GetCustomAttributes(typeof(MessageAttribute), true).Length > 0;
            return isMessage;
        }

        ~DataContractSerializationProvider()
        {
            if (m_serializer != null)
            {
                m_serializer = null;
                m_types.Clear();
            }
        }
    }
}

