using System;

namespace Carbon.ESB.Saga.Persister
{
    /// <summary>
    /// Contract for all conversations that can be persisted to a datastore.
    /// </summary>
    /// <typeparam name="TSaga">Saga instance to persist or remove from storage</typeparam>
    public interface ISagaPersister<TSaga>
        where TSaga : class, ISaga
    {
        /// <summary>
        /// This will retrieve a conversation from the persistance store by identifier
        /// </summary>
        /// <param name="id">Identifier to retrieve a particular conversation instance.</param>
        /// <returns></returns>
        TSaga Find(Guid id);

        /// <summary>
        /// This will record the state of the conversation to the persistance store:
        /// </summary>
        /// <param name="saga"><seealso cref="Saga">saga</seealso>to store</param>
        void Save(TSaga saga);

        /// <summary>
        /// This will remove the  conversation from the persistance store
        /// </summary>
        /// <param name="id">Identifier to retrieve a particular conversation instance.</param>
        void Complete(Guid id);
    }
}