using System;

namespace Carbon.ESB.Saga.Finder
{
    /// <summary>
    /// Contract for all sagas that need to be filtered for specific information.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    public interface ISagaFinder<TSaga> where TSaga : Saga
    {
        /// <summary>
        /// This will retrieve a conversation from the persistance store by identifier
        /// </summary>
        /// <param name="id">Identifier to retrieve a particular conversation instance.</param>
        /// <returns></returns>
        TSaga Find(Guid id);

        /// <summary>
        /// This will return the most recent saga instance from the persistance store.
        /// </summary>
        /// <returns></returns>
        TSaga FindFirst();

        /// <summary>
        /// This will return the oldest saga instance from the persistance store.
        /// </summary>
        /// <returns></returns>
        TSaga FindLast();
    }
}