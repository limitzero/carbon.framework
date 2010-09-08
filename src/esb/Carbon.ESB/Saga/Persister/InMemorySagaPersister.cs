using System;
using System.Collections.Generic;
using Carbon.Core.Internals.Serialization;
using Carbon.ESB.Messages;

namespace Carbon.ESB.Saga.Persister
{
    /// <summary>
    ///  Local instance of an in-memory saga persister that will have a singleton 
    ///  lifecycle instance in the container for storing all <seealso cref="Saga">saga instances</seealso>.
    /// </summary>
    /// <typeparam name="TSaga">Type of the saga to persist</typeparam>
    public class InMemorySagaPersister<TSaga> :
        ISagaPersister<TSaga> where TSaga : class, ISaga
    {
        private static readonly object m_threads_lock = new object();
        private readonly ISerializationProvider m_serialization_provider;
        private static IDictionary<Guid, SagaThread> m_threads = null;

        public InMemorySagaPersister(ISerializationProvider serializationProvider)
        {
            m_serialization_provider = serializationProvider;

            if (m_threads == null)
                m_threads = new Dictionary<Guid, SagaThread>();
        }

        public TSaga Find(Guid id)
        {
            object saga = null;
            SagaThread st = null;

            try
            {
                if (m_threads.TryGetValue(id, out st))
                {
                    if (st.SagaInstance != null)
                        saga = st.SagaInstance;
                    else
                    {
                        saga = m_serialization_provider
                            .Deserialize(st.Saga);
                    }
                }
                else
                {
                    return default(TSaga);
                }
            }
            catch
            {
                throw;
            }

            return (TSaga)saga;
        }

        public void Save(TSaga conversation)
        {
            try
            {
                var sagaThread = CreateSagaThread(conversation);

                if (m_threads.ContainsKey(sagaThread.SagaId))
                    lock (m_threads_lock)
                        m_threads.Remove(sagaThread.SagaId);

                AddThread(sagaThread);
            }
            catch (Exception exception)
            {
                var msg =
                    string.Format(
                        "An error has occured while attempting to save the current saga thread '{0} - {1} 'to storage. Reason: {2} ",
                        conversation.SagaId.ToString(), conversation.GetType().FullName, exception.ToString());
                throw new Exception(msg, exception);
            }
        }

        public void Complete(Guid id)
        {
            lock (m_threads_lock)
                m_threads.Remove(id);
        }

        private void AddThread(SagaThread thread)
        {
            try
            {
                lock (m_threads_lock)
                    m_threads.Add(new KeyValuePair<Guid, SagaThread>(thread.SagaId, thread));
            }
            catch(Exception exception)
            {
                throw;
            }

        }

        private SagaThread CreateSagaThread(object saga)
        {
           
            var cnv = saga as Saga;
            var token = cnv.SagaId;
            byte[] inst = { };

            try
            {
                inst = m_serialization_provider.SerializeToBytes(saga);
            }
            catch
            {
                // ignore this for now with the in-memory representation:
            }

            var ct = new SagaThread()
                         {
                             SagaId = token,
                             Saga = inst,
                             SagaInstance = saga,
                             SagaName = saga.GetType().FullName,
                             CreateDateTime =  System.DateTime.Now
                         };

            return ct;
        }
    }
}